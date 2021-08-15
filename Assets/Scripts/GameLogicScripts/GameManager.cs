using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static GameManager Singleton = null;

    public float m_gameLobbyTimerSeconds = 10f; // Number of seconds in between rounds.
    public float m_gameLengthTimerSeconds = 10f; // Length of game round, in seconds.
    public float m_gameHidingTimerSeconds = 10f; // Length of time players have to hide, in seconds.

    public int m_minPlayerCount = 2; // Minimum number of players for game to start.

    // This value is true if there are enough players in the server, it means we can start a game, etc.
    public NetworkVariableBool GameReady = new NetworkVariableBool(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone,
    });

    public NetworkVariableULong SeekerClientId = new NetworkVariableULong(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    public NetworkVariableBool GameInProgress = new NetworkVariableBool(new NetworkVariableSettings {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone,
    });

    public NetworkVariableFloat GameTimer = new NetworkVariableFloat(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone,
    });

    public NetworkVariableBool PlayersHiding = new NetworkVariableBool(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone,
    });

    [SerializeField] private Text m_timerText;

    private void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (Singleton != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += AddServerCallbacks;
    }

    private void Update()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (GameReady.Value)
            {
                GameTimer.Value -= Time.deltaTime;
                UpdateGameState();
            }
        }
        UpdateGameTimerUiElement();
    }


    private void UpdateGameState()
    {
        if (GameTimer.Value <= 0 && !GameInProgress.Value)
        {
            // Lobby timer expired, start a game.
            GameTimer.Value = 0;
            StartGame();
        }
        else if (GameTimer.Value <= 0 && GameInProgress.Value && PlayersHiding.Value)
        {
            // Hiding phase is over, allow seeker to begin seeking players.
            GameTimer.Value = 0;
            EndHidingPhase();
        }
        else if (GameTimer.Value <= 0 && GameInProgress.Value)
        {
            // Game timer expired, end game.
            GameTimer.Value = 0;
            EndGame();
        }
    }
    public Player GetPlayerComponent(ulong clientId)
    {
        NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var playerClient);
        if (playerClient == null)
            return null;
        return playerClient.PlayerObject.GetComponent<Player>();
    }

    public void UpdateGameTimerUiElement()
    {
        // Update Timer UI Element.
        if (GameReady.Value && m_timerText != null)
        {
            if (GameInProgress.Value && PlayersHiding.Value)
            {
                m_timerText.text = "Time To Hide: " + ((int)GameTimer.Value).ToString();
            }
            else if (GameInProgress.Value)
            {
                m_timerText.text = "Time Left In Round: " + ((int)GameTimer.Value).ToString();
            }
            else
            {
                m_timerText.text = "Time Until Next Round: " + ((int)GameTimer.Value).ToString();
            }
        }
        else if (m_timerText != null)
        {
            m_timerText.text = "Waiting For Players";
        }
    }

    private void AddServerCallbacks()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += ClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnected;
        }
    }

    private void ClientDisconnected(ulong clientId)
    {
        Debug.Log("Player disconnected.");
        if (NetworkManager.Singleton.IsServer && NetworkManager.Singleton.ConnectedClients.Count < m_minPlayerCount)
        {
            GameReady.Value = false;
        }
    }

    private void ClientConnected(ulong clientId)
    {
        Debug.Log("Player joined.");
        if (NetworkManager.Singleton.IsServer)
        {
            if (NetworkManager.Singleton.ConnectedClients.Count >= m_minPlayerCount && !GameReady.Value)
            {
                GameReady.Value = true;
                GameTimer.Value = m_gameLobbyTimerSeconds;
            }
        }
    }

    public void StartGame()
    {
        Debug.Log("Starting game round.");
        GameInProgress.Value = true;

        ChooseSeeker();
        LoadRandomLevel();
        TeleportPlayersExceptSeekerToLevel();
    }

    public void EndGame()
    {
        Debug.Log("Ending game round.");
        GameInProgress.Value = false;
        GameTimer.Value = m_gameLobbyTimerSeconds;

        TeleportAllPlayersToLobby();
        UnloadCurrentLevel();
    }

    public void ChooseSeeker()
    {
        bool needNewSeeker = true;
        while (needNewSeeker)
        {
            int randomIndex = UnityEngine.Random.Range(0, NetworkManager.Singleton.ConnectedClientsList.Count - 1);
            ulong randomClientId = NetworkManager.Singleton.ConnectedClientsList[randomIndex].ClientId;

            // TODO: Fix bug: This always seems to pick the same seeker, at least when there are only 2 clients.
            //if (SeekerClientId.Value == 0 && randomClientId != SeekerClientId.Value)
            //{
                needNewSeeker = false;
            //}
            RemoveSeekerStatusFromPlayer(SeekerClientId.Value);
            SeekerClientId.Value = randomClientId;
            GrantPlayerSeekerStatus(SeekerClientId.Value);
        }
        Debug.Log("Client " + SeekerClientId.Value + " is the new Seeker");
    }

    private void RemoveSeekerStatusFromPlayer(ulong clientId)
    {
        Player player = GetPlayerComponent(clientId);
        if (player == null) { return; }
        player.IsSeeker.Value = false;
    }

    private void GrantPlayerSeekerStatus(ulong clientId)
    {
        Player player = GetPlayerComponent(clientId);
        if (player == null) { return; }
        player.IsSeeker.Value = true;
    }

    private void LoadRandomLevel()
    {
        Debug.Log("Loading new level.");
        // TODO: Choose level from list of possible levels, load it additively to the current scene.
    }

    private void UnloadCurrentLevel()
    {
        Debug.Log("Removing current level.");
        // TODO: Remove currently loaded game level.
    }

    private void TeleportPlayersExceptSeekerToLevel()
    {
        Debug.Log("Starting game round hiding phase.");
        GameTimer.Value = m_gameHidingTimerSeconds;
        PlayersHiding.Value = true;
        // TODO: Teleport players except seeker to spawn points on current game level.
    }

    private void TeleportSeekerToLevel()
    {
        // TODO: Teleport the seeker to the current game level.
    }
    
    private void EndHidingPhase()
    {
        Debug.Log("Starting game round seeking phase.");
        GameTimer.Value = m_gameLengthTimerSeconds;
        PlayersHiding.Value = false;
        TeleportSeekerToLevel();
    }

    private void TeleportAllPlayersToLobby()
    {
        // TODO: Teleport all players to spawn points in the game lobby.
    }
}

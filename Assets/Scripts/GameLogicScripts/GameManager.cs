using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    private enum GameState
    {
        NOT_READY,
        LOBBY,
        IN_PROGRESS_HIDING,
        IN_PROGRESS_SEEKING
    }

    public static GameManager Singleton = null;

    public float m_gameLobbyTimerSeconds = 10f; // Number of seconds in between rounds.
    public float m_gameLengthTimerSeconds = 10f; // Length of game round, in seconds.
    public float m_gameHidingTimerSeconds = 10f; // Length of time players have to hide, in seconds.

    public int m_minPlayerCount = 2; // Minimum number of players for game to start.

    public NetworkVariableInt CurrentGameState = new NetworkVariableInt(new NetworkVariableSettings {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone,
    });

    public NetworkVariableFloat GameTimer = new NetworkVariableFloat(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone,
    });

    public NetworkVariableInt CurrentLevelIndex = new NetworkVariableInt(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone,
    });

    [SerializeField] private Text m_timerText;
    [SerializeField] private List<string> m_levelNames;
    [SerializeField] private Transform m_lobbySpawnPosition;
    private Transform m_currentLevelSpawnPosition;

    private Player m_seeker;
    private List<Player> m_players;
    private List<Player> m_playersLeftToFind;
    private AsyncOperation m_levelLoaded;

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
        m_players = new List<Player>();
        m_playersLeftToFind = new List<Player>();
        m_timerText.text = "";
        NetworkManager.Singleton.OnServerStarted += AddServerCallbacks;
    }

    private void Update()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if ((GameState)CurrentGameState.Value != GameState.NOT_READY)
            {
                GameTimer.Value -= Time.deltaTime;
                UpdateGameState();
            }
        }
        UpdateGameTimerUiElement();
    }


    private void UpdateGameState()
    {
        if (GameTimer.Value <= 0 && (GameState)CurrentGameState.Value == GameState.LOBBY)
        {
            GameTimer.Value = m_gameHidingTimerSeconds;
            CurrentGameState.Value = (int)GameState.IN_PROGRESS_HIDING;
            StartGame();
        }
        else if (GameTimer.Value <= 0 && (GameState)CurrentGameState.Value == GameState.IN_PROGRESS_HIDING)
        {
            GameTimer.Value = m_gameLengthTimerSeconds;
            CurrentGameState.Value = (int)GameState.IN_PROGRESS_SEEKING;
            EndHidingPhase();
        }
        else if (GameTimer.Value <= 0 && (GameState)CurrentGameState.Value == GameState.IN_PROGRESS_SEEKING)
        {
            GameTimer.Value = m_gameLobbyTimerSeconds;
            CurrentGameState.Value = (int)GameState.LOBBY;
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
        if ((GameState)CurrentGameState.Value == GameState.NOT_READY)
        {
            m_timerText.text = "Waiting For Players";
        } else
        {
            if ((GameState)CurrentGameState.Value == GameState.IN_PROGRESS_HIDING)
            {
                m_timerText.text = "Time To Hide: " + ((int)GameTimer.Value).ToString();
            }
            else if ((GameState)CurrentGameState.Value == GameState.IN_PROGRESS_SEEKING)
            {
                m_timerText.text = "Time Left In Round: " + ((int)GameTimer.Value).ToString();
            }
            else if ((GameState)CurrentGameState.Value == GameState.LOBBY)
            {
                m_timerText.text = "Time Until Next Round: " + ((int)GameTimer.Value).ToString();
            }
        }
    }

    private void AddServerCallbacks()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            CurrentGameState.Value = (int)GameState.NOT_READY;
            m_lobbySpawnPosition = GameObject.Find("LobbySpawnPosition").GetComponent<Transform>();
            NetworkManager.Singleton.OnClientConnectedCallback += ClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnected;
        }
    }

    private void ClientDisconnected(ulong clientId)
    {
        Debug.Log("Player disconnected.");
        if (NetworkManager.Singleton.IsServer)
        {
            // Remove player from all player lists.
            Player player = GetPlayerComponent(clientId);
            if (m_players.Contains(player))
            {
                m_players.Remove(player);
            }
            if (m_playersLeftToFind.Contains(player))
            {
                m_playersLeftToFind.Remove(player);
            }
            if (NetworkManager.Singleton.ConnectedClients.Count < m_minPlayerCount)
            {
               CurrentGameState.Value = (int)GameState.NOT_READY;
            }
        }
    }

    private void ClientConnected(ulong clientId)
    {
        Debug.Log("Player joined.");
        if (NetworkManager.Singleton.IsServer)
        {
            // Add player to player list.
            Player player = GetPlayerComponent(clientId);
            m_players.Add(player);
            player.IsSeeker.Value = false;

            // Update game ready-ness.
            if (NetworkManager.Singleton.ConnectedClients.Count >= m_minPlayerCount && (GameState)CurrentGameState.Value == GameState.NOT_READY)
            {
                CurrentGameState.Value = (int)GameState.LOBBY;
                GameTimer.Value = m_gameLobbyTimerSeconds;
            }
            else
            {
                // Send a load level RPC to this newly joined client specifically. All other clients should have already done so.
                ClientRpcParams clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { clientId }
                    }
                };
                LoadLevelClientRpc(clientRpcParams);
            }
        }
    }

    public void StartGame()
    {
        Debug.Log("Starting game round hiding phase.");

        ChooseSeeker();
        UpdatePlayersLeftToFind();
        LoadRandomLevel();
    }

    public void EndGame()
    {
        Debug.Log("Ending game round.");

        TeleportAllPlayersToLobby();
        UnloadCurrentLevelServerSide();
        UnloadCurrentLevelClientRpc();
    }

    public void ChooseSeeker()
    {
        bool needNewSeeker = true;
        ulong randomClientId = 0;
        while (needNewSeeker)
        {
            int randomIndex = UnityEngine.Random.Range(0, NetworkManager.Singleton.ConnectedClientsList.Count - 1);
            randomClientId = NetworkManager.Singleton.ConnectedClientsList[randomIndex].ClientId;

            // TODO: Fix bug: This always seems to pick the same seeker, at least when there are only 2 clients.
            //if (SeekerClientId.Value == 0 && randomClientId != SeekerClientId.Value)
            //{
                needNewSeeker = false;
            //}
            RemoveSeekerStatusFromCurrentSeeker();
            m_seeker = GetPlayerComponent(randomClientId);
            m_seeker.IsSeeker.Value = true;
            
        }
        Debug.Log("Client " + randomClientId + " is the new Seeker");
    }

    private void UpdatePlayersLeftToFind()
    {
        m_playersLeftToFind.Clear();
        foreach (Player player in m_players)
        {
            if (player != m_seeker)
            {
                m_playersLeftToFind.Add(player);
            }
        }
    }

    private void RemoveSeekerStatusFromCurrentSeeker()
    {
        if (m_seeker)
        {
            m_seeker.IsSeeker.Value = false;
            m_seeker = null;
        }
        
    }

    private void LoadRandomLevel()
    {
        Debug.Log("Loading new level.");
        CurrentLevelIndex.Value = (int)Random.Range(0, m_levelNames.Count);
        LoadLevel();
        LoadLevelClientRpc();
        
    }

    private void UpdateLevelSpawnPoint(AsyncOperation operation)
    {
        Debug.Log("Updating spawn position");
        GameObject levelRoot = GameObject.Find("LevelRoot");
        levelRoot.transform.position = new Vector3(1000, 1000, 0);
        if (IsServer)
        {
            m_currentLevelSpawnPosition = GameObject.Find("LevelSpawnPosition").GetComponent<Transform>();
            Debug.Log(m_currentLevelSpawnPosition);
            TeleportPlayersExceptSeekerToLevel();
        }
    }

    private void LoadLevel()
    {
        Debug.Log("Server loading new level.");
        m_levelLoaded = SceneManager.LoadSceneAsync(m_levelNames[CurrentLevelIndex.Value], LoadSceneMode.Additive);
        m_levelLoaded.completed += UpdateLevelSpawnPoint;
    }

    [ClientRpc]
    private void LoadLevelClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("Client loading new level.");
        LoadLevel();
    }

    private void UnloadCurrentLevelServerSide()
    {
        Debug.Log("Server unloading current level.");
        SceneManager.UnloadSceneAsync(m_levelNames[CurrentLevelIndex.Value]);
    }

    [ClientRpc]
    private void UnloadCurrentLevelClientRpc()
    {
        Debug.Log("Client unloading current level.");
        SceneManager.UnloadSceneAsync(m_levelNames[CurrentLevelIndex.Value]);
    }

    private void TeleportPlayersExceptSeekerToLevel()
    {
        Debug.Log("Teleporting non-seeker players to position:" + m_currentLevelSpawnPosition.position);
        foreach (Player player in m_playersLeftToFind)
        {
            player.IsCaught.Value = false;
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { player.OwnerClientId }
                }
            };
            UpdateLocalPlayerPositionClientRpc(m_currentLevelSpawnPosition.position, clientRpcParams);
        }
    }

    private void TeleportSeekerToLevel()
    {
        Debug.Log("Teleporting seeker to position:" + m_currentLevelSpawnPosition.position);
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { m_seeker.OwnerClientId }
            }
        };
        UpdateLocalPlayerPositionClientRpc(m_currentLevelSpawnPosition.position, clientRpcParams);
    }
    
    private void EndHidingPhase()
    {
        Debug.Log("Starting game round seeking phase.");
        TeleportSeekerToLevel();
    }

    private void TeleportAllPlayersToLobby()
    {
        Debug.Log("Teleporting players to position:" + m_lobbySpawnPosition.position);
        foreach (Player player in m_players)
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { player.OwnerClientId }
                }
            };
            UpdateLocalPlayerPositionClientRpc(m_lobbySpawnPosition.position, clientRpcParams);
        }
    }

    [ClientRpc]
    private void UpdateLocalPlayerPositionClientRpc(Vector3 newPosition, ClientRpcParams clientRpcParams)
    {
        GetPlayerComponent(NetworkManager.LocalClientId).transform.position = newPosition;
    }
}

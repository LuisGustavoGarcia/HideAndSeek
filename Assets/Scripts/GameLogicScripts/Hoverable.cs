using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using MLAPI;

public class Hoverable : MonoBehaviour
{
    private bool m_hovering = false;
    private bool m_isPlayer;
    private PlayerInputActions m_input;
    private Player m_localPlayer;
    private IEnumerator m_findLocalPlayerObjectCoroutine;

    private void Awake()
    {
        m_input = new PlayerInputActions();
        m_input.Desktop.Click.performed += DoTheThing;
    }

    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += AddServerCallbacks;
    }

    private void AddServerCallbacks()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += ClientConnected;
    }

    private void ClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            m_findLocalPlayerObjectCoroutine = FindLocalPlayerObject();
            StartCoroutine(m_findLocalPlayerObjectCoroutine);
        }
    }

    private void OnEnable()
    {
        m_input.Enable();
    }

    private void OnDisable()
    {
        m_input.Disable();
    }

    private void DoTheThing(InputAction.CallbackContext ctx)
    {
        if (m_hovering && m_isPlayer)
        {
            Debug.Log("Found a player!");
        } else if (m_hovering)
        {
            Debug.Log("Did not find a player!");
        }
    }

    private void OnMouseOver()
    {
        m_hovering = true;
    }

    private void OnMouseOut()
    {
        m_hovering = false;
    }

    private IEnumerator FindLocalPlayerObject()
    {
        ulong localClientId = NetworkManager.Singleton.LocalClientId;
        while (!NetworkManager.Singleton.ConnectedClients[localClientId].PlayerObject.IsSpawned)
        {
            yield return null;
        }
        m_localPlayer = GameManager.Singleton.GetPlayerComponent(localClientId);
        if (!m_localPlayer)
        {
            Destroy(this);
        }
        else if (m_localPlayer.IsSeeker.Value)
        {
            Destroy(this);
        } 
        else if (gameObject.GetComponent<Player>() != null)
        {
            this.m_isPlayer = true;
        }
    }
}

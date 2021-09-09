using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using MLAPI;

public class Hoverable : NetworkBehaviour
{
    private bool m_hovering = false;
    private bool m_isPlayer;
    private PlayerInputActions m_input;
    private Player m_player;

    private void Awake()
    {
        m_input = new PlayerInputActions();
        m_input.Desktop.Click.performed += Clicked;
        m_player = gameObject.GetComponent<Player>();
        if (m_player != null)
        {
            m_isPlayer = true;
        } else
        {
            m_isPlayer = false;
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

    private void Clicked(InputAction.CallbackContext ctx)
    {
        if (m_hovering && m_isPlayer)
        {
            Debug.Log("Found a player!");
            GameManager.Singleton.PlayerWasFoundServerRpc(m_player.OwnerClientId);
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
}

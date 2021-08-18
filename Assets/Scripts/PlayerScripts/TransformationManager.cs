using MLAPI;
using MLAPI.NetworkVariable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransformationManager : MonoBehaviour
{
    [SerializeField] private GameObject m_transformMenu;
    [SerializeField] private Dropdown m_transformDropdown;
    private Player m_localPlayer;

    void Start()
    {
        m_transformMenu.SetActive(false);
        NetworkManager.Singleton.OnClientConnectedCallback += AddTransformCallbacks;
    }

    private void AddTransformCallbacks(ulong client_id)
    {
        if (NetworkManager.Singleton.IsClient)
        {
            m_localPlayer = GameManager.Singleton.GetPlayerComponent(NetworkManager.Singleton.LocalClientId);
            m_localPlayer.IsCaught.OnValueChanged += ToggleTransformMenu;
        }
        else
        {
            Destroy(m_transformMenu);
            Destroy(this);
        }
    }

    private void ToggleTransformMenu(bool wasCaught, bool isCaught)
    {
        m_transformMenu.SetActive(!isCaught && !m_localPlayer.IsSeeker.Value);
    }
}

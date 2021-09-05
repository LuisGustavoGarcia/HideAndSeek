using MLAPI;
using MLAPI.Messaging;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransformationManager : NetworkBehaviour
{
    public static TransformationManager Singleton = null;

    [SerializeField] private GameObject m_transformMenu;
    [SerializeField] private Dropdown m_transformDropdown;
    private Player m_localPlayer;
    private Sprite m_playerSprite;

    private void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
            DontDestroyOnLoad(this.gameObject);
            m_transformMenu.SetActive(false);
        }
        else if (Singleton != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PopulateTransformDropdown();
        NetworkManager.Singleton.OnServerStarted += ServerCallbacks;
    }

    private void ServerCallbacks()
    {
        if (NetworkManager.Singleton.IsClient)
        {
            m_localPlayer = GameManager.Singleton.GetPlayerComponent(NetworkManager.Singleton.LocalClientId);
            m_playerSprite = m_localPlayer.GetComponent<SpriteRenderer>().sprite;
        }
    }

    private void PopulateTransformDropdown()
    {
        List<string> spriteNames = TransformSprites.Singleton.GetAllTransformSpriteNames();
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        
        // Add default player option.
        Dropdown.OptionData playerOption = new Dropdown.OptionData("PLAYER");
        options.Add(playerOption);
        
        // Add option for available sprites/objects.
        foreach (string spriteName in spriteNames)
        {
            Dropdown.OptionData option = new Dropdown.OptionData(spriteName);
            options.Add(option);
        }

        m_transformDropdown.AddOptions(options);
        m_transformDropdown.onValueChanged.AddListener(TransformPlayer);
    }

    public void ToggleTransformMenu(bool active)
    {
        m_transformMenu.SetActive(active);
    }

    private void TransformPlayer(int value)
    {
        if (NetworkManager.Singleton.IsClient)
        {
            Debug.Log("Player wants to transform.");
            string obj = m_transformDropdown.options[value].text;
            GameManager.Singleton.PlayerWantsToTransformServerRpc(obj, NetworkManager.Singleton.LocalClientId);
        }
    }

    public void UpdatePlayerModel(string transformObj, Animator animator, SpriteRenderer spriteRenderer)
    {
        if (transformObj == "PLAYER")
        {
            spriteRenderer.sprite = m_playerSprite;
            animator.enabled = true;
        }
        else
        {
            spriteRenderer.sprite = TransformSprites.Singleton.GetObjectSprite(transformObj);
            animator.enabled = false;
        }
    }
}

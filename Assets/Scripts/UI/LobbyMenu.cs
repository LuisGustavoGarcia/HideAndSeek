using UnityEngine;
using MLAPI;
using MLAPI.Transports.UNET;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{
    [SerializeField] private GameObject m_menuPanel;
    [SerializeField] private InputField m_inputField;
    [SerializeField] private Button m_joinServerButton;
    [SerializeField] private Button m_startServerButton;

    private void Start()
    {
        CheckCommandLineFlags();
        m_joinServerButton.onClick.AddListener(JoinAsClient);
        m_startServerButton.onClick.AddListener(StartServer);
    }

    private void CheckCommandLineFlags()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-launch-as-client")
            {
                JoinAsClient();
            }
            if (args[i] == "-launch-as-server")
            {
                StartServer();
            }
        }
    }

    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
        m_menuPanel.SetActive(false);
        Debug.Log("Started Server");
    }

    public void JoinAsClient()
    {
        if (m_inputField.text.Length <= 0)
        {
            var ip = "127.0.0.1";
            NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = ip;
        } 
        else
        {
            var ip = m_inputField.text;
            NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = ip;
        }
        NetworkManager.Singleton.StartClient();
        m_menuPanel.SetActive(false);
        Debug.Log("Started Client");
    }
}

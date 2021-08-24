using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : NetworkBehaviour
{
    Camera camera;
    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponent<Camera>();
        NetworkManager.Singleton.OnClientConnectedCallback += PlayerConnected;
    }

    void PlayerConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            MoveCameraToPlayer(clientId);
        }
    }

    public void MoveCameraToPlayer(ulong clientId)
    {
        Player player = GameManager.Singleton.GetPlayerComponent(clientId);
        camera.gameObject.transform.SetParent(player.gameObject.transform);
    }
}

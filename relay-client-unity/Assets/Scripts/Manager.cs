using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RelayClient;

public class Manager : MonoBehaviour
{
    public string ip = "127.0.0.1";
    public int port = 3333;

    // Start is called before the first frame update
    void Start()
    {
        Main.Connect(ip, port);
        RelayClient.Client.MessagesIncoming.OnLobbyJoined = (bool Success, int Id) => { Debug.Log("OnLobbyJoined: " + Success + " Id:"+Id); };
        RelayClient.Client.MessagesIncoming.OnLobbyUpdate = (bool IsHost) => { Debug.Log("OnLobbyUpdate-> IsHost:" + IsHost); };
        RelayClient.Network.Actions.OnIdentityUpdate = (RelayClient.Network.Identity _Identity) =>
        {
            NetworkObject networkObject = NetworkObject.List.Find(x => x.Id == _Identity.Id);
            if (networkObject == null)
            {
                Debug.Log("Spawn Identity(): " + _Identity.i);
                GameObject Asset = Resources.Load<GameObject>(_Identity.Asset);
                Asset = Instantiate(Asset);
                networkObject = Asset.AddComponent<NetworkObject>();
                networkObject.Id = _Identity.Id;
                networkObject.Sync(true); 
            }
        };

        //RelayClient.Client.MessagesIncoming.OnP2P = (string Message, int Sender) => { Debug.Log("Message: " + Message + ", Sender: " + Sender); };
        RelayClient.Network.Actions.OnError = (string Error) => { Debug.Log(Error); };
    }

    // Update is called once per frame
    void Update()
    {
        Main.Update ();
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 100, 20), "JoinLobby"))
        {
            RelayClient.Client.Methods.JoinLobby();
        }

        if (GUI.Button(new Rect(0, 50, 100, 20), "Spawnplayer"))
        {
            RelayClient.Network.Spawner.SpawnPlayer("Cube", NetworkObject.VectorToFloat (new Vector3 (0,0,1)), NetworkObject.VectorToFloat(new Vector3(0, 0, 45)));
        }
    }

    float nextUpdate = 0;
    /// <summary>
    /// Network update at late update.
    /// </summary>
    private void LateUpdate()
    {
        if (nextUpdate < Time.time)
        {
            nextUpdate = Time.time + 0.2f; // 5 times in a second.
            RelayClient.Network.Identity.Update();
        }
    }

    private void OnDestroy()
    {
        Main.Close();
    }
}

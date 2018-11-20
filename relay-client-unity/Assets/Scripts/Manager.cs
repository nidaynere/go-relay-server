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
        RelayClient.Network.Actions.OnIdentityUpdate = (RelayClient.Network.Identity Identity) => 
        {
            Debug.Log("OnIdentityUpdate->"+ Identity.Asset + ", Total Identity:  "+RelayClient.Network.Identity.List.Count);
            if (!RelayClient.Network.Identity.List.Contains(Identity))
            {
                Debug.Log("Spawn Identity()");
                GameObject Asset = Resources.Load<GameObject>(Identity.Asset);
                Asset = Instantiate(Asset);
                Asset.AddComponent<NetworkObject>().Identity = Identity;
            }
        };
        RelayClient.Client.MessagesIncoming.OnP2P = (string Message, int Sender) => { Debug.Log("Message: " + Message + ", Sender: " + Sender); };
        RelayClient.Network.Actions.OnError = (string Error) => { Debug.Log(Error); };
    }

    // Update is called once per frame
    void Update()
    {
        Main.Update ();

        //Test message send.
        if (Input.GetKeyDown(KeyCode.S))
        {
            RelayClient.Client.Methods.JoinLobby();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            RelayClient.Network.Spawner.Spawn("Cube", new float[3] { 1, 1, 1 }, new float[3] { 2, 2, 2 });
        }
        /*
        //Test message send.
        if (Input.GetKeyDown(KeyCode.D))
        {
            RelayClient.Client.Methods.RelayToLobby("seks var");
        }*/



        //MessagesIncoming.OnLobbyJoined = (bool b2, int id) => { OnLobbyJoined(b2, id); };
        //essagesIncoming.OnP2P = (string msg, int sender) => { OnP2P(msg, sender); };
    }
    /*
    void OnLobbyJoined (bool b, int id)
    {
        Debug.Log("OnLobbyCreated() isSuccess: " + b + " User Id: " + id);
    }

    void OnP2P(string msg, int sender)
    {
        Debug.Log("Peer to peer message: " + msg + " sender: " + sender);
    }*/

    private void OnDestroy()
    {
        Main.Close();
    }
}

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
        Client.Connect(ip, port);
    }

    // Update is called once per frame
    void Update()
    {
        Client.Update ();

        //Test message send.
        if (Input.GetKeyDown(KeyCode.S))
        {
            Methods.JoinLobby();
        }

        //Test message send.
        if (Input.GetKeyDown(KeyCode.D))
        {
            Methods.RelayToLobby("seks var");
        }

        MessagesIncoming.OnLobbyJoined = (bool b2) => { OnLobbyJoined(b2); };
        MessagesIncoming.OnP2P = (string msg) => { OnP2P(msg); };
    }

    void OnLobbyJoined (bool b)
    {
        Debug.Log("OnLobbyCreated() isSuccess: " + b);
    }

    void OnP2P(string msg)
    {
        Debug.Log("Peer to peer message: " + msg);
    }

    private void OnDestroy()
    {
        Client.Close();
    }
}

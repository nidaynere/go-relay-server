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

        MessagesIncoming.OnLobbyJoined = (bool b2, int id) => { OnLobbyJoined(b2, id); };
        MessagesIncoming.OnP2P = (string msg, int sender) => { OnP2P(msg, sender); };
    }

    void OnLobbyJoined (bool b, int id)
    {
        Debug.Log("OnLobbyCreated() isSuccess: " + b + " User Id: " + id);
    }

    void OnP2P(string msg, int sender)
    {
        Debug.Log("Peer to peer message: " + msg + " sender: " + sender);
    }

    private void OnDestroy()
    {
        Client.Close();
    }
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RelayClient;
using Unity.Entities;

public class Manager : MonoBehaviour
{
    public List<RelayClient.Network.Identity> List = new List<RelayClient.Network.Identity>();
    public string ip = "127.0.0.1";
    public int port = 3333;

    // Start is called before the first frame update
    void Start()
    {
        Main.Connect(ip, port);
        RelayClient.Client.MessagesIncoming.OnLobbyJoined = (bool Success, int Id) => { Debug.Log("OnLobbyJoined: " + Success + " Id:"+Id); };
        RelayClient.Client.MessagesIncoming.OnLobbyUpdate = (bool IsHost, int Disconnected, int Connected) => 
        {
            Debug.Log("OnLobbyUpdate-> IsHost:" + IsHost);

            if (Disconnected != 0)
            {
                Debug.Log("Player Disconnected: " + Disconnected);
            }

            if (Connected != 0)
            {
                Debug.Log("Player Connected: " + Connected);
            }
        };

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
                networkObject.CertainUpdate = true;

                Asset.AddComponent<GameObjectEntity>();

                if (_Identity.Id == RelayClient.Client.NetworkVariables.ConnectionId)
                {
                    /// Our player!
                    Asset.AddComponent<Player.PlayerController>();
                }
            }
        };

        //RelayClient.Client.MessagesIncoming.OnP2P = (string Message, int Sender) => { Debug.Log("Message: " + Message + ", Sender: " + Sender); };
        RelayClient.Network.Actions.OnError = (string Error) => { Debug.Log(Error); };
    }

    // Update is called once per frame
    void Update()
    {
        Main.Update ();

        List = RelayClient.Network.Identity.List;
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 100, 20), "JoinLobby"))
        {
            RelayClient.Client.Methods.JoinLobby();
        }

        if (GUI.Button(new Rect(0, 50, 100, 20), "Spawnplayer"))
        {
            RelayClient.Network.Spawner.SpawnPlayer("Guardian", new float[] { 0, 0, 2 }, new float[] { 0, 0, 0 });
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
            nextUpdate = Time.time + 0.1f; // 10 times in a second.
            RelayClient.Network.Identity.Update();
        }
    }

    private void OnDestroy()
    {
        Main.Close();
    }
}

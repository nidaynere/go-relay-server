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

    [System.Serializable]
    /// Base network message
    public class NetworkMessage {
        public int t; // Message Purpose=> 0=CreateLobby, 1=JoinLobby, 2=RequestLobbies, 4=RelayToLobby
    }

    // Update is called once per frame
    void Update()
    {
        string incomingMessage = Client.Read();
        if (!string.IsNullOrEmpty(incomingMessage))
        {
            Debug.Log(incomingMessage);
        }

        //Test message send.
        if (Input.GetKeyDown(KeyCode.A))
        {
            Client.Write(JsonUtility.ToJson(new NetworkMessage() { t = 1 } ));
        }
    }

    private void OnDestroy()
    {
        Client.Close();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RelayClient;
using Unity.Entities;

public class Manager : MonoBehaviour
{
    public List<RelayClient.Network.Identity> List = new List<RelayClient.Network.Identity>();
    public List<RelayClient.Client.NetworkVariables.Variables.VariableCallback> Callbacks = new List<RelayClient.Client.NetworkVariables.Variables.VariableCallback>();
    public string ip = "127.0.0.1";
    public int port = 3333;

    // Start is called before the first frame update
    void Start()
    {
        Main.Connect(ip, port);

        //TEST RelayClient.Client.Methods.Relaying = () => { Debug.Log("Message sending...");  };

        Main.OnNetworkMessage = (string Msg) => { Debug.Log(Msg); };
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
            if (networkObject == null && _Identity.Variables.GetVariableAsFloat ("Destroy") == 0)
            {
                Debug.Log("Spawn Identity(): " + _Identity.i);
                GameObject Asset = Resources.Load<GameObject>(_Identity.Asset);
                Asset = Instantiate(Asset);
                networkObject = Asset.AddComponent<NetworkObject>();
                networkObject.Id = _Identity.Id;
                networkObject.CertainUpdate = true;

                ///Destroy callback implementation
                RelayClient.Client.NetworkVariables.Variables.AddVariableCallback(_Identity.Id, "Destroy").OnChanged = 
                (string OldValue, string NewValue) => {
                    float destroyTime = float.Parse(NewValue);

                    Debug.Log("Destroy on changed: " + NewValue);
                    if (destroyTime > 0)
                    {
                        Debug.Log(Time.time);
                        Debug.Log ("Destroying asset: " + destroyTime);
                        Destroy(Asset, destroyTime);
                        networkObject.visual.UpdateMaterials();
                        networkObject.visual.OnUpdate += networkObject.visual.FadeOut;

                        RelayClient.Network.Identity willRemove = RelayClient.Network.Identity.List.Find(x => x.Id == networkObject.Id);
                        willRemove?.OnDestroyed();
                    }
                };
                //

                Asset.AddComponent<GameObjectEntity>();

                if (Asset.CompareTag("Agent"))
                {
                    if (_Identity.Variables.GetVariableAsBool("IsPlayer"))
                    {
                        if (_Identity.Id == RelayClient.Client.NetworkVariables.ConnectionId)
                        {
                            /// Our player!
                            Agents.PlayerController.Instance = Asset.AddComponent<Agents.PlayerController>();
                        }
                        else
                        {
                            Asset.AddComponent<Agents.Agent>();
                        }
                    }
                    else
                    {
                        Asset.AddComponent<Agents.AIController>();
                    }
                }
                else networkObject.OnUpdate += networkObject.SmoothSync;
            }

            if (networkObject)
            {
                networkObject.NetworkUpdate(_Identity);
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
        Callbacks = RelayClient.Client.NetworkVariables.Variables.Callbacks;
    }

    private void LateUpdate()
    {
        NetworkUpdater.Update();
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

        if (GUI.Button(new Rect(0, 100, 100, 20), "SpawnBoxFronOfMyChar"))
        {
            RelayClient.Network.Spawner.Spawn ("Skeleton", NetworkObject.Vector3ToFloat (Agents.PlayerController.Instance.transform.position + Agents.PlayerController.Instance.transform.forward*4), new float[] { 0, 0, 0 });
        }
    }

    private void OnDestroy()
    {
        Main.Close();
    }
}

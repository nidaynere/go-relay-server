using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Newtonsoft.Json;

namespace RelayClient
{
    /// <summary>
    /// Initilization class of Relay.
    /// </summary>
    public class Main
    {
        /// <summary>
        /// TCP Client
        /// </summary>
        public static TcpClient client;

        /// <summary>
        /// Connect to the relay server
        /// </summary>
        public static void Connect(string ip, int port) {
            client = new TcpClient();
            client.ReceiveBufferSize = 65535;
            client.SendBufferSize = 65535;
            client.SendTimeout = 5;
            client.ReceiveTimeout = 5;
            client.Connect(ip, port);
        }

        /// <summary>
        /// Closes the server connection.
        /// </summary>
        public static void Close() {
            if (client != null && client.Connected)
            {
                client.Close();
                Client.MessagesIncoming.OnConnectionClosed?.Invoke();
            }
        }

        /// <summary>
        /// Must be called on Update.
        /// This will return null string if no message received.
        /// </summary>
        private static string Read () {
            if (!client.Connected || client.Available == 0)
                return null;

            Stream stm = client.GetStream();

            byte[] bb = new byte[1024]; // Max message length;

            int k = stm.Read(bb, 0, bb.Length);

            if (k > 0) // Message received
            {
                string incomingMessage = "";
                for (int i = 0; i < k; i++)
                    incomingMessage += Convert.ToChar(bb[i]);

                return incomingMessage;
            }

            return null;
        }

        /// <summary>
        /// Call this to get actions.
        /// </summary>
        public static void Update()
        {
            if (!client.Connected)
                return;

            string Msg = Read();
            if (!string.IsNullOrEmpty(Msg))
            {
                var list = JsonExtensions.FromDelimitedJson<Client.MessagesIncoming.NetworkMessage>(new StringReader(Msg)).ToList();

                foreach (Client.MessagesIncoming.NetworkMessage message in list)
                {
                    switch (message.t)
                    {
                        case Client.MessagesIncoming.MessageType.JoinLobby:
                            Client.MessagesIncoming.OnLobbyJoined?.Invoke(message.jl.Success, message.jl.Id);
                            Client.NetworkVariables.ConnectionId = message.jl.Id;
                            break;

                        case Client.MessagesIncoming.MessageType.P2P:
                            Client.MessagesIncoming.OnP2P?.Invoke(message.m.Msg, message.m.s); // Available for custom methods.

                            try
                            {
                                Network.Identity identity = JsonConvert.DeserializeObject<Network.Identity>(message.m.Msg);

                                if (identity.netType == Network.Identity.NetworkType.Request)
                                { // Host assigns an id here.
                                    identity.Id = Network.Identity.IdCounter++;
                                    identity.netType = Network.Identity.NetworkType.Post;
                                    identity.Spawn();
                                }
                                else
                                {
                                    //This is a post.
                                    identity.OnSpawned();
                                    Network.Actions.OnIdentityUpdate?.Invoke(identity);
                                }
                            }
                            catch {

                            }

                            break;

                        case Client.MessagesIncoming.MessageType.LobbyUpdate:
                            Client.MessagesIncoming.OnLobbyUpdate?.Invoke(message.lu.IsHost, message.lu.DC, message.lu.C);

                            // Lobby update
                            Client.NetworkVariables.IsHost = message.lu.IsHost;

                            if (message.lu.DC != 0)
                            {
                                // Disconnected connection, remove spawned player if exist.
                                Network.Identity.List.Remove (Network.Identity.List.Find(x => x.Id == message.lu.DC));
                            }

                            break;

                        case Client.MessagesIncoming.MessageType.LobbyLeave:
                            Client.MessagesIncoming.OnLobbyLeave?.Invoke();
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Sends message to relay server. But this method cannot be called outside of this API.
        /// </summary>
        /// <param name="str"></param>
        public static void Write (String str)
        {
            if (!client.Connected)
                return;

            Stream stm = client.GetStream();
            ASCIIEncoding asen = new ASCIIEncoding();
            byte[] ba = asen.GetBytes(str);

            stm.Write(ba, 0, ba.Length);
        }
    }
}

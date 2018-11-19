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
    public class Client
    {
        /// <summary>
        /// TCP Client
        /// </summary>
        private static TcpClient client;

        /// <summary>
        /// Connect to a relay server.
        /// </summary>
        public static void Connect(string ip, int port) {
            client = new TcpClient();
            client.Connect(ip, port);
            client.NoDelay = true;
        }

        /// <summary>
        /// Closes the server connection.
        /// </summary>
        public static void Close() {
            if (client != null && client.Connected)
            {
                client.Close();
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
            string Msg = Read();
            if (!string.IsNullOrEmpty(Msg))
            {
                    MessagesIncoming.NetworkMessage message = JsonConvert.DeserializeObject<MessagesIncoming.NetworkMessage>(Msg);

                    switch (message.t)
                    {
                        case MessagesIncoming.MessageType.JoinLobby:
                            MessagesIncoming.OnLobbyJoined(message.jl.Success);
                            break;

                        case MessagesIncoming.MessageType.P2P:
                            MessagesIncoming.OnP2P(message.m.Msg);
                            break;

                        case MessagesIncoming.MessageType.LobbyUpdate:
                            MessagesIncoming.OnLobbyUpdate(message.lu.Members);
                            break;

                        case MessagesIncoming.MessageType.LobbyLeave:
                            MessagesIncoming.OnLobbyLeave();
                            break;
                    }
            }
        }

        /// <summary>
        /// Sends message to relay server. But this method cannot be called outside of this API.
        /// </summary>
        /// <param name="str"></param>
        public static void Write (String str) {
            if (!client.Connected)
                return;

            Stream stm = client.GetStream();
            ASCIIEncoding asen = new ASCIIEncoding();
            byte[] ba = asen.GetBytes(str);

            stm.Write(ba, 0, ba.Length);
        }
    }
}

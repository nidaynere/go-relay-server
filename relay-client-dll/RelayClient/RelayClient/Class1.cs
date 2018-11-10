using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Newtonsoft.Json;

/// <summary>
/// TODO 3 actions need to do. OnLobbyCreated, OnLobbyJoined, OnLobbyUpdate, OnLobbyList, OnP2PReceived
/// </summary>

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
        public static string Read () {
            if (!client.Connected || client.Available == 0)
                return null;

            Stream stm = client.GetStream();

            byte[] bb = new byte[100];

            int k = stm.Read(bb, 0, bb.Length);

            if (k > 0) // Message received
            {
                string incomingMessage = "";
                Console.Write("\n" + k);
                for (int i = 0; i < k; i++)
                    incomingMessage += Convert.ToChar(bb[i]);

                return incomingMessage;
            }

            return null;
        }

        /// <summary>
        /// Network message types.
        /// </summary>
        private enum MessageType {
            CreateLobby,
            JoinLobby,
            RequestLobbies,
            RelayToLobby
        }

        private class NetworkMessage
        {
            public MessageType t;
            public _CreateLobby cl;
            public _JoinLobby jl;
            public _RequestLobbies rl;
            public _RelayToLobby m;

            public NetworkMessage(_CreateLobby message)
            {
                t = MessageType.CreateLobby;
                cl = message;
            }

            public NetworkMessage(_JoinLobby message)
            {
                t = MessageType.JoinLobby;
                jl = message;
            }

            public NetworkMessage(_RequestLobbies message)
            {
                t = MessageType.RequestLobbies;
                rl = message;
            }

            public NetworkMessage(_RelayToLobby message)
            {
                t = MessageType.RelayToLobby;
                m = message;
            }
        }

        private class _CreateLobby
        {
            public string n;

            public _CreateLobby(string LobbyName) { n = LobbyName;  }
        }

        private class _JoinLobby
        {
            public int i;

            public _JoinLobby (int Index) { i = Index; }
        }

        private class _RequestLobbies
        {
            public int p;

            public _RequestLobbies(int Page) { p = Page; }
        }

        private class _RelayToLobby
        {
            public string m;

            public _RelayToLobby(string message) { m = message; }
        }

        /// <summary>
        /// Call this to Create Lobby.
        /// </summary>
        /// <param name="lobbyName">Lobby Name</param>
        public static void CreateLobby (string lobbyName) {
            Write (JsonConvert.SerializeObject(new NetworkMessage(new _CreateLobby(lobbyName))));
        }

        /// <summary>
        /// Join this to join a lobby.
        /// </summary>
        /// <param name="lobbyIndex">Target lobby index</param>
        public static void JoinLobby(int lobbyIndex) {
            Write (JsonConvert.SerializeObject(new NetworkMessage (new _JoinLobby(lobbyIndex))));
        }

        /// <summary>
        /// Request lobby list.
        /// </summary>
        /// <param name="page">Page number. 0 means 0-10, 1 means 11-20</param>
        public static void RequestLobbies (int page) {
            Write(JsonConvert.SerializeObject(new NetworkMessage (new _RequestLobbies(page))));
        }

        /// <summary>
        /// Relay the message to lobby.
        /// </summary>
        /// <param name="message"></param>
        public static void RelayToLobby (string message) {
            Write(JsonConvert.SerializeObject(new NetworkMessage (new _RelayToLobby (message))));
        }

        /// <summary>
        /// Sends message to relay server. But this method cannot be called outside of this API.
        /// </summary>
        /// <param name="str"></param>
        private static void Write (String str) {
            if (!client.Connected)
                return;

            Stream stm = client.GetStream();
            ASCIIEncoding asen = new ASCIIEncoding();
            byte[] ba = asen.GetBytes(str);

            stm.Write(ba, 0, ba.Length);
        }
    }
}

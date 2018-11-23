using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RelayClient
{
    namespace Client
    {
        public static class Methods
        {
            /// <summary>
            /// Join this to join a lobby.
            /// </summary>
            /// <param name="lobbyIndex">Target lobby index</param>
            public static void JoinLobby()
            {
                Main.Write(JsonConvert.SerializeObject(new MessagesOutgoing.NetworkMessage(MessagesOutgoing.MessageType.JoinLobby)));
            }

            public static Action Relaying;
            /// <summary>
            /// Relay the message to lobby.
            /// </summary>
            /// <param name="message">Json string</param>
            public static void RelayToLobby(string message)
            {
                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] ba = asen.GetBytes(message);
                ba = CLZF2.Compress(ba);

                Relaying?.Invoke();
                Main.Write(JsonConvert.SerializeObject(new MessagesOutgoing.NetworkMessage(new MessagesOutgoing._RelayToLobby(ba))));
            }

            /// <summary>
            /// Relay the object to lobby.
            /// </summary>
            /// <param name="Object">Object to json</param>
            public static void RelayToLobby (object Object)
            {
                RelayToLobby (JsonConvert.SerializeObject(Object));
            }
        }
    }
}

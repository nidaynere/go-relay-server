using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RelayClient
{
    public static class Methods
    {
        /// <summary>
        /// Join this to join a lobby.
        /// </summary>
        /// <param name="lobbyIndex">Target lobby index</param>
        public static void JoinLobby()
        {
            Client.Write(JsonConvert.SerializeObject(new MessagesOutgoing.NetworkMessage( MessagesOutgoing.MessageType.JoinLobby)));
        }

        /// <summary>
        /// Relay the message to lobby.
        /// </summary>
        /// <param name="message"></param>
        public static void RelayToLobby(string message)
        {
            Client.Write(JsonConvert.SerializeObject(new MessagesOutgoing.NetworkMessage(new MessagesOutgoing._RelayToLobby(message))));
        }
    }
}

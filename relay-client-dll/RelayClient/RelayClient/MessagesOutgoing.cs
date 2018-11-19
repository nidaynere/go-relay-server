using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelayClient
{
    class MessagesOutgoing
    {
        /// <summary>
        /// Network message types.
        /// </summary>
        public enum MessageType
        {
            JoinLobby,
            RelayToLobby
        }

        public class NetworkMessage
        {
            public MessageType t;
            public _RelayToLobby m;


            public NetworkMessage(MessageType Purpose)
            {
                t = Purpose;
            }

            public NetworkMessage(_RelayToLobby message)
            {
                t = MessageType.RelayToLobby;
                m = message;
            }
        }

        public class _RelayToLobby
        {
            public string m;

            public _RelayToLobby(string message) { m = message; }
        }
    }
}

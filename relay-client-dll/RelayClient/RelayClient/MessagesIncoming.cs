using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelayClient
{
    public class MessagesIncoming
    {
        #region actions
            public static Action<bool> OnLobbyJoined;
            public static Action<int> OnLobbyUpdate;
            public static Action OnLobbyLeave;
            public static Action<string> OnP2P;
        #endregion

        /// <summary>
        /// Network message types.
        /// </summary>
        public enum MessageType
        {
            JoinLobby,
            P2P,
            LobbyUpdate,
            LobbyLeave
        }

        public class NetworkMessage
        {
            public MessageType t;
            public _JoinLobby jl;
            public _P2P m;
            public _LobbyUpdate lu;
        }

        public class _CreateLobby
        {
            public string Name;
            public bool Success;
        }

        public class _JoinLobby
        {
            public string Name;
            public bool Success;
        }

        public class _LobbyUpdate
        {
            public int Members;
        }

        public class _P2P
        {
            public string Msg;
        }
    }
}

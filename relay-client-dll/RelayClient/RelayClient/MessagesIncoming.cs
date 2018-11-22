using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelayClient
{
    namespace Client
    {
        public class MessagesIncoming
        {
            #region actions
            public static Action OnConnectionClosed;
            public static Action<bool, int> OnLobbyJoined;
            public static Action<bool, int, int> OnLobbyUpdate;
            public static Action OnLobbyLeave;
            public static Action<string, int> OnP2P;
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

            public class _JoinLobby
            {
                public int Id;
                public bool Success;
            }

            public class _LobbyUpdate
            {
                /// <summary>
                /// Our client's host status. False if we are not the master of the lobby.
                /// </summary>
                public bool IsHost;

                /// <summary>
                /// Any connection is disconnected?, 0 if no disconnection.
                /// </summary>
                public int DC;

                /// <summary>
                /// New connection?
                /// </summary>
                public int C;
            }

            public class _P2P
            {
                /// <summary>
                /// Sender
                /// </summary>
                public int s;
                public string Msg;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelayClient
{
    namespace Client
    {
        class NetworkVariables
        {
            /// <summary>
            /// Our connection ID.
            /// </summary>
            public static int ConnectionId;
            /// <summary>
            /// Returns true If we are the host of the lobby.
            /// </summary>
            public static bool IsHost;
        }
    }
}

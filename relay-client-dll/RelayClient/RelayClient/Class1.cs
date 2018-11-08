using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

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
        public static void Close()
        {
            if (client != null && client.Connected)
            {
                client.Close();
            }
        }

        /// <summary>
        /// Must be called on Update.
        /// This will return null string if no message received.
        /// </summary>
        public static string Read ()
        {
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
        /// Sends message to relay server.
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

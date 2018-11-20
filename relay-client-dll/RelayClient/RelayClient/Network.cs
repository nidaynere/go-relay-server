using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

/// <summary>
/// Network, works after joining into a lobby.
/// </summary>
namespace RelayClient
{
    namespace Network
    {
        public class Actions
        {
            public static Action<Identity> OnIdentityUpdate;
            public static Action<string> OnError;
        }

        /// <summary>
        /// Networked object identity;
        /// </summary>
        [Serializable]
        public class Identity
        {
            public enum NetworkType
            {
                Post, // For lobby host
                Request // For lobby members
            }

            public enum ObjectType
            {
                Player, // Playable character.
                Object // Other objects.
            }

            /// <summary>
            /// Object type.
            /// </summary>
            public NetworkType netType;
            public ObjectType objType;

            /// <summary>
            /// List of all networked objects;
            /// </summary>
            public static List<Identity> List = new List<Identity>();

            /// <summary>
            /// Destroyed network object actions.
            /// </summary>
            public void OnDestroyed()
            {
                List.Remove(this);
            }

            /// <summary>
            /// Created network object actions.
            /// </summary>
            public void Spawn ()
            {
                //Relay to network.
                Client.Methods.RelayToLobby(this);
            }

            public void OnSpawned()
            {
                List.Add(this);
            }

            /// <summary>
            /// Id counter for networked objects;
            /// </summary>
            public static int IdCounter;

            #region Json Variables
            public string p; // AssetName;
            public Client.Transform t; // Transform of the object;
            public int i; // Id;
            #endregion

            [JsonIgnore]
            public string Asset { get { return p; } set { p = value; } }
            [JsonIgnore]
            public Client.Transform Transform { get { return t; } set { t = value; } }
            [JsonIgnore]
            public int Id { get { return i; } set { i = value; } }

            /// <summary>
            /// Create a networked object with automatic ID;
            /// </summary>
            /// <param name="_Asset">Asset name</param>
            /// <param name="_Position">Position</param>
            /// <param name="_Rotation">Rotation</param>
            public Identity(string _Asset, float[] _Position, float[] _Angles)
            {
                Asset = _Asset;
                Transform = new Client.Transform(_Position, _Angles);
            }

            /// <summary>
            /// Create a networked object with customId; Mostly used for player characters.
            /// </summary>
            /// <param name="_Asset">Asset name</param>
            /// /// <param name="_Id">Custom Id</param>
            /// <param name="_Position">Position</param>
            /// <param name="_Rotation">Rotation</param>
            public Identity(string _Asset, int _Id, float[] _Position, float[] _Angles)
            {
                Id = _Id;
                Asset = _Asset;
                Transform = new Client.Transform(_Position, _Angles);
            }

            /// <summary>
            /// Default constructor for Json deserialize.
            /// </summary>
            public Identity()
            {

            }
        }

        /// <summary>
        /// Network object spawner
        /// </summary>
        public class Spawner
        {
            /// <summary>
            /// Spawn a networked object.
            /// </summary>
            /// <param name="_Asset">Asset name</param>
            public static void Spawn (string _Asset, float[] _Position, float[] _Angles)
            {
                Identity New = new Identity(_Asset, _Position, _Angles);
                New.netType = Client.NetworkVariables.IsHost ? Identity.NetworkType.Post : Identity.NetworkType.Request;
                New.Spawn ();
            }

            /// <summary>
            /// Spawn a networked object by Custom Id, Only for player spawning.
            /// </summary>
            /// <param name="_Asset">Asset name</param>
            /// <param name="_Id">Custom Id</param>
            public static void Spawn(string _Asset, int _Id, float[] _Position, float[] _Angles)
            {
                Identity New = new Identity(_Asset, _Id, _Position, _Angles);
                New.netType = Identity.NetworkType.Post;
                New.Spawn ();
            }
        }
    }
}

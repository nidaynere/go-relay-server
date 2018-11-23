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
            /// Need to retrieve info for this identity.
            /// </summary>
            /// <param name="identity"></param>
            /// <returns></returns>
            public static bool NeedToRetrieve(Identity identity)
            {
                if (identity.objType == ObjectType.Player)
                {
                    if (identity.Id != Client.NetworkVariables.ConnectionId)
                        return true;
                    else return false;
                }
                else
                {
                    return !Client.NetworkVariables.IsHost;
                }
            }

            /// <summary>
            /// Need to sync this identity.
            /// </summary>
            /// <param name="identity"></param>
            /// <returns></returns>
            public static bool NeedToSync (Identity identity)
            {
                if (identity.objType == ObjectType.Player)
                {
                    if (identity.Id == Client.NetworkVariables.ConnectionId)
                        return true;
                    else return false;
                }
                else
                {
                    return Client.NetworkVariables.IsHost;
                }
            }

            /// <summary>
            /// List of all networked objects;
            /// </summary>
            public static List<Identity> List = new List<Identity>();

            /// <summary>
            /// Destroyed network object actions.
            /// </summary>
            public void OnDestroyed ()
            {
                // Remove this from List, it should be removed on the application.
                Client.NetworkVariables.Variables.RemoveCallbacks(Id);
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
                int Index = List.FindIndex(x => x.Id == Id);
                if (Index != -1)
                {
                    List[Index] = this; // Replace.
                    Client.NetworkVariables.Variables.CheckCallbacks(Id);
                }
                else
                {
                    if (Variables.GetVariableAsFloat("Destroy") == 0)
                    {
                        List.Add(this);
                    }
                }
            }

            /// <summary>
            /// Id counter for networked objects;
            /// </summary>
            public static int IdCounter = 1;

            #region Json Variables
            public string p; // AssetName;
            public int i; // Id;
            #endregion

            /// <summary>
            /// Variables like animator values, health, damage, speed etc. Store anything in this.
            /// </summary>
            public Client.NetworkVariables.Variables Variables = new Client.NetworkVariables.Variables();

            [JsonIgnore]
            public string Asset { get { return p; } set { p = value; } }
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
                Variables = new Client.NetworkVariables.Variables();
                Variables.SetVariable("PositionX", _Position[0].ToString());
                Variables.SetVariable("PositionY", _Position[1].ToString());
                Variables.SetVariable("PositionZ", _Position[2].ToString());
                Variables.SetVariable("AngleX", _Angles[0].ToString());
                Variables.SetVariable("AngleY", _Angles[1].ToString());
                Variables.SetVariable("AngleZ", _Angles[2].ToString());
                Variables.SetVariable("Destroy", 0);
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

                Variables.SetVariable("PositionX", _Position[0].ToString());
                Variables.SetVariable("PositionY", _Position[1].ToString());
                Variables.SetVariable("PositionZ", _Position[2].ToString());
                Variables.SetVariable("AngleX", _Angles[0].ToString());
                Variables.SetVariable("AngleY", _Angles[1].ToString());
                Variables.SetVariable("AngleZ", _Angles[2].ToString());
                Variables.SetVariable("Destroy", 0);
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
                New.Id = Identity.IdCounter++;
                New.objType = Identity.ObjectType.Object;
                New.netType = Client.NetworkVariables.IsHost ? Identity.NetworkType.Post : Identity.NetworkType.Request;
                New.Spawn ();
            }

            /// <summary>
            /// Spawn a networked object by Custom Id, Only for player spawning.
            /// </summary>
            /// <param name="_Asset">Asset name</param>
            /// <param name="_Id">Custom Id</param>
            public static void SpawnPlayer (string _Asset, float[] _Position, float[] _Angles)
            {
                if (Identity.List.Find(x => x.Id == Client.NetworkVariables.ConnectionId) != null)
                    return; // Already spawned.

                Identity New = new Identity(_Asset, Client.NetworkVariables.ConnectionId, _Position, _Angles);
                New.netType = Identity.NetworkType.Post;
                New.objType = Identity.ObjectType.Player;
                New.Variables.SetVariable("IsPlayer", true);
                New.Spawn ();
            }
        }
    }
}

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
        //Generic variable, Only string accepted.
        public class Variable
        {
            public string Id;
            public string Value;
        }

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
            /// Update Identities to all lobby, if its host.
            /// </summary>
            public static void Update()
            {
                if (!Main.client.Connected)
                    return;

                for (int i = 0; i < List.Count; i++)
                {
                    if (List[i] != null)
                    {
                        if (NeedToSync (List[i]))
                            List[i].Spawn();
                    }
                }
            }

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
                int Index = List.FindIndex(x => x.Id == Id);
                if (Index != -1)
                    List[Index] = this; // Replace.
                else
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

            /// <summary>
            /// Variables like animator values, health, damage, speed etc. Store anything in this.
            /// </summary>
            public List<Variable> Variables = new List<Variable>();

            /// <summary>
            /// Add/Set variable to variables.
            /// </summary>
            /// <param name="_Id"></param>
            /// <param name="_Value"></param>
            public void SetVariable(string _Id, string _Value) {
                Variable current = Variables.Find(x => x.Id == _Id);
                if (current == null)
                {
                    current = new Variable() { Id = _Id };
                    Variables.Add(current);
                }

                current.Value = _Value;
            }

            /// <summary>
            /// Returns a variable from variables.
            /// </summary>
            /// <param name="_Id"></param>
            /// <returns></returns>
            public string GetVariable(string _Id)
            {
                Variable current = Variables.Find(x => x.Id == _Id);
                if (current != null)
                    return current.Value;

                return "0";
            }

            /// <summary>
            /// Remove a variable.
            /// </summary>
            /// <param name="_Id"></param>
            public void RemoveVariable(string _Id)
            {
                Variable current = Variables.Find(x => x.Id == _Id);
                if (current != null)
                    Variables.Remove(current);
            }

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
                New.Id = Identity.IdCounter++;
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
                Identity New = new Identity(_Asset, Client.NetworkVariables.ConnectionId, _Position, _Angles);
                New.netType = Identity.NetworkType.Post;
                New.objType = Identity.ObjectType.Player;
                New.Spawn ();
            }
        }
    }
}

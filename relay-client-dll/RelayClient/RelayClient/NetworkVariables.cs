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
        public class NetworkVariables
        {
            /// <summary>
            /// Our connection ID.
            /// </summary>
            public static int ConnectionId;
            /// <summary>
            /// Returns true If we are the host of the lobby.
            /// </summary>
            public static bool IsHost;

            [Serializable]
            public class Variables
            {
                [System.Serializable]
                public class VariableCallback
                {
                    public int identity;
                    public string Id;
                    string OldValue;

                    public Action<string, string> OnChanged;
                    public void Check(string NewValue)
                    {
                        if (OldValue != NewValue)
                        {
                            OnChanged?.Invoke(OldValue, NewValue);
                            OldValue = NewValue;
                        }
                    }
                }

                public static List<VariableCallback> Callbacks = new List<VariableCallback>();

                public static VariableCallback AddVariableCallback(int identity, string Id)
                {
                    VariableCallback callback = new VariableCallback();
                    callback.identity = identity;
                    callback.Id = Id;
                    Callbacks.Add(callback);

                    return callback;
                }

                public static void RemoveCallbacks(int identity)
                {
                    List<VariableCallback> callbacks = Callbacks.FindAll(x => x.identity == identity);
                    foreach (VariableCallback callback in callbacks)
                        Callbacks.Remove(callback);
                }

                public static void CheckCallbacks(int identity)
                {
                    List<VariableCallback> callbacks = Callbacks.FindAll(x => x.identity == identity);
                    Network.Identity Identity = Network.Identity.List.Find(x => x.Id == identity);
                    foreach (VariableCallback callback in callbacks)
                    {
                        callback.Check(Identity.Variables.GetVariableAsString(callback.Id));
                    }
                }

                [Serializable]
                public class Variable
                {
                    public string Id;
                    public string Value;
                }

                public List<Variable> List = new List<Variable>();

                public Variable FindVariable(string _Id)
                {
                    return List.Find(x => x.Id == _Id);
                }

                /// <summary>
                /// Add/Set variable to variables.
                /// </summary>
                /// <param name="_Id"></param>
                /// <param name="_Value"></param>
                public void SetVariable(string _Id, object _Value)
                {
                    Variable current = List.Find(x => x.Id == _Id);
                    if (current == null)
                    {
                        current = new Variable() { Id = _Id };
                        List.Add(current);
                    }

                    current.Value = _Value.ToString ();
                }

                /// <summary>
                /// Returns a variable from variables.
                /// </summary>
                /// <param name="_Id"></param>
                /// <returns></returns>
                public string GetVariableAsString(string _Id)
                {
                    Variable current = List.Find(x => x.Id == _Id);
                    if (current != null)
                        return current.Value;

                    return "0";
                }

                public float GetVariableAsFloat(string _Id)
                {
                    Variable current = List.Find(x => x.Id == _Id);
                    if (current != null)
                        return float.Parse(current.Value);

                    return 0f;
                }

                public bool GetVariableAsBool(string _Id)
                {
                    Variable current = List.Find(x => x.Id == _Id);
                    if (current != null)
                        return bool.Parse(current.Value);

                    return false;
                }

                public int GetVariableAsInt (string _Id)
                {
                    Variable current = List.Find(x => x.Id == _Id);
                    if (current != null)
                        return int.Parse(current.Value);

                    return 0;
                }

                /// <summary>
                /// Remove a variable.
                /// </summary>
                /// <param name="_Id"></param>
                public void RemoveVariable(string _Id)
                {
                    Variable current = List.Find(x => x.Id == _Id);
                    if (current != null)
                        List.Remove(current);
                }
            }
        }
    }
}

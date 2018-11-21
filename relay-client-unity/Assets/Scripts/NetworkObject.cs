using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RelayClient.Network;

public class NetworkObject : MonoBehaviour
{
    /// <summary>
    /// Spawned network objects.
    /// </summary>
    public static List<NetworkObject> List = new List<NetworkObject>();

    Animator animator;

    void Awake ()
    {
        animator = GetComponent<Animator>();

        List.Add(this);
    }

    private void OnDestroy()
    {
        Identity identity = Identity.List.Find(x => x.Id == Id);
        if (identity != null)
            identity.OnDestroyed();

        List.Remove(this);
    }

    public static Vector3 FloatToVector(float[] array)
    {
        return new Vector3(array[0], array[1], array[2]);
    }

    public static float[] VectorToFloat(Vector3 vector)
    {
        return new float[3] { vector.x, vector.y, vector.z };
    }

    /// <summary>
    /// Identity Id
    /// </summary>
    public int Id;

    public void Sync (bool certain = false)
    {
        Identity identity = Identity.List.Find(x => x.Id == Id);
		
        #region get from network
        if (identity != null)
        {
            if (Identity.NeedToRetrieve(identity) || certain)
            {
                if (animator != null)
                {
                    foreach (AnimatorControllerParameter parameter in animator.parameters)
                    {
                        parameter.defaultFloat = float.Parse(identity.GetVariable(parameter.name));
                    }
                }

                Vector3 position = FloatToVector(identity.Transform.GetPosition());
                Vector3 angle = FloatToVector(identity.Transform.GetAngle());
                if (certain)
                { // This is certain order. Probably first spawn.
                    transform.position = position;
                    transform.eulerAngles = angle;
                }
                else
                {
                    transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime*10);
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(angle), Time.deltaTime * 10);
                }

            }
        }
        #endregion

        #region set to network
        if (identity != null)
        {
            if (Identity.NeedToSync(identity) || certain)
            {
                if (animator != null)
                {
                    foreach (AnimatorControllerParameter parameter in animator.parameters)
                    {
                        identity.SetVariable(parameter.name, parameter.defaultFloat.ToString());
                    }
                }

                identity.Transform.SetPosition(VectorToFloat(transform.position));
                identity.Transform.SetAngle(VectorToFloat(transform.eulerAngles));
            }
        }
        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        Sync();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RelayClient.Network;

public class NetworkObject : MonoBehaviour
{
    public static Vector3 FloatToVector(float[] array)
    {
        return new Vector3(array[0], array[1], array[2]);
    }

    public Identity Identity;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    /// <summary>
    /// TODO ENTITY COMPONENT SYSTEM-->
    /// </summary>
    // Update is called once per frame
    void Update()
    {
        if (Identity != null)
        {
            transform.position = FloatToVector (Identity.Transform.GetPosition());
            transform.eulerAngles = FloatToVector(Identity.Transform.GetAngle());
        }
    }
}

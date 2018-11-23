using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RelayClient.Network;
using Unity.Entities;

/// <summary>
/// This is the main network object behaviour. It will sync Transform, Rigidbody and Animator automatically.
/// You will probably not need to edit this.
/// </summary>
public class NetworkObject : MonoBehaviour
{
    public static float Decimal(float value)
    {
        return Mathf.Floor(value * 100) / 100;
    }

    public static float[] Vector3ToFloat(Vector3 vector)
    {
        return new float[3] { vector.x, vector.y, vector.z };
    }

    /// <summary>
    /// Spawned network objects.
    /// </summary>
    public static List<NetworkObject> List = new List<NetworkObject>();

    [HideInInspector]
    public Animator animator;

    /// <summary>
    /// This will control all the visual shader effects.
    /// </summary>
    public Visual visual;

    private void Start ()
    {
        animator = GetComponent<Animator>();

        List.Add(this);

        gameObject.name = Id.ToString();

        Identity identity = Identity.List.Find(x => x.Id == Id);

        if (identity != null && Identity.NeedToSync (identity))
        {
            if (animator != null)
                SyncAnimator(identity); // We are the syncer. Sync the starting variables of animator.
        }
    }

    public void SyncAnimator(Identity identity)
    {
        if (animator.runtimeAnimatorController != null)
        {
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                switch (parameter.type)
                {
                    case AnimatorControllerParameterType.Bool: identity.Variables.SetVariable(parameter.name, animator.GetBool (parameter.name)); break;
                    case AnimatorControllerParameterType.Float: identity.Variables.SetVariable(parameter.name, animator.GetFloat(parameter.name)); break;
                    case AnimatorControllerParameterType.Int: identity.Variables.SetVariable(parameter.name, animator.GetInteger(parameter.name)); break;
                }
            }
        }
    }

    /// <summary>
    /// This will be enabled at first spawn to receive the spawn position and rotation even if we are the syncer of this object.
    /// </summary>
    public bool CertainUpdate;

    /// <summary>
    /// Identity Id
    /// </summary>
    public int Id;

    public float Sync_PositionTolerance = 10f;
    public float Sync_RotationTolerance = 360f;

    /// <summary>
    /// Smooth syncing, used for non-agent objects.
    /// </summary>
    Vector3 SmoothSync_Position;
    Quaternion SmoothSync_Rotation;

    public System.Action OnUpdate;

    public void SmoothSync()
    {
        transform.position = Vector3.Lerp(transform.position, SmoothSync_Position, Time.deltaTime * 10);
        transform.rotation = Quaternion.Slerp(transform.rotation, SmoothSync_Rotation, Time.deltaTime * 10);
    }

    public void NetworkUpdate(Identity identity)
    {
        #region transform
        Vector3 position = new Vector3(
            identity.Variables.GetVariableAsFloat("PositionX"),
            identity.Variables.GetVariableAsFloat("PositionY"),
            identity.Variables.GetVariableAsFloat("PositionZ")
        );

        Vector3 angle = new Vector3(
            identity.Variables.GetVariableAsFloat("AngleX"),
            identity.Variables.GetVariableAsFloat("AngleY"),
            identity.Variables.GetVariableAsFloat("AngleZ")
            );

        SmoothSync_Position = position;
        SmoothSync_Rotation = Quaternion.Euler(angle);

        if (CertainUpdate)
        { // This is certain order. Probably first spawn.
            transform.position = SmoothSync_Position;
            transform.rotation = SmoothSync_Rotation;
            CertainUpdate = false;
        }
        else
        {
            if (Vector3.Distance(transform.position, position) > Sync_PositionTolerance)
            {
                transform.position = SmoothSync_Position;
            }

            if (Quaternion.Angle(transform.rotation, Quaternion.Euler(angle)) > Sync_RotationTolerance)
            {
                transform.rotation = SmoothSync_Rotation;
            }
        }
        #endregion
    }
}

/// <summary>
/// Network object entity.
/// </summary>
class NetworkObjectEntity : ComponentSystem
{
    struct Components
    {
        public NetworkObject networkObject;
        public Transform transform;
    }

    protected override void OnUpdate()
    {
        foreach (var e in GetEntities<Components>())
        {
            Identity identity = Identity.List.Find(x => x.Id == e.networkObject.Id);

            if (identity == null)
                continue;

            #region get from network
            if (Identity.NeedToRetrieve(identity) || e.networkObject.CertainUpdate)
            {
                e.networkObject.OnUpdate?.Invoke();

                #region animator
                if (e.networkObject.animator != null && e.networkObject.animator.runtimeAnimatorController != null)
                {
                    foreach (AnimatorControllerParameter parameter in e.networkObject.animator.parameters)
                    {
                        switch (parameter.type)
                        {
                            case AnimatorControllerParameterType.Bool:
                                e.networkObject.animator.SetBool (parameter.name, identity.Variables.GetVariableAsBool(parameter.name));
                                break;

                            case AnimatorControllerParameterType.Float:
                                e.networkObject.animator.SetFloat(parameter.name, identity.Variables.GetVariableAsFloat(parameter.name));
                                break;

                            case AnimatorControllerParameterType.Int:
                                e.networkObject.animator.SetInteger (parameter.name, identity.Variables.GetVariableAsInt(parameter.name));
                                break;
                        }
                    }
                }
                #endregion
            }
            #endregion

            #region set to network
            if (Identity.NeedToSync(identity))
            {
                #region transform
                identity.Variables.SetVariable("PositionX", NetworkObject.Decimal(e.transform.position.x));
                identity.Variables.SetVariable("PositionY", NetworkObject.Decimal(e.transform.position.y));
                identity.Variables.SetVariable("PositionZ", NetworkObject.Decimal(e.transform.position.z));
                identity.Variables.SetVariable("AngleX", NetworkObject.Decimal(e.transform.eulerAngles.x));
                identity.Variables.SetVariable("AngleY", NetworkObject.Decimal(e.transform.eulerAngles.y));
                identity.Variables.SetVariable("AngleZ", NetworkObject.Decimal(e.transform.eulerAngles.z));
                #endregion

                if (e.networkObject.animator != null)
                    e.networkObject.SyncAnimator(identity);
            }
            #endregion
        }
    }
}

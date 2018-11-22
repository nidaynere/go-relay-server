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
    /// <summary>
    /// Spawned network objects.
    /// </summary>
    public static List<NetworkObject> List = new List<NetworkObject>();

    [HideInInspector]
    public Rigidbody rbody;
    [HideInInspector]
    public Animator animator;

    public float SpeedSyncPosition = 10f;
    public float SpeedSyncRotation = 10f;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rbody = GetComponent<Rigidbody>();

        List.Add(this);

        Identity identity = Identity.List.Find(x => x.Id == Id);

        if (identity != null && Identity.NeedToSync (identity))
        {
            if (rbody != null)
                SyncRigidbody(identity); // We are the syncer. Sync the starting variables of rigidbody.

            if (animator != null)
                SyncAnimator(identity); // We are the syncer. Sync the starting variables of animator.
        }
    }

    /// <summary>
    /// Sync rigidbody variables
    /// </summary>
    /// <param name="identity"></param>
    public void SyncRigidbody(Identity identity)
    {
        identity.Variables.SetVariable("VelocityX", rbody.velocity.x);
        identity.Variables.SetVariable("VelocityY", rbody.velocity.y);
        identity.Variables.SetVariable("VelocityZ", rbody.velocity.z);
        identity.Variables.SetVariable("AVelocityX", rbody.angularVelocity.x);
        identity.Variables.SetVariable("AVelocityY", rbody.angularVelocity.y);
        identity.Variables.SetVariable("AVelocityZ", rbody.angularVelocity.z);
        identity.Variables.SetVariable("Mass", rbody.mass);
        identity.Variables.SetVariable("IsKinematic", rbody.isKinematic);
        identity.Variables.SetVariable("Drag", rbody.drag);
        identity.Variables.SetVariable("AngularDrag", rbody.angularDrag);
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

    private void OnDestroy()
    {
        Identity identity = Identity.List.Find(x => x.Id == Id);
        if (identity != null)
            identity.OnDestroyed();

        List.Remove(this);
    }

    /// <summary>
    /// This will be enabled at first spawn to receive the spawn position and rotation even if we are the syncer of this object.
    /// </summary>
    public bool CertainUpdate;

    /// <summary>
    /// Identity Id
    /// </summary>
    public int Id;
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
            {
                Object.Destroy (e.transform.gameObject);
                continue;
            }

            #region get from network
            if (Identity.NeedToRetrieve(identity) || e.networkObject.CertainUpdate)
            {
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


                if (e.networkObject.CertainUpdate)
                { // This is certain order. Probably first spawn.
                    e.transform.position = position;
                    e.transform.eulerAngles = angle;
                }
                else
                {
                    e.transform.position = Vector3.Lerp(e.transform.position, position, Time.deltaTime * e.networkObject.SpeedSyncPosition);
                    e.transform.rotation = Quaternion.Slerp(e.transform.rotation, Quaternion.Euler(angle), Time.deltaTime * e.networkObject.SpeedSyncRotation);
                }
                #endregion

                #region rigidbody
                if (e.networkObject.rbody != null)
                {
                    Vector3 velocity = new Vector3(
                        identity.Variables.GetVariableAsFloat("VelocityX"),
                        identity.Variables.GetVariableAsFloat("VelocityY"),
                        identity.Variables.GetVariableAsFloat("VelocityZ")
                    );

                    Vector3 angularVelocity = new Vector3(
                        identity.Variables.GetVariableAsFloat("AVelocityX"),
                        identity.Variables.GetVariableAsFloat("AVelocityY"),
                        identity.Variables.GetVariableAsFloat("AVelocityZ")
                    );

                    e.networkObject.rbody.velocity = velocity;
                    e.networkObject.rbody.angularVelocity = angularVelocity;
                    e.networkObject.rbody.mass = identity.Variables.GetVariableAsFloat("Mass");
                    e.networkObject.rbody.isKinematic = identity.Variables.GetVariableAsBool("IsKinematic");
                    e.networkObject.rbody.drag = identity.Variables.GetVariableAsFloat("Drag");
                    e.networkObject.rbody.angularDrag = identity.Variables.GetVariableAsFloat("AngularDrag");
                }
                #endregion

                e.networkObject.CertainUpdate = false;
            }
            #endregion

            #region set to network
            if (Identity.NeedToSync(identity))
            {
                #region transform
                identity.Variables.SetVariable("PositionX", e.transform.position.x);
                identity.Variables.SetVariable("PositionY", e.transform.position.y);
                identity.Variables.SetVariable("PositionZ", e.transform.position.z);
                identity.Variables.SetVariable("AngleX", e.transform.eulerAngles.x);
                identity.Variables.SetVariable("AngleY", e.transform.eulerAngles.y);
                identity.Variables.SetVariable("AngleZ", e.transform.eulerAngles.z);
                #endregion

                if (e.networkObject.rbody != null)
                    e.networkObject.SyncRigidbody(identity);

                if (e.networkObject.animator != null)
                    e.networkObject.SyncAnimator(identity);
            }
            #endregion
        }
    }
}

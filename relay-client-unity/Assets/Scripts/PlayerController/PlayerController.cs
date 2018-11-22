using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RelayClient.Network;

namespace Player
{
    /// <summary>
    /// Simple player controller.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        NetworkObject networkObject;
        Animator animator;
        PlayerVariables variables;
        // Start is called before the first frame update
        void Start()
        {
            variables = GetComponent<PlayerVariables>();
            animator = GetComponent<Animator>();
            networkObject = GetComponent<NetworkObject>();

            networkObject.SpeedSyncPosition = variables.MoveSpeed;

            Identity identity = Identity.List.Find(x => x.Id == networkObject.Id);

            //Sample variable implementation ->
            identity.Variables.SetVariable("HP", variables.HP);
            //-->Callback
            identity.Variables.FindVariable("HP").OnChanged = (string Value) =>
            {
                Debug.Log("OnHealthUpdate()-> " + Value);
            };
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                Identity identity = Identity.List.Find(x => x.Id == networkObject.Id);
                identity.Variables.SetVariable("HP", identity.Variables.GetVariableAsInt("HP") - 1);
            }

            bool isMoving = false;

            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            float abshorizontal = Mathf.Abs(horizontal);
            float absvertical = Mathf.Abs(vertical);

            if (abshorizontal > 0 || absvertical > 0)
                isMoving = true;

            if (animator != null)
            {
                animator.SetBool("Moving", isMoving);
            }

            if (isMoving)
            {
                Vector3 direction = new Vector3(horizontal, 0, vertical);
                Quaternion Look = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, Look, Time.deltaTime * 10);

                //Disable this if you have a root motion moving animation.
                transform.position += direction * Time.deltaTime * variables.MoveSpeed;
            }
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RelayClient.Network;
using RelayClient.Client;

namespace Agents
{
    public class Agent : MonoBehaviour
    {
        /// <summary>
        /// Local Player, Player or AI controller.
        /// </summary>
        public virtual void Controller(Identity identity)
        { }

        public NetworkObject networkObject;
        public Animator animator;
        public AgentVariables variables;

        /// <summary>
        /// This will control all the visual shader effects.
        /// </summary>
        public Visual visual;

        // Start is called before the first frame update
        void Start()
        {
            variables = GetComponent<AgentVariables>();
            animator = GetComponent<Animator>();
            networkObject = GetComponent<NetworkObject>();

            visual = gameObject.AddComponent<Visual>();

            visual.UpdateMaterials();
            visual.OnUpdate += visual.FadeIn;

            Identity identity = Identity.List.Find(x => x.Id == networkObject.Id);

            //Health ->
            identity.Variables.SetVariable("HP", variables.HP);
            NetworkVariables.Variables.AddVariableCallback(networkObject.Id, "HP").OnChanged = (string OldValue, string NewValue) =>
            { //-->Callback
                Debug.Log("OnHPUpdate()-> " +
                    "old value: " + OldValue +
                    ", new value: " + NewValue);

                if (!string.IsNullOrEmpty(OldValue))
                {
                    int Old = int.Parse(OldValue);
                    int New = int.Parse(NewValue);

                    if (New < Old)
                    {
                        visual.UpdateMaterials();
                        visual.OnUpdate += visual.DamageIn;

                        Debug.Log("Damage Visual()");

                        if (New <= 0)
                        {
                            //Dead.
                            animator.SetBool("Dead", true);
                        }
                    }
                }
            };
            //

            //Add movement variables ->
            identity.Variables.SetVariable("Horizontal", 0);
            identity.Variables.SetVariable("Vertical", 0);
            //

        }

        private void Update()
        {
            Identity identity = Identity.List.Find(x => x.Id == networkObject.Id);

            Controller(identity); // Call the agent controller

            float horizontal = identity.Variables.GetVariableAsFloat("Horizontal");
            float vertical = identity.Variables.GetVariableAsFloat("Vertical");
            float abshorizontal = Mathf.Abs(horizontal);
            float absvertical = Mathf.Abs(vertical);

            bool isMoving = false;
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

        void Attack()
        {
            Identity identity = Identity.List.Find(x => x.Id == networkObject.Id);
            animator.SetBool("Attack", false);

            Debug.DrawRay(transform.position + transform.up / 2f, transform.forward, Color.red, 1);

            RaycastHit[] hits = Physics.SphereCastAll(transform.position + transform.up / 2f, 1f, transform.forward, 1.5f);

            foreach (RaycastHit raycastHit in hits)
            {
                if (raycastHit.collider.gameObject == gameObject)
                   continue; // Skip self.

                Debug.Log(raycastHit.collider.name);
                switch (raycastHit.collider.tag)
                {
                    case "Agent": // This is an agent.
                        Agent agent = raycastHit.collider.GetComponent<Agent>();

                        Identity hit = Identity.List.Find(x => x.Id == agent.networkObject.Id);

                        if (PlayerController.Instance == agent || NetworkVariables.IsHost)
                        {
                            // Local player. control this.
                            hit.Variables.SetVariable("HP", hit.Variables.GetVariableAsInt("HP") - variables.Damage);
                        }
                        break;
                }
            }
        }
    }
}


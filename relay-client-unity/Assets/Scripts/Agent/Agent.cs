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
        public Variables variables;

        public System.Action OnDie;

        public static List<Agent> List = new List<Agent>();

        /// <summary>
        /// Is a player or AI?
        /// </summary>
        public bool IsPlayer;

        // Start is called before the first frame update
        void Awake ()
        {
            List.Add(this);

            animator = GetComponent<Animator>();
            networkObject = GetComponent<NetworkObject>();
            variables = GetComponent<Variables>();

            networkObject.visual = gameObject.AddComponent<Visual>();

            networkObject.visual.UpdateMaterials();
            networkObject.visual.Born();
            networkObject.visual.OnUpdate += networkObject.visual.FadeIn;

            Identity identity = Identity.List.Find(x => x.Id == networkObject.Id);

            IsPlayer = identity.Variables.GetVariableAsBool("IsPlayer");

            //Health ->
            identity.Variables.SetVariable("HP", variables.Health());
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
                        networkObject.visual.UpdateMaterials();
                        networkObject.visual.OnUpdate += networkObject.visual.DamageIn;

                        Debug.Log("Damage Visual()");

                        if (New <= 0)
                        {
                            //Dead.
                            animator.SetBool("Dead", true);

                            OnDie?.Invoke();
                        }
                    }
                    else if (animator.GetBool("Dead"))
                    {
                        animator.SetBool("Attack", false); // Disable the attack
                        animator.SetBool("Dead", false);
                    }
                }
            };
            //

            //Add movement variables ->
            identity.Variables.SetVariable("Horizontal", 0);
            identity.Variables.SetVariable("Vertical", 0);
            //

        }

        private void OnDestroy()
        {
            List.Remove(this);
        }

        private void Update()
        {
            Identity identity = Identity.List.Find(x => x.Id == networkObject.Id);

            if (identity == null)
                return;

            Controller(identity); // Call the agent controller

            float horizontal = identity.Variables.GetVariableAsFloat("Horizontal");
            float vertical = identity.Variables.GetVariableAsFloat("Vertical");
            float abshorizontal = Mathf.Abs(horizontal);
            float absvertical = Mathf.Abs(vertical);

            bool isMoving = false;
            if ((abshorizontal > 0 || absvertical > 0) && !animator.GetBool ("Dead"))
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
                transform.position += direction * Time.deltaTime * variables.MoveSpeed();
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
                   continue;

                Debug.Log(raycastHit.collider.name);
                switch (raycastHit.collider.tag)
                {
                    case "Agent": // This is an agent.
                        Agent agent = raycastHit.collider.GetComponent<Agent>();

                        Identity hit = Identity.List.Find(x => x.Id == agent.networkObject.Id);
                        hit.Variables.SetVariable("HP", hit.Variables.GetVariableAsInt("HP") - variables.Damage());
                        break;
                }
            }
        }
    }
}


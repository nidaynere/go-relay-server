using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RelayClient.Network;
using RelayClient.Client;
using System.Linq;

namespace Agents
{
    /// <summary>
    /// Simple AI controller.
    /// </summary>
    public class AIController : Agent
    {
        private void Start()
        {
            OnDie = () => {
                Identity identity = Identity.List.Find(x => x.Id == networkObject.Id);
                identity.Variables.SetVariable("Destroy", 3);
                Debug.Log("Destroy in: " + 3);
                Debug.Log(Time.time);
            };
        }

        Agent Target;
        // Update is called once per frame
        public override void Controller(Identity identity)
        {
            if (NetworkVariables.IsHost) // AIs are host controlled
            {
                if (Target == null)
                {
                    List<Agent> InSight = List.FindAll(x => x.IsPlayer && Vector3.Distance(x.transform.position, transform.position) < variables.AvoidDistance());
                    List<Agent> SortByDistance = InSight.OrderBy(x => Vector3.Distance(x.transform.position, transform.position)).ToList();
                    Target = SortByDistance.Find(x => !x.animator.GetBool("Dead"));

                    if (Target != null)
                    {
                        Debug.Log("Target is found: " + Target.name);
                    }
                }
                else
                {
                    if (Target.animator.GetBool("Dead"))
                    { // Target is dead
                        Target = null;
                        return;
                    }
                }

                if (Target != null)
                {
                    if (Vector3.Distance(transform.position, Target.transform.position) > variables.HitDistance())
                    {
                        // Target is TooFarAway, MoveToTarget
                        Vector3 move = (Target.transform.position - transform.position).normalized;
                        identity.Variables.SetVariable("Horizontal", NetworkObject.Decimal (move.x));
                        identity.Variables.SetVariable("Vertical", NetworkObject.Decimal (move.z));

                        animator.SetBool("Attack", false);
                    }
                    else
                    {
                        identity.Variables.SetVariable("Horizontal", 0);
                        identity.Variables.SetVariable("Vertical", 0);

                        animator.SetBool("Attack", true);
                    }
                }
            }
        }
    }
}


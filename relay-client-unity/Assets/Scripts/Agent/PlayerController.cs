using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RelayClient.Network;
using RelayClient.Client;

namespace Agents
{
    /// <summary>
    /// Simple player controller.
    /// </summary>
    public class PlayerController : Agent
    {
        /// <summary>
        /// Current player instance;
        /// </summary>
        public static PlayerController Instance;

        private void Start()
        {
            OnDie = () => {
                // This is a player. And should be resurrected.
                Invoke("Resurrection", 5); // After 5 seconds.
            };    
        }

        private void Resurrection()
        {
            Identity identity = Identity.List.Find(x => x.Id == networkObject.Id);
            identity.Variables.SetVariable("HP", variables.Health());
        }

        // Update is called once per frame
        public override void Controller (Identity identity)
        {
            identity.Variables.SetVariable("Horizontal", NetworkObject.Decimal (Input.GetAxis("Horizontal")));
            identity.Variables.SetVariable("Vertical", NetworkObject.Decimal(Input.GetAxis("Vertical")));

            if (Input.GetButtonDown("Fire1"))
                animator.SetBool("Attack", true);
        }
    }
}


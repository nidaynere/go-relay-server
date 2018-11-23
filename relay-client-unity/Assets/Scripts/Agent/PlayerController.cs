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

        // Update is called once per frame
        public override void Controller (Identity identity)
        {
            identity.Variables.SetVariable("Horizontal", Input.GetAxis("Horizontal"));
            identity.Variables.SetVariable("Vertical", Input.GetAxis("Vertical"));

            if (Input.GetButtonDown("Fire1"))
                animator.SetBool("Attack", true);
        }
    }
}


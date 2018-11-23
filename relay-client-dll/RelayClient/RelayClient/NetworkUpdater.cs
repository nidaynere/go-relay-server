using RelayClient.Network;

namespace RelayClient
{
    public class NetworkUpdater
    {
        /// <summary>
        /// Current Step.
        /// </summary>
        private static int Step = 0;

        /// <summary>
        /// Call this at LateUpdate
        /// </summary>
        public static void Update()
        {
            if (Step >= Identity.List.Count)
            {
                Step = 0;
                return;
            }

            if (Identity.List[Step] != null && Identity.NeedToSync (Identity.List[Step]))
                Identity.List[Step].Spawn();

            Step++;
        }
    }
}

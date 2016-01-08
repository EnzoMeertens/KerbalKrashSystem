using UnityEngine;

namespace KerbalKrashSystem
{
    public class ModuleKerbalKrashContainer : KerbalKrashGlobal
    {
        public float FlowScaling = 5.0f;

        protected override void OnEnabled()
        {
            base.ToleranceScaling = 5.0f;
            base.Malleability = 2.0f;
        }

        protected void FixedUpdate()
        {
            if (Damage <= 0)
                return;

            foreach (PartResource resource in part.Resources)
            {
                if (resource.amount <= 0.0)
                    continue;

                resource.amount -= Damage * Time.deltaTime * TimeWarp.CurrentRate * FlowScaling;

                if (resource.amount <= 0.0)
                    resource.amount = 0.0;
            }
        }
    }
}
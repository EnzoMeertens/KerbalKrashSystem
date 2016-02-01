using UnityEngine;

namespace KKS
{
    public class ModuleKerbalKrashSystem_Container : KerbalKrashSystem
    {
        [KSPField(guiName = "Flow scaling", guiActive = false)]
        public float _flowScaling = 5.0f;

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            //No need to do anything if damage equals zero (or less).
            if (Damage <= 0)
                return;

            //Drain all of this container's resources.
            foreach (PartResource resource in part.Resources)
            {
                //Resource drained, no need to drain more.
                if (resource.amount <= 0.0 || CheatOptions.InfiniteFuel)
                    continue;

                resource.amount -= Damage * Time.deltaTime * TimeWarp.CurrentRate * _flowScaling;

                //Clamp to a minimum of zero resources.
                if (resource.amount <= 0.0)
                    resource.amount = 0.0;
            }
        }
    }
}
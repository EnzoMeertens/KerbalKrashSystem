using UnityEngine;

namespace KKS
{
    public class ModuleKerbalKrashSystem_Other : KerbalKrashSystem
    {
        protected override void OnEnabled()
        {
            base.ToleranceScaling = 2.0f;
            base.Malleability = 1.0f;

            DamageReceived += ModuleKerbalKrashOther_DamageReceived;
        }

        protected override void OnDisabled()
        {
            DamageReceived -= ModuleKerbalKrashOther_DamageReceived;
        }

        private void ModuleKerbalKrashOther_DamageReceived(KerbalKrashSystem sender, float e)
        {
            if (Damage > 1)
                part.explode();
        }
    }
}

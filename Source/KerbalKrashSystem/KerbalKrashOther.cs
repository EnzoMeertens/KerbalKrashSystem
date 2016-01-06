using UnityEngine;

namespace KerbalKrashSystem
{
    public class ModuleKerbalKrashOther : KerbalKrashGlobal
    {
        protected override void OnEnabled()
        {
            base.ToleranceScaling = 5.0f;

            DamageReceived += ModuleKerbalKrashOther_DamageReceived;
        }

        protected override void OnDisabled()
        {
            DamageReceived -= ModuleKerbalKrashOther_DamageReceived;
        }

        private void ModuleKerbalKrashOther_DamageReceived(KerbalKrashGlobal sender, float e)
        {
            if (Damage > 100)
                part.explode();
        }
    }
}

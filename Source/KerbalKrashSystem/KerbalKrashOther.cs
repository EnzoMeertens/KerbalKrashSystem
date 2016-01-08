namespace KerbalKrashSystem
{
    public class ModuleKerbalKrashOther : KerbalKrashGlobal
    {
        protected override void OnEnabled()
        {
            base.ToleranceScaling = 2.0f;
            base.Malleability = 0.5f;

            DamageReceived += ModuleKerbalKrashOther_DamageReceived;
        }

        protected override void OnDisabled()
        {
            DamageReceived -= ModuleKerbalKrashOther_DamageReceived;
        }

        private void ModuleKerbalKrashOther_DamageReceived(KerbalKrashGlobal sender, float e)
        {
            if (Damage > 1)
                part.explode();
        }
    }
}

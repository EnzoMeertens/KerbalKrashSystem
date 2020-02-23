namespace KKS
{
    public class ModuleKerbalKrashSystem_Other : KerbalKrashSystem
    {
        protected bool _wasDamaged = false;

        protected override void OnEnabled()
        {
            DamageReceived += ModuleKerbalKrashSystem_Other_DamageReceived;
            DamageRepaired += ModuleKerbalKrashSystem_Other_DamageRepaired;
        }

        protected override void OnDisabled()
        {
            DamageReceived -= ModuleKerbalKrashSystem_Other_DamageReceived;
            DamageRepaired -= ModuleKerbalKrashSystem_Other_DamageRepaired;
        }

        private void ModuleKerbalKrashSystem_Other_DamageReceived(KerbalKrashSystem sender, float damage)
        {
            if (damage < _damageThreshold || _wasDamaged)
                return;

            _wasDamaged = true;
        }


        private void ModuleKerbalKrashSystem_Other_DamageRepaired(KerbalKrashSystem sender, float damage)
        {
            if (damage >= _damageThreshold || _wasDamaged == false)
                return;

            _wasDamaged = false;
        }
    }
}

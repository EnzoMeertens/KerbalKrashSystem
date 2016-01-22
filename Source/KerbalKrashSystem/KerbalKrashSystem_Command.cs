namespace KKS
{
    public class ModuleKerbalKrashSystem_Command : KerbalKrashSystem
    {
        private bool _wasDamaged = false;

        protected override void OnEnabled()
        {
            DamageReceived += ModuleKerbalKrashSystem_Command_DamageReceived;
            DamageRepaired += ModuleKerbalKrashSystem_Command_DamageRepaired;
        }

        protected override void OnDisabled()
        {
            DamageReceived -= ModuleKerbalKrashSystem_Command_DamageReceived;
            DamageRepaired += ModuleKerbalKrashSystem_Command_DamageRepaired;
        }

        private void ModuleKerbalKrashSystem_Command_DamageReceived(KerbalKrashSystem sender, float damage)
        {
            if (damage < DamageThreshold || _wasDamaged)
                return;

            _wasDamaged = true;

            ScreenMessages.PostScreenMessage(part.partInfo.title + " functionality damaged.", 4, ScreenMessageStyle.UPPER_CENTER);
        }


        private void ModuleKerbalKrashSystem_Command_DamageRepaired(KerbalKrashSystem sender, float damage)
        {
            if (damage >= DamageThreshold || !_wasDamaged)
                return;

            _wasDamaged = false;

            ScreenMessages.PostScreenMessage(part.partInfo.title + " functionality restored!", 4, ScreenMessageStyle.UPPER_CENTER);
        }
    }
}
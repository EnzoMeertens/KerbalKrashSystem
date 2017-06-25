using UnityEngine;

namespace KKS
{
    public class ModuleKerbalKrashSystem_Command : KerbalKrashSystem
    {
        private bool _wasDamaged = false;

        private GameObject _blockage = null;

        protected override void OnEnabled()
        {
            //TODO: This doesn't work. The command pod gets called twice on a revert to launch.
            //DamageReceived += ModuleKerbalKrashSystem_Command_DamageReceived;
            //DamageRepaired += ModuleKerbalKrashSystem_Command_DamageRepaired;
        }

        protected override void OnDisabled()
        {
            //TODO: This doesn't work. The command pod gets called twice on a revert to launch.
            //DamageReceived -= ModuleKerbalKrashSystem_Command_DamageReceived;
            //DamageRepaired -= ModuleKerbalKrashSystem_Command_DamageRepaired;
        }

        private void ModuleKerbalKrashSystem_Command_DamageReceived(KerbalKrashSystem sender, float damage)
        {
            if (damage < DamageThreshold || _wasDamaged || part.airlock == null)
                return;

            _blockage = new GameObject("blockage");
            _blockage.AddComponent<Collider>();
            _blockage.transform.localScale = Vector3.one * 2f;
            _blockage.transform.parent = part.transform;
            _blockage.transform.localPosition = part.airlock.localPosition;

            _wasDamaged = true;

            ScreenMessages.PostScreenMessage(part.partInfo.title + " damaged.", 4, ScreenMessageStyle.UPPER_CENTER);
        }

        private void ModuleKerbalKrashSystem_Command_DamageRepaired(KerbalKrashSystem sender, float damage)
        {
            if (damage >= DamageThreshold || !_wasDamaged)
                return;

            _wasDamaged = false;

            ScreenMessages.PostScreenMessage(part.partInfo.title + " restored!", 4, ScreenMessageStyle.UPPER_CENTER);
        }
    }
}
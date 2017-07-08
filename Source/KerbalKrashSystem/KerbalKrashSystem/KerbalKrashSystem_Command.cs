using UnityEngine;

namespace KKS
{
    public class ModuleKerbalKrashSystem_Command : KerbalKrashSystem
    {
        private bool _wasDamaged = false;

        private GameObject _blockage = null;

        protected override void OnEnabled()
        {
            _wasDamaged = false;

            DamageReceived += ModuleKerbalKrashSystem_Command_DamageReceived;
            DamageRepaired += ModuleKerbalKrashSystem_Command_DamageRepaired;
        }

        protected override void OnDisabled()
        {
            _wasDamaged = false;

            DamageReceived -= ModuleKerbalKrashSystem_Command_DamageReceived;
            DamageRepaired -= ModuleKerbalKrashSystem_Command_DamageRepaired;
        }

        private void ModuleKerbalKrashSystem_Command_DamageReceived(KerbalKrashSystem sender, float damage)
        {
            if (damage < DamageThreshold || _wasDamaged || part.airlock == null)
                return;

            //Add an airlock blockage to prevent Kerbals from exiting.
            _blockage = new GameObject("blockage");
            _blockage.AddComponent<BoxCollider>().isTrigger = true;
            _blockage.AddComponent<Rigidbody>().isKinematic = false;
            _blockage.transform.parent = part.airlock;
            _blockage.transform.position = part.airlock.position;
            _blockage.transform.rotation = part.airlock.rotation;
            _blockage.AddComponent<FixedJoint>().connectedBody = part.Rigidbody;
            _blockage.transform.localScale = new Vector3(0.25f, 0.25f, 0.05f);

            _wasDamaged = true;

            ScreenMessages.PostScreenMessage(part.partInfo.title + " damaged.", 4, ScreenMessageStyle.UPPER_CENTER);
        }

        private void ModuleKerbalKrashSystem_Command_DamageRepaired(KerbalKrashSystem sender, float damage)
        {
            if (damage >= DamageThreshold || !_wasDamaged)
                return;

            //Get rid of airlock blockage.
            Destroy(_blockage);

            _wasDamaged = false;

            ScreenMessages.PostScreenMessage(part.partInfo.title + " restored!", 4, ScreenMessageStyle.UPPER_CENTER);
        }
    }
}
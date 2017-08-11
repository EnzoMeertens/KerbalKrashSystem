using UnityEngine;

namespace KKS
{
    public class ModuleKerbalKrashSystem_Command : KerbalKrashSystem
    {
        #region Variables
        #region Protected fields
        [KSPField(guiName = "Hatch malfunction", guiActive = false)]
        public bool _hatchMalfunction = true;
        /// <summary>
        /// Boolean value indicating if the hatch can malfunction (get stuck) and trap Kerbals inside.
        /// </summary>
        protected bool HatchMalfunction
        {
            get { return _hatchMalfunction; }
            set { _hatchMalfunction = value; }
        }
        #endregion

        #region Private fields
        private bool _wasDamaged = false;

        private GameObject _blockage = null;
        #endregion
        #endregion

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

            if (HatchMalfunction)
            {
                //Add an airlock blockage to prevent Kerbals from exiting.
                _blockage = new GameObject("blockage");

                _blockage.AddComponent<BoxCollider>().isTrigger = true;

                //Has to be rigidbody to trigger the collider trigger.
                _blockage.AddComponent<Rigidbody>().isKinematic = false;

                _blockage.transform.parent = part.airlock;
                _blockage.transform.position = part.airlock.position;
                _blockage.transform.rotation = part.airlock.rotation;
                _blockage.AddComponent<FixedJoint>().connectedBody = part.Rigidbody;
                _blockage.transform.localScale = new Vector3(0.25f, 0.25f, 0.05f);
            }

            _wasDamaged = true;

            ScreenMessages.PostScreenMessage(part.partInfo.title + " damaged.", 4, ScreenMessageStyle.UPPER_CENTER);
        }

        private void ModuleKerbalKrashSystem_Command_DamageRepaired(KerbalKrashSystem sender, float damage)
        {
            if (damage >= DamageThreshold || !_wasDamaged)
                return;

            //Get rid of airlock blockage.
            if (HatchMalfunction)
                Destroy(_blockage); 

            _wasDamaged = false;

            ScreenMessages.PostScreenMessage(part.partInfo.title + " restored!", 4, ScreenMessageStyle.UPPER_CENTER);
        }
    }
}
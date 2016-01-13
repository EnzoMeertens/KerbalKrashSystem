using System.Collections.Generic;
using KIS;
using KKS;

namespace KerbalKrashSystem_KIS_Repair
{
    public class ModuleKerbalKrashSystem_KIS_Repair : PartModule
    {
        private KerbalKrashSystem _kerbalKrash;

        /// <summary>
        /// Called everytime the part is enabled.
        /// </summary>
        private void OnEnable()
        {
            _kerbalKrash = part.GetComponent<KerbalKrashSystem>();

            _kerbalKrash.DamageReceived += _kerbalKrash_DamageReceived;
            _kerbalKrash.DamageRepaired += _kerbalKrash_DamageReceived;

            KerbalKrashSystem_KIS_Helper.EquipmentChanged += KerbalKrashSystem_KIS_Helper_EquipmentChanged;
        }

        /// <summary>
        /// Called everytime the part is disabled.
        /// </summary>
        private void OnDisable()
        {
            _kerbalKrash.DamageReceived -= _kerbalKrash_DamageReceived;
            _kerbalKrash.DamageRepaired -= _kerbalKrash_DamageReceived;

            KerbalKrashSystem_KIS_Helper.EquipmentChanged -= KerbalKrashSystem_KIS_Helper_EquipmentChanged;
        }

        void _kerbalKrash_DamageReceived(KerbalKrashSystem sender, float damage)
        {
            Events["Repair"].guiName = "Repair (" + damage.ToString("P") + ")";
        }

        /// <summary>
        /// Event fired when KIS equipped equipment changes.
        /// </summary>
        /// <param name="sender">Event-firing object.</param>
        /// <param name="e">List of equipped equipment.</param>
        public void KerbalKrashSystem_KIS_Helper_EquipmentChanged(object sender, List<object> e)
        {
            if (e != null)
            {
                foreach (KIS_Item item in e)
                {
                    if (item.prefabModule is ModuleKISItemAttachTool)
                    {
                        Events["Repair"].active = true;
                        return;
                    }
                }
            }

            Events["Repair"].active = false;
        }

        /// <summary>
        /// Right-click repair button event: repairs the last applied damage.
        /// </summary>
        [KSPEvent(guiName = "Repair", guiActive = false, externalToEVAOnly = true, guiActiveEditor = false, active = true, guiActiveUnfocused = true, unfocusedRange = 3.0f)]
        public void Repair()
        {
            //No krashes to repair.
            if (_kerbalKrash.Krashes.Count == 0)
            {
                ScreenMessages.PostScreenMessage("No damage to repair!", 4, ScreenMessageStyle.UPPER_CENTER);
                return; 
            }

            //Fully repair the part.
            _kerbalKrash.Repair();

            //Remove the last krash.
            _kerbalKrash.Krashes.RemoveAt(_kerbalKrash.Krashes.Count - 1);

            //Apply all remaining krashes.
            foreach (KerbalKrashSystem.Krash krash in _kerbalKrash.Krashes)
                _kerbalKrash.ApplyKrash(krash);

            ScreenMessages.PostScreenMessage("Repaired to " + _kerbalKrash.Damage.ToString("P"), 4, ScreenMessageStyle.UPPER_CENTER);
        }
    }
}
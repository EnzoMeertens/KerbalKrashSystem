using System.Collections.Generic;
using KIS;
using KKS;

namespace KerbalKrashSystem_KIS_Repair
{
    public class ModuleKerbalKrashSystem_KIS_Repair : Damageable
    {
        private KerbalKrashSystem _kerbalKrash;

        /// <summary>
        /// Called everytime the part is enabled.
        /// </summary>
        private void OnEnable()
        {
            if (HighLogic.LoadedScene != GameScenes.FLIGHT)
                return;

            _kerbalKrash = part.GetComponent<KerbalKrashSystem>();

            if (_kerbalKrash == null)
                return;

            //TODO: Add script to Kerbal, Kerbal to this.
            _kerbalKrash.DamageReceived += _kerbalKrash_DamageReceived;
            _kerbalKrash.DamageRepaired += _kerbalKrash_DamageReceived;

            KerbalKrashSystem_KIS_Helper.EquipmentChanged += KerbalKrashSystem_KIS_Helper_EquipmentChanged;
        }

        /// <summary>
        /// Called everytime the part is disabled.
        /// </summary>
        private void OnDisable()
        {
            if (HighLogic.LoadedScene != GameScenes.FLIGHT || _kerbalKrash == null)
                return;

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
        [KSPEvent(name = "Repair", guiName = "Repair (0%)", active = false, guiActive = false, guiActiveEditor = false, externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 3.0f)]
        public void Repair()
        {
            //No krashes to repair.
            if (_kerbalKrash.Krashes.Count == 0)
            {
                ScreenMessages.PostScreenMessage("No damage to repair!", 4, ScreenMessageStyle.UPPER_CENTER);
                return; 
            }

            //Repair the newest krash of this part.
            _kerbalKrash.Repair();

            ScreenMessages.PostScreenMessage("Repaired " + part.partInfo.title + " to " + _kerbalKrash.Damage.ToString("P"), 4, ScreenMessageStyle.UPPER_CENTER);
        }
    }
}
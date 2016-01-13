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

            KerbalKrashSystem_KIS_Helper.EquipmentChanged += KerbalKrashSystem_KIS_Helper_EquipmentChanged;
        }


        /// <summary>
        /// Called everytime the part is disabled.
        /// </summary>
        private void OnDisable()
        {
            KerbalKrashSystem_KIS_Helper.EquipmentChanged -= KerbalKrashSystem_KIS_Helper_EquipmentChanged;
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
            if (_kerbalKrash.Krashes.Count == 0)
                return; //No krashes to repair.

            _kerbalKrash.ApplyKrash(_kerbalKrash.Krashes[_kerbalKrash.Krashes.Count - 1], true);

            _kerbalKrash.Krashes.Remove(_kerbalKrash.Krashes[_kerbalKrash.Krashes.Count - 1]);
        }
    }
}
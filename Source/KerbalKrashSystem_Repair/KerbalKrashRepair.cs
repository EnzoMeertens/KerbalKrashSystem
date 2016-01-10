using KerbalKrashSystem;

namespace KerbalKrashSystem_Repair
{
    public class ModuleKerbalKrashRepair : PartModule
    {
        private KerbalKrashGlobal _kerbalKrash;

        private void OnEnable()
        {
            _kerbalKrash = part.GetComponent<KerbalKrashGlobal>();
        }

        /// <summary>
        /// Right-click repair button event: repairs the last applied damage.
        /// </summary>
        [KSPEvent(guiName = "Repair", guiActive = true, externalToEVAOnly = true, guiActiveEditor = false, active = true, guiActiveUnfocused = true, unfocusedRange = 3.0f)]
        public void Repair()
        {
            if (_kerbalKrash.Krashes.Count == 0)
                return; //No krashes to repair.

            _kerbalKrash.RepairKrash(_kerbalKrash.Krashes[_kerbalKrash.Krashes.Count - 1]);

            _kerbalKrash.Krashes.Remove(_kerbalKrash.Krashes[_kerbalKrash.Krashes.Count - 1]);
        }
    }
}

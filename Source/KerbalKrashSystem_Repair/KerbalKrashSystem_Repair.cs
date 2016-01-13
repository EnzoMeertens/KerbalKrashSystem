using KKS;

namespace KerbalKrashSystem_Repair
{
    public class ModuleKerbalKrashRepair : PartModule
    {
        private KerbalKrashSystem _kerbalKrash;

        /// <summary>
        /// Called when this part gets enabled.
        /// </summary>
        private void OnEnable()
        {
            _kerbalKrash = part.GetComponent<KerbalKrashSystem>();
        }

        /// <summary>
        /// Right-click repair button event: repairs the last applied damage.
        /// </summary>
        [KSPEvent(guiName = "Repair", guiActive = true, externalToEVAOnly = true, guiActiveEditor = false, active = true, guiActiveUnfocused = true, unfocusedRange = 3.0f)]
        public void Repair()
        {
            if (_kerbalKrash.Krashes.Count == 0)
                return; //No krashes to repair.

            _kerbalKrash.ApplyKrash(_kerbalKrash.Krashes[_kerbalKrash.Krashes.Count - 1], true);

            _kerbalKrash.Krashes.Remove(_kerbalKrash.Krashes[_kerbalKrash.Krashes.Count - 1]);
        }
    }
}

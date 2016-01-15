using KKS;

namespace KerbalKrashSystem_Repair
{
    public class ModuleKerbalKrashSystem_Repair : PartModule
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




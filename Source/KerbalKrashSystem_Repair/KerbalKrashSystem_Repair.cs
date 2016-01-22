using KKS;

namespace KerbalKrashSystem_Repair
{
    public class ModuleKerbalKrashSystem_Repair : Damageable
    {
        private KerbalKrashSystem _kerbalKrash;

        /// <summary>
        /// Called when this part gets enabled.
        /// </summary>
        private void OnEnable()
        {
            if (HighLogic.LoadedScene != GameScenes.FLIGHT)
                return;

            _kerbalKrash = part.GetComponent<KerbalKrashSystem>();

            if (_kerbalKrash == null)
                return;

            _kerbalKrash.DamageReceived += _kerbalKrash_DamageReceived;
            _kerbalKrash.DamageRepaired += _kerbalKrash_DamageReceived;
        }

        private void OnDisable()
        {
            if (HighLogic.LoadedScene != GameScenes.FLIGHT)
                return;

            _kerbalKrash.DamageReceived -= _kerbalKrash_DamageReceived;
            _kerbalKrash.DamageRepaired -= _kerbalKrash_DamageReceived;
        }

        private void _kerbalKrash_DamageReceived(KerbalKrashSystem sender, float damage)
        {
            Events["Repair"].guiName = "Repair (" + damage.ToString("P") + ")";
            Events["Repair"].active = damage != 0;
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
                ScreenMessages.PostScreenMessage(part.partInfo.title + ": no damage to repair!", 4, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            //Only check trait and level requirements if trait is not "None".
            if (RequiredTrait != Trait.None.ToString())
            {
                //Get trait information.
                ProtoCrewMember trait = FlightGlobals.ActiveVessel.GetVesselCrew()[0];

                //Check if current Kerbal meets all trait requirements.
                if (trait == null || trait.experienceTrait.Title != RequiredTrait || trait.experienceLevel < RequiredLevel)
                {
                    ScreenMessages.PostScreenMessage(part.partInfo.title + " can only be repaired by a level " + RequiredLevel + "(+) " + RequiredTrait + "!", 4, ScreenMessageStyle.UPPER_CENTER);
                    return;
                }
            }

            //Repair the newest krash of this part.
            _kerbalKrash.Repair();

            ScreenMessages.PostScreenMessage(part.partInfo.title + ": repaired to " + _kerbalKrash.Damage.ToString("P"), 4, ScreenMessageStyle.UPPER_CENTER);
        }
    }
}




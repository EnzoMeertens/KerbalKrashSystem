using KKS;
using UnityEngine;

namespace KerbalKrashSystem_Repair
{
    public class ModuleKerbalKrashSystem_Repair : Damageable
    {
        /// <summary>
        /// Maximum delay between repairs.
        /// </summary>
        [KSPField(guiName = "Maximum repair delay", guiActive = false)]
        public float _maximumRepairDelay = 55.0f;

        /// <summary>
        /// Maximum delay between repairs minus the experience level times this scaling.
        /// </summary>
        [KSPField(guiName = "Experience level scaling", guiActive = false)]
        public float _experienceLevelScaling = 10.0f;

        /// <summary>
        /// Kerbal Krash System instance.
        /// </summary>
        private KerbalKrashSystem _kerbalKrash;

        /// <summary>
        /// The previous repair (mission) time.
        /// </summary>
        private double _repaired_time = 0;

        /// <summary>
        /// Called when this part gets enabled.
        /// </summary>
        private void OnEnable()
        {
            //Only do this when in flight.
            if (HighLogic.LoadedScene != GameScenes.FLIGHT)
                return;

            //Get the Kerbal Krash System instance.
            _kerbalKrash = part.GetComponent<KerbalKrashSystem>();

            if (_kerbalKrash == null)
                return;

            //Register the damage received/repaired events.
            _kerbalKrash.DamageReceived += _kerbalKrash_DamageReceived;
            _kerbalKrash.DamageRepaired += _kerbalKrash_DamageReceived;
        }

        private void OnDisable()
        {
            //Only do this when in flight and the Kerbal Krash System instance is available.
            if (HighLogic.LoadedScene != GameScenes.FLIGHT || _kerbalKrash == null)
                return;

            //Unregister the damage received/repaired events.
            _kerbalKrash.DamageReceived -= _kerbalKrash_DamageReceived;
            _kerbalKrash.DamageRepaired -= _kerbalKrash_DamageReceived;
        }

        private void _kerbalKrash_DamageReceived(KerbalKrashSystem sender, float damage)
        {
            //Enable the "Repair" button on the damaged part and set the text accordingly.
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

            //Get trait information.
            ProtoCrewMember trait = FlightGlobals.ActiveVessel.GetVesselCrew()[0];

            //Only check trait and level requirements if trait is not "None".
            if (RequiredTrait != Trait.None.ToString())
            {
                //Check if current Kerbal meets all trait requirements.
                if (trait == null || trait.experienceTrait.Title != RequiredTrait || trait.experienceLevel < RequiredLevel)
                {
                    //Kerbal does not have the appropriate trait (level).
                    ScreenMessages.PostScreenMessage(part.partInfo.title + " can only be repaired by a level " + RequiredLevel + "(+) " + RequiredTrait + "!", 4, ScreenMessageStyle.UPPER_CENTER);
                    return;
                }
            }

            //Calculate the repair delay for the given Kerbal's experience level.
            double delay = _maximumRepairDelay - (trait.experienceLevel * _experienceLevelScaling) - (part.vessel.missionTime - _repaired_time);

            //Check if the repair delay has passed (and vessel is not sitting on the launchpad).
            if (delay > 0 && part.vessel.missionTime > 0)
            {
                ScreenMessages.PostScreenMessage(part.partInfo.title + " can be repaired in " + delay.ToString("N0") + " seconds.", 4, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            //Repair the newest krash of this part.
            _kerbalKrash.Repair();
            
            //Set the last repair time.
            _repaired_time = part.vessel.missionTime;

            ScreenMessages.PostScreenMessage(part.partInfo.title + ": repaired to " + _kerbalKrash.Damage.ToString("P").PadLeft(7), 4, ScreenMessageStyle.UPPER_CENTER);
        }
    }
}
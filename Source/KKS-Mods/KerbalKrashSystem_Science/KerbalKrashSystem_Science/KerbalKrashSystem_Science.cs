using KKS;

namespace KerbalKrashSystem_Science
{
    public class ModuleKerbalKrashSystem_Science : KerbalKrashSystem
    {
        private KerbalKrashSystem _kerbalKrash;
        private ModuleScienceExperiment _scienceExperiment;

        private bool _wasDamaged = false;

        private void OnEnable()
        {
            if (HighLogic.LoadedScene != GameScenes.FLIGHT)
                return;

            _kerbalKrash = part.GetComponent<KerbalKrashSystem>();

            if (_kerbalKrash == null)
                return;

            _scienceExperiment = part.GetComponent<ModuleScienceExperiment>();

            _kerbalKrash.DamageReceived += _kerbalKrash_DamageReceived;
            _kerbalKrash.DamageRepaired += _kerbalKrash_DamageRepaired;
        }

        private void OnDisable()
        {
            if (HighLogic.LoadedScene != GameScenes.FLIGHT)
                return;

            if (_kerbalKrash == null)
                return;

            _kerbalKrash.DamageReceived -= _kerbalKrash_DamageReceived;
            _kerbalKrash.DamageRepaired -= _kerbalKrash_DamageRepaired;
        }

        private void _kerbalKrash_DamageReceived(KerbalKrashSystem sender, float damage)
        {
            if (damage < DamageThreshold || _wasDamaged)
                return;

            _wasDamaged = true;

            if (_scienceExperiment == null)
                return;

            _scienceExperiment.dataIsCollectable = false;
            _scienceExperiment.Inoperable = true;

            ScreenMessages.PostScreenMessage(part.partInfo.title + " ruined.", 4, ScreenMessageStyle.UPPER_CENTER);
        }


        private void _kerbalKrash_DamageRepaired(KerbalKrashSystem sender, float damage)
        {
            if (damage >= DamageThreshold || !_wasDamaged)
                return;

            _wasDamaged = false;

            if (_scienceExperiment == null)
                return;

            _scienceExperiment.dataIsCollectable = true;

            ScreenMessages.PostScreenMessage(part.partInfo.title + " functionality restored!", 4, ScreenMessageStyle.UPPER_CENTER);
        }
    }
}

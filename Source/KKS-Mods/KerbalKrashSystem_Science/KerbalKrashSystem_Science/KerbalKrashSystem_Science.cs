using KKS;

namespace KerbalKrashSystem_Science
{
    public class ModuleKerbalKrashSystem_Science : KerbalKrashSystem
    {
        private ModuleScienceExperiment _scienceExperiment;

        private bool _wasDamaged = false;

        protected override void OnEnabled()
        {
            _scienceExperiment = part.GetComponent<ModuleScienceExperiment>();

            DamageReceived += OnDamageReceived;
            DamageRepaired += OnDamageRepaired;
        }

        protected override void OnDisabled()
        {
            DamageReceived -= OnDamageReceived;
            DamageRepaired -= OnDamageRepaired;
        }

        private void OnDamageReceived(KerbalKrashSystem sender, float damage)
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


        private void OnDamageRepaired(KerbalKrashSystem sender, float damage)
        {
            if (damage >= DamageThreshold || !_wasDamaged)
                return;

            _wasDamaged = false;

            if (_scienceExperiment == null)
                return;

            _scienceExperiment.dataIsCollectable = true;

            ScreenMessages.PostScreenMessage(part.partInfo.title + " restored!", 4, ScreenMessageStyle.UPPER_CENTER);
        }
    }
}

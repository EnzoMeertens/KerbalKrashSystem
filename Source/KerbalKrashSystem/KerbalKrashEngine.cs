namespace KerbalKrashSystem
{
    public class ModuleKerbalKrashEngine : KerbalKrashGlobal
    {
        public float OverheatScaling = 10.0f;

        private float _originalHeatProduction;
        private ModuleEngines _engine;

        protected override void OnEnabled()
        {
            base.ToleranceScaling = 8.0f;
            base.Malleability = 2.0f;

            _engine = (ModuleEngines)part.GetComponent(typeof(ModuleEngines));
            _originalHeatProduction = _engine.heatProduction;

            DamageReceived += ModuleKerbalKrashEngine_DamageReceived;
        }

        protected override void OnDisabled()
        {
            DamageReceived -= ModuleKerbalKrashEngine_DamageReceived;
        }

        private void ModuleKerbalKrashEngine_DamageReceived(KerbalKrashGlobal sender, float e)
        {
            _engine.heatProduction = _originalHeatProduction + (float)(part.maxTemp * Damage * OverheatScaling);
        }
    }
}
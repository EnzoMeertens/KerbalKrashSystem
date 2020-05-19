namespace KKS
{
    public class ModuleKerbalKrashSystem_Engine : KerbalKrashSystem
    {
        [KSPField(guiName = "Overheat scaling", guiActive = false)]
        public float _overheatScaling = 10.0f;

        private float _originalHeatProduction;
        private ModuleEngines _engine;

        protected override void OnEnabled()
        {
            _engine = (ModuleEngines)part.GetComponent(typeof(ModuleEngines));
            _originalHeatProduction = _engine.heatProduction;

            DamageReceived += ModuleKerbalKrashEngine_DamageReceived;
            DamageRepaired += ModuleKerbalKrashEngine_DamageReceived;
        }

        protected override void OnDisabled()
        {
            DamageReceived -= ModuleKerbalKrashEngine_DamageReceived;
            DamageRepaired -= ModuleKerbalKrashEngine_DamageReceived;
        }

        /// <summary>
        /// This event handler function is called when this part gets damaged.
        /// </summary>
        /// <param name="sender">Object firing the event.</param>
        /// <param name="damage">New damage percentage.</param>
        private void ModuleKerbalKrashEngine_DamageReceived(KerbalKrashSystem sender, float damage)
        {
            //Increase this engine's heat production when damaged.
            //If damage equals zero, the original heat production will be used.
            _engine.heatProduction = _originalHeatProduction + (float)(part.maxTemp * damage * _overheatScaling);
        }
    }
}


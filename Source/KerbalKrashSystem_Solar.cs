using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KKS
{
    public class ModuleKerbalKrashSystem_Solar : KerbalKrashSystem
    {
        private bool _originalBreakable;
        private float _originalChargeRate;
        private ModuleDeployableSolarPanel _solarPanel;

        protected override void OnEnabled()
        {
            _solarPanel = (ModuleDeployableSolarPanel)part.GetComponent(typeof(ModuleDeployableSolarPanel));
            _originalBreakable = _solarPanel.isBreakable;
            _originalChargeRate = _solarPanel.chargeRate;
            _solarPanel.part.crashTolerance = 10000;
            _solarPanel.part.CanFail = false;
            

            _solarPanel.isBreakable = false;

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
            //Break the solar panel's when damage goes above 100%.
            if (damage > _damageThreshold)
            {
                _solarPanel.isBreakable = _originalBreakable;
                _solarPanel.breakPanels();
            }
            else
            {
                _solarPanel.deployState = ModuleDeployablePart.DeployState.RETRACTED;
                _solarPanel.part.State = PartStates.IDLE;
                _solarPanel.part.State = _solarPanel.part.PreFailState;
            }

            //Set the charge rate equal to the inverse of the damage percentage (and limit to 0% charge rate).
            _solarPanel.chargeRate = Math.Max(0.0f, _originalChargeRate * (1.0f - damage));
        }
    }
}

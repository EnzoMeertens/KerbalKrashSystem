using LibNoise;
using UnityEngine;

namespace KKS
{
    public class ModuleKerbalKrashSystem_Wheel : KerbalKrashSystem
    {
        private float _originalMaxSteerAngle;
        private float _originalMaxDriveTorque;
        private float _originalMaxBrakeTorque;

        private ModuleWheelBase _wheel;
        private KSPWheelController _wheel_controller;

        protected override void OnEnabled()
        {
            _wheel = (ModuleWheelBase)part.GetComponent(typeof(ModuleWheelBase));

            DamageReceived += ModuleKerbalKrashWheel_DamageReceived;
            DamageRepaired += ModuleKerbalKrashWheel_DamageReceived;
        }

        protected override void OnDisabled()
        {
            DamageReceived -= ModuleKerbalKrashWheel_DamageReceived;
            DamageRepaired -= ModuleKerbalKrashWheel_DamageReceived;
        }

        /// <summary>
        /// This event handler function is called when this part gets damaged.
        /// </summary>
        /// <param name="sender">Object firing the event.</param>
        /// <param name="damage">New damage percentage.</param>
        private void ModuleKerbalKrashWheel_DamageReceived(KerbalKrashSystem sender, float damage)
        {
            if (_wheel_controller == null && _wheel.Wheel != null)
            {
                _wheel_controller = _wheel.Wheel;
                _originalMaxSteerAngle = _wheel_controller.maxSteerAngle;
                _originalMaxDriveTorque = _wheel_controller.maxDriveTorque;
                _originalMaxBrakeTorque = _wheel_controller.maxBrakeTorque;
            }

            if (_wheel_controller == null)
                return;

            //Decrease the wheel's abilities based on damage.
            float inverse_damage = 1.0f - damage;
            inverse_damage = Mathf.Clamp01(inverse_damage);

            _wheel_controller.maxSteerAngle = _originalMaxSteerAngle * inverse_damage;
            _wheel_controller.maxDriveTorque = _originalMaxDriveTorque * inverse_damage;
            _wheel_controller.maxBrakeTorque = _originalMaxBrakeTorque * inverse_damage;

            //Debug.Log($"[KerbalKrashSystem] Wheel damaged. Original steering angle was {_originalMaxSteerAngle}. New maximum steering angle is {_wheel_controller.maxSteerAngle}");
            //Debug.Log($"[KerbalKrashSystem] Wheel damaged. Original drive torque was {_originalMaxDriveTorque}. New maximum drive torque is {_wheel_controller.maxDriveTorque}");
            //Debug.Log($"[KerbalKrashSystem] Wheel damaged. Original brake torque was {_originalMaxBrakeTorque}. New brake torque is {_wheel_controller.maxBrakeTorque}");
        }
    }
}


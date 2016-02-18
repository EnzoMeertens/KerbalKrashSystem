using UnityEngine;
using KKS;
using System.Collections.Generic;

namespace KerbalKrashSystem_Leak
{
    public class ModuleKerbalKrashSystem_Leak : Damageable
    {
        private KerbalKrashSystem _kerbalKrash;

        private List<GameObject> _leaks = new List<GameObject>();
        private float _averageResourceAmount = 1.0f;

        private readonly Vector3 _localForward = new Vector3(0, 0, -1);

        protected void OnEnable()
        {
            if (HighLogic.LoadedScene != GameScenes.FLIGHT)
                return;

            _kerbalKrash = part.GetComponent<KerbalKrashSystem>();

            if (_kerbalKrash == null)
                return;

            _kerbalKrash.DamageReceived += _kerbalKrash_DamageReceived;
            _kerbalKrash.DamageRepaired += _kerbalKrash_DamageRepaired;
        }

        protected void OnDisable()
        {
            ClearLeaks();

            if (_kerbalKrash == null)
                return;

            _kerbalKrash.DamageReceived -= _kerbalKrash_DamageReceived;
            _kerbalKrash.DamageRepaired -= _kerbalKrash_DamageRepaired;
        }

        protected void FixedUpdate()
        {
            //No need to do anything if damage equals zero (or less).
            if (_kerbalKrash == null || _kerbalKrash.Damage <= 0)
                return;

            bool leaking = false;
            bool sparks = false;
            //Drain all of this container's resources.
            foreach (PartResource resource in part.Resources)
            {
                //Resource drained, no need to drain more.
                if (resource.amount > 0.0)
                    leaking = true;

                if (resource.resourceName == "ElectricCharge" && part.Resources.Count == 1)
                {
                    sparks = true;
                    continue;
                }

                //Fast "average".
                _averageResourceAmount = (_averageResourceAmount / 2.0f) + ((float)(resource.amount / resource.maxAmount) / 2.0f);
            }

            foreach (GameObject leak in _leaks)
            {
                leak.particleEmitter.emit = leaking;
                leak.particleEmitter.localVelocity = _localForward * (sparks ? 0 : 5) * Mathf.Clamp(_averageResourceAmount, 0.5f, 1.0f);

                leak.particleEmitter.maxEnergy = 0.5f;
                leak.particleEmitter.minSize = sparks ? 0.01f : _averageResourceAmount * 0.025f;
                leak.particleEmitter.maxSize = sparks ? 0.02f : _averageResourceAmount * 0.05f;

                //Flow rate * number of resources vented * current time step * thrust coefficient (assuming ISP of ~65)
                float appliedForce = (sparks ? 0 : 5) * _averageResourceAmount * Time.fixedDeltaTime * .65f;
                this.rigidbody.AddRelativeForce(leak.transform.forward * appliedForce);
            }
        }

        private void _kerbalKrash_DamageReceived(KerbalKrashSystem sender, float damage)
        {
            if(damage == 0)
                return;

            //Get newest krash.
            KerbalKrashSystem.Krash krash = _kerbalKrash.Krashes[_kerbalKrash.Krashes.Count - 1];

            GameObject _leak = null;
            bool sparks = false;
            //Create leak emitter.
            foreach (PartResource resource in part.Resources)
            {
                if (_leak != null)
                    continue;

                //Only pumpable resources can leak.
                if (resource.info.resourceTransferMode != ResourceTransferMode.PUMP)
                    continue;

                //Only contains electric charge.
                if (resource.resourceName == "ElectricCharge" && part.Resources.Count == 1)
                {
                    _leak = Instantiate(EZPZ.EZ_Particlez.Instance.ParticleEffects["fx_exhaustSparks_flameout"]) as GameObject;
                    sparks = true;
                }
                else
                    _leak = Instantiate(EZPZ.EZ_Particlez.Instance.ParticleEffects["fx_gasJet_white"]) as GameObject;
            }

            //Attach the leak to this part.
            _leak.transform.parent = part.transform;

            //Set position to the damaged point.
            Vector3 position = krash.ContactPoint;
            position.x /= 2;
            position.z /= 2;
            _leak.transform.localPosition = sparks ? Vector3.zero : position;

            //Scale the emitter size down if sparking instead of leaking.
            _leak.transform.localScale = Vector3.one / (sparks ? 5f : 1f);

            //Leak away from the center of this part.
            _leak.transform.LookAt(part.transform.TransformPoint(new Vector3(0, krash.ContactPoint.y, 0)), _leak.transform.up);

            //Give the leak some speed.
            _leak.particleEmitter.localVelocity = _localForward * (sparks ? 0 : 5);
            _leak.particleEmitter.rndRotation = sparks;
            _leak.particleEmitter.rndAngularVelocity = sparks ? 1 : 0;
            _leak.particleEmitter.rndVelocity = sparks ? Vector3.one : Vector3.zero;

            _leak.particleEmitter.minSize = sparks ? 0.01f : 0.025f;
            _leak.particleEmitter.maxSize = sparks ? 0.02f : 0.05f;
            _leak.particleEmitter.useWorldSpace = false;
            _leak.particleEmitter.maxEnergy = 0.5f;
            _leak.particleEmitter.maxEmission = sparks ? 25 : 50; 

            //Start emiting particles from leak.
            _leak.particleEmitter.emit = true;

            //Save reference to this leak.
            _leaks.Add(_leak);
        }


        private void _kerbalKrash_DamageRepaired(KerbalKrashSystem sender, float damage)
        {
            //Make sure there are no leaks left on fully repaired part.
            if(damage <= 0)
            {
                ClearLeaks();
                return;
            }

            if (_leaks.Count == 0)
                return;

            //Get newest leak.
            GameObject leak = _leaks[_leaks.Count - 1];

            //Stop and remove leak.
            leak.particleEmitter.emit = false;
            _leaks.Remove(leak);
            leak.DestroyGameObject();
            leak = null;
        }

        private void ClearLeaks()
        {
            if(_leaks.Count > 0)
            {
                for (int i = 0; i < _leaks.Count; i++)
                {
                    //Get newest leak.
                    GameObject l = _leaks[i];

                    //Stop and remove leak.
                    l.particleEmitter.emit = false;
                    l.DestroyGameObject();
                    l = null;
                }

                _leaks.Clear();
            }
        }
    }
}

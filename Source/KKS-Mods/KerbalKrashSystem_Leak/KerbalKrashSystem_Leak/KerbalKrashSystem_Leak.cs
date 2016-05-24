using UnityEngine;
using KKS;
using System.Collections.Generic;

namespace KerbalKrashSystem_Leak
{
    public class ModuleKerbalKrashSystem_Leak : Damageable
    {
        [KSPField(guiName = "Flow scaling", guiActive = false)]
        public float _flowScaling = 5.0f;

        private KerbalKrashSystem _kerbalKrash;

        private List<GameObject> _leaks = new List<GameObject>();
        private float _averageResourceAmount = 1.0f;

        private readonly Vector3 _localForward = new Vector3(0, 0, -1);

        private float _previousDamage = 0;

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
                if (resource.info.resourceFlowMode == ResourceFlowMode.NO_FLOW)
                    continue;

                //Still resources available: keep draining.
                if (resource.amount > 0.0 || CheatOptions.InfinitePropellant)
                {
                    leaking = true;

                    resource.amount -= _kerbalKrash.Damage * Time.deltaTime * TimeWarp.CurrentRate * _flowScaling;

                    //Clamp to a minimum of zero resources.
                    if (resource.amount <= 0.0)
                        resource.amount = 0.0;
                }

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
                ParticleEmitter particleEmitter = leak.GetComponent<ParticleEmitter>();

                particleEmitter.emit = leaking;
                particleEmitter.localVelocity = _localForward * (sparks ? 0 : 5) * Mathf.Clamp(_averageResourceAmount, 0.5f, 1.0f);

                particleEmitter.maxEnergy = 0.5f;
                particleEmitter.minSize = sparks ? 0.01f : _averageResourceAmount * 0.025f;
                particleEmitter.maxSize = sparks ? 0.02f : _averageResourceAmount * 0.05f;

                //Flow rate * number of resources vented * current time step * thrust coefficient (assuming ISP of ~65)
                float appliedForce = (sparks ? 0 : 5) * _averageResourceAmount * Time.fixedDeltaTime * .65f;
                this.GetComponent<Rigidbody>().AddRelativeForce(leak.transform.forward * appliedForce);
            }
        }

        private void _kerbalKrash_DamageReceived(KerbalKrashSystem sender, float damage)
        {
            if(damage == 0)
                return;

            //Get newest krash.
            KerbalKrashSystem.Krash krash = _kerbalKrash.Krashes[_kerbalKrash.Krashes.Count - 1];

            GameObject leak = null;
            bool sparks = false;

            //Create leak emitter.
            foreach (PartResource resource in part.Resources)
            {
                if (leak != null)
                    continue;

                //Only flowable resources can leak: lose a fixed amount on impact for unflowable resources.
                if (resource.info.resourceFlowMode == ResourceFlowMode.NO_FLOW)
                {
                    //Remove resource based on remaining resource times delta-Damage (damage - previous damage).
                    resource.amount -= resource.amount * (damage - _previousDamage);

                    if (resource.amount <= 0)
                        resource.amount = 0;

                    continue;
                }

                //Only contains electric charge.
                if (resource.resourceName == "ElectricCharge" && part.Resources.Count == 1)
                {
                    leak = Instantiate(EZPZ.EZ_Particlez.Instance.ParticleEffects["fx_exhaustSparks_flameout"]) as GameObject;
                    sparks = true;
                }
                else
                    leak = Instantiate(EZPZ.EZ_Particlez.Instance.ParticleEffects["fx_gasJet_white"]) as GameObject;
            }

            if (leak == null)
                return;

            //Attach the leak to this part.
            leak.transform.parent = part.transform;

            //Set position to the damaged point.
            Vector3 position = krash.ContactPoint;
            position.x /= 2;
            position.z /= 2;
            leak.transform.localPosition = sparks ? Vector3.zero : position;

            //Scale the emitter size down if sparking instead of leaking.
            leak.transform.localScale = Vector3.one / (sparks ? 5f : 1f);

            //Leak away from the center of this part.
            leak.transform.LookAt(part.transform.TransformPoint(new Vector3(0, krash.ContactPoint.y, 0)), leak.transform.up);

            ParticleEmitter particleEmitter = leak.GetComponent<ParticleEmitter>();

            //Give the leak some speed.
            particleEmitter.localVelocity = _localForward * (sparks ? 0 : 5);
            particleEmitter.rndRotation = sparks;
            particleEmitter.rndAngularVelocity = sparks ? 1 : 0;
            particleEmitter.rndVelocity = sparks ? Vector3.one : Vector3.zero;

            particleEmitter.minSize = sparks ? 0.01f : 0.025f;
            particleEmitter.maxSize = sparks ? 0.02f : 0.05f;
            particleEmitter.useWorldSpace = false;
            particleEmitter.maxEnergy = 0.5f;
            particleEmitter.maxEmission = sparks ? 25 : 50;

            //Start emiting particles from leak.
            particleEmitter.emit = true;

            //Save reference to this leak.
            _leaks.Add(leak);

            //Assign previous damage to current damage (needed for NO_FLOW resource loss).
            _previousDamage = damage;
        }


        private void _kerbalKrash_DamageRepaired(KerbalKrashSystem sender, float damage)
        {
            //Make sure there are no leaks left on fully repaired part.
            if(damage <= 0)
            {
                ClearLeaks();
                return;
            }

            //No leaks to fix.
            if (_leaks.Count == 0)
                return;

            //Get newest leak.
            GameObject leak = _leaks[_leaks.Count - 1];

            //Stop and remove leak.
            leak.GetComponent<ParticleEmitter>().emit = false;

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
                    l.GetComponent<ParticleEmitter>().emit = false;
                    l.DestroyGameObject();
                    l = null;
                }

                _leaks.Clear();
            }
        }
    }
}

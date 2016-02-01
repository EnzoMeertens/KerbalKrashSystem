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

            //Drain all of this container's resources.
            foreach (PartResource resource in part.Resources)
            {
                //Resource drained, no need to drain more.
                if (resource.amount > 0.0)
                    leaking = true;

                //Fast "average".
                _averageResourceAmount = (_averageResourceAmount / 2.0f) + ((float)(resource.amount / resource.maxAmount) / 2.0f);
            }

            foreach (GameObject leak in _leaks)
            {
                leak.particleEmitter.emit = leaking;
                leak.particleEmitter.localVelocity = leak.transform.forward * 5 * Mathf.Clamp(_averageResourceAmount, 0.5f, 1.0f);
                leak.particleEmitter.maxEnergy = 0.5f;
                leak.particleEmitter.minSize = _averageResourceAmount * 0.025f;
                leak.particleEmitter.maxSize = _averageResourceAmount * 0.05f;
            }
        }

        private void _kerbalKrash_DamageReceived(KerbalKrashSystem sender, float damage)
        {
            if(damage == 0)
                return;

            KerbalKrashSystem.Krash krash = _kerbalKrash.Krashes[_kerbalKrash.Krashes.Count - 1];

            GameObject _leak = Instantiate(EZPZ.EZ_Particlez.Instance.ParticleEffects["fx_gasJet_white"]) as GameObject;

            //Save reference to this leak.
            _leaks.Add(_leak);

            //Make the leak normal sized.
            _leak.transform.localScale = Vector3.one;

            //Attach the leak to this part.
            _leak.transform.parent = part.transform;

            //Place the particle emitter at the center of this part.
            _leak.transform.localPosition = new Vector3(krash.ContactPoint.x / 2.0f, krash.ContactPoint.y, krash.ContactPoint.z / 2.0f);

            //Leak away from the center of this part.
            _leak.transform.rotation = Quaternion.LookRotation(Vector3.zero - part.transform.TransformPoint(krash.ContactPoint));

            //Give the leak some speed.
            _leak.particleEmitter.localVelocity = _leak.transform.forward * 5.0f;

            _leak.particleEmitter.minSize = 0.01f;
            _leak.particleEmitter.maxSize = 0.05f;
            _leak.particleEmitter.useWorldSpace = false;
            _leak.particleEmitter.maxEnergy = 0.25f;
            _leak.particleEmitter.maxEmission = 50; 

            //Start emiting particles from leak.
            _leak.particleEmitter.emit = true;
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

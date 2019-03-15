using UnityEngine;
using KKS;
using System.Collections.Generic;
using System.Linq;

namespace KerbalKrashSystem_Leak
{
    public class ModuleKerbalKrashSystem_Leak : Damageable
    {
        [KSPField(guiName = "Flow scaling", guiActive = false)]
        public float _flowScaling = 5.0f;

        [KSPField(guiName = "Particle size multiplier", guiActive = false)]
        public float _sizeMultiplier = 10.0f;

        private KerbalKrashSystem _kerbalKrash;

        /// <summary>
        /// List of current leaks.
        /// </summary>
        private List<GameObject> _leaks = new List<GameObject>();

        /// <summary>
        /// The current (average) resource percentage.
        /// </summary>
        private float _averageResourceAmount = 1.0f;

        /// <summary>
        /// Previous damage (needed for non-flowing resource loss, e.g. cargo).
        /// </summary>
        private float _previousDamage = 0;

        #region Pre-defined curves
        /// <summary>
        /// Increase in size over time.
        /// </summary>
        private readonly AnimationCurve curve_up = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 1.0f)) { preWrapMode = WrapMode.Loop, postWrapMode = WrapMode.Loop };

        /// <summary>
        /// Decrease in size over time.
        /// </summary>
        private readonly AnimationCurve curve_down = new AnimationCurve(new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 0.0f)) { preWrapMode = WrapMode.Loop, postWrapMode = WrapMode.Loop };
        #endregion

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

                    if(CheatOptions.InfinitePropellant == false)
                        resource.amount -= _kerbalKrash.Damage * Time.fixedDeltaTime * TimeWarp.CurrentRate * _flowScaling;

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
                ParticleSystem particleEmitter = leak.GetComponent<ParticleSystem>();
                var emission = particleEmitter.emission;
                var main = particleEmitter.main;
                var size_over_lifetime = particleEmitter.sizeOverLifetime;

                emission.enabled = leaking;

                //Size over lifetime
                size_over_lifetime.sizeMultiplier = _averageResourceAmount * _sizeMultiplier;

                //Flow rate * number of resources vented * current time step * thrust coefficient (assuming ISP of ~65)
                float appliedForce = (sparks ? 0.0f : _flowScaling) * _averageResourceAmount * Time.fixedDeltaTime * 0.65f;
                //Apply a force from the leak.
                this.GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * appliedForce);
            }
        }

        private void _kerbalKrash_DamageReceived(KerbalKrashSystem sender, float damage)
        {
            //No damage received.
            if(damage == 0 || _kerbalKrash.Krashes.Count == 0)
                return;

            //Get latest krash.
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

                    //Don't drain below zero.
                    if (resource.amount <= 0)
                        resource.amount = 0;

                    continue;
                }

                //Only contains electric charge.
                if (resource.resourceName == "ElectricCharge" && part.Resources.Count == 1)
                {
                    leak = (GameObject)Instantiate(Resources.Load("Effects/fx_exhaustSparks_yellow"));
                    sparks = true;
                }
                else //Contains something else (oxidizer, monopropellant, etc.).
                {
                    leak = (GameObject)Instantiate(Resources.Load("Effects/fx_smokeTrail_light"));
                }
            }

            //No leak available.
            if (leak == null)
                return;

            //Attach the leak to this part.
            leak.transform.parent = part.transform;
            leak.transform.localEulerAngles = Vector3.zero;

            //Set the local position to the damaged point.
            Vector3 position = krash.ContactPoint;
            //Move the leak inwards a bit to make it leak from within the part.
            position.x = position.x * (sparks ? 0.95f : 0.75f);
            position.y = position.y * (sparks ? 0.75f : 0.75f);
            position.z = position.z * (sparks ? 0.95f : 0.75f);
            leak.transform.localPosition = position;

            //Scale the emitter size down (empirically determined).
            float partSize = Mathf.Clamp(part.partInfo.partSize, 0.1f, 10.0f);
            Vector3 scale = Vector3.one / (sparks ? 500.0f : 100.0f) * partSize; 
            leak.transform.localScale = scale;

            //Leak away from the center of this part.
            leak.transform.LookAt(part.transform.TransformPoint(new Vector3(0, krash.ContactPoint.y, 0)), leak.transform.up);
            leak.transform.Rotate(new Vector3(0, 90, 90));

            //Get the attached particle system.
            ParticleSystem particleEmitter = leak.GetComponent<ParticleSystem>();

            #region Debug
            //VisualDebugLine line1 = new VisualDebugLine(leak, Color.red, Color.red);
            //line1.Length = 2;
            //line1.line.transform.localRotation = Quaternion.Euler(90, 0, 0);

            //VisualDebugLine line2 = new VisualDebugLine(leak, Color.blue, Color.blue);
            //line2.Length = 2;
            //line2.line.transform.localRotation = Quaternion.Euler(0, 90, 0);

            //VisualDebugLine line3 = new VisualDebugLine(leak, Color.green, Color.green);
            //line3.Length = 2;
            //line3.line.transform.localRotation = Quaternion.Euler(0, 0, 90);
            #endregion

            #region Main
            var main = particleEmitter.main;
            main.playOnAwake = false;
            main.loop = true;
            main.duration = 6.0f;
            main.startLifetime = 1.0f;
            main.startSpeed = sparks ? 200.0f : 0.0f;
            main.startSize = 1.0f;
            main.scalingMode = ParticleSystemScalingMode.Local;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 50;
            #endregion

            #region Emission
            var emission = particleEmitter.emission;
            emission.enabled = true;
            emission.rateOverTime = sparks ? 0.0f : 25.0f;
            emission.rateOverDistance = 0;
            if (sparks)
            {
                float random_time = Random.Range(0.5f, 2.0f);
                emission.SetBursts(new ParticleSystem.Burst[]
                {
                    new ParticleSystem.Burst(0.00f, 5, 25, 0, random_time),
                    new ParticleSystem.Burst(0.25f, 5, 25, 0, random_time),
                });
            }
            #endregion

            #region Shape
            var shape = particleEmitter.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 1.0f;
            shape.radiusThickness = 1.0f;
            shape.scale = Vector3.one;
            shape.randomDirectionAmount = sparks ? 0.5f : 0.0f;
            shape.sphericalDirectionAmount = 0.0f;
            shape.randomPositionAmount = sparks ? 5.0f : 0.0f;
            #endregion

            #region Velocity over lifetime
            var velocity_over_lifetime = particleEmitter.velocityOverLifetime;
            velocity_over_lifetime.enabled = !sparks;
            velocity_over_lifetime.space = ParticleSystemSimulationSpace.Local;
            velocity_over_lifetime.x = new ParticleSystem.MinMaxCurve(sparks ? -0.0f : -5.0f, sparks ? 0.0f : -5.0f); //"Up"
            velocity_over_lifetime.y = new ParticleSystem.MinMaxCurve(sparks ? -0.0f :  -25.0f, sparks ? 0.0f : -25.0f); //"Backwards"
            velocity_over_lifetime.z = new ParticleSystem.MinMaxCurve(sparks ? -0.0f : -5.0f, sparks ? 0.0f : -5.0f); //"Left"
            #endregion

            #region Color over lifetime
            var color_over_lifetime = particleEmitter.colorOverLifetime;
            color_over_lifetime.enabled = !sparks;
            Gradient gradient = new Gradient();
            gradient.mode = GradientMode.Blend;
            gradient.alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(sparks ? 0.0f : 0.5f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) };
            var color = color_over_lifetime.color = gradient;
            #endregion

            #region Size over lifetime
            var size_over_lifetime = particleEmitter.sizeOverLifetime;
            size_over_lifetime.enabled = true;
            size_over_lifetime.size = new ParticleSystem.MinMaxCurve(sparks ? 1.0f : _sizeMultiplier, sparks ? curve_down : curve_up);
            #endregion

            #region trails
            //var trails = particleEmitter.trails;
            //trails.enabled = sparks;
            //trails.ratio = 1.0f;
            //trails.lifetime = 1.0f;
            //trails.textureMode = ParticleSystemTrailTextureMode.Stretch;
            //trails.dieWithParticles = true;
            //trails.sizeAffectsWidth = true;
            //trails.minVertexDistance = 10.0f;
            //trails.inheritParticleColor = false;

            //Gradient trails_gradient = new Gradient();
            //gradient.mode = GradientMode.Blend;
            //gradient.alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 0.0f) };
            //gradient.colorKeys = new GradientColorKey[] { new GradientColorKey(new Color(1.0f, 0.25f, 0.0f, 1.0f), 0.0f), new GradientColorKey(new Color(1.0f, 0.75f, 0.0f, 0.0f), 1.0f) };

            //trails.colorOverLifetime = gradient;

            //trails.worldSpace = true;
            //trails.sizeAffectsLifetime = false;
            //trails.widthOverTrail = 0.5f;
            #endregion

            #region Renderer
            ParticleSystemRenderer renderer = leak.GetComponent<ParticleSystemRenderer>();
            renderer.minParticleSize = 0.0f;
            renderer.maxParticleSize = sparks ? 0.25f : 1.0f;
            renderer.renderMode = sparks ? ParticleSystemRenderMode.Stretch : ParticleSystemRenderMode.Billboard;
            renderer.lengthScale = 1.0f;
            renderer.velocityScale = sparks ? -0.1f : 1.0f;
            if (sparks)
                renderer.material.shader = Shader.Find("Particles/Additive");
            #endregion

            particleEmitter.Play();

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
            leak.GetComponent<ParticleSystem>().Stop();

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
                    GameObject leak = _leaks[i];

                    //Stop and remove leak.
                    leak.GetComponent<ParticleSystem>().Stop();

                    leak.DestroyGameObject();
                    leak = null;
                }

                _leaks.Clear();
            }
        }
    }
}

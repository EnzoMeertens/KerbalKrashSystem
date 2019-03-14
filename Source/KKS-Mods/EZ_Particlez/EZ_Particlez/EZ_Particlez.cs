using System.Collections.Generic;
using UnityEngine;

namespace EZPZ
{
    [KSPAddon(KSPAddon.Startup.Flight, true)]
    public class EZ_Particlez : MonoBehaviour
    {
        public static EZ_Particlez Instance { get; private set; }
        public Dictionary<string, GameObject> ParticleEffects = new Dictionary<string, GameObject>();

        private EZ_Particlez()
        {
            Instance = this;
        }

        public void Start()
        {
            DontDestroyOnLoad(Instance);

            InitializeParticles();
        }

        private void InitializeParticles()
        {
            ConfigNode[] particleConfigNodes = GameDatabase.Instance.GetConfigNodes("EZ_Particle");

            foreach (ConfigNode node in particleConfigNodes)
            {
                GameObject particle = (GameObject)Instantiate(Resources.Load(node.GetValue("Resource")));

                DontDestroyOnLoad(particle);

                ParticleSystem particleEmitter = particle.GetComponent<ParticleSystem>();
                var emission = particleEmitter.emission;
                var main = particleEmitter.main;
                var speed = particleEmitter.main.startSpeed;

                emission.enabled = false;
                main.playOnAwake = false;
                emission.rateOverTime = 0.0f;
                particleEmitter.Stop();

                ParticleEffects.Add(node.GetValue("Name"), particle);
            }

        }
    }
}

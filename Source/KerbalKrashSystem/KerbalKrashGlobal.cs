//Do not use Linq, KSP doesn't like Linq.
using System.Collections.Generic;

using UnityEngine;

namespace KerbalKrashSystem
{
    public delegate void DamageReceivedEvent(KerbalKrashGlobal sender, float damage);
    public delegate void DamageRepairedEvent(KerbalKrashGlobal sender, float damage);

    public abstract class KerbalKrashGlobal : PartModule
    {

        #region Structs
        #region Krash
        /// <summary>
        /// Krash struct containing the relevant information about recorded krashes.
        /// </summary>
        public struct Krash
        {
            /// <summary>
            /// Relative velocity of the recorded krash.
            /// </summary>
            public Vector3 RelativeVelocity;

            /// <summary>
            /// The location of the recorded krash.
            /// </summary>
            public Vector3 ContactPoint;
        }
        #endregion
        #endregion

        #region Variables
        #region Public fields
        /// <summary>
        /// Fired when Damage is received.
        /// </summary>
        public event DamageReceivedEvent DamageReceived;

        /// <summary>
        /// Fired when Damage is repaired.
        /// </summary>
        public event DamageRepairedEvent DamageRepaired;

        /// <summary>
        /// Value indicating the damage percentage of the part.
        /// </summary>
        [KSPField(guiName = "Damage", isPersistant = false, guiActive = true, guiActiveEditor = false, guiFormat = "P")]
        public float Damage;
        #endregion

        #region Protected fields
        /// <summary>
        /// List containing all recorded krashes.
        /// </summary>
        [Persistent]
        public List<Krash> Krashes = new List<Krash>();

        private float _toleranceScaling = 1.0f;
        /// <summary>
        /// Value indicating the scaling of the krash tolerance of the part.
        /// Tolerances are scaled to create a margin for damaging instead of exploding.
        /// </summary>
        protected float ToleranceScaling
        {
            get { return _toleranceScaling; }
            set
            {
                _toleranceScaling = value;
                part.crashTolerance = OriginalCrashTolerance * ToleranceScaling;
            }
        }

        /// <summary>
        /// Original krash tolerance of the part.
        /// </summary>
        protected float OriginalCrashTolerance { get; private set; }

        private float _malleability = 1.0f;
        /// <summary>
        /// The plasticity of the part: a higher malleability allows for more low-speed deformations.
        /// </summary>
        /// This value can not be negative.
        protected float Malleability
        {
            get { return _malleability; }
            set { _malleability = Mathf.Abs(value); }
        }

        /// <summary>
        /// Cut-off distance from contact point to vertex.
        /// </summary>
        protected float DentDistance = 1.25f;

        /// <summary>
        /// grouping distance from vertex to vertex.
        /// </summary>
        protected float GroupDistance = .001f;

        /// <summary>
        /// This value scales the minimum random damage (based on the krash velocity) down by this amount.
        /// Default value is 30.0f.
        /// </summary>
        protected float RandomMinDivider = 30.0f;

        /// <summary>
        /// This value scales the maximum random damage (based on the krash velocity) down by this amount.
        /// Default value is 2.0f.
        /// </summary>
        protected float RandomMaxDivider = 2.0f;
        #endregion
        #endregion

        #region Methods
        /// <summary>
        /// Repair this part using the inverse function on the krash.
        /// </summary>
        /// <param name="krash">Krash to apply.</param>
        public void RepairKrash(Krash krash)
        {
            ApplyKrash(krash, true);

            if (DamageRepaired != null)
                DamageRepaired(this, Damage);
        }        

        #region Methods

        /// <summary>
        /// Apply krash to all meshes in this part.
        /// </summary>
        /// <param name="krash">Krash to apply.</param>
        /// <param name="inverse">Apply or undo krash.</param>
        protected void ApplyKrash(Krash krash, bool inverse = false)
        {
            Vector3 relativeVelocity = part.transform.TransformDirection(krash.RelativeVelocity); //Transform the direction of the collision to the world reference frame.

            Damage += (relativeVelocity.magnitude / part.crashTolerance) * (inverse ? -1 : 1);

            Vector4 worldPosContact = part.transform.TransformPoint(krash.ContactPoint);
            MeshFilter[] meshList = part.FindModelComponents<MeshFilter>();

            Vector3 transform = (relativeVelocity / (2f*part.partInfo.partSize) / (part.crashTolerance / Malleability)) * (inverse ? -1 : 1);
            foreach (MeshFilter meshFilter in meshList)
            {
                Vector3 transformT = meshFilter.transform.InverseTransformVector(transform);
                Vector3 contactPointT = meshFilter.transform.InverseTransformPoint(worldPosContact);
                Vector3 dentDistanceT = meshFilter.transform.TransformDirection(Vector3.one).normalized;
                dentDistanceT = meshFilter.transform.InverseTransformVector(DentDistance*dentDistanceT);
                dentDistanceT = Vector3.Max(-dentDistanceT, dentDistanceT);
                Debug.Log(part.partInfo.partUrl);
                Debug.Log("dentDistanceT: "+dentDistanceT.ToString("F4"));
                Vector3 dentDistanceTInv;
                dentDistanceTInv.x = 1f / dentDistanceT.x;
                dentDistanceTInv.y = 1f / dentDistanceT.y;
                dentDistanceTInv.z = 1f / dentDistanceT.z;
                //transformT = Vector3.Scale(transformT, dentDistanceTInv);
                transformT /= dentDistanceT.sqrMagnitude;

                Debug.Log("transformT: " + transformT.ToString("F4"));
                Mesh mesh = meshFilter.mesh;
                if (mesh == null)
                {
                    mesh = meshFilter.sharedMesh;
                }
                Vector3[] vertices = mesh.vertices;
                for (int v = 0; v < vertices.Length; v++)
                {
                    Vector3 distance = vertices[v] - contactPointT;
                    distance = Vector3.Max(-distance, distance);
                    distance = dentDistanceT-distance;
                    
                    if (distance.x >= 0 && 
                        distance.y >= 0 && 
                        distance.z >= 0)
                    {
                        float mag = distance.sqrMagnitude;
                        //look into directional displacement.
                        vertices[v] += mag*transformT;
                    }
                }
                mesh.vertices = vertices;
                meshFilter.mesh = mesh;
            }

            //Fire "DamageReceived" event.
            if (DamageReceived != null)
                DamageReceived(this, Damage);
        }
        #endregion

        #region Events/Callbacks
        #region OnEnable(d)/Disable(d)
        private void OnEnable()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return; //Only do stuff when in Flight Scene.

            OriginalCrashTolerance = part.crashTolerance;
            ToleranceScaling = ToleranceScaling;

            OnEnabled();
        }

        private void OnDisable()
        {
            OnDisabled();
        }

        /// <summary>
        /// Called when this part gets enabled during the Flight Scene.
        /// </summary>
        protected virtual void OnEnabled() { }

        /// <summary>
        /// Called when this parts get disabled during every other Scene.
        /// </summary>
        protected virtual void OnDisabled() { }
        #endregion

        #region OnCollision
        /// <summary>
        /// Called when this part enters a collision with another object.
        /// </summary>
        /// <param name="collision">Collision object containing information about the collision.</param>
        protected virtual void OnCollisionEnter(Collision collision)
        {
            //Only receive damage if part exists and relative velocity is greater than the original tolerance divided malleability of the part.
            if (part == null || collision.relativeVelocity.magnitude <= (OriginalCrashTolerance / Malleability))
                return;

            //TODO: Temporary fix.
            foreach (ContactPoint contactPoint in collision.contacts)
            {
                if (contactPoint.thisCollider is WheelCollider || contactPoint.otherCollider is WheelCollider)
                    return;
            }

            //No need to do anything if the damage is neglible.
            if (collision.relativeVelocity.magnitude / part.crashTolerance <= 0)
                return;

            Krash krash = new Krash
            {
                //Transform the velocity of the collision into the reference frame of the part. 
                RelativeVelocity = part.transform.InverseTransformDirection(collision.relativeVelocity),

                //Transform the direction of the collision to the reference frame of the part.
                ContactPoint = part.transform.InverseTransformPoint(collision.contacts[0].point),
            };

            Krashes.Add(krash);

            ApplyKrash(krash);
        }
        #endregion

        #region OnSave
        /// <summary>
        /// Called when this part gets saved.
        /// </summary>
        /// <param name="node"></param>
        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);

            //No need to save anything when not in flight scene or if no noticable damage has been taken.
            if (!HighLogic.LoadedSceneIsFlight || Damage <= 0 || Krashes.Count <= 0 || part == null || part.vessel == null)
                return;

            foreach (Krash krash in Krashes)
            {
                ConfigNode krashNode = node.AddNode("Krash");

                krashNode.AddValue("RelativeVelocity.x", krash.RelativeVelocity.x);
                krashNode.AddValue("RelativeVelocity.y", krash.RelativeVelocity.y);
                krashNode.AddValue("RelativeVelocity.z", krash.RelativeVelocity.z);
                krashNode.AddValue("ContactPoint.x", krash.ContactPoint.x);
                krashNode.AddValue("ContactPoint.y", krash.ContactPoint.y);
                krashNode.AddValue("ContactPoint.z", krash.ContactPoint.z);
            }

            #if DEBUG
                Debug.Log("[KerbalKrashSystem] Saved " + Krashes.Count +  " krashes for part ID: " + part.flightID);
            #endif
        }
        #endregion

        #region OnLoad
        /// <summary>
        /// Called when this part gets loaded.
        /// </summary>
        /// <param name="node"></param>
        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            //No need to load krashes when not in Flight Scene or non-existent parts/vessels.
            if (!HighLogic.LoadedSceneIsFlight || part == null || part.vessel == null)
                return;
            
            //Clear damage and krashes.
            Damage = 0;
            Krashes = new List<Krash>();

            foreach (ConfigNode cn in node.nodes)
            {
                if (cn.name != "Krash")
                    continue; //No krash to apply.

                Vector3 relativeVelocity = new Vector3(float.Parse(cn.GetValue("RelativeVelocity.x")), float.Parse(cn.GetValue("RelativeVelocity.y")), float.Parse(cn.GetValue("RelativeVelocity.z")));
                Vector3 contactPoint = new Vector3(float.Parse(cn.GetValue("ContactPoint.x")), float.Parse(cn.GetValue("ContactPoint.y")), float.Parse(cn.GetValue("ContactPoint.z")));

                Krash krash = new Krash
                {
                    //Load the relative velocity of the saved krash.
                    RelativeVelocity = relativeVelocity,

                    //Load the position of the saved krash.
                    ContactPoint = contactPoint,
                };

                Krashes.Add(krash);

                ApplyKrash(krash);
            }


            #if DEBUG
            if (Krashes.Count > 0)
                Debug.Log("[KerbalKrashSystem] Applied " + Krashes.Count + " krashes for part ID: " + part.flightID);
            #endif
        }
        #endregion
        #endregion
    }
    #endregion
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace KerbalKrashSystem
{
    public delegate void DamageReceivedEvent(KerbalKrashGlobal sender, float e);

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
        /// Value indicating the damage percentage of the part.
        /// </summary>
        [KSPField(guiName = "Damage", isPersistant = false, guiActive = true, guiActiveEditor = false, guiFormat = "P")] 
        public float Damage;
        #endregion

        #region Protected fields
        private float _toleranceScaling = 5.0f;
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
        protected float GroupDistance = .05f;

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

        #region Private fields
        /// <summary>
        /// List containing all recorded krashes.
        /// </summary>
        [Persistent]
        private List<Krash> _krashes;
        #endregion
        #endregion

        List<VertexGroup> vGroups;

        #region Methods
        /// <summary>
        /// Apply krash to all meshes in this part.
        /// </summary>
        /// <param name="krash"></param>
        private void ApplyKrash(Krash krash)
        {
            Vector3 relativeVelocity = part.transform.TransformDirection(krash.RelativeVelocity); //Transform the direction of the collision to the world reference frame.

            Vector4 worldPosContact = part.transform.TransformPoint(krash.ContactPoint);
            foreach (VertexGroup group in vGroups)
            {
                group.Deform(relativeVelocity / RandomMinDivider, relativeVelocity / RandomMaxDivider , part.crashTolerance / Malleability, worldPosContact, DentDistance);
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
            InitVertexGroups();

            OnEnabled();
        }

        private void InitVertexGroups()
        {
            vGroups = new List<VertexGroup>();
            foreach (MeshFilter meshFilter in gameObject.GetComponentsInChildren(typeof(MeshFilter))) //Apply deformation to every mesh in this part.
            {
                Mesh mesh = meshFilter.mesh;

                if (mesh == null)
                    continue; //Unable to apply damage on invalid mesh.

                Vector3[] vertices = mesh.vertices; //Save all vertices from the mesh in a temporary local variable (speed increase).

                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector3 worldVertex = meshFilter.transform.TransformPoint(vertices[i]); //Transform the point of contact into the world reference frame.
                    float minDistance = float.MaxValue;
                    VertexGroup minGroup = null;
                    foreach (VertexGroup group in vGroups)
                    {
                        float distance = Vector3.Distance(worldVertex, group.Center); //Get the distance from the vertex to the position of the krash.

                        if (distance < GroupDistance && distance < minDistance)
                        {
                            minDistance = distance;
                            minGroup = group;
                        }
                    }
                    if (minGroup == null)
                    {
                        minGroup = new VertexGroup(i, meshFilter);
                        vGroups.Add(minGroup);
                    }
                    minGroup.AddVertex(meshFilter, i);
                }

                #region Experimental
                //if (part.Modules.Contains("ModuleEnginesFX") //For some reason ModuleEnginesFX-engines sink into the terrain when updating collider mesh.
                //    || part.collider == null) 
                //    continue;

                //((MeshCollider) (part.collider)).sharedMesh = null;
                //((MeshCollider) (part.collider)).sharedMesh = mesh;
                #endregion
            }
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

            //Damage "percentage" per axis. 
            Damage += collision.relativeVelocity.magnitude / part.crashTolerance;

            //No need to do anything if the damage is neglible.
            if (Damage <= 0) 
                return; 

            //First krash of this part.
            if(_krashes == null) 
                _krashes = new List<Krash>();

            Krash krash = new Krash
            {
                //Transform the point of contact into the reference frame of the part. 
                RelativeVelocity = part.transform.InverseTransformDirection(collision.relativeVelocity),

                //Transform the direction of the collision to the reference frame of the part.
                ContactPoint = part.transform.InverseTransformPoint(collision.contacts[0].point),
            };

            _krashes.Add(krash);

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
            if (!HighLogic.LoadedSceneIsFlight || Damage <= 0 || _krashes.Count <= 0 || part == null || part.vessel == null)
                return;

            foreach (Krash krash in _krashes)
            {
                ConfigNode krashNode = node.AddNode("Krash");

                krashNode.AddValue("RelativeVelocity.x", krash.RelativeVelocity.x);
                krashNode.AddValue("RelativeVelocity.y", krash.RelativeVelocity.y);
                krashNode.AddValue("RelativeVelocity.z", krash.RelativeVelocity.z);
                krashNode.AddValue("ContactPoint.x", krash.ContactPoint.x);
                krashNode.AddValue("ContactPoint.y", krash.ContactPoint.y);
                krashNode.AddValue("ContactPoint.z", krash.ContactPoint.z);
            }

            Debug.Log("[KerbalKrashSystem] Saved " + _krashes.Count + " krashes for part ID: " + part.flightID);
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
            _krashes = new List<Krash>();

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

                Damage += relativeVelocity.magnitude / part.crashTolerance;

                _krashes.Add(krash);
                
                ApplyKrash(krash);
            }

            if (_krashes.Count <= 0)
                return;

            Debug.Log("[KerbalKrashSystem] Applied " + _krashes.Count + " krashes for part ID: " + part.flightID);

        }
        #endregion
        #endregion
    }
}

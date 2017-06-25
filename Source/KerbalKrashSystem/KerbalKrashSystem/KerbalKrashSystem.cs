//Do not use Linq, KSP doesn't like Linq.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KKS
{
    public delegate void DamageReceivedEvent(KerbalKrashSystem sender, float damage);
    public delegate void DamageRepairedEvent(KerbalKrashSystem sender, float damage);
    public delegate void SplashdownEvent(KerbalKrashSystem sender, KerbalKrashSystem.Krash krash);

    public abstract class KerbalKrashSystem : Damageable
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

            /// <summary>
            /// Value indicating if damage was caused by heat.
            /// </summary>
            public bool ThermalDamage;
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
        /// Fired on vessel splashdown.
        /// </summary>
        public event SplashdownEvent Splashdown;

        /// <summary>
        /// Value indicating the damage percentage of the part.
        /// </summary>
        [KSPField(guiName = "Damage", isPersistant = false, guiActive = true, guiActiveEditor = false, guiFormat = "P")]
        public float Damage;

        /// <summary>
        /// List containing all recorded krashes.
        /// </summary>
        [Persistent]
        public List<Krash> Krashes = new List<Krash>();

        /// <summary>
        /// Boolean indicating if part is excluded from deformations.
        /// </summary>
        [KSPField(guiName = "Exclude", guiActive = false)]
        public bool _exclude = false;
        #endregion

        #region Protected fields
        /// <summary>
        /// Original krash tolerance of the part.
        /// </summary>
        protected float OriginalCrashTolerance { get; private set; }

        [KSPField(guiName = "Tolerance scaling", guiActive = false)]
        public float _toleranceScaling = 1.0f;
        /// <summary>
        /// Value indicating the scaling of the krash tolerance of the part.
        /// Tolerances are scaled to create a margin for damaging instead of exploding.
        /// </summary>
        /// This value can not be negative.
        protected float ToleranceScaling
        {
            get { return _toleranceScaling; }
            set { _toleranceScaling = Mathf.Abs(value); }
        }

        [KSPField(guiName = "Malleability", guiActive = false)]
        public float _malleability = 1.0f;
        /// <summary>
        /// The plasticity of the part: a higher malleability allows for more low-speed deformations and damage.
        /// </summary>
        /// This value can not be negative.
        protected float Malleability
        {
            get { return _malleability; }
            set { _malleability = Mathf.Abs(value); }
        }

        [KSPField(guiName = "Horizontal splashdown scaling", guiActive = false)]
        public float _horizontalSplashdownScaling = 10.0f;
        /// <summary>
        /// The plasticity of the part: a higher malleability allows for more low-speed deformations and damage.
        /// </summary>
        /// This value can not be negative.
        protected float HorizontalSplashdownScaling
        {
            get { return _horizontalSplashdownScaling; }
            set { _horizontalSplashdownScaling = Mathf.Abs(value); }
        }

        /// <summary>
        /// Cut-off distance from contact point to vertex.
        /// </summary>
        protected float DentDistance
        {
            get { return part.partInfo.partSize / 5f; }
        }
        #endregion

        #region Private fields
        /// <summary>
        /// Calculation help constant: (approx.) 1 / √(3).
        /// </summary>
        private const float invSqrt3 = 0.57735026919f;

        /// <summary>
        /// Help variable to calculate splashdown damage only once per splashdown.
        /// </summary>
        [Persistent(isPersistant = true)]
        private bool _splashed = false;

        /// <summary>
        /// Help constant (in units of 0.02 seconds) to prevent "infinite" collisions. 
        /// In some cases the collider gets deformed into the path of the 
        /// collision direction, which deforms the collider into the path 
        /// of the collision.
        /// </summary>
        private const int _collisionDelay = 5;

        /// <summary>
        /// Help variable to prevent "infinite" collisions. 
        /// In some cases the collider gets deformed into the path of the 
        /// collision direction, which deforms the collider into the path 
        /// of the collision.
        /// </summary>
        private int _collisionDelayCounter = 0;

        private List<MeshFilter> _meshFilters = null;
        /// <summary>
        /// Returns all meshes of this part.
        /// </summary>
        private List<MeshFilter> meshFilters
        {
            get
            {
                if(_meshFilters == null)
                    _meshFilters = part.FindModelComponents<MeshFilter>();

                return _meshFilters;
            }
        }

        private MeshCollider _meshCollider = null;
        /// <summary>
        /// Returns the MeshCollider of this part.
        /// </summary>
        private MeshCollider meshCollider
        {
            get
            {
                if(_meshCollider == null)
                    _meshCollider = part.FindModelComponent<MeshCollider>();

                return _meshCollider;
            }
        }

        private Mesh _colliderMesh = null;
        /// <summary>
        /// Returns the mesh of the MeshCollider of this part.
        /// </summary>
        private Mesh colliderMesh
        {
            get
            {
                if(_colliderMesh == null)
                    _colliderMesh = Instantiate(meshCollider.sharedMesh) as Mesh;

                return _colliderMesh;
            }
        }
        #endregion
        #endregion

        #region Methods
        /// <summary>
        /// Repairs specified number of krashes.
        /// Positive value repairs newest krash(es).
        /// Negative value repairs oldest krash(es).
        /// Zero value doesn't do anything.
        /// </summary>
        /// <param name="count">Positive value repairs newest krashes. Negative value repairs first krashes. Zero doesn't do anything.</param>
        public void Repair(int count = 1)
        {
            if (count == 0)
                return;

            #region Restore mesh
            //TODO: This can probably be optimized by storing the variables. But that would increase permanent RAM usage(?)
            List<MeshFilter> currentMeshFilters = part.FindModelComponents<MeshFilter>();
            List<MeshFilter> originalMeshFilters = part.partInfo.partPrefab.FindModelComponents<MeshFilter>();

            for(int i = 0; i < currentMeshFilters.Count; i++)
                currentMeshFilters[i].mesh = originalMeshFilters[i].mesh;
            #endregion

            #region Restore collider
            MeshCollider currentMeshCollider = part.FindModelComponent<MeshCollider>();
            MeshCollider originalMeshCollider = part.partInfo.partPrefab.FindModelComponent<MeshCollider>();

            currentMeshCollider.sharedMesh = originalMeshCollider.sharedMesh;
            currentMeshCollider.convex = true;
            #endregion

            Damage = 0;

            if(count < 0)
                //Remove the last krashes.
                Krashes.RemoveRange(Krashes.Count + count, -count);
            else
                //Remove the first krashes.
                Krashes.RemoveRange(0, count);

            //Apply all remaining krashes.
            foreach (Krash krash in Krashes)
                ApplyKrash(krash, false);

            //Fire DamageRepaired event.
            if (DamageRepaired != null)
                DamageRepaired(this, Damage);
        }

        //private bool subdivided = false;

        /// <summary>
        /// Apply krash to all meshes in this part.
        /// </summary>
        /// <param name="krash">Krash to apply.</param>
        /// <param name="fireEvent">Fire "DamageReceived" event.</param>
        public void ApplyKrash(Krash krash, bool fireEvent = true)
        {
            Vector3 relativeVelocity = part.transform.TransformDirection(krash.RelativeVelocity); //Transform the direction of the collision to the world reference frame.

            Damage += (relativeVelocity.magnitude / part.crashTolerance) / _damageDivider;

            //Fire "DamageReceived" event.
            if (fireEvent && DamageReceived != null)
                DamageReceived(this, Damage);

            if (_exclude)
                return;

            Vector3 transform = (relativeVelocity / (0.75f * part.partInfo.partSize) / (part.crashTolerance / Malleability));
            Vector3 worldPosition = part.transform.TransformPoint(krash.ContactPoint);

            DeformMesh(transform, worldPosition);

            DeformCollider(transform, worldPosition);
        }

        /// <summary>
        /// Updates the visual components of the part.
        /// Thanks Ryan Bray (https://github.com/rbray89).
        /// </summary>
        /// <param name="transform">Vector indicating the amount of deformation.</param>
        /// <param name="worldPosition">Position in the world to apply the deformation from.</param>
        private void DeformMesh(Vector3 transform, Vector3 worldPosition)
        {
            foreach (MeshFilter meshFilter in meshFilters)
            {
                Mesh mesh = meshFilter.mesh;

                if (meshFilter.sharedMesh == null)
                    continue;

                if (mesh == null)
                    mesh = meshFilter.sharedMesh;

                //if (!subdivided && part.partInfo.partSize >= 2)
                //{
                //    subdivided = true;
                //    MeshHelper.Subdivide(mesh, 2, worldPosContact, DentDistance);
                //}

                Vector3 transformT = meshFilter.transform.InverseTransformVector(transform);
                Vector3 contactPointLocal = meshFilter.transform.InverseTransformPoint(worldPosition);
                Vector3 dentDistanceLocal = meshFilter.transform.TransformDirection(Vector3.one).normalized;

                dentDistanceLocal = meshFilter.transform.InverseTransformVector(DentDistance * dentDistanceLocal);
                dentDistanceLocal = Vector3.Max(-dentDistanceLocal, dentDistanceLocal);

                Vector3 dentDistanceInv;
                dentDistanceInv.x = invSqrt3 / dentDistanceLocal.x;
                dentDistanceInv.y = invSqrt3 / dentDistanceLocal.y;
                dentDistanceInv.z = invSqrt3 / dentDistanceLocal.z;

                Vector3[] vertices = mesh.vertices;
                Color32[] Colors = new Color32[vertices.Length];

                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector3 distance = vertices[i] - contactPointLocal;
                    distance = Vector3.Max(-distance, distance);
                    distance = dentDistanceLocal - distance;
                    distance.Scale(dentDistanceInv);

                    if (distance.x < 0 || distance.y < 0 || distance.z < 0)
                        continue;

                    Colors[i] = Color32.Lerp(new Color32(255, 255, 255, 255), new Color(0, 0, 0, 255), Damage);

                    vertices[i] += distance.sqrMagnitude * transformT;
                }

                mesh.vertices = vertices;

                //TODO: Make this a KKS-mod
                //mesh.colors32 = Colors;
            }
        }

        /// <summary>
        /// Updates the collider component of the part.
        /// </summary>
        /// <param name="transform">Vector indicating the amount of deformation.</param>
        /// <param name="worldPosition">Position in the world to apply the deformation from.</param>
        private void DeformCollider(Vector3 transform, Vector3 worldPosition)
        {
            if (meshCollider == null || colliderMesh == null)
                return; //Nothing to deform.

            Vector3 transformT = meshCollider.transform.InverseTransformVector(transform);
            Vector3 contactPointLocal = meshCollider.transform.InverseTransformPoint(worldPosition);

            Vector3 dentDistanceLocal = meshCollider.transform.TransformDirection(Vector3.one).normalized;
            dentDistanceLocal = meshCollider.transform.InverseTransformVector(DentDistance * dentDistanceLocal);
            dentDistanceLocal = Vector3.Max(-dentDistanceLocal, dentDistanceLocal);

            Vector3 dentDistanceInv = new Vector3(invSqrt3 / dentDistanceLocal.x, invSqrt3 / dentDistanceLocal.y, invSqrt3 / dentDistanceLocal.z);

            Vector3[] vertices = colliderMesh.vertices;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 distance = vertices[i] - contactPointLocal;
                distance = Vector3.Max(-distance, distance);
                distance = dentDistanceLocal - distance;
                distance.Scale(dentDistanceInv);

                if (distance.x < 0 || distance.y < 0 || distance.z < 0)
                    continue;

                vertices[i] += distance.sqrMagnitude * transformT;
            }

            colliderMesh.vertices = vertices;
            meshCollider.convex = true;
        }
        #endregion

        #region Events/Callbacks
        #region OnEnable(d)/Disable(d)
        bool _valid = false;

        private void OnEnable()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return; //Only needed in Flight Scene.

            //TODO: This is an ugly temporary fix, because command pods throw multiple disable events on revert to launch.
            if (part.name.Contains("(unloaded)"))
                return;

            _valid = false;

            //TODO: This is an ugly temporary fix, because command pods throw multiple disable events on revert to launch.
            GameEvents.onShowUI.Add(OnUI);

            //Save the original crash tolerance value.
            OriginalCrashTolerance = part.crashTolerance;

            //Set the part's new crash tolerance.
            part.crashTolerance = OriginalCrashTolerance * _toleranceScaling;

            //Register the OnSplashDown event.
            Splashdown += OnSplashdown;

            //Invoke OnEnabled function on derived classes.
            OnEnabled();
        }

        private void OnDisable()
        {
            //TODO: This is an ugly temporary fix, because command pods throw multiple disable events on revert to launch.
            if (!_valid)
                return;

            GameEvents.onShowUI.Remove(OnUI);

            //Reset part's crash tolerance back to original value.
            part.crashTolerance = OriginalCrashTolerance;

            //Unregister the OnSplashDown event.
            Splashdown -= OnSplashdown;

            //Invoke OnDisabled function on derived classes.
            OnDisabled();
        }

        //TODO: This is an ugly temporary fix, because command pods throw multiple disable events on revert to launch.
        private void OnUI()
        {
            _valid = true;
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
            if (!HighLogic.LoadedSceneIsFlight)
                return;

            //Transform the velocity of the collision into the reference frame of the part. 
            Vector3 relativeVelocity = part.transform.InverseTransformDirection(collision.relativeVelocity);

            float angle = Vector3.Angle(relativeVelocity, part.transform.InverseTransformDirection(collision.contacts[0].normal));

            //If collision occurs under a large angle, damage is ignored for now.
            //TODO: angle: [70, 90>: SCRAPING. ADD TEXTURES?
            if (angle > 70)
            {
                //Debug.Log("Angle too steep, no damage.");
                return;
            }

            //Convert angle to [0, 1].
            angle = Mathf.Cos(Mathf.Deg2Rad * angle);

            //Scale the impact velocity by the angle. 
            relativeVelocity *= angle;

            ////Limit collisions to one per collision delay.
            //if (_collisionDelayCounter < _collisionDelay)
            //    return;

            ////Reset the physics counter.
            //_collisionDelayCounter = 0;

            //Only receive damage if part exists and relative velocity is greater than the original tolerance divided malleability of the part.
            if (part == null || relativeVelocity.magnitude <= OriginalCrashTolerance)
                return;

            //No need to do anything if the damage is neglible.
            if (relativeVelocity.magnitude / part.crashTolerance <= 0)
                return;

            Krash krash = new Krash
            {
                RelativeVelocity = relativeVelocity,

                //Transform the direction of the collision to the reference frame of the part.
                ContactPoint = part.transform.InverseTransformPoint(collision.contacts[0].point),
            };

            Krashes.Add(krash);

            ApplyKrash(krash);
        }
        #endregion

        #region OnSplashdown
        private void OnSplashdown(KerbalKrashSystem sender, Krash krash)
        {
            Krashes.Add(krash);

            ApplyKrash(krash);
        }
        #endregion

        #region OnFixedUpdate
        protected virtual void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight || part == null)
            {
                _splashed = false;
                return;
            }

            //if (_collisionDelayCounter < _collisionDelay)
            //    _collisionDelayCounter++;

            //Get the altitude of the part, instead of the altitude of the vessel.
            double partAltitude = FlightGlobals.getAltitudeAtPos(part.transform.position, FlightGlobals.currentMainBody);
            if (partAltitude > 0)
                return;

            //Only receive damage if part's velocity is greater than the original tolerance divided malleability of the part.
            double scaledHorizontalSpeed = part.vessel.horizontalSrfSpeed / HorizontalSplashdownScaling;
            if (part.vessel.verticalSpeed + scaledHorizontalSpeed <= (OriginalCrashTolerance / Malleability))
                return;

            //Already splashed down.
            if (_splashed)
                return;

            _splashed = true;

            //Closest part to the core (lowest point on the collider). This should be faster than checking all vertices.
            Vector3 contactPoint = part.collider.ClosestPointOnBounds(FlightGlobals.currentMainBody.position);

            //Transform the direction of the collision to the reference frame of the part and scale it down a bit to match the actual mesh a bit better.
            contactPoint = part.transform.InverseTransformPoint(contactPoint) / part.collider.bounds.size.magnitude;

            Krash krash = new Krash
            {
                ContactPoint = contactPoint,
                RelativeVelocity = new Vector3((float)scaledHorizontalSpeed, (float)part.vessel.verticalSpeed),
            };

            //Fire "Splashdown" event.
            if (Splashdown != null)
                Splashdown(this, krash);
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
}
namespace KKS
{
    public enum Trait
    {
        None,
        Pilot,
        Engineer,
        Scientist,
        Tourist,
    }

    public abstract class Damageable : PartModule
    {
        #region Configurable damage variables
        [KSPField(guiName = "Damage threshold", guiActive = false)]
        public float _damageThreshold = 1.0f;
        /// <summary>
        /// Parts damaged beyond this threshold will show
        /// </summary>
        protected float DamageThreshold { get { return _damageThreshold; } }
        #endregion

        #region Trait
        [KSPField(guiName = "Required level", guiActive = false)]
        public int _requiredLevel = 0;
        /// <summary>
        /// Value indicating required trait level to restore this part.
        /// </summary>
        public int RequiredLevel { get { return _requiredLevel; } }

        [KSPField(guiName = "Required trait", guiActive = false)]
        public string _requiredTrait = Trait.Engineer.ToString();

        /// <summary>
        /// Value indicating required trait to restore this part.
        /// </summary>
        public string RequiredTrait { get { return _requiredTrait; } }
        #endregion
    }
}

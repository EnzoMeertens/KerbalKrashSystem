namespace KKS
{
    public class ModuleKerbalKrashSystem_Command : KerbalKrashSystem
    {
        protected override void OnEnabled()
        {
            base.ToleranceScaling = 2.0f;
            base.Malleability = 4.0f;
        }
    }
}
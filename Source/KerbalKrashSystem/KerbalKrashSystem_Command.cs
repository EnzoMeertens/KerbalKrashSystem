namespace KKS
{
    public class ModuleKerbalKrashCommand : KerbalKrashSystem
    {
        protected override void OnEnabled()
        {
            base.ToleranceScaling = 2.0f;
            base.Malleability = 4.0f;
        }
    }
}
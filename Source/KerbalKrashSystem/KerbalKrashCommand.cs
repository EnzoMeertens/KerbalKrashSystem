namespace KerbalKrashSystem
{
    public class ModuleKerbalKrashCommand : KerbalKrashGlobal
    {
        protected override void OnEnabled()
        {
            base.ToleranceScaling = 2.0f;
            base.Malleability = 4.0f;
        }
    }
}
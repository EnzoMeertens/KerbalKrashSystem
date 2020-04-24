namespace KKS
{
    public class ModuleKerbalKrashSystem_Exclude : KerbalKrashSystem
    {
        protected override void OnEnabled()
        {
            base._exclude = true;
        }
    }
}

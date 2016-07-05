using UnityEngine;

namespace KerbalKrashSystem_KIS_Support
{
    public class ModuleKerbalKrashSystem_KIS_Support : PartModule
    {
        public override void OnAwake()
        {
            Debug.Log("ModuleKerbalKrashSystem_KIS_Support added!");
        }

        public void OnKISItemEquipped(string value)
        {
            Debug.Log("Received message OnKISItemEquipped!");
            //Component itemModule = GetComponent("ModuleKISItem");
            //Debug.Log("OnEquip message: " + itemModule.name + ": " + itemModule.GetType() + ": " + value);
            //GameObject.Find("Messenger").SendMessage("PassThrough", new object[] { "OnEquip", itemModule.name });
        }

        public void OnKISItemUnequipped(string value)
        {
            Debug.Log("Received message OnKISItemUnequipped!");
            //Component itemModule = GetComponent("ModuleKISItem");
            //Debug.Log("OnEquip message: " + itemModule.name + ": " + itemModule.GetType() + ": " + value);
            //GameObject.Find("Messenger").SendMessage("PassThrough", new object[] { "OnUnequip", itemModule.GetType().ToString() });
        }

        public void OnDestroy()
        {
            Debug.Log("ModuleKerbalKrashSystem_KIS_Support destroyed");
        }
    }
}

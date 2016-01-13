using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalKrashSystem
{
    /*
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    class ModuleLoader : MonoBehaviour
    {
        static int partLoadedIndex = 0;
        private void Update()
        {
            if (HighLogic.LoadedScene == GameScenes.LOADING || HighLogic.LoadedScene == GameScenes.MAINMENU)
            {
                List<AvailablePart> parts = PartLoader.LoadedPartsList;
                int i = partLoadedIndex;
                for (; parts != null && i < parts.Count; i++)
                {
                    AvailablePart ap = parts[i];
                    if (ap.partUrl != null && ap.partUrl != "")
                    {
                        Part part = ap.partPrefab;
                        KerbalKrashGlobal module = (KerbalKrashGlobal)part.FindModuleImplementing<KerbalKrashGlobal>();
                        if (module != null)
                        {
                            module.InitVertexGroups();
                        }
                    }
                }
                partLoadedIndex = i;
            }
        }

    }
    */
}

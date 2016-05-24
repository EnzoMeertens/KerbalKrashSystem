using System.Collections.Generic;
//using KIS;
using UnityEngine;
using System.Linq;
using System;

[assembly: KSPAssemblyDependency("KIS", 1, 2)]
namespace KerbalKrashSystem_KIS_Repair
{
    public delegate void EquipmentChangedEvent(object sender, List<object> e);

    /// <summary>
    /// Helper class for Kerbal Inventory System.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class KerbalKrashSystem_KIS_Helper : MonoBehaviour
    {
        /// <summary>
        /// Event indicating a change (equip/unequip) of equipment.
        /// </summary>
        public static event EquipmentChangedEvent EquipmentChanged;

        /// <summary>
        /// List of currently equipped equipment.
        /// </summary>
        private readonly List<object> _equippedTools = new List<object>(); //This has to be an List of objects. Unity doesn't like KIS_Item here, for some weird reason. (AssemblyLoader cannot load type).

        /// <summary>
        /// Array to compare current/previous equipped equipment.
        /// </summary>
        private object[] previousEquipment; //This has to be an object array, because List will use references instead of values.

        /// <summary>
        /// Called once per script on Start.
        /// </summary>
        public void Start()
        {
            bool _assemblyKISLoaded = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name.Equals("KIS", StringComparison.InvariantCultureIgnoreCase));

            if (!_assemblyKISLoaded)
                Destroy(this);

           // GameEvents.onVesselChange.Add(OnVesselChange);
        }

        ///// <summary>
        ///// Called when the Script is destroyed.
        ///// </summary>
        //public void OnDestroy()
        //{
        //    GameEvents.onVesselChange.Remove(OnVesselChange);
        //}

        ///// <summary>
        ///// Called when the user switches vessels.
        ///// </summary>
        ///// <param name="vessel">New vessel.</param>
        //private void OnVesselChange(Vessel vessel)
        //{
        //    if (FlightGlobals.ActiveVessel == null)
        //        return;

        //    ModuleKISInventory inventory = FlightGlobals.ActiveVessel.GetComponent<ModuleKISInventory>();

        //    CheckEquipmentChange(inventory);
        //}

        ///// <summary>
        ///// Called every 20ms.
        ///// </summary>
        //private void FixedUpdate()
        //{
        //    if (FlightGlobals.ActiveVessel == null)
        //        return;

        //    ModuleKISInventory inventory = FlightGlobals.ActiveVessel.GetComponent<ModuleKISInventory>();

        //    CheckEquipmentChange(inventory);
        //}

        ///// <summary>
        ///// Checks if the current equipped equipment is different from the previous equipped equipment.
        ///// Throws an event if equipment changed.
        ///// </summary>
        ///// <param name="inventory">Inventory to compare to the previous inventory.</param>
        //private void CheckEquipmentChange(ModuleKISInventory inventory)
        //{
        //    //No inventory.
        //    if (inventory == null)
        //    {
        //        //Only send "Changed" event if something changed.
        //        if (previousEquipment != null && EquipmentChanged != null)
        //            EquipmentChanged(this, null);

        //        previousEquipment = null;
        //        return;
        //    }

        //    //No items in inventory.
        //    if (inventory.items.Values.Count == 0)
        //    {
        //        //Only send "Changed" event if something changed.
        //        if (previousEquipment != null && EquipmentChanged != null)
        //            EquipmentChanged(this, null);

        //        previousEquipment = null;
        //        return;
        //    }

        //    //Items in inventory.
        //    foreach (KIS_Item item in inventory.items.Values)
        //    {
        //        if (!_equippedTools.Contains(item))
        //        {
        //            //Only get equipped items.
        //            if (!item.equipped)
        //                continue;

        //            //Add equipped items.
        //            _equippedTools.Add(item);
        //        }
        //            //Remove unequipped items.
        //        else if (!item.equipped)
        //            _equippedTools.Remove(item);
        //    }

        //    //Only sent a "Changed" event when something changed.
        //    if ((previousEquipment == null || !previousEquipment.Equals(_equippedTools)) && EquipmentChanged != null)
        //        EquipmentChanged(this, _equippedTools); //Fire EquipmentChanged event.

        //    previousEquipment = _equippedTools.ToArray();
        //}
    }
}

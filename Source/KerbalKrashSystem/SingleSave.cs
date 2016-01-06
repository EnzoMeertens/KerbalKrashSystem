using System;
using KSP.IO;
using UnityEngine;

namespace KerbalKrashSystem
{
    public class SingleSave
    {
        public PluginConfiguration Configuration;

        private static SingleSave _instance;
        public static SingleSave Instance
        {
            get { return _instance ?? (_instance = new SingleSave()); }
        }

        public SingleSave(bool allow = false)
        {
            throw new Exception("Unable to instantiate singleton class using the constructor. Please use \"Instance\" instead.");
        }

        private SingleSave()
        {
            Configuration = PluginConfiguration.CreateForType<KerbalKrashGlobal>();
            Configuration.load();

            Debug.Log("[KerbalKrashSystem] SingleSave instance loaded.");
        }

        public void Save()
        {
            Configuration.save();
        }

        public T GetValue<T>(string name)
        {
            return Configuration.GetValue<T>(name);
        }

        public void SetValue(string name, object value)
        {
            Configuration.SetValue(name, value);
        }
    }
}

using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Robants.Engine;
using UnityEngine;

namespace BlueprintsPlus
{
    [BepInPlugin(MyGUID, PluginName, VersionString)]
    public class BlueprintsPlusPlugin : BaseUnityPlugin
    {
        // Mod specific details. MyGUID should be unique, and follow the reverse domain pattern
        // e.g.
        // com.mynameororg.pluginname
        // Version should be a valid version string.
        // e.g.
        // 1.0.0
        private const string MyGUID = "host.vierra.Icaria.BlueprintsPlus";
        private const string PluginName = "BlueprintsPlus";
        private const string VersionString = "1.0.0";

        public static string exportDirectoryPath = Path.Combine(DirectoryPath.RolebooksDirectoryFullPath, "Exports");
        public static int ExportVersion = 1;

        private static readonly Harmony Harmony = new Harmony(MyGUID);
        public static ManualLogSource Log = new ManualLogSource(PluginName);

        /// <summary>
        /// Initialise the configuration settings and patch methods
        /// </summary>
        private void Awake()
        {
            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loading...");
            Logger.LogDebug(string.Format("Verifying that exports directory ({0}) is present", exportDirectoryPath));
            if (!Directory.Exists(exportDirectoryPath))
            {
                Directory.CreateDirectory(exportDirectoryPath);
            }
            // Apply all of our patches

            Harmony.PatchAll();
            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loaded.");
            Log = Logger;
        }
    }
}

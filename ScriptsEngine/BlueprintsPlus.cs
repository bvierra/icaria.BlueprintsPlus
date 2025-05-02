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
        private const string MyGUID = "host.vierra.Icaria.BlueprintsPlus";
        private const string PluginName = "BlueprintsPlus";
        private const string VersionString = "1.0.0";

        public static string exportDirectoryPath = Path.Combine(DirectoryPath.RolebooksDirectoryFullPath, "Exports");
        public static int ExportVersion = 1;

        private static Harmony _hi;
        public static ManualLogSource Log = new ManualLogSource(PluginName);

        /// <summary>
        /// Initialise the configuration settings and patch methods
        /// </summary>
        private void Awake()
        {
          _hi = Harmony.CreateAndPatchAll(typeof(Hooks));
          Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loading...");
          Logger.LogDebug(string.Format("Verifying that exports directory ({0}) is present", exportDirectoryPath));
          if (!Directory.Exists(exportDirectoryPath))
          {
            Directory.CreateDirectory(exportDirectoryPath);
          }
          // Apply all of our patches

          //Harmony.PatchAll();
          Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loaded.");
          Log = Logger;
        }

        private void OnDestroy()
        {
          // Unpatch all of our patches
          _hi.UnpatchSelf();
          Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is unloading...");
        }
    }
}

namespace BlueprintsPlus.Patches
{
  // TODO Review this file and update to your own requirements, or remove it altogether if not required

  /// <summary>
  /// Sample Harmony Patch class. Suggestion is to use one file per patched class
  /// though you can include multiple patch classes in one file.
  /// Below is included as an example, and should be replaced by classes and methods
  /// for your mod.
  /// </summary>
  [HarmonyPatch(typeof(Robants.Engine.RoleBook))]
  internal class RoleBookPatch
  {

    // TODO: Add Role RoleBookLoad_Prefix
    // Must be prefix to properly convert the rolebook.
    [HarmonyPatch(nameof(Robants.Engine.RoleBook.Save))]
    [HarmonyPostfix]
    public static void RoleBookSave_Postfix(RoleBook __instance)
    {
      if (string.IsNullOrEmpty(__instance.Name) || __instance.Database == null || __instance.Database.Count == 0)
      {
        return;
      }
      try
      {
        string rolebookName = SecurityElement.Escape(__instance.Name);
        BlueprintsPlusPlugin.Log.LogDebug(string.Format("Saving RoleBook: {0}...", rolebookName));
        string exportFilePath = Path.Combine(BlueprintsPlusPlugin.exportDirectoryPath, rolebookName + ".txt");
        string exportFilePath2 = Path.Combine(BlueprintsPlusPlugin.exportDirectoryPath, rolebookName + ".bak");

        if (File.Exists(exportFilePath))
        {
          if (File.Exists(exportFilePath2))
          {
            File.Delete(exportFilePath2);
          }
          File.Move(exportFilePath, exportFilePath2);
        }
        StreamWriter sw = new StreamWriter(exportFilePath);
        foreach (List<Robants.Engine.Role> list in __instance.Database.TypedRoles.Values)
        {
          foreach (Robants.Engine.Role role in list)
          {
            BlueprintsPlusPlugin.Log.LogDebug(string.Format("\n{0}", role.RoleScript.ToYamlString()));
            BlueprintsPlusPlugin.Log.LogDebug(string.Format("\n{0}", Compression.ToCompressedBase64(role.RoleScript.ToYamlString())));

            string name = SecurityElement.Escape(role.Tag.ToString());
            name = name.Remove(0, 5);
            string entityType = SecurityElement.Escape(role.EntityType.ToString());
            entityType = entityType.Remove(0, 5);
            string data = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(role.RoleScript.ToYamlString()));
            // stroing data = Compression.ToCompressedBase64(role.RoleScript.ToYamlString())
            string outputText = string.Concat(new string[] { "{\"Name\":\"", name, "\",\"Type\":\"", entityType, "\", \"ExportVersion\":" + BlueprintsPlusPlugin.ExportVersion + ", \"Data\":\"", data, "\"}" });
            BlueprintsPlusPlugin.Log.LogDebug(string.Format("\n{0}", outputText));
            try
            {
              sw.WriteLine(outputText);
            }
            catch (Exception e)
            {
              BlueprintsPlusPlugin.Log.LogError(string.Format("Error exporting Role \"{0}\"", name));
              BlueprintsPlusPlugin.Log.LogError(string.Format("File Location: {0}", exportFilePath));
              BlueprintsPlusPlugin.Log.LogError(string.Format("Exception: {0}", e));
            }
            finally
            {
            }
            BlueprintsPlusPlugin.Log.LogDebug(string.Format(" * Exporting Role: {0}...OK", name));
          }
        }
        sw.Close();
        BlueprintsPlusPlugin.Log.LogDebug(string.Format("Completed exporting RoleBook: {0}", rolebookName));
      }
      catch
      {

      }
      finally
      {
        
      }
    }
  }
}


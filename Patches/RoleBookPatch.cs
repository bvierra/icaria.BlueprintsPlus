using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Xml.Linq;
using HarmonyLib;
using Robants.Engine;
using Robants.Schema;

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
        [HarmonyPatch(nameof(Robants.Engine.RoleBook.Save))]
        [HarmonyPostfix]
        public static void RoleBooks_Postfix(RoleBook __instance)
        {
            try
            {
                if (string.IsNullOrEmpty(__instance.Name) || __instance.Database == null || __instance.Database.Count == 0)
                {
                    return;
                }
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
                        string data = Compression.ToCompressedBase64(role.RoleScript.ToYamlString());
                        string outputText = string.Concat(new string[] { "{\"Name\":\"", name, "\",\"Type\":\"", entityType, "\", \"ExportVersion\":" + BlueprintsPlusPlugin.ExportVersion + ", \"Data\":\"", data, "\"}" });
                        BlueprintsPlusPlugin.Log.LogDebug(string.Format("\n{0}", outputText));
                        try
                        {
                            sw.WriteLine(outputText);
                        }
                        catch (Exception e)
                        {
                            BlueprintsPlusPlugin.Log.LogError(string.Format("Error exporting Role {0}", name));
                            BlueprintsPlusPlugin.Log.LogError(string.Format("File Location: {0}", exportFilePath));
                            BlueprintsPlusPlugin.Log.LogError(string.Format("Exception: {0}", e));
                        }
                        finally
                        {
                            BlueprintsPlusPlugin.Log.LogInfo(string.Format(" * Exporting Role: {0}...OK", name));
                        }
                    }

                }
                BlueprintsPlusPlugin.Log.LogDebug(string.Format("Completed saving RoleBook: {0}", rolebookName));
                sw.Close();
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
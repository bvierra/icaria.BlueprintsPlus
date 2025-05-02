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
      BlueprintsPlusPlugin.Log.LogDebug(string.Format("Saving Compressed RoleBook {0}...", __instance.Name));
      ExportCompressed(__instance);
      BlueprintsPlusPlugin.Log.LogDebug(string.Format("Saved Compressed RoleBook {0}...", __instance.Name));
// BlueprintsPlusPlugin.Log.LogDebug(string.Format("Saving UnCompressed RoleBook {0}...", __instance.Name));
// ExportUnCompressed(__instance);
// BlueprintsPlusPlugin.Log.LogDebug(string.Format("Saved UnCompressed RoleBook {0}...", __instance.Name));
    }


    public static void ExportCompressed(RoleBook __instance)
    {
      try
      {
        string rolebookName = SecurityElement.Escape(__instance.Name);
        BlueprintsPlusPlugin.Log.LogDebug(string.Format("Saving Compressed RoleBook: {0}...", rolebookName));
        string exportFilePath = Path.Combine(BlueprintsPlusPlugin.exportDirectoryPath, rolebookName + "-compressed.txt");
        //string exportFilePath2 = Path.Combine(BlueprintsPlusPlugin.exportDirectoryPath, rolebookName + ".bak");

        StreamWriter sw = new StreamWriter(exportFilePath + ".tmp");
        sw.WriteLine("[");
        int i = 0;
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
            //string data = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(role.RoleScript.ToYamlString()));
            string data = Compression.ToCompressedBase64(role.RoleScript.ToYamlString());

            if (i >= 1)
            {
              sw.WriteLine(",");
            }
            i++;
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
        sw.WriteLine("]");
        sw.Close();
        if (File.Exists(exportFilePath))
        {
          File.Delete(exportFilePath);
        }
        File.Move(exportFilePath + ".tmp", exportFilePath);
        BlueprintsPlusPlugin.Log.LogDebug(string.Format("Completed exporting RoleBook: {0}", rolebookName));
      }
      catch
      {

      }
      finally
      {

      }
    }

    public static void ExportUnCompressed(RoleBook __instance)
    {
      try
      {
        string rolebookName = SecurityElement.Escape(__instance.Name);
        BlueprintsPlusPlugin.Log.LogDebug(string.Format("Saving Compressed RoleBook: {0}...", rolebookName));
        string exportFilePath = Path.Combine(BlueprintsPlusPlugin.exportDirectoryPath, rolebookName + "-uncompressed.txt");

        StreamWriter sw = new StreamWriter(exportFilePath + ".tmp");
        sw.WriteLine("[");
        int i = 0;
        foreach (List<Robants.Engine.Role> list in __instance.Database.TypedRoles.Values)
        {
          foreach (Robants.Engine.Role role in list)
          {
            BlueprintsPlusPlugin.Log.LogDebug(string.Format("\n{0}", role.RoleScript.ToYamlString()));
            //BlueprintsPlusPlugin.Log.LogDebug(string.Format("\n{0}", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(role.RoleScript.ToYamlString()))));

            string name = SecurityElement.Escape(role.Tag.ToString());
            name = name.Remove(0, 5);
            string entityType = SecurityElement.Escape(role.EntityType.ToString());
            entityType = entityType.Remove(0, 5);
            string data = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(role.RoleScript.ToYamlString()));
            //string data = Compression.ToCompressedBase64(role.RoleScript.ToYamlString());

            if (i >= 1)
            {
              sw.WriteLine(",");
            }
            i++;
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
        sw.WriteLine("]");
        sw.Close();
        if (File.Exists(exportFilePath))
        {
          File.Delete(exportFilePath);
        }
        File.Move(exportFilePath + ".tmp", exportFilePath);
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
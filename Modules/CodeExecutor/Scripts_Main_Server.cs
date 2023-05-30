using Marketplace.Paths;

namespace Marketplace.Modules.CodeExecutor;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Last, "OnInit", new[]{"ScriptFile.cs"}, new[]{"OnScriptsUpdate"})]
public static class Scripts_Main_Server
{
    private static void OnInit() => ReadScripts();

    private static void ReadScripts()
    {
        Scripts_DataTypes.SyncedScripts.Value.Clear();
        string path = Market_Paths.ScriptsFolder;
        string[] files = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            string scriptName = Path.GetFileNameWithoutExtension(file).ToLower();
            string scriptCode = File.ReadAllText(file).Replace("\n", "");
            Scripts_DataTypes.SyncedScripts.Value[scriptName] = scriptCode;
        }
        Scripts_DataTypes.SyncedScripts.Update();
    }

    private static void OnScriptsUpdate()
    {
        ReadScripts();
        Utils.print($"Scripts updated. {Scripts_DataTypes.SyncedScripts.Value.Count} scripts found. Sending to clients");
    }
}
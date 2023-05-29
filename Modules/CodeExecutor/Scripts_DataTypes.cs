namespace Marketplace.Modules.CodeExecutor;

public static class Scripts_DataTypes
{
    internal static readonly CustomSyncedValue<Dictionary<string, string>> SyncedScripts = 
        new(Marketplace.configSync, "SyncedScripts", new Dictionary<string, string>());
}
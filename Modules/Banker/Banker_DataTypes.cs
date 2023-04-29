namespace Marketplace.Modules.Banker;

public static class Banker_DataTypes
{
    internal static readonly CustomSyncedValue<Dictionary<string, List<int>>> SyncedBankerProfiles = new(Marketplace.configSync, "bankerData", new Dictionary<string, List<int>>());
    public static readonly Dictionary<int, int> BankerClientData = new();
    
    
    
}
using ServerSync;

namespace Marketplace.Modules.Banker;

public static class Banker_DataTypes
{
    internal static readonly CustomSyncedValue<Dictionary<string, List<int>>> SyncedBankerProfiles = new(Marketplace.configSync, "bankerData", new());
    public static readonly Dictionary<int, int> BankerClientData = new();
    
    
    
}
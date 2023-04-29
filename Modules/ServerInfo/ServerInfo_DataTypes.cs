namespace Marketplace.Modules.ServerInfo;

public static class ServerInfo_DataTypes
{
    internal static readonly CustomSyncedValue<Dictionary<string, string>> ServerInfoData =
        new(Marketplace.configSync, "infoData", new Dictionary<string, string>());
}
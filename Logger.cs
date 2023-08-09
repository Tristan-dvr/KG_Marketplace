using Marketplace.Paths;

namespace Marketplace;

internal static class Market_Logger
{
    public enum LogType
    {
        Banker,
        Marketplace,
        Territory,
        Trader,
        Transmog,
    }

    internal static void Log(LogType type, string message)
    {
        if(type is LogType.Trader && !Global_Values.EnableTraderLog) return;
        if(type is LogType.Transmog && !Global_Values.EnableTransmogLog) return;
        string LogStr = type switch
        {
            LogType.Banker => $"[{DateTime.Now}] [Banker] " + message + "\n",
            LogType.Marketplace => $"[{DateTime.Now}] [Marketplace] " + message + "\n",
            LogType.Territory => $"[{DateTime.Now}] [Territory] " + message + "\n",
            LogType.Trader => $"[{DateTime.Now}] [Trader] " + message + "\n",
            LogType.Transmog => $"[{DateTime.Now}] [Transmog] " + message + "\n",
            _ => ""
        };
        File.AppendAllText(Market_Paths.LoggerPath, LogStr);
    }

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    [ServerOnlyPatch]
    private static class ZNetScene_Awake_Patch
    {
        private static void Postfix()
        {
            if(!ZNet.instance.IsServer()) return;
            ZRoutedRpc.instance.Register("LogOnServer_mpasn",
                new Action<long, int, string>((_, type, message) =>
                {
                    Log((LogType)type, Localization.instance.Localize(message));
                }));
        }
    }
}
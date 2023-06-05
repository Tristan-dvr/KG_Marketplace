using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Marketplace.Paths;

namespace Marketplace.Modules.Leaderboard;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Last, "OnInit")]
public static class LeaderBoard_Main_Client
{
    private static void OnInit()
    {
        GameEvents.OnPlayerDeath += () => SendToServer(Leaderboard_DataTypes.TriggerType.Died);
        GameEvents.OnCreatureKilled += (prefab, _) => SendToServer(Leaderboard_DataTypes.TriggerType.MonstersKilled, prefab);
        GameEvents.OnStructureBuilt += prefab => SendToServer(Leaderboard_DataTypes.TriggerType.StructuresBuilt, prefab);
        GameEvents.OnItemCrafted += (prefab, _) => SendToServer(Leaderboard_DataTypes.TriggerType.ItemsCrafted, prefab);
        GameEvents.KilledBy += prefab => SendToServer(Leaderboard_DataTypes.TriggerType.KilledBy, prefab);
        Leaderboard_UI.Init();
        Marketplace.Global_Updator += Update;
    }

    private static void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && Leaderboard_UI.IsVisible())
        {
            Leaderboard_UI.Hide();
            Menu.instance.OnClose();
        }
    }

    private static void SendToServer(Leaderboard_DataTypes.TriggerType type, params object[] args)
    {
        switch (type)
        {
            case Leaderboard_DataTypes.TriggerType.MonstersKilled:
            case Leaderboard_DataTypes.TriggerType.ItemsCrafted:
            case Leaderboard_DataTypes.TriggerType.StructuresBuilt:
            case Leaderboard_DataTypes.TriggerType.KilledBy:
                ZPackage pkg = new();
                pkg.Write((string)args[0]);
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(),
                    "Marketplace_Leaderboard_Receive", (int)type, pkg);
                return; 
            case Leaderboard_DataTypes.TriggerType.Died:
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(),
                    "Marketplace_Leaderboard_Receive", (int)type, new ZPackage());
                return;
            case Leaderboard_DataTypes.TriggerType.Explored:
                pkg = new();
                pkg.Write((float)args[0]);
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(),
                    "Marketplace_Leaderboard_Receive", (int)type, pkg);
                return;
            default:
                return;
        }
    }
    
    [HarmonyPatch(typeof(Player), nameof(Player.Update))]
    [ClientOnlyPatch]
    private static class Player_Update_Patch
    {
        private static float _time;
        
        private static void Postfix(Player __instance)
        {
            if (__instance != Player.m_localPlayer || !Minimap.instance) return;
            _time += Time.deltaTime;
            if (_time >= 3 * 60)
            {
                int length = Minimap.instance.m_explored.Length;
                int trueAmount = 0;
                Parallel.ForEach(Partitioner.Create(0, length), new(){MaxDegreeOfParallelism = Environment.ProcessorCount / 2}, range =>
                {
                    int localCount = 0;
                    for (int i = range.Item1; i < range.Item2; i++)
                        if (Minimap.instance.m_explored[i])
                            ++localCount;
                    Interlocked.Add(ref trueAmount, localCount);
                });
                float exploration = (float)trueAmount / Minimap.instance.m_explored.Length;
                exploration = (float)Math.Round(exploration * 100, 2);
                SendToServer(Leaderboard_DataTypes.TriggerType.Explored, exploration);
            }

        }
    }
}
using System.Diagnostics;
using Marketplace.Paths;

namespace Marketplace.Modules.Leaderboard;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Last, "OnInit",
    new[] { "LeaderboardTitles.cfg" },
    new[] { "OnTitlesFileChange" })]
public static class Leaderboard_Server_Main
{
    private static void OnInit()
    {
        string data = File.ReadAllText(Market_Paths.MarketLeaderboardJSON);
        if (!string.IsNullOrEmpty(data))
            Leaderboard_DataTypes.ServersidePlayersLeaderboard =
                JSON.ToObject<Dictionary<string, Leaderboard_DataTypes.Player_Leaderboard>>(data);
        ReadTitlesProfiles(File.ReadAllLines(Market_Paths.TitlesProfiles));
        SendToPlayers();
        Marketplace._thistype.StartCoroutine(SaveAndUpdate());
    }

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    [ServerOnlyPatch]
    private static class ZNetScene_Awake_Patch
    {
        private static void Postfix() =>
            ZRoutedRpc.instance.Register<int, ZPackage>("Marketplace_Leaderboard_Receive", RPC_ReceiveLeaderboardEvent);
    }

    private static void RPC_ReceiveLeaderboardEvent(long sender, int type, ZPackage pkg)
    {
        if(!Global_Values._container.Value._useLeaderboard) return;
        Leaderboard_DataTypes.TriggerType triggerType = (Leaderboard_DataTypes.TriggerType)type;
        ZNetPeer peer = ZNet.instance.GetPeer(sender);
        if (peer == null) return;
        string id = peer.m_socket.GetHostName();
        if (id == "0") return;
        if (!Leaderboard_DataTypes.ServersidePlayersLeaderboard.ContainsKey(id))
            Leaderboard_DataTypes.ServersidePlayersLeaderboard.Add(id, new Leaderboard_DataTypes.Player_Leaderboard());

        if (Leaderboard_DataTypes.ServersidePlayersLeaderboard[id].PlayerName == null)
            Leaderboard_DataTypes.ServersidePlayersLeaderboard[id].PlayerName = peer.m_playerName;

        switch (triggerType)
        {
            case Leaderboard_DataTypes.TriggerType.MonstersKilled:
                string prefab = pkg.ReadString();
                if (!Leaderboard_DataTypes.ServersidePlayersLeaderboard[id].KilledCreatures.ContainsKey(prefab))
                    Leaderboard_DataTypes.ServersidePlayersLeaderboard[id].KilledCreatures.Add(prefab, 1);
                else Leaderboard_DataTypes.ServersidePlayersLeaderboard[id].KilledCreatures[prefab]++;
                break;
            case Leaderboard_DataTypes.TriggerType.ItemsCrafted:
                prefab = pkg.ReadString();
                if (!Leaderboard_DataTypes.ServersidePlayersLeaderboard[id].ItemsCrafted.ContainsKey(prefab))
                    Leaderboard_DataTypes.ServersidePlayersLeaderboard[id].ItemsCrafted.Add(prefab, 1);
                else Leaderboard_DataTypes.ServersidePlayersLeaderboard[id].ItemsCrafted[prefab]++;
                break;
            case Leaderboard_DataTypes.TriggerType.StructuresBuilt:
                prefab = pkg.ReadString();
                if (!Leaderboard_DataTypes.ServersidePlayersLeaderboard[id].BuiltStructures.ContainsKey(prefab))
                    Leaderboard_DataTypes.ServersidePlayersLeaderboard[id].BuiltStructures.Add(prefab, 1);
                else Leaderboard_DataTypes.ServersidePlayersLeaderboard[id].BuiltStructures[prefab]++;
                break;
            case Leaderboard_DataTypes.TriggerType.KilledBy:
                prefab = pkg.ReadString();
                if (!Leaderboard_DataTypes.ServersidePlayersLeaderboard[id].KilledBy.ContainsKey(prefab))
                    Leaderboard_DataTypes.ServersidePlayersLeaderboard[id].KilledBy.Add(prefab, 1);
                else Leaderboard_DataTypes.ServersidePlayersLeaderboard[id].KilledBy[prefab]++;
                break;
            case Leaderboard_DataTypes.TriggerType.Died:
                Leaderboard_DataTypes.ServersidePlayersLeaderboard[id].DeathAmount++;
                break;
            case Leaderboard_DataTypes.TriggerType.Explored:
                Leaderboard_DataTypes.ServersidePlayersLeaderboard[id].MapExplored = pkg.ReadSingle();
                break;
            default:
                return;
        }
    }

    private static IEnumerator SaveAndUpdate()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(5 * 60);
            if (!ZNet.instance || !ZNet.instance.IsServer() || !Global_Values._container.Value._useLeaderboard) continue;
            File.WriteAllText(Market_Paths.MarketLeaderboardJSON,
                JSON.ToNiceJSON(Leaderboard_DataTypes.ServersidePlayersLeaderboard));
            SendToPlayers();
        }
    }

    private static void SendToPlayers()
    {
        Leaderboard_DataTypes.SyncedClientLeaderboard.Value.Clear();

        foreach (KeyValuePair<string, Leaderboard_DataTypes.Player_Leaderboard> leaderboard in Leaderboard_DataTypes
                     .ServersidePlayersLeaderboard)
        {
            Leaderboard_DataTypes.Client_Leaderboard newLeaderboard = new()
            {
                PlayerName = leaderboard.Value.PlayerName,
                ItemsCrafted = leaderboard.Value.ItemsCrafted.Sum(x => x.Value),
                KilledCreatures = leaderboard.Value.KilledCreatures.Sum(x => x.Value),
                BuiltStructures = leaderboard.Value.BuiltStructures.Sum(x => x.Value),
                KilledPlayers = leaderboard.Value.KilledPlayers,
                Died = leaderboard.Value.DeathAmount,
                MapExplored = leaderboard.Value.MapExplored,
                Titles = new()
            };
            foreach (var title in Leaderboard_DataTypes.AllTitles)
            {
                if (title.Check(leaderboard.Key)) newLeaderboard.Titles.Add(title.ID);
            }

            Leaderboard_DataTypes.SyncedClientLeaderboard.Value.Add(leaderboard.Key, newLeaderboard);
        }

        Leaderboard_DataTypes.SyncedClientLeaderboard.Update();
    }

    private static void ReadTitlesProfiles(IReadOnlyList<string> profiles)
    {
        Leaderboard_DataTypes.AllTitles.Clear();
        Leaderboard_DataTypes.Title currentTitle = null;
        for (int i = 0; i < profiles.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(profiles[i]) || profiles[i].StartsWith("#")) continue;
            if (profiles[i].StartsWith("["))
            {
                currentTitle = new Leaderboard_DataTypes.Title
                {
                    ID = profiles[i].Replace("[", "").Replace("]", "").ToLower().GetStableHashCode()
                };
            }
            else
            {
                if (currentTitle == null) continue;
                try
                {
                    string type = profiles[i];
                    string name = profiles[i + 1];
                    string description = profiles[i + 2];
                    string prefabParse = profiles[i + 3];
                    string color = profiles[i + 4];
                    string tierParse = profiles[i + 5];

                    if (!Enum.TryParse(type, out Leaderboard_DataTypes.TriggerType triggerType))
                    {
                        i += 5;
                        continue;
                    }
                    currentTitle.Type = triggerType;
                    currentTitle.Name = name;
                    currentTitle.Description = description;

                    if (triggerType is Leaderboard_DataTypes.TriggerType.ItemsCrafted
                        or Leaderboard_DataTypes.TriggerType.MonstersKilled
                        or Leaderboard_DataTypes.TriggerType.StructuresBuilt)
                    {
                        string[] prefabSplit = prefabParse.Replace(" ", "").Split(',');
                        currentTitle.Prefab = prefabSplit[0];
                        currentTitle.MinAmount = int.Parse(prefabSplit[1]);
                    }
                    else
                    {
                        currentTitle.MinAmount = int.Parse(prefabParse);
                    }

                    string[] colorSplit = color.Replace(" ", "").Split(',');
                    currentTitle.Color = new Color32(byte.Parse(colorSplit[0]), byte.Parse(colorSplit[1]),
                        byte.Parse(colorSplit[2]), 255);
                    currentTitle.Score = int.Parse(tierParse);
                    Leaderboard_DataTypes.AllTitles.Add(currentTitle);
                    currentTitle = null;
                }
                catch (Exception ex)
                {
                    Utils.print($"Failed to parse title {currentTitle.Name}: {ex.Message}", ConsoleColor.Red);
                    i += 5;
                }
            }
        }
        
        Leaderboard_DataTypes.AllTitles = Leaderboard_DataTypes.AllTitles.OrderByDescending(x => x.Score).ToList();
        Leaderboard_DataTypes.SyncedClientTitles.Value.Clear();
        foreach (var title in Leaderboard_DataTypes.AllTitles)
        {
            Leaderboard_DataTypes.SyncedClientTitles.Value.Add(new()
            {
                ID = title.ID,
                Name = title.Name,
                Description = title.Description,
                Color = title.Color,
                Score = title.Score
            });
        }
        Leaderboard_DataTypes.SyncedClientTitles.Update();
    }

    private static void OnTitlesFileChange()
    {
        ReadTitlesProfiles(File.ReadAllLines(Market_Paths.TitlesProfiles));
        Utils.print("Titles Changed. Sending new info to all clients");
    }
}
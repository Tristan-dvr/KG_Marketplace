using Marketplace.Modules.Global_Options;
using Marketplace.Paths;

namespace Marketplace.Modules.Leaderboard;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Last, "OnInit",
    new[] { "LA" },
    new[] { "OnAchievementsFileChange" })]
public static class Leaderboard_Server_Main
{
    [UsedImplicitly]
    private static void OnInit()
    {
        string data = File.ReadAllText(Market_Paths.MarketLeaderboardJSON);
        if (!string.IsNullOrEmpty(data))
            Leaderboard_DataTypes.ServersidePlayersLeaderboard =
                JSON.ToObject<Dictionary<string, Leaderboard_DataTypes.Player_Leaderboard>>(data);
        ReadAchievementsProfiles();
        SendToPlayers();
        Marketplace._thistype.StartCoroutine(SaveAndUpdate());
    }

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    [ServerOnlyPatch]
    private static class ZNetScene_Awake_Patch
    {
        [UsedImplicitly]
        private static void Postfix() =>
            ZRoutedRpc.instance.Register<int, ZPackage>("Marketplace_Leaderboard_Receive", RPC_ReceiveLeaderboardEvent);
    }

    private static void RPC_ReceiveLeaderboardEvent(long sender, int type, ZPackage pkg)
    {
        if (!Global_Configs.SyncedGlobalOptions.Value._useLeaderboard) return;
        Leaderboard_DataTypes.TriggerType triggerType = (Leaderboard_DataTypes.TriggerType)type;
        ZNetPeer peer = ZNet.instance.GetPeer(sender);
        if (peer == null) return;
        string id = peer.m_socket.GetHostName();
        if (id == "0") return;
        
        id = id + "_" + peer.m_playerName;
        
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
            case Leaderboard_DataTypes.TriggerType.Harvested:
                prefab = pkg.ReadString();
                if (!Leaderboard_DataTypes.ServersidePlayersLeaderboard[id].Harvested.ContainsKey(prefab))
                    Leaderboard_DataTypes.ServersidePlayersLeaderboard[id].Harvested.Add(prefab, 1);
                else Leaderboard_DataTypes.ServersidePlayersLeaderboard[id].Harvested[prefab]++;
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
            if (!ZNet.instance || !ZNet.instance.IsServer() ||
                !Global_Configs.SyncedGlobalOptions.Value._useLeaderboard) continue;
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
                Harvested = leaderboard.Value.Harvested.Sum(x => x.Value),
                KilledPlayers = leaderboard.Value.KilledPlayers,
                Died = leaderboard.Value.DeathAmount,
                MapExplored = leaderboard.Value.MapExplored,
                Achievements = new()
            };
            foreach (Leaderboard_DataTypes.Achievement achievement in Leaderboard_DataTypes.AllAchievements)
            {
                if (achievement.Check(leaderboard.Key)) newLeaderboard.Achievements.Add(achievement.ID);
            }

            Leaderboard_DataTypes.SyncedClientLeaderboard.Value.Add(leaderboard.Key, newLeaderboard);
        }

        Leaderboard_DataTypes.SyncedClientLeaderboard.Update();
    }

    private static void ProcessAchievementsProfile(string fPath, IReadOnlyList<string> profiles)
    {
        Leaderboard_DataTypes.Achievement currentAchievement = null;
        for (int i = 0; i < profiles.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(profiles[i]) || profiles[i].StartsWith("#")) continue;
            if (profiles[i].StartsWith("["))
            {
                currentAchievement = new Leaderboard_DataTypes.Achievement
                {
                    ID = profiles[i].Replace("[", "").Replace("]", "").ToLower().GetStableHashCode()
                };
            }
            else
            {
                if (currentAchievement == null) continue;
                try
                {
                    string type = profiles[i];
                    string name = profiles[i + 1];
                    string description = profiles[i + 2];
                    string prefabParse = profiles[i + 3];
                    string color = profiles[i + 4];
                    string tierParse = profiles[i + 5];
                    if (!Enum.TryParse(type, true, out Leaderboard_DataTypes.TriggerType triggerType))
                    {
                        i += 5;
                        continue;
                    }

                    currentAchievement.Type = triggerType;
                    currentAchievement.Name = name;
                    currentAchievement.Description = description;

                    if (triggerType is not (Leaderboard_DataTypes.TriggerType.Explored 
                        or Leaderboard_DataTypes.TriggerType.Died 
                        or Leaderboard_DataTypes.TriggerType.PlayersKilled)) 
                    {
                        string[] prefabSplit = prefabParse.Replace(" ", "").Split(',');
                        currentAchievement.Prefab = prefabSplit[0];
                        currentAchievement.MinAmount = int.Parse(prefabSplit[1]);
                    }
                    else
                    {
                        currentAchievement.MinAmount = int.Parse(prefabParse);
                    }

                    string[] colorSplit = color.Replace(" ", "").Split(',');
                    currentAchievement.Color = new Color32(byte.Parse(colorSplit[0]), byte.Parse(colorSplit[1]),
                        byte.Parse(colorSplit[2]), 255);
                    currentAchievement.Score = int.Parse(tierParse);
                    Leaderboard_DataTypes.AllAchievements.Add(currentAchievement);
                    currentAchievement = null;
                }
                catch (Exception ex)
                {
                    Utils.print($"Failed to parse achievement {fPath} {currentAchievement?.Name}: {ex.Message}",
                        ConsoleColor.Red);
                    i += 5;
                }
            }
        }

        Leaderboard_DataTypes.AllAchievements =
            Leaderboard_DataTypes.AllAchievements.OrderByDescending(x => x.Score).ToList();
        Leaderboard_DataTypes.SyncedClientAchievements.Value.Clear();
        foreach (Leaderboard_DataTypes.Achievement achievement in Leaderboard_DataTypes.AllAchievements)
        {
            Leaderboard_DataTypes.SyncedClientAchievements.Value.Add(new()
            {
                ID = achievement.ID,
                Name = achievement.Name,
                Description = achievement.Description,
                Color = achievement.Color,
                Score = achievement.Score
            });
        }
    }
    
    private static void ReadAchievementsProfiles()
    {
        Leaderboard_DataTypes.AllAchievements.Clear();
        string folder = Market_Paths.LeaderboardAchievementsFolder;
        string[] files = Directory.GetFiles(folder, "*.cfg", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            IReadOnlyList<string> profiles = File.ReadAllLines(file).ToList();
            ProcessAchievementsProfile(file, profiles);
        }
        Leaderboard_DataTypes.SyncedClientAchievements.Update();
    }

    [UsedImplicitly]
    private static void OnAchievementsFileChange()
    {
        ReadAchievementsProfiles();
        Utils.print("Achievements Changed. Sending new info to all clients");
    }
}
using BepInEx.Configuration;
using Marketplace.Paths;
using Random = UnityEngine.Random;

namespace Marketplace.Modules.Battlepass;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Normal, "OnInit",
    new[] { "BattlepassFreeRewards.cfg", "BattlepassPremiumRewards.cfg", "BattlepassConfig.cfg" },
    new[] { "OnBattlepassProfileChange", "OnBattlepassProfileChange", "OnBattlepassProfileChange" })]
public static class Battlepass_Main_Server
{
    private static ConfigFile BP_Config;

    private static void OnInit()
    {
        BP_Config = new ConfigFile(Market_Paths.BattlepassConfigPath, true);
        ReadServerBattlepassRewards();
    }

    private static void OnBattlepassProfileChange()
    {
        Marketplace._thistype.StartCoroutine(DelayedReloadBattlepass());
    }

    private static IEnumerator DelayedReloadBattlepass()
    {
        yield return new WaitForSecondsRealtime(2f);
        BP_Config.Reload();
        ReadServerBattlepassRewards();
        Utils.print("Battlepass Changed. Sending new info to all clients");
    }

    private static void ReadServerBattlepassRewards()
    {
        string name = BP_Config.Bind("BP", "Battlepass Name (Unique)", "FirstSeason",
            "After changing battlepass name it will drop all experience / rewards for previous battlepass").Value;
        Battlepass_DataTypes.SyncedBattlepassData.Value.UID = name.ToLower().GetStableHashCode();
        Battlepass_DataTypes.SyncedBattlepassData.Value.Name = name;
        Battlepass_DataTypes.SyncedBattlepassData.Value.ExpStep = BP_Config.Bind("BP", "Battlepass Experience Step",
            100, "Experience Step Between Each Reward Order").Value;
        Battlepass_DataTypes.SyncedBattlepassData.Value.PremiumUsers =
            BP_Config.Bind("BP", "Premium Users", "User IDs", "Premium Users").Value;
        Battlepass_DataTypes.SyncedBattlepassData.Value.FreeRewards =
            new List<Battlepass_DataTypes.BattlePassElement>();
        Battlepass_DataTypes.SyncedBattlepassData.Value.PremiumRewards =
            new List<Battlepass_DataTypes.BattlePassElement>();
        Battlepass_DataTypes.SyncedBattlepassData.Value._revision = Random.Range(int.MinValue, int.MaxValue);
        List<string> freeData = File.ReadAllLines(Market_Paths.BattlepassFreeRewardsPath).ToList();
        if (freeData.Count > 0)
        {
            HashSet<int> TakenOrders = new();
            Battlepass_DataTypes.BattlePassElement currentElement = null;
            int latestOrder = -1;
            for (int i = 0; i < freeData.Count; i++)
            {
                try
                {
                    if (freeData[i].StartsWith("["))
                    {
                        if (currentElement != null)
                        {
                            Battlepass_DataTypes.SyncedBattlepassData.Value.FreeRewards.Add(currentElement);
                            currentElement = null;
                        }

                        string[] split = freeData[i].Replace("[", "").Replace("]", "").Split('=');
                        latestOrder = split.Length == 2 ? Convert.ToInt32(split[1]) - 1 : latestOrder + 1;
                        latestOrder = Mathf.Max(0, latestOrder);
                        if (!TakenOrders.Contains(latestOrder))
                        {
                            TakenOrders.Add(latestOrder);
                        }
                        else
                        {
                            Utils.print($" (Battlepass) Skipping {split[0]} free element because order already exists");
                            continue;
                        }

                        currentElement = new Battlepass_DataTypes.BattlePassElement
                        {
                            RewardName = split[0],
                            Order = latestOrder,
                            ItemNames = new List<string>(),
                            ItemCounts = new List<int>(),
                            ItemLevels = new List<int>()
                        };
                    }
                    else
                    {
                        if (currentElement == null || string.IsNullOrWhiteSpace(freeData[i]) ||
                            freeData[i].StartsWith("#")) continue;
                        string[] split = freeData[i].Split(',');
                        if (split.Length == 2)
                        {
                            currentElement.ItemNames.Add(split[0]);
                            currentElement.ItemCounts.Add(Convert.ToInt32(split[1]));
                            currentElement.ItemLevels.Add(1);
                        }

                        if (split.Length == 3)
                        {
                            currentElement.ItemNames.Add(split[0]);
                            currentElement.ItemCounts.Add(Convert.ToInt32(split[1]));
                            currentElement.ItemLevels.Add(Convert.ToInt32(split[2]));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utils.print($"Error adding free rewards at line {i + 1}. Exception:\n {ex}", ConsoleColor.Red);
                }
            }

            if (currentElement != null)
            {
                Battlepass_DataTypes.SyncedBattlepassData.Value.FreeRewards.Add(currentElement);
            }
        }


        List<string> premiumData = File.ReadAllLines(Market_Paths.BattlepassPremiumRewardsPath).ToList();
        if (premiumData.Count > 0)
        {
            HashSet<int> TakenOrders = new();
            Battlepass_DataTypes.BattlePassElement currentElement = null;
            int latestOrder = -1;
            for (int i = 0; i < premiumData.Count; i++)
            {
                try
                {
                    if (premiumData[i].StartsWith("["))
                    {
                        if (currentElement != null)
                        {
                            Battlepass_DataTypes.SyncedBattlepassData.Value.PremiumRewards.Add(currentElement);
                            currentElement = null;
                        }

                        string[] split = premiumData[i].Replace("[", "").Replace("]", "").Split('=');
                        latestOrder = split.Length == 2 ? Convert.ToInt32(split[1]) - 1 : latestOrder + 1;
                        latestOrder = Mathf.Max(0, latestOrder);
                        if (!TakenOrders.Contains(latestOrder))
                        {
                            TakenOrders.Add(latestOrder);
                        }
                        else
                        {
                            Utils.print(
                                $" (Battlepass) Skipping {split[0]} premium element because order already exists");
                            continue;
                        }

                        currentElement = new Battlepass_DataTypes.BattlePassElement
                        {
                            RewardName = split[0],
                            Order = latestOrder,
                            ItemNames = new List<string>(),
                            ItemCounts = new List<int>(),
                            ItemLevels = new List<int>()
                        };
                    }
                    else
                    {
                        if (currentElement == null || string.IsNullOrWhiteSpace(premiumData[i]) ||
                            premiumData[i].StartsWith("#")) continue;
                        string[] split = premiumData[i].Split(',');
                        if (split.Length == 2)
                        {
                            currentElement.ItemNames.Add(split[0]);
                            currentElement.ItemCounts.Add(Convert.ToInt32(split[1]));
                            currentElement.ItemLevels.Add(1);
                        }

                        if (split.Length == 3)
                        {
                            currentElement.ItemNames.Add(split[0]);
                            currentElement.ItemCounts.Add(Convert.ToInt32(split[1]));
                            currentElement.ItemLevels.Add(Convert.ToInt32(split[2]));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utils.print($"Error adding premium rewards at line {i + 1}. Exception:\n {ex}", ConsoleColor.Red);
                }
            }

            if (currentElement != null)
            {
                Battlepass_DataTypes.SyncedBattlepassData.Value.PremiumRewards.Add(currentElement);
            }
        }

        Battlepass_DataTypes.SyncedBattlepassData.Update();
    }
}
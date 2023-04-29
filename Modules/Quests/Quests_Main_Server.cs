using Marketplace.Paths;

namespace Marketplace.Modules.Quests;

[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Normal, "OnInit",
    new[] { "QuestsProfiles.cfg", "QuestsDatabase.cfg", "QuestsEvents.cfg" },
    new[] { "OnQuestsProfilesFileChange", "OnQuestDatabaseFileChange", "OnQuestsEventFileChange" })]
public static class Quests_Main_Server
{
    private static void OnInit()
    { 
        List<string> profiles = File.ReadAllLines(Market_Paths.QuestProfilesPath).ToList();
        ReadQuestProfiles(profiles);
        List<string> database = File.ReadAllLines(Market_Paths.QuestDatabasePath).ToList();
        ReadQuestDatabase(database);
        List<string> events = File.ReadAllLines(Market_Paths.QuestEventsPath).ToList();
        ReadEventDatabase(events);
    }

    private static void OnQuestsProfilesFileChange()
    {
        List<string> profiles = File.ReadAllLines(Market_Paths.QuestProfilesPath).ToList();
        ReadQuestProfiles(profiles);
        Utils.print("Quests Profiles Changed. Sending new info to all clients");
    }

    private static void OnQuestDatabaseFileChange()
    {
        List<string> database = File.ReadAllLines(Market_Paths.QuestDatabasePath).ToList();
        ReadQuestDatabase(database);
        Utils.print("Quests Database Changed. Sending new info to all clients");
    }

    private static void OnQuestsEventFileChange()
    {
        List<string> events = File.ReadAllLines(Market_Paths.QuestEventsPath).ToList();
        ReadEventDatabase(events);
        Utils.print("Quests Events Changed. Sending new info to all clients");
    }
    
    private static void ReadQuestProfiles(List<string> profiles)
    {
        Quests_DataTypes.SyncedQuestProfiles.Value.Clear();
        string splitProfile = "default";
        for (int i = 0; i < profiles.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(profiles[i]) || profiles[i].StartsWith("#")) continue;
            if (profiles[i].StartsWith("["))
            {
                splitProfile = profiles[i].Replace("[", "").Replace("]", "").ToLower();
            }
            else
            {
                string[] split = profiles[i].Replace(" ", "").Split(',').Distinct().ToArray();
                if (!Quests_DataTypes.SyncedQuestProfiles.Value.ContainsKey(splitProfile))
                {
                    Quests_DataTypes.SyncedQuestProfiles.Value.Add(splitProfile, new List<int>());
                }

                foreach (string quest in split)
                {
                    int questToHashcode = quest.ToLower().GetStableHashCode();
                    Quests_DataTypes.SyncedQuestProfiles.Value[splitProfile].Add(questToHashcode);
                }
            }
        }
        Quests_DataTypes.SyncedQuestProfiles.Update();
    }
    
    private static void ReadQuestDatabase(List<string> profiles)
    {
        if (profiles.Count == 0) return;
        Quests_DataTypes.SyncedQuestData.Value.Clear();
        string dbProfile = null;
        Quests_DataTypes.SpecialQuestTag specialQuestTag = Quests_DataTypes.SpecialQuestTag.None;
        for (int i = 0; i < profiles.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(profiles[i]) || profiles[i].StartsWith("#")) continue;
            if (profiles[i].StartsWith("["))
            {
                dbProfile = profiles[i].Replace("[", "").Replace("]", "").Replace(" ", "").ToLower();
                string[] checkTags = dbProfile.Split('=');
                if (checkTags.Length == 2)
                {
                    if (!Enum.TryParse(checkTags[1], true, out Quests_DataTypes.SpecialQuestTag tag)) continue;
                    specialQuestTag |= tag;
                    dbProfile = checkTags[0];
                }
                else
                {
                    specialQuestTag = Quests_DataTypes.SpecialQuestTag.None;
                }
            }
            else
            {
                if (dbProfile == null) continue;
                try
                {
                    int UID = dbProfile.GetStableHashCode();
                    string typeString = profiles[i];
                    string name = profiles[i + 1];
                    string image = "";
                    int imageIndex = name.IndexOf("<image=", StringComparison.Ordinal);
                    if (imageIndex != -1)
                    {
                        int imageEndIndex = name.IndexOf(">", imageIndex, StringComparison.Ordinal);
                        if (imageEndIndex != -1)
                        {
                            image = name.Substring(imageIndex + 7, imageEndIndex - imageIndex - 7);
                            name = name.Remove(imageIndex, imageEndIndex - imageIndex + 1);
                        }
                    }
                    string description = profiles[i + 2];
                    string target = profiles[i + 3];
                    string reward = profiles[i + 4];
                    string cooldown = profiles[i + 5];
                    string restrictions = profiles[i + 6];
                    if (!(Enum.TryParse(typeString, out Quests_DataTypes.QuestType type) && Enum.IsDefined(typeof(Quests_DataTypes.QuestType), type)))
                    {
                        dbProfile = null;
                        continue;
                    }

                    string[] rewardsArray = reward.Replace(" ", "").Split('|');
                    int _RewardsAMOUNT = Mathf.Max(1, rewardsArray.Length);
                    Quests_DataTypes.QuestRewardType[] rewardTypes = new Quests_DataTypes.QuestRewardType[_RewardsAMOUNT];
                    string[] RewardPrefabs = new string[_RewardsAMOUNT];
                    int[] RewardLevels = new int[_RewardsAMOUNT];
                    int[] RewardCounts = new int[_RewardsAMOUNT];
                    for (int r = 0; r < _RewardsAMOUNT; ++r)
                    {
                        string[] rwdTypeCheck = rewardsArray[r].Split(':');
                        if (!(Enum.TryParse(rwdTypeCheck[0], true,
                                out rewardTypes[r])))
                        {
                            goto GoNextLabel;
                        }

                        string[] RewardSplit = rwdTypeCheck[1].Split(',');

                        if (rewardTypes[r] is Quests_DataTypes.QuestRewardType.Battlepass_EXP or Quests_DataTypes.QuestRewardType.EpicMMO_EXP
                            or Quests_DataTypes.QuestRewardType.MH_EXP)
                        {
                            RewardPrefabs[r] = "NONE";
                            RewardCounts[r] = int.Parse(RewardSplit[0]);
                            RewardLevels[r] = 1;
                        }
                        else
                        {
                            RewardPrefabs[r] = RewardSplit[0];
                            RewardCounts[r] = int.Parse(RewardSplit[1]);
                            RewardLevels[r] = 1;
                        }

                        if (RewardSplit.Length >= 3)
                        {
                            RewardLevels[r] = Convert.ToInt32(RewardSplit[2]);
                            if (rewardTypes[r] == Quests_DataTypes.QuestRewardType.Pet) ++RewardLevels[r];
                        }
                    }

                    if (type is not Quests_DataTypes.QuestType.Talk)
                    {
                        target = target.Replace(" ", "");
                    }

                    string[] targetsArray = target.Split('|');
                    int _targetsCount = Mathf.Max(1, targetsArray.Length);
                    string[] TargetPrefabs = new string[_targetsCount];
                    int[] TargetLevels = new int[_targetsCount];
                    int[] TargetCounts = new int[_targetsCount];
                    for (int t = 0; t < _targetsCount; ++t)
                    {
                        string[] TargetSplit = targetsArray[t].Split(',');
                        TargetPrefabs[t] = TargetSplit[0];
                        TargetLevels[t] = 1;
                        TargetCounts[t] = 1;
                        
                        if (type is Quests_DataTypes.QuestType.Kill && TargetSplit.Length >= 3)
                        {
                            TargetLevels[t] = Mathf.Max(1, Convert.ToInt32(TargetSplit[2]) + 1);
                        }

                        if (type is Quests_DataTypes.QuestType.Craft or Quests_DataTypes.QuestType.Collect && TargetSplit.Length >= 3)
                        {
                            TargetLevels[t] = Mathf.Max(1, Convert.ToInt32(TargetSplit[2]));
                        }

                        if (type != Quests_DataTypes.QuestType.Talk)
                        {
                            TargetCounts[t] = Mathf.Max(1, int.Parse(TargetSplit[1]));
                        }
                    }


                    string[] restrictionArray = restrictions.Replace(" ", "").Split('|');
                    int _RestrictionsAMOUNT = Mathf.Max(1, restrictionArray.Length);
                    Quests_DataTypes.QuestRequirementType[] reqs = new Quests_DataTypes.QuestRequirementType[_RestrictionsAMOUNT];
                    string[] QuestRestriction = new string[_RestrictionsAMOUNT];
                    int[] QuestRestrictionLevel = new int[_RestrictionsAMOUNT];

                    for (int r = 0; r < _RestrictionsAMOUNT; ++r)
                    {
                        string[] RestrictionSplit = restrictionArray[r].Split(':');
                        if (Enum.TryParse(RestrictionSplit[0], out Quests_DataTypes.QuestRequirementType restrType) &&
                            Enum.IsDefined(typeof(Quests_DataTypes.QuestRequirementType), restrType) &&
                            restrType is not Quests_DataTypes.QuestRequirementType.None)
                        {
                            reqs[r] = restrType;

                            if (RestrictionSplit.Length == 1)
                            {
                                QuestRestriction[r] = "NONE";
                                QuestRestrictionLevel[r] = 0;
                                continue;
                            }

                            string[] RestrData = RestrictionSplit[1].Split(',');
                            QuestRestriction[r] = RestrData[0];
                            if (RestrData.Length == 2)
                            {
                                QuestRestrictionLevel[r] = int.Parse(RestrData[1]);
                            }
                        }
                        else
                        {
                            reqs[r] = Quests_DataTypes.QuestRequirementType.None;
                            QuestRestriction[r] = "NONE";
                            QuestRestrictionLevel[r] = 0;
                        }
                    }

                    Quests_DataTypes.Quest quest = new()
                    {
                        RewardsAMOUNT = _RewardsAMOUNT,
                        Type = type,
                        RewardType = rewardTypes,
                        Name = name,
                        Description = description,
                        TargetAMOUNT = _targetsCount,
                        TargetPrefab = TargetPrefabs,
                        TargetCount = TargetCounts,
                        TargetLevel = TargetLevels,
                        RewardPrefab = RewardPrefabs,
                        RewardCount = RewardCounts,
                        ResetTime = int.Parse(cooldown),
                        RewardLevel = RewardLevels,
                        RequirementsAMOUNT = _RestrictionsAMOUNT,
                        RequirementType = reqs,
                        QuestRequirementPrefab = QuestRestriction,
                        QuestRequirementLevel = QuestRestrictionLevel,
                        SpecialTag = specialQuestTag,
                        PreviewImage = image
                    };
                    if (!Quests_DataTypes.SyncedQuestData.Value.ContainsKey(UID))
                        Quests_DataTypes.SyncedQuestData.Value.Add(UID, quest);
                }
                catch (Exception ex)
                {
                    Utils.print($"Error in Quests {dbProfile} DB file\n{ex}", ConsoleColor.Red);
                }

                GoNextLabel:
                dbProfile = null;
            }
        }
        Quests_DataTypes.SyncedQuestData.Update();
    }

    private static void ReadEventDatabase(IReadOnlyList<string> profiles)
    {
        Quests_DataTypes.SyncedQuestsEvents.Value.Clear();
        bool ValidateEventArguments(Quests_DataTypes.QuestEventAction action, string args, string QuestID)
        {
            string[] split = args.Split(',');
            int count = split.Length;
            bool result = action switch
            {
                Quests_DataTypes.QuestEventAction.GiveItem => count == 3,
                Quests_DataTypes.QuestEventAction.GiveQuest => count == 1,
                Quests_DataTypes.QuestEventAction.Spawn => count == 3,
                Quests_DataTypes.QuestEventAction.Teleport => count == 3,
                Quests_DataTypes.QuestEventAction.Damage => count == 1,
                Quests_DataTypes.QuestEventAction.Heal => count == 1,
                Quests_DataTypes.QuestEventAction.RemoveQuest => count == 1,
                Quests_DataTypes.QuestEventAction.PlaySound => count == 1,
                Quests_DataTypes.QuestEventAction.NpcText => count >= 1,
                _ => false
            };
            if (!result)
            {
                Utils.print($"Arguments for Quest: {QuestID} => action {action} are invalid: {args}", ConsoleColor.Red);
            }

            return result;
        }

        string splitProfile = "default";
        for (int i = 0; i < profiles.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(profiles[i]) || profiles[i].StartsWith("#")) continue;
            if (profiles[i].StartsWith("["))
            {
                splitProfile = profiles[i].Replace("[", "").Replace("]", "").ToLower();
            }
            else
            {
                string[] split = profiles[i].Split(':');
                if (split.Length < 2) continue;
                string key = split[0];
                string value = split[1];
                if (!Enum.TryParse(key, out Quests_DataTypes.QuestEventCondition cond) ||
                    !Enum.IsDefined(typeof(Quests_DataTypes.QuestEventCondition), cond)) continue;

                string[] actionSplit = value.Split(new[] { ',' }, 2, StringSplitOptions.RemoveEmptyEntries);

                if (!Enum.TryParse(actionSplit[0], out Quests_DataTypes.QuestEventAction action) ||
                    !Enum.IsDefined(typeof(Quests_DataTypes.QuestEventAction), action)) continue;

                string args = actionSplit.Length > 1 ? actionSplit[1] : string.Empty;
                if(action is not Quests_DataTypes.QuestEventAction.NpcText) args = args.Replace(" ", "");

                if (!ValidateEventArguments(action, args, splitProfile)) continue;
                if (Quests_DataTypes.SyncedQuestsEvents.Value.ContainsKey(splitProfile.GetStableHashCode()))
                {
                    Quests_DataTypes.SyncedQuestsEvents.Value[splitProfile.GetStableHashCode()]
                        .Add(new Quests_DataTypes.QuestEvent(cond, action, args));
                }
                else
                {
                    Quests_DataTypes.SyncedQuestsEvents.Value.Add(splitProfile.GetStableHashCode(),
                        new List<Quests_DataTypes.QuestEvent> { new(cond, action, args) });
                }
            }
        }

        Quests_DataTypes.SyncedQuestsEvents.Update();
    }
}
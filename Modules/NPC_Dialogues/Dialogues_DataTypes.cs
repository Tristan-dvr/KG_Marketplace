using Marketplace.ExternalLoads;
using Marketplace.Modules.Global_Options;
using Marketplace.Modules.Leaderboard;
using Marketplace.Modules.NPC;
using Marketplace.Modules.Quests;
using Marketplace.OtherModsAPIs;
using Random = UnityEngine.Random;

namespace Marketplace.Modules.NPC_Dialogues;

public static class Dialogues_DataTypes
{
    internal static readonly CustomSyncedValue<List<RawDialogue>> SyncedDialoguesData =
        new(Marketplace.configSync, "dialoguesData", new List<RawDialogue>());

    internal static readonly Dictionary<string, Dialogue> ClientReadyDialogues = new();

    public delegate bool Dialogue_Condition(out string reason);

    private enum OptionCommand
    {
        OpenUI,
        PlaySound,
        GiveQuest,
        GiveItem,
        RemoveItem,
        Spawn,
        SpawnXYZ,
        Teleport,
        RemoveQuest,
        Damage,
        Heal,
        GiveBuff,
        FinishQuest,
        PingMap,
        AddPin,
        AddEpicMMOExp,
        AddCozyheimExp,
        PlayAnimation,
        AddCustomValue,
        SetCustomValue,
    }

    private const byte reverseFlag = 1 << 7;

    private static OptionCondition Reverse(this OptionCondition condition) =>
        (OptionCondition)((byte)condition ^ reverseFlag);

    private enum OptionCondition : byte
    {
        HasItem = 1,
        NotHasItem = 1 | reverseFlag,
        HasBuff = 2,
        NotHasBuff = 2 | reverseFlag,
        SkillMore = 3,
        SkillLess = 3 | reverseFlag,
        GlobalKey = 4,
        NotGlobalKey = 4 | reverseFlag,
        IsVIP = 5,
        NotIsVIP = 5 | reverseFlag,
        HasQuest = 6,
        NotHasQuest = 6 | reverseFlag,
        QuestProgressDone = 7,
        QuestProgressNotDone = 7 | reverseFlag,
        QuestFinished = 8,
        QuestNotFinished = 8 | reverseFlag,
        EpicMMOLevelMore = 9,
        EpicMMOLevelLess = 9 | reverseFlag,
        CozyheimLevelMore = 10,
        CozyheimLevelLess = 10 | reverseFlag,
        HasAchievement = 11,
        NotHasAchievement = 11 | reverseFlag,
        HasAchievementScore = 12,
        NotHasAchievementScore = 12 | reverseFlag,
        CustomValueMore = 13,
        CustomValueLess = 13 | reverseFlag,
    }

    public class RawDialogue : ISerializableParameter
    {
        public string UID;
        public string Text;
        public string BG_ImageLink;
        public RawPlayerOption[] Options = Array.Empty<RawPlayerOption>();

        public class RawPlayerOption
        {
            public string Text;
            public string Icon;
            public string NextUID;
            public string[] Commands = Array.Empty<string>();
            public string[] Conditions = Array.Empty<string>();
            public bool AlwaysVisible = true;
            public Color Color = Color.white;
        }

        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write(UID ?? "default");
            pkg.Write(Text ?? "");
            pkg.Write(BG_ImageLink ?? "");
            pkg.Write(Options.Length);
            foreach (RawPlayerOption option in Options)
            {
                pkg.Write(option.Text ?? "");
                pkg.Write(option.Icon ?? "");
                pkg.Write(option.NextUID ?? "");
                pkg.Write(option.Commands.Length);
                foreach (string command in option.Commands)
                {
                    pkg.Write(command);
                }

                pkg.Write(option.Conditions.Length);
                foreach (string condition in option.Conditions)
                {
                    pkg.Write(condition);
                }

                pkg.Write(option.AlwaysVisible);
                pkg.Write(global::Utils.ColorToVec3(option.Color));
            }
        }

        public void Deserialize(ref ZPackage pkg)
        {
            UID = pkg.ReadString();
            Text = pkg.ReadString();
            BG_ImageLink = pkg.ReadString();
            int optionsLength = pkg.ReadInt();
            Options = new RawPlayerOption[optionsLength];
            for (int i = 0; i < optionsLength; i++)
            {
                Options[i] = new RawPlayerOption
                {
                    Text = pkg.ReadString(),
                    Icon = pkg.ReadString(),
                    NextUID = pkg.ReadString(),
                };
                int commandsLength = pkg.ReadInt();
                Options[i].Commands = new string[commandsLength];
                for (int j = 0; j < commandsLength; j++)
                {
                    Options[i].Commands[j] = pkg.ReadString();
                }

                int conditionsLength = pkg.ReadInt();
                Options[i].Conditions = new string[conditionsLength];
                for (int j = 0; j < conditionsLength; j++)
                {
                    Options[i].Conditions[j] = pkg.ReadString();
                }

                Options[i].AlwaysVisible = pkg.ReadBool();
                Options[i].Color = global::Utils.Vec3ToColor(pkg.ReadVector3());
            }
        }
    }

    public class Dialogue
    {
        public string Text;
        public Sprite BG_Image = null!;
        public PlayerOption[] Options = Array.Empty<PlayerOption>();

        public class PlayerOption
        {
            public string Text;
            public Sprite Icon = null!;
            public string NextUID;
            public Action<Market_NPC.NPCcomponent> Command;
            public Dialogue_Condition Condition;
            public bool AlwaysVisible;
            public Color Color = Color.white;

            public bool CheckCondition(out string reason)
            {
                reason = "";
                if (Condition == null) return true;
                foreach (Dialogue_Condition cast in Condition.GetInvocationList().Cast<Dialogue_Condition>())
                {
                    if (!cast(out reason)) return false;
                }

                return true;
            }
        }

        private static Action<Market_NPC.NPCcomponent> TryParseCommands(IEnumerable<string> commands)
        {
            Action<Market_NPC.NPCcomponent> result = null;
            foreach (string command in commands)
            {
                try
                {
                    string[] split = command.Split(',');
                    if (Enum.TryParse(split[0], true, out OptionCommand optionCommand))
                    {
                        switch (optionCommand)
                        {
                            case OptionCommand.SetCustomValue:
                                result += (_) =>
                                {
                                    string key = split[1];
                                    int value = int.Parse(split[2]);
                                    if (value != 0)
                                        Player.m_localPlayer.SetCustomValue(key, value);
                                    else
                                        Player.m_localPlayer.RemoveCustomValue(key);
                                };
                                break;
                            case OptionCommand.AddCustomValue:
                                result += (_) =>
                                {
                                    string key = split[1];
                                    int value = int.Parse(split[2]);
                                    Player.m_localPlayer.AddCustomValue(key, value);
                                };
                                break;
                            case OptionCommand.PlayAnimation:
                                result += (npc) => { npc.zanim.SetTrigger(split[1]); };
                                break;
                            case OptionCommand.OpenUI:
                                result += (npc) => npc.OpenUIForType(
                                    split.Length > 1 ? split[1] : null, split.Length > 2 ? split[2] : null);
                                break;
                            case OptionCommand.PlaySound:
                                result += (npc) =>
                                {
                                    if (AssetStorage.NPC_AudioClips.TryGetValue(split[1],
                                            out AudioClip clip))
                                    {
                                        npc.NPC_SoundSource.Stop();
                                        npc.NPC_SoundSource.clip = clip;
                                        npc.NPC_SoundSource.Play();
                                    }
                                };
                                break;
                            case OptionCommand.GiveQuest:
                                result += (_) =>
                                {
                                    string questName = split[1].ToLower();
                                    Quests_DataTypes.Quest.AcceptQuest(questName.GetStableHashCode(),
                                        handleEvent: false);
                                    Quests_UIs.AcceptedQuestsUI.CheckQuests();
                                };
                                break;
                            case OptionCommand.GiveItem:
                                result += (_) =>
                                {
                                    string itemPrefab = split[1];
                                    GameObject prefab = ZNetScene.instance.GetPrefab(itemPrefab);
                                    if (!prefab || !prefab.GetComponent<ItemDrop>()) return;
                                    int amount = int.Parse(split[2]);
                                    int level = int.Parse(split[3]);
                                    Utils.InstantiateItem(prefab, amount, level);
                                };
                                break;
                            case OptionCommand.RemoveItem:
                                result += (_) => { Utils.CustomRemoveItems(split[1], int.Parse(split[2]), 1); };
                                break;
                            case OptionCommand.Spawn:
                                result += (_) =>
                                {
                                    string spawnPrefab = split[1];
                                    GameObject spawn = ZNetScene.instance.GetPrefab(spawnPrefab);
                                    if (!spawn || !spawn.GetComponent<Character>()) return;
                                    Vector3 spawnPos = Player.m_localPlayer.transform.position;
                                    int spawnAmount = int.Parse(split[2]);
                                    int spawnLevel = Mathf.Max(1, int.Parse(split[3]) + 1);
                                    for (int i = 0; i < spawnAmount; i++)
                                    {
                                        float randomX = Random.Range(-15, 15);
                                        float randomZ = Random.Range(-15, 15);
                                        Vector3 randomPos = new Vector3(spawnPos.x + randomX, spawnPos.y,
                                            spawnPos.z + randomZ);
                                        Utils.CustomFindFloor(randomPos, out randomPos.y, 3f);
                                        GameObject newSpawn =
                                            UnityEngine.Object.Instantiate(spawn, randomPos, Quaternion.identity);
                                        newSpawn.GetComponent<Character>().SetLevel(spawnLevel);
                                    }
                                };
                                break;
                            case OptionCommand.SpawnXYZ:
                                result += (_) =>
                                {
                                    string spawnPrefab = split[1];
                                    GameObject spawn = ZNetScene.instance.GetPrefab(spawnPrefab);
                                    if (!spawn || !spawn.GetComponent<Character>()) return;
                                    int spawnAmount = int.Parse(split[2]);
                                    int spawnLevel = Mathf.Max(1, int.Parse(split[3]) + 1);
                                    Vector3 spawnPoint = new Vector3(int.Parse(split[4]), int.Parse(split[5]),
                                        int.Parse(split[6]));
                                    int maxDistance = int.Parse(split[7]);
                                    for (int i = 0; i < spawnAmount; i++)
                                    {
                                        float randomX = Random.Range(-maxDistance, maxDistance);
                                        float randomZ = Random.Range(-maxDistance, maxDistance);
                                        Vector3 randomPos = new Vector3(spawnPoint.x + randomX, spawnPoint.y,
                                            spawnPoint.z + randomZ);
                                        GameObject newSpawn =
                                            UnityEngine.Object.Instantiate(spawn, randomPos, Quaternion.identity);
                                        newSpawn.GetComponent<Character>().SetLevel(spawnLevel);
                                    }
                                };
                                break;
                            case OptionCommand.Teleport:
                                result += (_) =>
                                {
                                    int x = int.Parse(split[1]);
                                    int y = int.Parse(split[2]);
                                    int z = int.Parse(split[3]);
                                    Player.m_localPlayer.TeleportTo(new Vector3(x, y, z),
                                        Player.m_localPlayer.transform.rotation,
                                        true);
                                };
                                break;
                            case OptionCommand.Damage:
                                result += (_) =>
                                {
                                    int damage = int.Parse(split[1]);
                                    HitData hitData = new HitData();
                                    hitData.m_damage.m_damage = damage;
                                    Player.m_localPlayer.Damage(hitData);
                                };
                                break;
                            case OptionCommand.Heal:
                                result += (_) =>
                                {
                                    int heal = int.Parse(split[1]);
                                    Player.m_localPlayer.Heal(heal);
                                };
                                break;
                            case OptionCommand.RemoveQuest:
                                result += (_) =>
                                {
                                    string removeQuestName = split[1].ToLower();
                                    Quests_DataTypes.Quest.RemoveQuestFailed(removeQuestName.GetStableHashCode(),
                                        false);
                                    Quests_UIs.AcceptedQuestsUI.CheckQuests();
                                };
                                break;
                            case OptionCommand.GiveBuff:
                                result += (_) =>
                                {
                                    Player.m_localPlayer.GetSEMan()
                                        .RemoveStatusEffect(split[1].GetStableHashCode(), true);
                                    StatusEffect se = Player.m_localPlayer.GetSEMan()
                                        .AddStatusEffect(split[1].GetStableHashCode(), true);
                                    if (se && split.Length > 2)
                                        se.m_ttl = Mathf.Min(1, float.Parse(split[2]));
                                };
                                break;
                            case OptionCommand.FinishQuest:
                                result += (_) =>
                                {
                                    int reqID = split[1].ToLower().GetStableHashCode();
                                    if (!Quests_DataTypes.AllQuests.ContainsKey(reqID)) return;
                                    Quests_DataTypes.Quest.RemoveQuestComplete(reqID);
                                    Quests_UIs.AcceptedQuestsUI.CheckQuests();
                                };
                                break;
                            case OptionCommand.PingMap:
                                result += (_) =>
                                {
                                    Vector3 pos = new Vector3(int.Parse(split[2]), int.Parse(split[3]),
                                        int.Parse(split[4]));
                                    long sender = Random.Range(-10000, 10000);
                                    Chat.instance.AddInworldText(null, sender, pos, Talker.Type.Ping,
                                        new() { Name = "", Gamertag = "", NetworkUserId = "" }, "");
                                    Chat.WorldTextInstance worldText = Chat.instance.FindExistingWorldText(sender);
                                    if (worldText != null)
                                        worldText.m_text = split[1];
                                    Minimap.instance.ShowPointOnMap(pos);
                                    Minimap.instance.m_largeZoom = 0.1f;
                                };
                                break;
                            case OptionCommand.AddPin:
                                result += (_) =>
                                {
                                    Vector3 pos = new Vector3(int.Parse(split[2]), int.Parse(split[3]),
                                        int.Parse(split[4]));
                                    Minimap.instance.AddPin(pos, NPC_MapPins.PINTYPENPC, split[1], true, false);
                                    Minimap.instance.ShowPointOnMap(pos);
                                    Minimap.instance.m_largeZoom = 0.1f;
                                };
                                break;
                            case OptionCommand.AddEpicMMOExp:
                                result += (_) =>
                                {
                                    int exp = int.Parse(split[1]);
                                    EpicMMOSystem_API.AddExp(exp);
                                };
                                break;
                            case OptionCommand.AddCozyheimExp:
                                result += (_) =>
                                {
                                    int exp = int.Parse(split[1]);
                                    Cozyheim_LevelingSystem.AddExp(exp);
                                };
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utils.print($"Error while parsing dialogue command ({command}):\n{ex}");
                }
            }

            return result;
        }


        private static Dialogue_Condition TryParseConditions(IEnumerable<string> conditions)
        {
            Dialogue_Condition result = null;
            foreach (string condition in conditions)
            {
                try
                {
                    string[] split = condition.Split(',');
                    bool reverse = false;
                    if (split[0][0] == '!')
                    {
                        reverse = true;
                        split[0] = split[0].Substring(1);
                    }

                    if (Enum.TryParse(split[0], true, out OptionCondition optionCondition))
                    {
                        if (reverse) optionCondition = optionCondition.Reverse();
                        switch (optionCondition)
                        {
                            case OptionCondition.CustomValueMore:
                                result += (out string reason) =>
                                {
                                    string key = split[1];
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_needcustomvalue")}: <color=#00ff00>{split[1].Replace("_", " ")}</color>";
                                    return Player.m_localPlayer.GetCustomValue(key) >= int.Parse(split[2]);
                                };
                                break;
                            case OptionCondition.CustomValueLess:
                                result += (out string reason) =>
                                {
                                    string key = split[1];
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_dontneedcustomvalue")}: <color=#00ff00>{split[1].Replace("_", " ")}</color>";
                                    return Player.m_localPlayer.GetCustomValue(key) < int.Parse(split[2]);
                                };
                                break;
                            case OptionCondition.HasAchievement:
                                result += (out string reason) =>
                                {
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_needtitle")}: <color=#00ff00>{LeaderBoard_Main_Client.GetAchievementName(split[1])}</color>";
                                    return LeaderBoard_Main_Client.HasAchievement(split[1]);
                                };
                                break;
                            case OptionCondition.NotHasAchievement:
                                result += (out string reason) =>
                                {
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_dontneedtitle")}: <color=#00ff00>{LeaderBoard_Main_Client.GetAchievementName(split[1])}</color>";
                                    return !LeaderBoard_Main_Client.HasAchievement(split[1]);
                                };
                                break;
                            case OptionCondition.HasAchievementScore:
                                result += (out string reason) =>
                                {
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_needtitlescore")}: <color=#00ff00>{split[1]}</color>";
                                    return Leaderboard_DataTypes.SyncedClientLeaderboard.Value.TryGetValue(
                                               Global_Configs._localUserID + "_" +
                                               Game.instance.m_playerProfile.m_playerName,
                                               out Leaderboard_DataTypes.Client_Leaderboard LB) &&
                                           Leaderboard_UI.GetAchievementScore(LB.Achievements) >=
                                           int.Parse(split[1]);
                                };
                                break;
                            case OptionCondition.NotHasAchievementScore:
                                result += (out string reason) =>
                                {
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_dontneedtitlescore")}: <color=#00ff00>{split[1]}</color>";
                                    return Leaderboard_DataTypes.SyncedClientLeaderboard.Value.TryGetValue(
                                               Global_Configs._localUserID + "_" +
                                               Game.instance.m_playerProfile.m_playerName,
                                               out Leaderboard_DataTypes.Client_Leaderboard LB) &&
                                           Leaderboard_UI.GetAchievementScore(LB.Achievements) <
                                           int.Parse(split[1]);
                                };
                                break;
                            case OptionCondition.SkillMore:
                                result += (out string reason) =>
                                {
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_notenoughskilllevel")}: <color=#00ff00>{Utils.LocalizeSkill(split[1])} {split[2]}</color>";
                                    return Utils.GetPlayerSkillLevelCustom(split[1]) >= int.Parse(split[2]);
                                };
                                break;
                            case OptionCondition.SkillLess:
                                result += (out string reason) =>
                                {
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_toomuchskilllevel")}: <color=#00ff00>{Utils.LocalizeSkill(split[1])} {split[2]}</color>";
                                    return Utils.GetPlayerSkillLevelCustom(split[1]) < int.Parse(split[2]);
                                };
                                break;
                            case OptionCondition.QuestFinished:
                                result += (out string reason) =>
                                {
                                    reason = "";
                                    int reqID = split[1].ToLower().GetStableHashCode();
                                    if (!Quests_DataTypes.AllQuests.ContainsKey(reqID)) return true;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_needtofinishquest")}: <color=#00ff00>{Quests_DataTypes.AllQuests[reqID].Name}</color>";
                                    return Quests_DataTypes.Quest.IsOnCooldown(reqID, out _);
                                };
                                break;
                            case OptionCondition.QuestNotFinished:
                                result += (out string reason) =>
                                {
                                    reason = "";
                                    int reqID = split[1].ToLower().GetStableHashCode();

                                    if (Quests_DataTypes.AcceptedQuests.TryGetValue(reqID,
                                            out Quests_DataTypes.Quest quest))
                                    {
                                        reason = $"$mpasn_questtaken: <color=#00ff00>{quest.Name}</color>".Localize();
                                        return false;
                                    }

                                    if (Quests_DataTypes.AllQuests.TryGetValue(reqID,
                                            out Quests_DataTypes.Quest reqQuest))
                                        reason =
                                            $"{Localization.instance.Localize("$mpasn_questfinished")}: <color=#00ff00>{reqQuest.Name}</color>";
                                    return !Quests_DataTypes.Quest.IsOnCooldown(reqID, out _);
                                };
                                break;
                            case OptionCondition.HasItem:
                                result += (out string reason) =>
                                {
                                    reason = "";
                                    GameObject prefab = ZNetScene.instance.GetPrefab(split[1]);
                                    if (!prefab || !prefab.GetComponent<ItemDrop>()) return true;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_needhasitem")}: <color=#00ff00>{Localization.instance.Localize(prefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_name)} x{split[2]}</color>";
                                    return Utils.CustomCountItemsNoLevel(split[1]) >= int.Parse(split[2]);
                                };
                                break;
                            case OptionCondition.NotHasItem:
                                result += (out string reason) =>
                                {
                                    reason = "";
                                    GameObject prefab = ZNetScene.instance.GetPrefab(split[1]);
                                    if (!prefab || !prefab.GetComponent<ItemDrop>()) return true;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_neednothasitem")}: <color=#00ff00>{Localization.instance.Localize(prefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_name)} x{split[2]}</color>";
                                    return Utils.CustomCountItemsNoLevel(split[1]) < int.Parse(split[2]);
                                };
                                break;
                            case OptionCondition.IsVIP:
                                result += (out string reason) =>
                                {
                                    reason = $"{Localization.instance.Localize("$mpasn_onlyforvip")}";
                                    return Global_Configs.SyncedGlobalOptions.Value._vipPlayerList.Contains(Global_Configs
                                        ._localUserID);
                                };
                                break;
                            case OptionCondition.NotIsVIP:
                                result += (out string reason) =>
                                {
                                    reason = $"{Localization.instance.Localize("$mpasn_notforvip")}";
                                    return !Global_Configs.SyncedGlobalOptions.Value._vipPlayerList.Contains(
                                        Global_Configs
                                            ._localUserID);
                                };
                                break;
                            case OptionCondition.GlobalKey:
                                result += (out string reason) =>
                                {
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_needglobalkey")}: <color=#00ff00>{split[1]}</color>";
                                    return ZoneSystem.instance.GetGlobalKey(split[1]);
                                };
                                break;
                            case OptionCondition.NotGlobalKey:
                                result += (out string reason) =>
                                {
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_notneedglobalkey")}: <color=#00ff00>{split[1]}</color>";
                                    return !ZoneSystem.instance.GetGlobalKey(split[1]);
                                };
                                break;
                            case OptionCondition.HasBuff:
                                result += (out string reason) =>
                                {
                                    StatusEffect findSe = ObjectDB.instance.m_StatusEffects.FirstOrDefault(s => s.name == split[1])!;
                                    string seName = findSe == null
                                        ? split[1]
                                        : Localization.instance.Localize(findSe.m_name);
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_needhasbuff")}: <color=#00ff00>{seName}</color>";
                                    return Player.m_localPlayer.m_seman.HaveStatusEffect(split[1]);
                                };
                                break;
                            case OptionCondition.NotHasBuff:
                                result += (out string reason) =>
                                {
                                    StatusEffect findSe = ObjectDB.instance.m_StatusEffects.FirstOrDefault(s => s.name == split[1])!;
                                    string seName = findSe == null
                                        ? split[1]
                                        : Localization.instance.Localize(findSe.m_name);
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_notneedhasbuff")}: <color=#00ff00>{seName}</color>";
                                    return !Player.m_localPlayer.m_seman.HaveStatusEffect(split[1]);
                                };
                                break;
                            case OptionCondition.QuestProgressDone:
                                result += (out string reason) =>
                                {
                                    reason = "";
                                    int reqID = split[1].ToLower().GetStableHashCode();
                                    if (!Quests_DataTypes.AllQuests.TryGetValue(reqID,
                                            out Quests_DataTypes.Quest reqQuest) ||
                                        !Quests_DataTypes.AcceptedQuests.ContainsKey(reqID))
                                        return false;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_queststillnotfinished")}: <color=#00ff00>{reqQuest.Name}</color>";
                                    return Quests_DataTypes.AllQuests[reqID].IsComplete();
                                };
                                break;
                            case OptionCondition.QuestProgressNotDone:
                                result += (out string reason) =>
                                {
                                    reason = "";
                                    int reqID = split[1].ToLower().GetStableHashCode();
                                    if (!Quests_DataTypes.AllQuests.TryGetValue(reqID,
                                            out Quests_DataTypes.Quest reqQuest) ||
                                        !Quests_DataTypes.AcceptedQuests.ContainsKey(reqID))
                                        return false;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_questalreadyfinished")}: <color=#00ff00>{reqQuest.Name}</color>";
                                    return !Quests_DataTypes.AllQuests[reqID].IsComplete();
                                };
                                break;
                            case OptionCondition.HasQuest:
                                result += (out string reason) =>
                                {
                                    reason = "";
                                    int reqID = split[1].ToLower().GetStableHashCode();
                                    if (!Quests_DataTypes.AllQuests.TryGetValue(reqID,
                                            out Quests_DataTypes.Quest reqQuest))
                                        return false;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_questnottaken")}: <color=#00ff00>{reqQuest.Name}</color>";
                                    return Quests_DataTypes.AcceptedQuests.ContainsKey(reqID);
                                };
                                break;
                            case OptionCondition.NotHasQuest:
                                result += (out string reason) =>
                                {
                                    reason = "";
                                    int reqID = split[1].ToLower().GetStableHashCode();
                                    if (!Quests_DataTypes.AllQuests.TryGetValue(reqID,
                                            out Quests_DataTypes.Quest reqQuest))
                                        return false;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_questtaken")}: <color=#00ff00>{reqQuest.Name}</color>";
                                    return !Quests_DataTypes.AcceptedQuests.ContainsKey(reqID);
                                };
                                break;
                            case OptionCondition.EpicMMOLevelMore:
                                result += (out string reason) =>
                                {
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_needepicmmolevel")}: <color=#00ff00>{split[1]}</color>";
                                    return EpicMMOSystem_API.GetLevel() >= int.Parse(split[1]);
                                };
                                break;
                            case OptionCondition.EpicMMOLevelLess:
                                result += (out string reason) =>
                                {
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_toomuchepicmmolevel")}: <color=#00ff00>{split[1]}</color>";
                                    return EpicMMOSystem_API.GetLevel() < int.Parse(split[1]);
                                };
                                break;
                            case OptionCondition.CozyheimLevelMore:
                                result += (out string reason) =>
                                {
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_needcozyheimlevel")}: <color=#00ff00>{split[1]}</color>";
                                    return Cozyheim_LevelingSystem.GetLevel() >= int.Parse(split[1]);
                                };
                                break;
                            case OptionCondition.CozyheimLevelLess:
                                result += (out string reason) =>
                                {
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_toomuchcozyheimlevel")}: <color=#00ff00>{split[1]}</color>";
                                    return Cozyheim_LevelingSystem.GetLevel() < int.Parse(split[1]);
                                };
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utils.print($"Error while parsing dialogue condition ({condition}):\n{ex}");
                }
            }

            return result;
        }

        public static implicit operator Dialogue(RawDialogue raw)
        {
            Dialogue dialogue = new()
            {
                Text = raw.Text,
                Options = new PlayerOption[raw.Options.Length]
            };
            Utils.LoadImageFromWEB(raw.BG_ImageLink!, (sprite) => dialogue.BG_Image = sprite);
            for (int i = 0; i < raw.Options.Length; ++i)
            {
                dialogue.Options[i] = new PlayerOption
                {
                    Text = raw.Options[i].Text,
                    Icon = Utils.TryFindIcon(raw.Options[i].Icon!),
                    NextUID = raw.Options[i].NextUID,
                    Command = TryParseCommands(raw.Options[i].Commands),
                    Condition = TryParseConditions(raw.Options[i].Conditions),
                    AlwaysVisible = raw.Options[i].AlwaysVisible,
                    Color = raw.Options[i].Color
                };
            }

            return dialogue;
        }
    }
}
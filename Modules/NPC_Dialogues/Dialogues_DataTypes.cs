using Marketplace.Modules.NPC;
using Marketplace.Modules.Quests;

namespace Marketplace.Modules.NPC_Dialogues;

public static class Dialogues_DataTypes
{
    internal static readonly CustomSyncedValue<List<RawDialogue>> SyncedDialoguesData =
        new(Marketplace.configSync, "dialoguesData", new List<RawDialogue>());

    internal static readonly Dictionary<string, Dialogue> ClientReadyDialogues = new();

    public delegate bool Dialogue_Condition<T>(out T reason);

    private enum OptionCommand
    {
        OpenUI,
        PlaySound,
        GiveQuest,
        GiveItem,
        RemoveItem,
        Spawn,
        Teleport,
        RemoveQuest,
        Damage,
        Heal,
        GiveBuff
    }

    private enum OptionCondition
    {
        NotFinished,
        OtherQuest,
        HasItem,
        HasBuff,
        Skill,
        GlobalKey,
        IsVIP,
    }

    public class RawDialogue : ISerializableParameter
    {
        public string UID;
        public string Text;
        public RawPlayerOption[] Options = Array.Empty<RawPlayerOption>();

        public class RawPlayerOption
        {
            public string Text;
            public string Icon;
            public string NextUID;
            public string[] Commands;
            public string[] Conditions;
            public bool AlwaysVisible = true;
        }

        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write(UID ?? "default");
            pkg.Write(Text ?? "");
            pkg.Write(Options.Length);
            foreach (var option in Options)
            {
                pkg.Write(option.Text ?? "");
                pkg.Write(option.Icon ?? "");
                pkg.Write(option.NextUID ?? "");
                pkg.Write(option.Commands.Length);
                foreach (string command in option.Commands)
                {
                    pkg.Write(command ?? "");
                }

                pkg.Write(option.Conditions.Length);
                foreach (string condition in option.Conditions)
                {
                    pkg.Write(condition ?? "");
                }

                pkg.Write(option.AlwaysVisible);
            }
        }

        public void Deserialize(ref ZPackage pkg)
        {
            UID = pkg.ReadString();
            Text = pkg.ReadString();
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
            }
        }
    }

    public class Dialogue
    {
        public string Text;
        public PlayerOption[] Options = Array.Empty<PlayerOption>();

        public class PlayerOption
        {
            public string Text;
            public Sprite Icon;
            public string NextUID;
            public Action<Market_NPC.NPCcomponent> Command;
            public Dialogue_Condition<string> Condition;
            public bool AlwaysVisible;

            public bool CheckCondition(out string reason)
            {
                reason = "";
                if (Condition == null) return true;
                foreach (var cast in Condition.GetInvocationList().Cast<Dialogue_Condition<string>>())
                {
                    if (!cast(out reason)) return false;
                }

                return true;
            }
        }

        private static Action<Market_NPC.NPCcomponent> TryParseCommand(IEnumerable<string> commands)
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
                            case OptionCommand.OpenUI:
                                result += (npc) => npc.OpenUIForType(
                                    split.Length > 1 ? split[1] : null,
                                    split.Length > 2 ? split[2] : null);
                                break;
                            case OptionCommand.PlaySound:
                                result += (npc) =>
                                {
                                    if (AssetStorage.AssetStorage.NPC_AudioClips.TryGetValue(split[1],
                                            out AudioClip clip))
                                    {
                                        npc.NPC_SoundSource.PlayOneShot(clip);
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
                                result += (_) =>
                                {
                                    Utils.CustomRemoveItems(split[1], int.Parse(split[2]), 1);
                                };
                                break;
                            case OptionCommand.Spawn:
                                result += (_) =>
                                {
                                    string spawnPrefab = split[1];
                                    Vector3 spawnPos = Player.m_localPlayer.transform.position;
                                    GameObject spawn = ZNetScene.instance.GetPrefab(spawnPrefab);
                                    if (!spawn || !spawn.GetComponent<Character>()) return;
                                    int spawnAmount = int.Parse(split[2]);
                                    int spawnLevel = Mathf.Max(1, int.Parse(split[3]) + 1);
                                    for (int i = 0; i < spawnAmount; i++)
                                    {
                                        float randomX = UnityEngine.Random.Range(-15, 15);
                                        float randomZ = UnityEngine.Random.Range(-15, 15);
                                        Vector3 randomPos = new Vector3(spawnPos.x + randomX, spawnPos.y,
                                            spawnPos.z + randomZ);
                                        float height = ZoneSystem.instance.GetSolidHeight(randomPos);
                                        randomPos.y = height;
                                        GameObject newSpawn = UnityEngine.Object.Instantiate(spawn, randomPos,
                                            Quaternion.identity);
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
                                result += (_) => { Player.m_localPlayer.GetSEMan().AddStatusEffect(split[1], true); };
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


        private static Dialogue_Condition<string> TryParseCondition(IEnumerable<string> conditions)
        {
            Dialogue_Condition<string> result = null;
            foreach (string condition in conditions)
            {
                try
                {
                    string[] split = condition.Split(',');
                    if (Enum.TryParse(split[0], true, out OptionCondition optionCondition))
                    {
                        switch (optionCondition)
                        {
                            case OptionCondition.Skill:
                                result += (out string reason) =>
                                {
                                    string localizedSkill = Enum.TryParse(split[1], out Skills.SkillType _)
                                        ? Localization.instance.Localize("$skill_" + split[1].ToLower())
                                        : Localization.instance.Localize($"$skill_" +
                                                                         Mathf.Abs(split[1].GetStableHashCode()));
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_notenoughskilllevel")}: <color=#00ff00>{localizedSkill} {split[2]}</color>";
                                    return Utils.GetPlayerSkillLevelCustom(split[1]) >= int.Parse(split[2]);
                                };
                                break;
                            case OptionCondition.OtherQuest:
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
                            case OptionCondition.NotFinished:
                                result += (out string reason) =>
                                {
                                    reason = "";
                                    int reqID = split[1].ToLower().GetStableHashCode();
                                    if (Quests_DataTypes.AcceptedQuests.TryGetValue(reqID, out var quest))
                                    {
                                        reason =
                                            $"{Localization.instance.Localize("$mpasn_questtaken")}: <color=#00ff00>{quest.Name}</color>";
                                        return false;
                                    }

                                    if (Quests_DataTypes.AllQuests.TryGetValue(reqID, out var reqQuest))
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
                            case OptionCondition.IsVIP:
                                result += (out string reason) =>
                                {
                                    reason = $"{Localization.instance.Localize("$mpasn_onlyforvip")}";
                                    return Global_Values._container.Value._vipPlayerList.Contains(Global_Values
                                        ._localUserID);
                                };
                                break;
                            case OptionCondition.GlobalKey:
                                result += (out string reason) =>
                                {
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_needglobalkey")}: <color=#00ff00>{split[1]}</color>";
                                    return ZoneSystem.instance.m_globalKeys.Contains(split[1]);
                                };
                                break;
                            case OptionCondition.HasBuff:
                                result += (out string reason) =>
                                {
                                    StatusEffect findSe = ObjectDB.instance.m_StatusEffects.FirstOrDefault(s => s.name == split[1]);
                                    string seName = findSe == null ? split[1] : Localization.instance.Localize(findSe.m_name);
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_needhasbuff")}: <color=#00ff00>{seName}</color>";
                                    return Player.m_localPlayer.m_seman.HaveStatusEffect(split[1]);
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
            for (int i = 0; i < raw.Options.Length; ++i)
            {
                dialogue.Options[i] = new PlayerOption
                {
                    Text = raw.Options[i].Text,
                    Icon = Utils.TryFindIcon(raw.Options[i].Icon),
                    NextUID = raw.Options[i].NextUID,
                    Command = TryParseCommand(raw.Options[i].Commands),
                    Condition = TryParseCondition(raw.Options[i].Conditions),
                    AlwaysVisible = raw.Options[i].AlwaysVisible
                };
            }

            return dialogue;
        }
    }
}
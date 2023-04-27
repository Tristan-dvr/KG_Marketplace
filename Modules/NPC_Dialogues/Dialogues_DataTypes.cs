using Marketplace.Modules.NPC;
using Marketplace.Modules.Quests;

namespace Marketplace.Modules.NPC_Dialogues;

public static class Dialogues_DataTypes
{
    internal static readonly CustomSyncedValue<List<RawDialogue>> SyncedDialoguesData =
        new(Marketplace.configSync, "dialoguesData", new());

    internal static readonly Dictionary<string, Dialogue> ClientReadyDialogues = new();

    private enum OptionCommand
    {
        OpenUI,
        PlaySound,
        GiveQuest,
        GiveItem,
        Spawn,
        Teleport,
        RemoveQuest,
        Damage,
        Heal,
        GiveBuff
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
                foreach (var command in option.Commands)
                {
                    pkg.Write(command ?? "");
                }
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
                                result += (npc) => npc.OpenUIForType(split[1]);
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
                                        float randomX = UnityEngine.Random.Range(-15f, 15f);
                                        float randomZ = UnityEngine.Random.Range(-15f, 15f);
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
                    Command = TryParseCommand(raw.Options[i].Commands)
                };
            }

            return dialogue;
        }
    }
}
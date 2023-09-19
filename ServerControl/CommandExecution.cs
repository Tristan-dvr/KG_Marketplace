using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace kg.ServerControl;

public static class CommandExecution
{
    private static readonly Dictionary<string, Dictionary<string, string>> LOCALIZATION = new()
    {
        {
            "English", new()
            {
                { "ERROR", "ERROR" },
                { "Position", "Position" },
                { "Health", "Health" },
                { "Players", "Players" },
                { "Banlist", "Banlist" },
                { "CantFind", "Can't find player {0}" },
                { "CantGetPrefab", "Can't get prefab {0}" },
                { "Banned", "{0} Banned" },
                { "Unbanned", "{0} Unbanned" },
                { "Kicked", "{0} Kicked" },
                { "Killed", "{0} Killed" },
                { "MessageSent", "Message Sent" },
                { "Spawned", "Spawned {0} {1} in {2}" },
                { "Gave", "Gave {0} {1} to {2}" },
                { "Damage", "Dealt {0} damage to {1}" },
                { "Heal", "Healed {0} HP for {1}" },
                { "BadCommand", "Bad Command" },
                { "ErrorPassword", "Wrong Password" },
                { "Whitelist", "Whitelist" },
                { "AddWhitelist", "{0} Added in Whitelist" },
                { "RemoveWhitelist", "{0} Removed from Whitelist" },
                { "PlayerTeleported", "{0} Teleported to ({1}, {2}, {3})" }
            }
        },

        {
            "Russian", new()
            {
                { "ERROR", "ОШИБКА" },
                { "Position", "Позиция" },
                { "Health", "ХП" },
                { "Players", "Игроки" },
                { "Banlist", "Банлист" },
                { "CantFind", "Игрок {0} не найден" },
                { "CantGetPrefab", "Префаб {0} не найден" },
                { "Banned", "{0} Забанен" },
                { "Unbanned", "{0} Разбанен" },
                { "Kicked", "{0} Кикнут" },
                { "Killed", "{0} Убит" },
                { "MessageSent", "Сообщение Отправленно" },
                { "Spawned", "Заспавнено {0} {1} в {2}" },
                { "Gave", "{2} получил {0} {1}" },
                { "Damage", "Нанесено {0} урона {1}" },
                { "Heal", "{1} исцелён на {0} ХП" },
                { "BadCommand", "Bad Command" },
                { "ErrorPassword", "Неверный Пароль" },
                { "Whitelist", "Whitelist" },
                { "AddWhitelist", "{0} Добавлен в Whitelist" },
                { "RemoveWhitelist", "{0} Удалён из Whitelist'а" },
                { "PlayerTeleported", "{0} Телепортирован в ({1}, {2}, {3})" }
            }
        }
    };

    public enum Command
    {
        ShowPlayers = 1,
        Kick = 2,
        Ban = 3,
        Unban = 4,
        BanList = 5,
        BanSteamID = 6,
        Give = 7,
        Spawn = 8,
        Position = 9,
        Say = 10,
        Damage = 11,
        Heal = 12,
        SayChat = 13,
        ScanObject = 14,
        RemoveObjects = 15,
        Whitelist = 16,
        AddWhitelist = 17,
        RemoveWhitelist = 18,
        ShowGlobalKeys = 35,
        AddGlobalKey = 36,
        RemoveGlobalKey = 37,
        Teleport = 38,
        Adminlist = 39,
        AddAdminlist = 40,
        RemoveAdminlist = 41,
        StartEvent = 42,
        StopEvent = 43,

        RCON = 99,
    }

    private delegate bool Invoke(string[] args, out string result);

    private static readonly Dictionary<Command, Invoke> _commands = new();

    public static void Init(string language = "English")
    {
        Dictionary<string, string> Localize = LOCALIZATION[language];
        _commands.Add(Command.ShowPlayers, (string[] _, out string result) =>
        {
            string players = "";
            foreach (ZNetPeer player in ZNet.instance.m_peers)
            {
                ZDO zdo = ZDOMan.instance.GetZDO(player.m_characterID);
                if (zdo == null) continue;
                players +=
                    $"\n{player.m_playerName} ({Localize["Position"]}: {(int)player.m_refPos.x} {(int)player.m_refPos.y} {(int)player.m_refPos.z}  {Localize["Health"]}: {(int)zdo.GetFloat("health")}/{(int)zdo.GetFloat("max_health")}  SteamID: {player.m_socket.GetHostName()} CreatorID: {zdo.GetLong("playerID")})";
            }
            result = $"{Localize["Players"]}:\n" + players;
            return true;
        });

        _commands.Add(Command.Kick, (string[] args, out string result) =>
        {
            ZNetPeer peer = ZNet.instance.GetPeerByPlayerName(args[0]);
            if (peer != null)
            {
                ZNet.instance.ClearPlayerData(peer);
                ZNet.instance.m_peers.Remove(peer);
                peer.Dispose();
                result = string.Format(Localize["Kicked"], args[0]);
                return true;
            }
            result = string.Format(Localize["CantFind"], args[0]);
            return false;
        });

        _commands.Add(Command.Ban, (string[] args, out string result) =>
        {
            ZNetPeer peer = ZNet.instance.GetPeerByPlayerName(args[0]);
            if (peer != null)
            {
                ZNet.instance.Ban(args[0]);
                result = string.Format(Localize["Banned"], args[0]);
                return true;
            }
            result = string.Format(Localize["CantFind"], args[0]);
            return false;
        });

        _commands.Add(Command.Unban, (string[] args, out string result) =>
        {
            ZNet.instance.m_bannedList.Remove(args[0]);
            result = string.Format(Localize["Unbanned"], args[0]);
            return true;
        });

        _commands.Add(Command.BanList, (string[] _, out string result) =>
        {
            string players = "";
            foreach (string id in ZNet.instance.m_bannedList.GetList())
            {
                players += $"\n{id}";
            }
            result = $"{Localize["Banlist"]}:" + players;
            return true;
        });

        _commands.Add(Command.BanSteamID, (string[] args, out string result) =>
        {
            ZNet.instance.m_bannedList.Add(args[0]);
            result = string.Format(Localize["Banned"], args[0]);
            return true;
        });

        _commands.Add(Command.Give, (string[] args, out string result) =>
        {
            ZNetPeer peer = ZNet.instance.GetPeerByPlayerName(args[0]);
            if (peer != null)
            {
                string prefab = args[1];
                int count = int.Parse(args[2]);
                int level = int.Parse(args[3]);
                GameObject go = ObjectDB.instance.GetItemPrefab(prefab);
                if (go && go.GetComponent<ItemDrop>())
                {
                    ZDO zdo = ZDOMan.instance.GetZDO(peer.m_characterID);
                    Vector3 pos = zdo.m_position - zdo.GetRotation() * zdo.m_position.normalized * 2f;
                    CreateZdoFromPrefab("fx_creature_tamed", pos + Vector3.up * 2f, peer.m_uid);
                    int maxStack = go.GetComponent<ItemDrop>().m_itemData.m_shared.m_maxStackSize;

                    if (maxStack > 1)
                    {
                        ZDO itemZdo = CreateZdoFromPrefab(prefab, pos + Vector3.up * 2f);
                        itemZdo.Set("stack", count);
                        itemZdo.Set("quality", level);
                        itemZdo.Set("durability",
                            go.GetComponent<ItemDrop>().m_itemData.GetMaxDurability(level));
                    }
                    else
                    {
                        for (int i = 0; i < count; i++)
                        {
                            float value = UnityEngine.Random.Range(-1f, 1f);
                            float value2 = UnityEngine.Random.Range(-1f, 1f);
                            ZDO itemZdo = CreateZdoFromPrefab(prefab,
                                pos + Vector3.left * value + Vector3.forward * value2 + Vector3.up);
                            itemZdo.Set("stack", 1);
                            itemZdo.Set("quality", level);
                            itemZdo.Set("durability",
                                go.GetComponent<ItemDrop>().m_itemData.GetMaxDurability(level));
                        }
                    }
                    result = string.Format(Localize["Gave"], count, Localization.instance.Localize(go.GetComponent<ItemDrop>().m_itemData.m_shared.m_name), args[0]);
                    return true;
                }
                result = string.Format(Localize["CantGetPrefab"], prefab);
                return false;
            }
            result = string.Format(Localize["CantFind"], args[0]);
            return false;
        });
        
        _commands.Add(Command.Spawn, (string[] args, out string result) =>
        {
            string prefab = args[0];
            Vector3 vec = new Vector3(int.Parse(args[1]), int.Parse(args[2]), int.Parse(args[3]));
            int count = int.Parse(args[4]);
            GameObject go = ZNetScene.instance.GetPrefab(prefab);
            int level = int.Parse(args[5]);
            if (!go)
            {
                result = string.Format(Localize["CantGetPrefab"], prefab);
                return false;
            }
            if (go.GetComponent<ZNetView>() &&
                (go.GetComponent<ItemDrop>() || go.GetComponent<Character>() ||
                 go.GetComponent<Piece>()))
            {
                for (int i = 0; i < count; i++)
                {
                    float value = UnityEngine.Random.Range(-3f, 3f);
                    float value2 = UnityEngine.Random.Range(-3f, 3f);

                    ZDO zdo = CreateZdoFromPrefab(prefab,
                        vec + Vector3.left * value + Vector3.forward * value2 + Vector3.up);

                    if (go.GetComponent<ItemDrop>())
                    {
                        zdo.Set("quality", level);
                    }

                    if (go.GetComponent<Character>())
                    {
                        zdo.Set("level", level);
                    }
                }

                result = string.Format(Localize["Spawned"], count, prefab, vec);
                return true;
            }
            result = Localize["ERROR"];
            return false;
        });
        
        _commands.Add(Command.Position, (string[] args, out string result) =>
        {
            ZNetPeer peer = ZNet.instance.GetPeerByPlayerName(args[0]);
            if (peer != null)
            {
                Vector3 pos = peer.m_refPos;
                result = $"{args[0]} {Localize["Position"]}: {(int)pos.x} {(int)pos.y} {(int)pos.z}";
                return true;
            }
            result = string.Format(Localize["CantFind"], args[0]);
            return false;
        });
        
        _commands.Add(Command.Say, (string[] args, out string result) =>
        {
            MessageHud.instance.MessageAll(MessageHud.MessageType.Center, args[0]);
            result = Localize["MessageSent"];
            return true;
        });
        
        _commands.Add(Command.Damage, (string[] args, out string result) =>
        {
            ZNetPeer peer = ZNet.instance.GetPeerByPlayerName(args[0]);
            if (peer != null)
            {
                int damage = int.Parse(args[1]);
                HitData hit = new HitData();
                hit.m_damage.m_damage = damage;
                CreateZdoFromPrefab("fx_shaman_fireball_expl", peer.m_refPos, peer.m_uid);
                ZRoutedRpc.instance.InvokeRoutedRPC(peer.m_uid, peer.m_characterID, "Damage", hit);
                result = damage == 999999 ? string.Format(Localize["Killed"], args[0]) : string.Format(Localize["Damage"], damage, args[0]);
                return true;
            }
            result = string.Format(Localize["CantFind"], args[0]);
            return false;
        });
        
        _commands.Add(Command.Heal, (string[] args, out string result) =>
        {
            ZNetPeer peer = ZNet.instance.GetPeerByPlayerName(args[0]);
            if (peer != null)
            {
                float heal = float.Parse(args[1]);
                CreateZdoFromPrefab("fx_creature_tamed", peer.m_refPos, peer.m_uid);
                ZRoutedRpc.instance.InvokeRoutedRPC(peer.m_uid, peer.m_characterID, "Heal", heal, true);
                result = string.Format(Localize["Heal"], heal, args[0]);
                return true;
            }
            result = string.Format(Localize["CantFind"], args[0]);
            return false;
        });
        
        _commands.Add(Command.SayChat, (string[] args, out string result) =>
        {
            UserInfo info = new()
            {
                Name = args[0],
                Gamertag = "",
                NetworkUserId = "Steam_0"
            };
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "ChatMessage", new object[]
            {
                new Vector3(0, 200, 0),
                2,
                info,
                args[1],
                "Steam_0"
            });
            result = Localize["MessageSent"];
            return true;
        });
        
        _commands.Add(Command.ScanObject, (string[] args, out string result) =>
        {
            Dictionary<long, int> counts = new();
            int stableHashCode = args[0].GetStableHashCode();
            foreach (List<ZDO> zdos in ZDOMan.instance.m_objectsBySector)
            {
                if (zdos == null) continue;
                for (int i = 0; i < zdos.Count; ++i)
                {
                    ZDO TestingZDO = zdos[i];
                    if (TestingZDO.GetPrefab() == stableHashCode)
                    {
                        long hash = TestingZDO.GetLong(ZDOVars.s_creator);
                        if (hash == 0) continue;
                        if (!counts.ContainsKey(hash)) counts[hash] = 0;
                        counts[hash]++;
                    }
                }
            }
            string ret = $"{args[0]}'s:\n";
            if (counts.Count == 0)
            {
                result = ret;
                return true;
            }
            foreach (KeyValuePair<long, int> value in counts)
            {
                ret += $"Creator ID: {value.Key} - Quantity: {value.Value}\n";
            }

            result = ret;
            return true;
        });
        
        _commands.Add(Command.RemoveObjects, (string[] args, out string result) =>
        {
            string CallbackRemoveObjects;
            int stableHashCode = args[0].GetStableHashCode();
            long CreatorID = long.Parse(args[1]);
            List<ZDO> tempRemoveList = new();
            foreach (List<ZDO> zdos in ZDOMan.instance.m_objectsBySector)
            {
                if (zdos == null) continue;
                for (int i = 0; i < zdos.Count; ++i)
                {
                    ZDO TestingZDO = zdos[i];
                    if (TestingZDO.GetPrefab() == stableHashCode)
                    {
                        tempRemoveList.Add(TestingZDO);
                    }
                }
            }

            if (CreatorID != 0)
            {
                CallbackRemoveObjects = $"Removing Objects With name {args[0]} for player (creator id) {CreatorID}:\n";
                tempRemoveList = tempRemoveList.Where(zdo => zdo.GetLong(ZDOVars.s_creator) == CreatorID).ToList();
            }
            else
            {
                CallbackRemoveObjects = $"Removing Objects With name {args[0]} for ALL PLAYERS\n";
            }

            tempRemoveList.ForEach(z => ZDOMan.instance.m_destroySendList.Add(z.m_uid));
            CallbackRemoveObjects += $"Removed total {tempRemoveList.Count} objects";
            result = CallbackRemoveObjects;
            return true;
        });
        
        _commands.Add(Command.Whitelist, (string[] _, out string result) =>
        {
            string players = "";
            foreach (string id in ZNet.instance.m_permittedList.GetList())
            {
                players += $"\n{id}";
            }
            result = $"{Localize["Whitelist"]}:" + players;
            return true;
        });
        
        _commands.Add(Command.AddWhitelist, (string[] args, out string result) =>
        {
            ZNet.instance.m_permittedList.Add(args[0]);
            result = string.Format(Localize["AddWhitelist"], args[0]);
            return true;
        });
        
        _commands.Add(Command.RemoveWhitelist, (string[] args, out string result) =>
        {
            ZNet.instance.m_permittedList.Remove(args[0]);
            result = string.Format(Localize["RemoveWhitelist"], args[0]);
            return true;
        });
        
        _commands.Add(Command.ShowGlobalKeys, (string[] _, out string result) =>
        {
            string keys = "";
            foreach (string key in ZoneSystem.instance.GetGlobalKeys())
            {
                keys += $"\n{key}";
            }
            result = $"Global Keys:" + keys;
            return true;
        });
        
        _commands.Add(Command.AddGlobalKey, (string[] args, out string result) =>
        {
            ZoneSystem.instance.GlobalKeyAdd(args[0]);
            result = $"Added {args[0]} to Global Keys";
            return true;
        });
        
        _commands.Add(Command.RemoveGlobalKey, (string[] args, out string result) =>
        {
            ZoneSystem.instance.GlobalKeyRemove(args[0]);
            result = $"Removed {args[0]} from Global Keys";
            return true;
        });
        
        _commands.Add(Command.Teleport, (string[] args, out string result) =>
        {
            ZNetPeer peer = ZNet.instance.GetPeerByPlayerName(args[0]);
            if (peer != null)
            {
                int x = int.Parse(args[1]);
                int y = int.Parse(args[2]);
                int z = int.Parse(args[3]);
                ZRoutedRpc.instance.InvokeRoutedRPC(peer.m_uid, peer.m_characterID, "RPC_TeleportTo", new Vector3(x, y, z), Quaternion.identity, true);
                result = string.Format(Localize["PlayerTeleported"], args[0], args[1], args[2], args[3]);
                return true;
            }
            result = string.Format(Localize["CantFind"], args[0]);
            return false;
        });
        
        _commands.Add(Command.Adminlist, (string[] _, out string result) =>
        {
            string players = "";
            foreach (string id in ZNet.instance.m_adminList.GetList())
            {
                players += $"\n{id}";
            }
            result = $"Adminlist:" + players;
            return true;
        });
        
        _commands.Add(Command.AddAdminlist, (string[] args, out string result) =>
        {
            ZNet.instance.m_adminList.Add(args[0]);
            result = $"Added {args[0]} to Adminlist";
            return true;
        });
        
        _commands.Add(Command.RemoveAdminlist, (string[] args, out string result) =>
        {
            ZNet.instance.m_adminList.Remove(args[0]);
            result = $"Removed {args[0]} from Adminlist";
            return true;
        });
        
        _commands.Add(Command.StartEvent, (string[] args, out string result) =>
        {
            RandomEvent testEvent = RandEventSystem.instance.GetEvent(args[0]);
            if (testEvent == null)
            {
                result = $"Can't find event {args[0]}";
                return false;
            }
            Vector3 EventPos = new Vector3(Convert.ToInt32(args[1]), Convert.ToInt32(args[2]), Convert.ToInt32(args[3]));
            RandEventSystem.instance.SetRandomEventByName(args[0], EventPos);
            result = $"Event {args[0]} started at: {EventPos}";
            return true;
        });
        
        _commands.Add(Command.StopEvent, (string[] _, out string result) =>
        {
            string currentEvent = RandEventSystem.instance.m_activeEvent?.m_name ?? "Unknown";
            RandEventSystem.instance.SetRandomEventByName("stopEvent", Vector3.zero);
            result = $"Latest Event stopped ({currentEvent})";
            return true;
        });
        
        _commands.Add(Command.RCON, (string[] args, out string result) =>
        {
            LatestRCONmessage = "Unknown";
            HookRCON = true;
            Console.instance.TryRunCommand(args[0], false, true);
            HookRCON = false;
            result = $"Command executed. Result:\n{(string.IsNullOrWhiteSpace(LatestRCONmessage) ? "Unknown" : LatestRCONmessage)}";
            return true;
        });
    }

    private static ZDO CreateZdoFromPrefab(string prefab, Vector3 pos, long ID = 0L)
    {
        ZNetView znv = ZNetScene.instance.GetPrefab(prefab)?.GetComponent<ZNetView>();
        if (znv == null) return null;
        ZDO createObject = ZDOMan.instance.CreateNewZDO(pos, prefab.GetStableHashCode());
        createObject.Persistent = znv.m_persistent;
        createObject.Type = znv.m_type;
        createObject.Distant = znv.m_distant;
        createObject.SetPrefab(prefab.GetStableHashCode());
        createObject.SetRotation(Quaternion.identity);
        if (znv.m_syncInitialScale)
        {
            createObject.Set("scale", znv.transform.localScale);
        }

        createObject.SetOwner(ID);
        return createObject;
    }
    private static string LatestRCONmessage = "Unknown";
    private static bool HookRCON;
    [HarmonyPatch(typeof(Terminal), nameof(Terminal.AddString), typeof(string))]
    static class Terminal_AddString_Patch
    {
        static void Prefix(string text)
        {
            if (HookRCON) LatestRCONmessage = text;
        }
    }

    public static bool Execute(Command cmd, string args, out string result)
    {
        if (_commands.TryGetValue(cmd, out Invoke execute))
        {
            try
            {
                ServerControl.print($"Executing command {cmd} with args: \"{args.Replace("&%&", " | ")}\"", ConsoleColor.Cyan);
                return execute(args.Split(new[]{ "&%&"}, StringSplitOptions.None), out result);
            }
            catch (Exception ex)
            {
                result = ex.ToString();
                return false;
            }
        }
        result = "Unknown command";
        return false;
    }
}
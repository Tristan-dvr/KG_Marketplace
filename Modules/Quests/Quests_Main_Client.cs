using BepInEx.Configuration;
using UnityEngine.Networking;

namespace Marketplace.Modules.Quests;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Normal)]
public static class Quests_Main_Client
{
    private static ConfigEntry<KeyCode> QuestJournalOpenKey;
    private static int LatestRevision;

    private static void OnInit()
    {
        QuestJournalOpenKey = Marketplace._thistype.Config.Bind("General", "Quest Journal Keycode", KeyCode.J);
        Quests_UIs.QuestUI.Init();
        Quests_UIs.AcceptedQuestsUI.Init();
        Quests_DataTypes.SyncedQuestData.ValueChanged += OnQuestDataUpdate;
        Quests_DataTypes.SyncedQuestProfiles.ValueChanged += OnQuestProfilesUpdate;
        Quests_DataTypes.SyncedQuestsEvents.Value.Count();
        Marketplace.Global_Updator += Update;
    }

    private static void OnQuestDataUpdate()
    {
        Quests_DataTypes.AllQuests.Clear();
        if (Player.m_localPlayer)
        {
            Quest_Main_LoadPatch.Postfix();
            Quests_UIs.QuestUI.Reload();
        }
    }

    private static void OnQuestProfilesUpdate()
    {
        Quests_UIs.QuestUI.Reload();
    }

    private static void Update(float dt)
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Quests_UIs.QuestUI.IsVisible())
            {
                Quests_UIs.QuestUI.Hide();
                Menu.instance.OnClose();
            }
        }

        if (!Player.m_localPlayer) return;
        if (Input.GetKeyDown(QuestJournalOpenKey.Value) && Player.m_localPlayer.TakeInput())
            Quests_UIs.QuestUI.ClickJournal();
    }


    [HarmonyPatch(typeof(Player), nameof(Player.Load))]
    [ClientOnlyPatch]
    private static class Quest_Main_LoadPatch
    {
        private static void ClearEmptyQuestCooldowns()
        {
            if (!Player.m_localPlayer) return;
            HashSet<string> toRemove = new();
            const string str = "[MPASN]questCD=";
            const string str2 = "[MPASN]quest=";
            foreach (KeyValuePair<string, string> key in Player.m_localPlayer.m_customData)
            {
                if (key.Key.Contains(str))
                {
                    int UID = Convert.ToInt32(key.Key.Split('=')[1]);
                    if (!Quests_DataTypes.AllQuests.ContainsKey(UID) ||
                        !Quests_DataTypes.Quest.IsOnCooldown(UID, out _))
                    {
                        toRemove.Add(key.Key);
                    }
                }

                if (key.Key.Contains(str2))
                {
                    int UID = Convert.ToInt32(key.Key.Split('=')[1]);
                    if (!Quests_DataTypes.AllQuests.ContainsKey(UID))
                    {
                        toRemove.Add(key.Key);
                    }
                }
            }

            foreach (string remove in toRemove)
            {
                Player.m_localPlayer.m_customData.Remove(remove);
            }
        }

        private static void LoadQuests()
        {
            Quests_DataTypes.AcceptedQuests.Clear();
            if (!Player.m_localPlayer) return;
            const string str = "[MPASN]quest=";
            Dictionary<int, string> temp = new();
            foreach (KeyValuePair<string, string> key in Player.m_localPlayer.m_customData)
            {
                if (key.Key.Contains(str))
                {
                    int UID = Convert.ToInt32(key.Key.Split('=')[1]);
                    temp[UID] = key.Value;
                }
            }

            foreach (KeyValuePair<int, string> key in temp)
            {
                string[] split = key.Value.Split(';');
                string score = split[0];
                string time = split.Length > 1 ? split[1] : null;
                Quests_DataTypes.Quest.AcceptQuest(key.Key, score, time, false);
            }

            Quests_UIs.AcceptedQuestsUI.CheckQuests();
        }

        private static void InitRawQuests()
        {
            if (!Player.m_localPlayer || Quests_DataTypes.SyncedQuestData.Value.Count == 0) return;
            if (LatestRevision == Quests_DataTypes.SyncedQuestRevision.Value) return;
            LatestRevision = Quests_DataTypes.SyncedQuestRevision.Value;

            foreach (KeyValuePair<int, Quests_DataTypes.Quest> quest in Quests_DataTypes.SyncedQuestData.Value)
            {
                if (quest.Value.Init())
                {
                    Quests_DataTypes.AllQuests[quest.Key] = quest.Value;
                }
                else
                {
                    Utils.print($"{quest.Value.Name} (id {quest.Key}) can't finish init");
                }
            }

            foreach (KeyValuePair<Quests_DataTypes.Quest, string> url in Quests_DataTypes.AllQuests.Select(x =>
                         new KeyValuePair<Quests_DataTypes.Quest, string>(x.Value, x.Value.PreviewImage)))
            {
                Utils.LoadImageFromWEB(url.Value, (sprite) => url.Key.SetPreviewSprite(sprite));
            }
        }

        public static void Postfix()
        {
            InitRawQuests();
            ClearEmptyQuestCooldowns();
            LoadQuests();
        }
    }

    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Awake))]
    [ClientOnlyPatch]
    private class CloseUIMenuLogout
    {
        private static void Postfix()
        {
            Quests_UIs.AcceptedQuestsUI.Hide();
            LatestRevision = int.MaxValue;
        }
    }
}
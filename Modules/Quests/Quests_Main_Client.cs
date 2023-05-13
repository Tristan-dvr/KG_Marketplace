using System.Diagnostics;
using BepInEx.Configuration;
using UnityEngine.Networking;

namespace Marketplace.Modules.Quests;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Normal, "OnInit")]
public static class Quests_Main_Client
{
    private static ConfigEntry<KeyCode> QuestJournalOpenKey;
    private static int LatestRevision;
    private static Coroutine LoadImagesRoutine;

    private static void OnInit()
    {
        QuestJournalOpenKey = Marketplace._thistype.Config.Bind("General", "Quest Journal Keycode", KeyCode.J);
        Quests_UIs.QuestUI.Init();
        Quests_UIs.AcceptedQuestsUI.Init();
        Quests_DataTypes.SyncedQuestData.ValueChanged += OnQuestDataUpdate;
        Quests_DataTypes.SyncedQuestProfiles.ValueChanged += OnQuestProfilesUpdate;
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

    private static void Update()
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
        private static void MigrateToCustomData()
        {
            const string str = "[MPASN]questCD=";
            const string str2 = "[MPASN]quest=";
            HashSet<string> toRemove = new();
            foreach (KeyValuePair<string, string> kvp in Player.m_localPlayer.m_knownTexts.Where(kvp =>
                         kvp.Key.Contains(str) || kvp.Key.Contains(str2)))
            {
                Player.m_localPlayer.m_customData[kvp.Key] = kvp.Value;
                toRemove.Add(kvp.Key);
            }

            foreach (string key in toRemove)
            {
                Player.m_localPlayer.m_knownTexts.Remove(key);
            }
        }

        private static void ClearEmptyQuestCooldowns()
        {
            if (!Player.m_localPlayer) return;
            MigrateToCustomData();
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
            var questsRevision = Quests_DataTypes.SyncedQuestData.Value.ElementAt(0).Value._revision;
            if (LatestRevision == questsRevision) return;
            LatestRevision = questsRevision;

            foreach (KeyValuePair<int, Quests_DataTypes.Quest> quest in Quests_DataTypes.SyncedQuestData.Value)
            {
                if (quest.Value.Init())
                {
                    Quests_DataTypes.AllQuests.Add(quest.Key, quest.Value);
                }
                else
                {
                    Utils.print($"{quest.Value.Name} (id {quest.Key}) can't finish init");
                }
            }

            if (LoadImagesRoutine != null)
                Marketplace._thistype.StopCoroutine(LoadImagesRoutine);
            LoadImagesRoutine = Marketplace._thistype.StartCoroutine(LoadQuestImages());
        }

        private static IEnumerator LoadQuestImages()
        {
            yield return new WaitForSeconds(3f);
            foreach (KeyValuePair<Quests_DataTypes.Quest, string> url in Quests_DataTypes.AllQuests.Select(x =>
                         new KeyValuePair<Quests_DataTypes.Quest, string>(x.Value, x.Value.PreviewImage)))
            {
                if (string.IsNullOrEmpty(url.Value)) continue;
                UnityWebRequest request = UnityWebRequestTexture.GetTexture(url.Value);
                yield return request.SendWebRequest();
                if (!request.isNetworkError && !request.isHttpError)
                {
                    Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                    if (texture == null || texture.width == 0 || texture.height == 0) continue;
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                    url.Key.SetPreviewSprite(sprite);
                }
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
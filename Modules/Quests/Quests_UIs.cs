﻿using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

namespace Marketplace.Modules.Quests;

public static class Quests_UIs
{
    private const string Star_Emoji = " ★";

    internal static class QuestUI
    {
        private static GameObject UI;
        private static Transform MainTransform;
        private static GameObject QuestGO;
        private static Transform RewardTransform;
        private static GameObject RewardGO;
        private static Text Description;
        private static Image PreviewImage;
        private static Transform DescriptionTransform;
        private static GameObject QuestTarget;
        private static GameObject QuestTargetText;
        private static Button RestrictionButton;
        private static Text RestrictionText;
        private static Text NPCName;
        private static readonly List<GameObject> AllGO = new();
        private static readonly List<GameObject> TOREMOVE = new();
        private static readonly Dictionary<GameObject, int> QuestLink = new();
        private static readonly List<ContentSizeFitter> AllFilters = new();
        private static Scrollbar QuestsScrollbar;
        private static Scrollbar DescriptionScrollbar;
        private static bool IsJournal;
        private static Transform TargetContent;
        private static string CurrentProfile;

        public static bool IsVisible()
        {
            return UI && UI.activeSelf;
        }

        public static void Init()
        {
            UI = UnityEngine.Object.Instantiate(AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("QuestHud"));
            MainTransform = UI.transform.Find("Canvas/QuestPanel/QuestList/ListPanel/Scroll View/Viewport/Content");
            QuestGO = AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("QuestListItem");
            RewardGO = AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("RewardGO");
            RewardTransform =
                UI.transform.Find(
                    "Canvas/QuestPanel/QuestInfoTab/Description/Scroll View/Viewport/Content/RewardsContent");
            Description = UI.transform
                .Find("Canvas/QuestPanel/QuestInfoTab/Description/Scroll View/Viewport/Content/DescriptionText")
                .GetComponent<Text>();
            DescriptionTransform = UI.transform.Find("Canvas/QuestPanel/QuestInfoTab/Description");
            TargetContent =
                UI.transform.Find("Canvas/QuestPanel/QuestInfoTab/Description/Scroll View/Viewport/Content");
            RestrictionText = UI.transform.Find("Canvas/QuestPanel/QuestInfoTab/Action/StatusPanel/StatusText")
                .GetComponent<Text>();
            RestrictionButton = UI.transform.Find("Canvas/QuestPanel/QuestInfoTab/Action/StatusPanel/StatusButton")
                .GetComponent<Button>();
            NPCName = UI.transform.Find("Canvas/QuestPanel/Header/Text").GetComponent<Text>();
            UnityEngine.Object.DontDestroyOnLoad(UI);
            UI.SetActive(false);
            AllFilters.AddRange(UI.GetComponentsInChildren<ContentSizeFitter>().ToList());
            DescriptionScrollbar = UI.transform.Find("Canvas/QuestPanel/QuestInfoTab/Description/Scroll View/Scrollbar")
                .GetComponent<Scrollbar>();
            QuestsScrollbar = UI.transform.Find("Canvas/QuestPanel/QuestList/ListPanel/Scroll View/Scrollbar")
                .GetComponent<Scrollbar>();
            PreviewImage = UI.transform
                .Find("Canvas/QuestPanel/QuestInfoTab/Description/Scroll View/Viewport/Content/IMAGE")
                .GetComponent<Image>();

            QuestTarget = AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("QuestTarget");
            QuestTargetText = AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("QuestTargetText");

            Localization.instance.Localize(UI.transform);
        }


        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Awake))]
        [ClientOnlyPatch]
        private static class InventoryGui_Awake_Patch
        {
            private static void Postfix(InventoryGui __instance)
            {
                foreach (UITooltip uiTooltip in RewardGO.GetComponentsInChildren<UITooltip>(true))
                {
                    uiTooltip.m_tooltipPrefab = __instance.m_playerGrid.m_elementPrefab.GetComponent<UITooltip>()
                        .m_tooltipPrefab;
                }
            }
        }

        private static void CLICKMAINBUTTONQUEST(Quests_DataTypes.Quest quest, int UID, int index)
        {
            Quests_DataTypes.Quest.ClickQuestButton(UID);
            if (!IsVisible()) return;
            Reload();
            if (!(Quests_DataTypes.Quest.IsOnCooldown(UID, out int days) && days > 5000))
                InitQuestData(quest, UID, index);
        }

        private static void ResetUIs()
        {
            Canvas.ForceUpdateCanvases();
            AllFilters.ForEach(filter => filter.enabled = false);
            AllFilters.ForEach(filter => filter.enabled = true);
            DescriptionScrollbar.value = 1;
            QuestsScrollbar.value = 1;
        }

        private static void InitQuestData(Quests_DataTypes.Quest quest, int UID, int index)
        {
            RestrictionButton.onClick.RemoveAllListeners();
            RestrictionButton.onClick.AddListener(() => { CLICKMAINBUTTONQUEST(quest, UID, index); });
            TOREMOVE.ForEach(UnityEngine.Object.Destroy);
            TOREMOVE.Clear();
            SetColors();
            AllGO[index].GetComponent<Image>().color = Color.yellow;
            AssetStorage.AssetStorage.AUsrc.Play();
            RestrictionText.gameObject.SetActive(false);
            RestrictionButton.gameObject.SetActive(false);
            DescriptionTransform.gameObject.SetActive(true);
            string timeLimitString = quest.TimeLimit > 0
                ? $"\n<color=#B20000>{"$mpasn_questtimelimit".Localize()}: {quest.TimeLimit.ToTime()}</color>"
                : "";
            Description.text = quest.Description.Localize().Replace(@"\n", "\n") + timeLimitString;

            PreviewImage.gameObject.SetActive(false);
            if (quest.GetPreviewSprite != null)
            {
                PreviewImage.gameObject.SetActive(true);
                PreviewImage.sprite = quest.GetPreviewSprite;
            }

            for (int i = quest.TargetAMOUNT - 1; i >= 0; --i)
            {
                Image TargetImage = UnityEngine.Object.Instantiate(QuestTarget, TargetContent).transform
                    .Find("TargetImage")
                    .GetComponent<Image>();
                Text TargetText = UnityEngine.Object.Instantiate(QuestTargetText, TargetContent).GetComponent<Text>();
                TOREMOVE.Add(TargetImage.transform.parent.gameObject);
                TOREMOVE.Add(TargetText.gameObject);
                TargetImage.transform.parent.SetSiblingIndex(3);
                TargetText.transform.SetSiblingIndex(4);
                switch (quest.Type)
                {
                    case Quests_DataTypes.QuestType.Kill:
                        TargetText.text =
                            $"{Localization.instance.Localize("$mpasn_" + quest.Type)} {quest.GetLocalizedTarget(i)} x{quest.TargetCount[i]} <color=#00ff00>{Star_Emoji}{quest.TargetLevel[i] - 1}</color>";
                        break;
                    case Quests_DataTypes.QuestType.Talk:
                        TargetText.text =
                            $"{Localization.instance.Localize("$mpasn_" + quest.Type)} {quest.GetLocalizedTarget(i)}";
                        break;
                    case Quests_DataTypes.QuestType.Craft or Quests_DataTypes.QuestType.Collect:
                        string useLevel = $" <color=#00ff00>{Star_Emoji}{quest.TargetLevel[i]}</color>";
                        ItemDrop questItem = ZNetScene.instance.GetPrefab(quest.TargetPrefab[i])
                            .GetComponent<ItemDrop>();
                        if (quest.TargetLevel[i] == 1 && questItem.m_itemData.m_shared.m_maxQuality == 1) useLevel = "";
                        TargetText.text =
                            $"{Localization.instance.Localize("$mpasn_" + quest.Type)} {quest.GetLocalizedTarget(i)} x{quest.TargetCount[i]}{useLevel}";
                        break;
                    default:
                        TargetText.text =
                            $"{Localization.instance.Localize("$mpasn_" + quest.Type)} {quest.GetLocalizedTarget(i)} x{quest.TargetCount[i]}";
                        break;
                }

                switch (quest.Type)
                {
                    case Quests_DataTypes.QuestType.Kill or Quests_DataTypes.QuestType.Harvest:
                        TargetImage.sprite =
                            PhotoManager.__instance.GetSprite(quest.TargetPrefab[i],
                                AssetStorage.AssetStorage.PlaceholderMonsterIcon,
                                quest.TargetLevel[i]);
                        break;
                    case Quests_DataTypes.QuestType.Collect or Quests_DataTypes.QuestType.Craft:
                        TargetImage.sprite = ZNetScene.instance.GetPrefab(quest.TargetPrefab[i])
                            .GetComponent<ItemDrop>()
                            .m_itemData.m_shared.m_icons[0];
                        break;
                    case Quests_DataTypes.QuestType.Build:
                        TargetImage.sprite = ZNetScene.instance.GetPrefab(quest.TargetPrefab[i]).GetComponent<Piece>()
                            .m_icon;
                        break;
                    case Quests_DataTypes.QuestType.Talk:
                        TargetImage.sprite = AssetStorage.AssetStorage.PlaceholderGamblerIcon;
                        break;
                }
            }

            for (int i = 0; i < quest.RewardsAMOUNT; ++i)
            {
                GameObject rewardGO = UnityEngine.Object.Instantiate(RewardGO, RewardTransform);
                TOREMOVE.Add(rewardGO);
                Text rewardText = rewardGO.transform.Find("Text").GetComponent<Text>();
                Image rewardImage = rewardGO.transform.Find("Images/RewardImage").GetComponent<Image>();
                rewardText.text =
                    $"{Localization.instance.Localize("$mpasn_" + quest.RewardType[i])}: {quest.GetLocalizedReward(i)} x{quest.RewardCount[i]} ";
                if (quest.RewardType[i] is Quests_DataTypes.QuestRewardType.Pet)
                {
                    rewardText.text += $" <color=#00ff00>{Star_Emoji}{quest.RewardLevel[i] - 1}</color>";
                }
                else if (quest.RewardType[i] is Quests_DataTypes.QuestRewardType.Item && ZNetScene.instance
                             .GetPrefab(quest.RewardPrefab[i]).GetComponent<ItemDrop>().m_itemData.m_shared
                             .m_maxQuality > 1)
                {
                    rewardText.text += $" <color=#00ff00>{Star_Emoji}{quest.RewardLevel[i]}</color>";
                }

                UITooltip tooltip = rewardGO.GetComponentInChildren<UITooltip>();
                tooltip.m_topic = "Reward";
                tooltip.m_text = "";
                switch (quest.RewardType[i])
                {
                    case Quests_DataTypes.QuestRewardType.Pet:
                        rewardImage.sprite =
                            PhotoManager.__instance.GetSprite(quest.RewardPrefab[i],
                                AssetStorage.AssetStorage.PlaceholderMonsterIcon,
                                quest.RewardLevel[i]);
                        tooltip.m_topic = quest.GetLocalizedReward(i) +
                                          $" <color=#00ff00>{Star_Emoji}{quest.RewardLevel[i] - 1}</color>";
                        tooltip.m_text = Localization.instance.Localize("$mpasn_tooltip_pet");
                        break;
                    case Quests_DataTypes.QuestRewardType.Item:
                        rewardImage.sprite = ZNetScene.instance.GetPrefab(quest.RewardPrefab[i])
                            .GetComponent<ItemDrop>().m_itemData.m_shared.m_icons[0];
                        tooltip.m_topic = quest.GetLocalizedReward(i);
                        tooltip.m_text = ItemDrop.ItemData.GetTooltip(
                            ZNetScene.instance.GetPrefab(quest.RewardPrefab[i]).GetComponent<ItemDrop>().m_itemData,
                            quest.RewardLevel[i], false);
                        break;
                    case Quests_DataTypes.QuestRewardType.Skill or Quests_DataTypes.QuestRewardType.Skill_EXP:
                        rewardImage.sprite = Utils.GetSkillIcon(quest.RewardPrefab[i]);
                        tooltip.m_topic = quest.GetLocalizedReward(i);
                        tooltip.m_text = Localization.instance.Localize(
                                             quest.RewardType[i] is Quests_DataTypes.QuestRewardType.Skill_EXP
                                                 ? "$mpasn_tooltip_addskillexp"
                                                 : "$mpasn_tooltip_addskilllevel") +
                                         " x" + quest.RewardCount[i];
                        break;
                    case Quests_DataTypes.QuestRewardType.EpicMMO_EXP:
                        rewardImage.sprite = AssetStorage.AssetStorage.EpicMMO_Exp;
                        tooltip.m_topic = Localization.instance.Localize("$mpasn_EpicMMO_EXP");
                        tooltip.m_text = Localization.instance.Localize("$mpasn_tooltip_epicmmo ") + "x" +
                                         quest.RewardCount[i];
                        break;
                    case Quests_DataTypes.QuestRewardType.Battlepass_EXP:
                        rewardImage.sprite = AssetStorage.AssetStorage.Battlepass_EXP_Icon;
                        tooltip.m_topic = Localization.instance.Localize("$mpasn_Battlepass_EXP");
                        tooltip.m_text = Localization.instance.Localize("$mpasn_tooltip_battlepass ") + "x" +
                                         quest.RewardCount[i];
                        break;
                    case Quests_DataTypes.QuestRewardType.MH_EXP:
                        rewardImage.sprite = AssetStorage.AssetStorage.MH_Exp_Icon;
                        tooltip.m_topic = Localization.instance.Localize("$mpasn_MH_EXP");
                        tooltip.m_text = Localization.instance.Localize("$mpasn_tooltip_mh ") + "x" +
                                         quest.RewardCount[i];
                        break;
                }
            }

            if (Quests_DataTypes.Quest.IsOnCooldown(UID, out int left))
            {
                RestrictionText.gameObject.SetActive(true);
                RestrictionText.text =
                    $"{Localization.instance.Localize("$mpasn_cooldown")}: <color=#00ff00>{left}</color> {Localization.instance.Localize("$mpasn_daysleft")}";
            }

            else if (Quests_DataTypes.Quest.IsAccepted(UID))
            {
                if (Quests_DataTypes.AcceptedQuests[UID].IsComplete())
                {
                    RestrictionButton.transform.Find("Text").GetComponent<Text>().text =
                        Localization.instance.Localize("$mpasn_complete");
                    RestrictionButton.gameObject.SetActive(true);
                    RestrictionButton.gameObject.GetComponent<Image>().color = new Color(0.57f, 1f, 0.63f);
                }
                else
                {
                    RestrictionButton.transform.Find("Text").GetComponent<Text>().text =
                        Localization.instance.Localize("$mpasn_cancelquest");
                    RestrictionButton.gameObject.SetActive(true);
                    RestrictionButton.gameObject.GetComponent<Image>().color = new Color(1f, 0.27f, 0.35f);
                }
            }
            else
            {
                if (!Quests_DataTypes.Quest.CanTake(UID, out string msg, out _))
                {
                    RestrictionText.gameObject.SetActive(true);
                    RestrictionText.text = msg;
                }
                else
                {
                    RestrictionButton.gameObject.SetActive(true);
                    RestrictionButton.transform.Find("Text").GetComponent<Text>().text =
                        Localization.instance.Localize("$mpasn_takequest");
                    RestrictionButton.gameObject.GetComponent<Image>().color = new Color(1f, 1f, 1f);
                }
            }

            if (IsJournal)
            {
                RestrictionButton.gameObject.SetActive(false);
                RestrictionText.gameObject.SetActive(false);
            }

            Canvas.ForceUpdateCanvases();
            AllFilters.ForEach(filter => filter.enabled = false);
            AllFilters.ForEach(filter => filter.enabled = true);
            DescriptionScrollbar.value = 1;
            if (LatestEnumeratorFrame != null) Marketplace._thistype.StopCoroutine(LatestEnumeratorFrame);
            LatestEnumeratorFrame = Marketplace._thistype.StartCoroutine(FrameWaitShow());
        }

        private static Coroutine LatestEnumeratorFrame;

        private static IEnumerator FrameWaitShow()
        {
            DescriptionScrollbar.gameObject.GetComponent<Image>().enabled = false;
            DescriptionScrollbar.transform.GetChild(0).gameObject.SetActive(false);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            DescriptionScrollbar.gameObject.GetComponent<Image>().enabled = true;
            DescriptionScrollbar.transform.GetChild(0).gameObject.SetActive(true);
        }

        private static void SetColors()
        {
            foreach (KeyValuePair<GameObject, int> link in QuestLink)
            {
                Color c = Color.white;
                if (Quests_DataTypes.Quest.IsOnCooldown(link.Value, out _) ||
                    !Quests_DataTypes.Quest.CanTake(link.Value, out _, out _))
                {
                    c = Color.gray;
                }
                else if (Quests_DataTypes.Quest.IsAccepted(link.Value))
                {
                    c = Quests_DataTypes.AcceptedQuests[link.Value].IsComplete() ? Color.green : Color.red;
                }

                link.Key.GetComponent<Image>().color = IsJournal ? Color.white : c;
            }
        }

        public static void Reload()
        {
            Default();
            AllGO.ForEach(UnityEngine.Object.Destroy);
            AllGO.Clear();
            QuestLink.Clear();
            TOREMOVE.ForEach(UnityEngine.Object.Destroy);
            TOREMOVE.Clear();
            if (!IsVisible()) return;
            int count = 0;
            Quests_DataTypes.Quest ForceQuest;
            int ForceUID;
            int ForceInt;

            List<int> enumerationTarget = IsJournal
                ? Quests_DataTypes.AcceptedQuests.Keys.ToList()
                : (Quests_DataTypes.SyncedQuestProfiles.Value.TryGetValue(CurrentProfile, out var value)
                    ? value
                    : new List<int>());

            foreach (int profileID in enumerationTarget)
            {
                if (Quests_DataTypes.AllQuests.TryGetValue(profileID, out Quests_DataTypes.Quest data))
                {
                    bool canTake = Quests_DataTypes.Quest.CanTake(profileID, out _,
                        out Quests_DataTypes.QuestRequirementType type);

                    if (!canTake && Global_Values._container.Value._hideOtherQuestRequirementQuests &&
                        type is Quests_DataTypes.QuestRequirementType.OtherQuest) continue;

                    if (!canTake && type is Quests_DataTypes.QuestRequirementType.IsVIP) continue;

                    if (Quests_DataTypes.Quest.IsOnCooldown(profileID, out int cd) && cd > 5000) continue;
                    GameObject newGo = UnityEngine.Object.Instantiate(QuestGO, MainTransform);
                    newGo.transform.Find("Text").GetComponent<Text>().text = data.Name.Localize();
                    newGo.transform.Find("ImageList").GetChild((int)data.Type).gameObject.SetActive(true);
                    QuestLink[newGo] = profileID;
                    int send = count;
                    newGo.GetComponent<Button>().onClick.AddListener(() => { InitQuestData(data, profileID, send); });
                    ++count;
                    AllGO.Add(newGo);
                }
            }

            SetColors();
        }

        private static void Default()
        {
            AllGO.ForEach(UnityEngine.Object.Destroy);
            AllGO.Clear();
            QuestLink.Clear();
            RestrictionButton.onClick.RemoveAllListeners();
            TOREMOVE.ForEach(UnityEngine.Object.Destroy);
            TOREMOVE.Clear();
            Description.text = "";
            RestrictionText.text = "";
            RestrictionText.gameObject.SetActive(false);
            RestrictionButton.gameObject.SetActive(false);
            DescriptionTransform.gameObject.SetActive(false);
        }

        public static void ClickJournal()
        {
            AssetStorage.AssetStorage.AUsrc.Play();
            if (IsVisible())
            {
                Hide();
                return;
            }

            IsJournal = true;
            ResetUIs();
            CurrentProfile = "";
            UI.SetActive(true);
            NPCName.text = Localization.instance.Localize("$mpasn_questjournal");
            Reload();
        }


        public static void Show(string profile, string _npcName)
        {
            IsJournal = false;
            ResetUIs();
            CurrentProfile = profile;
            if (!Quests_DataTypes.SyncedQuestProfiles.Value.ContainsKey(profile)) return;
            UI.SetActive(true);
            _npcName = Utils.RichTextFormatting(_npcName);
            NPCName.text = string.IsNullOrEmpty(_npcName) ? Localization.instance.Localize("$mpasn_Quests") : _npcName;
            Reload();
        }

        public static void Hide()
        {
            ResetUIs();
            UI.SetActive(false);
            AllGO.ForEach(UnityEngine.Object.Destroy);
            AllGO.Clear();
            TOREMOVE.ForEach(UnityEngine.Object.Destroy);
            TOREMOVE.Clear();
            QuestLink.Clear();
        }
    }


    public static class AcceptedQuestsUI
    {
        private static GameObject UI;
        private static Transform MainTransform;
        private static GameObject QuestGO;
        private static readonly Dictionary<Quests_DataTypes.Quest, GameObject> UpdateData = new();
        private static readonly List<ContentSizeFitter> AllFilters = new();
        private static Scrollbar MainBar;

        private static readonly float OFFSET = 22f;

        public static bool IsVisible()
        {
            return UI && UI.activeSelf;
        }

        private static void InitQuestData(GameObject go, Quests_DataTypes.Quest data, int UID)
        {
            string timeLeft = data.TimeLimit > 0 ? $"\n (<color=red>{CalculateTimeLeft(data).ToTime()}</color>)" : "";
            go.transform.Find("QuestName").GetComponent<Text>().text =
                $"<color=yellow> [ {data.Name} ]</color>".Localize() + timeLeft;
            go.transform.Find("QuestName/Button").GetComponent<Button>().onClick.AddListener(() =>
            {
                AssetStorage.AssetStorage.AUsrc.Play();
                Quests_DataTypes.Quest.RemoveQuestFailed(UID);
                CheckQuests();
                QuestUI.Reload();
            });


            Transform InitialObject = go.transform.Find("Duplicate");
            InitialObject.gameObject.SetActive(false);

            for (int i = 0; i < data.TargetAMOUNT; ++i)
            {
                GameObject newGo = UnityEngine.Object.Instantiate(InitialObject.gameObject, go.transform);
                newGo.name = i.ToString();


                string stars = data.Type switch
                {
                    Quests_DataTypes.QuestType.Kill => $" (<color=#00ff00>{data.TargetLevel[i] - 1}★</color>)",
                    Quests_DataTypes.QuestType.Craft or Quests_DataTypes.QuestType.Collect =>
                        $" (<color=#00ff00>{data.TargetLevel[i]}★</color>)",
                    _ => ""
                };

                if (data.Type is Quests_DataTypes.QuestType.Craft or Quests_DataTypes.QuestType.Collect)
                {
                    ItemDrop questItem = ZNetScene.instance.GetPrefab(data.TargetPrefab[i]).GetComponent<ItemDrop>();
                    if (data.TargetLevel[i] == 1 && questItem.m_itemData.m_shared.m_maxQuality == 1) stars = "";
                }

                newGo.transform.GetComponent<Text>().text =
                    $"- <color=#FFA400>{Localization.instance.Localize("$mpasn_" + data.Type)}</color> {data.GetLocalizedTarget(i)}{stars}";
                newGo.transform.Find("Score").GetComponent<Text>().text =
                    $"[{data.ScoreArray[i]}/{data.TargetCount[i]}]";
                newGo.transform.Find("Score").GetComponent<Text>().color =
                    data.IsComplete(i) ? Color.green : new Color(1f, 0.6f, 0);
                newGo.SetActive(true);
            }
        }


        public static void UpdateStatus(Quests_DataTypes.Quest data)
        {
            if (!UpdateData.ContainsKey(data)) return;
            GameObject go = UpdateData[data];

            for (int i = 0; i < data.TargetAMOUNT; ++i)
            {
                go.transform.Find($"{i}/Score").GetComponent<Text>().text =
                    $"[{data.ScoreArray[i]}/{data.TargetCount[i]}]";
                go.transform.Find($"{i}/Score").GetComponent<Text>().color =
                    data.IsComplete(i) ? Color.green : new Color(1f, 0.6f, 0);
            }
        }

        public static void Init()
        {
            UI = UnityEngine.Object.Instantiate(
                AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("MarketACCEPTEDquests"));
            MainTransform = UI.transform.Find("UI/panel/ALLSTUFF/Scroll View/Viewport/Content");
            QuestGO = AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("AcceptedQuestGONew");
            UI.transform.Find("UI/panel/btn").GetComponent<Button>().onClick.AddListener(ClickHideButton);
            UI.transform.Find("UI/panel/btn2").GetComponent<Button>().onClick.AddListener(QuestUI.ClickJournal);
            UnityEngine.Object.DontDestroyOnLoad(UI);
            UI.SetActive(false);
            AllFilters.AddRange(UI.GetComponentsInChildren<ContentSizeFitter>().ToList());
            MainBar = UI.GetComponentInChildren<Scrollbar>();
            MainBar.onValueChanged.AddListener(ScrollBarValueChange);
            Marketplace._thistype.StartCoroutine(UpdateTimedQuests());
        }

        private static long CalculateTimeLeft(Quests_DataTypes.Quest quest)
        {
            long finishTime = quest.TimeLimit + quest.AcceptedTime;
            long currentTime = (long)EnvMan.instance.m_totalSeconds;
            long timeLeft = finishTime - currentTime;
            return Math.Max(0, timeLeft);
        }

        private static IEnumerator UpdateTimedQuests()
        {
            var yield = new WaitForSeconds(1f);
            HashSet<int> toRemove = new();
            while (true)
            {
                yield return yield;
                if (!Player.m_localPlayer) continue;
                foreach (var quest in Quests_DataTypes.AcceptedQuests)
                {
                    if (quest.Value.TimeLimit <= 0) continue;
                    long timeLeft = CalculateTimeLeft(quest.Value);
                    if (timeLeft <= 0)
                    {
                        toRemove.Add(quest.Key);
                    }
                    else
                    {
                        if (UpdateData.TryGetValue(quest.Value, out var value))
                        {
                            value.transform.Find("QuestName").GetComponent<Text>().text =
                                    $"<color=yellow> [ {quest.Value.Name} ]</color>".Localize() +
                                    $"\n (<color=red>{CalculateTimeLeft(quest.Value).ToTime()}</color>)";
                        }
                    }
                }
                foreach (int id in toRemove)
                {
                    string questName = Quests_DataTypes.AcceptedQuests[id].Name.Localize();
                    MessageHud.instance.ShowMessage(MessageHud.MessageType.Center,
                        $"{questName} $mpasn_questtimelimitfail");
                    Quests_DataTypes.Quest.RemoveQuestFailed(id);
                    CheckQuests();
                }

                toRemove.Clear();
            }
        }

        private static void ScrollBarValueChange(float arg0)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }


        private static void ClickHideButton()
        {
            AssetStorage.AssetStorage.AUsrc.Play();
            GameObject transform = UI.transform.Find("UI/panel/ALLSTUFF").gameObject;
            transform.SetActive(!transform.activeSelf);
        }

        public static void CheckQuests()
        {
            Quests_DataTypes.Quest.CheckCharacterTargets();
            Quests_DataTypes.Quest.CheckItemDropsTargets();
            Quests_DataTypes.Quest.CheckPickableTargets();
            Quests_DataTypes.Quest.CheckTalkTargets();
            if (Quests_DataTypes.AcceptedQuests.Count == 0)
            {
                Hide();
                return;
            }

            Show();
            foreach (var go in UpdateData)
            {
                UnityEngine.Object.Destroy(go.Value);
            }

            UpdateData.Clear();
            int c = 0;
            foreach (KeyValuePair<int, Quests_DataTypes.Quest> quest in Quests_DataTypes.AcceptedQuests.OrderBy(d =>
                         d.Value.Name))
            {
                GameObject newGo = UnityEngine.Object.Instantiate(QuestGO, MainTransform);
                InitQuestData(newGo, quest.Value, quest.Key);
                UpdateData.Add(quest.Value, newGo);
                newGo.transform.SetAsLastSibling();
            }

            Canvas.ForceUpdateCanvases();
            AllFilters.ForEach(filter => filter.enabled = false);
            AllFilters.ForEach(filter => filter.enabled = true);
        }


        private static void Show()
        {
            UI.SetActive(true);
        }

        public static void Hide()
        {
            UI.SetActive(false);
            MainBar.value = 1f;
        }
    }

    [HarmonyPatch(typeof(Menu), nameof(Menu.IsVisible))]
    [ClientOnlyPatch]
    private static class QuestUIFix
    {
        private static void Postfix(ref bool __result)
        {
            if (QuestUI.IsVisible()) __result = true;
        }
    }
}
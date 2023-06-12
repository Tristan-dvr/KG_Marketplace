using Marketplace.UI_OptimizationHandler;

namespace Marketplace.Modules.Battlepass;

public static class Battlepass_UI
{
      private static GameObject UI;
        private static GameObject UI_Scale;
        private static GameObject FreeUserElement;
        private static GameObject PremiumUserElement;
        private static Transform MainContent;
        private static Transform ExperienceBarTransform;
        private static GameObject RewardElement;
        private static Image ExperienceBarSlider;
        private static Text Header_Text;
        private static Text Exp_Text;
        private static RectTransform EXP_BAR_Effect;
        private static Scrollbar Scrollbar_Main;
        private const float EXP_BAR_OFFEST = 225;
        private const float EXP_BAR_DEFAULT_WIDTH = 330;
        private const float OFFSET_EXPBAR_GLOW = 18f;
        public static int FindMax = -1;
    
        private static readonly Sprite[] Icons = new Sprite[3];

        private static readonly Dictionary<GameObject, KeyValuePair<int, Battlepass_Main_Client.Type>> AllGO = new();

        public static bool IsPanelVisible()
        {
            return UI && UI_Scale.activeInHierarchy;
        }


        public static void Init()
        {
            UI = UnityEngine.Object.Instantiate(AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("BattlePassUI"));
            UnityEngine.Object.DontDestroyOnLoad(UI);
            UI_Scale = UI.transform.Find("Canvas/Buffer").gameObject;
            FreeUserElement = AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("BPELEMENTFree");
            PremiumUserElement = AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("BPELEMENTPREMIUM");
            RewardElement = AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("BPrewardElement");
            Icons[0] = AssetStorage.AssetStorage.asset.LoadAsset<Sprite>("NotAvaliableIcon");
            Icons[1] = AssetStorage.AssetStorage.asset.LoadAsset<Sprite>("Check_Icon_256");
            Icons[2] = AssetStorage.AssetStorage.asset.LoadAsset<Sprite>("Star_Full_Icon_256");
            MainContent =
                UI.transform.Find("Canvas/Buffer/BufferList/ListPanel/Scroll View/Viewport/Content");
            ExperienceBarTransform =
                UI.transform.Find("Canvas/Buffer/BufferList/ListPanel/Scroll View/Viewport/Content/ExperienceBar");
            Header_Text = UI.transform.Find("Canvas/Buffer/Header/Text").GetComponent<Text>();
            ExperienceBarSlider = ExperienceBarTransform.Find("WhiteSpace").GetComponent<Image>();
            Exp_Text = UI.transform.Find("Canvas/Buffer/Background/Separator/Text").GetComponent<Text>();
            ExperienceBarTransform.GetComponent<RectTransform>()
                .SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
            EXP_BAR_Effect = ExperienceBarTransform.transform.Find("Glow").GetComponent<RectTransform>();
            Exp_Text.text = "";
            UI.SetActive(false);
            UI_Scale.SetActive(false);
            Scrollbar_Main = UI.transform.Find("Canvas/Buffer/BufferList/ListPanel/Scroll View/Scrollbar")
                .GetComponent<Scrollbar>();
            UI.transform.Find("Canvas/btn2").GetComponent<Button>().onClick.AddListener(OpenButtonClick);
            Localization.instance.Localize(UI.transform);
        }

        [HarmonyPatch(typeof(Menu), nameof(Menu.IsVisible))]
        [ClientOnlyPatch]
        private static class BattlePass
        {
            private static void Postfix(ref bool __result)
            {
                if (IsPanelVisible()) __result = true;
            }
        }
        
        
        private static void OpenButtonClick()
        {
            if (IsPanelVisible())
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        private static void UI_Switch(bool tf)
        {
            UI.SetActive(tf);
        }

        private static void InitElementData(GameObject element, Battlepass_DataTypes.BattlePassElement data, Battlepass_Main_Client.Type type)
        {
            element.transform.Find("Main/RewardName").GetComponent<Text>().text = data.RewardName;
            element.transform.Find("Main/ExpNeeded").GetComponent<Text>().text =
                $"Exp: {(data.Order + 1) * Battlepass_DataTypes.SyncedBattlepassData.Value.ExpStep}";
            element.transform.Find("CLICK").GetComponent<Button>().onClick
                .AddListener(() => ClickElement(data, type));
            Transform content = element.transform.Find("DataScroll/Viewport/Content");
            for (int i = 0; i < data.ItemNames.Count; ++i)
            {
                GameObject go = UnityEngine.Object.Instantiate(RewardElement, content);
                go.transform.Find("Main").GetComponent<Text>().text =
                    $"x{data.ItemCounts[i]} {data.GetLocalizedName(i)}{data.GetString(i)}";
                go.transform.Find("Icon").GetComponent<Image>().sprite = data.GetSprite(i);
            }
        }

        private static void ClickElement(Battlepass_DataTypes.BattlePassElement data, Battlepass_Main_Client.Type type)
        {
            if (type is Battlepass_Main_Client.Type.Premium && !Battlepass_Main_Client.IsPremium()) return;
            AssetStorage.AssetStorage.AUsrc.Play();
            if (Battlepass_Main_Client.IsTaken(data.Order, type)) return;
            int neededExp = Battlepass_DataTypes.SyncedBattlepassData.Value.ExpStep * (data.Order + 1);
            if (Battlepass_Main_Client.GetExp() < neededExp) return;
            for (int index = 0; index < data.ItemNames.Count; index++)
            {
                string prefab = data.ItemNames[index];
                int count = data.ItemCounts[index];
                int level = data.ItemLevels[index];
                GameObject pref = ZNetScene.instance.GetPrefab(prefab);
                if (pref)
                {
                    Utils.InstantiateItem(pref, count, level);
                }
                else
                {
                    float skillLevel = Utils.GetPlayerSkillLevelCustom(prefab);
                    if (skillLevel > 0)
                        Player.m_localPlayer.GetSkills().CheatRaiseSkill(prefab, count);
                }
            }

            Battlepass_Main_Client.SetTaken(data.Order, type);
            OnShowData();
        }


        public static void LoadData()
        {
            if(!Player.m_localPlayer) return;
            FindMax = -1;
            foreach (KeyValuePair<GameObject, KeyValuePair<int, Battlepass_Main_Client.Type>> i in AllGO)
            {
                UnityEngine.Object.Destroy(i.Key);
            }

            AllGO.Clear();
            if (Battlepass_DataTypes.SyncedBattlepassData.Value.FreeRewards.Count == 0 && Battlepass_DataTypes.SyncedBattlepassData.Value.PremiumRewards.Count == 0)
            {
                OpenClose(false, true);
                UI_Switch(false);
                return;
            }

            int maxFree = Battlepass_DataTypes.SyncedBattlepassData.Value.FreeRewards.Count > 0 ? Battlepass_DataTypes.SyncedBattlepassData.Value.FreeRewards.Max(d => d.Order) : -1;
            int maxPremium = Battlepass_DataTypes.SyncedBattlepassData.Value.PremiumRewards.Count > 0 ? Battlepass_DataTypes.SyncedBattlepassData.Value.PremiumRewards.Max(d => d.Order) : -1;
            FindMax = Mathf.Max(maxFree, maxPremium);
            ExperienceBarSlider.fillAmount = 0;
            float barSize = EXP_BAR_OFFEST + EXP_BAR_OFFEST * FindMax;
            ExperienceBarTransform.GetComponent<RectTransform>()
                .SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, barSize);

            //create free elements
            if (Battlepass_DataTypes.SyncedBattlepassData.Value.FreeRewards.Count > 0)
            {
                for (int i = 0; i < Battlepass_DataTypes.SyncedBattlepassData.Value.FreeRewards.Count; ++i)
                {
                    Battlepass_DataTypes.BattlePassElement currentData = Battlepass_DataTypes.SyncedBattlepassData.Value.FreeRewards[i];
                    GameObject go = UnityEngine.Object.Instantiate(FreeUserElement, MainContent);
                    Vector2 offset = new Vector2(EXP_BAR_OFFEST * currentData.Order, 0);
                    go.GetComponent<RectTransform>().anchoredPosition += offset;
                    AllGO.Add(go,
                        new KeyValuePair<int, Battlepass_Main_Client.Type>(currentData.Order,
                            Battlepass_Main_Client.Type.Free));
                    InitElementData(go, currentData, Battlepass_Main_Client.Type.Free);
                }
            }

            //create premium elements
            if (Battlepass_DataTypes.SyncedBattlepassData.Value.PremiumRewards.Count > 0)
            {
                for (int i = 0; i < Battlepass_DataTypes.SyncedBattlepassData.Value.PremiumRewards.Count; ++i)
                {
                    Battlepass_DataTypes.BattlePassElement currentData = Battlepass_DataTypes.SyncedBattlepassData.Value.PremiumRewards[i];
                    GameObject go = UnityEngine.Object.Instantiate(PremiumUserElement, MainContent);
                    Vector2 offset = new Vector2(EXP_BAR_OFFEST * currentData.Order, 0);
                    go.GetComponent<RectTransform>().anchoredPosition += offset;
                    AllGO.Add(go,
                        new KeyValuePair<int, Battlepass_Main_Client.Type>(currentData.Order,
                            Battlepass_Main_Client.Type.Premium));
                    InitElementData(go, currentData, Battlepass_Main_Client.Type.Premium);
                }
            }

            MainContent.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                EXP_BAR_DEFAULT_WIDTH + EXP_BAR_OFFEST * FindMax + EXP_BAR_OFFEST / 2);
            ExperienceBarTransform.SetAsLastSibling();
            List<ContentSizeFitter> AllFilters = UI.GetComponentsInChildren<ContentSizeFitter>().ToList();
            Canvas.ForceUpdateCanvases();
            AllFilters.ForEach(filter => filter.enabled = false);
            AllFilters.ForEach(filter => filter.enabled = true);
            Scrollbar_Main.value = 0f;
            OnShowData();
            OpenClose(true, true);
            OpenClose(false, true);
            UI_Switch(true);
        }

    
        
        private static void OnShowData()
        {
            Header_Text.text = Battlepass_DataTypes.SyncedBattlepassData.Value.Name;
            Battlepass_Main_Client.GetExp();
            int maxXP = (FindMax + 1) * Battlepass_DataTypes.SyncedBattlepassData.Value.ExpStep;
            Exp_Text.text = $"{Battlepass_Main_Client.GetExp()} / {maxXP}";
            ExperienceBarSlider.fillAmount = Mathf.Clamp01((float)Battlepass_Main_Client.GetExp() / maxXP);
            float test = EXP_BAR_Effect.transform.parent.GetComponent<RectTransform>().sizeDelta.x *
                         ExperienceBarSlider.fillAmount;
            EXP_BAR_Effect.anchoredPosition = new Vector2(test, 0);
            UI.transform.Find("Canvas/Buffer/PremiumMask").gameObject.SetActive(!Battlepass_Main_Client.IsPremium());
            foreach (KeyValuePair<GameObject, KeyValuePair<int, Battlepass_Main_Client.Type>> go in AllGO)
            {
                Image transform = go.Key.transform.Find("CLICK").GetComponent<Image>();
                ParticleSystem glow = go.Key.transform.Find("Glow").GetComponent<ParticleSystem>();

                transform.gameObject.SetActive(true);
                glow.gameObject.SetActive(true);

                if (go.Value.Value is Battlepass_Main_Client.Type.Premium && !Battlepass_Main_Client.IsPremium())
                {
                    transform.gameObject.SetActive(false);
                    glow.gameObject.SetActive(false);
                    continue;
                }

                Color c;
                if (Battlepass_Main_Client.IsTaken(go.Value.Key, go.Value.Value))
                {
                    transform.sprite = Icons[1];
                    c = Color.green;
                    c.a = 0.58f;
                    ParticleSystem.MainModule main = glow.main;
                    main.startColor = c;
                    continue;
                }

                int neededExp = Battlepass_DataTypes.SyncedBattlepassData.Value.ExpStep * (go.Value.Key + 1);
                if (Battlepass_Main_Client.GetExp() >= neededExp)
                {
                    transform.sprite = Icons[2];
                    c = Color.yellow;
                    c.a = 0.58f;
                    ParticleSystem.MainModule main = glow.main;
                    main.startColor = c;
                }
                else
                {
                    c = Color.red;
                    c.a = 0.58f;
                    ParticleSystem.MainModule main = glow.main;
                    main.startColor = c;
                    transform.sprite = Icons[0];
                }
            }
        }


        public static void Show(bool silent = false)
        {
            if (!Player.m_localPlayer) return;
            OpenClose(true, silent);
            OnShowData();
        }

        public static void Hide(bool silent = false)
        {
            OpenClose(false, silent);
        }


        private static void OpenClose(bool open, bool silent)
        {
            if (!UI.activeSelf) return;
            if (open)
            {
                if (!silent) AssetStorage.AssetStorage.AUsrc.PlayOneShot(AssetStorage.AssetStorage.OpenUI_Sound);
                UI_Scale.SetActive(true);
                ScrollView_Optimization.StartOptimization(UI, IsPanelVisible, AllGO.Keys, UI_Optimizations.OptimizationType.Horizontal);
            }
            else
            {
                if (!silent) AssetStorage.AssetStorage.AUsrc.PlayOneShot(AssetStorage.AssetStorage.CloseUI_Sound);
                UI_Scale.SetActive(false);
            }
        }


        [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Awake))]
        [ClientOnlyPatch]
        private class CloseUIMenuLogout_Battlepass
        {
            private static void Postfix()
            {
                UI_Switch(false);
            }
        }
}
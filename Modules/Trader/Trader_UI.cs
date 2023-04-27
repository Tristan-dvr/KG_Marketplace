using Marketplace.UI_OptimizationHandler;
using Object = UnityEngine.Object;

namespace Marketplace.Modules.Trader;

public static class Trader_UI
{
    private static GameObject UI;
    private static GameObject BuyElement;
    private static Transform ContentTransform;
    private static readonly Color Enabled = new(0.5361788f, 1f, 0.1176471f, 0.6862745f);
    private static readonly Color Disabled = new(1f, 0.1766787f, 0.1176471f, 0.6862745f);
    private static readonly List<GameObject> CurrentObjects = new();
    private static string CurrentProfile = "";
    private static readonly List<ContentSizeFitter> AllFilters = new();
    private static Scrollbar ScrollbarMain;
    private static Text NPCname;
    private static IEnumerable<Trader_DataTypes.TraderData> SortedList;
    private static InputField SearchInput;
    private static Modifier CurrentModifier;
    private static readonly Transform[] ModifierButtons = new Transform[Enum.GetValues(typeof(Modifier)).Length];

    public static bool IsPanelVisible()
    {
        return UI && UI.activeSelf;
    }

    private enum Modifier
    {
        x1,
        x5,
        x10,
        x100
    }

    private static readonly Dictionary<Modifier, int> ModifierValues = new()
    {
        { Modifier.x1, 1 },
        { Modifier.x5, 5 },
        { Modifier.x10, 10 },
        { Modifier.x100, 100 }
    };


    public static void Init()
    {
        UI = Object.Instantiate(AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("MarketplaceTraderNewUI"));
        BuyElement = AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("TraderInstantiateV2");
        ContentTransform = UI.transform.Find("Canvas/Scroll View/Viewport/Content");
        Object.DontDestroyOnLoad(UI);
        UI.SetActive(false);
        AllFilters.AddRange(UI.GetComponentsInChildren<ContentSizeFitter>().ToList());
        ScrollbarMain = UI.GetComponentInChildren<Scrollbar>();
        NPCname = UI.transform.Find("Canvas/Header/Text").GetComponent<Text>();
        SearchInput = UI.GetComponentInChildren<InputField>();
        SearchInput.onValueChanged.AddListener(OnSearchInput);

        ModifierButtons[0] = UI.transform.Find("Canvas/Background/x1");
        ModifierButtons[1] = UI.transform.Find("Canvas/Background/x5");
        ModifierButtons[2] = UI.transform.Find("Canvas/Background/x10");
        ModifierButtons[3] = UI.transform.Find("Canvas/Background/x100");

        for (int i = 0; i < ModifierButtons.Length; i++)
        {
            int i1 = i;
            ModifierButtons[i].GetComponent<Button>().onClick.AddListener(() =>
            {
                AssetStorage.AssetStorage.AUsrc.Play();
                SetModifier((Modifier)i1);
                CreateElementsNew();
            });
        }

        Localization.instance.Localize(UI.transform);
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Awake))]
    [ClientOnlyPatch]
    private static class InventoryGui_Awake_Patch
    {
        private static void Postfix(InventoryGui __instance)
        {
            foreach (UITooltip uiTooltip in BuyElement.GetComponentsInChildren<UITooltip>(true))
            {
                uiTooltip.m_tooltipPrefab = __instance.m_playerGrid.m_elementPrefab.GetComponent<UITooltip>()
                    .m_tooltipPrefab;
            }
        }
    }
    
    [HarmonyPatch(typeof(Menu), nameof(Menu.IsVisible))]
    [ClientOnlyPatch]
    private static class TraderUIFix
    {
        private static void Postfix(ref bool __result)
        {
            if (IsPanelVisible()) __result = true;
        }
    }

    private static void SetModifier(Modifier modifier)
    {
        CurrentModifier = modifier;
        for (int i = 0; i < ModifierButtons.Length; i++)
        {
            ModifierButtons[i].transform.Find("Image").GetComponent<Image>().color =
                i == (int)modifier ? Color.green : Color.white;
            ModifierButtons[i].transform.Find("Text").GetComponent<Text>().color =
                i == (int)modifier ? Color.green : Color.white;
        }
    }

    private static void OnSearchInput(string arg0)
    {
        AssetStorage.AssetStorage.AUsrc.PlayOneShot(AssetStorage.AssetStorage.TypeClip, 0.7f);
        SortList();
        CreateElementsNew();
    }

    private static void SortList()
    {
        if (!Trader_DataTypes.TraderItemList.Value.ContainsKey(CurrentProfile)) return;
        string search = SearchInput.text.ToLower();
        if (string.IsNullOrWhiteSpace(search))
        {
            SortedList = Trader_DataTypes.TraderItemList.Value[CurrentProfile];
            return;
        }

        SortedList = Trader_DataTypes.TraderItemList.Value[CurrentProfile].Where(data =>
            data.NeededItems.Any(i => i.ItemName.ToLower().Contains(search)) ||
            data.ResultItems.Any(i => i.ItemName.ToLower().Contains(search)));
    }

    private static float CalculateOffset(int amount) => Mathf.Max(0, (amount - 1) * 34.5f);

    private static void CreateElementsNew()
    {
        CurrentObjects.ForEach(Object.Destroy);
        CurrentObjects.Clear();
        float y = 0;
        foreach (Trader_DataTypes.TraderData data in SortedList)
        {
            if (data.NeedToKnow)
            {
                bool breakLoop =
                    data.NeededItems.Any(item =>
                        !Player.m_localPlayer.m_knownMaterial.Contains(item.OriginalItemName)) ||
                    data.ResultItems.Any(item =>
                        !item.IsMonster && !Player.m_localPlayer.m_knownMaterial.Contains(item.OriginalItemName));
                if (breakLoop) continue;
            }

            GameObject go = Object.Instantiate(BuyElement, ContentTransform);

            int biggest = data.NeededItems.Count > data.ResultItems.Count
                ? data.NeededItems.Count
                : data.ResultItems.Count;
            int internalOffset = 0;
            for (int i = 0; i < data.NeededItems.Count; ++i)
            {
                Transform transform = i == 0
                    ? go.transform.Find("Background/NeededItems/NeededItem")
                    : Object.Instantiate(go.transform.Find("Background/NeededItems/NeededItem"),
                        go.transform.Find("Background/NeededItems"));
                transform.GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, internalOffset);
                internalOffset += 69;
                int starCount = data.NeededItems[i].Level;
                string stars = data.NeededItems[i].DisplayStars ? $" <color=#00ff00>({starCount}★)</color>" : "";
                transform.transform.Find("Text").GetComponent<Text>().text =
                    $"{data.NeededItems[i].ItemName}{stars}\n<color=yellow>x{data.NeededItems[i].Count * ModifierValues[CurrentModifier]}</color>";
                transform.transform.Find("Icon").GetComponent<Image>().sprite = data.NeededItems[i].GetIcon();
                transform.GetComponent<UITooltip>().m_topic = data.NeededItems[i].ItemName;
                transform.GetComponent<UITooltip>().m_text = ItemDrop.ItemData.GetTooltip(
                    ZNetScene.instance.GetPrefab(data.NeededItems[i].ItemPrefab).GetComponent<ItemDrop>()
                        .m_itemData, data.NeededItems[i].Level, false);
                bool hasEnough =
                    Utils.CustomCountItems(data.NeededItems[i].ItemPrefab,
                        data.NeededItems[i].Level) >=
                    data.NeededItems[i].Count * ModifierValues[CurrentModifier];
                transform.GetComponent<Outline>().effectColor = hasEnough ? Color.green : Color.red;
            }

            go.transform.Find("Background/NeededItems").GetComponent<RectTransform>().anchoredPosition +=
                new Vector2(0, CalculateOffset(data.NeededItems.Count));

            internalOffset = 0;
            for (int i = 0; i < data.ResultItems.Count; ++i)
            {
                Transform transform = i == 0
                    ? go.transform.Find("Background/ResultItems/ResultItem")
                    : Object.Instantiate(go.transform.Find("Background/ResultItems/ResultItem"),
                        go.transform.Find("Background/ResultItems"));
                transform.GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, internalOffset);
                internalOffset += 69;
                int starCount = data.ResultItems[i].IsMonster
                    ? data.ResultItems[i].Level - 1
                    : data.ResultItems[i].Level;
                string stars = data.ResultItems[i].DisplayStars ? $" <color=#00ff00>({starCount}★)</color>" : "";
                transform.transform.Find("Text").GetComponent<Text>().text =
                    $"{data.ResultItems[i].ItemName}{stars}\n<color=yellow>x{data.ResultItems[i].Count * ModifierValues[CurrentModifier]}</color>";
                transform.transform.Find("Icon").GetComponent<Image>().sprite = data.ResultItems[i].GetIcon();
                transform.GetComponent<UITooltip>().m_topic = data.ResultItems[i].ItemName;
                if (data.ResultItems[i].IsMonster)
                    transform.GetComponent<UITooltip>().m_text =
                        Localization.instance.Localize("$mpasn_tooltip_pet");
                else
                    transform.GetComponent<UITooltip>().m_text = ItemDrop.ItemData.GetTooltip(
                        ZNetScene.instance.GetPrefab(data.ResultItems[i].ItemPrefab).GetComponent<ItemDrop>()
                            .m_itemData, data.ResultItems[i].Level, false);
            }

            go.transform.Find("Background/ResultItems").GetComponent<RectTransform>().anchoredPosition +=
                new Vector2(0, CalculateOffset(data.ResultItems.Count));
            go.transform.Find("Background/Conversion").GetComponent<Button>().onClick
                .AddListener(delegate { TradeItemByGo(data); });
            go.GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, y);
            CurrentObjects.Add(go);
            go.transform.Find("Background/Conversion/Conversion").GetComponent<Image>().color =
                CanBuy(data) ? Color.green : Color.red;


            go.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 72.5f * (biggest - 1));
            y += 72.5f * (biggest - 1) + 79f;
        }

        ContentTransform.GetComponent<RectTransform>()
            .SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, y + 20f);
        Canvas.ForceUpdateCanvases();
        AllFilters.ForEach(filter => filter.enabled = false);
        AllFilters.ForEach(filter => filter.enabled = true);
    }

    private static bool CanBuy(Trader_DataTypes.TraderData data)
    {
        foreach (Trader_DataTypes.TraderItem r in data.NeededItems)
            if (Utils.CustomCountItems(r.ItemPrefab, r.Level) <
                r.Count * ModifierValues[CurrentModifier])
                return false;

        return true;
    }


    private static void TradeItemByGo(Trader_DataTypes.TraderData data)
    {
        Player p = Player.m_localPlayer;
        string logMessage =
            $"{Player.m_localPlayer.GetPlayerName()} ({Global_Values._localUserID}) TraderNPC: ";
        AssetStorage.AssetStorage.AUsrc.Play();
        if (!CanBuy(data)) return;

        foreach (Trader_DataTypes.TraderItem neededItem in data.NeededItems)
        {
            int amount = neededItem.Count * ModifierValues[CurrentModifier];
            Utils.CustomRemoveItems(neededItem.ItemPrefab, amount, neededItem.Level);
            logMessage += $"x{amount} {neededItem.ItemName}, ";
        }

        logMessage += "for => ";
        foreach (Trader_DataTypes.TraderItem resultItem in data.ResultItems)
        {
            GameObject prefab = ZNetScene.instance.GetPrefab(resultItem.ItemPrefab);
            if (!resultItem.IsMonster)
            {
                string text = Localization.instance.Localize("$mpasn_added",
                    (resultItem.Count * ModifierValues[CurrentModifier]).ToString(),
                    Localization.instance.Localize(resultItem.ItemName));
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, text);
                if (prefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_maxStackSize > 1)
                {
                    GameObject go = Object.Instantiate(prefab,
                        p.transform.position + p.transform.forward * 1.5f + Vector3.up * 1.5f, Quaternion.identity);
                    ItemDrop itemDrop = go.GetComponent<ItemDrop>();
                    itemDrop.m_itemData.m_quality = resultItem.Level;
                    itemDrop.m_itemData.m_stack = resultItem.Count * ModifierValues[CurrentModifier];
                    itemDrop.Save();
                    if (p.m_inventory.CanAddItem(go))
                    {
                        p.m_inventory.AddItem(itemDrop.m_itemData);
                        ZNetScene.instance.Destroy(go);
                    }
                }
                else
                {
                    for (int i = 0; i < resultItem.Count * ModifierValues[CurrentModifier]; i++)
                    {
                        GameObject go = Object.Instantiate(prefab,
                            p.transform.position + p.transform.forward * 1.5f + Vector3.up * 1.5f,
                            Quaternion.identity);
                        ItemDrop itemDrop = go.GetComponent<ItemDrop>();
                        itemDrop.m_itemData.m_quality = resultItem.Level;
                        itemDrop.Save();
                        if (p.m_inventory.CanAddItem(go))
                        {
                            p.m_inventory.AddItem(itemDrop.m_itemData);
                            ZNetScene.instance.Destroy(go);
                        }
                    }
                }
            }
            else
            {
                string text = Localization.instance.Localize("$mpasn_spawned",
                    (resultItem.Count * ModifierValues[CurrentModifier]).ToString(),
                    Localization.instance.Localize(resultItem.ItemName));
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, text);
                for (int i = 0; i < resultItem.Count * ModifierValues[CurrentModifier]; i++)
                {
                    GameObject go = Object.Instantiate(prefab,
                        Player.m_localPlayer.transform.position - Player.m_localPlayer.transform.forward * 3f + Vector3.up * 1f,
                        Quaternion.identity);
                    go.GetComponent<Character>().SetLevel(resultItem.Level);
                    Tameable tame = go.GetComponent<Tameable>();
                    if (tame) tame.Tame();
                }
            }

            logMessage += $"x{resultItem.Count * ModifierValues[CurrentModifier]} {resultItem.ItemName}, ";
        }

        logMessage = logMessage.Substring(0, logMessage.Length - 2);
        ZRoutedRpc.instance.InvokeRoutedRPC("LogOnServer_mpasn", 3, logMessage);
        CreateElementsNew();
    }

    public static void Hide()
    {
        ScrollbarMain.value = 1;
        UI.SetActive(false);
        CurrentObjects.ForEach(Object.Destroy);
        CurrentObjects.Clear();
    }

    public static void Reload()
    {
        if (IsPanelVisible())
        {
            SortList();
            CreateElementsNew();
        }
    }
    

    public static void Show(string profile, string _npcName)
    {
        if (!Trader_DataTypes.TraderItemList.Value.ContainsKey(profile)) return;
        CurrentProfile = profile;
        SearchInput.text = "";
        SortList();
        SetModifier(Modifier.x1);
        CreateElementsNew();
        InventoryGui.instance.Show(null);
        UI.SetActive(true);
        _npcName = Utils.RichTextFormatting(_npcName);
        NPCname.text = string.IsNullOrEmpty(_npcName) ? Localization.instance.Localize("$mpasn_Trader") : _npcName;
        ScrollView_Optimization.StartOptimization(UI, IsPanelVisible, CurrentObjects,
            UI_Optimizations.OptimizationType.Vertical);
    }
}
namespace Marketplace.Modules.Trader;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Normal, "OnInit")]
public static class Trader_Main_Client
{
    private static void OnInit()
    {
        Trader_UI.Init();
        Trader_DataTypes.SyncedTraderItemList.ValueChanged += OnTraderUpdate;
        Marketplace.Global_Updator += Update;
    }

    private static void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape) || !Trader_UI.IsPanelVisible()) return;
        Trader_UI.Hide();
        Menu.instance.OnClose();
    }

    private static void OnTraderUpdate()
    {
        InitTraderItems();
        Trader_UI.Reload();
    }

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    [ClientOnlyPatch]
    private static class ZNetScene_Awake_Patch
    {
        private static void Postfix() => InitTraderItems();
    }

    
    public static void InitTraderItems()
    {
        Trader_DataTypes.ClientSideItemList.Clear();
        if (!ZNetScene.instance) return;
        foreach (var kvp in Trader_DataTypes.SyncedTraderItemList.Value)
        {
            List<Trader_DataTypes.TraderData> newTraderItems = new List<Trader_DataTypes.TraderData>();
            foreach (Trader_DataTypes.TraderData value in kvp.Value)
            {
                List<Trader_DataTypes.TraderItem> _NeededItems = new List<Trader_DataTypes.TraderItem>();
                List<Trader_DataTypes.TraderItem> _ResultItems = new List<Trader_DataTypes.TraderItem>();
                foreach (Trader_DataTypes.TraderItem NI in value.NeededItems)
                {
                    GameObject neededItemPrefab = ZNetScene.instance.GetPrefab(NI.ItemPrefab);
                    if (!neededItemPrefab || !neededItemPrefab.GetComponent<ItemDrop>())
                        continue;
                    ItemDrop.ItemData neededItem = neededItemPrefab.GetComponent<ItemDrop>().m_itemData;
                    NI.IsMonster = false;
                    NI.DisplayStars = neededItem.m_shared.m_maxQuality > 1;
                    NI.SetIcon(neededItem.m_shared.m_icons[0]);
                    NI.OriginalItemName = neededItem.m_shared.m_name;
                    NI.ItemName = Localization.instance.Localize(NI.OriginalItemName);
                    _NeededItems.Add(NI);
                }

                foreach (Trader_DataTypes.TraderItem RI in value.ResultItems)
                {
                    GameObject resultItemPrefab = ZNetScene.instance.GetPrefab(RI.ItemPrefab);
                    if (!resultItemPrefab) continue;
                    if (resultItemPrefab.GetComponent<ItemDrop>())
                    {
                        ItemDrop.ItemData resultItem = resultItemPrefab.GetComponent<ItemDrop>().m_itemData;
                        RI.DisplayStars = resultItem.m_shared.m_maxQuality > 1;
                        RI.SetIcon(resultItem.m_shared.m_icons[0]);
                        RI.OriginalItemName = resultItem.m_shared.m_name;
                        RI.ItemName = Localization.instance.Localize(RI.OriginalItemName);
                        RI.IsMonster = false;
                    }
                    else if (resultItemPrefab.GetComponent<Character>())
                    {
                        Character c = resultItemPrefab.GetComponent<Character>();
                        PhotoManager.__instance.MakeSprite(resultItemPrefab, 0.6f, 0.25f, RI.Level);
                        RI.SetIcon(PhotoManager.__instance.GetSprite(resultItemPrefab.name,
                            AssetStorage.AssetStorage.PlaceholderMonsterIcon, RI.Level));
                        RI.ItemName = Localization.instance.Localize(c.m_name ?? "Default");
                        RI.IsMonster = true;
                        RI.DisplayStars = true;
                    }

                    _ResultItems.Add(RI);
                }

                if (_NeededItems.Count > 0 && _ResultItems.Count > 0)
                    newTraderItems.Add(new Trader_DataTypes.TraderData()
                        { NeedToKnow = value.NeedToKnow, NeededItems = _NeededItems, ResultItems = _ResultItems });
            }
            Trader_DataTypes.ClientSideItemList[kvp.Key] = newTraderItems;
        }
    }
}
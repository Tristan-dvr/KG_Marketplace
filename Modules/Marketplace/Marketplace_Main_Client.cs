using Object = UnityEngine.Object;

namespace Marketplace.Modules.Marketplace_NPC;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Normal, "OnInit")]
public static class Marketplace_Main_Client
{
    public static int IncomeValue;
    public static Action OnUpdateCurrency; 

    private static void OnInit()
    {
        Marketplace_UI.Init();
        Marketplace.Global_Updator += Update;
        Marketplace.Global_OnGUI_Updator += Marketplace_Messages.OnGUI;
        Marketplace_DataTypes.ServerMarketPlaceData.ValueChanged += OnMarketplaceUpdate;
        Global_Values._container.ValueChanged += () => Marketplace_Main_Client.OnUpdateCurrency();
    }

    private static void OnMarketplaceUpdate()
    {
        if (Marketplace_UI.IsPanelVisible()) Marketplace_UI.ResetBUYPage();
    }
    
    private static void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && (Marketplace_UI.IsPanelVisible() || Marketplace_Messages._showMessageBox))
        {
            Marketplace_UI.Hide();
            Menu.instance.OnClose();
            Marketplace_Messages._showMessageBox = false;
        }
    }

    [HarmonyPatch(typeof(Menu), nameof(Menu.IsVisible))]
    [ClientOnlyPatch]
    private static class MarketplaceUIFix
    {
        private static void Postfix(ref bool __result)
        {
            if (Marketplace_UI.IsPanelVisible()) __result = true;
        }
    }

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    [ClientOnlyPatch]
    private static class ZrouteMethodsClient
    {
        private static void Postfix()
        {
            ZRoutedRpc.instance.Register("KGmarket BuyItemAnswer",
                new Action<long, string>(InstantiateItemFromServer));
            ZRoutedRpc.instance.Register("KGmarket ReceiveIncome",
                new Action<long, int>(ReceiveIncomeFromServer));
            ZRoutedRpc.instance.Register("KGmarket GetLocalMessages",
                new Action<long, ZPackage>(Marketplace_Messages.Messenger.GetMessages));
        }
    }
    
    private static void ReceiveIncomeFromServer(long sender, int value)
    {
        IncomeValue = value;
        Marketplace_UI.ResetIncome();
    }
    
    private static void InstantiateItemFromServer(long sender, string data)
    {
        if (sender == ZNet.instance.GetServerPeer().m_uid && data.Length > 0)
        {
            Player p = Player.m_localPlayer;
            Marketplace_DataTypes.ServerMarketSendData forTest = JSON.ToObject<Marketplace_DataTypes.ServerMarketSendData>(data);
            GameObject main = ZNetScene.instance.GetPrefab(forTest.ItemPrefab);
            if (!main) return;
            string text = Localization.instance.Localize("$mpasn_added", forTest.Count.ToString(),
                Localization.instance.Localize(main.GetComponent<ItemDrop>().m_itemData.m_shared.m_name));
            MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, text);
            ItemDrop item = main.GetComponent<ItemDrop>();
            forTest.Quality = Mathf.Max(1, forTest.Quality);
            int stack = forTest.Count;
            Dictionary<string, string> NewCustomData = JSON.ToObject<Dictionary<string, string>>(forTest.CUSTOMdata);

            while (stack > 0)
            {
                if (p.m_inventory.FindEmptySlot(false) is { x: >= 0 } pos)
                {
                    int addStack = Math.Min(stack, item.m_itemData.m_shared.m_maxStackSize);
                    stack -= addStack;
                    p.m_inventory.AddItem(forTest.ItemPrefab, addStack,
                        item.m_itemData.GetMaxDurability(forTest.Quality), pos,
                        false, forTest.Quality, forTest.Variant, forTest.CrafterID, forTest.CrafterName,
                        NewCustomData,  Game.m_worldLevel, true);
                }
                else
                {
                    break;
                }
            }

            if (stack <= 0) return;
            while (stack > 0)
            {
                int addStack = Math.Min(stack, item.m_itemData.m_shared.m_maxStackSize);
                stack -= addStack;
                Transform transform = p.transform;
                Vector3 position = transform.position;
                ItemDrop itemDrop = Object.Instantiate(main, position + Vector3.up, transform.rotation)
                    .GetComponent<ItemDrop>();
                itemDrop.m_itemData.m_customData = NewCustomData;
                itemDrop.m_itemData.m_stack = addStack;
                itemDrop.m_itemData.m_crafterName = forTest.CrafterName;
                itemDrop.m_itemData.m_crafterID = forTest.CrafterID;
                itemDrop.Save();
                itemDrop.OnPlayerDrop();
                itemDrop.GetComponent<Rigidbody>().velocity = (transform.forward + Vector3.up);
                p.m_dropEffects.Create(position, Quaternion.identity);
            }
        }
    }
}
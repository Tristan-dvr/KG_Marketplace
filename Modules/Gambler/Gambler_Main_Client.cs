namespace Marketplace.Modules.Gambler;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Normal, "OnInit")]
public static class Gambler_Main_Client
{
    private static void OnInit()
    {
        Gambler_UI.Init();
        Gambler_DataTypes.GamblerData.ValueChanged += OnGamblerUpdate;
        Marketplace.Global_Updator += Update;
    }

    private static void OnGamblerUpdate()
    {
        GamblerInit();
        if (Gambler_UI.IsPanelVisible() && Gambler_UI.CurrentStatus == Gambler_UI.Status.Idle)
        {
            Gambler_UI.Reload();
        }
    }
    
    private static void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape) || !Gambler_UI.IsPanelVisible()) return;
        Gambler_UI.Hide();
        Menu.instance.OnClose();
    }
    
    [HarmonyPatch(typeof(ZNetScene),nameof(ZNetScene.Awake))]
    [ClientOnlyPatch]
    private static class ZNetScene_Awake_Patch
    {
        private static void Postfix() => GamblerInit();
    }

    public static readonly Dictionary<string, Gambler_DataTypes.Item> RequiredItem = new();
    private static void GamblerInit()
    {
        if(!ZNetScene.instance) return;
        RequiredItem.Clear();
        foreach (KeyValuePair<string, Gambler_DataTypes.BigData> item in Gambler_DataTypes.GamblerData.Value)
        {
            foreach (Gambler_DataTypes.Item data in item.Value.Data)
            {
                GameObject prefab = ZNetScene.instance.GetPrefab(data.Prefab);
                if (!prefab)
                {
                    data.SetData(data.Prefab, AssetStorage.AssetStorage.PlaceholderGamblerIcon);
                }
                else
                {
                    Sprite hook = prefab.GetComponent<ItemDrop>()?.m_itemData.GetIcon();
                    string name = prefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_name;
                    data.SetData(name, hook);
                }
            }

            RequiredItem[item.Key] = item.Value.Data[0];
            item.Value.Data.RemoveAt(0);
        }
    }
}
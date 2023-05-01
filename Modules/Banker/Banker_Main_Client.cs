namespace Marketplace.Modules.Banker;

[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Normal, "OnInit")]
public static class Banker_Main_Client
{
    private static void OnInit()
    {
        Banker_UI.Init();
        Banker_DataTypes.SyncedBankerProfiles.ValueChanged += OnBankerUpdate;
        Marketplace.Global_Updator += Update;
    }

    private static void OnBankerUpdate()
    {
        Banker_UI.Reload();
    }
    
    private static void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape) || !Banker_UI.IsPanelVisible()) return;
        Banker_UI.Hide();
        Menu.instance.OnClose();
    }

    
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    [ClientOnlyPatch]
    private static class ZrouteMethodsClientBanker
    {
        private static void Postfix()
        {
            ZRoutedRpc.instance.Register("KGmarket GetBankerClientData", new Action<long, ZPackage>(GetBankerClientData));
        }
    }
    
    private static void GetBankerClientData(long sender, ZPackage pkg)
    {
        pkg.Decompress();
        Banker_DataTypes.BankerClientData.Clear();
        Banker_DataTypes.BankerClientData.AddRange(JSON.ToObject<Dictionary<int, int>>(pkg.ReadString()));
        Banker_UI.Reload();
    }
    
    
}
namespace Marketplace.Modules.ServerInfo;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Normal, "OnInit")]
public static class ServerInfo_Main_Client
{
    private static void OnInit()
    {
        ServerInfo_UI.Init();
        ServerInfo_DataTypes.ServerInfoData.ValueChanged += OnInfoUpdate;
        Marketplace.Global_Updator += Update;
    }
    
    private static void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape) || !ServerInfo_UI.IsPanelVisible()) return;
        ServerInfo_UI.Hide();
        Menu.instance.OnClose();
    }

    private static void OnInfoUpdate()
    {
        if (ServerInfo_UI.IsPanelVisible()) ServerInfo_UI.Reload();
    }
}
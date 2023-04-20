namespace Marketplace.Modules.DistancedUI;

[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Normal, "OnInit")]
public static class DistancedUI_Main_Client
{
    private static void OnInit()
    {
        DistancedUI_UI.Init();
        DistancedUI_DataType.CurrentPremiumSystemData.ValueChanged += OnPremiumSystemUpdator; 
        Marketplace.Global_Updator += Update;
    }

    private static void OnPremiumSystemUpdator()
    {
        DistancedUI_DataType.CurrentPremiumSystemData.Value.isAllowed = DistancedUI_DataType.CurrentPremiumSystemData.Value.Users.Contains(Global_Values._localUserID) || DistancedUI_DataType.CurrentPremiumSystemData.Value.EveryoneIsVIP;
        Utils.print($"Got Premium System data. Am i premium? : {DistancedUI_DataType.CurrentPremiumSystemData.Value.isAllowed}");
        if (DistancedUI_DataType.CurrentPremiumSystemData.Value.isAllowed)
        {
            DistancedUI_UI.Show(DistancedUI_DataType.CurrentPremiumSystemData.Value.MarketplaceEnabled);
        }
        else
        {
            DistancedUI_UI.Hide();
        }
    }
    
    
    private static void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && DistancedUI_UI.IsViewProfilesVisible())
        {
            DistancedUI_UI.HideViewProfiles();
            Menu.instance.OnClose();
        }

        if (!Player.m_localPlayer) return;
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.BackQuote))
        {
            DistancedUI_UI.ClickView();
        }
    }
    
    
}
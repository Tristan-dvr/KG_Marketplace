namespace Marketplace.Modules.Feedback;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Normal)]
public static class Feedback_Main_Client
{
    private static void OnInit()
    {
        Feedback_UI.Init();
        Marketplace.Global_Updator += Update;
    }
    
    private static void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape) || !Feedback_UI.IsPanelVisible()) return;
        Feedback_UI.Hide();
        Menu.instance.OnClose();
    }

}
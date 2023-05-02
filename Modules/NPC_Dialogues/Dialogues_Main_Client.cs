namespace Marketplace.Modules.NPC_Dialogues;

[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Normal, "OnInit")]
public static class Dialogues_Main_Client
{
    private static void OnInit()
    {
        Dialogues_UI.Init();
        Dialogues_DataTypes.SyncedDialoguesData.ValueChanged += InitDialogues;
        Marketplace.Global_Updator += Update;
    }

    private static void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape) || !Dialogues_UI.IsVisible()) return;
        Dialogues_UI.Hide();
        Menu.instance.OnClose();
    }
    
    [HarmonyPatch(typeof(ZNetScene),nameof(ZNetScene.Awake))]
    [ClientOnlyPatch]
    private static class ZNetScene_Awake_Patch
    {
        private static void Postfix() => InitDialogues();
    }
    
    private static void InitDialogues()
    {
        if(!ZNetScene.instance) return;
        Dialogues_DataTypes.ClientReadyDialogues.Clear();
        foreach (Dialogues_DataTypes.RawDialogue dialogue in Dialogues_DataTypes.SyncedDialoguesData.Value)
        {
            Dialogues_DataTypes.ClientReadyDialogues[dialogue.UID] = dialogue;
        }
    }
}
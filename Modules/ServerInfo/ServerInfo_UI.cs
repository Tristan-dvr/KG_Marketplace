namespace Marketplace.Modules.ServerInfo;

public static class ServerInfo_UI
{
    private static GameObject UI;
    private static Text NPCname;
    private static Text InfoText;
    private static Scrollbar Scrollbar;
    private static string CurrentProfile;
    private static List<ContentSizeFitter> AllFilters;

    public static bool IsPanelVisible()
    {
        return UI && UI.activeSelf;
    }

    public static void Init()
    {
        UI = UnityEngine.Object.Instantiate(
            AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("MarketplaceInfoNewUI"));
        UnityEngine.Object.DontDestroyOnLoad(UI);
        NPCname = UI.transform.Find("Canvas/Header/Text").GetComponent<Text>();
        InfoText = UI.transform.Find("Canvas/Scroll View/Viewport/Content/Text").GetComponent<Text>();
        Scrollbar = UI.GetComponentInChildren<Scrollbar>();
        AllFilters = UI.GetComponentsInChildren<ContentSizeFitter>().ToList();
        UI.SetActive(false);
    }

    public static void OnInfoUpdate()
    {
        if (IsPanelVisible()) Reload();
    }

    public static void Reload()
    {
        if (!ServerInfo_DataTypes.ServerInfoData.Value.ContainsKey(CurrentProfile))
        {
            Hide();
            return;
        }

        InfoText.text = ServerInfo_DataTypes.ServerInfoData.Value[CurrentProfile];
        Canvas.ForceUpdateCanvases();
        AllFilters.ForEach(filter => filter.enabled = false);
        AllFilters.ForEach(filter => filter.enabled = true);
    }

    public static void Hide()
    {
        InfoText.text = "";
        UI.SetActive(false);
    }


    public static void Show(string profile, string _npcName)
    {
        if (!ServerInfo_DataTypes.ServerInfoData.Value.ContainsKey(profile)) return;
        CurrentProfile = profile;
        UI.SetActive(true);
        _npcName = Utils.RichTextFormatting(_npcName);
        NPCname.text = string.IsNullOrEmpty(_npcName) ? Localization.instance.Localize("$mpasn_Info") : _npcName;
        InfoText.text = ServerInfo_DataTypes.ServerInfoData.Value[profile];
        Scrollbar.value = 1;
        Canvas.ForceUpdateCanvases();
        AllFilters.ForEach(filter => filter.enabled = false);
        AllFilters.ForEach(filter => filter.enabled = true);
    }
    
    [HarmonyPatch(typeof(Menu), nameof(Menu.IsVisible))]
    [ClientOnlyPatch]
    private static class InfoUIFix
    {
        private static void Postfix(ref bool __result)
        {
            if (IsPanelVisible()) __result = true;
        }
    }
}
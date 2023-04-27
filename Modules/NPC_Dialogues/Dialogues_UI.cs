using Marketplace.Modules.NPC;

namespace Marketplace.Modules.NPC_Dialogues;

public static class Dialogues_UI
{
    private static GameObject UI;
    private static GameObject Dialogue_Element;
    private static readonly List<GameObject> Elements = new();
    private static Text NPC_Name;
    private static Text Dialogue_Text;
    private static Transform Content;

    public static bool IsVisible()
    {
        return UI && UI.activeSelf;
    }

    private static void ResetFitters()
    {
        Canvas.ForceUpdateCanvases();
        foreach (var fitter in UI.GetComponentsInChildren<ContentSizeFitter>())
        {
            fitter.enabled = false;
            fitter.enabled = true;
        }
    }

    public static void Init()
    {
        UI = UnityEngine.Object.Instantiate(AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("NPCDialogueUI"));
        Dialogue_Element = AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("Dialogue_Option");
        UnityEngine.Object.DontDestroyOnLoad(UI);
        UI.SetActive(false);

        NPC_Name = UI.transform.Find("GO/NPC_Name").GetComponent<Text>();
        Dialogue_Text = UI.transform.Find("GO/Dialogue_Text").GetComponent<Text>();
        Content = UI.transform.Find("GO");
        Default();
    }

    private static void Default()
    {
        foreach (var element in Elements)
        {
            UnityEngine.Object.Destroy(element);
        }

        Elements.Clear();
        NPC_Name.text = "";
        Dialogue_Text.text = "";
    }

    public static void LoadDialogue(Market_NPC.NPCcomponent npc, string UID)
    {
        Default();
        if (!Dialogues_DataTypes.ClientReadyDialogues.TryGetValue(UID, out var dialogue))
        {
            Hide();
            return;
        }

        UI.SetActive(true);
        string name = npc.GetNPCName();
        if (string.IsNullOrWhiteSpace(name))
        {
            name = Localization.instance.Localize("$mpasn_" + npc._currentNpcType);
        }

        NPC_Name.text = name;
        Dialogue_Text.text = dialogue.Text;
        int c = 0;
        foreach (var option in dialogue.Options)
        {
            var element = UnityEngine.Object.Instantiate(Dialogue_Element, Content);
            element.transform.Find("Text").GetComponent<Text>().text = "\"" + option.Text + "\"";
            element.GetComponent<Button>().onClick.AddListener(() =>
            {
                AssetStorage.AssetStorage.AUsrc.Play();
                if (!npc || !Player.m_localPlayer) return;
                option.Command?.Invoke(npc);
                if (!string.IsNullOrWhiteSpace(option.NextUID))
                {
                    LoadDialogue(npc, option.NextUID);
                }
                else
                {
                    Hide();
                }
            });
            element.transform.Find("Indexer/Text").GetComponent<Text>().text = (++c).ToString();
            if (option.Icon != null)
                element.transform.Find("Text/Icon").GetComponent<Image>().sprite = option.Icon;
            Elements.Add(element);
        }

        ResetFitters();
    }

    public static void Hide()
    {
        UI.SetActive(false);
    }

    [HarmonyPatch(typeof(Menu), nameof(Menu.IsVisible))]
    [ClientOnlyPatch]
    private static class BankerUIFix
    {
        private static void Postfix(ref bool __result)
        {
            if (IsVisible()) __result = true;
        }
    }
}
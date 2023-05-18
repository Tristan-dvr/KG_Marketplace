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


    private static CanvasGroup CanvasAlpha;
    private static readonly Dictionary<int, Action> HotbarActions = new();

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
        CanvasAlpha = UI.GetComponent<CanvasGroup>();
        Dialogue_Element = AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("Dialogue_Option");
        UnityEngine.Object.DontDestroyOnLoad(UI);
        UI.SetActive(false);

        NPC_Name = UI.transform.Find("GO/NPC_Name").GetComponent<Text>();
        Dialogue_Text = UI.transform.Find("GO/Dialogue_Text").GetComponent<Text>();
        Content = UI.transform.Find("GO");
        Default();
        Marketplace.Global_Updator += PressHotbarUpdate;
    }

    private static readonly KeyCode[] hotbarKeys =
    {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6,
        KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0
    };

    private static void PressHotbarUpdate()
    {
        if (!IsVisible() || !Player.m_localPlayer) return;
        for (int i = 0; i < hotbarKeys.Length; ++i)
        {
            if (!Input.GetKeyDown(hotbarKeys[i])) continue;
            if (HotbarActions.TryGetValue(i + 1, out var action))
            {
                action();
            }
        }
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
        HotbarActions.Clear();
    }


    private static Coroutine FadeCoroutine;

    private enum Fade
    {
        Show,
        Hide
    }

    private static void SmoothAlpha(Fade type, float time)
    {
        if (FadeCoroutine != null)
            Marketplace._thistype.StopCoroutine(FadeCoroutine);
        FadeCoroutine = Marketplace._thistype.StartCoroutine(SmoothAlphaCoroutine(type, time));
    }

    private static IEnumerator SmoothAlphaCoroutine(Fade type, float time)
    {
        float start = type == Fade.Show ? 0 : 1;
        CanvasAlpha.alpha = start;
        float target = type == Fade.Show ? 1 : 0;
        float counter = 0;
        const float exponent = 2f;
        while (counter <= 1f)
        {
            if (!UI) yield break;
            counter += Time.unscaledDeltaTime / time;
            float easedCounter = counter < 1 ? 1 - Mathf.Pow(1 - counter, exponent) : 1;
            CanvasAlpha.alpha = Mathf.Clamp01(Mathf.Lerp(start, target, easedCounter));
            yield return null;
        }
    }

    public static bool LoadDialogue(Market_NPC.NPCcomponent npc, string UID)
    {
        Default();
        if (!Dialogues_DataTypes.ClientReadyDialogues.TryGetValue(UID, out var dialogue))
        {
            Hide(true);
            return false;
        }

        UI.SetActive(true);
        SmoothAlpha(Fade.Show, 0.5f);

        string name = npc.GetNPCName();
        if (string.IsNullOrWhiteSpace(name))
        {
            name = Localization.instance.Localize("$mpasn_" + npc._currentNpcType);
        }

        NPC_Name.text = name;
        Dialogue_Text.text = "\"" + Localization.instance.Localize(dialogue.Text) + "\"";
        int c = 0;
        foreach (var option in dialogue.Options)
        {
            bool alwaysVisibleCheck = false;
            if (!option.CheckCondition(out string reason))
            {
                if (option.AlwaysVisible)
                    alwaysVisibleCheck = true;
                else
                    continue;
            }

            var element = UnityEngine.Object.Instantiate(Dialogue_Element, Content);
            Elements.Add(element);
            element.transform.Find("Text").GetComponent<Text>().text = Localization.instance.Localize(option.Text);
            element.transform.Find("Indexer/Text").GetComponent<Text>().text = (++c).ToString();
            if (option.Icon != null)
                element.transform.Find("Text/Icon").GetComponent<Image>().sprite = option.Icon;
            element.GetComponent<Image>().color = option.Color;
            element.transform.Find("Indexer").GetComponent<Image>().color = option.Color;

            if (alwaysVisibleCheck)
            {
                UnityEngine.Object.Destroy(element.GetComponent<Button>());
                element.GetComponent<Image>().color = Color.red;
                element.transform.Find("Indexer").GetComponent<Image>().color = Color.red;
                if (!string.IsNullOrWhiteSpace(reason))
                    element.transform.Find("Text").GetComponent<Text>().text +=
                        $"\n<color=red>[</color>{reason}<color=red>]</color>";
                continue;
            }

            void OnClick()
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
                    Hide(true);
                }
            }

            HotbarActions[c] = OnClick;
            element.GetComponent<Button>().onClick.AddListener(OnClick);
        }

        ResetFitters();
        return true;
    }

    private static IEnumerator HideCoroutine()
    {
        yield return null;
        Hide();
    }

    public static void Hide(bool nextFrame = false)
    {
        if (!IsVisible()) return;
        if (nextFrame)
        {
            Marketplace._thistype.StartCoroutine(HideCoroutine());
            return;
        }

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

    [HarmonyPatch(typeof(Player), nameof(Player.OnDeath))]
    [ClientOnlyPatch]
    private static class Player_OnDeath_Patch
    {
        private static void Prefix()
        {
            Hide();
        }
    }
}
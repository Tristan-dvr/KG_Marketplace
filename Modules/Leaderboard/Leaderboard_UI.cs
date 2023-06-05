namespace Marketplace.Modules.Leaderboard;

using Object = UnityEngine.Object;

public static class Leaderboard_UI
{
    private static GameObject UI;
    private static SortBy CurrentSort;
    private static GameObject Element;
    private static Transform Content;
    private static GameObject Achievements;
    private static GameObject AchievementsElement;
    private static Transform AchievementContent;
    private static Text PageText;

    public static bool IsVisible() => UI && UI.activeSelf;

    private enum SortBy
    {
        CreaturesKilled,
        BuiltStructures,
        ItemsCrafted,
        Died,
        MapExplored,
        TotalAchievements
    }

    private static readonly Dictionary<SortBy, Button> SortButtons = new();

    private static int CurrentPage = 1;
    private const int MaxPerPage = 10;
    private static GameObject CurrentSelectedElement = null;
    private static List<Leaderboard_DataTypes.Client_Leaderboard> SortedList = new();

    public static void Init()
    {
        UI = Object.Instantiate(AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("MarketplaceLeaderboardUI"));
        Element = AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("MarketplaceLB_PlayerElement");
        Object.DontDestroyOnLoad(UI);
        Transform buttonsTransform = UI.transform.Find("GO/Header_Buttons");
        SortButtons.Add(SortBy.CreaturesKilled, buttonsTransform.Find("CreaturesKilled").GetComponent<Button>());
        SortButtons.Add(SortBy.BuiltStructures, buttonsTransform.Find("BuiltStructures").GetComponent<Button>());
        SortButtons.Add(SortBy.ItemsCrafted, buttonsTransform.Find("ItemsCrafted").GetComponent<Button>());
        SortButtons.Add(SortBy.Died, buttonsTransform.Find("Died").GetComponent<Button>());
        SortButtons.Add(SortBy.MapExplored, buttonsTransform.Find("MapExplored").GetComponent<Button>());
        SortButtons.Add(SortBy.TotalAchievements, buttonsTransform.Find("TotalAchievements").GetComponent<Button>());
        foreach (var button in SortButtons)
            button.Value.onClick.AddListener(() =>
            {
                AssetStorage.AssetStorage.AUsrc.Play();
                SetSortBy(button.Key);
                CreateElements();
            });
        Content = UI.transform.Find("GO/LB");
        PageText = UI.transform.Find("GO/Page/Text").GetComponent<Text>();
        UI.transform.Find("GO/PageRight").GetComponent<Button>().onClick.AddListener(NextPage);
        UI.transform.Find("GO/PageLeft").GetComponent<Button>().onClick.AddListener(PrevPage);
        Achievements = UI.transform.Find("Achievements").gameObject;
        AchievementsElement = AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("Marketplace_Achievement_Element");
        AchievementContent = Achievements.transform.Find("Scroll Rect/Viewport/Content");
        UI.SetActive(false);
        SetSortBy(SortBy.TotalAchievements);
    }

    private static void NextPage()
    {
        AssetStorage.AssetStorage.AUsrc.Play();
        int maxPage = Mathf.CeilToInt(SortedList.Count / (float)MaxPerPage);
        if (CurrentPage + 1 > maxPage) return;
        CurrentPage++;
        PageText.text = $"{CurrentPage}/{maxPage}";
        CreateElements();
    }

    private static void PrevPage()
    {
        AssetStorage.AssetStorage.AUsrc.Play();
        if (CurrentPage - 1 < 1) return;
        CurrentPage--;
        int maxPage = Mathf.CeilToInt(SortedList.Count / (float)MaxPerPage);
        PageText.text = $"{CurrentPage}/{maxPage}";
        CreateElements();
    }

    private static void SetSortBy(SortBy sort)
    {
        CurrentSort = sort;
        foreach (var button in SortButtons)
            button.Value.GetComponent<Text>().color = Color.yellow;
        SortButtons[sort].GetComponent<Text>().color = Color.green;

        List<Leaderboard_DataTypes.Client_Leaderboard> valuesOnly =
            Leaderboard_DataTypes.SyncedClientLeaderboard.Value.Select(x => x.Value).ToList();
        SortedList = sort switch
        {
            SortBy.CreaturesKilled => valuesOnly.OrderByDescending(x => x.KilledCreatures).ToList(),
            SortBy.BuiltStructures => valuesOnly.OrderByDescending(x => x.BuiltStructures).ToList(),
            SortBy.ItemsCrafted => valuesOnly.OrderByDescending(x => x.ItemsCrafted).ToList(),
            SortBy.Died => valuesOnly.OrderByDescending(x => x.Died).ToList(),
            SortBy.MapExplored => valuesOnly.OrderByDescending(x => x.MapExplored).ToList(),
            SortBy.TotalAchievements => valuesOnly.OrderByDescending(x => x.Titles.Count).ToList(),
        };
        int maxPage = Mathf.CeilToInt(SortedList.Count / (float)MaxPerPage);
        PageText.text = $"{CurrentPage}/{maxPage}";
    }

    private static void Default()
    {
        Achievements.SetActive(false);
        CurrentPage = 1;
        CurrentSelectedElement = null;
        SetSortBy(SortBy.TotalAchievements);
    } 

    private static void CreateElements()
    {
        foreach (Transform child in Content)
            Object.Destroy(child.gameObject);
        Achievements.SetActive(false);
        CurrentSelectedElement = null;
        int start = (CurrentPage - 1) * MaxPerPage;
        int end = start + MaxPerPage;
        for (int i = start; i < end; ++i)
        {
            if (i >= SortedList.Count) break;
            var element = Object.Instantiate(Element, Content);
            var data = SortedList[i];

            var rankTransform = element.transform.Find("Rank");
            var cstring = (i + 1).ToString();
            if (i + 1 <= 3)
                rankTransform.Find(cstring).gameObject.SetActive(true);
            else
                rankTransform.Find("etc").gameObject.SetActive(true);
            rankTransform.Find("Text").GetComponent<Text>().text = cstring;

            element.transform.Find("PlayerName").GetComponent<Text>().text = data.PlayerName;
            element.transform.Find("CreaturesKilled").GetComponent<Text>().text = data.KilledCreatures.ToString();
            element.transform.Find("BuiltStructures").GetComponent<Text>().text = data.BuiltStructures.ToString();
            element.transform.Find("ItemsCrafted").GetComponent<Text>().text = data.ItemsCrafted.ToString();
            element.transform.Find("Died").GetComponent<Text>().text = data.Died.ToString();
            element.transform.Find("MapExplored").GetComponent<Text>().text = data.MapExplored + "%";
            element.transform.Find("TotalAchievements").GetComponent<Text>().text = data.Titles.Sum(x => x.Score).ToString();

            element.transform.Find(CurrentSort.ToString()).GetComponent<Text>().color = Color.green;

            element.GetComponent<UIInputHandler>().m_onPointerEnter += (_) => OnHoverStart(element);
            element.GetComponent<UIInputHandler>().m_onPointerExit += (_) => OnHoverEnd();
            element.GetComponent<UIInputHandler>().m_onLeftClick += (_) => OnElementClick(element, data);
        }
    }

    private static void OnHoverStart(GameObject go)
    {
        foreach (Transform child in Content)
            if (child.gameObject != CurrentSelectedElement)
                child.Find("fill").GetComponent<Image>().color = Color.white;
        go.transform.Find("fill").GetComponent<Image>().color = new Color(0.44f, 1f, 0.51f);
    }

    private static void OnHoverEnd()
    {
        foreach (Transform child in Content)
            if (child.gameObject != CurrentSelectedElement)
                child.Find("fill").GetComponent<Image>().color = Color.white;
    }
    
    private static void OnElementClick(GameObject go, Leaderboard_DataTypes.Client_Leaderboard board)
    {
        AssetStorage.AssetStorage.AUsrc.Play();
        if (CurrentSelectedElement == go)
        {
            CurrentSelectedElement.transform.Find("fill").GetComponent<Image>().color = Color.white;
            CurrentSelectedElement = null;
            Achievements.SetActive(false);
            return;
        }
        if (CurrentSelectedElement != null)
            CurrentSelectedElement.transform.Find("fill").GetComponent<Image>().color = Color.white;
        CurrentSelectedElement = go;
        go.transform.Find("fill").GetComponent<Image>().color = Color.green;
        InitAchievements(board);
    }

    private static void InitAchievements(Leaderboard_DataTypes.Client_Leaderboard board)
    {
        Achievements.SetActive(true);
        foreach (Transform child in AchievementContent)
            Object.Destroy(child.gameObject);
        Achievements.transform.Find("Achievements").GetComponent<Text>().text = board.PlayerName + "\nAchievements";
        foreach (var title in board.Titles)
        {
            var element = Object.Instantiate(AchievementsElement, AchievementContent);
            element.GetComponent<Image>().color = new Color(title.Color.r, title.Color.g, title.Color.b, 0.2f);
            element.transform.Find("color").GetComponent<Image>().color = title.Color;
            element.transform.Find("Name").GetComponent<Text>().text = title.Name;
            element.transform.Find("Name").GetComponent<Text>().color = title.Color;
            element.transform.Find("Description").GetComponent<Text>().text = "(" + title.Description + ")";
        }
    }
    
    public static void Show()
    {
        UI.SetActive(true);
        Default();
        CreateElements();
    }

    public static void Hide()
    {
        UI.SetActive(false);
    }


    [HarmonyPatch(typeof(Menu), nameof(Menu.IsVisible))]
    [ClientOnlyPatch]
    private static class Menu_IsVisible_Patch
    {
        private static void Postfix(ref bool __result) => __result |= IsVisible();
    }
}
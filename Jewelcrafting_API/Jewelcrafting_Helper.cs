using ItemDataManager;


namespace Marketplace.Jewelcrafting_API;

[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Last, "OnInit")]
public static class Jewelcrafting_Helper
{
    private const string JEWELCRAFTING_MAIN_KEY = "org.bepinex.plugins.jewelcrafting";
    private const string JEWELCRAFTING_ADDITIONAL_KEY = "Jewelcrafting.Sockets";
    private static readonly Dictionary<int, List<string>> GemsByTier = new();

    public record struct Range(int min, int max);

    private static readonly HashSet<string> Exclusions = new()
    {
        "Boss_Crystal_", "EikthyrGem", "ElderGem", "BonemassGem", "ModerGem", "YagluthGem", "TheQueenGem"
    };

    private static void OnInit()
    {
        Type GemStoneSetup = Type.GetType("Jewelcrafting.GemStoneSetup, Jewelcrafting");
        object Gems = GemStoneSetup?.GetField("Gems", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);
        if (Gems == null) return;
        foreach (object gemInfo in (IEnumerable)Gems)
        {
            PropertyInfo P_value = gemInfo.GetType().GetProperty("Value");
            IList value = (IList)P_value?.GetValue(gemInfo);
            for (int i = 0; i < value!.Count; ++i)
            {
                object gem = value[i];
                int tier = i + 1;
                string prefab = ((GameObject)gem.GetType().GetField("Prefab").GetValue(gem)).name;
                if (Exclusions.Any(x => prefab.Contains(x))) continue;
                if (!GemsByTier.ContainsKey(tier))
                    GemsByTier.Add(tier, new List<string>());
                GemsByTier[tier].Add(prefab);
            }
        }
    }

    private static string GetRandomGem(int maxTier = 0)
    {
        if (maxTier == 0)
            maxTier = GemsByTier.Keys.Max();
        int tier = UnityEngine.Random.Range(1, maxTier + 1);
        return GemsByTier[tier][UnityEngine.Random.Range(0, GemsByTier[tier].Count)];
    }

    private static void LoadDataIntoJC(ItemDrop.ItemData item, string newData)
    {
        var foreignData = item.Data(JEWELCRAFTING_MAIN_KEY)!.foreignItemInfo;
        var dataDictionary = AccessTools.Field(foreignData.GetType(), "data").GetValue(foreignData);
        foreach (var n in (IEnumerable)dataDictionary)
        {
            var P_key = n.GetType().GetProperty("Key");
            string key = (string)P_key!.GetValue(n);
            if (key != JEWELCRAFTING_ADDITIONAL_KEY) continue;
            var P_value = n.GetType().GetProperty("Value");
            object val = P_value!.GetValue(n);
            MethodInfo P_data = AccessTools.PropertySetter(val.GetType(), "Value");
            P_data.Invoke(val, new object[] { newData });
            AccessTools.Method(val.GetType(), "Load").Invoke(val, Array.Empty<object>());
            return;
        }
    }

    private static void AddGemsToItem(ItemDrop.ItemData item, params string[] gems)
    {
        if (gems == null || gems.Length == 0)
            return;
        bool isGemValid(string gem) => GemsByTier.Values.Any(tier => tier.Contains(gem));
        int counter = 0;
        const int maxGems = 4;
        string result = "";
        foreach (string gem in gems)
        {
            result += isGemValid(gem) ? gem + "," : "";
            if (++counter >= maxGems)
                break;
        }

        LoadDataIntoJC(item, result.TrimEnd(','));
    }

    public static void AddRandomGemsToItem(ItemDrop.ItemData item, int maxTier, Range maxGemsRange)
    {
        int maxGems = UnityEngine.Random.Range(maxGemsRange.min, maxGemsRange.max + 1);
        maxGems = Mathf.Clamp(maxGems, 1, 4);
        string[] gems = new string[maxGems];
        for (int i = 0; i < maxGems; ++i)
            gems[i] = GetRandomGem(maxTier);
        AddGemsToItem(item, gems);
    }
}
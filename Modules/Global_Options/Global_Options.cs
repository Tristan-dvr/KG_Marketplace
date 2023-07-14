using BepInEx.Configuration;
using Marketplace.Paths;

namespace Marketplace;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Normal, "OnInit", new[] { "MarketPlace.cfg" },
    new[] { "OnChange" })]
public static class Global_Values
{
    public static string CurrencyName =>
        ObjectDB.instance?.GetItemPrefab(_container.Value._serverCurrency.GetStableHashCode()) is { } item
            ? item.GetComponent<ItemDrop>().m_itemData.m_shared.m_name
            : "$item_coins";

    public static string _localUserID = "none";

    [HarmonyPatch]
    private static class Game_Awake_Patch
    {
        public static IEnumerable<MethodInfo> TargetMethods()
        {
            yield return AccessTools.Method(typeof(ZNet), nameof(ZNet.Start));
            yield return AccessTools.Method(typeof(FejdStartup), nameof(FejdStartup.Awake));
        }

        private static void Postfix()
        {
            Utils.print($"Searching User ID...", ConsoleColor.Cyan);
            _localUserID = ZNet.m_onlineBackend == OnlineBackendType.Steamworks
                ? PrivilegeManager.GetNetworkUserId().Split('_')[1]
                : PrivilegeManager.GetNetworkUserId();
            if (string.IsNullOrWhiteSpace(_localUserID))
            {
                _localUserID = "ERROR";
            }

            Utils.print($"User ID found: {_localUserID}", ConsoleColor.Cyan);
        }
    }

    internal static readonly CustomSyncedValue<Container> _container = new(Marketplace.configSync, "Global_Values",
        new Container());

    public class Container : ISerializableParameter
    {
        public string _blockedPrefabsServer = "";
        public string _blockedPlayerList = "";
        public string _vipPlayerList = "";
        public string _serverCurrency = "Coins";
        public int _itemMarketLimit;
        public int _marketTaxes;
        public bool _canTeleportWithOre;
        public int _vipmarketTaxes;
        public bool _gamblerEnableNotifications;
        public bool _allowMultipleQuestScore;
        public int _maxAcceptedQuests;
        public bool _hideOtherQuestRequirementQuests;
        public bool _allowKillQuestsInParty;
        public bool _enableKGChat;
        public string _mailPostRecipe = "";
        public int _mailPostWaitTime;
        public string _mailPostExcludeItems = "";
        public string _pieceSaverRecipe = "";
        public bool _useLeaderboard;
        public bool _rebuildHeightmap;


        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write(_blockedPrefabsServer ?? "");
            pkg.Write(_blockedPlayerList ?? "");
            pkg.Write(_itemMarketLimit);
            pkg.Write(_marketTaxes);
            pkg.Write(_canTeleportWithOre);
            pkg.Write(_vipmarketTaxes);
            pkg.Write(_vipPlayerList ?? "");
            pkg.Write(_serverCurrency ?? "");
            pkg.Write(_gamblerEnableNotifications);
            pkg.Write(_allowMultipleQuestScore);
            pkg.Write(_maxAcceptedQuests);
            pkg.Write(_hideOtherQuestRequirementQuests);
            pkg.Write(_allowKillQuestsInParty);
            pkg.Write(_enableKGChat);
            pkg.Write( _mailPostRecipe ?? "");
            pkg.Write(_mailPostWaitTime);
            pkg.Write(_mailPostExcludeItems ?? "");
            pkg.Write(_pieceSaverRecipe ?? "");
            pkg.Write(_useLeaderboard);
            pkg.Write(_rebuildHeightmap);
        }

        public void Deserialize(ref ZPackage pkg)
        {
            _blockedPrefabsServer = pkg.ReadString();
            _blockedPlayerList = pkg.ReadString();
            _itemMarketLimit = pkg.ReadInt();
            _marketTaxes = pkg.ReadInt();
            _canTeleportWithOre = pkg.ReadBool();
            _vipmarketTaxes = pkg.ReadInt();
            _vipPlayerList = pkg.ReadString();
            _serverCurrency = pkg.ReadString();
            _gamblerEnableNotifications = pkg.ReadBool();
            _allowMultipleQuestScore = pkg.ReadBool();
            _maxAcceptedQuests = pkg.ReadInt();
            _hideOtherQuestRequirementQuests = pkg.ReadBool();
            _allowKillQuestsInParty = pkg.ReadBool();
            _enableKGChat = pkg.ReadBool();
            _mailPostRecipe = pkg.ReadString();
            _mailPostWaitTime = pkg.ReadInt();
            _mailPostExcludeItems = pkg.ReadString();
            _pieceSaverRecipe = pkg.ReadString();
            _useLeaderboard = pkg.ReadBool();
            _rebuildHeightmap = pkg.ReadBool();
        }
    }


    //local server
    public static string WebHookLink;
    public static int BankerIncomeTime;
    public static float BankerIncomeMultiplier;
    public static float BankerVIPIncomeMultiplier;
    public static bool EnableTraderLog;
    public static bool EnableTransmogLog;
    public static string BankerInterestItems = "All";


    private static ConfigFile _config;


    private static void OnInit()
    {
        _config = new ConfigFile(Market_Paths.MainConfig, true);
        ReadConfigValues();
    }

    private static void OnChange()
    {
        Utils.DelayReloadConfig(_config, ReadConfigValues);
        Utils.print("Main Configs Changed. Sending new info to all clients");
    }

    private static T SearchOption<T>(string option, T defaultValue, string description)
    {
        T value = _config.Bind("Main", option, defaultValue, description).Value;
        return value;
    }

    private static void ReadConfigValues()
    {
        EnableTransmogLog = SearchOption("EnableTransmogLog", false, "Enable/Disable Transmog Log");
        EnableTraderLog = SearchOption("EnableTraderLog", false, "Enable/Disable Trader Log");
        BankerIncomeTime = SearchOption("BankerIncomeTime", 1, "Banker Income Time (hours)");
        BankerIncomeMultiplier = SearchOption("BankerIncomeMultiplier", 0f, "Banker Income Multiplier (per time)");
        BankerVIPIncomeMultiplier = SearchOption("BankerVIPIncomeMultiplier", 0f, "VIP Banker Income Multiplier");
        WebHookLink = SearchOption("FeedbackWebhookLink", "webhook link", "Feedback Webhook Link");
        BankerInterestItems = SearchOption("BankerInterestItems", "All", "Banker Interest Items").Replace(" ","");

        _container.Value._itemMarketLimit =
            SearchOption("ItemMarketLimit", 15, "Limit amount of slots player can sell in marketpalce");
        _container.Value._blockedPlayerList = SearchOption("BlockedPlayers", "User IDs", "Marketplace Blocked Players");
        _container.Value._vipPlayerList = SearchOption("VIPplayersList", "User IDs", "Marketplace VIP Players List ");
        _container.Value._marketTaxes = SearchOption("MarketTaxes", 0, "Market Taxes From Each Sell");
        _container.Value._vipmarketTaxes = SearchOption("VIPplayersTaxes", 0, "VIP Player Market Taxes");
        _container.Value._marketTaxes = Mathf.Clamp(_container.Value._marketTaxes, 0, 100);
        _container.Value._vipmarketTaxes = Mathf.Clamp(_container.Value._vipmarketTaxes, 0, 100);
        _container.Value._canTeleportWithOre =
            SearchOption("CanTeleportWithOre", true, "Enable/Disable players teleporter with ore");
        _container.Value._blockedPrefabsServer = SearchOption("MarketSellBlockedPrefabs", "Coins, SwordCheat",
            "Marketplace Blocked Prefabs For Selling");
        _container.Value._serverCurrency =
            SearchOption("ServerCurrency", "Coins", "Prefab Of Server Currency (marketplace)");
        _container.Value._gamblerEnableNotifications =
            SearchOption("GamblerEnableWinNotifications", false, "Enable Gambler Win Notification");
        _container.Value._allowMultipleQuestScore = SearchOption("AllowMultipleQuestsScore", false,
            "Enable Kill / Harvest Craft Same Target Quests Get + Score In Same Time");
        _container.Value._maxAcceptedQuests = SearchOption("MaxAcceptedQuests", 7, "Max Amount Of Accpeted Quests");
        _container.Value._hideOtherQuestRequirementQuests = SearchOption("HideOtherQuestRequirementQuests", false,
            "Hide Quest in UI if they have OtherQuest as requirement");
        _container.Value._allowKillQuestsInParty =
            SearchOption("AllowKillQuestsInParty", true, "Allow Kill Quests In Party");
        _container.Value._enableKGChat = SearchOption("EnableKGChat", true, "Enable KGChat");
        _container.Value._mailPostRecipe = SearchOption("MailPostRecipe", "SwordCheat,1", "Recipe for Mailpost creation");
        _container.Value._mailPostWaitTime = SearchOption("MailPostWaitTime", 5, "Mailpost wait time (minutes)");
        _container.Value._mailPostExcludeItems = SearchOption("MailPostExcludeItems", "Items Here", "Mailpost exclude items (with coma)");
        _container.Value._pieceSaverRecipe = SearchOption("PieceSaverRecipe", "SwordCheat,1", "Recipe for Piece Saver Crystal creation");
        _container.Value._useLeaderboard = SearchOption("UseLeaderboard", false, "Use Leaderboard");
        _container.Value._rebuildHeightmap = SearchOption("RebuildHeightmap", false, "Rebuild Heightmap On Territory Change");
        _container.Update();
    }


    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    [ServerOnlyPatch]
    private static class OBJECTPATCH
    {
        private static void Postfix()
        {
            if (!ZNet.instance.IsServer()) return;
            ItemDrop currencyItem = ZNetScene.instance.GetPrefab(_container.Value._serverCurrency)
                ?.GetComponent<ItemDrop>();
            if (currencyItem)
            {
                Utils.print($"New server currency accepted, Currency Name: {CurrencyName}");
            }
            else
            {
                Utils.print("Can't accept new currency so changing that to Default: Coins, $item_coins",
                    ConsoleColor.Red);
                _container.Value._serverCurrency = "Coins";
            }

            _container.Update();
        }
    }
}
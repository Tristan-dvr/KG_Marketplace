namespace Marketplace.Paths;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Both, Market_Autoload.Priority.Init)]
public static class Market_Paths
{
    public static string MainPath => Path.Combine(BepInEx.Paths.ConfigPath, "Marketplace");
    public static string NPC_SoundsPath => Path.Combine(BepInEx.Paths.ConfigPath, "MarketplaceNPC_Sounds");
    public static string NPC_Saved => Path.Combine(BepInEx.Paths.ConfigPath, "MarketplaceNPC_Saved");
    public static string CachedImagesFolder => Path.Combine(BepInEx.Paths.ConfigPath, "MarketplaceCachedImages");
    public static string DiscordStuffFolder => Path.Combine(MainPath, "DiscordWebhooks");
    

    private static string DataFolder => Path.Combine(MainPath, "DO NOT TOUCH");

    public static string BankerDataJSONFile => Path.Combine(DataFolder, "BankerData.json");

    public static string LoggerPath => Path.Combine(MainPath, "Logger.log");

    public static string ServerMarketDataJSON => Path.Combine(DataFolder, "MarketplaceData.json");
    public static string MarketPlayersIncomeJSON => Path.Combine(DataFolder, "PlayersIncome.json");
    public static string MarketPlayerMessagesJSON => Path.Combine(DataFolder, "PlayerMessages.json");
    public static string MarketLeaderboardJSON => Path.Combine(DataFolder, "LeaderboardV3.json");

    private static string DistancedUIFolder => Path.Combine(MainPath, "DistancedUI");
    public static string DistancedUIConfig => Path.Combine(DistancedUIFolder, "DistancedUI.cfg"); 

    public static string MainConfig => Path.Combine(MainPath, "MarketPlace.cfg");

    private static string ConfigsFolder => Path.Combine(MainPath, "Configs");
    public static string QuestsDatabaseFolder => Path.Combine(ConfigsFolder, "Quests");
    public static string DialoguesFolder => Path.Combine(ConfigsFolder, "Dialogues");
    public static string TerritoriesFolder => Path.Combine(ConfigsFolder, "Territories");
    public static string QuestsProfilesFolder => Path.Combine(ConfigsFolder, "QuestProfiles");
    public static string QuestsEventsFolder => Path.Combine(ConfigsFolder, "QuestEvents");
    public static string BankerProfilesFolder => Path.Combine(ConfigsFolder, "Bankers");
    public static string TeleportHubProfilesFolder => Path.Combine(ConfigsFolder, "Teleporters");
    public static string TraderProfilesFolder => Path.Combine(ConfigsFolder, "Traders");
    public static string ServerInfoProfilesFolder => Path.Combine(ConfigsFolder, "ServerInfos");
    public static string GamblerProfilesFolder => Path.Combine(ConfigsFolder, "Gamblers");
    public static string BufferDatabaseFolder => Path.Combine(ConfigsFolder, "Buffers");
    public static string BufferProfilesFolder => Path.Combine(ConfigsFolder, "BufferProfiles");
    public static string TransmogrificationFolder => Path.Combine(ConfigsFolder, "Transmogrifications");
    public static string LeaderboardAchievementsFolder => Path.Combine(ConfigsFolder, "LeaderboardAchievements");

    private static string PlayerTagsFolder => Path.Combine(MainPath, "PlayerTags");
    public static string PlayerTagsConfig => Path.Combine(PlayerTagsFolder, "PlayerTags.cfg");

    [UsedImplicitly]
    private static void OnInit()
    {
        if (Marketplace.WorkingAsType is Marketplace.WorkingAs.Server or Marketplace.WorkingAs.Both)
        {
            if (!Directory.Exists(MainPath))
                Directory.CreateDirectory(MainPath);
            if (!Directory.Exists(DataFolder))
                Directory.CreateDirectory(DataFolder);
            if (!Directory.Exists(DiscordStuffFolder))
                Directory.CreateDirectory(DiscordStuffFolder);
            if (!Directory.Exists(DistancedUIFolder))
                Directory.CreateDirectory(DistancedUIFolder);
            if (!Directory.Exists(PlayerTagsFolder))
                Directory.CreateDirectory(PlayerTagsFolder);
            if (!Directory.Exists(ConfigsFolder))
                Directory.CreateDirectory(ConfigsFolder);
            if (!Directory.Exists(QuestsDatabaseFolder))
                Directory.CreateDirectory(QuestsDatabaseFolder);
            if (!Directory.Exists(DialoguesFolder))
                Directory.CreateDirectory(DialoguesFolder);
            if (!Directory.Exists(TerritoriesFolder))
                Directory.CreateDirectory(TerritoriesFolder);
            if (!Directory.Exists(QuestsProfilesFolder))
                Directory.CreateDirectory(QuestsProfilesFolder);
            if (!Directory.Exists(QuestsEventsFolder))
                Directory.CreateDirectory(QuestsEventsFolder);
            if (!Directory.Exists(BankerProfilesFolder))
                Directory.CreateDirectory(BankerProfilesFolder);
            if (!Directory.Exists(TeleportHubProfilesFolder))
                Directory.CreateDirectory(TeleportHubProfilesFolder);
            if (!Directory.Exists(TraderProfilesFolder))
                Directory.CreateDirectory(TraderProfilesFolder);
            if (!Directory.Exists(ServerInfoProfilesFolder))
                Directory.CreateDirectory(ServerInfoProfilesFolder);
            if (!Directory.Exists(GamblerProfilesFolder))
                Directory.CreateDirectory(GamblerProfilesFolder);
            if (!Directory.Exists(BufferDatabaseFolder))
                Directory.CreateDirectory(BufferDatabaseFolder);
            if (!Directory.Exists(BufferProfilesFolder))
                Directory.CreateDirectory(BufferProfilesFolder);
            if (!Directory.Exists(TransmogrificationFolder))
                Directory.CreateDirectory(TransmogrificationFolder);
            if (!Directory.Exists(LeaderboardAchievementsFolder))
                Directory.CreateDirectory(LeaderboardAchievementsFolder);
            
     
            if (!File.Exists(BankerDataJSONFile)) File.Create(BankerDataJSONFile).Dispose();
            if (!File.Exists(MarketLeaderboardJSON)) File.Create(MarketLeaderboardJSON).Dispose();
            if (!File.Exists(PlayerTagsConfig)) File.Create(PlayerTagsConfig).Dispose();
        }

        if (Marketplace.WorkingAsType is Marketplace.WorkingAs.Client or Marketplace.WorkingAs.Both)
        {
            if (!Directory.Exists(NPC_SoundsPath))
                Directory.CreateDirectory(NPC_SoundsPath);
            if (!Directory.Exists(CachedImagesFolder))
                Directory.CreateDirectory(CachedImagesFolder);
            if (!Directory.Exists(NPC_Saved))
                Directory.CreateDirectory(NPC_Saved);
        }
    }
}
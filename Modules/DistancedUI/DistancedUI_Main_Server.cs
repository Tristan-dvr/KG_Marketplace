using BepInEx.Configuration;
using Marketplace.Paths;

namespace Marketplace.Modules.DistancedUI;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Normal, "OnInit", new[] { "DistancedUI.cfg" },
    new[] { "OnConfigChange" })]
public static class DistanceUI_Main_Server
{
    private static ConfigFile PremiumSystem_Config;
    private static ConfigEntry<bool> CanUseMarketplace_PremiumSystem;
    private static ConfigEntry<bool> EveryoneIsVIP_PremiumSystem;
    private static ConfigEntry<string> Users_PremiumSystem;
    private static ConfigEntry<string> TraderProfiles_PremiumSystem;
    private static ConfigEntry<string> BankerProfiles_PremiumSystem;
    private static ConfigEntry<string> TeleporterProfiles_PremiumSystem;
    private static ConfigEntry<string> GamblerProfiles_PremiumSystem;
    private static ConfigEntry<string> BufferProfiles_PremiumSystem;
    private static ConfigEntry<string> QuestProfiles_PremiumSystem;
    private static ConfigEntry<string> InfoProfiles_PremiumSystem;
    private static ConfigEntry<string> TransmogrificationProfiles_PremiumSystem;

    private static void OnInit()
    {
        PremiumSystem_Config = new ConfigFile(Market_Paths.DistancedUIConfig, true);
        EveryoneIsVIP_PremiumSystem = PremiumSystem_Config.Bind("PremiumSystem", "EveryoneIsPremium", false,
            "Turn on if you want everyone to be able to use Premium System features.");
        Users_PremiumSystem = PremiumSystem_Config.Bind("PremiumSystem", "Users", "User IDs here with ,",
            "List of users that can use Premium System features. Separate each user with a comma.");
        CanUseMarketplace_PremiumSystem =
            PremiumSystem_Config.Bind("PremiumSystem", "CanUseMarketplace", true, "Can use the marketplace?");
        TraderProfiles_PremiumSystem = PremiumSystem_Config.Bind("PremiumSystem", "TraderProfiles",
            "Profiles here with ,", "Trader profiles");
        TeleporterProfiles_PremiumSystem = PremiumSystem_Config.Bind("PremiumSystem", "TeleporterProfiles",
            "Profiles here with ,", "Teleporter profiles");
        GamblerProfiles_PremiumSystem = PremiumSystem_Config.Bind("PremiumSystem", "GamblerProfiles",
            "Profiles here with ,", "Gambler profiles");
        BufferProfiles_PremiumSystem = PremiumSystem_Config.Bind("PremiumSystem", "BufferProfiles",
            "Profiles here with ,", "Buffer profiles");
        BankerProfiles_PremiumSystem = PremiumSystem_Config.Bind("PremiumSystem", "BankerProfiles",
            "Profiles here with ,", "Banker profiles");
        QuestProfiles_PremiumSystem =
            PremiumSystem_Config.Bind("PremiumSystem", "QuestProfiles", "Profiles here with ,", "Quest profiles");
        InfoProfiles_PremiumSystem =
            PremiumSystem_Config.Bind("PremiumSystem", "InfoProfiles", "Profiles here with ,", "Info profiles");
        TransmogrificationProfiles_PremiumSystem = 
            PremiumSystem_Config.Bind("PremiumSystem", "TransmogrificationProfiles", "Profiles here with ,", "Transmogrification profiles");

        ReadPremiumSystemData();
    }

    private static void ReadPremiumSystemData()
    {
        DistancedUI_DataType.CurrentPremiumSystemData.Value = new DistancedUI_DataType.PremiumSystemData
        {
            isAllowed = false,
            MarketplaceEnabled = CanUseMarketplace_PremiumSystem.Value,
            EveryoneIsVIP = EveryoneIsVIP_PremiumSystem.Value,
            Users = Users_PremiumSystem.Value.Replace(" ", "").Split(',').Select(d => d.ToLower().Trim()).ToList(),
            TraderProfiles = TraderProfiles_PremiumSystem.Value.Split(',').Select(d => d.ToLower().Trim()).ToList(),
            BankerProfiles = BankerProfiles_PremiumSystem.Value.Split(',').Select(d => d.ToLower().Trim()).ToList(),
            TeleporterProfiles = TeleporterProfiles_PremiumSystem.Value.Split(',').Select(d => d.ToLower().Trim())
                .ToList(),
            GamblerProfiles = GamblerProfiles_PremiumSystem.Value.Split(',').Select(d => d.ToLower().Trim()).ToList(),
            BufferProfiles = BufferProfiles_PremiumSystem.Value.Split(',').Select(d => d.ToLower().Trim()).ToList(),
            QuestProfiles = QuestProfiles_PremiumSystem.Value.Split(',').Select(d => d.ToLower().Trim()).ToList(),
            InfoProfiles = InfoProfiles_PremiumSystem.Value.Split(',').Select(d => d.ToLower().Trim()).ToList(),
            TransmogrificationProfiles = TransmogrificationProfiles_PremiumSystem.Value.Split(',').Select(d => d.ToLower().Trim()).ToList()
        };
    }

    private static void OnConfigChange()
    {
        Utils.DelayReloadConfig(PremiumSystem_Config, ReadPremiumSystemData);
    }
}
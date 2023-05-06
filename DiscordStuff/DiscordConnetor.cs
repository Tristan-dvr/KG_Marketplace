using System.Net;
using System.Threading.Tasks;
using BepInEx.Configuration;
using Marketplace.Modules.Marketplace_NPC;
using Marketplace.Paths;

namespace Marketplace;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Last, "OnInit", 
    new[] { "DiscordSettings.cfg" },
    new[] { "OnDiscordSettingsChange" })]
public static class DiscordStuff
{
    public enum Webhooks
    {
        Marketplace,
        Gambler,
        Quest
    }

    private static ConfigFile DiscordConfig;
    private static Dictionary<Webhooks, ConfigEntry<string>> WebhookLinks;
    private static Dictionary<Webhooks, ConfigEntry<string>> LocalizedWebhookTitles;
    private static Dictionary<Webhooks, ConfigEntry<string>> WebhookMessages;

    private static void OnInit()
    {
        string filePath = Path.Combine(Market_Paths.DiscordStuffFolder, "DiscordSettings.cfg");
        
        DiscordConfig = new ConfigFile(filePath, true)
        {
            SaveOnConfigSet = true
        };
        WebhookLinks = new Dictionary<Webhooks, ConfigEntry<string>>
        {
            [Webhooks.Marketplace] = DiscordConfig.Bind("Webhook Links", "Marketplace Webhook Link", "LINK HERE"),
            [Webhooks.Gambler] = DiscordConfig.Bind("Webhook Links", "Gambler Webhook Link", "LINK HERE"),
            [Webhooks.Quest] = DiscordConfig.Bind("Webhook Links", "Quest Webhook Link", "LINK HERE")
        };
        WebhookMessages = new Dictionary<Webhooks, ConfigEntry<string>>
        {
            [Webhooks.Marketplace] = DiscordConfig.Bind("Webhook Messages", "Marketplace Webhook Message",
                "**{0}** posted **x{1} {2}** with **{3} gold each**"),
            [Webhooks.Gambler] =
                DiscordConfig.Bind("Webhook Messages", "Gambler Webhook Message", "**{0}** won **x{1} {2}**!"),
            [Webhooks.Quest] =
                DiscordConfig.Bind("Webhook Messages", "Quest Webhook Message", "**{0}** finished quest **{1}**")
        };
        LocalizedWebhookTitles = new Dictionary<Webhooks, ConfigEntry<string>>
        {
            [Webhooks.Marketplace] =
                DiscordConfig.Bind("Webhook Titles", "Marketplace Webhook Title", "Marketplace Message"),
            [Webhooks.Gambler] = DiscordConfig.Bind("Webhook Titles", "Gambler Webhook Title", "Gambler Message"),
            [Webhooks.Quest] = DiscordConfig.Bind("Webhook Titles", "Quest Webhook Title", "Quest Message")
        };
    }

    private static void OnDiscordSettingsChange()
    {
        Utils.DelayReloadConfig(DiscordConfig);
        Utils.print("Discord Settings Changed.");
    }


    public static void SendMarketplaceWebhook(Marketplace_DataTypes.ServerMarketSendData data)
    {
        if (data.SellerName == "**************") return;
        ZPackage pkg = new();
        pkg.Write((int)Webhooks.Marketplace);
        pkg.Write(data.SellerName);
        pkg.Write(data.Count);
        pkg.Write(data.ItemName);
        pkg.Write(data.Price);
        pkg.SetPos(0);
        SendWebhook(0, pkg);
    }


    private static void SendWebhook(long sender, ZPackage pkg)
    {
        Webhooks type = (Webhooks)pkg.ReadInt();
        string link = WebhookLinks[type].Value;
        if (!Uri.TryCreate(link, UriKind.Absolute, out _)) return;

        string playername = pkg.ReadString();
        string text;
        switch (type)
        {
            case Webhooks.Marketplace:
                text = string.Format(WebhookMessages[type].Value, playername, pkg.ReadInt(),
                    Localization.instance.Localize(pkg.ReadString()), pkg.ReadInt());
                break;
            case Webhooks.Gambler:
                text = string.Format(WebhookMessages[type].Value, playername, pkg.ReadInt(),
                    Localization.instance.Localize(pkg.ReadString()));
                break;
            case Webhooks.Quest:
                text = string.Format(WebhookMessages[type].Value, playername, pkg.ReadString());
                break;
            default:
                text = "No Data";
                break;
        }

        text = Utils.RemoveRichTextDynamicTag(text, "color");
        Task.Run(async () =>
        {
            string json =
                "{\n  \"username\": \"" +
                $"{LocalizedWebhookTitles[type].Value}" +
                "\",\n  \"avatar_url\": \"\",\n  \"content\": \"" +
                $"{text}" +
                "\",\n  \"embeds\": [],\n  \"components\": []\n}";
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(link);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                await streamWriter.WriteAsync(json);
            }

            httpWebRequest.GetResponseAsync();
        });
    }

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    [ServerOnlyPatch]
    private static class ZrouteMethodsServerWebhooks
    {
        private static void Postfix()
        {
            if(!ZNet.instance.IsServer()) return;
            ZRoutedRpc.instance.Register("KGmarket CustomWebhooks", new Action<long, ZPackage>(SendWebhook));
        }
    }
}
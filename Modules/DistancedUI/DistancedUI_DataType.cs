namespace Marketplace.Modules.DistancedUI;

public static class DistancedUI_DataType
{
    internal static readonly CustomSyncedValue<PremiumSystemData> CurrentPremiumSystemData =
        new(Marketplace.configSync, "premiumSystemData", new());

    public class PremiumSystemData : ISerializableParameter
    {
        public bool isAllowed;
        public bool EveryoneIsVIP;
        public bool MarketplaceEnabled;
        public List<string> Users = new();
        public List<string> TraderProfiles = new();
        public List<string> TeleporterProfiles = new();
        public List<string> GamblerProfiles = new();
        public List<string> BufferProfiles = new();
        public List<string> QuestProfiles = new();
        public List<string> InfoProfiles = new();
        public List<string> BankerProfiles = new();
        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write(isAllowed);
            pkg.Write(EveryoneIsVIP);
            pkg.Write(MarketplaceEnabled);
            pkg.Write(Users.Count);
            foreach (string user in Users)
            {
                pkg.Write(user ?? "");
            }
            pkg.Write(TraderProfiles.Count);
            foreach (string profile in TraderProfiles)
            {
                pkg.Write(profile ?? "");
            }
            pkg.Write(TeleporterProfiles.Count);
            foreach (string profile in TeleporterProfiles)
            {
                pkg.Write(profile ?? "");
            }
            pkg.Write(GamblerProfiles.Count);
            foreach (string profile in GamblerProfiles)
            {
                pkg.Write(profile ?? "");
            }
            pkg.Write(BufferProfiles.Count);
            foreach (string profile in BufferProfiles)
            {
                pkg.Write(profile ?? "");
            }
            pkg.Write(QuestProfiles.Count);
            foreach (string profile in QuestProfiles)
            {
                pkg.Write(profile ?? "");
            }
            pkg.Write(InfoProfiles.Count);
            foreach (string profile in InfoProfiles)
            {
                pkg.Write(profile ?? "");
            }
            pkg.Write(BankerProfiles.Count);
            foreach (string profile in BankerProfiles)
            {
                pkg.Write(profile ?? "");
            }
        }

        public void Deserialize(ref ZPackage pkg)
        {
            isAllowed = pkg.ReadBool();
            EveryoneIsVIP = pkg.ReadBool();
            MarketplaceEnabled = pkg.ReadBool();
            int count = pkg.ReadInt();
            for (int i = 0; i < count; i++)
            {
                Users.Add(pkg.ReadString());
            }
            count = pkg.ReadInt();
            for (int i = 0; i < count; i++)
            {
                TraderProfiles.Add(pkg.ReadString());
            }
            count = pkg.ReadInt();
            for (int i = 0; i < count; i++)
            {
                TeleporterProfiles.Add(pkg.ReadString());
            }
            count = pkg.ReadInt();
            for (int i = 0; i < count; i++)
            {
                GamblerProfiles.Add(pkg.ReadString());
            }
            count = pkg.ReadInt();
            for (int i = 0; i < count; i++)
            {
                BufferProfiles.Add(pkg.ReadString());
            }
            count = pkg.ReadInt();
            for (int i = 0; i < count; i++)
            {
                QuestProfiles.Add(pkg.ReadString());
            }
            count = pkg.ReadInt();
            for (int i = 0; i < count; i++)
            {
                InfoProfiles.Add(pkg.ReadString());
            }
            count = pkg.ReadInt();
            for (int i = 0; i < count; i++)
            {
                BankerProfiles.Add(pkg.ReadString());
            }
        }
    }
}
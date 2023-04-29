namespace Marketplace.Modules.Battlepass;

public static class Battlepass_DataTypes
{
    internal static readonly CustomSyncedValue<BattlePassData> SyncedBattlepassData =
        new(Marketplace.configSync, "battlePassData", new BattlePassData());
    
   public class BattlePassData : ISerializableParameter
    {
        public string Name;
        public string PremiumUsers;
        public int UID;
        public int ExpStep;
        public List<BattlePassElement> FreeRewards = new();
        public List<BattlePassElement> PremiumRewards = new();

        public override string ToString()
        {
            return
                $"{nameof(UID)}: {UID}, {nameof(ExpStep)}: {ExpStep}, {nameof(FreeRewards)}: {FreeRewards}, {nameof(PremiumRewards)}: {PremiumRewards}";
        }

        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write(Name ?? "");
            pkg.Write(UID);
            pkg.Write(ExpStep);
            pkg.Write(FreeRewards.Count);
            foreach (BattlePassElement element in FreeRewards)
            {
                pkg.Write(element.RewardName ?? "");
                pkg.Write(element.Order);
                pkg.Write(element.ItemNames.Count);
                foreach (string itemName in element.ItemNames)
                {
                    pkg.Write(itemName ?? "");
                }

                pkg.Write(element.ItemCounts.Count);
                foreach (int itemCount in element.ItemCounts)
                {
                    pkg.Write(itemCount);
                }

                pkg.Write(element.ItemLevels.Count);
                foreach (int itemLevel in element.ItemLevels)
                {
                    pkg.Write(itemLevel);
                }
            }
            
            pkg.Write(PremiumRewards.Count);
            foreach (BattlePassElement element in PremiumRewards)
            {
                pkg.Write(element.RewardName ?? "");
                pkg.Write(element.Order);
                pkg.Write(element.ItemNames.Count);
                foreach (string itemName in element.ItemNames)
                {
                    pkg.Write(itemName ?? "");
                }

                pkg.Write(element.ItemCounts.Count);
                foreach (int itemCount in element.ItemCounts)
                {
                    pkg.Write(itemCount);
                }

                pkg.Write(element.ItemLevels.Count);
                foreach (int itemLevel in element.ItemLevels)
                {
                    pkg.Write(itemLevel);
                }
            }

            pkg.Write(PremiumUsers ?? "");
        }

        public void Deserialize(ref ZPackage pkg) 
        {
            Name = pkg.ReadString();
            UID = pkg.ReadInt();
            ExpStep = pkg.ReadInt();
            int freeCount = pkg.ReadInt();
            for (int i = 0; i < freeCount; i++)
            {
                BattlePassElement element = new()
                {
                    RewardName = pkg.ReadString(),
                    Order = pkg.ReadInt(),
                    ItemNames = new List<string>(),
                    ItemCounts = new List<int>(),
                    ItemLevels = new List<int>()
                };
                int itemCount = pkg.ReadInt();
                for (int j = 0; j < itemCount; j++)
                {
                    element.ItemNames.Add(pkg.ReadString());
                }

                itemCount = pkg.ReadInt();
                for (int j = 0; j < itemCount; j++)
                {
                    element.ItemCounts.Add(pkg.ReadInt());
                }

                itemCount = pkg.ReadInt();
                for (int j = 0; j < itemCount; j++)
                {
                    element.ItemLevels.Add(pkg.ReadInt());
                }

                FreeRewards.Add(element);
            }
            
            int premiumCount = pkg.ReadInt();
            for (int i = 0; i < premiumCount; i++)
            {
                BattlePassElement element = new()
                {
                    RewardName = pkg.ReadString(),
                    Order = pkg.ReadInt(),
                    ItemNames = new List<string>(),
                    ItemCounts = new List<int>(),
                    ItemLevels = new List<int>()
                };
                int itemCount = pkg.ReadInt();
                for (int j = 0; j < itemCount; j++)
                {
                    element.ItemNames.Add(pkg.ReadString());
                }

                itemCount = pkg.ReadInt();
                for (int j = 0; j < itemCount; j++)
                {
                    element.ItemCounts.Add(pkg.ReadInt());
                }

                itemCount = pkg.ReadInt();
                for (int j = 0; j < itemCount; j++)
                {
                    element.ItemLevels.Add(pkg.ReadInt());
                }

                PremiumRewards.Add(element);
            }
            PremiumUsers = pkg.ReadString();
        }
    }

   public class BattlePassElement
   {
       public string RewardName;
       public int Order;
       public List<string> ItemNames = new();
       public List<int> ItemCounts = new();
       public List<int> ItemLevels = new();


       private readonly List<Sprite> ItemSprites = new();
       private readonly List<string> AdditionalString = new();
       private readonly List<string> Localized = new();

       public Sprite GetSprite(int index) => ItemSprites[index];
       public void AddSprite(Sprite s) => ItemSprites.Add(s);
       public void AddString(string s) => AdditionalString.Add(s);
       public string GetString(int index) => AdditionalString[index];
       public void SetLocalized(string s) => Localized.Add(s);
       public string GetLocalizedName(int index) => Localized[index];


       public override string ToString()
       {
           return
               $"{nameof(RewardName)}: {RewardName}, {nameof(Order)}: {Order}, {nameof(ItemNames)}: {ItemNames}, {nameof(ItemCounts)}: {ItemCounts}, {nameof(ItemLevels)}: {ItemLevels}";
       }
   }
}
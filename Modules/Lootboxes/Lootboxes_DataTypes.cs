namespace Marketplace.Modules.Lootboxes;

public static class Lootboxes_DataTypes
{
    internal static readonly CustomSyncedValue<List<Lootbox>> SyncedLootboxData =
        new(Marketplace.configSync, "lootboxesData", new(), CustomSyncedValueBase.Config_Priority.First);

    private const int VERSION = 1;
    
    public class Lootbox : ISerializableParameter
    {
        public enum LBType
        {
            One, All, AllWithChance, AllWithChanceShowTooltip
        }
        
        public string UID;
        public string Icon;
        public string OpenVFX;
        public string AdditionalDescription;
        public LBType Type;
        public List<Item> Items = new();

        public class Item
        {
            public string Prefab;
            public int Min;
            public int Max;
            public int Level;
            public int Chance;

            public Item() {}
            public Item(string prefab, int min, int max, int level, int chance)
            {
                Prefab = prefab;
                Min = min;
                Max = max;
                Level = level;
                Chance = chance;
            }
        }

        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write(VERSION);
            pkg.Write(UID ?? "");
            pkg.Write((int)Type);
            pkg.Write(Icon ?? "");
            pkg.Write(OpenVFX ?? "");
            pkg.Write(AdditionalDescription ?? "");
            pkg.Write(Items.Count);
            foreach (var item in Items)
            {
                pkg.Write(item.Prefab ?? "");
                pkg.Write(item.Min);
                pkg.Write(item.Max);
                pkg.Write(item.Level);
                pkg.Write(item.Chance);
            }
        }

        public void Deserialize(ref ZPackage pkg)
        {
            int version = pkg.ReadInt();
            if (version == 1)
            {
                UID = pkg.ReadString();
                Type = (LBType)pkg.ReadInt();
                Icon = pkg.ReadString();
                OpenVFX = pkg.ReadString();
                AdditionalDescription = pkg.ReadString();
                int count = pkg.ReadInt();
                for (int i = 0; i < count; i++)
                {
                    Items.Add(new()
                    {
                        Prefab = pkg.ReadString(),
                        Min = pkg.ReadInt(),
                        Max = pkg.ReadInt(),
                        Level = pkg.ReadInt(),
                        Chance = pkg.ReadInt()
                    });
                }
            }
        }

        public static implicit operator bool(Lootbox lootbox)
        {
            return lootbox != null;
        }
    }
}
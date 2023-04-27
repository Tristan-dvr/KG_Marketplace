namespace Marketplace.Modules.Gambler;

public static class Gambler_DataTypes
{
    internal static readonly CustomSyncedValue<Dictionary<string, BigData>>
        GamblerData = new(Marketplace.configSync, "gamblerData", new());
    
    public class BigData : ISerializableParameter
    {
        public List<Item> Data = new();
        public int MAXROLLS;

        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write(MAXROLLS);
            pkg.Write(Data.Count);
            foreach (var item in Data)
            {
                pkg.Write(item.Prefab ?? "");
                pkg.Write(item.Min);
                pkg.Write(item.Max);
            }
        }

        public void Deserialize(ref ZPackage pkg)
        {
            MAXROLLS = pkg.ReadInt();
            int count = pkg.ReadInt();
            for (int i = 0; i < count; i++)
            {
                Data.Add(new Item
                {
                    Prefab = pkg.ReadString(),
                    Min = pkg.ReadInt(),
                    Max = pkg.ReadInt()
                });
            }
        }
    }
    
    public class Item
    {
        public string Prefab;
        public int Min, Max;
        private Sprite icon;
        private string localiedName;
        private string rawName;
        public Sprite Sprite => icon;
        public string Name => localiedName;

        public string RawName => rawName;

        public void SetData(string RAWNAME, Sprite s)
        {
            rawName = RAWNAME;
            localiedName = Localization.instance.Localize(rawName);
            icon = s;
        }
    }
}
namespace Marketplace.Modules.ItemMocker;

public static class ItemMocker_DataTypes
{
    internal static readonly CustomSyncedValue<List<ItemMock>> SyncedMockedItems = new(Marketplace.configSync,
        "marketItemMocker", new());

    public class ItemMock : ISerializableParameter
    {
        public string UID;
        public string Model;
        public string Name;
        public string Description;
        public int MaxStack;
        public int Weight;

        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write(UID);
            pkg.Write(MaxStack);
            pkg.Write(Weight);
            pkg.Write(Model ?? "");
            pkg.Write(Name ?? "");
            pkg.Write(Description ?? "");
        }

        public void Deserialize(ref ZPackage pkg)
        {
            UID = pkg.ReadString();
            MaxStack = pkg.ReadInt();
            Weight = pkg.ReadInt();
            Model = pkg.ReadString();
            Name = pkg.ReadString();
            Description = pkg.ReadString();
        }
    }
}
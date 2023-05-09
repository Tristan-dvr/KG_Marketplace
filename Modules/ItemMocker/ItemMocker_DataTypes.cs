namespace Marketplace.Modules.ItemMocker;

public static class ItemMocker_DataTypes
{
    internal static readonly CustomSyncedValue<List<ItemMock>> SyncedMockedItems = new(Marketplace.configSync,
        "marketItemMocker", new List<ItemMock>(), CustomSyncedValueBase.Config_Priority.First);

    public class ItemMock : ISerializableParameter
    {
        public string UID;
        public string Model;
        public string Name;
        public string Description;
        public int MaxStack;
        public float Scale;
        public string Recipe;

        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write(UID);
            pkg.Write(MaxStack);
            pkg.Write(Model ?? "");
            pkg.Write(Name ?? "");
            pkg.Write(Description ?? "");
            pkg.Write(Scale);
            pkg.Write(Recipe ?? "");
        }

        public void Deserialize(ref ZPackage pkg)
        {
            UID = pkg.ReadString();
            MaxStack = pkg.ReadInt();
            Model = pkg.ReadString();
            Name = pkg.ReadString();
            Description = pkg.ReadString();
            Scale = pkg.ReadSingle();
            Recipe = pkg.ReadString();
        }
    }
}
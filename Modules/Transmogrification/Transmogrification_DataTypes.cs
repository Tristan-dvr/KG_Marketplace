using ItemDataManager;

namespace Marketplace.Modules.Transmogrification;

public static class Transmogrification_DataTypes
{
    public static readonly CustomSyncedValue<Dictionary<string, List<TransmogItem_Data>>>
        TransmogData = new(Marketplace.configSync, "transmogData", new Dictionary<string, List<TransmogItem_Data>>());
    
    
    public class TransmogItem_Data : ISerializableParameter
    {
        public string Prefab;
        public string Price_Prefab;
        public int Price_Amount;
        public bool IgnoreCategory;
        public int VFX_ID;

        private string LocalizedName;
        private string LocalizedPrice;
        private Sprite Icon;
        private Sprite PriceIcon;
        public string GetLocalizedName() => LocalizedName;
        public string GetLocalizedPrice() => LocalizedPrice;
        public Sprite GetIcon() => Icon;
        public Sprite GetPriceIcon() => PriceIcon;
        public void SetLocalizedName(string name) => LocalizedName = name;
        public void SetLocalizedPrice(string price) => LocalizedPrice = price;
        public void SetIcon(Sprite icon) => Icon = icon;
        public void SetPriceIcon(Sprite icon) => PriceIcon = icon;
        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write(Prefab ?? "");
            pkg.Write(Price_Prefab ?? "");
            pkg.Write(Price_Amount);
            pkg.Write(IgnoreCategory);
            pkg.Write(VFX_ID);
        }

        public void Deserialize(ref ZPackage pkg)
        {
            Prefab = pkg.ReadString();
            Price_Prefab = pkg.ReadString();
            Price_Amount = pkg.ReadInt();
            IgnoreCategory = pkg.ReadBool();
            VFX_ID = pkg.ReadInt();
        }
    }
    
    public class TransmogItem_Component : ItemData
    {
        public string ReplacedPrefab;
        public int Variant;
        public int VFX_ID;

        public TransmogItem_Component(){}
        
        public TransmogItem_Component(string replacedPrefab, int variant, int vfx_id)
        {
            ReplacedPrefab = replacedPrefab;
            Variant = variant;
            VFX_ID = vfx_id;
        }

        public override void Load()
        {
            string[] split = Value.Split('|');
            ReplacedPrefab = split[0];
            Variant = int.Parse(split[1]);
            VFX_ID = int.Parse(split[2]);
        }

        public override void Save()
        {
            Value = $"{ReplacedPrefab}|{Variant}|{VFX_ID}";
        }
        
    }
}
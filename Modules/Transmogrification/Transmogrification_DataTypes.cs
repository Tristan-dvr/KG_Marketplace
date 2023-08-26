﻿using ItemDataManager;

namespace Marketplace.Modules.Transmogrification;

public static class Transmogrification_DataTypes
{
    internal static readonly CustomSyncedValue<Dictionary<string, List<TransmogItem_Data>>>
        SyncedTransmogData = new(Marketplace.configSync, "transmogData", new Dictionary<string, List<TransmogItem_Data>>());
    
    
    public class TransmogItem_Data : ISerializableParameter
    {
        public string Prefab;
        public string Price_Prefab;
        public int Price_Amount;
        public bool IgnoreCategory;

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
        }

        public void Deserialize(ref ZPackage pkg)
        {
            Prefab = pkg.ReadString();
            Price_Prefab = pkg.ReadString();
            Price_Amount = pkg.ReadInt();
            IgnoreCategory = pkg.ReadBool();
        }
    }
    
    public class TransmogItem_Component : ItemData
    {
        public string ReplacedPrefab;
        public int Variant;
        public string ItemColor;

        public TransmogItem_Component(){}

        public override void Load()
        {
            string[] split = Value.Split('|');
            ReplacedPrefab = split[0];
            Variant = int.Parse(split[1]);
            ItemColor = split.Length > 3 ? split[3] : "";
        }

        public override void Save()
        {
            Value = $"{ReplacedPrefab}|{Variant}|0|{ItemColor}";
        }
        
    }
}
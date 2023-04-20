using ServerSync;

namespace Marketplace.Modules.Marketplace_NPC;

public static class Marketplace_DataTypes
{
    internal static readonly CustomSyncedValue<List<ServerMarketSendData>> ServerMarketPlaceData = 
        new(Marketplace.configSync, "marketplaceData", new());
    
    [Serializable]
    public class ServerMarketSendData : ISerializableParameter
    {
        public int UID;
        public string ItemPrefab;
        public int Count;
        public int Price;
        public string SellerName;
        public string SellerUserID;
        public ItemData_ItemCategory ItemCategory;
        public int Quality;
        public int Variant;
        public string CrafterName = "";
        public long CrafterID;
        public string CUSTOMdata = "{}";


        public ServerMarketSendData()
        {
        }

        public ServerMarketSendData(ClientMarketSendData other, string user)
        {
            ItemPrefab = other.ItemPrefab;
            Count = other.Count;
            Price = other.Price;
            SellerName = other.SellerName;
            SellerUserID = user;
            ItemCategory = other.ItemCategory;
            Quality = other.Quality;
            Variant = other.Variant;
            CUSTOMdata = other.CUSTOMdata;
            CrafterName = other.CrafterName;
            CrafterID = other.CrafterID;
            UID = Marketplacec_Main_Server.LastIDConfig.Value;
            Marketplacec_Main_Server.LastIDConfig.Value++;
            Marketplace._thistype.Config.Save();
        }
        
        public string ItemName => ZNetScene.instance.GetPrefab(ItemPrefab) is {} item ? item.GetComponent<ItemDrop>().m_itemData.m_shared.m_name : ItemPrefab;
        
        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write(UID);
            pkg.Write(ItemPrefab ?? "");
            pkg.Write(Count);
            pkg.Write(Price);
            pkg.Write(SellerName ?? "");
            pkg.Write(SellerUserID ?? "");
            pkg.Write((int) ItemCategory);
            pkg.Write(Quality);
            pkg.Write(Variant);
            pkg.Write(CrafterName ?? "");
            pkg.Write(CrafterID);
            pkg.Write(CUSTOMdata ?? "{}");
        }

        public void Deserialize(ref ZPackage pkg)
        {
            UID = pkg.ReadInt();
            ItemPrefab = pkg.ReadString();
            Count = pkg.ReadInt();
            Price = pkg.ReadInt();
            SellerName = pkg.ReadString();
            SellerUserID = pkg.ReadString();
            ItemCategory = (ItemData_ItemCategory) pkg.ReadInt();
            Quality = pkg.ReadInt();
            Variant = pkg.ReadInt();
            CrafterName = pkg.ReadString();
            CrafterID = pkg.ReadLong();
            CUSTOMdata = pkg.ReadString();
        }
    }
    
    [Serializable]
    public class ClientMarketSendData
    {
        public string ItemPrefab;
        public int Count;
        public int Price;
        public string SellerName;
        public ItemData_ItemCategory ItemCategory;
        public int Quality;
        public int Variant;
        public string CUSTOMdata = "{}";
        public string CrafterName = "";
        public long CrafterID;
        
        public string ItemName => ZNetScene.instance.GetPrefab(ItemPrefab) is {} item ? item.GetComponent<ItemDrop>().m_itemData.m_shared.m_name : ItemPrefab;
        
    }
    public enum ItemData_ItemCategory
    {
        ALL,
        WEAPONS,
        ARMOR,
        CONSUMABLE,
        TOOLS,
        RESOURCES
    } 
    public enum MarketMode
    {
        BUY,
        SELL
    }
    public enum SortBy
    {
        None,
        ItemName,
        Count,
        Price,
        Seller
    }
    public enum SortType
    {
        UP,
        DOWN
    }
    
}
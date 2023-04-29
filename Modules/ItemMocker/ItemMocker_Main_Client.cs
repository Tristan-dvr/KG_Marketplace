using Marketplace.OBJimporter;

namespace Marketplace.Modules.ItemMocker;

[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Normal, "OnInit")]
public static class ItemMocker_Main_Client
{
    private static GameObject MockItemBase;
    private static bool CanMockItems;

    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Awake))]
    [ClientOnlyPatch]
    private static class FejdStartup_Awake_Patch
    {
        private static void Prefix() => CanMockItems = true;
    }

    private static void OnInit()
    {
        MockItemBase = AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("Marketplace_ItemMockerBase");
        ItemMocker_DataTypes.SyncedMockedItems.ValueChanged += MockItems;
    }

    private static void MockItems()
    {
        if (!ZNetScene.instance || !CanMockItems || ItemMocker_DataTypes.SyncedMockedItems.Value.Count == 0) return;
        LayerMask itemLayer = LayerMask.NameToLayer("item");
        GameObject inactive = new GameObject("Inactive_MockerBase");
        inactive.SetActive(false);
        foreach (var mock in ItemMocker_DataTypes.SyncedMockedItems.Value)
        {
            GameObject newObj = UnityEngine.Object.Instantiate(MockItemBase, inactive.transform);
            newObj.name = mock.UID; ;
            ItemDrop itemDrop = newObj.GetComponent<ItemDrop>();
            itemDrop.m_itemData.m_shared.m_name = mock.Name;
            itemDrop.m_itemData.m_shared.m_description = mock.Description;
            itemDrop.m_itemData.m_shared.m_maxStackSize = mock.MaxStack;
            itemDrop.m_itemData.m_shared.m_weight = 0;
            if (!ObjModelLoader._loadedIcons.TryGetValue(mock.Model, out Sprite icon))
                icon = AssetStorage.AssetStorage.PlaceholderGamblerIcon;
            itemDrop.m_itemData.m_shared.m_icons[0] = icon;
            if (ObjModelLoader._loadedModels.TryGetValue(mock.Model, out var model))
            {
                newObj.transform.Find("Cube").gameObject.SetActive(false);
                var newModel = UnityEngine.Object.Instantiate(model, newObj.transform);
                newModel.name = "attach";
                newModel.transform.localScale = Vector3.one * mock.Scale;
                newModel.layer = itemLayer;
                foreach (var transform in newModel.GetComponentsInChildren<Transform>())
                {
                    transform.gameObject.layer = itemLayer;
                }
            }
            ObjectDB.instance.m_items.Add(newObj);
            ZNetScene.instance.m_namedPrefabs[mock.UID.GetStableHashCode()] = newObj;
            ObjectDB.instance.m_itemByHash[mock.UID.GetStableHashCode()] = newObj;
        }
        CanMockItems = false;
    }

}
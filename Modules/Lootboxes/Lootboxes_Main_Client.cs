using ItemDataManager;
using Marketplace.ExternalLoads;

namespace Marketplace.Modules.Lootboxes;

[Market_Autoload(Market_Autoload.Type.Client)]
public static class Lootboxes_Main_Client
{
    public static GameObject LootBox_Base;
    private static Dictionary<int, Lootboxes_DataTypes.Lootbox> _mapper = new();

    private static Sprite Lootbox_DefaultIcon;

    [UsedImplicitly]
    private static void OnInit()
    {
        LootBox_Base = AssetStorage.asset.LoadAsset<GameObject>("LootBox_Base");
        LootBox_Base.GetComponent<ItemDrop>().m_itemData.Data().Add<Lootboxes_Logic.Lootbox_IDM>()
            .Assign(new()
            {
                UID = "Lootbox",
                AdditionalDescription = "<color=red>This is a default lootbox, you can edit it or delete it.</color>"
            });
        Lootbox_DefaultIcon = LootBox_Base.GetComponent<ItemDrop>().m_itemData.m_shared.m_icons[0];
        Lootboxes_DataTypes.SyncedLootboxData.ValueChanged += InitLootboxes;
    }

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    [ClientOnlyPatch]
    private static class ZNS_AddPrefab
    {
        [UsedImplicitly]
        private static void Postfix(ZNetScene __instance)
        {
            __instance.m_namedPrefabs[LootBox_Base.name.GetStableHashCode()] = LootBox_Base;
        }
    }

    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    [ClientOnlyPatch]
    private static class ObjectDB_Awake_Patch
    {
        [UsedImplicitly]
        private static void Postfix(ObjectDB __instance)
        {
            if (!__instance.m_items.Contains(LootBox_Base))
            {
                __instance.m_items.Add(LootBox_Base);
                __instance.m_itemByHash[LootBox_Base.name.GetStableHashCode()] = LootBox_Base;
            }
        }
    }

    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.CopyOtherDB))]
    [ClientOnlyPatch]
    private static class ObjectDB_CopyOtherDB_Patch
    {
        [UsedImplicitly]
        private static void Postfix(ObjectDB __instance, ObjectDB other)
        {
            if (!__instance.m_items.Contains(LootBox_Base))
            {
                __instance.m_items.Add(LootBox_Base);
                __instance.m_itemByHash[LootBox_Base.name.GetStableHashCode()] = LootBox_Base;
            }
        }
    }

    private static void InitLootboxes()
    {
        _mapper.Clear();
        foreach (Lootboxes_DataTypes.Lootbox lootbox in Lootboxes_DataTypes.SyncedLootboxData.Value)
        {
            _mapper[lootbox.UID.RemoveRichTextTags().GetStableHashCode()] = lootbox;
        }

        Terminal.commands["spawn"].m_tabOptions = null;
    }

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.GetPrefabNames))]
    [ClientOnlyPatch]
    private static class ZNetScene_GetPrefabNames_Patch
    {
        [UsedImplicitly]
        private static void Postfix(ref List<string> __result)
        {
            foreach (Lootboxes_DataTypes.Lootbox lootbox in Lootboxes_DataTypes.SyncedLootboxData.Value)
            {
                if (!__result.Contains(lootbox.UID))
                    __result.Add(lootbox.UID);
            }
        }
    }

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.GetPrefab), typeof(int))]
    [ClientOnlyPatch]
    private static class ZNetScene_GetPrefab_Patch
    {
        [UsedImplicitly]
        private static void Prefix(ref int hash)
        {
            if (_mapper.TryGetValue(hash, out var LB))
            {
                ItemDrop drop = LootBox_Base.GetComponent<ItemDrop>();
                drop.m_itemData.Data().Get<Lootboxes_Logic.Lootbox_IDM>().Assign(LB);
                drop.m_itemData.m_shared.m_name = LB.UID.Replace("_", " ");
                drop.m_itemData.m_shared.m_icons[0] = Utils.TryFindIcon(LB.Icon, Lootbox_DefaultIcon);
                hash = LootBox_Base.name.GetStableHashCode();
            }
        }
    }

    [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetIcon))]
    [ClientOnlyPatch]
    private static class ItemDrop__Patch
    {
        [UsedImplicitly]
        private static void Postfix(ItemDrop.ItemData __instance, ref Sprite __result)
        {
            if (__instance.Data().Get<Lootboxes_Logic.Lootbox_IDM>() is { } idm)
                __result = Utils.TryFindIcon(idm.GetIcon(), Lootbox_DefaultIcon);
        }
    }

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    [ClientOnlyPatch]
    private static class ZNetScene_Awake_Patch
    {
        [UsedImplicitly]
        private static void Postfix() => InitLootboxes();
    }
}
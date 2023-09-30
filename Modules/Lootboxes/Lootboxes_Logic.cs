using ItemDataManager;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Marketplace.Modules.Lootboxes;

public static class Lootboxes_Logic
{
    public class Lootbox_IDM : ItemData
    {
        [SerializeField] public Lootboxes_DataTypes.Lootbox _data;

        protected override bool AllowStackingIdenticalValues { get; set; } = true;

        public string GetName() => _data?.UID.Replace("_", " ") ?? "Lootbox";
        public string GetIcon() => _data?.Icon;
        public string GetDescription() => BuildDescription();

        public void Assign(Lootboxes_DataTypes.Lootbox data)
        {
            _data = data;
            Save();
        }

        bool validCheck(GameObject item) => item && (item.GetComponent<ItemDrop>() || item.GetComponent<Character>());

        private string BuildDescription()
        {
            StringBuilder sb = new();
            if (!string.IsNullOrEmpty(_data.AdditionalDescription))
                sb.AppendLine(_data.AdditionalDescription);
            if (_data.Items.Count == 0) return sb.ToString();
            sb.AppendLine("\n<color=orange>You have a chance to get:</color>");
            foreach (var item in _data.Items)
            {
                GameObject test = ZNetScene.instance.GetPrefab(item.Prefab);
                if (!validCheck(test))
                {
                    sb.AppendLine($"<color=red>ERROR: {item.Prefab} not found!</color>");
                    continue;
                }

                ItemDrop idrop = test.GetComponent<ItemDrop>();
                if (idrop)
                {
                    string displayLevel = idrop.m_itemData.m_shared.m_maxQuality > 1
                        ? $" <color=#00ff00>({item.Level}★)</color>"
                        : "";
                    string displayAmouny = item.Min == item.Max ? $"{item.Min}" : $"{item.Min}-{item.Max}";
                    sb.AppendLine(
                        $"<color=yellow>{displayAmouny}x {idrop.m_itemData.m_shared.m_name.Localize()}{displayLevel}</color>");
                }
                else
                {
                    Character character = test.GetComponent<Character>();
                    string displayLevel = $" <color=#00ff00>({item.Level - 1}★)</color>";
                    string displayAmouny = item.Min == item.Max ? $"{item.Min}" : $"{item.Min}-{item.Max}";
                    string displayTamed = character.GetComponent<Tameable>()
                        ? " (<color=green>Tamed</color>)"
                        : " (<color=red>Hostile</color>)";
                    sb.AppendLine(
                        $"<color=yellow>{displayAmouny}x {character.m_name.Localize()}{displayLevel}{displayTamed}</color>");
                }
            }

            return sb.ToString();
        }

        public void Open()
        {
            List<Lootboxes_DataTypes.Lootbox.Item> validOnly =
                _data.Items.Where(i => validCheck(ZNetScene.instance.GetPrefab(i.Prefab))).ToList();
            if (validOnly.Count == 0) return;
            if (_data.GiveAll)
            {
                Chat.instance.m_hideTimer = 0f;
                foreach (var item in validOnly)
                {
                    int randomAmount = Random.Range(item.Min, item.Max + 1);
                    GameObject obj = ZNetScene.instance.GetPrefab(item.Prefab);
                    Utils.InstantiateItem(obj, randomAmount, item.Level);
                    string prefabLocalized = obj.GetComponent<ItemDrop>()
                        ? obj.GetComponent<ItemDrop>().m_itemData.m_shared.m_name.Localize()
                        : obj.GetComponent<Character>().m_name.Localize();
                    Chat.instance.AddString($"<color=yellow>You got {randomAmount}x {prefabLocalized}!</color>");
                    MessageHud.instance.ShowMessage(MessageHud.MessageType.Center,
                        $"You got {randomAmount}x {prefabLocalized}!");
                }

                string vfx = _data.OpenVFX;
                if (vfx != null && ZNetScene.instance.GetPrefab(vfx) is { } vfxPrefab)
                    Object.Instantiate(vfxPrefab, Player.m_localPlayer.transform.position, Quaternion.identity);
            }
            else
            {
                Lootboxes_DataTypes.Lootbox.Item item = validOnly[Random.Range(0, validOnly.Count)];
                int randomAmount = Random.Range(item.Min, item.Max + 1);
                GameObject obj = ZNetScene.instance.GetPrefab(item.Prefab);
                Utils.InstantiateItem(obj, randomAmount, item.Level);
                string prefabLocalized = obj.GetComponent<ItemDrop>()
                    ? obj.GetComponent<ItemDrop>().m_itemData.m_shared.m_name.Localize()
                    : obj.GetComponent<Character>().m_name.Localize();
                Chat.instance.m_hideTimer = 0f;
                Chat.instance.AddString($"<color=yellow>You got {randomAmount}x {prefabLocalized}!</color>");
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center,
                    $"You got {randomAmount}x {prefabLocalized}!");
                string vfx = _data.OpenVFX;
                if (vfx != null && ZNetScene.instance.GetPrefab(vfx) is { } vfxPrefab)
                    Object.Instantiate(vfxPrefab, Player.m_localPlayer.transform.position, Quaternion.identity);
            }
        }
    }

    [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetTooltip), typeof(ItemDrop.ItemData),
        typeof(int), typeof(bool), typeof(float))]
    [ClientOnlyPatch]
    private static class ItemDrop__Patch
    {
        [UsedImplicitly]
        private static void Postfix(ItemDrop.ItemData item, bool crafting, ref string __result)
        {
            if (item.Data().Get<Lootbox_IDM>() is { } idm && !crafting)
                __result = idm.GetDescription();
        }
    }

    [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.CreateItemTooltip))]
    [ClientOnlyPatch]
    private static class InventoryGrid_CreateItemTooltip_Patch
    {
        [UsedImplicitly]
        private static void Prefix(ItemDrop.ItemData item)
        {
            if (item.Data().Get<Lootbox_IDM>() is { } idm)
                item.m_shared.m_name = idm.GetName();
        }
    }

    [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.Start))]
    [ClientOnlyPatch]
    private static class ItemDrop_GetHoverText_Patch
    {
        [UsedImplicitly]
        private static void Postfix(ItemDrop __instance)
        {
            if (__instance.m_itemData.Data().Get<Lootbox_IDM>() is { } idm)
                __instance.m_itemData.m_shared.m_name = idm.GetName();
        }
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnRightClickItem))]
    [ClientOnlyPatch]
    private static class Player_ConsumeItem_PatchDItem
    {
        private static bool Prefix(InventoryGrid grid, ItemDrop.ItemData item)
        {
            if (grid.m_inventory != Player.m_localPlayer.m_inventory) return true;
            if (item != null && Player.m_localPlayer && item.Data().Get<Lootbox_IDM>() is {} idm)
            {
                grid.m_inventory.RemoveOneItem(item);
                idm.Open();
                return false;
            }
            return true;
        }
    }
}
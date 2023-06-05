using ItemDataManager;

namespace Marketplace.Modules.Transmogrification;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Normal, "OnInit")]
public static class Transmogrification_Main_Client
{
    private static GameObject Transmog_UI_Icon_Part;
    private static readonly List<GameObject> MEL_Effects = new(20);
    public static readonly Dictionary<string, Dictionary<ItemDrop.ItemData.ItemType, List<Transmogrification_DataTypes.TransmogItem_Data>>> FilteredTransmogData = new();

    private static void OnInit()
    {
        Transmogrification_UI.Init();
        Transmog_UI_Icon_Part = AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("TransmogUI_Part");
        for (int i = 0; i < 20; i++)
        {
            MEL_Effects.Add(AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("TransmogME" + (i + 1)));
        }
        Transmogrification_DataTypes.TransmogData.ValueChanged += InitTransmogData;
        Marketplace.Global_Updator += Update;
    }
    
    private static void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape) || !Transmogrification_UI.IsVisble()) return;
        Transmogrification_UI.Hide();
        Menu.instance.OnClose();
    }

    private static void InitTransmogData()
    {
        FilteredTransmogData.Clear();
        int count = 0;
        foreach (KeyValuePair<string, List<Transmogrification_DataTypes.TransmogItem_Data>> profile in
                 Transmogrification_DataTypes.TransmogData.Value)
        {
            if (!FilteredTransmogData.ContainsKey(profile.Key)) FilteredTransmogData.Add(profile.Key, new Dictionary<ItemDrop.ItemData.ItemType, List<Transmogrification_DataTypes.TransmogItem_Data>>());
            foreach (Transmogrification_DataTypes.TransmogItem_Data data in profile.Value)
            {
                GameObject prefab = ZNetScene.instance.GetPrefab(data.Prefab);
                GameObject priceItem = ZNetScene.instance.GetPrefab(data.Price_Prefab);
                if (!prefab || !priceItem) continue;
                ItemDrop.ItemData item = prefab.GetComponent<ItemDrop>().m_itemData;
                data.SetIcon(item.m_shared.m_icons[0]);
                string localizedName = Localization.instance.Localize(item.m_shared.m_name);
                if (data.VFX_ID > 0)
                    localizedName +=
                        $" <color=#00FFFF>({Localization.instance.Localize("$mpasn_transmog_eff" + data.VFX_ID)})</color>";
                data.SetLocalizedName(localizedName);
                data.SetLocalizedPrice(Localization.instance.Localize("$mpasn_transmog_price: <color=#00ff00>") +
                                       Localization.instance.Localize(priceItem.GetComponent<ItemDrop>().m_itemData
                                           .m_shared.m_name) + "</color>");
                data.SetPriceIcon(priceItem.GetComponent<ItemDrop>().m_itemData.m_shared.m_icons[0]);
                if (data.IgnoreCategory)
                {
                    if (FilteredTransmogData[profile.Key].ContainsKey((ItemDrop.ItemData.ItemType)999))
                    {
                        FilteredTransmogData[profile.Key][(ItemDrop.ItemData.ItemType)999].Add(data);
                    }
                    else
                    {
                        FilteredTransmogData[profile.Key]
                            .Add((ItemDrop.ItemData.ItemType)999,
                                new List<Transmogrification_DataTypes.TransmogItem_Data>() { data });
                    }

                    continue;
                }

                if (FilteredTransmogData[profile.Key].ContainsKey(item.m_shared.m_itemType))
                {
                    FilteredTransmogData[profile.Key][item.m_shared.m_itemType].Add(data);
                }
                else
                {
                    FilteredTransmogData[profile.Key].Add(item.m_shared.m_itemType,
                        new List<Transmogrification_DataTypes.TransmogItem_Data>() { data });
                }
            }
        }

        Transmogrification_UI.Reload();
    }


    private static string AddTooltipTransmog(ItemDrop.ItemData item)
    {
        TempItem transmog = item.T_Data();
        string go = ZNetScene.instance.GetPrefab(transmog.ReplacedPrefab)?.GetComponent<ItemDrop>()?.m_itemData
            .m_shared.m_name ?? "";
        string result =
            Localization.instance.Localize(
                $"\n<color=#FF00FF>$mpasn_transmog_transmogrifiedinfo: <color=#00FFFF>{go}</color></color>");
        if (transmog.VFX_ID > 0)
            result +=
                $"\n<color=#FF00FF>VFX: <color=#00FFFF>{Localization.instance.Localize("$mpasn_transmog_eff" + transmog.VFX_ID)}</color></color>";
        return result;
    }

    public static bool HasTransmog(this ItemDrop.ItemData item)
    {
        return item?.Data().Get<Transmogrification_DataTypes.TransmogItem_Component>() != null;
    }

    private struct TempItem
    {
        public string ReplacedPrefab;
        public int Variant;
        public int VFX_ID;
    }
    
    
    private static TempItem T_Data(this ItemDrop.ItemData item)
    {
        if (item == null) return new TempItem { ReplacedPrefab = "", Variant = 0, VFX_ID = 0 };

        if (item.Data().Get<Transmogrification_DataTypes.TransmogItem_Component>() is { } t)
            return new TempItem { ReplacedPrefab = t.ReplacedPrefab, Variant = 0, VFX_ID = t.VFX_ID };
        return new TempItem { ReplacedPrefab = item.m_dropPrefab.name, Variant = item.m_variant, VFX_ID = 0 };
    }


    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.SetupVisEquipment))]
    [ClientOnlyPatch]
    private static class Humanoid_SetupVisEquipment_Patch_TRANSMOG
    {
        [HarmonyPriority(-6000)]
        public static bool Prefix(Humanoid __instance, VisEquipment visEq, bool isRagdoll)
        {
            Player p = ZNetScene.instance ? Player.m_localPlayer : __instance as Player;
            if (__instance == p)
            {
                if (!isRagdoll)
                {
                    //////////LEFT ITEM
                    TempItem LeftItem = p!.m_leftItem.T_Data();
                    visEq.SetLeftItem(LeftItem.ReplacedPrefab, LeftItem.Variant);
                    ///////////RIGHT ITEM
                    TempItem RightItem = p!.m_rightItem.T_Data();
                    visEq.SetRightItem(RightItem.ReplacedPrefab);
                    ///////////LEFT BACK ITEM 
                    TempItem LeftBackItem = p!.m_hiddenLeftItem.T_Data();
                    visEq.SetLeftBackItem(LeftBackItem.ReplacedPrefab, LeftBackItem.Variant);
                    ///////////RIGHT BACK ITEM
                    TempItem RightBackItem = p!.m_hiddenRightItem.T_Data();
                    visEq.SetRightBackItem(RightBackItem.ReplacedPrefab);

                    if (ZNetScene.instance)
                    {
                        p.m_nview.m_zdo.Set("MPASN_TMGrightitem", RightItem.VFX_ID);
                        p.m_nview.m_zdo.Set("MPASN_TMGleftitem", LeftItem.VFX_ID);
                        p.m_nview.m_zdo.Set("MPASN_TMGleftbackitem", LeftBackItem.VFX_ID);
                        p.m_nview.m_zdo.Set("MPASN_TMGrightbackitem", RightBackItem.VFX_ID);
                    }
                }

                ///////////CHEST ITEM
                TempItem ChestItem = p!.m_chestItem.T_Data();
                visEq.SetChestItem(ChestItem.ReplacedPrefab);
                ///////////LEG ITEM
                TempItem LegItem = p!.m_legItem.T_Data();
                visEq.SetLegItem(LegItem.ReplacedPrefab);
                ///////////Helmet 
                TempItem HelmetItem = p!.m_helmetItem.T_Data();
                visEq.SetHelmetItem(HelmetItem.ReplacedPrefab);
                ///////////SHOULDER ITEM 
                TempItem ShoulderItem = p!.m_shoulderItem.T_Data();
                visEq.SetShoulderItem(ShoulderItem.ReplacedPrefab, ShoulderItem.Variant);

                if (ZNetScene.instance)
                {
                    p.m_nview.m_zdo.Set("MPASN_TMGchestitem", ChestItem.VFX_ID);
                    p.m_nview.m_zdo.Set("MPASN_TMGlegitem", LegItem.VFX_ID);
                    p.m_nview.m_zdo.Set("MPASN_TMGhelmetitem", HelmetItem.VFX_ID);
                    p.m_nview.m_zdo.Set("MPASN_TMGshoulderitem", ShoulderItem.VFX_ID);
                }

                visEq.SetUtilityItem((p.m_utilityItem != null) ? p.m_utilityItem.m_dropPrefab.name : "");
                visEq.SetBeardItem(p.m_beardItem);
                visEq.SetHairItem(p.m_hairItem);

                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.Awake))]
    [ClientOnlyPatch]
    public static class InventoryGrid_Awake_Patch
    {
        public static void Postfix(InventoryGrid __instance)
        {
            if (!__instance.m_elementPrefab) return;
            Transform transform = __instance.m_elementPrefab.transform;
            GameObject newIcon = UnityEngine.Object.Instantiate(Transmog_UI_Icon_Part);
            newIcon!.transform.SetParent(transform);
            newIcon.name = "MPASN_Transmog";
            newIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -26);
            newIcon.gameObject.SetActive(false);
        }
    }


    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Awake))]
    [ClientOnlyPatch]
    private static class Game_Awake_Patch_Transmog
    {
        private static void Postfix()
        {
            HotkeyBar_UpdateIcons_Patch.FirstInit = false;
        }
    }

    [HarmonyPatch(typeof(HotkeyBar), nameof(HotkeyBar.UpdateIcons))]
    [ClientOnlyPatch]
    private static class HotkeyBar_UpdateIcons_Patch
    {
        public static bool FirstInit;

        public static void Postfix(HotkeyBar __instance)
        {
            if (__instance.gameObject.name != "HotKeyBar") return;
            if (!FirstInit)
            {
                FirstInit = true;
                Transform transform = __instance.m_elementPrefab.transform;
                GameObject newIcon = UnityEngine.Object.Instantiate(Transmog_UI_Icon_Part);
                newIcon!.transform.SetParent(transform);
                newIcon.name = "MPASN_Transmog";
                newIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -26);
                newIcon.gameObject.SetActive(false);
            }

            if (!Player.m_localPlayer || Player.m_localPlayer.IsDead()) return;
            foreach (HotkeyBar.ElementData element in __instance.m_elements.Where(element => !element.m_used))
            {
                element.m_go.transform.Find("MPASN_Transmog").gameObject.SetActive(false);
            }

            for (int j = 0; j < __instance.m_items.Count; j++)
            {
                ItemDrop.ItemData itemData = __instance.m_items[j];
                HotkeyBar.ElementData element = __instance.m_elements[itemData.m_gridPos.x];
                element.m_go.transform.Find("MPASN_Transmog").gameObject
                    .SetActive(itemData.HasTransmog());
            }
        }
    }

    [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.UpdateGui))]
    [ClientOnlyPatch]
    private static class InventoryGrid_UpdateGui_Patch
    {
        public static void Postfix(InventoryGrid __instance)
        {
            int width = __instance.m_inventory.GetWidth();
            foreach (InventoryGrid.Element element in __instance.m_elements)
            {
                if (!element.m_used) element.m_go.transform.Find("MPASN_Transmog")?.gameObject.SetActive(false);
            }

            foreach (ItemDrop.ItemData itemData in __instance.m_inventory.GetAllItems())
            {
                InventoryGrid.Element element =
                    __instance.GetElement(itemData.m_gridPos.x, itemData.m_gridPos.y, width);
                element.m_go.transform.Find("MPASN_Transmog")?.gameObject
                    .SetActive(itemData.HasTransmog());
            }
        }
    }

    [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetTooltip), typeof(ItemDrop.ItemData),
        typeof(int), typeof(bool))]
    [ClientOnlyPatch]
    private static class ItemDrop_ItemData_Tooltip__Patch_TRANSMOG
    {
        public static void Postfix(ItemDrop.ItemData item, ref string __result)
        {
            if (item.HasTransmog())
            {
                __result += AddTooltipTransmog(item);
            }
        }
    }

    private static GameObject MEL_GetEffect(int index)
    {
        index--;
        if (index < 0 || index >= MEL_Effects.Count) return null;
        return MEL_Effects[index];
    }
    
    [HarmonyPatch(typeof(VisEquipment), nameof(VisEquipment.SetRightHandEquipped))]
    [ClientOnlyPatch]
    private static class MockRight
    {
        private static bool Transfer;

        private static void Prefix(VisEquipment __instance, int hash)
        {
            if (__instance.m_currentRightItemHash != hash)
            {
                Transfer = true;
            }
        }


        private static void Postfix(VisEquipment __instance)
        {
            if (!Transfer || !__instance.m_nview || __instance.m_nview.m_zdo == null) return;
            Transfer = false;
            if (__instance.m_rightItemInstance)
            {
                int test = __instance.m_nview.m_zdo.GetInt("MPASN_TMGrightitem");
                if (test > 0)
                {
                    GameObject go = UnityEngine.Object.Instantiate(MEL_GetEffect(test),
                        __instance.m_rightItemInstance.transform);
                    PSMeshRendererUpdater update = go.GetComponent<PSMeshRendererUpdater>();
                    update.MeshObject = go.transform.parent.gameObject;
                    update.UpdateMeshEffect();
                }
            }
        }
    }


    [HarmonyPatch(typeof(VisEquipment), nameof(VisEquipment.SetLeftHandEquipped))]
    [ClientOnlyPatch]
    private static class MockLeft
    {
        private static bool Transfer;

        private static void Prefix(VisEquipment __instance, int hash)
        {
            if (__instance.m_currentLeftItemHash != hash)
            {
                Transfer = true;
            }
        }


        private static void Postfix(VisEquipment __instance)
        {
            if (!Transfer || !__instance.m_nview || __instance.m_nview.m_zdo == null) return;
            Transfer = false;
            if (__instance.m_leftItemInstance)
            {
                int test = __instance.m_nview.m_zdo.GetInt("MPASN_TMGleftitem");
                if (test > 0)
                {
                    GameObject go = UnityEngine.Object.Instantiate(MEL_GetEffect(test),
                        __instance.m_leftItemInstance.transform);
                    PSMeshRendererUpdater update = go.GetComponent<PSMeshRendererUpdater>();
                    update.MeshObject = go.transform.parent.gameObject;
                    update.UpdateMeshEffect();
                }
            }
        }
    }

    [HarmonyPatch(typeof(VisEquipment), nameof(VisEquipment.SetHelmetEquipped))]
    [ClientOnlyPatch]
    private static class MockHelmet
    {
        private static bool Transfer;

        private static void Prefix(VisEquipment __instance, int hash)
        {
            if (__instance.m_currentHelmetItemHash != hash)
            {
                Transfer = true;
            }
        }


        private static void Postfix(VisEquipment __instance)
        {
            if (!Transfer || !__instance.m_nview || __instance.m_nview.m_zdo == null) return;
            Transfer = false;
            if (__instance.m_helmetItemInstance)
            {
                int test = __instance.m_nview.m_zdo.GetInt("MPASN_TMGhelmetitem");
                if (test > 0)
                {
                    GameObject go = UnityEngine.Object.Instantiate(MEL_GetEffect(test),
                        __instance.m_helmetItemInstance.transform);
                    PSMeshRendererUpdater update = go.GetComponent<PSMeshRendererUpdater>();
                    foreach (ParticleSystem eff in go.GetComponentsInChildren<ParticleSystem>())
                    {
                        eff.gameObject.SetActive(false);
                    }

                    update.MeshObject = go.transform.parent.gameObject;
                    update.UpdateMeshEffect();
                }
            }
        }
    }

    [HarmonyPatch(typeof(VisEquipment), nameof(VisEquipment.SetShoulderEquipped))]
    [ClientOnlyPatch]
    private static class MockCape
    {
        private static bool Transfer;

        private static void Prefix(VisEquipment __instance, int hash)
        {
            if (__instance.m_currentShoulderItemHash != hash)
            {
                Transfer = true;
            }
        }


        private static void Postfix(VisEquipment __instance)
        {
            if (!Transfer || !__instance.m_nview || __instance.m_nview.m_zdo == null) return;
            Transfer = false;
            if (__instance.m_shoulderItemInstances is { Count: > 0 })
            {
                int test = __instance.m_nview.m_zdo.GetInt("MPASN_TMGshoulderitem");
                if (test > 0)
                {
                    foreach (GameObject VARIABLE in __instance.m_shoulderItemInstances)
                    {
                        GameObject go = UnityEngine.Object.Instantiate(MEL_GetEffect(test), VARIABLE.transform);
                        PSMeshRendererUpdater update = go.GetComponent<PSMeshRendererUpdater>();
                        foreach (ParticleSystem eff in go.GetComponentsInChildren<ParticleSystem>())
                        {
                            eff.gameObject.SetActive(false);
                        }

                        update.MeshObject = go.transform.parent.gameObject;
                        update.UpdateMeshEffect();
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(VisEquipment), nameof(VisEquipment.SetLegEquipped))]
    [ClientOnlyPatch]
    private static class MockLegs
    {
        private static bool Transfer;

        private static void Prefix(VisEquipment __instance, int hash)
        {
            if (__instance.m_currentLegItemHash != hash)
            {
                Transfer = true;
            }
        }


        private static void Postfix(VisEquipment __instance)
        {
            if (!Transfer || !__instance.m_nview || __instance.m_nview.m_zdo == null) return;
            Transfer = false;
            if (__instance.m_legItemInstances is { Count: > 0 })
            {
                int test = __instance.m_nview.m_zdo.GetInt("MPASN_TMGlegitem");
                if (test > 0)
                {
                    foreach (GameObject VARIABLE in __instance.m_legItemInstances)
                    {
                        GameObject go = UnityEngine.Object.Instantiate(MEL_GetEffect(test), VARIABLE.transform);
                        PSMeshRendererUpdater update = go.GetComponent<PSMeshRendererUpdater>();
                        foreach (ParticleSystem eff in go.GetComponentsInChildren<ParticleSystem>())
                        {
                            eff.gameObject.SetActive(false);
                        }

                        update.MeshObject = go.transform.parent.gameObject;
                        update.UpdateMeshEffect();
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(VisEquipment), nameof(VisEquipment.SetBackEquipped))]
    [ClientOnlyPatch]
    private static class MockBackItems
    {
        private static bool Transfer;

        private static void Prefix(VisEquipment __instance, int leftItem, int rightItem, int leftVariant)
        {
            if (__instance.m_currentLeftBackItemHash != leftItem ||
                __instance.m_currentRightBackItemHash != rightItem ||
                __instance.m_currentLeftBackItemVariant != leftVariant)
            {
                Transfer = true;
            }
        }


        private static void Postfix(VisEquipment __instance)
        {
            if (!Transfer || !__instance.m_nview || __instance.m_nview.m_zdo == null) return;
            Transfer = false;
            if (__instance.m_leftBackItemInstance)
            {
                int test = __instance.m_nview.m_zdo.GetInt("MPASN_TMGleftbackitem");
                if (test > 0)
                {
                    GameObject go = UnityEngine.Object.Instantiate(MEL_GetEffect(test),
                        __instance.m_leftBackItemInstance.transform);
                    PSMeshRendererUpdater update = go.GetComponent<PSMeshRendererUpdater>();
                    update.MeshObject = go.transform.parent.gameObject;
                    update.UpdateMeshEffect();
                }
            }

            if (__instance.m_rightBackItemInstance)
            {
                int test = __instance.m_nview.m_zdo.GetInt("MPASN_TMGrightbackitem");
                if (test > 0)
                {
                    GameObject go = UnityEngine.Object.Instantiate(MEL_GetEffect(test),
                        __instance.m_rightBackItemInstance.transform);
                    PSMeshRendererUpdater update = go.GetComponent<PSMeshRendererUpdater>();
                    update.MeshObject = go.transform.parent.gameObject;
                    update.UpdateMeshEffect();
                }
            }
        }
    }

    [HarmonyPatch(typeof(VisEquipment), nameof(VisEquipment.SetChestEquipped))]
    [ClientOnlyPatch]
    private static class MockChest
    {
        private static bool Transfer;

        private static void Prefix(VisEquipment __instance, int hash)
        {
            if (__instance.m_currentChestItemHash != hash)
            {
                Transfer = true;
            }
        }


        private static void Postfix(VisEquipment __instance)
        {
            if (!Transfer || !__instance.m_nview || __instance.m_nview.m_zdo == null) return;
            Transfer = false;
            if (__instance.m_chestItemInstances is { Count: > 0 })
            {
                int test = __instance.m_nview.m_zdo.GetInt("MPASN_TMGchestitem");
                if (test > 0)
                {
                    foreach (GameObject VARIABLE in __instance.m_chestItemInstances)
                    {
                        GameObject go = UnityEngine.Object.Instantiate(MEL_GetEffect(test), VARIABLE.transform);
                        PSMeshRendererUpdater update = go.GetComponent<PSMeshRendererUpdater>();
                        foreach (ParticleSystem eff in go.GetComponentsInChildren<ParticleSystem>())
                        {
                            eff.gameObject.SetActive(false);
                        }

                        update.MeshObject = go.transform.parent.gameObject;
                        update.UpdateMeshEffect();
                    }
                }
            }
        }
    }
}
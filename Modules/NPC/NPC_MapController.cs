﻿using Marketplace.ExternalLoads;

namespace Marketplace.Modules.NPC;

public static class NPC_MapController
{
    private static readonly Dictionary<Minimap.PinData, ZDO> _pins = new();
    private const string npcToSearchPrefabName = "MarketPlaceNPC";
    private const string npcToSearchPrefabName_Pinned = "MarketPlaceNPCpinned";
    private const Minimap.PinType PINTYPENPC = (Minimap.PinType)72;
    
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.GetSprite))]
    [ClientOnlyPatch]
    private static class Minimap_GetSprite_Patch
    {
        [UsedImplicitly]
        private static void Postfix(Minimap.PinType type, ref Sprite __result)
        {
            if (type is PINTYPENPC) __result = AssetStorage.NPC_MapControl;
        }
    }

    [HarmonyPatch(typeof(Minimap), nameof(Minimap.SetMapMode))]
    [ClientOnlyPatch]
    private static class NPC_MapControllerPatch
    {
        public static void ReapplyPins()
        {
            foreach (var pin in _pins) Minimap.instance.RemovePin(pin.Key);
            _pins.Clear();
            List<ZDO> AllNPCs = new();
            int index = 0;
            while (!ZDOMan.instance.GetAllZDOsWithPrefabIterative(npcToSearchPrefabName, AllNPCs, ref index)) { }
            index = 0;
            while (!ZDOMan.instance.GetAllZDOsWithPrefabIterative(npcToSearchPrefabName_Pinned, AllNPCs, ref index)) { }

            foreach (ZDO zdo in AllNPCs)
            {
                if (!zdo.IsValid()) continue;
                string name = zdo.GetString("KGnpcNameOverride");
                int type = zdo.GetInt("KGmarketNPC");
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = Localization.instance.Localize("$" + (Market_NPC.NPCType)type);
                }

                Minimap.PinData pinData = new Minimap.PinData
                {
                    m_type = PINTYPENPC,
                    m_name = Utils.RichTextFormatting(name),
                    m_pos = zdo.GetPosition(),
                };
                if (!string.IsNullOrEmpty(pinData.m_name))
                {
                    pinData.m_NamePinData = new Minimap.PinNameData(pinData);
                }

                pinData.m_icon = AssetStorage.NPC_MapControl;
                pinData.m_save = false;
                pinData.m_checked = false;
                pinData.m_ownerID = 0L;
                _pins.Add(pinData, zdo);
            }

            foreach (var p in _pins)
            {
                Minimap.instance.m_pins.Add(p.Key);
            }
        }

        [UsedImplicitly]
        private static void Prefix(Minimap __instance, Minimap.MapMode mode)
        {
            if (mode != Minimap.MapMode.Large)
            {
                foreach (var pin in _pins) __instance.RemovePin(pin.Key);
                _pins.Clear();
            }

            if (mode != Minimap.MapMode.Large) return;
            if (!Utils.IsDebug) return;
            ReapplyPins();
        }
    }
    
    private static bool Control(bool leftClick)
    {
        Vector3 pos = Minimap.instance.ScreenToWorldPoint(Input.mousePosition);
        Minimap.PinData closestPin = Utils.GetCustomPin(PINTYPENPC, pos, Minimap.instance.m_removeRadius * (Minimap.instance.m_largeZoom * 2f));
        if (closestPin != null && _pins.TryGetValue(closestPin, out var zdo))
        {
            if (leftClick)
            {
                Market_NPC.NPCUI.ShowMain(zdo,NPC_MapControllerPatch.ReapplyPins);
            }
            else
            {
                Market_NPC.NPCUI.ShowFashion(zdo,NPC_MapControllerPatch.ReapplyPins);
            }
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(Minimap), nameof(Minimap.OnMapLeftClick))]
    [ClientOnlyPatch]
    private static class PatchClickIconMinimap
    {
        [UsedImplicitly]
        private static bool Prefix() => Control(true);
    }
    
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.OnMapRightClick))]
    [ClientOnlyPatch]
    private static class PatchRightClickIconMinimap
    {
        [UsedImplicitly]
        private static bool Prefix() => Control(false);
    }
}
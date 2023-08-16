using System.Diagnostics.CodeAnalysis;
using Marketplace.ExternalLoads;
using Marketplace.Modules.Quests;

namespace Marketplace.Modules.NPC;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Both)]
[SuppressMessage("ReSharper", "IteratorNeverReturns")]
public static class NPC_MapPins
{
    private const string npcToSearchPrefabName = "MarketPlaceNPC";
    private const string npcToSearchPrefabName_Pinned = "MarketPlaceNPCpinned";
    private static readonly List<ZDO> TempNPCList = new();
    private static readonly List<Minimap.PinData> TempNPCpins = new();
    public const Minimap.PinType PINTYPENPC = (Minimap.PinType)175;
    private static Sprite QuestCompleteIcon = null!;

    [UsedImplicitly]
    private static void OnInit()
    {
        Marketplace._thistype.StartCoroutine(SendNPCsToClients());
        Marketplace._thistype.StartCoroutine(UpdateNPCpins());
    }

    private static IEnumerator UpdateNPCpins()
    {
        while (true)
        {
            if (Player.m_localPlayer && ZDOMan.instance != null && Minimap.instance)
            {
                foreach (Minimap.PinData obj in TempNPCpins) Minimap.instance.RemovePin(obj);
                TempNPCpins.Clear();
                if (Minimap.instance.m_mode == Minimap.MapMode.Large && Utils.IsDebug)
                {
                    yield return new WaitForSecondsRealtime(1f);
                    continue;
                }
                List<ZDO> AllNPCs = new();
                int index = 0;
                while (!ZDOMan.instance.GetAllZDOsWithPrefabIterative(npcToSearchPrefabName_Pinned, AllNPCs, ref index))
                {
                    if (!Player.m_localPlayer || !Minimap.instance) break;
                    yield return null;
                }

                foreach (ZDO zdo in AllNPCs)
                {
                    if (!zdo.IsValid()) continue;
                    string name = zdo.GetString("KGnpcNameOverride");
                    int type = zdo.GetInt("KGmarketNPC");
                    bool questTarget = Quests_DataTypes.Quest.IsQuestTarget(name);
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

                    Sprite icon = AssetStorage.PlaceholderGamblerIcon;

                    if (questTarget)
                    {
                        if (!QuestCompleteIcon)
                            QuestCompleteIcon = AssetStorage.asset.LoadAsset<Sprite>("questcomplete");
                        icon = QuestCompleteIcon;
                        pinData.m_animate = true;
                        pinData.m_doubleSize = true;
                    }
                    else
                    {
                        string[] split = name.Split(new[] { "<icon>" }, StringSplitOptions.RemoveEmptyEntries);
                        if (split.Length == 2)
                        {
                            split[1] = split[1].Replace("</icon>", "");
                            GameObject prefab = ZNetScene.instance.GetPrefab(split[1]);
                            if (AssetStorage.GlobalCachedSprites.ContainsKey(split[1]))
                            {
                                icon = AssetStorage.GlobalCachedSprites[split[1]];
                            }

                            if (!prefab) goto GOTOLABEL;
                            if (prefab.GetComponent<ItemDrop>())
                            {
                                icon = prefab.GetComponent<ItemDrop>().m_itemData.GetIcon();
                            }
                            else if (prefab.GetComponent<Character>())
                            {
                                PhotoManager.__instance.MakeSprite(prefab, 0.6f, 0.25f);
                                icon = PhotoManager.__instance.GetSprite(prefab.name,
                                    AssetStorage.PlaceholderMonsterIcon, 1);
                            }
                        }
                    }

                    GOTOLABEL:
                    pinData.m_icon = icon;
                    pinData.m_save = false;
                    pinData.m_checked = false;
                    pinData.m_ownerID = 0L;
                    TempNPCpins.Add(pinData);
                }
            
                foreach (Minimap.PinData p in TempNPCpins.OrderBy(pin => pin.m_animate))
                {
                    Minimap.instance.m_pins.Add(p);
                }
            }

            yield return new WaitForSecondsRealtime(10f);
        }
    }
    
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.GetSprite))]
    [ClientOnlyPatch]
    private static class Minimap_GetSprite_Patch
    {
        [UsedImplicitly]
private static void Postfix(Minimap.PinType type, ref Sprite __result)
        {
            if (type is PINTYPENPC) __result = AssetStorage.PlaceholderGamblerIcon;
        }
    }

    [HarmonyPatch(typeof(Minimap), nameof(Minimap.SetMapMode))]
    [ClientOnlyPatch]
    private static class NPC_MapControllerPatch
    {
        [UsedImplicitly]
        private static void Prefix(Minimap __instance, Minimap.MapMode mode)
        {
            if (mode != Minimap.MapMode.Large) return;
            if (Utils.IsDebug)
            {
                foreach (Minimap.PinData obj in TempNPCpins) __instance.RemovePin(obj);
                TempNPCpins.Clear();
            }
        }
    }

    private static IEnumerator SendNPCsToClients()
    {
        for (;;)
        {
            if (Game.instance && ZDOMan.instance != null && ZNet.instance && ZNet.instance.IsServer())
            {
                TempNPCList.Clear();
                int index = 0;
                while (!ZDOMan.instance.GetAllZDOsWithPrefabIterative(npcToSearchPrefabName, TempNPCList, ref index))
                {
                    yield return null;
                }

                while (!ZDOMan.instance.GetAllZDOsWithPrefabIterative(npcToSearchPrefabName_Pinned, TempNPCList,
                           ref index))
                {
                    yield return null;
                }

                foreach (ZDO zdo in TempNPCList)
                {
                    ZDOMan.instance.ForceSendZDO(zdo.m_uid);
                }
            }

            yield return new WaitForSeconds(10f);
        }
    }
}
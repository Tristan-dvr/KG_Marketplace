using System.Diagnostics.CodeAnalysis;
using Marketplace.Modules.Quests;

namespace Marketplace.Modules.NPC;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Both, Market_Autoload.Priority.Last, "OnInit")]
[SuppressMessage("ReSharper", "IteratorNeverReturns")]
public static class Market_NPC_MapPins
{
    private const string npcToSearchPrefabName = "MarketPlaceNPCpinned";
    private static readonly List<ZDO> TempNPCList = new();
    private static readonly List<Minimap.PinData> TempNPCpins = new();
    public const Minimap.PinType PINTYPENPC = (Minimap.PinType)175;
    private static Sprite QuestCompleteIcon;

    // ReSharper disable once UnusedMember.Global
    private static void OnInit()
    {
        Marketplace._thistype.StartCoroutine(SendNPCsToClients());
        Marketplace._thistype.StartCoroutine(UpdateNPCpins());
    }

    [HarmonyPatch(typeof(Minimap), nameof(Minimap.GetSprite))]
    [ClientOnlyPatch]
    private static class Minimap_GetSprite_Patch
    {
        private static void Postfix(Minimap.PinType type, ref Sprite __result)
        {
            if (type is PINTYPENPC) __result = AssetStorage.AssetStorage.PlaceholderGamblerIcon;
        }
    }

    private static IEnumerator UpdateNPCpins()
    {
        while (true)
        {
            if (Player.m_localPlayer && ZDOMan.instance != null && Minimap.instance)
            {
                foreach (Minimap.PinData obj in TempNPCpins) Minimap.instance.RemovePin(obj);
                TempNPCpins.Clear();
                List<ZDO> AllNPCs = new();
                int index = 0;
                while (!ZDOMan.instance.GetAllZDOsWithPrefabIterative(npcToSearchPrefabName, AllNPCs, ref index))
                {
                    if (!Player.m_localPlayer || ZDOMan.instance == null || !Minimap.instance) break;
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

                    Sprite icon = AssetStorage.AssetStorage.PlaceholderGamblerIcon;

                    if (questTarget)
                    {
                        if (!QuestCompleteIcon)
                            QuestCompleteIcon = AssetStorage.AssetStorage.asset.LoadAsset<Sprite>("questcomplete");
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
                            if (AssetStorage.AssetStorage.GlobalCachedSprites.ContainsKey(split[1]))
                            {
                                icon = AssetStorage.AssetStorage.GlobalCachedSprites[split[1]];
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
                                    AssetStorage.AssetStorage.PlaceholderMonsterIcon, 1);
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

                foreach (ZDO zdo in TempNPCList)
                {
                    ZDOMan.instance.ForceSendZDO(zdo.m_uid);
                }
            }

            yield return new WaitForSeconds(10f);
        }
    }
}
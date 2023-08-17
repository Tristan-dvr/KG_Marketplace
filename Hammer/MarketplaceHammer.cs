using System.Reflection.Emit;
using Marketplace.ExternalLoads;
using Marketplace.Modules.NPC;
using Marketplace.Modules.Transmogrification;
using Marketplace.Paths;
using Object = UnityEngine.Object;
using static Marketplace.Modules.NPC.NPC_DataTypes;
using Random = UnityEngine.Random;

namespace Marketplace.Hammer;

[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Last, "OnInit", new[] { "SN" }, new[] { "Reload" })]
public static class MarketplaceHammer
{
    private static Dictionary<string, KeyValuePair<NPC_Main, NPC_Fashion>> _npcData = new();
    private static List<Piece> _pieces = new();
    private static GameObject CopyFrom;

    private static GameObject INACTIVE = new GameObject("INACTIVE_SAVEDNPCS")
    {
        hideFlags = HideFlags.HideAndDontSave
    };

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    [ClientOnlyPatch]
    private static class ZNetScene_Awake_Patch
    {
        [UsedImplicitly]
        private static void Postfix(ZNetScene __instance)
        {
            var hammer = AssetStorage.asset.LoadAsset<GameObject>("MarketplaceHammer");
            __instance.m_namedPrefabs[hammer.name.GetStableHashCode()] = hammer;
        }
    }

    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    [ClientOnlyPatch]
    private static class ObjectDB_Awake_Patch
    {
        [UsedImplicitly]
        private static void Postfix(ObjectDB __instance)
        {
            var hammer = AssetStorage.asset.LoadAsset<GameObject>("MarketplaceHammer");
            __instance.m_items.Add(hammer);
            __instance.m_itemByHash[hammer.name.GetStableHashCode()] = hammer;
        }
    }

    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.CopyOtherDB))]
    [ClientOnlyPatch]
    private static class ObjectDB_Awake_Patch2
    {
        [UsedImplicitly]
        private static void Postfix(ObjectDB __instance)
        {
            var hammer = AssetStorage.asset.LoadAsset<GameObject>("MarketplaceHammer");
            __instance.m_items.Add(hammer);
            __instance.m_itemByHash[hammer.name.GetStableHashCode()] = hammer;
        }
    }

    [HarmonyPatch(typeof(PieceTable), nameof(PieceTable.UpdateAvailable))]
    [ClientOnlyPatch]
    private static class PieceTable_UpdateAvailable_Patch
    {
        [UsedImplicitly]
        private static void Postfix(PieceTable __instance)
        {
            if (__instance.name != "_MarketplacePieceTable") return;
            List<Piece> avaliablePieces = __instance.m_availablePieces[(int)Piece.PieceCategory.Misc];
            if (Utils.IsDebug)
            {
                foreach (var data in _pieces)
                    avaliablePieces.Add(data);
            }
            else
            {
                foreach (var data in _pieces)
                    avaliablePieces.Remove(data);
            }
        }
    }

    private enum SpecificationsMain
    {
        Type,
        NameOverride,
        Profile,
        Prefab,
        Patrol,
        Dialogue
    }

    private enum SpecificationsFashion
    {
        LeftItem,
        RightItem,
        HelmetItem,
        ChestItem,
        LegsItem,
        CapeItem,
        HairItem,
        HairColor,
        ModelScale,
        LeftItemHidden,
        RightItemHidden,
        InteractAnimation,
        GreetAnimation,
        ByeAnimation,
        GreetText,
        ByeText,
        SkinColor,
        CraftingAnimation,
        BeardItem,
        BeardColor,
        InteractAudioClip,
        TextSize,
        TextHeight,
        PeriodicAnimation,
        PeriodicAnimationTime,
        PeriodicSound,
        PeriodicSoundTime,
    }
    
    private static void ProcessSavedNPC(IReadOnlyList<string> profiles)
    {
        for (int i = 0; i < profiles.Count; ++i)
        {
            try
            {
                if (profiles[i].StartsWith("["))
                {
                    NPC_Main mainData = new();
                    NPC_Fashion fashionData = new();
                    string UID = profiles[i].Replace("[", "").Replace("]", "").ToLower();

                    string[] main = profiles[i + 1].Split(new string[] { "@|@" }, StringSplitOptions.None);
                    mainData.Type = (Market_NPC.NPCType)Enum.Parse(typeof(Market_NPC.NPCType), main[0].Trim());
                    mainData.NameOverride = main[1].Trim();
                    mainData.Profile = main[2].Trim();
                    mainData.Prefab = main[3].Trim();
                    mainData.Patrol = main[4].Trim();
                    mainData.Dialogue = main[5].Trim();

                    string[] fashion = profiles[i + 2].Split(new string[] { "@|@" }, StringSplitOptions.None);
                    fashionData.LeftItem = fashion[0].Trim();
                    fashionData.RightItem = fashion[1].Trim();
                    fashionData.HelmetItem = fashion[2].Trim();
                    fashionData.ChestItem = fashion[3].Trim();
                    fashionData.LegsItem = fashion[4].Trim();
                    fashionData.CapeItem = fashion[5].Trim();
                    fashionData.HairItem = fashion[6].Trim();
                    fashionData.HairColor = fashion[7].Trim();
                    fashionData.ModelScale = fashion[8].Trim();
                    fashionData.LeftItemHidden = fashion[9].Trim();
                    fashionData.RightItemHidden = fashion[10].Trim();
                    fashionData.InteractAnimation = fashion[11].Trim();
                    fashionData.GreetAnimation = fashion[12].Trim();
                    fashionData.ByeAnimation = fashion[13].Trim();
                    fashionData.GreetText = fashion[14].Trim();
                    fashionData.ByeText = fashion[15].Trim();
                    fashionData.SkinColor = fashion[16].Trim();
                    fashionData.CraftingAnimation = fashion[17].Trim();
                    fashionData.BeardItem = fashion[18].Trim();
                    fashionData.BeardColor = fashion[19].Trim();
                    fashionData.InteractAudioClip = fashion[20].Trim();
                    fashionData.TextSize = fashion[21].Trim();
                    fashionData.TextHeight = fashion[22].Trim();
                    fashionData.PeriodicAnimation = fashion[23].Trim();
                    fashionData.PeriodicAnimationTime = fashion[24].Trim();
                    fashionData.PeriodicSound = fashion[25].Trim();
                    fashionData.PeriodicSoundTime = fashion[26].Trim();
                    
                    string image = profiles[i + 3];
                    _npcData[UID] = new KeyValuePair<NPC_Main, NPC_Fashion>(mainData, fashionData);
                    GameObject copy = Object.Instantiate(CopyFrom, INACTIVE.transform);
                    copy.name = "MARKETNPC_HAMMER_BUILDED";
                    copy.GetComponent<Piece>().m_name = UID;
                    copy.GetComponent<Piece>().m_description = mainData.Description;
                    Texture2D loadTex = new(1, 1);
                    loadTex.LoadImage(Convert.FromBase64String(image));
                    copy.GetComponent<Piece>().m_icon = Sprite.Create(loadTex, new Rect(0, 0, loadTex.width, loadTex.height), new Vector2(0, 0));
                    _pieces.Add(copy.GetComponent<Piece>());
                    i += 3;
                }
            }
            catch (Exception ex)
            {
                Utils.print($"Failed to load NPC from file\n: {ex}");
            }
        }
    }

    [UsedImplicitly]
    private static void OnInit()
    {
        INACTIVE.SetActive(false);
        CopyFrom = AssetStorage.asset.LoadAsset<GameObject>("MarketplaceHammerCopyFrom");
        Reload();
    }

    private static void Reload()
    {
        Utils.print($"Reloading NPC Hammer List");
        _npcData.Clear();
        _pieces.Clear();
        foreach (Transform transform in INACTIVE.transform)
            Object.Destroy(transform.gameObject);
        string folder = Market_Paths.NPC_Saved;
        string[] files = Directory.GetFiles(folder, "*.cfg", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            IReadOnlyList<string> profiles = File.ReadAllLines(file).ToList();
            ProcessSavedNPC(profiles);
        }
    }

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
    [ClientOnlyPatch]
    private static class Terminal_InitTerminal_Patch
    {
        [UsedImplicitly]
        private static void Postfix(Terminal __instance)
        {
            new Terminal.ConsoleCommand("reloadnpcs", "Reloads all saved NPCs", (_) =>
            {
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, $"Reloading NPC Hammer List");
                Reload();
            });
        }
    }

    public static void SaveNPC(Market_NPC.NPCcomponent npc)
    {
        string folder = Market_Paths.NPC_Saved;
        string random = $"{npc._currentNpcType} random{Random.Range(0, 100000)}";
        string file = Path.Combine(folder, $"{random}.cfg");
        NPC_Main mainData = new();
        NPC_Fashion fashionData = new();

        mainData.Type = npc._currentNpcType;
        mainData.NameOverride = npc.znv.m_zdo.GET_NPC_Name();
        mainData.Profile = npc.znv.m_zdo.GET_NPC_Profile();
        mainData.Prefab = npc.znv.m_zdo.GET_NPC_Model();
        mainData.Patrol = npc.znv.m_zdo.GET_NPC_PatrolData();
        mainData.Dialogue = npc.znv.m_zdo.GET_NPC_Dialogue();

        fashionData.LeftItem = npc.znv.m_zdo.GET_NPC_LeftItem();
        fashionData.RightItem = npc.znv.m_zdo.GET_NPC_RightItem();
        fashionData.HelmetItem = npc.znv.m_zdo.GET_NPC_HelmetItem();
        fashionData.ChestItem = npc.znv.m_zdo.GET_NPC_ChestItem();
        fashionData.LegsItem = npc.znv.m_zdo.GET_NPC_LegsItem();
        fashionData.CapeItem = npc.znv.m_zdo.GET_NPC_CapeItem();
        fashionData.HairItem = npc.znv.m_zdo.GET_NPC_HairItem();
        fashionData.HairColor = npc.znv.m_zdo.GET_NPC_HairColor();
        fashionData.ModelScale = npc.znv.m_zdo.GET_NPC_NPCScale().ToString();
        fashionData.LeftItemHidden = npc.znv.m_zdo.GET_NPC_LeftItemBack();
        fashionData.RightItemHidden = npc.znv.m_zdo.GET_NPC_RightItemBack();
        fashionData.InteractAnimation = npc.znv.m_zdo.GET_NPC_InteractAnimation();
        fashionData.GreetAnimation = npc.znv.m_zdo.GET_NPC_GreetingAnimation();
        fashionData.ByeAnimation = npc.znv.m_zdo.GET_NPC_ByeAnimation();
        fashionData.GreetText = npc.znv.m_zdo.GET_NPC_GreetingText();
        fashionData.ByeText = npc.znv.m_zdo.GET_NPC_ByeText();
        fashionData.SkinColor = npc.znv.m_zdo.GET_NPC_SkinColor();
        fashionData.CraftingAnimation = npc.znv.m_zdo.GET_NPC_CraftingAnimation();
        fashionData.BeardItem = npc.znv.m_zdo.GET_NPC_BeardItem();
        fashionData.BeardColor = npc.znv.m_zdo.GET_NPC_BeardColor();
        fashionData.InteractAudioClip = npc.znv.m_zdo.GET_NPC_InteractSound();
        fashionData.TextSize = npc.znv.m_zdo.GET_NPC_TextSize().ToString();
        fashionData.TextHeight = npc.znv.m_zdo.GET_NPC_TextHeight().ToString();
        fashionData.PeriodicAnimation = npc.znv.m_zdo.GET_NPC_PeriodicAnimation();
        fashionData.PeriodicAnimationTime = npc.znv.m_zdo.GET_NPC_PeriodicAnimationTime().ToString();
        fashionData.PeriodicSound = npc.znv.m_zdo.GET_NPC_PeriodicSound();
        fashionData.PeriodicSoundTime = npc.znv.m_zdo.GET_NPC_PeriodicSoundTime().ToString();

        npc.transform.Find("TMP").gameObject.SetActive(false);
        string image = PlayerModelPreview.MakeScreenshot(npc.gameObject, 256);
        npc.transform.Find("TMP").gameObject.SetActive(true);

        string parsed = $"[{random}]\n" +
                        $"{mainData}\n" +
                        $"{fashionData}\n" +
                        $"{image}\n";

        if (!File.Exists(file)) File.Create(file).Dispose();
        File.WriteAllText(file, parsed);
        Utils.print($"Saved NPC to {file}");
        MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, $"Saved NPC to {Path.GetFileName(file)}");
    }
    
    
    [HarmonyPatch(typeof(Player), nameof(Player.PlacePiece))]
    [ClientOnlyPatch]
    private static class HookBuild
    {
        public static void PlacedPiece(GameObject obj)
        {
            if (obj?.GetComponent<Piece>() is not { } piece) return;
            string objName = piece.name;
            if(!objName.Contains("MARKETNPC_HAMMER_BUILDED")) return;
            var KVP = _npcData.TryGetValue(piece.m_name, out var value);
            Vector3 pos = piece.transform.position;
            Quaternion rot = piece.transform.rotation;
            UnityEngine.Object.Destroy(obj);
            if (!KVP) return;
            var newNPC = Object.Instantiate(Market_NPC.NPC, pos, rot);
            Market_NPC.NPCcomponent comp = newNPC.GetComponent<Market_NPC.NPCcomponent>();
            comp.ChangeNpcType(0, (int)value.Key.Type);
            comp.ChangeProfile(0, value.Key.Profile, value.Key.Dialogue);
            comp.OverrideName(0, value.Key.NameOverride);
            comp.OverrideModel(0, value.Key.Prefab);
            comp.GetPatrolData(0, value.Key.Patrol);
            comp.FashionApply(0, value.Value);
        }

        [UsedImplicitly]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo method =
                AccessTools.Method(typeof(HookBuild), nameof(PlacedPiece), new[] { typeof(GameObject) });
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;
                if (instruction.opcode == OpCodes.Stloc_3)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 3);
                    yield return new CodeInstruction(OpCodes.Call, method);
                }
            }
        }
    }
    
    
    
    
}
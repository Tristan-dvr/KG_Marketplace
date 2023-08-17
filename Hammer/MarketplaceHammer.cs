using System.Reflection.Emit;
using Marketplace.ExternalLoads;
using Marketplace.Modules.NPC;
using Marketplace.Modules.Transmogrification;
using Marketplace.Paths;
using Object = UnityEngine.Object;
using static Marketplace.Modules.NPC.NPC_DataTypes;
using Random = UnityEngine.Random;

namespace Marketplace.Hammer;

[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Last, "OnInit", new[] { "SN" },
    new[] { "Reload" })]
public static class MarketplaceHammer
{
    private static Dictionary<string, Wrapper> _npcData = new();
    private static List<Piece> _pieces = new();
    private static GameObject CopyFrom;
    
    public struct Wrapper
    {
        public NPC_Main main;
        public NPC_Fashion fashion;
    }

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
        }
    }

    private static void ProcessSavedNPC(string fileName, string data)
    {
        try
        {
            Wrapper KVP = fastJSON.JSON.ToObject<Wrapper>(data);
            _npcData[fileName] = KVP;
            GameObject copy = Object.Instantiate(CopyFrom, INACTIVE.transform);
            copy.name = "MARKETNPC_HAMMER_BUILDED";
            copy.GetComponent<Piece>().m_name = fileName;
            copy.GetComponent<Piece>().m_description = KVP.main.Description;
            if (!string.IsNullOrEmpty(KVP.main.IMAGE))
            {
                Texture2D loadTex = new(1, 1);
                loadTex.LoadImage(Convert.FromBase64String(KVP.main.IMAGE));
                copy.GetComponent<Piece>().m_icon = Sprite.Create(loadTex, new Rect(0, 0, loadTex.width, loadTex.height), new Vector2(0, 0));
            }
            else
            {
                copy.GetComponent<Piece>().m_icon = AssetStorage.NullSprite;
            }
            _pieces.Add(copy.GetComponent<Piece>());
        }
        catch
        {
            
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
        MessageHud.instance?.ShowMessage(MessageHud.MessageType.Center, $"Reloading NPC Hammer List");
        _npcData.Clear();
        _pieces.Clear();
        foreach (Transform transform in INACTIVE.transform)
            Object.Destroy(transform.gameObject);
        string folder = Market_Paths.NPC_Saved;
        string[] files = Directory.GetFiles(folder, "*.json", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            string fNameNoExt = Path.GetFileNameWithoutExtension(file);
            string fContent = File.ReadAllText(file);
            ProcessSavedNPC(fNameNoExt, fContent);
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

    private static JSONParameters NPC_Save_Params = new()
    {
        UseExtensions = false,
        SerializeNullValues = true,
        DateTimeMilliseconds = false,
        UseUTCDateTime = true,
        UseOptimizedDatasetSchema = true,
        UseValuesOfEnums = false,
    };

    public static void SaveNPC(Market_NPC.NPCcomponent npc)
    {
        string folder = Market_Paths.NPC_Saved;
        string random = $"{npc._currentNpcType} random{Random.Range(0, 100000)}";
        string file = Path.Combine(folder, $"{random}.json");
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
        
        if (!File.Exists(file)) File.Create(file).Dispose();
        
        File.WriteAllText(file, fastJSON.JSON.ToNiceJSON(new Wrapper(){fashion = fashionData, main = mainData}, NPC_Save_Params));
        Utils.print($"Saved NPC to {file}");
        MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, $"Saved NPC to {Path.GetFileName(file)}");
        Reload();
    }


    [HarmonyPatch(typeof(Player), nameof(Player.PlacePiece))]
    [ClientOnlyPatch]
    private static class HookBuild
    {
        public static void PlacedPiece(GameObject obj)
        {
            if (obj?.GetComponent<Piece>() is not { } piece) return;
            string objName = piece.name;
            if (!objName.Contains("MARKETNPC_HAMMER_BUILDED")) return;
            var KVP = _npcData.TryGetValue(piece.m_name, out var value);
            Vector3 pos = piece.transform.position;
            Quaternion rot = piece.transform.rotation;
            UnityEngine.Object.Destroy(obj);
            if (!KVP) return;
            var newNPC = Object.Instantiate(Market_NPC.NPC, pos, rot);
            Market_NPC.NPCcomponent comp = newNPC.GetComponent<Market_NPC.NPCcomponent>();
            comp.ChangeNpcType(0, (int)value.main.Type);
            comp.ChangeProfile(0, value.main.Profile, value.main.Dialogue);
            comp.OverrideName(0, value.main.NameOverride);
            comp.OverrideModel(0, value.main.Prefab);
            comp.GetPatrolData(0, value.main.Patrol);
            comp.FashionApply(0, value.fashion);
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
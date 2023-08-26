﻿using System.Reflection.Emit;
using API;
using Marketplace.ExternalLoads;
using Marketplace.Modules.NPC;
using Marketplace.Modules.Transmogrification;
using Marketplace.Paths;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using Object = UnityEngine.Object;
using static Marketplace.Modules.NPC.NPC_DataTypes;
using Random = UnityEngine.Random;

namespace Marketplace.Hammer;

[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Last, "OnInit")]
public static class MarketplaceHammer
{
    private static Dictionary<string, Wrapper> _npcData = new();
    private static List<Piece> _pieces = new();
    private static GameObject CopyFrom;
    public static Piece.PieceCategory _category = Piece.PieceCategory.Misc;

    public struct Wrapper
    {
        public bool isPinned;
        public NPC_Main main;
        public NPC_Fashion fashion;
    }

    private static GameObject INACTIVE = new GameObject("INACTIVE_SAVEDNPCS")
    {
        hideFlags = HideFlags.HideAndDontSave
    };

    [HarmonyPatch(typeof(PieceTable), nameof(PieceTable.UpdateAvailable))]
    [ClientOnlyPatch]
    private static class PieceTable_UpdateAvailable_Patch
    {
        [UsedImplicitly]
        private static void Postfix(PieceTable __instance)
        {
            if (__instance.name != "_HammerPieceTable") return;
            List<Piece> avaliablePieces = __instance.m_availablePieces[(int)_category];
            Hud_Awake_Patch.UI.SetActive(__instance.m_selectedCategory == _category);
            if (Utils.IsDebug)
            {
                avaliablePieces.RemoveAt(0);
                avaliablePieces.Add(Market_NPC.NPC.GetComponent<Piece>());
                avaliablePieces.Add(Market_NPC.PinnedNPC.GetComponent<Piece>());

                int skip = (Hud_Awake_Patch.CurrentPage - 1) * Hud_Awake_Patch.MaxPerPage;
                foreach (Piece data in _pieces.Skip(skip).Take(Hud_Awake_Patch.MaxPerPage))
                    avaliablePieces.Add(data);
            }
        }
    }

    private static void ProcessSavedNPC(string fileName, string data)
    {
        if (_npcData.ContainsKey(fileName)) return;
        try
        {
            Wrapper KVP = new DeserializerBuilder().Build().Deserialize<Wrapper>(data);
            _npcData[fileName] = KVP;
            GameObject copy = Object.Instantiate(CopyFrom, INACTIVE.transform);
            copy.name = "MARKETNPC_HAMMER_BUILDED";
            copy.GetComponent<Piece>().m_name = fileName;
            copy.GetComponent<Piece>().m_description = KVP.main.Description();
            if (!string.IsNullOrEmpty(KVP.main.IMAGE))
            {
                Texture2D loadTex = new(1, 1);
                loadTex.LoadImage(Convert.FromBase64String(KVP.main.IMAGE));
                copy.GetComponent<Piece>().m_icon = Sprite.Create(loadTex,
                    new Rect(0, 0, loadTex.width, loadTex.height), new Vector2(0, 0));
            }
            else
            {
                copy.GetComponent<Piece>().m_icon = AssetStorage.NullSprite;
            }

            _pieces.Add(copy.GetComponent<Piece>());
        }
        catch (Exception ex)
        {
            Utils.print($"Error file parsing saved npc: {fileName}: \n{ex}");
        }
    }

    [UsedImplicitly]
    private static void OnInit()
    {
        _category = PieceManager.PiecePrefabManager.GetCategory("<color=cyan><b>Marketplace</b></color>");
        INACTIVE.SetActive(false);
        CopyFrom = AssetStorage.asset.LoadAsset<GameObject>("MarketplaceHammerCopyFrom");
        Reload();
    }

    private static void Reload()
    {
        MessageHud.instance?.ShowMessage(MessageHud.MessageType.Center, $"Reloading NPC Hammer List");
        if (Chat.instance)
        {
            Chat.instance.m_hideTimer = 0f;
            Chat.instance.AddString($"<color=green>Reloading NPC Hammer List</color>");
        }

        _npcData.Clear();
        _pieces.Clear();
        foreach (Transform transform in INACTIVE.transform)
            Object.Destroy(transform.gameObject);
        string folder = Market_Paths.NPC_Saved;
        string[] yml = Directory.GetFiles(folder, "*.yml", SearchOption.AllDirectories);
        string[] yaml = Directory.GetFiles(folder, "*.yaml", SearchOption.AllDirectories);
        foreach (string file in yml.Concat(yaml))
        {
            string fNameNoExt = Path.GetFileNameWithoutExtension(file);
            string fContent = File.ReadAllText(file);
            ProcessSavedNPC(fNameNoExt, fContent);
        }

        Hud_Awake_Patch.SetPage(1);
    }

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
    [ClientOnlyPatch]
    private static class Terminal_InitTerminal_Patch
    {
        [UsedImplicitly]
        private static void Postfix(Terminal __instance)
        {
            new Terminal.ConsoleCommand("reloadnpcs", "Reloads all saved NPCs", (_) => { Reload(); });
        }
    }

    public static void SaveNPC(Market_NPC.NPCcomponent npc)
    {
        string folder = Market_Paths.NPC_Saved;
        NPC_Main mainData = new();
        NPC_Fashion fashionData = new();

        mainData.Type = npc._currentNpcType;
        mainData.NameOverride = npc.znv.m_zdo.GET_NPC_Name();
        mainData.Profile = npc.znv.m_zdo.GET_NPC_Profile();
        mainData.Prefab = npc.znv.m_zdo.GET_NPC_Model();
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
        mainData.IMAGE = PlayerModelPreview.MakeScreenshot(npc.gameObject, 256);
        npc.transform.Find("TMP").gameObject.SetActive(true);

        string fName = $"{npc._currentNpcType} {mainData.NameOverride.NoRichText()} {mainData.Profile.NoRichText()}";
        string fPath = Path.Combine(folder, $"{fName}.yml");

        if (!File.Exists(fPath)) File.Create(fPath).Dispose();
        Wrapper wrapper = new Wrapper()
        {
            main = mainData,
            fashion = fashionData,
            isPinned = npc.gameObject.name.Contains(Market_NPC.PinnedNPC.name)
        };

        string yaml = new SerializerBuilder().Build().Serialize(wrapper);
        File.WriteAllText(fPath, yaml);
        Chat.instance.m_hideTimer = 0f;
        Chat.instance.AddString(
            $"<color=green>NPC Saved to {Path.GetFileName(fPath)}. Write <color=yellow>/reloadnpcs</color> to reload NPC list in hammer</color>");
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
            bool hasValue = _npcData.TryGetValue(piece.m_name, out Wrapper value);
            Vector3 pos = piece.transform.position;
            Quaternion rot = piece.transform.rotation;
            UnityEngine.Object.Destroy(obj);
            if (!hasValue) return;
            GameObject newNPC = Object.Instantiate(value.isPinned ? Market_NPC.PinnedNPC : Market_NPC.NPC, pos, rot);
            Market_NPC.NPCcomponent comp = newNPC.GetComponent<Market_NPC.NPCcomponent>();
            comp.ChangeNpcType(0, (int)value.main.Type);
            comp.ChangeProfile(0, value.main.Profile, value.main.Dialogue);
            comp.OverrideName(0, value.main.NameOverride);
            if (!string.IsNullOrWhiteSpace(value.main.Prefab))
                comp.OverrideModel(0, value.main.Prefab.RandomSplitSpace());
            value.fashion.ApplyRandom();
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

    [HarmonyPatch(typeof(Piece), nameof(Piece.CanBeRemoved))]
    private static class Piece_CanBeRemoved_Patch
    {
        [UsedImplicitly]
        private static void Postfix(Piece __instance, ref bool __result)
        {
            if (__instance.GetComponent<Market_NPC.NPCcomponent>() && !Utils.IsDebug)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(Hud), nameof(Hud.Awake))]
    private static class Hud_Awake_Patch
    {
        public static GameObject UI;
        private static Text _text;

        public static int CurrentPage = 1;
        public const int MaxPerPage = 88;
        private static int MaxPages => Mathf.CeilToInt(_pieces.Count / (float)MaxPerPage);


        [UsedImplicitly]
        private static void Postfix(Hud __instance)
        {
            CurrentPage = 1;
            UI = UnityEngine.Object.Instantiate(AssetStorage.asset.LoadAsset<GameObject>("MarketHammer_Pages"),
                __instance.m_buildHud.transform.Find("bar/SelectionWindow"));
            UI.transform.SetAsLastSibling();
            UI.SetActive(false);
            _text = UI.transform.Find("Page/Text").GetComponent<Text>();
            UI.transform.Find("PageLeft").GetComponent<Button>().onClick.AddListener(OnPageLeft);
            UI.transform.Find("PageRight").GetComponent<Button>().onClick.AddListener(OnPageRight);
            _text.text = $"{CurrentPage} / {MaxPages}";
        }

        public static void SetPage(int page)
        {
            CurrentPage = Mathf.Clamp(page, 1, MaxPages);
            Player.m_localPlayer?.UpdateAvailablePiecesList();
            if (_text)
                _text.text = $"{CurrentPage} / {MaxPages}";
        }

        private static void OnPageLeft()
        {
            AssetStorage.AUsrc.Play();
            SetPage(CurrentPage - 1);
        }

        private static void OnPageRight()
        {
            AssetStorage.AUsrc.Play();
            SetPage(CurrentPage + 1);
        }
    }
}
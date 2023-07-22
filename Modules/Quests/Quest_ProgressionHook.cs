using System.Reflection.Emit;
using Groups;

namespace Marketplace.Modules.Quests;

public static class Quest_ProgressionHook
{
    [HarmonyPatch(typeof(Pickable), nameof(Pickable.RPC_Pick))]
    [ClientOnlyPatch]
    private static class Pickable_Interact_Patch
    {
        private static void Prefix(Pickable __instance, long sender)
        {
            if (__instance.m_picked) return;
            string prefab = global::Utils.GetPrefabName(__instance.gameObject);
            ZRoutedRpc.instance.InvokeRoutedRPC(sender, "KGmarket QuestPickup", prefab);
        }
    }

    public static void HookCrafting()
    {
        if (InventoryGui.instance.m_craftRecipe == null || InventoryGui.instance.m_selectedRecipe.Key == null) return;
        KeyValuePair<Recipe, ItemDrop.ItemData> recipe = InventoryGui.instance.m_selectedRecipe;
        string CraftedItemName = recipe.Key.m_item?.gameObject.name;
        if (string.IsNullOrEmpty(CraftedItemName)) return;
        GameEvents.OnItemCrafted?.Invoke(CraftedItemName, recipe.Value?.m_quality ?? 1);
        Quests_DataTypes.Quest.TryAddRewardCraft(CraftedItemName, recipe.Value?.m_quality ?? 1);
    }


    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.DoCrafting))]
    [ClientOnlyPatch]
    private static class HOOKCRAFTING
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo craftsField = AccessTools.DeclaredField(typeof(PlayerProfile.PlayerStats), "m_crafts");
            MethodInfo method = AccessTools.DeclaredMethod(typeof(Quest_ProgressionHook), nameof(HookCrafting));
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;
                if (instruction.opcode == OpCodes.Stfld && instruction.OperandIs(craftsField))
                {
                    yield return new CodeInstruction(OpCodes.Call, method);
                }
            }
        }
    }

    private static readonly Dictionary<Character, long> CharacterLastDamageList = new();

    [HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
    [ClientOnlyPatch]
    private static class QuestEnemyKill
    {
        private static void Prefix(Character __instance, long sender, HitData hit)
        {
            if (__instance.GetHealth() <= 0) return;
            Character attacker = hit.GetAttacker();
            if (attacker)
            {
                if (attacker.IsPlayer())
                {
                    CharacterLastDamageList[__instance] = sender;
                }
                else
                {
                    if (!attacker.IsTamed())
                    {
                        CharacterLastDamageList[__instance] = 100;
                    }
                }
            }
        }

        private static void Postfix(Character __instance)
        {
            if (__instance.GetHealth() <= 0f && CharacterLastDamageList.ContainsKey(__instance))
            {
                ZRoutedRpc.instance.InvokeRoutedRPC(CharacterLastDamageList[__instance], "KGmarket QuestKill",
                    global::Utils.GetPrefabName(__instance.gameObject), __instance.GetLevel(),
                    __instance.transform.position,
                    true);
                CharacterLastDamageList.Remove(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(Character), nameof(Character.ApplyDamage))]
    [ClientOnlyPatch]
    private static class Character_ApplyDamage_Patch
    {
        private static void Postfix(Character __instance)
        {
            if (__instance.GetHealth() <= 0f && CharacterLastDamageList.ContainsKey(__instance))
            {
                ZRoutedRpc.instance.InvokeRoutedRPC(CharacterLastDamageList[__instance], "KGmarket QuestKill",
                    global::Utils.GetPrefabName(__instance.gameObject), __instance.GetLevel(),
                    __instance.transform.position,
                    true);
                CharacterLastDamageList.Remove(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(Character), nameof(Character.OnDestroy))]
    [ClientOnlyPatch]
    private static class Character_OnDestroy_Patch
    {
        private static void Postfix(Character __instance)
        {
            if (CharacterLastDamageList.ContainsKey(__instance)) CharacterLastDamageList.Remove(__instance);
        }
    }


    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    [ClientOnlyPatch]
    public static class ZNetScene_Awake_Patch_QuestsInit
    {
        private static void Postfix()
        {
            ZRoutedRpc.instance.Register("KGmarket QuestKill",
                new RoutedMethod<string, int, Vector3, bool>.Method(QuestKillEvent));
            ZRoutedRpc.instance.Register("KGmarket QuestPickup", new Action<long, string>(QuestPickup));
        }

        private static void QuestPickup(long sender, string prefab)
        {
            Quests_DataTypes.Quest.TryAddRewardPickup(prefab);
            GameEvents.OnHarvest?.Invoke(prefab);
        }


        private static void QuestKillEvent(long sender, string prefab, int level, Vector3 pos, bool ownerRPC)
        {
            if (!Player.m_localPlayer) return;
            GameEvents.OnCreatureKilled?.Invoke(prefab, level);
            if (Global_Values.SyncedGlobalOptions.Value._allowKillQuestsInParty && ownerRPC && Groups.API.IsLoaded() &&
                Groups.API.GroupPlayers() is { Count: > 1 } group)
                foreach (PlayerReference member in group)
                    if (member.peerId != ZDOMan.instance.m_sessionID)
                        ZRoutedRpc.instance.InvokeRoutedRPC(member.peerId, "KGmarket QuestKill", prefab, level, pos,
                            false);
            if (!ownerRPC && Vector3.Distance(Player.m_localPlayer.transform.position, pos) >= 100f) return;
            Quests_DataTypes.Quest.TryAddRewardKill(prefab, level);
        }
    }


    [HarmonyPatch(typeof(Player), nameof(Player.OnInventoryChanged))]
    [ClientOnlyPatch]
    private static class Player_OnInventoryChanged_Patch
    {
        private static void Postfix()
        {
            if (!Player.m_localPlayer || Player.m_localPlayer.m_isLoading) return;
            Quests_DataTypes.Quest.InventoryChanged();
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.PlacePiece))]
    [ClientOnlyPatch]
    private static class HookBuildQuest
    {
        public static void PlacedPiece(GameObject obj)
        {
            if (obj?.GetComponent<Piece>() is not { } piece) return;
            string pieceName = global::Utils.GetPrefabName(piece.gameObject);
            GameEvents.OnStructureBuilt?.Invoke(pieceName);
            if (Quests_DataTypes.Quest.TryAddRewardBuild(pieceName) && piece.m_nview &&
                piece.m_nview.m_zdo != null)
            {
                piece.m_nview.m_zdo.Set("MPASNquestBuild", true);
            }
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo method =
                AccessTools.Method(typeof(HookBuildQuest), nameof(PlacedPiece), new[] { typeof(GameObject) });
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

    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.Start))]
    [ClientOnlyPatch]
    private static class Character_Awake_Quest
    {
        private static void Postfix(Humanoid __instance)
        {
            float radius = __instance.m_collider.radius;
            float height = __instance.m_collider.height - 1.7f;
            GameObject go = UnityEngine.Object.Instantiate(AssetStorage.AssetStorage.MarketplaceQuestTargetIcon,
                __instance.transform);
            go.name = "MPASNquest";
            go.transform.localPosition += Vector3.up * height;
            go.transform.localScale += new Vector3(radius, radius, radius);
            go.SetActive(Quests_DataTypes.Quest.IsQuestTarget(__instance));
        }
    }

    [HarmonyPatch(typeof(Pickable), nameof(Pickable.Awake))]
    [ClientOnlyPatch]
    private static class Pickable_Awake_Quest
    {
        private static void Postfix(Pickable __instance)
        {
            __instance.gameObject.AddComponent<Pickable_Hook>();
            GameObject go = UnityEngine.Object.Instantiate(AssetStorage.AssetStorage.MarketplaceQuestTargetIcon,
                __instance.transform);
            go.name = "MPASNquest";
            go.SetActive(Quests_DataTypes.Quest.IsQuestTarget(__instance));
        }
    }

    [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.Awake))]
    [ClientOnlyPatch]
    private static class ItemDrop_Awake_Quest
    {
        private static void Postfix(ItemDrop __instance)
        {
            GameObject go = UnityEngine.Object.Instantiate(AssetStorage.AssetStorage.MarketplaceQuestTargetIcon,
                __instance.transform);
            go.name = "MPASNquest";
            go.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.Start))]
    [ClientOnlyPatch]
    private static class ItemDrop_Awake_Quest2
    {
        private static void Postfix(ItemDrop __instance)
        {
            __instance.transform.Find("MPASNquest")?.gameObject
                .SetActive(Quests_DataTypes.Quest.IsQuestTarget(__instance));
        }
    }


    public class Pickable_Hook : MonoBehaviour
    {
        private static readonly List<Pickable> pickables = new();
        private Pickable pick;

        private void Awake()
        {
            ZNetView znv = GetComponent<ZNetView>();
            if (!znv || !znv.IsValid()) return;
            pick = GetComponent<Pickable>();
            if (!pick) return;
            pickables.Add(pick);
        }

        private void OnDestroy()
        {
            if (pick) pickables.Remove(pick);
        }

        public static List<Pickable> GetPickables()
        {
            return pickables;
        }
    }


    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.AddRecipeToList))]
    [ClientOnlyPatch]
    private static class TranspileRecipeQuestTest
    {
        private static void ChangeRecipeName(Recipe r, ref string str, ItemDrop.ItemData level)
        {
            if (Quests_DataTypes.Quest.IsQuestTarget(r, level?.m_quality + 1 ?? 1))
            {
                if (!str.Contains('★'))
                    str = "<color=#00ff00>★</color> " + str;
            }
        }


        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> TanspileRecipeQuest(IEnumerable<CodeInstruction> code)
        {
            MethodInfo method = AccessTools.DeclaredMethod(typeof(TranspileRecipeQuestTest), nameof(ChangeRecipeName));
            bool isDone = false;
            foreach (CodeInstruction instruction in code)
            {
                yield return instruction;
                if (instruction.opcode == OpCodes.Stloc_2 && !isDone)
                {
                    isDone = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 2);
                    yield return new CodeInstruction(OpCodes.Ldarg_3);
                    yield return new CodeInstruction(OpCodes.Call, method);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Piece), nameof(Piece.DropResources))]
    [ClientOnlyPatch]
    private static class Piece_DropResources_Patch
    {
        private static bool Prefix(Piece __instance)
        {
            return !__instance.m_nview.m_zdo.GetBool("MPASNquestBuild");
        }
    }


    [HarmonyPatch(typeof(Hud), nameof(Hud.Awake))]
    [ClientOnlyPatch]
    private static class Hud_Awake_Patch
    {
        public static int ChildNumber;

        private static void Postfix(Hud __instance)
        {
            GameObject instant = AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("QuestBuild");
            GameObject go = UnityEngine.Object.Instantiate(instant);
            go.name = "QuestBuild";
            // ReSharper disable once Unity.InstantiateWithoutParent
            go.transform.SetParent(__instance.m_pieceIconPrefab.transform);
            go.transform.localPosition = new Vector3(48, -48, 0);
            go.SetActive(false);
            ChildNumber = __instance.m_pieceIconPrefab.transform.childCount - 1;
            go.transform.SetAsLastSibling();
        }
    }

    [HarmonyPatch(typeof(Hud), nameof(Hud.UpdatePieceList))]
    [ClientOnlyPatch]
    private static class Hud_UpdatePieceList_Patch
    {
        private static void SpriteEnabler(Piece p, Hud.PieceIconData data)
        {
            if (!p) return;
            GameObject t = data.m_go.transform.GetChild(Hud_Awake_Patch.ChildNumber).gameObject;
            t.SetActive(Quests_DataTypes.Quest.IsQuestTarget(p));
        }


        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Patch(IEnumerable<CodeInstruction> code)
        {
            MethodInfo method = AccessTools.DeclaredPropertySetter(typeof(Image), nameof(Image.sprite));
            foreach (CodeInstruction instruction in code)
            {
                yield return instruction;
                if (instruction.opcode == OpCodes.Callvirt && ReferenceEquals(instruction.operand, method))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 12);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 11);
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.DeclaredMethod(typeof(Hud_UpdatePieceList_Patch), nameof(SpriteEnabler)));
                }
            }
        }
    }
}
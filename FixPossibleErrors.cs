using System.Reflection.Emit;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Marketplace;

public static class IsExternalInit
{
}

public static class FixPossibleErrors
{
    [HarmonyPatch(typeof(GameCamera), nameof(GameCamera.UpdateMouseCapture))]
    [ClientOnlyPatch]
    private static class GameCamera_UpdateMouseCapture_Patch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> code,
            ILGenerator ilGenerator)
        {
            MethodInfo targetMethod = AccessTools.Method(typeof(Menu), nameof(Menu.IsVisible));
            CodeMatcher matcher = new CodeMatcher(code);
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ret), new CodeMatch(OpCodes.Call, targetMethod),
                new CodeMatch(OpCodes.Brtrue), new CodeMatch(OpCodes.Ldc_I4_0));
            if (matcher.IsInvalid) return code;
            matcher.Advance(1);
            List<Label> labels = matcher.Instruction.labels;
            CodeInstruction newInstruction = new CodeInstruction(OpCodes.Nop)
            {
                labels = labels
            };
            matcher.SetInstructionAndAdvance(newInstruction).SetInstruction(new CodeInstruction(OpCodes.Nop));
            return matcher.Instructions();
        }
    }

    [HarmonyPatch(typeof(CharacterAnimEvent), nameof(CharacterAnimEvent.Awake))]
    [ClientOnlyPatch]
    private static class Fix1
    {
        private static bool Prefix(CharacterAnimEvent __instance)
        {
            if (__instance.GetComponentInParent<Character>() != null) return true;
            UnityEngine.Object.Destroy(__instance);
            return false;
        }
    }

    [HarmonyPatch(typeof(LevelEffects), nameof(LevelEffects.Start))]
    [ClientOnlyPatch]
    private static class Fix2
    {
        private static bool Prefix(LevelEffects __instance)
        {
            if (__instance.GetComponentInParent<Character>() == null)
            {
                UnityEngine.Object.Destroy(__instance);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Minimap), nameof(Minimap.Start))]
    [ClientOnlyPatch]
    private static class SetMaxPins
    {
        public static void Postfix(Minimap __instance)
        {
            if (__instance.m_visibleIconTypes.Length < 200)
            {
                __instance.m_visibleIconTypes = new bool[200];
                for (int i = 0; i < __instance.m_visibleIconTypes.Length; i++)
                    __instance.m_visibleIconTypes[i] = true;
            }
        }
    }
        
}
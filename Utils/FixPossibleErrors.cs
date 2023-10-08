using System.Reflection.Emit;
using Marketplace.Modules.Global_Options;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Marketplace;

public static class IsExternalInit
{
}

public static class FixPossibleErrors
{
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
                for (int i = 0; i < __instance.m_visibleIconTypes.Length; ++i)
                    __instance.m_visibleIconTypes[i] = true;
            }
        }
    }

    [HarmonyPatch(typeof(GameCamera), nameof(GameCamera.UpdateCamera))]
    [ClientOnlyPatch]
    private static class GameCamera_UpdateCamera_Patch
    {
        private static void Nullify(ref float f)
        {
            if (TextInput.IsVisible()) f = 0;
        }

        [UsedImplicitly]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> code)
        {
            CodeMatcher matcher = new CodeMatcher(code);
            MethodInfo target = AccessTools.Method(typeof(ZInput), nameof(ZInput.GetAxis), new[] { typeof(string) });
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "Mouse ScrollWheel"),
                new CodeMatch(OpCodes.Call, target), new CodeMatch(OpCodes.Stloc_2));
            if (matcher.IsInvalid)
                return code;
            matcher.Advance(3);
            matcher.Insert(new CodeInstruction(OpCodes.Ldloca_S, 2),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(GameCamera_UpdateCamera_Patch), nameof(Nullify))));
            return matcher.Instructions();
        }
    }
}
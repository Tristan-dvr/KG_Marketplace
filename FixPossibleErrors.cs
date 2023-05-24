namespace System.Runtime.CompilerServices
{
    public static class IsExternalInit{}
}

namespace Marketplace
{

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
                    for (int i = 0; i < __instance.m_visibleIconTypes.Length; i++)
                        __instance.m_visibleIconTypes[i] = true;
                }
            }
        }
    }
}
using System.Reflection.Emit;

namespace Marketplace;

public static class GameEvents
{
    public static Action OnPlayerFirstSpawn;

    [HarmonyPatch(typeof(Player),nameof(Player.SetLocalPlayer))]
    private static class Player_SetLocalPlayer_Patch
    {
        public static bool firstTime = true;
        private static void Postfix()
        {
            if (firstTime)
            {
                firstTime = false;
                OnPlayerFirstSpawn?.Invoke();
            }
        }
    }

    [HarmonyPatch(typeof(FejdStartup),nameof(FejdStartup.Awake))]
    private static class FejdStartup_Awake_Patch
    {
        private static void Postfix() => Player_SetLocalPlayer_Patch.firstTime = true;
    }
    
}
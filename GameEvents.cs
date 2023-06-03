using System.Reflection.Emit;

namespace Marketplace;

public static class GameEvents
{
    public static Action OnPlayerFirstSpawn;
    public static Action OnPlayerSpawn;
    public static Action<string, int, Vector3> OnCharacterKill;
    public static Action OnPlayerDeath;
    public static Action<GameObject> OnInteract;
    public static Action<string> OnChatMessage;
    
    [HarmonyPatch(typeof(Player),nameof(Player.OnDeath))]
    private static class Player_OnDeath_Patch
    {
        private static void Prefix() => OnPlayerDeath?.Invoke();
    }

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
            OnPlayerSpawn?.Invoke();
        }
    }

    [HarmonyPatch(typeof(FejdStartup),nameof(FejdStartup.Awake))]
    private static class FejdStartup_Awake_Patch
    {
        private static void Postfix() => Player_SetLocalPlayer_Patch.firstTime = true;
    }

    [HarmonyPatch(typeof(Player),nameof(Player.Interact))]
    private static class Player_Interact_Patch
    {
        private static void E_OnInteract(Interactable obj) => OnInteract?.Invoke(((MonoBehaviour)obj).gameObject);

        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Code(IEnumerable<CodeInstruction> code)
        {
            var targetMethod = AccessTools.Field(typeof(Player), nameof(Player.m_lastHoverInteractTime));
            foreach (var codeInstruction in code)
            {
                yield return codeInstruction;
                if (codeInstruction.opcode == OpCodes.Stfld && codeInstruction.operand == targetMethod)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Player_Interact_Patch), nameof(E_OnInteract)));
                }
            }
        }
    }

    [HarmonyPatch(typeof(Chat),nameof(Chat.InputText))]
    private static class Chat_InputText_Patch
    {
        private static void Postfix(Chat __instance) => OnChatMessage?.Invoke(__instance.m_input.text);
    }
}
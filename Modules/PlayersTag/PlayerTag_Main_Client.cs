namespace Marketplace.Modules.PlayersTag;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Last, "OnInit")]
public static class PlayerTag_Main_Client
{
    private static void OnInit()
    {
        PlayersTag_DataTypes.SyncedPlayersTagData.ValueChanged += OnTagsUpdate;
    }

    private static void OnTagsUpdate()
    {
        if (!Game.instance || !Player.m_localPlayer) return;
        string name = Game.instance.GetPlayerProfile().m_playerName;
        long id = Game.instance.GetPlayerProfile().m_playerID;
        Player.m_localPlayer.SetPlayerID(id, name);
    }


    [HarmonyPatch(typeof(Player), nameof(Player.SetPlayerID))]
    [ClientOnlyPatch]
    static class Player_SetPlayerID_Patch
    {
        static void Prefix(ref string name)
        {
            if (PlayersTag_DataTypes.SyncedPlayersTagData.Value.TryGetValue(Global_Values._localUserID,
                    out string SpecialTag)) name = $"{SpecialTag} {name}";
        }
    }

    [HarmonyPatch(typeof(UserInfo), nameof(UserInfo.GetLocalUser))]
    [ClientOnlyPatch]
    private static class UserInfo_GetLocalUser_Patch
    {
        private static void Postfix(ref UserInfo __result)
        {
            if (PlayersTag_DataTypes.SyncedPlayersTagData.Value.TryGetValue(Global_Values._localUserID,
                    out string SpecialTag))
            {
                __result.Name = $"{SpecialTag} {__result.Name}";
            }
        }
    }
}
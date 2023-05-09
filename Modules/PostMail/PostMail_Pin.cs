namespace Marketplace.Modules.PostMail;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Both, Market_Autoload.Priority.Last, "OnInit")]
public static class PostMail_Pin
{
    private const string PrefabToSearch = "MarketplacePostMail";
    private static readonly List<ZDO> TempZDOs = new();

    
    // ReSharper disable once UnusedMember.Global
    private static void OnInit()
    {
        Marketplace._thistype.StartCoroutine(SendPinsToClient());
    }
    
    private static IEnumerator SendPinsToClient()
    {
        for (;;)
        {
            if (Game.instance && ZDOMan.instance != null && ZNet.instance && ZNet.instance.IsServer())
            {
                TempZDOs.Clear();
                int index = 0;
                while (!ZDOMan.instance.GetAllZDOsWithPrefabIterative(PrefabToSearch, TempZDOs, ref index))
                {
                    yield return null;
                }
                foreach (ZDO zdo in TempZDOs)
                {
                    ZDOMan.instance.ForceSendZDO(zdo.m_uid);
                }
            }
            yield return new WaitForSeconds(10f);
        }
    }
    

}
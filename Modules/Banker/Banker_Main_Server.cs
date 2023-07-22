using System.Threading.Tasks;
using Marketplace.Paths;
using Marketplace.Modules.Marketplace_NPC;

namespace Marketplace.Modules.Banker;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Normal, "OnInit", new[] { "BankerProfiles.cfg" },
    new[] { "OnBankerProfilesFileChange" })]
public static class Banker_Main_Server
{
    private static readonly Dictionary<string, Dictionary<int, int>> BankerServerSideData = new();
    private static readonly Dictionary<string, Dictionary<int, DateTime>> BankerTimeStamp = new();

    private static void OnInit()
    {
        Market_Paths.BankerDataJSONFile.DecryptOldData();
        string bankData = Market_Paths.BankerDataJSONFile.ReadClear();
        if (!string.IsNullOrWhiteSpace(bankData))
            BankerServerSideData.AddRange(JSON.ToObject<Dictionary<string, Dictionary<int, int>>>(bankData));
        if (Global_Values.BankerIncomeTime > 0)
        {
            Marketplace._thistype.StartCoroutine(BankerIncome());
        }

        ReadServerBankerProfiles();
    }

    private static void OnBankerProfilesFileChange()
    {
        ReadServerBankerProfiles();
        Utils.print("Banker Changed. Sending new info to all clients");
    }

    private static void ReadServerBankerProfiles()
    {
        IReadOnlyList<string> profiles = File.ReadAllLines(Market_Paths.BankerFile);
        Banker_DataTypes.SyncedBankerProfiles.Value.Clear();
        string splitProfile = "default";
        for (int i = 0; i < profiles.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(profiles[i]) || profiles[i].StartsWith("#")) continue;
            if (profiles[i].StartsWith("["))
            {
                splitProfile = profiles[i].Replace("[", "").Replace("]", "").Replace(" ", "").ToLower();
            }
            else
            {
                int test = profiles[i].Replace(" ", "").GetStableHashCode();
                if (Banker_DataTypes.SyncedBankerProfiles.Value.TryGetValue(splitProfile, out List<int> value))
                {
                    value.Add(test);
                }
                else
                {
                    Banker_DataTypes.SyncedBankerProfiles.Value[splitProfile] = new List<int> { test };
                }
            }
        }

        Banker_DataTypes.SyncedBankerProfiles.Update();
    }

    private static IEnumerator BankerIncome()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(Global_Values.BankerIncomeTime * 3600);
            if (!ZNet.instance || !ZNet.instance.IsServer()) continue;
            Utils.print("Adding Banker Income");
            Task task = Task.Run(() =>
            {
                HashSet<int> interestItems = new(Global_Values.BankerInterestItems.Split(',').Select(i => i.GetStableHashCode()));
                foreach (string id in BankerServerSideData.Keys)
                {
                    float multiplier = Global_Values.SyncedGlobalOptions.Value._vipPlayerList.Contains(id)
                        ? Global_Values.BankerVIPIncomeMultiplier
                        : Global_Values.BankerIncomeMultiplier;
                    if (multiplier > 0)
                    {
                        foreach (int item in new List<int>(BankerServerSideData[id].Keys))
                        {
                            if (Global_Values.BankerInterestItems != "All" && !interestItems.Contains(item)) continue;
                            if (!BankerTimeStamp.ContainsKey(id) || !BankerTimeStamp[id].ContainsKey(item) ||
                                (DateTime.Now - BankerTimeStamp[id][item]).TotalHours >=
                                Global_Values.BankerIncomeTime)
                            {
                                int val = BankerServerSideData[id][item];
                                double toAdd = Math.Ceiling(val * multiplier);
                                if (val + toAdd > int.MaxValue)
                                    val = int.MaxValue;
                                else
                                    val = (int)(toAdd + val);
                                BankerServerSideData[id][item] = val;
                            }
                        }
                    }
                }
            });
            yield return new WaitUntil(() => task.IsCompleted);
            SaveBankerData();
            foreach (string userID in BankerServerSideData.Keys)
            {
                ZNetPeer peer = ZNet.instance.GetPeerByHostName(userID);
                if (peer != null) SendBankerDataToClient(peer);
            }
        }
    }

    private static void SendBankerDataToClient(ZNetPeer peer)
    {
        string userID = peer.m_socket.GetHostName();

        if (userID == "0") return;
        if (BankerServerSideData.TryGetValue(userID, out Dictionary<int, int> value))
        {
            string data = JSON.ToJSON(value);
            ZPackage pkg = new();
            pkg.Write(data);
            pkg.Compress();
            ZRoutedRpc.instance.InvokeRoutedRPC(peer.m_uid, "KGmarket GetBankerClientData", pkg);
        }
    }

    private static void SaveBankerData()
    {
        Market_Paths.BankerDataJSONFile.WriteClear(JSON.ToNiceJSON(BankerServerSideData));
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_CharacterID))]
    [ServerOnlyPatch]
    private static class ZnetSyncBankerProfiles
    {
        private static void Postfix(ZRpc rpc)
        {
            if (!ZNet.instance.IsServer()) return;
            if (!BankerTimeStamp.ContainsKey(rpc.m_socket.GetHostName()))
                BankerTimeStamp[rpc.m_socket.GetHostName()] = new Dictionary<int, DateTime>();
            ZNetPeer peer = ZNet.instance.GetPeer(rpc);
            if (peer == null) return;
            SendBankerDataToClient(peer);
        }
    }

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    [ServerOnlyPatch]
    private static class ZrouteMethodsServerBanker
    {
        private static void Postfix()
        {
            if (!ZNet.instance.IsServer()) return;
            ZRoutedRpc.instance.Register("KGmarket BankerDeposit",
                new Action<long, string, int>(MethodBankerDeposit));
            ZRoutedRpc.instance.Register("KGmarket BankerWithdraw",
                new Action<long, string, int>(MethodBankerWithdraw));
        }
    }

    public static void MethodBankerDeposit(long sender, string item, int value)
    {
        ZNetPeer peer = ZNet.instance.GetPeer(sender);
        if (peer is null) return;
        int hash = item.GetStableHashCode();
        string userID = peer.m_socket.GetHostName();
        if (!BankerServerSideData.ContainsKey(userID)) BankerServerSideData[userID] = new Dictionary<int, int>();
        if (!BankerServerSideData[userID].ContainsKey(hash)) BankerServerSideData[userID][hash] = 0;
        BankerTimeStamp[userID][hash] = DateTime.Now;
        BankerServerSideData[userID][hash] += value;
        SendBankerDataToClient(peer);
        SaveBankerData();
        Market_Logger.Log(Market_Logger.LogType.Banker,
            $"Player User ID: {userID} Deposit an item {item} with quantity: {value}. Current quantity: {BankerServerSideData[userID][hash]}");
    }


    private static void MethodBankerWithdraw(long sender, string item, int value)
    {
        ZNetPeer peer = ZNet.instance.GetPeer(sender);
        if (peer is null) return;
        string userID = peer.m_socket.GetHostName();
        if (!BankerServerSideData.ContainsKey(userID)) return;
        int hash = item.GetStableHashCode();
        if (!BankerServerSideData[userID].ContainsKey(hash) || BankerServerSideData[userID][hash] <= 0) return;
        int fixedValue = Mathf.Min(BankerServerSideData[userID][hash], value);
        BankerTimeStamp[userID][hash] = DateTime.Now;
        BankerServerSideData[userID][hash] -= fixedValue;
        Marketplace_DataTypes.ServerMarketSendData mockData = new Marketplace_DataTypes.ServerMarketSendData
            { Count = fixedValue, ItemPrefab = item, Quality = 1 };
        string json = JSON.ToJSON(mockData);
        ZRoutedRpc.instance.InvokeRoutedRPC(sender, "KGmarket BuyItemAnswer", json);
        SendBankerDataToClient(peer);
        SaveBankerData();
        Market_Logger.Log(Market_Logger.LogType.Banker,
            $"Player User ID: {userID} Withdraw an item {item} with quantity: {value}");
    }
}
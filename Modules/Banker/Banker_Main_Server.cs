using System.Threading.Tasks;
using Marketplace.Modules.Global_Options;
using Marketplace.Modules.MainMarketplace;
using Marketplace.Paths;

namespace Marketplace.Modules.Banker;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Normal, "OnInit", new[] { "BA" },
    new[] { "OnBankerProfilesFileChange" })]
public static class Banker_Main_Server
{
    private static readonly Dictionary<string, Dictionary<int, int>> BankerServerSideData = new();
    private static readonly Dictionary<string, Dictionary<int, DateTime>> BankerTimeStamp = new();

    [UsedImplicitly]
    private static void OnInit()
    {
        if (!File.Exists(Market_Paths.BankerDataJSONFile)) File.Create(Market_Paths.BankerDataJSONFile).Dispose();
        string bankData = Market_Paths.BankerDataJSONFile.ReadFile();
        if (!string.IsNullOrWhiteSpace(bankData))
            BankerServerSideData.AddRange(JSON.ToObject<Dictionary<string, Dictionary<int, int>>>(bankData));
        if (Global_Configs.BankerIncomeTime > 0)
        {
            Marketplace._thistype.StartCoroutine(BankerIncome());
        }

        ReadServerBankerProfiles();
    }

    [UsedImplicitly]
    private static void OnBankerProfilesFileChange()
    {
        ReadServerBankerProfiles();
        Utils.print("Banker Changed. Sending new info to all clients");
    }

    private static void ProcessBankerProfiles(IReadOnlyList<string> profiles)
    {
        string splitProfile = "default";
        foreach (string line in profiles)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
            if (line.StartsWith("["))
            {
                splitProfile = line.Replace("[", "").Replace("]", "").Replace(" ", "").ToLower();
            }
            else
            {
                int test = line.Replace(" ", "").GetStableHashCode();
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
    }

    private static void ReadServerBankerProfiles()
    {
        Banker_DataTypes.SyncedBankerProfiles.Value.Clear();
        string folder = Market_Paths.BankerProfilesFolder;
        string[] files = Directory.GetFiles(folder, "*.cfg", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            IReadOnlyList<string> profiles = File.ReadAllLines(file).ToList();
            ProcessBankerProfiles(profiles);
        }
        Banker_DataTypes.SyncedBankerProfiles.Update();
    }

    private static IEnumerator BankerIncome()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(Global_Configs.BankerIncomeTime * 3600);
            if (!ZNet.instance || !ZNet.instance.IsServer()) continue;
            Utils.print("Adding Banker Income");
            Task task = Task.Run(() =>
            {
                HashSet<int> interestItems =
                    new(Global_Configs.BankerInterestItems.Split(',').Select(i => i.GetStableHashCode()));
                foreach (string id in BankerServerSideData.Keys)
                {
                    float multiplier = Global_Configs.SyncedGlobalOptions.Value._vipPlayerList.Contains(id)
                        ? Global_Configs.BankerVIPIncomeMultiplier
                        : Global_Configs.BankerIncomeMultiplier;
                    if (multiplier > 0)
                    {
                        foreach (int item in new List<int>(BankerServerSideData[id].Keys))
                        {
                            if (Global_Configs.BankerInterestItems != "All" && !interestItems.Contains(item)) continue;
                            if (!BankerTimeStamp.ContainsKey(id) || !BankerTimeStamp[id].ContainsKey(item) ||
                                (DateTime.Now - BankerTimeStamp[id][item]).TotalHours >=
                                Global_Configs.BankerIncomeTime)
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
            foreach (ZNetPeer peer in BankerServerSideData.Keys.Select(userID => ZNet.instance.GetPeerByHostName(userID)).Where(peer => peer != null))
            {
                SendBankerDataToClient(peer);
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
        Market_Paths.BankerDataJSONFile.WriteFile(JSON.ToNiceJSON(BankerServerSideData));
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_CharacterID))]
    [ServerOnlyPatch]
    private static class ZnetSyncBankerProfiles
    {
        [UsedImplicitly]
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
        [UsedImplicitly]
        private static void Postfix()
        {
            if (!ZNet.instance.IsServer()) return;
            ZRoutedRpc.instance.Register("KGmarket BankerDeposit",
                new Action<long, string, int>(MethodBankerDeposit));
            ZRoutedRpc.instance.Register("KGmarket BankerWithdraw",
                new Action<long, string, int>(MethodBankerWithdraw));
            ZRoutedRpc.instance.Register("KGmarket BankerRemove",
                new Action<long, string, int>(MethodBankerRemove));
        }
    }

    public static void MethodBankerDeposit(long sender, string item, int value)
    {
        ZNetPeer peer = ZNet.instance.GetPeer(sender);
        if (peer == null) return;
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
        if (peer == null) return;
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

    private static void MethodBankerRemove(long sender, string item, int value)
    {
        ZNetPeer peer = ZNet.instance.GetPeer(sender);
        if (peer == null) return;
        string userID = peer.m_socket.GetHostName();
        if (!BankerServerSideData.ContainsKey(userID)) return;
        int hash = item.GetStableHashCode();
        if (!BankerServerSideData[userID].ContainsKey(hash) || BankerServerSideData[userID][hash] <= 0) return;
        int fixedValue = Mathf.Min(BankerServerSideData[userID][hash], value);
        BankerServerSideData[userID][hash] -= fixedValue;
        SendBankerDataToClient(peer);
        SaveBankerData();
        Market_Logger.Log(Market_Logger.LogType.Banker,
            $"Player User ID: {userID} Removed (Paid with) an item {item} with quantity: {value}");
    }
}
using System.Threading.Tasks;
using Marketplace.ExternalLoads;
using Marketplace.Modules.Global_Options;
using Marketplace.Paths;

namespace Marketplace.Modules.MainMarketplace;

public static class Marketplace_Messages
{
    private static float _mult1 = 1;
    private static float _mult2 = 1;
    private static int _currentPage;
    public static bool _showMessageBox;
    private static Rect _mainMenuRect;
    private static GUIStyle guistylebutton2 = null!;
    private static GUIStyle normalField = null!;

    private static void SetGuiStyles()
    {
        guistylebutton2 = new GUIStyle(GUI.skin.button)
        {
            fontSize = (int)(15 * _mult1),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal =
            {
                textColor = Color.red
            },
            hover =
            {
                textColor = Color.red
            }
        };
        normalField = new GUIStyle(GUI.skin.textField);
    }
    
    public static void OnGUI()
    {
        if (_showMessageBox)
        {
            _mult1 = (float)Screen.width / 1920;
            _mult2 = (float)Screen.height / 1080;
            _mainMenuRect = new Rect(10f, 10f,
                1920 * _mult1 - 20, 1080 * _mult2 - 20);
            SetGuiStyles();
            GUI.backgroundColor = Color.black;
            GUI.Window(12286777, _mainMenuRect,
                MessageBoxGui, "Message Box");
            GUI.Window(435323215, _mainMenuRect, Test,
                "");
            GUI.Window(431312538, _mainMenuRect, Test,
                "");
            GUI.DrawTextureWithTexCoords(_mainMenuRect,AssetStorage.WoodTex,
                new Rect(0, 0,
                    _mainMenuRect.width /
                    _mainMenuRect.width,
                    _mainMenuRect.height /
                    _mainMenuRect.height));
        }
    }


    [HarmonyPatch(typeof(TextInput), nameof(TextInput.IsVisible))]
    [ClientOnlyPatch]
    private static class MSGUIFix
    {
        [UsedImplicitly]
        private static void Postfix(ref bool __result)
        {
            if (_showMessageBox) __result = true;
        }
    }

    private static void Test(int window)
    {
    }

    private static void MessageBoxGui(int window)
    {
        if (GUI.Button(new Rect(1920 * _mult1 - 100 * _mult1, 20, 70f * _mult1, 70f * _mult2), "X",
                guistylebutton2))
        {
            AssetStorage.AUsrc.Play();
            Marketplace_UI.Show();

            _showMessageBox = false;
        }

        guistylebutton2.normal.textColor = Color.green;
        guistylebutton2.fontSize = (int)(20 * _mult1);
        if (GUI.Button(new Rect(20, 1080 * _mult2 - 80, 200f * _mult1, 50f * _mult2), "Delete All",
                guistylebutton2) && Messenger.LocalCount > 0)
        {
            ZRoutedRpc.instance.InvokeRoutedRPC("KGmarket ClearMessages");
            Messenger.ClientPlayerMessages.Clear();
            Messenger.LocalCount = 0;
            AssetStorage.AUsrc.Play();
        }

        GUI.Label(new Rect(690 * _mult1, 1080 * _mult2 - 80, 130f * _mult1, 50f * _mult2),
            $"{_currentPage + 1}/{Messenger.LocalCount / 36 + 1}", guistylebutton2);
        if (GUI.Button(new Rect(820 * _mult1, 1080 * _mult2 - 80, 100f * _mult1, 50f * _mult2), "<",
                guistylebutton2))
        {
            AssetStorage.AUsrc.Play();
            if (_currentPage > 0) _currentPage--;
        }

        if (GUI.Button(new Rect(920 * _mult1, 1080 * _mult2 - 80, 100f * _mult1, 50f * _mult2), ">",
                guistylebutton2))
        {
            AssetStorage.AUsrc.Play();
            if (_currentPage < Messenger.LocalCount / 36) _currentPage++;
        }

        guistylebutton2.fontSize = (int)(20 * _mult1);
        GUILayout.BeginArea(new Rect(40 * _mult1, 40 * _mult2, 1920 * _mult1 - 160, 1080 * _mult2));
        normalField.richText = true;
        normalField.fontSize = (int)(15 * _mult1);
        for (int i = 35 * _currentPage;
             i < Mathf.Clamp(Messenger.ClientPlayerMessages.Count, 0, 35 * (1 + _currentPage));
             i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<color=red>[{i + 1}]</color>" + Messenger.ClientPlayerMessages[i], normalField);
            GUILayout.EndHorizontal();
        }

        GUILayout.EndArea();
    }

    public static class Messenger
    {
        public static List<string> ClientPlayerMessages = new();
        public static int LocalCount;

        
        public static Dictionary<string, string> PlayerMessages = new();
        private static bool IsSaving;

        public static void GetMessages(long sender, ZPackage pkg)
        {
            if (sender == ZRoutedRpc.instance.GetServerPeerID())
            {
                pkg.Decompress();
                string data = pkg.ReadString();
                ClientPlayerMessages = data.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                LocalCount = ClientPlayerMessages.Count;
            }
        }


        public static void ClearPlayerMessages(long sender)
        {
            ZNetPeer peer = ZNet.instance.GetPeer(sender);
            if (peer == null) return;
            string userID = peer.m_socket.GetHostName();
            PlayerMessages[userID] = "";
            SaveMsgAsync();
        }


        public static void BuyCancelMessage(string buyer, string seller, Marketplace_DataTypes.ServerMarketSendData data, int howMany)
        {
            if (buyer != seller)
            {
                int applyTaxes = Global_Configs.SyncedGlobalOptions.Value._vipPlayerList.Contains(seller) ? Global_Configs.SyncedGlobalOptions.Value._vipmarketTaxes : Global_Configs.SyncedGlobalOptions.Value._marketTaxes;
                long value = howMany * data.Price;
                value = (long)(value - value * (applyTaxes / 100f)); 
                
                if (PlayerMessages.ContainsKey(seller))
                    PlayerMessages[seller] =
                        $"[{DateTime.Now}] You sold <color=green>{Localization.instance.Localize(data.ItemName)}</color> <color=#FF00FF>x{howMany}</color> | <color=yellow>Income : {value} {Localization.instance.Localize(Global_Configs.CurrencyName)}</color>. Taxes: {applyTaxes}%\n" +
                        PlayerMessages[seller];
                else
                    PlayerMessages[seller] =
                        $"[{DateTime.Now}] You sold <color=green>{Localization.instance.Localize(data.ItemName)}</color> <color=#FF00FF>x{howMany}</color> | <color=yellow>Income : {value} {Localization.instance.Localize(Global_Configs.CurrencyName)}</color>. Taxes: {applyTaxes}%";

                if (PlayerMessages.ContainsKey(buyer))
                    PlayerMessages[buyer] =
                        $"[{DateTime.Now}] You bought <color=green>{Localization.instance.Localize(data.ItemName)}</color> <color=#FF00FF>x{howMany}</color> | <color=yellow>price: {data.Price * howMany} {Localization.instance.Localize(Global_Configs.CurrencyName)}</color>\n" +
                        PlayerMessages[buyer];
                else
                    PlayerMessages[buyer] =
                        $"[{DateTime.Now}] You bought <color=green>{Localization.instance.Localize(data.ItemName)}</color> <color=#FF00FF>x{howMany}</color> | <color=yellow>price: {data.Price * howMany} {Localization.instance.Localize(Global_Configs.CurrencyName)}</color>";
            }
            else
            {
                if (PlayerMessages.ContainsKey(buyer))
                    PlayerMessages[buyer] =
                        $"[{DateTime.Now}] You cancelled <color=green>{Localization.instance.Localize(data.ItemName)}</color> <color=#FF00FF>x{howMany}</color>| <color=yellow>price: {data.Price * howMany} {Localization.instance.Localize(Global_Configs.CurrencyName)}</color>\n" +
                        PlayerMessages[buyer];
                else
                    PlayerMessages[buyer] =
                        $"[{DateTime.Now}] You cancelled <color=green>{Localization.instance.Localize(data.ItemName)}</color> <color=#FF00FF>x{howMany}</color> | <color=yellow>price: {data.Price * howMany} {Localization.instance.Localize(Global_Configs.CurrencyName)}</color>";
            }
            Marketplace_Main_Server.SendMessagesToClient(buyer);
            Marketplace_Main_Server.SendMessagesToClient(seller);
            SaveMsgAsync();
        }

        public static void PostNewItemMessage(string seller, Marketplace_DataTypes.ServerMarketSendData data)
        {
            if (PlayerMessages.ContainsKey(seller))
                PlayerMessages[seller] =
                    $"[{DateTime.Now}] You placed <color=green>{Localization.instance.Localize(data.ItemName)}</color> <color=#FF00FF>x{data.Count}</color> | <color=yellow>price:{data.Price * data.Count}</color>\n" +
                    PlayerMessages[seller];
            else
                PlayerMessages[seller] =
                    $"[{DateTime.Now}] You placed <color=green>{Localization.instance.Localize(data.ItemName)}</color> <color=#FF00FF>x{data.Count}</color> | <color=yellow>price:{data.Price * data.Count}</color>";

            Marketplace_Main_Server.SendMessagesToClient(seller);
            SaveMsgAsync();
        }


        private static void SaveMsg()
        {
            for (int i = 0; i < PlayerMessages.Count; i++)
            {
                KeyValuePair<string, string> key = PlayerMessages.ElementAt(i);
                if (key.Value.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).Length > 175)
                    PlayerMessages[key.Key] = PlayerMessages[key.Key]
                        .Remove(PlayerMessages[key.Key].LastIndexOf("\n", StringComparison.Ordinal));
            }
        }

        private static async void SaveMsgAsync()
        {
            if (IsSaving) return;
            IsSaving = true;
            await Task.Run(SaveMsg);
            Market_Paths.MarketPlayerMessagesJSON.WriteFile(JSON.ToNiceJSON(PlayerMessages));
            IsSaving = false;
        }
    }
}
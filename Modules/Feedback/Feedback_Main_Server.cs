using System.Net;
using System.Threading.Tasks;

namespace Marketplace.Modules.Feedback;

public static class Feedback_Main_Server
{
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    [ServerOnlyPatch]
    private static class ZrouteMethodsServerFeedback
    {
        private static void Postfix()
        {
            ZRoutedRpc.instance.Register("KGmarket ReceiveFeedback",
                new Action<long, ZPackage>(ReceiveFeedback));
        }

        private static void ReceiveFeedback(long sender, ZPackage pkg)
        {
            ZNetPeer peer = ZNet.instance.GetPeer(sender);
            string PlayerInfo = pkg.ReadString() + " (" + peer.m_socket.GetHostName() + ")";
            Utils.print($"Got feedback from {PlayerInfo}");
            string Subject = pkg.ReadString();
            string Message = pkg.ReadString();
            Task.Run(() =>
            {
                string json =
                    @"{""username"":""FeedbackNPC""," +
                    @"""embeds"":[{""author"":{""name"":""" + "Player: " + PlayerInfo +
                    @"""},""title"":""Subject"",""description"":""" + Subject +
                    @""",""color"":15258703,""fields"":[{""name"":""Message"",""value"":""" + Message +
                    @""",""inline"":true}]}]}";
                SendMSG(Global_Values.WebHookLink, json);
            });
        }
    }
    
    private static void SendMSG(string link, string message)
    {
        if (Uri.TryCreate(link, UriKind.Absolute, out Uri outUri)
            && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps))
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(link);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(message);
            }

            httpWebRequest.GetResponseAsync();
        }
    }
}
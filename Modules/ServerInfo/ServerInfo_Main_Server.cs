using Marketplace.Paths;

namespace Marketplace.Modules.ServerInfo;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Normal, "OnInit",
    new[] { "ServerInfoProfiles.cfg" }, new[] { "OnServerInfoProfileChange" })]
public static class ServerInfo_Main_Server
{
    private const string no_breaking_space = "\u00A0";
    
    private static void OnInit()
    {
        ReadServerInfoProfiles();
    }
    
    private static void OnServerInfoProfileChange()
    {
        ReadServerInfoProfiles();
        Utils.print("Info Profiles Changed. Sending new info to all clients");
    }
    
    private static void ReadServerInfoProfiles()
    {
        List<string> profiles = File.ReadAllLines(Market_Paths.ServerInfoConfig).ToList();
        ServerInfo_DataTypes.ServerInfoData.Value.Clear();
        string splitProfile = "default";
        string data = "";
        bool breakSpaces = false;
        for (int i = 0; i < profiles.Count; i++)
        {
            if (profiles[i].StartsWith("#")) continue;
            if (profiles[i].StartsWith("[")) 
            {
                if (!string.IsNullOrEmpty(data))
                {
                    if (breakSpaces) data = data.Replace(" ", no_breaking_space);
                    ServerInfo_DataTypes.ServerInfoData.Value[splitProfile] =  data.Trim('\n');
                }

                string[] split = profiles[i].Replace("[", "").Replace("]", "").Replace(" ","").ToLower().Split('=');

                splitProfile = split[0];
                if (split.Length == 2)
                {
                    breakSpaces = bool.Parse(split[1]);
                }
                
                data = "";
            }
            else
            { 
                data += "\n" + profiles[i]; 
            }
        } 
        if (!string.IsNullOrEmpty(data))
        {
            if (breakSpaces) data = data.Replace(" ", no_breaking_space);
            ServerInfo_DataTypes.ServerInfoData.Value[splitProfile] =  data.Trim('\n');
        }
        ServerInfo_DataTypes.ServerInfoData.Update();
    }
    
}
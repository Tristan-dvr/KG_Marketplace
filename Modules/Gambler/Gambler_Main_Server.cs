using Marketplace.Paths;

namespace Marketplace.Modules.Gambler;

[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Normal, "OnInit",
    new[] { "GamblerProfiles.cfg" }, new[] { "OnGamblerProfilesFileChange" })]
public static class Gambler_Main_Server
{
    private static void OnInit()
    {
        ReadGamblerProfiles();
    }

    private static void OnGamblerProfilesFileChange()
    {
        ReadGamblerProfiles();
        Utils.print("Gambler Changed. Sending new info to all clients");
    }

    private static void ReadGamblerProfiles()
    {
        List<string> list = File.ReadAllLines(Market_Paths.GamblerConfig).ToList();
        ReadGamblerProfiles(list);
        ClientClearData();
    }
    
    private static void ClientClearData()
    {
        foreach (KeyValuePair<string, Gambler_DataTypes.BigData> data in Gambler_DataTypes.GamblerData.Value)
        {
            if (data.Value.Data.Count() > 19)
            {
                data.Value.Data.RemoveRange(19, data.Value.Data.Count() - 19);
            }
        }
    }

    private static void ReadGamblerProfiles(List<string> profiles)
    {
        Gambler_DataTypes.GamblerData.Value.Clear();
        string splitProfile = "default";
        int MAXSROLL = 1;
        for (int i = 0; i < profiles.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(profiles[i]) || profiles[i].StartsWith("#")) continue;
            if (profiles[i].StartsWith("["))
            {
                splitProfile = profiles[i].Replace("[", "").Replace("]", "").ToLower();

                string[] testSplit = splitProfile.Split('=');
                if (testSplit.Length > 1)
                {
                    splitProfile = testSplit[0];
                    MAXSROLL = int.Parse(testSplit[1]);
                }
                else
                {
                    MAXSROLL = 1;
                }
            }
            else
            {
                string[] split = profiles[i].Replace(" ", "").Split(',');
                if (split.Length % 2 != 0)
                {
                    Utils.print($"Line {i + 1} has error, number of data is wrong", ConsoleColor.Red);
                    continue;
                }

                List<Gambler_DataTypes.Item> data = new List<Gambler_DataTypes.Item>();
                for (int j = 0; j < split.Length; j += 2)
                {
                    Gambler_DataTypes.Item itm = new Gambler_DataTypes.Item
                    {
                        Prefab = split[j]
                    };

                    string[] minmax = split[j + 1].Split('-');

                    if (minmax.Length == 1)
                    {
                        itm.Min = int.Parse(minmax[0]);
                        itm.Max = itm.Min;
                    }
                    else
                    {
                        itm.Min = int.Parse(minmax[0]);
                        itm.Max = int.Parse(minmax[1]);
                    }

                    data.Add(itm);
                    Gambler_DataTypes.GamblerData.Value[splitProfile] = new Gambler_DataTypes.BigData()
                    {
                        MAXROLLS = MAXSROLL, Data = data
                    };
                }
            }
        }

        Gambler_DataTypes.GamblerData.Update();
    }
}
using Marketplace.Paths;

namespace Marketplace.Modules.Teleporter;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Normal, "OnInit",
    new[] { "TeleportHubProfiles.cfg" }, new[] { "OnTeleporterProfilesChange" })]
public static class Teleporter_Main_Server
{
    
    private static void OnInit()
    {
        ProcessTeleporterData();
    }
    
    private static void ReadServerTeleporterSprites()
    {
        Teleporter_DataTypes.TeleporterSprites.Value.Clear();
        string[] files = Directory.GetFiles(Market_Paths.TeleporterPinsFolder, "*.png", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            string name = Path.GetFileNameWithoutExtension(file);
            byte[] data = File.ReadAllBytes(file);
            Teleporter_DataTypes.TeleporterSprites.Value[name] = new Teleporter_DataTypes.TransferBytes(){array = data};
        }
        Teleporter_DataTypes.TeleporterSprites.Update();
    }
    
    private static void ReadServerTeleporterProfile()
    {
        IReadOnlyList<string> profiles = File.ReadAllLines(Market_Paths.TeleporterPinsConfig);
        Teleporter_DataTypes.TeleporterDataServer.Value.Clear();
        string splitProfile = "default";
        for (int i = 0; i < profiles.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(profiles[i]) || profiles[i].StartsWith("#")) continue;
            if (profiles[i].StartsWith("["))
            {
                splitProfile = profiles[i].Replace("[", "").Replace("]", "").Replace(" ","").ToLower();
            }
            else
            {
                string[] test = profiles[i].Split(',');
                try
                {

                    int _speed = 0;
                    string name = test[0];
                    int index = name.IndexOf("<speed=", StringComparison.Ordinal);
                    if (index != -1)
                    {
                        string sub = name.Substring(index + "<speed=".Length);
                        int endIndex = sub.IndexOf('>');
                        if (endIndex > 0)
                        {
                            string findSpeed = sub.Replace(">", "");
                            if (!string.IsNullOrWhiteSpace(findSpeed))
                            { 
                                _speed = Convert.ToInt32(findSpeed);
                            }
                            name = Utils.RemoveRichTextDynamicTag(name, "speed");
                        }
                    }

                    Teleporter_DataTypes.TeleporterData pinnerData = new Teleporter_DataTypes.TeleporterData
                    {
                        name = name,
                        x = int.Parse(test[1].Replace(" ", "")),
                        y = int.Parse(test[2].Replace(" ", "")),
                        z = int.Parse(test[3].Replace(" ", "")),
                        sprite = test.Length == 5 ? test[4].Replace(" ", "") : "none",
                        speed =  _speed
                    };

                    if (Teleporter_DataTypes.TeleporterDataServer.Value.TryGetValue(splitProfile, out List<Teleporter_DataTypes.TeleporterData> value))
                    {
                        value.Add(pinnerData);
                    }
                    else
                    {
                        Teleporter_DataTypes.TeleporterDataServer.Value[splitProfile] = new List<Teleporter_DataTypes.TeleporterData> { pinnerData };
                    }
                }
                catch (Exception ex)
                {
                    Utils.print($"{ex}\nERROR: Can't add pins in TeleportHubProfiles.cfg, line: {i}", ConsoleColor.Red);
                    return;
                }
            }
        }
        Teleporter_DataTypes.TeleporterDataServer.Update();
    }
    
    private static void ProcessTeleporterData()
    {
        ReadServerTeleporterSprites();
        ReadServerTeleporterProfile();
    }
    

    private static void OnTeleporterProfilesChange()
    {
        ProcessTeleporterData();
        Utils.print("TeleportHub changed, sending options to peers");
    }
    
}
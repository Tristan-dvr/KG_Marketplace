using Marketplace.Paths;

namespace Marketplace.Modules.NPC_Dialogues;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Normal, "OnInit",
    new[] { "NpcDialogues.cfg" },
    new[] { "OnDialoguesChange" })]
public class Dialogues_Main_Server
{
    private static void OnInit()
    {
        ReadDialoguesData();
    }

    private enum DataType
    {
        Text,
        Transition,
        Command,
        Icon,
        Condition,
        AlwaysVisible
    }

    private static void ProcessDialogueProfiles(List<string> profiles)
    {
        Dialogues_DataTypes.RawDialogue dialogue = null;
        List<Dialogues_DataTypes.RawDialogue.RawPlayerOption> options = null;
        for (int i = 0; i < profiles.Count; i++)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(profiles[i]) || profiles[i].StartsWith("#")) continue;
                if (profiles[i].StartsWith("["))
                {
                    if (dialogue != null)
                    {
                        dialogue.Options = options.ToArray();
                        Dialogues_DataTypes.SyncedDialoguesData.Value.Add(dialogue);
                        dialogue = null;
                    }

                    string splitProfile = profiles[i].Replace("[", "").Replace("]", "").Replace(" ","").ToLower();
                    dialogue = new Dialogues_DataTypes.RawDialogue
                    {
                        UID = splitProfile.Replace(@"\n", "\n")
                    };
                    options = new List<Dialogues_DataTypes.RawDialogue.RawPlayerOption>();
                }
                else
                {
                    if (dialogue == null) continue;
                    if (dialogue.Text == null)
                    {
                        dialogue.Text = profiles[i];
                    }
                    else
                    {
                        Dialogues_DataTypes.RawDialogue.RawPlayerOption option = new Dialogues_DataTypes.RawDialogue.RawPlayerOption();
                        List<string> commands = new List<string>();
                        List<string> conditions = new List<string>(); 
                        string[] split = profiles[i].Split('|');
                        foreach (string s in split)
                        {
                            string[] enumCheck = s.Split(new[] { ':' }, 2);
                            if (enumCheck.Length != 2) continue;
                            if (!Enum.TryParse(enumCheck[0], true, out DataType type)) continue;
                            switch (type)
                            {
                                case DataType.Text:
                                    option.Text = enumCheck[1].Trim().Replace(@"\n", "\n");
                                    break;
                                case DataType.Transition:
                                    option.NextUID = enumCheck[1].Replace(" ", "").ToLower();
                                    break;
                                case DataType.Command:
                                    commands.Add(enumCheck[1].Replace(" ", ""));
                                    break;
                                case DataType.Icon:
                                    option.Icon = enumCheck[1].Replace(" ", "");
                                    break;
                                case DataType.Condition:
                                    conditions.Add(enumCheck[1].Replace(" ", ""));
                                    break;
                                case DataType.AlwaysVisible:
                                    option.AlwaysVisible = bool.Parse(enumCheck[1].Replace(" ", ""));
                                    break;
                            }
                        }
                        option.Commands = commands.ToArray();
                        option.Conditions = conditions.ToArray();
                        options.Add(option);
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.print($"Error reading NpcDialogues.cfg (line {i + 1}):\n" + ex);
                break;
            }
        }

        if (dialogue != null)
        {
            dialogue.Options = options?.ToArray();
            Dialogues_DataTypes.SyncedDialoguesData.Value.Add(dialogue);
        }
    }
    
    private static void ReadDialoguesData()
    {
        List<string> profiles = File.ReadAllLines(Market_Paths.NpcDialoguesConfig).ToList();
        Dialogues_DataTypes.SyncedDialoguesData.Value.Clear();
        ProcessDialogueProfiles(profiles);
        string folder = Market_Paths.AdditionalConfigsDialoguesFolder;
        string[] files = Directory.GetFiles(folder, "*.cfg", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            profiles = File.ReadAllLines(file).ToList();
            ProcessDialogueProfiles(profiles);
        }
        
        Dialogues_DataTypes.SyncedDialoguesData.Update();
    }

    private static void OnDialoguesChange()
    {
        ReadDialoguesData();
        Utils.print("NpcDialogues changed, sending options to peers");
    }
}
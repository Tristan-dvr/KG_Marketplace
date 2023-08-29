using Marketplace.Paths;

namespace Marketplace.Modules.NPC_Dialogues;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Normal, "OnInit",
    new[] { "DI" },
    new[] { "OnDialoguesChange" })]
public class Dialogues_Main_Server
{
    [UsedImplicitly]
    private static void OnInit()
    {
        ReadDialoguesData();
    }

    private enum InputType
    {
        Text,
        Transition,
        Command,
        Icon,
        Condition,
        AlwaysVisible,
        Color
    }

    private static void ProcessDialogueProfiles(string fPath, IReadOnlyList<string> profiles)
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
                        dialogue.Options = options!.ToArray();
                        Dialogues_DataTypes.SyncedDialoguesData.Value.Add(dialogue);
                        dialogue = null;
                    }

                    string[] splitProfile = profiles[i].Replace("[", "").Replace("]", "").Replace(" ", "").Split('=');
                    dialogue = new Dialogues_DataTypes.RawDialogue
                    {
                        UID = splitProfile[0].ToLower(),
                        BG_ImageLink = splitProfile.Length > 1 ? splitProfile[1] : null
                    };
                    options = new List<Dialogues_DataTypes.RawDialogue.RawPlayerOption>();
                }
                else
                {
                    if (dialogue == null) continue;
                    if (dialogue.Text == null)
                    {
                        dialogue.Text = profiles[i].Replace(@"\n", "\n");
                    }
                    else
                    {
                        Dialogues_DataTypes.RawDialogue.RawPlayerOption option =
                            new Dialogues_DataTypes.RawDialogue.RawPlayerOption();
                        List<string> commands = new List<string>();
                        List<string> conditions = new List<string>();
                        string[] split = profiles[i].Split('|');
                        foreach (string s in split)
                        {
                            string[] enumCheck = s.Split(new[] { ':' }, 2);
                            if (enumCheck.Length != 2) continue;
                            if (!Enum.TryParse(enumCheck[0], true, out InputType type)) continue;
                            switch (type)
                            {
                                case InputType.Text:
                                    option.Text = enumCheck[1].Trim().Replace(@"\n", "\n");
                                    break;
                                case InputType.Transition:
                                    option.NextUID = enumCheck[1].Replace(" ", "").ToLower();
                                    break;
                                case InputType.Command:
                                    commands.Add(enumCheck[1].Replace(" ", ""));
                                    break;
                                case InputType.Icon:
                                    option.Icon = enumCheck[1].Replace(" ", "");
                                    break;
                                case InputType.Condition:
                                    conditions.Add(enumCheck[1].Replace(" ", ""));
                                    break;
                                case InputType.AlwaysVisible:
                                    option.AlwaysVisible = bool.Parse(enumCheck[1].Replace(" ", ""));
                                    break;
                                case InputType.Color:
                                    string[] colorSplit = enumCheck[1].Split(',');
                                    if (colorSplit.Length != 3) continue;
                                    option.Color = new Color32(byte.Parse(colorSplit[0]), byte.Parse(colorSplit[1]),
                                        byte.Parse(colorSplit[2]), 255);
                                    break;
                            }
                        }

                        option.Commands = commands.ToArray();
                        option.Conditions = conditions.ToArray();
                        options!.Add(option);
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.print($"Error reading {fPath} (line {i + 1}):\n" + ex);
                break;
            }
        }

        if (dialogue != null)
        {
            dialogue.Options = options?.ToArray()!;
            Dialogues_DataTypes.SyncedDialoguesData.Value.Add(dialogue);
        }
    }

    private static void ReadDialoguesData()
    {
        Dialogues_DataTypes.SyncedDialoguesData.Value.Clear();
        string folder = Market_Paths.DialoguesFolder;
        string[] files = Directory.GetFiles(folder, "*.cfg", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            IReadOnlyList<string> profiles = File.ReadAllLines(file).ToList();
            ProcessDialogueProfiles(file, profiles);
        }

        Dialogues_DataTypes.SyncedDialoguesData.Update();
    }

    [UsedImplicitly]
    private static void OnDialoguesChange()
    {
        ReadDialoguesData();
        Utils.print("NpcDialogues changed, sending options to peers");
    }
}
using Jewelcrafting;
using Marketplace.Paths;

namespace Marketplace.Modules.Lootboxes;

[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Normal,
    OnWatcherNames: new[] { "LB" },
    OnWatcherMethods: new[] { "Reload" })]
public class Lootboxes_Main_Server
{
    [UsedImplicitly]
    private static void OnInit()
    {
        ReadLootboxesData();
    }

    private static void ProcessLootboxesProfiles(string fpath, IReadOnlyList<string> profiles)
    {
        Lootboxes_DataTypes.Lootbox _currentLootbox = null;
        for (var k = 0; k < profiles.Count; k++)
        {
            if (string.IsNullOrWhiteSpace(profiles[k]) || profiles[k].StartsWith("#")) continue;
            if (profiles[k].StartsWith("["))
            {
                _currentLootbox = new();
                string[] profileSplit = profiles[k].Replace("[", "").Replace("]", "").Replace(" ","").Split('=');
                _currentLootbox.UID = profileSplit[0];
                _currentLootbox.GiveAll = profileSplit.Length == 2 && profileSplit[1].ToLower() == "all";
            }
            else
            {
                if (_currentLootbox == null) continue;
                try
                {
                    string[] split = profiles[k].Replace(" ", "").Split(',');
                    if (split.Length % 3 != 0) continue;
                    for (int i = 0; i < split.Length; i += 3)
                    {
                        string prefab = split[i];
                        string range = split[i + 1];
                        string[] splitRange = range.Split('-');
                        int min = int.Parse(splitRange[0]);
                        int max = splitRange.Length == 2 ? int.Parse(splitRange[1]) : min;
                        int level = int.Parse(split[i + 2]);
                        _currentLootbox.Items.Add(new(prefab, min, max, level));
                    }

                    string description = profiles[k + 1];
                    if (!string.IsNullOrWhiteSpace(description))
                        _currentLootbox.AdditionalDescription = description.Replace(@"\n", "\n");
                    string icon = profiles[k + 2].Replace(" ", "");
                    if (!string.IsNullOrWhiteSpace(icon))
                        _currentLootbox.Icon = icon;
                    string openVFX = profiles[k + 3].Replace(" ", "");
                    if (!string.IsNullOrWhiteSpace(openVFX))
                        _currentLootbox.OpenVFX = openVFX;

                    Lootboxes_DataTypes.SyncedLootboxData.Value.Add(_currentLootbox);
                    _currentLootbox = null;
                }
                catch (Exception ex)
                {
                    Utils.print($"Error while processing Lootboxes in file {fpath}: {ex}");
                }
            }
        }
    }

    private static void ReadLootboxesData()
    {
        Lootboxes_DataTypes.SyncedLootboxData.Value.Clear();
        string folder = Market_Paths.LootboxesFolder;
        string[] files = Directory.GetFiles(folder, "*.cfg", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            IReadOnlyList<string> profiles = File.ReadAllLines(file).ToList();
            ProcessLootboxesProfiles(file, profiles);
        }

        Lootboxes_DataTypes.SyncedLootboxData.Value.RemoveAll(item => item.Items.Count == 0);
        Lootboxes_DataTypes.SyncedLootboxData.Update();
    }

    [UsedImplicitly]
    private static void Reload()
    {
        ReadLootboxesData();
        Utils.print("Lootboxes changed, sending options to peers");
    }
}
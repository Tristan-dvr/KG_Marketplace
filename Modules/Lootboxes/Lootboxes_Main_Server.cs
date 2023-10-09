using Jewelcrafting;
using Marketplace.Paths;
using YamlDotNet.Serialization;

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
                _currentLootbox.UID = profiles[k].Replace("[", "").Replace("]", "").Replace(" ", "");
                if (_currentLootbox.UID.Contains("@DISCORD"))
                {
                    _currentLootbox.UID = _currentLootbox.UID.Replace("@DISCORD", "");
                    _currentLootbox.Webhook = true;
                }
            }
            else
            {
                if (_currentLootbox == null) continue;
                try
                {
                    string type = profiles[k].Replace(" ", "").ToLower();
                    Lootboxes_DataTypes.Lootbox.LBType? toEnum =
                        Enum.TryParse(type, true, out Lootboxes_DataTypes.Lootbox.LBType result) ? result : null;
                    if (toEnum == null)
                    {
                        Utils.print(
                            $"Error while processing Lootboxes in file {fpath}: Unknown type {type}. Lootbox: {_currentLootbox?.UID}");
                        _currentLootbox = null;
                        continue;
                    }

                    _currentLootbox.Type = toEnum.Value;
                    string[] split = profiles[k + 1].Replace(" ", "").Split(',');
                    bool needChance = toEnum is Lootboxes_DataTypes.Lootbox.LBType.AllWithChance
                        or Lootboxes_DataTypes.Lootbox.LBType.AllWithChanceShowTooltip;
                    int needChanceVal = needChance ? 4 : 3;
                    if (split.Length % needChanceVal != 0) continue;
                    for (int i = 0; i < split.Length; i += needChanceVal)
                    {
                        string prefab = split[i];
                        string range = split[i + 1];
                        string[] splitRange = range.Split('-');
                        int min = int.Parse(splitRange[0]);
                        int max = splitRange.Length == 2 ? int.Parse(splitRange[1]) : min;
                        int level = int.Parse(split[i + 2]);
                        int chance = needChance ? int.Parse(split[i + 3]) : 100;
                        _currentLootbox.Items.Add(new(prefab, min, max, level, chance));
                    }

                    string description = profiles[k + 2];
                    if (!string.IsNullOrWhiteSpace(description))
                        _currentLootbox.AdditionalDescription = description.Replace(@"\n", "\n");
                    string icon = profiles[k + 3].Replace(" ", "");
                    if (!string.IsNullOrWhiteSpace(icon))
                        _currentLootbox.Icon = icon;
                    string openVFX = profiles[k + 4].Replace(" ", "");
                    if (!string.IsNullOrWhiteSpace(openVFX))
                        _currentLootbox.OpenVFX = openVFX;

                    Lootboxes_DataTypes.SyncedLootboxData.Value.Add(_currentLootbox);
                    _currentLootbox = null;
                }
                catch (Exception ex)
                {
                    Utils.print(
                        $"Error while processing Lootboxes in file {fpath}: {ex}. Lootbox: {_currentLootbox?.UID}");
                    _currentLootbox = null;
                }
            }
        }
    }

    private static void ProcessLootboxesProfiles_YAML(string fpath, string text)
    {
        try
        {
            List<Lootboxes_DataTypes.Lootbox> lootboxes = new DeserializerBuilder().Build().Deserialize<List<Lootboxes_DataTypes.Lootbox>>(text);
            if (lootboxes == null) return;
            foreach (Lootboxes_DataTypes.Lootbox lootbox in lootboxes)
            {
                if (lootbox.Items.Count == 0) continue;
                lootbox.UID = lootbox.UID.Replace(" ", "_");
                Lootboxes_DataTypes.SyncedLootboxData.Value.Add(lootbox);
            }
        }
        catch (Exception ex)
        {
            Utils.print($"Error while processing Lootboxes (YAML) in file {fpath}: {ex}");
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

        string[] ymlFiles = Directory.GetFiles(folder, "*.yml", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles(folder, "*.yaml", SearchOption.AllDirectories)).ToArray();
        foreach (string file in ymlFiles)
        {
            string text = File.ReadAllText(file);
            ProcessLootboxesProfiles_YAML(file, text);
        }
        
        Lootboxes_DataTypes.SyncedLootboxData.Value.RemoveAll(lootbox => lootbox.Items.Count == 0);
        Lootboxes_DataTypes.SyncedLootboxData.Update();
    }

    [UsedImplicitly]
    private static void Reload()
    {
        ReadLootboxesData();
        Utils.print("Lootboxes changed, sending options to peers");
    }
}
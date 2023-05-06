using Marketplace.Paths;

namespace Marketplace.Modules.ItemMocker;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Normal, "OnInit",
    new[] { "ItemMocker.cfg" },
    new[] { "OnConfigChange" })]
public static class ItemMocker_Main_Server
{
    private static void OnInit()
    {
        ReadMockingProfiles();
    }

    private static void ReadMockingProfiles()
    {
        List<string> profiles = File.ReadAllLines(Market_Paths.MockerConfig).ToList();
        ItemMocker_DataTypes.SyncedMockedItems.Value.Clear();
        string splitProfile = null;
        for (int i = 0; i < profiles.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(profiles[i]) || profiles[i].StartsWith("#")) continue;
            if (profiles[i].StartsWith("["))
            {
                splitProfile = profiles[i].Replace("[", "").Replace("]", "");
            }
            else
            {
                if (splitProfile == null) continue;
                try
                {
                    ItemMocker_DataTypes.ItemMock itemMock = new()
                    {
                        UID = splitProfile,
                        Model = profiles[i],
                        Name = profiles[i + 1].Replace(@"\n", "\n"),
                        Description = profiles[i + 2].Replace(@"\n", "\n"),
                        MaxStack = int.Parse(profiles[i + 3]),
                        Scale = float.Parse(profiles[i + 4]),
                        Recipe = profiles[i + 5]
                    };
                    ItemMocker_DataTypes.SyncedMockedItems.Value.Add(itemMock);
                    splitProfile = null;
                }
                catch (Exception ex)
                {
                    splitProfile = null;
                    Utils.print($"Error while reading ItemMocker.cfg (line {i + 1})\n: {ex}");
                }
            }
        }

        ItemMocker_DataTypes.SyncedMockedItems.Update();
    }


    private static void OnConfigChange()
    {
        ReadMockingProfiles();
        Utils.print("ItemMocker Changed. Sending new info to all clients");
    }
}
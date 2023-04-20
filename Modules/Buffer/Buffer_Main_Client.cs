namespace Marketplace.Modules.Buffer;

[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Normal, "OnInit")]
public static class Buffer_Main_Client
{
    private static void OnInit()
    {
        Buffer_UI.Init();
        Marketplace.Global_Updator += Update;
        Buffer_DataTypes.BufferProfiles.ValueChanged += OnBufferUpdate;
        Buffer_DataTypes.BufferBuffs.ValueChanged += OnBufferUpdate;
    }
    
    private static void Update()
    {
        if (!Player.m_localPlayer || !Input.GetKeyDown(KeyCode.Escape)) return;
        if (Buffer_UI.IsVisible())
        {
            Buffer_UI.Hide();
            Menu.instance.OnClose();
        }
    }

    private static void OnBufferUpdate()
    {
        Buffer_DataTypes.ALLBufferProfiles.Clear();
        foreach (Buffer_DataTypes.BufferBuffData buff in Buffer_DataTypes.AllCustomBuffs)
        {
            ObjectDB.instance.m_StatusEffects.Remove(buff.GetMain());
        }

        Buffer_DataTypes.AllCustomBuffs.Clear();
        try
        {
            foreach (KeyValuePair<string, string> kvp in Buffer_DataTypes.BufferProfiles.Value)
            {
                Buffer_DataTypes.ALLBufferProfiles.Add(kvp.Key, new());
                foreach (string split in kvp.Value.Split(','))
                {
                    if (string.IsNullOrEmpty(split)) continue;
                    if (Buffer_DataTypes.BufferBuffs.Value.Find(d => d.UniqueName.ToLower() == split.ToLower()) is var find and not null)
                    {
                        if (find.Init())
                            Buffer_DataTypes.ALLBufferProfiles[kvp.Key].Add(find);
                    }
                }
            }

            Buffer_UI.Reload();
        }
        catch (Exception ex)
        {
            Utils.print($"Error on trying to Init Buff:\n{ex}", ConsoleColor.Red);
        }
    }
}
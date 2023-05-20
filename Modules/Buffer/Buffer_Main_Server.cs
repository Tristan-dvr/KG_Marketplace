using Marketplace.Paths;

namespace Marketplace.Modules.Buffer;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Normal, "OnInit",
    new[] { "BufferProfiles.cfg", "BufferDatabase.cfg" },
    new[] { "OnBufferProfilesFileChange", "OnBufferProfilesFileChange" })]
public static class Buffer_Main_Server
{
    private static void OnInit()
    {
        try
        {
            IReadOnlyList<string> profiles = File.ReadAllLines(Market_Paths.BufferProfilesConfig);
            ReadBufferProfiles(profiles);
            IReadOnlyList<string> database = File.ReadAllLines(Market_Paths.BufferDatabaseConfig);
            ReadBufferDatabase(database);
        }
        catch (Exception ex)
        {
            Utils.print($"BUFFER ERROR:\n{ex}", ConsoleColor.Red);
        }
    }

    private static void OnBufferProfilesFileChange()
    {
        try
        {
            IReadOnlyList<string> profiles = File.ReadAllLines(Market_Paths.BufferProfilesConfig);
            ReadBufferProfiles(profiles);
            IReadOnlyList<string> database = File.ReadAllLines(Market_Paths.BufferDatabaseConfig);
            ReadBufferDatabase(database);
        }
        catch (Exception ex)
        {
            Utils.print($"Got exception on save buffer. Not sending data:{ex}", ConsoleColor.Red);
            return;
        }
        Utils.print("Buffer Profiles / Database Changed. Sending new info to all clients");
    }


    private static void ReadBufferProfiles(IReadOnlyList<string> profiles)
    {
        Buffer_DataTypes.BufferProfiles.Value.Clear();
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
                if (!Buffer_DataTypes.BufferProfiles.Value.ContainsKey(splitProfile))
                {
                    Buffer_DataTypes.BufferProfiles.Value.Add(splitProfile, profiles[i].Replace(" ", ""));
                }
            }
        }

        Buffer_DataTypes.BufferProfiles.Update();
    }

    private static void ReadBufferDatabase(IReadOnlyList<string> profiles)
    {
        Buffer_DataTypes.BufferBuffs.Value.Clear();
        string dbProfile = null;
        for (int i = 0; i < profiles.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(profiles[i]) || profiles[i].StartsWith("#")) continue;
            if (profiles[i].StartsWith("["))
            {
                dbProfile = profiles[i].Replace("[", "").Replace("]", "").Replace(" ", "");
            }
            else
            {
                if (dbProfile == null) continue;
                if (i + 5 > profiles.Count) break;
                Buffer_DataTypes.BufferBuffData data = new()
                {
                    UniqueName = dbProfile,
                    Name = profiles[i],
                    Duration = int.Parse(profiles[i + 1].Replace(" ", "")),
                    SpritePrefab = profiles[i + 2].Replace(" ", "")
                };
                string[] price = profiles[i + 3].Replace(" ", "").Split(',');
                data.NeededPrefab = price[0];
                data.NeededPrefabCount = int.Parse(price[1]);
                Buffer_DataTypes.WhatToModify _flags = Buffer_DataTypes.WhatToModify.None;
                string[] splitFlags = profiles[i + 4].Replace(" ", "").Split(',');
                foreach (string flag in splitFlags)
                {
                    string[] split = flag.Split('=');
                    if (split.Length != 2) continue;
                    string workingFlag = split[0];
                    float customData = float.Parse(split[1], NumberStyles.AllowDecimalPoint,
                        CultureInfo.InvariantCulture);
                    if (!Enum.TryParse(workingFlag, true, out Buffer_DataTypes.WhatToModify testFlag)) continue;
                    _flags |= testFlag;
                    switch (testFlag)
                    {
                        case Buffer_DataTypes.WhatToModify.ModifyAttack:
                            data.ModifyAttack = customData;
                            break;
                        case Buffer_DataTypes.WhatToModify.ModifyHealthRegen:
                            data.ModifyHealthRegen = customData;
                            break;
                        case Buffer_DataTypes.WhatToModify.ModifyStaminaRegen:
                            data.ModifyStaminaRegen = customData;
                            break;
                        case Buffer_DataTypes.WhatToModify.ModifyRaiseSkills:
                            data.ModifyRaiseSkills = customData;
                            break;
                        case Buffer_DataTypes.WhatToModify.ModifySpeed:
                            data.MofidySpeed = customData;
                            break;
                        case Buffer_DataTypes.WhatToModify.ModifyNoise:
                            data.ModifyNoise = customData;
                            break;
                        case Buffer_DataTypes.WhatToModify.ModifyMaxCarryWeight:
                            data.ModifyMaxCarryWeight = customData;
                            break;
                        case Buffer_DataTypes.WhatToModify.ModifyStealth:
                            data.MofidyStealth = customData;
                            break;
                        case Buffer_DataTypes.WhatToModify.RunStaminaDrain:
                            data.RunStaminaDrain = customData;
                            data.ModifyJumpStaminaUsage = customData;
                            break;
                        case Buffer_DataTypes.WhatToModify.DamageReduction:
                            data.DamageReduction = customData;
                            break;
                    }
                }

                data.Flags = _flags;
                data.StartEffectPrefab = profiles[i + 5].Replace(" ", "");
                data.BuffGroup = profiles[i + 6].Trim(' ');
                Buffer_DataTypes.BufferBuffs.Value.Add(data);
                dbProfile = null;
            }
        }

        Buffer_DataTypes.BufferBuffs.Update();
    }
}
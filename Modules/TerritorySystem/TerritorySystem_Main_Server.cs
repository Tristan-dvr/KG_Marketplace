using Marketplace.Paths;

namespace Marketplace.Modules.TerritorySystem;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Normal, "OnInit",
    new[] { "TD" },
    new[] { "OnTerritoryConfigChange" })]
public static class TerritorySystem_Main_Server
{
    private static void OnInit()
    {
        ReadServerTerritoryDatabase();
    }

    private static void OnTerritoryConfigChange()
    {
        ReadServerTerritoryDatabase();
        Utils.print("Territory Changed. Sending new info to all clients");
    }

    private static void ProcessTerritoryConfig(string fPath, IReadOnlyList<string> profiles)
    {
        string splitProfile = "default";
        int Priority = 1;
        for (int i = 0; i < profiles.Count; ++i)
        {
            if (string.IsNullOrWhiteSpace(profiles[i]) || profiles[i].StartsWith("#")) continue;
            if (profiles[i].StartsWith("["))
            {
                string[] split = profiles[i].Replace("[", "").Replace("]", "").Split('@');
                splitProfile = split[0];
                Priority = 1;
                if (split.Length == 2)
                {
                    Priority = int.Parse(split[1]);
                }
            }
            else
            {
                try
                {
                    if (i + 4 > profiles.Count) break;
                    TerritorySystem_DataTypes.Territory newTerritory = new()
                    {
                        Name = splitProfile,
                        Priority = Priority
                    };
                    if (!(Enum.TryParse(profiles[i], true,  out TerritorySystem_DataTypes.TerritoryType type) &&
                          Enum.IsDefined(typeof(TerritorySystem_DataTypes.TerritoryType), type))) continue;
                    newTerritory.Type = type;

                    string[] xyr = profiles[i + 1].Replace(" ", "").Split(',');

                    if (type is not TerritorySystem_DataTypes.TerritoryType.Custom)
                    {
                        newTerritory.X = Convert.ToInt32(xyr[0]);
                        newTerritory.Y = Convert.ToInt32(xyr[1]);
                        newTerritory.Radius = Convert.ToInt32(xyr[2]);
                    }
                    else
                    {
                        newTerritory.X = Convert.ToInt32(xyr[0]);
                        newTerritory.Y = Convert.ToInt32(xyr[1]);
                        newTerritory.Xlength = Convert.ToInt32(xyr[2]);
                        newTerritory.Ylength = Convert.ToInt32(xyr[3]);
                    }


                    List<string> rgb = profiles[i + 2].Replace(" ", "").Split(',').ToList();

                    // draw water
                    for (int tf = 0; tf < rgb.Count; ++tf)
                    {
                        if (rgb[tf].ToLower() is "true" or "false")
                        {
                            newTerritory.ShowExternalWater = Convert.ToBoolean(rgb[tf]);
                            rgb.RemoveAt(tf);
                            break;
                        }
                    }

                    // find gradient type
                    for (int tf = 0; tf < rgb.Count; ++tf)
                    {
                        if (!int.TryParse(rgb[tf], out _) && Enum.TryParse(rgb[tf], true,
                                out TerritorySystem_DataTypes.GradientType gradient))
                        {
                            newTerritory.GradientType = gradient;
                            rgb.RemoveAt(tf);
                            break;
                        }
                    }

                    // find exp value
                    for (int tf = 0; tf < rgb.Count; ++tf)
                    {
                        if (rgb[tf].ToLower().Contains("exp:"))
                        {
                            string val = rgb[tf].Split(':')[1];
                            if (!float.TryParse(val, NumberStyles.AllowDecimalPoint,
                                    CultureInfo.InvariantCulture, out float exp)) continue;
                            newTerritory.ExponentialValue = exp;
                            rgb.RemoveAt(tf);
                            break;
                        }
                    }

                    // find specify height override
                    for (int tf = 0; tf < rgb.Count; ++tf)
                    {
                        if (rgb[tf].ToLower().Contains("heightbounds:"))
                        {
                            string val = rgb[tf].Split(':')[1];
                            string[] split = val.Split('-');
                            newTerritory.HeightBounds = new(int.Parse(split[0]), int.Parse(split[1]));
                            rgb.RemoveAt(tf);
                            break;
                        }
                    }

                    newTerritory.Colors = new();
                    for (int tf = 0; tf < rgb.Count; tf += 3)
                    {
                        try
                        {
                            newTerritory.Colors.Add(new Color32
                            {
                                r = Convert.ToByte(rgb[tf]),
                                g = Convert.ToByte(rgb[tf + 1]),
                                b = Convert.ToByte(rgb[tf + 2])
                            });
                        }
                        catch
                        {
                            break;
                        }
                    }


                    TerritorySystem_DataTypes.TerritoryFlags flags = TerritorySystem_DataTypes.TerritoryFlags.None;
                    TerritorySystem_DataTypes.AdditionalTerritoryFlags additionalflags =
                        TerritorySystem_DataTypes.AdditionalTerritoryFlags.None;
                    TerritorySystem_DataTypes.PaintType PaintGround = TerritorySystem_DataTypes.PaintType.Paved;
                    string[] splitFlags = profiles[i + 3].ReplaceSpacesOutsideQuotes().Split(',');
                    foreach (string flag in splitFlags)
                    {
                        string customData = "";
                        string[] split = flag.Split('=');
                        string workingFlag = split[0];
                        if (split.Length == 2)
                        {
                            customData = split[1];
                        }

                        if (Enum.TryParse(workingFlag, true, out TerritorySystem_DataTypes.AdditionalTerritoryFlags testAdditionalFlag) &&
                            Enum.IsDefined(typeof(TerritorySystem_DataTypes.AdditionalTerritoryFlags),
                                testAdditionalFlag))
                        {
                            additionalflags |= testAdditionalFlag;
                            switch (testAdditionalFlag)
                            {
                                case TerritorySystem_DataTypes.AdditionalTerritoryFlags.ForceWind:
                                    newTerritory.Wind = Convert.ToSingle(customData, new CultureInfo("en-US"));
                                    break;
                                case TerritorySystem_DataTypes.AdditionalTerritoryFlags.DropMultiplier:
                                    newTerritory.DropMultiplier = Convert.ToSingle(customData, new CultureInfo("en-US"));
                                    break;
                                case TerritorySystem_DataTypes.AdditionalTerritoryFlags.OnlyForGuild:
                                    newTerritory.OnlyForGuild = customData;
                                    break;
                            }
                            continue;
                        }

                        if (!(Enum.TryParse(workingFlag, true, out TerritorySystem_DataTypes.TerritoryFlags testFlag) &&
                              Enum.IsDefined(typeof(TerritorySystem_DataTypes.TerritoryFlags), testFlag))) continue;
                        flags |= testFlag;
                        switch (testFlag)
                        {
                            case TerritorySystem_DataTypes.TerritoryFlags.CustomEnvironment:
                                newTerritory.CustomEnvironment = customData;
                                break;
                            case TerritorySystem_DataTypes.TerritoryFlags.PeriodicDamage:
                                newTerritory.PeriodicDamageValue = Convert.ToSingle(customData, new CultureInfo("en-US"));
                                break;
                            case TerritorySystem_DataTypes.TerritoryFlags.PeriodicHealALL:
                            case TerritorySystem_DataTypes.TerritoryFlags.PeriodicHeal:
                                newTerritory.PeriodicHealValue = Convert.ToSingle(customData, new CultureInfo("en-US"));
                                break;
                            case TerritorySystem_DataTypes.TerritoryFlags.IncreasedMonsterDamage:
                                newTerritory.IncreasedMonsterDamageValue = Convert.ToSingle(customData, new CultureInfo("en-US"));
                                break;
                            case TerritorySystem_DataTypes.TerritoryFlags.IncreasedPlayerDamage:
                                newTerritory.IncreasedPlayerDamageValue = Convert.ToSingle(customData, new CultureInfo("en-US"));
                                break;
                            case TerritorySystem_DataTypes.TerritoryFlags.MoveSpeedMultiplier:
                                newTerritory.MoveSpeedMultiplier = Convert.ToSingle(customData, new CultureInfo("en-US"));
                                break;
                            case TerritorySystem_DataTypes.TerritoryFlags.ForceGroundHeight
                                or TerritorySystem_DataTypes.TerritoryFlags.AddGroundHeight
                                or TerritorySystem_DataTypes.TerritoryFlags.LimitZoneHeight:
                                newTerritory.OverridenHeight = Convert.ToSingle(customData, new CultureInfo("en-US"));
                                break;
                            case TerritorySystem_DataTypes.TerritoryFlags.ForceBiome:
                                newTerritory.OverridenBiome = Convert.ToInt32(customData, new CultureInfo("en-US"));
                                break;
                            case TerritorySystem_DataTypes.TerritoryFlags.MonstersAddStars:
                                newTerritory.AddMonsterLevel = Convert.ToInt32(customData, new CultureInfo("en-US"));
                                break;
                            case TerritorySystem_DataTypes.TerritoryFlags.CustomPaint:
                                PaintGround =
                                    (TerritorySystem_DataTypes.PaintType)Convert.ToInt32(customData,
                                        new CultureInfo("en-US"));
                                break;
                        }
                    }
                    newTerritory.Owners = string.IsNullOrEmpty(profiles[i + 4]) ? "None" : profiles[i + 4].Replace(" ", "");
                    newTerritory.Flags = flags;
                    newTerritory.AdditionalFlags = additionalflags;
                    TerritorySystem_DataTypes.SyncedTerritoriesData.Value.Add(newTerritory);
                }
                catch (Exception ex)
                {
                    Utils.print($"Error loading zone profile {splitProfile} in {fPath}:\n{ex}", ConsoleColor.Red);
                }
            }
        }
    }

    private static void ReadServerTerritoryDatabase()
    {
        TerritorySystem_DataTypes.SyncedTerritoriesData.Value.Clear();
        string folder = Market_Paths.TerritoriesFolder;
        string[] files = Directory.GetFiles(folder, "*.cfg", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            IReadOnlyList<string> profiles = File.ReadAllLines(file).ToList();
            ProcessTerritoryConfig(file, profiles);
        }

        TerritorySystem_DataTypes.SyncedTerritoriesData.Value.Sort((x, y) => y.Priority.CompareTo(x.Priority));
        TerritorySystem_DataTypes.SyncedTerritoriesData.Update();
    }
}
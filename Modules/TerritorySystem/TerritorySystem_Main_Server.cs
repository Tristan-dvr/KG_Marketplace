using Marketplace.Paths;

namespace Marketplace.Modules.TerritorySystem;

[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Normal, "OnInit",
    new[] { "TerritoryDatabase.cfg" },
    new[] { "OnTerritoryConfigChange" })]
public static class TerritorySystem_Main_Server
{
    private static void OnInit()
    {
        List<string> territories = File.ReadAllLines(Market_Paths.TerritoriesConfigPath).ToList();
        ReadServerTerritoryDatabase(territories);
    }

    private static void OnTerritoryConfigChange()
    {
        List<string> territories = File.ReadAllLines(Market_Paths.TerritoriesConfigPath).ToList();
        try
        {
            ReadServerTerritoryDatabase(territories);
        }
        catch
        {
            // ignored
        }

        Utils.print("Territory Changed. Sending new info to all clients");
    }

    private static void ReadServerTerritoryDatabase(List<string> profiles)
    {
        TerritorySystem_DataTypes.TerritoriesData.Value.Clear();
        string splitProfile = "default";
        int zonePrio = 1;
        for (int i = 0; i < profiles.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(profiles[i]) || profiles[i].StartsWith("#")) continue;
            if (profiles[i].StartsWith("["))
            {
                string[] split = profiles[i].Replace("[", "").Replace("]", "").Split('@');
                splitProfile = split[0];
                if (split.Length == 2)
                {
                    zonePrio = int.Parse(split[1]);
                }
            }
            else
            {
                try
                {
                    if (i + 4 > profiles.Count) break;
                    TerritorySystem_DataTypes.Territory newTerritory = new()
                    {
                        Name = splitProfile
                    };
                    if (!(Enum.TryParse(profiles[i], out TerritorySystem_DataTypes.TerritoryType type) &&
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


                    string[] rgb = profiles[i + 2].Replace(" ", "").Split(',');

                    newTerritory.R = Convert.ToInt32(rgb[0]);
                    newTerritory.G = Convert.ToInt32(rgb[1]);
                    newTerritory.B = Convert.ToInt32(rgb[2]);

                    newTerritory.DrawOnMap = newTerritory.R >= 0 && newTerritory.G >= 0 && newTerritory.B >= 0;

                    newTerritory.R = Mathf.Clamp(newTerritory.R, 0, 255);
                    newTerritory.G = Mathf.Clamp(newTerritory.G, 0, 255);
                    newTerritory.B = Mathf.Clamp(newTerritory.B, 0, 255);

                    newTerritory.ShowExternalWater = true;
                    if (rgb.Length == 4)
                        newTerritory.ShowExternalWater = Convert.ToBoolean(rgb[3]);

                    TerritorySystem_DataTypes.TerritoryFlags flags = TerritorySystem_DataTypes.TerritoryFlags.None;
                    TerritorySystem_DataTypes.AdditionalTerritoryFlags additionalflags =
                        TerritorySystem_DataTypes.AdditionalTerritoryFlags.None;
                    float PeriodicHealValue = 0;
                    float PeriodicDamageValue = 0;
                    float IncreasedPlayerDamageValue = 0;
                    float IncreasedMonsterDamageValue = 0;
                    float PeriodicHealOwner = 0;
                    string CustomEnvironment = "";
                    float MoveSpeedMultiplier = 0;
                    float OverrideHeight = 0;
                    int overrideBiome = 0;
                    int addmonsterstars = 0;
                    TerritorySystem_DataTypes.PaintType _paintGround = TerritorySystem_DataTypes.PaintType.Paved;
                    string[] splitFlags = profiles[i + 3].Replace(" ", "").Split(',');
                    foreach (string flag in splitFlags)
                    {
                        string customData = "";
                        string[] split = flag.Split('=');
                        string workingFlag = split[0];
                        if (split.Length == 2)
                        {
                            customData = split[1];
                        }

                        if (Enum.TryParse(workingFlag,
                                out TerritorySystem_DataTypes.AdditionalTerritoryFlags testAdditionalFlag) &&
                            Enum.IsDefined(typeof(TerritorySystem_DataTypes.AdditionalTerritoryFlags),
                                testAdditionalFlag))
                        {
                            additionalflags |= testAdditionalFlag;
                            continue;
                        }

                        if (!(Enum.TryParse(workingFlag, out TerritorySystem_DataTypes.TerritoryFlags testFlag) &&
                              Enum.IsDefined(typeof(TerritorySystem_DataTypes.TerritoryFlags), testFlag))) continue;
                        flags |= testFlag;
                        switch (testFlag)
                        {
                            case TerritorySystem_DataTypes.TerritoryFlags.CustomEnvironment:
                                CustomEnvironment = customData;
                                break;
                            case TerritorySystem_DataTypes.TerritoryFlags.PeriodicDamage:
                                PeriodicDamageValue = Convert.ToSingle(customData, new CultureInfo("en-US"));
                                break;
                            case TerritorySystem_DataTypes.TerritoryFlags.PeriodicHealALL:
                            case TerritorySystem_DataTypes.TerritoryFlags.PeriodicHeal:
                                PeriodicHealValue = Convert.ToSingle(customData, new CultureInfo("en-US"));
                                break;
                            case TerritorySystem_DataTypes.TerritoryFlags.IncreasedMonsterDamage:
                                IncreasedMonsterDamageValue = Convert.ToSingle(customData, new CultureInfo("en-US"));
                                break;
                            case TerritorySystem_DataTypes.TerritoryFlags.IncreasedPlayerDamage:
                                IncreasedPlayerDamageValue = Convert.ToSingle(customData, new CultureInfo("en-US"));
                                break;
                            case TerritorySystem_DataTypes.TerritoryFlags.MoveSpeedMultiplier:
                                MoveSpeedMultiplier = Convert.ToSingle(customData, new CultureInfo("en-US"));
                                break;
                            case TerritorySystem_DataTypes.TerritoryFlags.ForceGroundHeight
                                or TerritorySystem_DataTypes.TerritoryFlags.AddGroundHeight
                                or TerritorySystem_DataTypes.TerritoryFlags.LimitZoneHeight:
                                OverrideHeight = Convert.ToSingle(customData, new CultureInfo("en-US"));
                                break;
                            case TerritorySystem_DataTypes.TerritoryFlags.ForceBiome:
                                overrideBiome = Convert.ToInt32(customData, new CultureInfo("en-US"));
                                break;
                            case TerritorySystem_DataTypes.TerritoryFlags.MonstersAddStars:
                                addmonsterstars = Convert.ToInt32(customData, new CultureInfo("en-US"));
                                break;
                            case TerritorySystem_DataTypes.TerritoryFlags.CustomPaint:
                                _paintGround =
                                    (TerritorySystem_DataTypes.PaintType)Convert.ToInt32(customData,
                                        new CultureInfo("en-US"));
                                break;
                        }
                    }

                    string owners = string.IsNullOrEmpty(profiles[i + 4]) ? "None" : profiles[i + 4].Replace(" ", "");
                    newTerritory.Flags = flags;
                    newTerritory.AdditionalFlags = additionalflags;
                    newTerritory.CustomEnvironment = CustomEnvironment;
                    newTerritory.PeriodicDamageValue = PeriodicDamageValue;
                    newTerritory.PeriodicHealValue = PeriodicHealValue;
                    newTerritory.IncreasedMonsterDamageValue = IncreasedMonsterDamageValue;
                    newTerritory.IncreasedPlayerDamageValue = IncreasedPlayerDamageValue;
                    newTerritory.MoveSpeedMultiplier = MoveSpeedMultiplier;
                    newTerritory.Owners = owners;
                    newTerritory.Priority = zonePrio;
                    newTerritory.OverridenBiome = overrideBiome;
                    newTerritory.OverridenHeight = OverrideHeight;
                    newTerritory.AddMonsterLevel = addmonsterstars;
                    newTerritory.PaintGround = _paintGround;
                    TerritorySystem_DataTypes.TerritoriesData.Value.Add(newTerritory);
                }
                catch (Exception ex)
                {
                    Utils.print($"Error loading zone profile {splitProfile} : {ex}", ConsoleColor.Red);
                }
            }
        }

        TerritorySystem_DataTypes.TerritoriesData.Value.Sort((x, y) => y.Priority.CompareTo(x.Priority));
        TerritorySystem_DataTypes.TerritoriesData.Update();
    }
}
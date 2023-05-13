namespace Marketplace.Modules.Battlepass;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Normal, "OnInit")]
public static class Battlepass_Main_Client
{
    private static void OnInit()
    {
        Battlepass_UI.Init();
        Battlepass_DataTypes.SyncedBattlepassData.ValueChanged += OnBattlepassUpdate;
        Marketplace.Global_Updator += Update;
    }

    private static void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
        {
            if (Battlepass_UI.IsPanelVisible())
            {
                Battlepass_UI.Hide();
            }
            else
            {
                Battlepass_UI.Show();
            }

            return;
        }
        
        if (Input.GetKeyDown(KeyCode.Escape) && Battlepass_UI.IsPanelVisible())
        {
            Battlepass_UI.Hide();
            Menu.instance.OnClose();
        }
    }

    private static void OnBattlepassUpdate()
    {
        ProcessBattlepass();
        SetPremium(Battlepass_DataTypes.SyncedBattlepassData.Value.PremiumUsers.Contains(Global_Values._localUserID));
    }
    
    [HarmonyPatch(typeof(Player), nameof(Player.Load))]
    [ClientOnlyPatch]
    private static class Player_OnSpawned_Patch_Battlepass
    {
        private static void Postfix()
        {
            ProcessBattlepass();
            ReadEXP();
        }
    }


    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    [ClientOnlyPatch]
    public static class ZNS_Battlepass_AddPrefabs
    {
        private static void Postfix()
        {
            ZNetScene.instance.m_namedPrefabs.Add(AssetStorage.AssetStorage.FreeTakeEffect.name.GetStableHashCode(),
                AssetStorage.AssetStorage.FreeTakeEffect);
            ZNetScene.instance.m_namedPrefabs.Add(AssetStorage.AssetStorage.PremiumTakeEffect.name.GetStableHashCode(),
                AssetStorage.AssetStorage.PremiumTakeEffect);
        }
    }

    private static int CurrentEXP;
    private static readonly HashSet<int> TakenFree = new();
    private static readonly HashSet<int> TakenPremium = new();
    private static bool IsPremiumResult;

    private static void ReadEXP()
    {
        TakenFree.Clear();
        TakenPremium.Clear();
        CurrentEXP = 0;
        if (!Player.m_localPlayer) return;

        if (Player.m_localPlayer.m_knownTexts.ContainsKey("[MPASN]bp"))
        {
            Player.m_localPlayer.m_customData["[MPASN]bp"] = Player.m_localPlayer.m_knownTexts["[MPASN]bp"];
            Player.m_localPlayer.m_knownTexts.Remove("[MPASN]bp");
        }

        if (Player.m_localPlayer.m_customData.ContainsKey("[MPASN]bp"))
        {
            string data = Player.m_localPlayer.m_customData["[MPASN]bp"];
            string[] split = data.Split('|');
            int BPhash = Convert.ToInt32(split[0]);
            if (BPhash != Battlepass_DataTypes.SyncedBattlepassData.Value.UID)
            {
                Utils.print($"Battlepass Hash Chagned. Clearing data");
                Player.m_localPlayer.m_customData.Remove("[MPASN]bp");
                CurrentEXP = 0;
                SaveExp();
                return;
            }

            int exp = Convert.ToInt32(split[1]);
            CurrentEXP = exp;
            if (!string.IsNullOrEmpty(split[2]))
            {
                string[] takenFree = split[2].Split(',');
                foreach (string str in takenFree)
                {
                    TakenFree.Add(Convert.ToInt32(str));
                }
            }

            if (!string.IsNullOrEmpty(split[3]))
            {
                string[] takenPremium = split[3].Split(',');
                foreach (string str in takenPremium)
                {
                    TakenPremium.Add(Convert.ToInt32(str));
                }
            }
        }
    }

    public enum Type
    {
        Premium,
        Free
    }

    public static bool IsTaken(int order, Type type)
    {
        switch (type)
        {
            case Type.Premium:
                return TakenPremium.Contains(order);
            case Type.Free:
                return TakenFree.Contains(order);
            default:
                return TakenPremium.Contains(order);
        }
    }

    public static void SetTaken(int order, Type type)
    {
        GameObject prefab = IsPremium()
            ? AssetStorage.AssetStorage.PremiumTakeEffect
            : AssetStorage.AssetStorage.FreeTakeEffect;
        UnityEngine.Object.Instantiate(prefab, Player.m_localPlayer.transform.position + Vector3.up,
            Quaternion.identity);
        switch (type)
        {
            case Type.Premium:
                TakenPremium.Add(order);
                break;
            case Type.Free:
                TakenFree.Add(order);
                break;
        }

        SaveExp();
    }

    private static void SetPremium(bool tf)
    {
        IsPremiumResult = tf;
    }

    public static bool IsPremium()
    {
        return IsPremiumResult;
    }

    private static void SaveExp()
    {
        if (!Player.m_localPlayer) return;
        int BPhash = Battlepass_DataTypes.SyncedBattlepassData.Value.UID;
        string takenFree = "";
        foreach (int VARIABLE in TakenFree)
        {
            takenFree += VARIABLE + ",";
        }

        takenFree = takenFree.TrimEnd(',');

        string takenPremium = "";
        foreach (int VARIABLE in TakenPremium)
        {
            takenPremium += VARIABLE + ",";
        }

        takenPremium = takenPremium.TrimEnd(',');

        Player.m_localPlayer.m_customData["[MPASN]bp"] = $"{BPhash}|{CurrentEXP}|{takenFree}|{takenPremium}";
    }

    public static void AddExp(int value)
    {
        if (Battlepass_DataTypes.SyncedBattlepassData.Value.FreeRewards.Count == 0 &&
            Battlepass_DataTypes.SyncedBattlepassData.Value.PremiumRewards.Count == 0) return;
        CurrentEXP += value;
        CurrentEXP = Mathf.Min(Battlepass_DataTypes.SyncedBattlepassData.Value.ExpStep * (Battlepass_UI.FindMax + 1),
            CurrentEXP);
        SaveExp();
    }

    public static int GetExp() => CurrentEXP;

    private static int LatestRevision;
    
    [HarmonyPatch(typeof(FejdStartup),nameof(FejdStartup.Awake))]
    [ClientOnlyPatch]
    private static class FejdStartup_Awake_Patch
    {
        private static void Postfix()
        {
            LatestRevision = 0;
        }
    }
    
    private static void ProcessBattlepass()
    {
        if (LatestRevision == Battlepass_DataTypes.SyncedBattlepassData.Value._revision || !Player.m_localPlayer) return;
        LatestRevision = Battlepass_DataTypes.SyncedBattlepassData.Value._revision;
        if (Battlepass_DataTypes.SyncedBattlepassData.Value.FreeRewards.Count == 0 &&
            Battlepass_DataTypes.SyncedBattlepassData.Value.PremiumRewards.Count == 0) return;
        foreach (Battlepass_DataTypes.BattlePassElement passElement in Battlepass_DataTypes.SyncedBattlepassData.Value
                     .FreeRewards.Concat(
                         Battlepass_DataTypes.SyncedBattlepassData.Value.PremiumRewards))
        {
            for (int i = 0; i < passElement.ItemNames.Count; ++i)
            {
                GameObject prefab = ZNetScene.instance.GetPrefab(passElement.ItemNames[i]);
                if (prefab)
                {
                    if (prefab.GetComponent<ItemDrop>())
                    {
                        passElement.AddString(prefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_maxQuality > 1
                            ? $" (<color=#00ff00>{passElement.ItemLevels[i]}★</color>)"
                            : "");
                        passElement.AddSprite(prefab.GetComponent<ItemDrop>().m_itemData.GetIcon());
                        passElement.SetLocalized(
                            Localization.instance.Localize(prefab.GetComponent<ItemDrop>().m_itemData.m_shared
                                .m_name));
                    }
                    else if (prefab.GetComponent<Character>())
                    {
                        passElement.SetLocalized(
                            Localization.instance.Localize(prefab.GetComponent<Character>().m_name));
                        passElement.AddString($" (<color=#00ff00>{passElement.ItemLevels[i] - 1}★</color>)");
                        PhotoManager.__instance.MakeSprite(prefab, 0.6f, 0.25f, passElement.ItemLevels[i]);
                        passElement.AddSprite(PhotoManager.__instance.GetSprite(prefab.name,
                            AssetStorage.AssetStorage.PlaceholderMonsterIcon,
                            passElement.ItemLevels[i]));
                    }
                }
                else
                {
                    Sprite resultIcon;
                    if (!Enum.TryParse(passElement.ItemNames[i], out Skills.SkillType skill))
                    {
                        Skills.SkillDef SkillDef = Player.m_localPlayer.m_skills.GetSkillDef(
                            (Skills.SkillType)Mathf.Abs(passElement.ItemNames[i].GetStableHashCode()));
                        resultIcon = SkillDef == null
                            ? AssetStorage.AssetStorage.NullSprite
                            : SkillDef.m_icon;
                    }
                    else
                    {
                        Skills.Skill SkillDef = Player.m_localPlayer.m_skills.GetSkill(skill);
                        resultIcon = SkillDef.m_info.m_icon;
                    }

                    string name = Enum.TryParse(passElement.ItemNames[i], out Skills.SkillType _)
                        ? Localization.instance.Localize("$skill_" + passElement.ItemNames[i].ToLower())
                        : Localization.instance.Localize(
                            $"$skill_" + Mathf.Abs(passElement.ItemNames[i].GetStableHashCode()));
                    passElement.SetLocalized(name);
                    passElement.AddSprite(resultIcon);
                    passElement.AddString(" (<color=#00ff00>Skill</color>)");
                }
            }
        }
        Battlepass_UI.LoadData();
    }
}
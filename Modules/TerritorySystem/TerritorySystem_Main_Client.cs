using System.Threading.Tasks;
using Marketplace.Modules.Teleporter;

namespace Marketplace.Modules.TerritorySystem;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Normal)]
public static class TerritorySystem_Main_Client
{
    public static TerritorySystem_DataTypes.Territory CurrentTerritory;
    private static Color[] originalMapColors = null!;
    private static Color[] originalHeightColors = null!;
    private static readonly int Pulse = Animator.StringToHash("pulse");

    public static readonly
        Dictionary<TerritorySystem_DataTypes.TerritoryFlags, List<TerritorySystem_DataTypes.Territory>>
        TerritoriesByFlags = new();

    public static readonly Dictionary<TerritorySystem_DataTypes.AdditionalTerritoryFlags,
            List<TerritorySystem_DataTypes.Territory>>
        TerritoriesByFlags_Additional = new();

    private static float ZoneTick;
    private static DateTime LastNoAccessMesssage;

    private static void OnInit()
    {
        foreach (TerritorySystem_DataTypes.TerritoryFlags flag in TerritorySystem_DataTypes.AllTerritoryFlagsArray)
        {
            TerritoriesByFlags[flag] = new List<TerritorySystem_DataTypes.Territory>();
        }

        foreach (TerritorySystem_DataTypes.AdditionalTerritoryFlags flag in TerritorySystem_DataTypes
                     .AllAdditionaTerritoryFlagsArray)
        {
            TerritoriesByFlags_Additional[flag] = new List<TerritorySystem_DataTypes.Territory>();
        }

        TerritorySystem_DataTypes.SyncedTerritoriesData.ValueChanged += OnTerritoryUpdate;
        Marketplace.Global_FixedUpdator += TerritoryFixedUpdate;
        Marketplace.Global_FixedUpdator += HeightmapRebuild;
    }

    private static void TerritoryFixedUpdate()
    {
        ZoneTick -= Time.fixedDeltaTime;
        if (!Player.m_localPlayer) return;
        Player p = Player.m_localPlayer;
        Vector3 vec = p.transform.position;
        CurrentTerritory = TerritorySystem_DataTypes.Territory.GetCurrentTerritory(vec);
        if (CurrentTerritory == null) return;

        if (ParticleMist.instance && ParticleMist.instance.m_ps)
        {
            if (CurrentTerritory.AdditionalFlags.HasFlagFast(TerritorySystem_DataTypes.AdditionalTerritoryFlags.NoMist))
            {
                if (ParticleMist.instance.m_ps.emission.enabled)
                {
                    var emission = ParticleMist.instance.m_ps.emission;
                    emission.enabled = false;
                }
            }
            else
            {
                if (!ParticleMist.instance.m_ps.emission.enabled)
                {
                    var emission = ParticleMist.instance.m_ps.emission;
                    emission.enabled = true;
                }
            }
        }


        if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.PushAway) &&
            !CurrentTerritory.IsOwner())
        {
            float pushValue = Time.fixedDeltaTime * 7f;

            Vector3 newVector3 = p.transform.position +
                                 (p.transform.position - CurrentTerritory.Pos3D()).normalized * pushValue;
            p.m_body.isKinematic = true;
            p.transform.position = new Vector3(newVector3.x, p.transform.position.y, newVector3.z);
            p.m_body.isKinematic = false;
        }

        if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.PveOnly))
        {
            p.SetPVP(false);
            p.m_lastCombatTimer = 0;
            InventoryGui.instance.m_pvp.isOn = false;
        }

        if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.PvpOnly))
        {
            p.SetPVP(true);
            p.m_lastCombatTimer = 0;
            InventoryGui.instance.m_pvp.isOn = true;
        }

        if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.PeriodicDamage) &&
            !CurrentTerritory.IsOwner() &&
            ZoneTick <= 0)
        {
            HitData hit = new HitData();
            hit.m_damage.m_fire = CurrentTerritory.PeriodicDamageValue;
            p.Damage(hit);
        }

        if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.PeriodicHealALL) &&
            ZoneTick <= 0)
        {
            p.Heal(CurrentTerritory.PeriodicHealValue);
        }

        if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.PeriodicHeal) &&
            CurrentTerritory.IsOwner() &&
            ZoneTick <= 0)
        {
            p.Heal(CurrentTerritory.PeriodicHealValue);
        }

        if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.CustomEnvironment))
        {
            EnvMan.instance.SetForceEnvironment(CurrentTerritory.CustomEnvironment);
        }

        if (ZoneTick <= 0) ZoneTick = 1f;
    }

    private static void OnTerritoryUpdate()
    {
        API.ClientSide.FillingTerritoryData = true;
        foreach (KeyValuePair<TerritorySystem_DataTypes.TerritoryFlags, List<TerritorySystem_DataTypes.Territory>>
                     territoriesByFlag in TerritoriesByFlags)
        {
            territoriesByFlag.Value.Clear();
        }

        foreach (KeyValuePair<TerritorySystem_DataTypes.AdditionalTerritoryFlags,
                     List<TerritorySystem_DataTypes.Territory>> territoriesByFlag in
                 TerritoriesByFlags_Additional)
        {
            territoriesByFlag.Value.Clear();
        }

        if (TerritorySystem_DataTypes.SyncedTerritoriesData.Value.Count == 0)
        {
            API.ClientSide.FillingTerritoryData = false;
            DoMapMagic();
            return;
        }

        foreach (TerritorySystem_DataTypes.Territory territory in TerritorySystem_DataTypes.SyncedTerritoriesData.Value)
        {
            foreach (TerritorySystem_DataTypes.TerritoryFlags flag in TerritorySystem_DataTypes.AllTerritoryFlagsArray)
            {
                if (territory.Flags.HasFlagFast(flag))
                {
                    TerritoriesByFlags[flag].Add(territory);
                }
            }

            foreach (TerritorySystem_DataTypes.AdditionalTerritoryFlags flag in TerritorySystem_DataTypes
                         .AllAdditionaTerritoryFlagsArray)
            {
                if (territory.AdditionalFlags.HasFlagFast(flag))
                {
                    TerritoriesByFlags_Additional[flag].Add(territory);
                }
            }
        }

        TerritoriesByFlags[TerritorySystem_DataTypes.TerritoryFlags.ForceGroundHeight]
            .Sort((a, b) => a.Priority.CompareTo(b.Priority));
        TerritoriesByFlags[TerritorySystem_DataTypes.TerritoryFlags.AddGroundHeight]
            .Sort((a, b) => a.Priority.CompareTo(b.Priority));
        TerritoriesByFlags[TerritorySystem_DataTypes.TerritoryFlags.LimitZoneHeight]
            .Sort((a, b) => a.Priority.CompareTo(b.Priority));
        API.ClientSide.FillingTerritoryData = false;
        DoMapMagic();
        ZoneVisualizer.OnMapChange();
        if (Global_Values.SyncedGlobalOptions.Value._rebuildHeightmap &&
            (TerritoriesByFlags[TerritorySystem_DataTypes.TerritoryFlags.AddGroundHeight].Count > 0 ||
             TerritoriesByFlags[TerritorySystem_DataTypes.TerritoryFlags.ForceGroundHeight].Count > 0 ||
             TerritoriesByFlags[TerritorySystem_DataTypes.TerritoryFlags.LimitZoneHeight].Count > 0))
            rebuildIndex = 0;
    }

    private static int rebuildIndex = -1;
    private static float rebuildUptime;
    private static void HeightmapRebuild()
    {
        if (rebuildIndex == -1) return;
        rebuildUptime += Time.fixedDeltaTime;
        if (rebuildUptime < 0.08f) return;
        if (Heightmap.Instances.Count <= rebuildIndex || !Player.m_localPlayer || !Heightmap.Instances[rebuildIndex])
        {
            rebuildIndex = -1;
            return;
        }
        Heightmap.Instances[rebuildIndex].m_buildData = null;
        Heightmap.Instances[rebuildIndex].Regenerate();
        rebuildIndex++;
        rebuildUptime = 0f;
    }


    [HarmonyPatch(typeof(Minimap), nameof(Minimap.UpdateBiome))]
    [ClientOnlyPatch]
    private static class Minimap_UpdateBiome_Patch
    {
        private static bool Prefix()
        {
            if (CurrentTerritory != null && Minimap.instance.m_mode != Minimap.MapMode.Large) return false;
            return true;
        }

        private const string search = "\n<i><b></b></i>";

        private static void Postfix(Minimap __instance)
        {
            if (!Player.m_localPlayer) return;
            if (__instance.m_mode != Minimap.MapMode.Large)
            {
                if (CurrentTerritory != null && TerritorySystem_DataTypes.Territory.LastTerritory != CurrentTerritory)
                {
                    string newText = $"<color=green>{CurrentTerritory.GetName()}</color>";
                    __instance.m_biomeNameSmall.text = newText;
                    __instance.m_biomeNameSmall.GetComponent<Animator>().SetTrigger(Pulse);
                    ShowCustomTerritoryMessage(CurrentTerritory.RawName());
                    TerritorySystem_DataTypes.Territory.LastTerritory = CurrentTerritory;
                    EnvMan.instance.SetForceEnvironment("");
                }

                if (CurrentTerritory == null && TerritorySystem_DataTypes.Territory.LastTerritory != null)
                {
                    TerritorySystem_DataTypes.Territory.LastTerritory = null;
                    Minimap.instance.m_biome = Heightmap.Biome.None;
                    EnvMan.instance.SetForceEnvironment("");
                    __instance.m_biomeNameSmall.text = "";
                }
            }
            else
            {
                Vector3 vector = __instance.ScreenToWorldPoint(ZInput.IsMouseActive()
                    ? Input.mousePosition
                    : new Vector3(Screen.width / 2, Screen.height / 2));
                bool found = false;
                foreach (TerritorySystem_DataTypes.Territory territory in TerritorySystem_DataTypes.SyncedTerritoriesData
                             .Value)
                {
                    if (territory.IsInside2D(new Vector2(vector.x, vector.z)))
                    {
                        string newText = search + "\n" + territory.GetName() + territory.GetTerritoryFlags() + search;
                        TryReplaceTerritoryString(__instance.m_biomeNameLarge, newText);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    TryReplaceTerritoryString(__instance.m_biomeNameLarge, "");
                }
            }
        }

        private static void TryReplaceTerritoryString(TMP_Text textElement, string to)
        {
            string text = textElement.text;
            int startIndex = text.IndexOf(search, StringComparison.Ordinal);
            if (startIndex > 0)
            {
                int endIndex = text.IndexOf(search, startIndex + search.Length, StringComparison.Ordinal);
                string match = text.Substring(startIndex, endIndex - startIndex + search.Length);
                text = text.Replace(match, to);
                textElement.text = text;
            }
            else
            {
                textElement.text += to;
            }
        }

        private static DateTime LastTimeTerritoryMessage;

        private static void ShowCustomTerritoryMessage(string rawName)
        {
            if ((DateTime.Now - LastTimeTerritoryMessage).TotalSeconds <= 5) return;
            LastTimeTerritoryMessage = DateTime.Now;
            GameObject Prefab = UnityEngine.Object.Instantiate(MessageHud.instance.m_biomeFoundPrefab,
                MessageHud.instance.transform);
            RectTransform Rect = Prefab.GetComponent<RectTransform>();
            Rect.anchorMin = new Vector2(0.5f, 1f);
            Rect.anchorMax = new Vector2(0.5f, 1f);
            Rect.anchoredPosition = new Vector2(0, -200f);
            TimedDestruction timed = Prefab.AddComponent<TimedDestruction>();
            Prefab.transform.GetChild(0).GetComponent<Animator>().speed = 2f;
            global::Utils.FindChild(Prefab.transform, "Title").GetComponent<Text>().text = rawName;
            timed.m_timeout = 2f;
            timed.Trigger();
        }
    }

    [HarmonyPatch(typeof(Minimap), nameof(Minimap.Awake))]
    [ClientOnlyPatch]
    private static class Minimap_Awake_Patch
    {
        private static void Postfix(Minimap __instance)
        {
            __instance.m_biomeNameLarge.alignment = TextAlignmentOptions.TopRight;
            __instance.m_biomeNameSmall.alignment = TextAlignmentOptions.TopRight;
        }
    }

    [HarmonyPatch(typeof(Minimap), nameof(Minimap.GenerateWorldMap))]
    [ClientOnlyPatch]
    private class PatchMinimapCircles
    {
        public static bool IsGeneratingMap;

        private static void Prefix()
        {
            IsGeneratingMap = true;
        }

        [HarmonyPriority(-3000)]
        private static void Postfix(Minimap __instance)
        {
            IsGeneratingMap = false;
            originalMapColors = __instance.m_mapTexture.GetPixels();
            originalHeightColors = __instance.m_heightTexture.GetPixels();
            DoMapMagic();
        }
    }

    private static int MapMagicCounter;

    private static async void DoMapMagic()
    {
        if (originalMapColors == null || TerritorySystem_DataTypes.SyncedTerritoriesData.Value == null ||
            TerritorySystem_DataTypes.SyncedTerritoriesData.Value.Count == 0) return;
        MapMagicCounter++;
        int currentCounter = MapMagicCounter;
        try
        {
            Color[] mapColors = new Color[originalMapColors.Length];
            Color[] heightColors = new Color[originalHeightColors.Length];
            int segments = Environment.ProcessorCount;
            await Task.Run(() =>
            {
                Array.Copy(originalMapColors, mapColors, originalMapColors.Length);
                Array.Copy(originalHeightColors, heightColors, originalHeightColors.Length);
                Parallel.ForEach(Enumerable.Range(0, segments), segment =>
                {
                    float pixelSize = Minimap.instance.m_pixelSize;
                    int textureSize = Minimap.instance.m_textureSize;
                    int num = textureSize / 2;
                    float num2 = pixelSize / 2;
                    int segmentSize = Mathf.RoundToInt(textureSize / (float)segments);
                    int segmentStart = segment * segmentSize;
                    int segmentEnd = segmentStart + segmentSize;
                    foreach (TerritorySystem_DataTypes.Territory territory in TerritorySystem_DataTypes.SyncedTerritoriesData
                                 .Value.Where(t => t.DrawOnMap).OrderBy(t => t.Priority))
                    {
                        Color32 MainColor = territory.GetColor();
                        Vector2 center = territory.Pos();
                        bool externalWater = territory.ShowExternalWater;
                        int y = Mathf.Clamp(Mathf.RoundToInt((territory.Type switch
                        {
                            TerritorySystem_DataTypes.TerritoryType.Custom => center.y,
                            _ => center.y - territory.Radius,
                        } + num2) / pixelSize + num), segmentStart, segmentEnd);
                        int endY = Mathf.Clamp(Mathf.RoundToInt((territory.Type switch
                        {
                            TerritorySystem_DataTypes.TerritoryType.Custom => center.y + territory.Ylength,
                            _ => center.y + territory.Radius,
                        } + num2) / pixelSize + num), segmentStart, segmentEnd);
                        for (; y < endY; y++)
                        {
                            int x, endX;
                            switch (territory.Type)
                            {
                                case TerritorySystem_DataTypes.TerritoryType.Custom:
                                    x = Mathf.RoundToInt((center.x + num2) / pixelSize + num);
                                    endX = Mathf.RoundToInt((center.x + territory.Xlength + num2) / pixelSize +
                                                            num);
                                    break;
                                case TerritorySystem_DataTypes.TerritoryType.Square:
                                    x = Mathf.RoundToInt((center.x - territory.Radius + num2) / pixelSize + num);
                                    endX = Mathf.RoundToInt((center.x + territory.Radius + num2) / pixelSize + num);
                                    break;
                                default:
                                    float halfWidth =
                                        Mathf.Cos(Mathf.Asin(((y - num) * pixelSize - num2 - center.y) /
                                                             territory.Radius)) * territory.Radius;
                                    x = Mathf.RoundToInt((center.x - halfWidth + num2) / pixelSize + num);
                                    endX = Mathf.RoundToInt((center.x + halfWidth + num2) / pixelSize + num);
                                    break;
                            }

                            for (; x < endX; x++)
                            {
                                int idx = y * textureSize + x;
                                if (territory.UsingGradient)
                                {
                                    mapColors[idx] = territory.GradientType switch
                                    {
                                        TerritorySystem_DataTypes.GradientType.FromCenter => territory
                                            .GetGradientFromCenter(new Vector2((x - 1024) * pixelSize,
                                                (y - 1024) * pixelSize)),
                                        TerritorySystem_DataTypes.GradientType.ToCenter => territory
                                            .GetGradientFromCenter(
                                                new Vector2((x - 1024) * pixelSize, (y - 1024) * pixelSize), true),

                                        TerritorySystem_DataTypes.GradientType.LeftRight => territory.GetGradientX(
                                            (x - 1024) * pixelSize),
                                        TerritorySystem_DataTypes.GradientType.RightLeft => territory.GetGradientX(
                                            (x - 1024) * pixelSize, true),

                                        TerritorySystem_DataTypes.GradientType.BottomTop => territory.GetGradientY(
                                            (y - 1024) * pixelSize),
                                        TerritorySystem_DataTypes.GradientType.TopBottom => territory.GetGradientY(
                                            (y - 1024) * pixelSize, true),

                                        TerritorySystem_DataTypes.GradientType.BottomLeftTopRight =>
                                            territory.GetGradientXY(new Vector2((x - 1024) * pixelSize,
                                                (y - 1024) * pixelSize)),
                                        TerritorySystem_DataTypes.GradientType.TopRightBottomLeft =>
                                            territory.GetGradientXY(
                                                new Vector2((x - 1024) * pixelSize, (y - 1024) * pixelSize), true),

                                        TerritorySystem_DataTypes.GradientType.BottomRightTopLeft =>
                                            territory.GetGradientXY_2(new Vector2((x - 1024) * pixelSize,
                                                (y - 1024) * pixelSize)),
                                        TerritorySystem_DataTypes.GradientType.TopLeftBottomRight =>
                                            territory.GetGradientXY_2(
                                                new Vector2((x - 1024) * pixelSize, (y - 1024) * pixelSize), true),

                                        _ => MainColor
                                    };
                                }
                                else
                                {
                                    mapColors[idx] = MainColor;
                                }

                                if (externalWater)
                                {
                                    heightColors[idx] = new Color(Mathf.Clamp(heightColors[idx].r, 29f, 89), 0, 0);
                                }
                            }
                        }
                    }
                });
            });
            if (currentCounter != MapMagicCounter) return;
            Minimap.instance.m_mapTexture.SetPixels(mapColors);
            Minimap.instance.m_mapTexture.Apply();
            Minimap.instance.m_heightTexture.SetPixels(heightColors);
            Minimap.instance.m_heightTexture.Apply();
        }
        catch (Exception ex)
        {
            Utils.print($"Error while drawing territories on map: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private static void DoAreaEffect(Vector3 pos)
    {
        if ((DateTime.Now - LastNoAccessMesssage).TotalSeconds <= 2) return;
        LastNoAccessMesssage = DateTime.Now;
        GameObject znet = ZNetScene.instance.GetPrefab("vfx_lootspawn");
        UnityEngine.Object.Instantiate(znet, pos, Quaternion.identity);
        DamageText.WorldTextInstance worldTextInstance = new DamageText.WorldTextInstance
        {
            m_worldPos = pos,
            m_gui = UnityEngine.Object.Instantiate(DamageText.instance.m_worldTextBase, DamageText.instance.transform)
        };
        worldTextInstance.m_textField = worldTextInstance.m_gui.GetComponent<Text>();
        DamageText.instance.m_worldTexts.Add(worldTextInstance);
        worldTextInstance.m_textField.color = Color.cyan;
        worldTextInstance.m_textField.fontSize = 24;
        worldTextInstance.m_textField.text = "NO ACCESS";
        worldTextInstance.m_timer = -2f;
    }


    [HarmonyPatch(typeof(Player), nameof(Player.PlacePiece))]
    [ClientOnlyPatch]
    private static class NoBuild_Patch
    {
        private static bool Prefix(Player __instance)
        {
            if (CurrentTerritory == null) return true;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoBuild) &&
                !CurrentTerritory.IsOwner())
            {
                DoAreaEffect(__instance.m_placementGhost.transform.position);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.Remove))]
    [ClientOnlyPatch]
    private static class WearNTear_Damage_Patch_Remove
    {
        private static bool Prefix(WearNTear __instance)
        {
            if (CurrentTerritory == null) return true;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoBuild) &&
                !CurrentTerritory.IsOwner())
            {
                DoAreaEffect(__instance.transform.position);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.Damage))]
    [ClientOnlyPatch]
    private static class WearNTear_Damage_Patch_Damage
    {
        private static bool Prefix()
        {
            if (CurrentTerritory == null) return true;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoBuildDamage))
            {
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Character), nameof(Character.Awake))]
    [ClientOnlyPatch]
    private static class Character_Awake_Patch
    {
        private static void Postfix(Character __instance)
        {
            __instance.m_nview.Register("KGtsLevelUpdate",
                new Action<long, int>((_, level) => { __instance.m_onLevelSet?.Invoke(level); }));
        }
    }

    [HarmonyPatch(typeof(Character), nameof(Character.Start))]
    [ClientOnlyPatch]
    private static class Character_Start_Patch
    {
        private static void Postfix(Character __instance)
        {
            if (CurrentTerritory == null || !__instance.IsOwner() || __instance.IsPlayer()) return;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.MonstersAddStars) &&
                !__instance.m_nview.m_zdo.GetBool("KGtsLevel"))
            {
                int level = __instance.GetLevel() + CurrentTerritory.AddMonsterLevel;
                __instance.m_nview.m_zdo.Set("KGtsLevel", true);
                __instance.SetLevel(level);
                __instance.m_nview.GetZDO().Set("level", level);
                __instance.SetupMaxHealth();
                __instance.m_nview.InvokeRPC(ZNetView.Everybody, "KGtsLevelUpdate", __instance.GetLevel());
            }
        }
    }

    [HarmonyPatch(typeof(Attack), nameof(Attack.SpawnOnHitTerrain))]
    [ClientOnlyPatch]
    private static class NoPickaxe_Patch
    {
        private static bool Prefix()
        {
            if (CurrentTerritory == null) return true;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoPickaxe) &&
                !CurrentTerritory.IsOwner())
            {
                DoAreaEffect(Player.m_localPlayer.transform.position);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.Interact))]
    [ClientOnlyPatch]
    private static class NoInteract_Patch
    {
        private static bool Prefix(GameObject go)
        {
            if (CurrentTerritory == null) return true;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoInteract) &&
                !CurrentTerritory.IsOwner())
            {
                DoAreaEffect(go.transform.position);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.Interact))]
    [ClientOnlyPatch]
    private static class NoInteractItems_Patch
    {
        private static bool Prefix()
        {
            if (CurrentTerritory == null) return true;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoInteractItems) &&
                !CurrentTerritory.IsOwner())
            {
                DoAreaEffect(Player.m_localPlayer.transform.position);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.Interact))]
    [ClientOnlyPatch]
    private static class NoInteractPortals_Patch
    {
        private static bool Prefix()
        {
            if (CurrentTerritory == null) return true;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoInteractPortals) &&
                !CurrentTerritory.IsOwner())
            {
                DoAreaEffect(Player.m_localPlayer.transform.position);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(CraftingStation), nameof(CraftingStation.Interact))]
    [ClientOnlyPatch]
    private static class NoInteractCraftingStation_Patch
    {
        private static bool Prefix()
        {
            if (CurrentTerritory == null) return true;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags
                    .NoInteractCraftingStation) &&
                !CurrentTerritory.IsOwner())
            {
                DoAreaEffect(Player.m_localPlayer.transform.position);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ItemStand), nameof(ItemStand.Interact))]
    [ClientOnlyPatch]
    private static class NoInteractItemStands_Patch
    {
        private static bool Prefix()
        {
            if (CurrentTerritory == null) return true;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoInteractItemStands) &&
                !CurrentTerritory.IsOwner())
            {
                DoAreaEffect(Player.m_localPlayer.transform.position);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ArmorStand), nameof(ArmorStand.UseItem))]
    [ClientOnlyPatch]
    private static class NoInteractItemStands2_Patch
    {
        private static bool Prefix()
        {
            if (CurrentTerritory == null) return true;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoInteractItemStands) &&
                !CurrentTerritory.IsOwner())
            {
                DoAreaEffect(Player.m_localPlayer.transform.position);
                return false;
            }

            return true;
        }
    }


    [HarmonyPatch(typeof(Container), nameof(Container.Interact))]
    [ClientOnlyPatch]
    private static class NoInteractChests_Patch
    {
        private static bool Prefix()
        {
            if (CurrentTerritory == null) return true;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoInteractChests) &&
                !CurrentTerritory.IsOwner())
            {
                DoAreaEffect(Player.m_localPlayer.transform.position);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Door), nameof(Door.Interact))]
    [ClientOnlyPatch]
    private static class NoInteractDoors_Patch
    {
        private static bool Prefix()
        {
            if (CurrentTerritory == null) return true;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoInteractDoors) &&
                !CurrentTerritory.IsOwner())
            {
                DoAreaEffect(Player.m_localPlayer.transform.position);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.UseStamina))]
    [ClientOnlyPatch]
    private static class InfiniteStamina_Patch
    {
        private static bool Prefix()
        {
            if (CurrentTerritory == null) return true;
            if (CurrentTerritory.AdditionalFlags.HasFlagFast(TerritorySystem_DataTypes.AdditionalTerritoryFlags
                    .InfiniteStamina))
            {
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.HaveSupport))]
    [ClientOnlyPatch]
    private static class NoStructureSupport_Patch
    {
        private static void Postfix(WearNTear __instance, ref bool __result)
        {
            foreach (TerritorySystem_DataTypes.Territory territory in TerritoriesByFlags[
                         TerritorySystem_DataTypes.TerritoryFlags.NoStructureSupport])
            {
                if (territory.IsInside(__instance.transform.position))
                {
                    __result = true;
                    return;
                }
            }
        }
    }


    [HarmonyPatch(typeof(Player), nameof(Player.HaveEitr))]
    [ClientOnlyPatch]
    private static class InfiniteEitr_Patch
    {
        private static void Postfix(ref bool __result)
        {
            if (CurrentTerritory == null) return;
            if (CurrentTerritory.AdditionalFlags.HasFlagFast(TerritorySystem_DataTypes.AdditionalTerritoryFlags
                    .InfiniteEitr))
            {
                __result = true;
            }
        }
    }


    [HarmonyPatch(typeof(Character), nameof(Character.Damage))]
    [ClientOnlyPatch]
    private static class IncreaseTerritoryDamagePatch
    {
        private static void Prefix(Character __instance, ref HitData hit)
        {
            if (CurrentTerritory == null) return;
            if (__instance.IsPlayer())
            {
                if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.IncreasedMonsterDamage))
                {
                    hit.ApplyModifier(CurrentTerritory.IncreasedMonsterDamageValue);
                }
            }
            else
            {
                if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.IncreasedPlayerDamage))
                {
                    hit.ApplyModifier(CurrentTerritory.IncreasedPlayerDamageValue);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Attack), nameof(Attack.Start))]
    [ClientOnlyPatch]
    private static class NoAttackPatch
    {
        private static bool Prefix(Humanoid character)
        {
            if (!Player.m_localPlayer || CurrentTerritory == null || character != Player.m_localPlayer)
            {
                return true;
            }

            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoAttack) &&
                !CurrentTerritory.IsOwner())
            {
                DoAreaEffect(Player.m_localPlayer.transform.position);
                return false;
            }

            return true;
        }
    }


    [HarmonyPatch(typeof(CreatureSpawner), nameof(CreatureSpawner.Spawn))]
    [ClientOnlyPatch]
    private static class NoMonsters_Start_Patch
    {
        private static bool Prefix(CreatureSpawner __instance)
        {
            foreach (TerritorySystem_DataTypes.Territory territory in TerritoriesByFlags[
                         TerritorySystem_DataTypes.TerritoryFlags.NoMonsters])
            {
                if (territory.IsInside(__instance.transform.position))
                    return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(SpawnSystem), nameof(SpawnSystem.UpdateSpawning))]
    [ClientOnlyPatch]
    private static class NoMonsters_Start_Patch2
    {
        private static bool Prefix()
        {
            if (CurrentTerritory == null) return true;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoMonsters))
            {
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(SpawnSystem), nameof(SpawnSystem.Spawn))]
    [ClientOnlyPatch]
    private static class SpawnSystem_Spawn_Patch
    {
        private static bool Prefix(Vector3 spawnPoint)
        {
            foreach (TerritorySystem_DataTypes.Territory territory in TerritoriesByFlags[
                         TerritorySystem_DataTypes.TerritoryFlags.NoMonsters])
            {
                if (territory.IsInside(spawnPoint))
                    return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.GetRunSpeedFactor))]
    [ClientOnlyPatch]
    private static class MoveSpeed_Patch_Territory
    {
        private static void Postfix(ref float __result)
        {
            if (CurrentTerritory == null) return;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.MoveSpeedMultiplier))
            {
                __result *= CurrentTerritory.MoveSpeedMultiplier;
            }
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.GetJogSpeedFactor))]
    [ClientOnlyPatch]
    private static class MoveSpeed2_Patch_Territory
    {
        private static void Postfix(ref float __result)
        {
            if (CurrentTerritory == null) return;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.MoveSpeedMultiplier))
            {
                __result *= CurrentTerritory.MoveSpeedMultiplier;
            }
        }
    }

    [HarmonyPatch(typeof(Skills), nameof(Skills.OnDeath))]
    [ClientOnlyPatch]
    private static class Skills_OnDeath_Patch
    {
        private static bool Prefix()
        {
            if (CurrentTerritory == null) return true;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoDeathPenalty))
            {
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.AutoPickup))]
    [ClientOnlyPatch]
    private static class Player_AutoPickup_Patch
    {
        private static bool Prefix()
        {
            if (CurrentTerritory == null) return true;
            if ((CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoInteractItems) ||
                 CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoInteract)) &&
                !CurrentTerritory.IsOwner())
            {
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.TeleportTo))]
    [ClientOnlyPatch]
    private static class NoPortals_Patch
    {
        private static bool Prefix(Vector3 pos)
        {
            if (Teleporter_Main_Client.DEBUG_TELEPORTTO_TERRITORY)
            {
                Teleporter_Main_Client.DEBUG_TELEPORTTO_TERRITORY = false;
                return true;
            }

            foreach (TerritorySystem_DataTypes.Territory noportals in TerritoriesByFlags[
                         TerritorySystem_DataTypes.TerritoryFlags.NoPortals])
            {
                if (noportals.IsOwner() || (!noportals.IsInside(Player.m_localPlayer.transform.position) &&
                                            !noportals.IsInside(pos))) continue;
                DoAreaEffect(Player.m_localPlayer.transform.position + Vector3.up);
                MessageHud.instance.ShowMessage(MessageHud.MessageType.TopLeft,
                    Localization.instance.Localize("$mpasn_cantteleport"));
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetBiomeHeight))]
    [ClientOnlyPatch]
    private static class WorldGenerator_GetBiomeHeight_Patch
    {
        private static void Postfix(float wx, float wy, ref float __result)
        {
            if (PatchMinimapCircles.IsGeneratingMap) return;
            Vector2 vec = new Vector2(wx, wy);

            foreach (TerritorySystem_DataTypes.Territory territory in TerritoriesByFlags[
                         TerritorySystem_DataTypes.TerritoryFlags.ForceGroundHeight])
            {
                if (territory.IsInside2D(vec))
                {
                    __result = territory.OverridenHeight;
                    //break;
                }
            }

            foreach (TerritorySystem_DataTypes.Territory territory in TerritoriesByFlags[
                         TerritorySystem_DataTypes.TerritoryFlags.AddGroundHeight])
            {
                if (territory.IsInside2D(vec))
                {
                    __result += territory.OverridenHeight;
                    //break;
                }
            }

            foreach (TerritorySystem_DataTypes.Territory territory in TerritoriesByFlags[
                         TerritorySystem_DataTypes.TerritoryFlags.LimitZoneHeight])
            {
                if (territory.IsInside2D(vec))
                {
                    __result = Mathf.Max(territory.OverridenHeight, __result);
                    //break;
                }
            }
        }
    }

    [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetBiome), typeof(float), typeof(float))]
    [ClientOnlyPatch]
    private static class WorldGenerator_GetBiome_Patch
    {
        private static void Postfix(float wx, float wy, ref Heightmap.Biome __result)
        {
            if (PatchMinimapCircles.IsGeneratingMap) return;
            Vector2 vec = new Vector2(wx, wy);
            foreach (TerritorySystem_DataTypes.Territory ground in TerritoriesByFlags[
                         TerritorySystem_DataTypes.TerritoryFlags.ForceBiome])
            {
                if (ground.IsInside2D(vec))
                {
                    __result = (Heightmap.Biome)ground.OverridenBiome;
                    break;
                }
            }
        }
    }

    [HarmonyPatch(typeof(CharacterDrop), nameof(CharacterDrop.DropItems))]
    [ClientOnlyPatch]
    private static class CharacterDrop_DropItems_Patch
    {
        private static bool Prefix()
        {
            return CurrentTerritory == null ||
                   !CurrentTerritory.AdditionalFlags.HasFlagFast(TerritorySystem_DataTypes.AdditionalTerritoryFlags
                       .NoCreatureDrops);
        }
    }


    [HarmonyPatch(typeof(Fireplace), nameof(Fireplace.UpdateFireplace))]
    [ClientOnlyPatch]
    private static class UnlimitedFuel1
    {
        private static void Postfix(Fireplace __instance)
        {
            if (CurrentTerritory == null || !__instance.m_nview.IsValid() || !__instance.m_nview.IsOwner() ||
                !CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.InfiniteFuel)) return;
            __instance.m_nview.m_zdo.Set("fuel", __instance.m_maxFuel);
        }
    }


    [HarmonyPatch(typeof(Smelter), nameof(Smelter.UpdateSmelter))]
    [ClientOnlyPatch]
    private static class UnlimitedFuel2
    {
        private static void Postfix(Smelter __instance)
        {
            if (CurrentTerritory == null || !__instance.m_nview.IsValid() || !__instance.m_nview.IsOwner() ||
                !CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.InfiniteFuel)) return;
            __instance.m_nview.m_zdo.Set("fuel", __instance.m_maxFuel);
        }
    }

    [HarmonyPatch(typeof(CookingStation), nameof(CookingStation.UpdateCooking))]
    [ClientOnlyPatch]
    private static class UnlimitedFuel3
    {
        private static void Postfix(CookingStation __instance)
        {
            if (CurrentTerritory == null || !__instance.m_nview.IsValid() || !__instance.m_nview.IsOwner() ||
                !CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.InfiniteFuel)) return;
            __instance.m_nview.m_zdo.Set("fuel", __instance.m_maxFuel);
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.CreateTombStone))]
    [ClientOnlyPatch]
    private static class Player_CreateTombStone_Patch
    {
        private static bool Prefix()
        {
            return CurrentTerritory == null ||
                   !CurrentTerritory.AdditionalFlags.HasFlagFast(TerritorySystem_DataTypes.AdditionalTerritoryFlags
                       .NoItemLoss);
        }
    }

    [HarmonyPatch(typeof(Heightmap), nameof(Heightmap.ApplyModifiers))]
    [ClientOnlyPatch]
    private static class TerrainComp_ApplyToHeightmap_Patch
    {
        private static void Postfix(Heightmap __instance)
        {
            if (__instance.m_isDistantLod ||
                TerritoriesByFlags[TerritorySystem_DataTypes.TerritoryFlags.CustomPaint].Count == 0)
            {
                return;
            }

            Vector3 vector = __instance.transform.position -
                             new Vector3(__instance.m_width * __instance.m_scale * 0.5f, 0f,
                                 __instance.m_width * __instance.m_scale * 0.5f);
            bool invoke = false;
            for (int x = 0; x < __instance.m_width; ++x)
            {
                for (int z = 0; z < __instance.m_width; ++z)
                {
                    float FinalX = vector.x + x;
                    float FinalZ = vector.z + z;
                    foreach (TerritorySystem_DataTypes.Territory paint in TerritoriesByFlags[
                                 TerritorySystem_DataTypes.TerritoryFlags.CustomPaint])
                    {
                        if (!paint.IsInside2D(new Vector2(FinalX, FinalZ))) continue;
                        Color c = paint.PaintGround switch
                        {
                            TerritorySystem_DataTypes.PaintType.Paved => Color.blue,
                            TerritorySystem_DataTypes.PaintType.Grass => Color.black,
                            TerritorySystem_DataTypes.PaintType.Cultivated => Color.green,
                            TerritorySystem_DataTypes.PaintType.Dirt => Color.red,
                            _ => Color.black
                        };
                        invoke = true;
                        __instance.m_paintMask.SetPixel(x, z, c);
                        break;
                    }
                }
            }

            if (invoke)
            {
                __instance.m_paintMask.Apply();
            }
        }
    }


    [HarmonyPatch(typeof(Heightmap), nameof(Heightmap.RebuildRenderMesh))]
    [ClientOnlyPatch]
    private static class TerrainComp_ApplyToHeightmap_Patch_SnowMask
    {
        private static void Postfix(Heightmap __instance)
        {
            if (TerritoriesByFlags_Additional[TerritorySystem_DataTypes.AdditionalTerritoryFlags.SnowMask].Count == 0)
            {
                return;
            }

            Vector3 vector = __instance.transform.position - new Vector3(__instance.m_width * __instance.m_scale * 0.5f,
                0f, __instance.m_width * __instance.m_scale * 0.5f);
            int num = __instance.m_width + 1;
            bool invoke = false;
            Heightmap.s_tempColors.Clear();
            for (int x = 0; x < num; ++x)
            {
                for (int z = 0; z < num; ++z)
                {
                    float FinalX = vector.x + z;
                    float FinalZ = vector.z + x;
                    foreach (TerritorySystem_DataTypes.Territory paint in TerritoriesByFlags_Additional[
                                 TerritorySystem_DataTypes.AdditionalTerritoryFlags.SnowMask])
                    {
                        if (!paint.IsInside2D(new Vector2(FinalX, FinalZ)))
                        {
                            Heightmap.Biome biome = WorldGenerator.instance.GetBiome(FinalX, FinalZ);
                            Heightmap.s_tempColors.Add(Heightmap.GetBiomeColor(biome));
                        }
                        else
                        {
                            Heightmap.s_tempColors.Add(Heightmap.GetBiomeColor(Heightmap.Biome.Mountain));
                            invoke = true;
                        }

                        break;
                    }
                }
            }

            if (invoke)
            {
                __instance.m_renderMesh.SetColors(Heightmap.s_tempColors);
            }
        }
    }
}
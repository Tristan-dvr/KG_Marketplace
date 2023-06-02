namespace Marketplace.Modules.TerritorySystem;

public static class TerritorySystem_DataTypes
{
    internal static readonly CustomSyncedValue<List<Territory>> TerritoriesData =
        new(Marketplace.configSync, "territoryData", new List<Territory>());

    public static readonly TerritoryFlags[] AllTerritoryFlagsArray =
        (TerritoryFlags[])Enum.GetValues(typeof(TerritoryFlags));

    public static readonly AdditionalTerritoryFlags[] AllAdditionaTerritoryFlagsArray =
        (AdditionalTerritoryFlags[])Enum.GetValues(typeof(AdditionalTerritoryFlags));

    //serialization part

    public partial class Territory : ISerializableParameter
    {
        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write((int)Type);
            pkg.Write((int)Flags);
            pkg.Write((int)AdditionalFlags);
            pkg.Write(X);
            pkg.Write(Y);
            pkg.Write(Xlength);
            pkg.Write(Ylength);
            pkg.Write(Name ?? "");
            pkg.Write(R);
            pkg.Write(G);
            pkg.Write(B);
            pkg.Write(Radius);
            pkg.Write(ShowExternalWater);
            pkg.Write(Priority);
            pkg.Write(PeriodicHealValue);
            pkg.Write(PeriodicDamageValue);
            pkg.Write(IncreasedPlayerDamageValue);
            pkg.Write(IncreasedMonsterDamageValue);
            pkg.Write(CustomEnvironment ?? "");
            pkg.Write(MoveSpeedMultiplier);
            pkg.Write(Owners ?? "");
            pkg.Write(OverridenHeight);
            pkg.Write(OverridenBiome);
            pkg.Write(AddMonsterLevel);
            pkg.Write((int)PaintGround);
            pkg.Write(DrawOnMap);
            pkg.Write(R_End);
            pkg.Write(G_End);
            pkg.Write(B_End);
            pkg.Write(UsingGradient);
            pkg.Write((int)GradientType);
        }

        public void Deserialize(ref ZPackage pkg)
        {
            Type = (TerritoryType)pkg.ReadInt();
            Flags = (TerritoryFlags)pkg.ReadInt();
            AdditionalFlags = (AdditionalTerritoryFlags)pkg.ReadInt();
            X = pkg.ReadInt();
            Y = pkg.ReadInt();
            Xlength = pkg.ReadInt();
            Ylength = pkg.ReadInt();
            Name = pkg.ReadString();
            R = pkg.ReadInt();
            G = pkg.ReadInt();
            B = pkg.ReadInt();
            Radius = pkg.ReadInt();
            ShowExternalWater = pkg.ReadBool();
            Priority = pkg.ReadInt();
            PeriodicHealValue = pkg.ReadSingle();
            PeriodicDamageValue = pkg.ReadSingle();
            IncreasedPlayerDamageValue = pkg.ReadSingle();
            IncreasedMonsterDamageValue = pkg.ReadSingle();
            CustomEnvironment = pkg.ReadString();
            MoveSpeedMultiplier = pkg.ReadSingle();
            Owners = pkg.ReadString();
            OverridenHeight = pkg.ReadSingle();
            OverridenBiome = pkg.ReadInt();
            AddMonsterLevel = pkg.ReadInt();
            PaintGround = (PaintType)pkg.ReadInt();
            DrawOnMap = pkg.ReadBool();
            R_End = pkg.ReadInt();
            G_End = pkg.ReadInt();
            B_End = pkg.ReadInt();
            UsingGradient = pkg.ReadBool();
            GradientType = (GradientType)pkg.ReadInt();
        }
    }

    //main part
    public partial class Territory
    {
        public static Territory LastTerritory;
        public TerritoryType Type;
        public TerritoryFlags Flags;
        public AdditionalTerritoryFlags AdditionalFlags;
        public int X;
        public int Y;
        public int Xlength;
        public int Ylength;
        public string Name = "";
        public int R;
        public int G;
        public int B;
        public int R_End = -1;
        public int G_End = -1;
        public int B_End = -1;
        public bool UsingGradient;
        public int Radius;
        public bool ShowExternalWater = true;
        public int Priority;
        public float PeriodicHealValue;
        public float PeriodicDamageValue;
        public float IncreasedPlayerDamageValue;
        public float IncreasedMonsterDamageValue;
        public string CustomEnvironment = "";
        public float MoveSpeedMultiplier;
        public string Owners = "";
        public float OverridenHeight;
        public int OverridenBiome;
        public int AddMonsterLevel;
        public PaintType PaintGround;
        public bool DrawOnMap;
        public GradientType GradientType = GradientType.FromCenter;


        public static Territory GetCurrentTerritory(Vector3 pos)
        {
            foreach (Territory territory in TerritoriesData.Value)
            {
                if (territory.IsInside(pos))
                {
                    return territory;
                }
            }

            return null;
        }


        public Vector2 Pos()
        {
            return new Vector2(X, Y);
        }

        public Vector3 Pos3D()
        {
            return new Vector3(X, 0, Y);
        }

        public Color32 GetColor()
        {
            return new Color32((byte)R, (byte)G, (byte)B, 255);
        }

        private Color32 CalculateGradient(float t)
        {
            Color32 start = GetColor();
            Color32 end = new Color32((byte)R_End, (byte)G_End, (byte)B_End, 255);
            return Color32.Lerp(start, end, t);
        }

        public Color32 GetGradientX(float x, bool reverse = false)
        {
            float startX = Pos().x - Radius;
            float endX = Pos().x + Radius;
            float t = Mathf.Clamp01((x - startX) / (endX - startX));
            if (reverse) t = 1 - t;
            return CalculateGradient(t);
        }

        public Color32 GetGradientY(float y, bool reverse = false)
        {
            float startY = Pos().y - Radius;
            float endY = Pos().y + Radius;
            float t = Mathf.Clamp01((y - startY) / (endY - startY));
            if (reverse) t = 1 - t;
            return CalculateGradient(t);
        }

        public Color32 GetGradientXY(Vector2 pos, bool reverse = false)
        {
            float startX = Pos().x - Radius;
            float endX = Pos().x + Radius;
            float startY = Pos().y - Radius;
            float endY = Pos().y + Radius;
            float tX = Mathf.Clamp01((pos.x - startX) / (endX - startX));
            float tY = Mathf.Clamp01((pos.y - startY) / (endY - startY));
            float t = (tX + tY) / 2;
            if (reverse) t = 1 - t;
            return CalculateGradient(t);
        }

        public Color32 GetGradientXY_2(Vector2 pos, bool reverse = false)
        {
            float startX = Pos().x + Radius;
            float endX = Pos().x - Radius;
            float startY = Pos().y - Radius;
            float endY = Pos().y + Radius;
            float tX = Mathf.Clamp01((pos.x - startX) / (endX - startX));
            float tY = Mathf.Clamp01((pos.y - startY) / (endY - startY));
            float t = (tX + tY) / 2;
            if (reverse) t = 1 - t;
            return CalculateGradient(t);
        }

        public Color32 GetGradientFromCenter(Vector2 pos, bool reverse = false)
        {
            float t = Mathf.Clamp01(Vector2.Distance(pos, Pos()) / Radius);
            if (reverse) t = 1 - t;
            return CalculateGradient(t);
        }


        public string RawName()
        {
            return Name;
        }

        public string GetName()
        {
            return $"{Name}\n{(IsOwner() ? "<color=#00ff00>Permitted</color>\n" : "")}";
        }

        private bool? IsOwnerCached;

        public bool IsOwner()
        {
            if (Player.m_debugMode) return true;
            IsOwnerCached ??= Owners.Contains(Global_Values._localUserID) || Owners.Contains("ALL");
            return IsOwnerCached.Value;
        }

        public bool IsInside(Vector2 mouse)
        {
            Vector2 p = Pos();
            switch (Type)
            {
                case TerritoryType.Square:
                    return mouse.x >= p.x - Radius && mouse.x <= p.x + Radius && mouse.y >= p.y - Radius &&
                           mouse.y <= p.y + Radius;
                case TerritoryType.Circle:
                    return Vector2.Distance(p, mouse) <= Radius;
                case TerritoryType.Custom:
                    return mouse.x >= p.x && mouse.x <= p.x + Xlength && mouse.y >= p.y && mouse.y <= p.y + Ylength;
                default:
                    return Vector2.Distance(p, mouse) <= Radius;
            }
        }

        public bool IsInside(Vector3 init)
        {
            Vector2 mouse = new Vector2(init.x, init.z);
            Vector2 p = Pos();
            switch (Type)
            {
                case TerritoryType.Square:
                    return mouse.x >= p.x - Radius && mouse.x <= p.x + Radius && mouse.y >= p.y - Radius &&
                           mouse.y <= p.y + Radius;
                case TerritoryType.Circle:
                    return Vector2.Distance(p, mouse) <= Radius;
                case TerritoryType.Custom:
                    return mouse.x >= p.x && mouse.x <= p.x + Xlength && mouse.y >= p.y && mouse.y <= p.y + Ylength;
                default:
                    return Vector2.Distance(p, mouse) <= Radius;
            }
        }

        public override string ToString()
        {
            return
                $"Name: {Name}, Type: {Type}, Flags: {Flags}, values:\nHeal: {PeriodicHealValue}, Damage: {PeriodicDamageValue}, MoveSpeed: {MoveSpeedMultiplier}, PlayerDamage: {IncreasedPlayerDamageValue}, MonsterDamage: {IncreasedMonsterDamageValue}, CustomEnvironment: {CustomEnvironment}\nOwners: {Owners}";
        }

        public string GetTerritoryFlags()
        {
            string ret = "";
            foreach (TerritoryFlags flag in AllTerritoryFlagsArray)
            {
                if (!Flags.HasFlagFast(flag)) continue;
                if (LocalizedTerritoryFlags.TryGetValue(flag, out string territoryFlag))
                    ret += Localization.instance.Localize(territoryFlag);
            }

            foreach (AdditionalTerritoryFlags flag in AllAdditionaTerritoryFlagsArray)
            {
                if (!AdditionalFlags.HasFlagFast(flag)) continue;
                if (LocalizedAdditionalTerritoryFlags.TryGetValue(flag, out string territoryFlag))
                    ret += Localization.instance.Localize(territoryFlag);
            }

            return ret;
        }
    }


    public enum TerritoryType
    {
        Circle,
        Square,
        Custom
    }

    public enum GradientType
    {
        FromCenter,
        ToCenter,
        LeftRight,
        RightLeft,
        TopBottom,
        BottomTop,
        BottomRightTopLeft,
        TopRightBottomLeft,
        BottomLeftTopRight,
        TopLeftBottomRight
    }

    public enum PaintType
    {
        Paved,
        Grass,
        Cultivated,
        Dirt
    }

    private static readonly Dictionary<TerritoryFlags, string> LocalizedTerritoryFlags = new()
    {
        { TerritoryFlags.NoBuildDamage, "\n<color=#00FFFF>$mpasn_nobuilddamage</color>" },
        { TerritoryFlags.PushAway, "\n<color=#00FFFF>$mpasn_pushaway</color>" },
        { TerritoryFlags.NoBuild, "\n<color=#00FFFF>$mpasn_cantbuild</color>" },
        { TerritoryFlags.NoPickaxe, "\n<color=#00FFFF>$mpasn_cantusepickaxe</color>" },
        { TerritoryFlags.NoInteract, "\n<color=#00FFFF>$mpasn_nointeractions</color>" },
        { TerritoryFlags.NoAttack, "\n<color=#00FFFF>$mpasn_cantattack</color>" },
        { TerritoryFlags.PvpOnly, "\n<color=#00FFFF>$mpasn_pvponly</color>" },
        { TerritoryFlags.PveOnly, "\n<color=#00FFFF>$mpasn_pveonly</color>" },
        { TerritoryFlags.PeriodicHealALL, "\n<color=#00FFFF>$mpasn_periodichealALL</color>" },
        { TerritoryFlags.PeriodicHeal, "\n<color=#00FFFF>$mpasn_periodicheal</color>" },
        { TerritoryFlags.PeriodicDamage, "\n<color=#00FFFF>$mpasn_periodicdamage</color>" },
        { TerritoryFlags.IncreasedPlayerDamage, "\n<color=#00FFFF>$mpasn_increasedamagePlayer</color>" },
        { TerritoryFlags.IncreasedMonsterDamage, "\n<color=#00FFFF>$mpasn_increasedamageMonsters</color>" },
        { TerritoryFlags.NoMonsters, "\n<color=#00FFFF>$mpasn_nomonsters</color>" },
        { TerritoryFlags.MoveSpeedMultiplier, "\n<color=#00FFFF>$mpasn_movementspeedmultiplier</color>" },
        { TerritoryFlags.NoDeathPenalty, "\n<color=#00FFFF>$mpasn_nodeathpenalty</color>" },
        { TerritoryFlags.NoPortals, "\n<color=#00FFFF>$mpasn_noportals</color>" },
        { TerritoryFlags.InfiniteFuel, "\n<color=#00FFFF>$mpasn_infinitefuel</color>" },
        { TerritoryFlags.NoInteractItems, "\n<color=#00FFFF>$mpasn_nointeractitems</color>" },
        { TerritoryFlags.NoInteractCraftingStation, "\n<color=#00FFFF>$mpasn_nointeractcraftingstations</color>" },
        { TerritoryFlags.NoInteractItemStands, "\n<color=#00FFFF>$mpasn_nointeractitemstands</color>" },
        { TerritoryFlags.NoInteractChests, "\n<color=#00FFFF>$mpasn_nointeractchests</color>" },
        { TerritoryFlags.NoInteractDoors, "\n<color=#00FFFF>$mpasn_nointeractdoors</color>" },
        { TerritoryFlags.NoStructureSupport, "\n<color=#00FFFF>$mpasn_nostructuresupport</color>" },
        { TerritoryFlags.NoInteractPortals, "\n<color=#00FFFF>$mpasn_nointeractportals</color>" },
    };

    private static readonly Dictionary<AdditionalTerritoryFlags, string> LocalizedAdditionalTerritoryFlags = new()
    {
        { AdditionalTerritoryFlags.NoItemLoss, "\n<color=#00FFFF>$mpasn_noitemloss</color>" },
        { AdditionalTerritoryFlags.InfiniteEitr, "\n<color=#00FFFF>$mpasn_infiniteeitr</color>" },
        { AdditionalTerritoryFlags.InfiniteStamina, "\n<color=#00FFFF>$mpasn_infinitestamina</color>" },
        { AdditionalTerritoryFlags.NoMist, "\n<color=#00FFFF>$mpasn_nomistlandsmist</color>" },
        { AdditionalTerritoryFlags.NoCreatureDrops, "\n<color=#00FFFF>$mpasn_nocreaturedrops</color>" },
    };


    [Flags]
    public enum TerritoryFlags
    {
        None = 0,
        PushAway = 1 << 0,
        NoBuild = 1 << 1,
        NoPickaxe = 1 << 2,
        NoInteract = 1 << 3,
        NoAttack = 1 << 4,
        PvpOnly = 1 << 5,
        PveOnly = 1 << 6,
        PeriodicHeal = 1 << 7,
        PeriodicDamage = 1 << 8,
        IncreasedPlayerDamage = 1 << 9,
        IncreasedMonsterDamage = 1 << 10,
        NoMonsters = 1 << 11,
        CustomEnvironment = 1 << 12,
        MoveSpeedMultiplier = 1 << 13,
        NoDeathPenalty = 1 << 14,
        NoPortals = 1 << 15,
        PeriodicHealALL = 1 << 16,
        ForceGroundHeight = 1 << 17,
        ForceBiome = 1 << 18,
        AddGroundHeight = 1 << 19,
        NoBuildDamage = 1 << 20,
        MonstersAddStars = 1 << 21,
        InfiniteFuel = 1 << 22,
        NoInteractItems = 1 << 23,
        NoInteractCraftingStation = 1 << 24,
        NoInteractItemStands = 1 << 25,
        NoInteractChests = 1 << 26,
        NoInteractDoors = 1 << 27,
        NoStructureSupport = 1 << 28,
        NoInteractPortals = 1 << 29,
        CustomPaint = 1 << 30,
        LimitZoneHeight = 1 << 31,
    }

    [Flags]
    public enum AdditionalTerritoryFlags
    {
        None = 0,
        NoItemLoss = 1 << 0,
        SnowMask = 1 << 1,
        NoMist = 1 << 2,
        InfiniteEitr = 1 << 3,
        InfiniteStamina = 1 << 4,
        NoCreatureDrops = 1 << 5,
    }
}
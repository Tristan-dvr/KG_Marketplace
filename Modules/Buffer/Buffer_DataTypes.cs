namespace Marketplace.Modules.Buffer;

public static class Buffer_DataTypes
{
    internal static readonly CustomSyncedValue<Dictionary<string, string>> BufferProfiles =
        new(Marketplace.configSync, "bufferProfiles", new Dictionary<string, string>());

    internal static readonly CustomSyncedValue<List<BufferBuffData>> BufferBuffs =
        new(Marketplace.configSync, "bufferBuffs", new List<BufferBuffData>());
    
    public static readonly Dictionary<string, List<BufferBuffData>> ClientSideBufferProfiles = new();

    public static readonly WhatToModify[] BufferModifyList = (WhatToModify[])Enum.GetValues(typeof(WhatToModify));

    [Flags]
    public enum WhatToModify
    {
        None = 0,
        ModifyAttack = 1 << 0,
        ModifyHealthRegen = 1 << 1,
        ModifyStaminaRegen = 1 << 2,
        ModifyRaiseSkills = 1 << 3,
        ModifySpeed = 1 << 4,
        ModifyNoise = 1 << 5,
        ModifyMaxCarryWeight = 1 << 6,
        ModifyStealth = 1 << 7,
        RunStaminaDrain = 1 << 8,
        DamageReduction = 1 << 9
    }

    public class BufferBuffData : ISerializableParameter
    {
        public string UniqueName;
        public string Name;
        public int Duration;
        public float ModifyAttack = 1f;
        public float ModifyHealthRegen = 1f;
        public float ModifyStaminaRegen = 1f;
        public float ModifyRaiseSkills = 1f;
        public float MofidySpeed = 1f;
        public float ModifyNoise = 1f;
        public float ModifyMaxCarryWeight;
        public float MofidyStealth = 1f;
        public float RunStaminaDrain = 1f;
        public float ModifyJumpStaminaUsage = 1f;
        public float DamageReduction;

        public string NeededPrefab;
        public int NeededPrefabCount;
        public WhatToModify Flags;
        public string SpritePrefab;
        public string StartEffectPrefab;
        public string BuffGroup;

        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write(UniqueName ?? "");
            pkg.Write(Name ?? "");
            pkg.Write(Duration);
            pkg.Write(ModifyAttack);
            pkg.Write(ModifyHealthRegen);
            pkg.Write(ModifyStaminaRegen);
            pkg.Write(ModifyRaiseSkills);
            pkg.Write(MofidySpeed);
            pkg.Write(ModifyNoise);
            pkg.Write(ModifyMaxCarryWeight);
            pkg.Write(MofidyStealth);
            pkg.Write(RunStaminaDrain);
            pkg.Write(ModifyJumpStaminaUsage);
            pkg.Write(DamageReduction);
            pkg.Write(NeededPrefab ?? "");
            pkg.Write(NeededPrefabCount);
            pkg.Write((int)Flags);
            pkg.Write(SpritePrefab ?? "");
            pkg.Write(StartEffectPrefab ?? "");
            pkg.Write(BuffGroup ?? "");
        }

        public void Deserialize(ref ZPackage pkg)
        {
            UniqueName = pkg.ReadString();
            Name = pkg.ReadString();
            Duration = pkg.ReadInt();
            ModifyAttack = pkg.ReadSingle();
            ModifyHealthRegen = pkg.ReadSingle();
            ModifyStaminaRegen = pkg.ReadSingle();
            ModifyRaiseSkills = pkg.ReadSingle();
            MofidySpeed = pkg.ReadSingle();
            ModifyNoise = pkg.ReadSingle();
            ModifyMaxCarryWeight = pkg.ReadSingle();
            MofidyStealth = pkg.ReadSingle();
            RunStaminaDrain = pkg.ReadSingle();
            ModifyJumpStaminaUsage = pkg.ReadSingle();
            DamageReduction = pkg.ReadSingle();
            NeededPrefab = pkg.ReadString();
            NeededPrefabCount = pkg.ReadInt();
            Flags = (WhatToModify)pkg.ReadInt();
            SpritePrefab = pkg.ReadString();
            StartEffectPrefab = pkg.ReadString();
            BuffGroup = pkg.ReadString();
        }

        private BufferMain Main;

        public BufferMain GetMain() => Main;
        public string GetItemName() => NeededItemName;
        private string NeededItemName;
        private Sprite Icon;
        private Sprite NeededItemIcon;
        private GameObject StartEffect;

        public bool? IsValid = null;

        public void Init()
        {
            if (IsValid.HasValue) return;
            IsValid = false;
            GameObject obj = ZNetScene.instance.GetPrefab(SpritePrefab);
            GameObject neededItemPrefab = ZNetScene.instance.GetPrefab(NeededPrefab);
            if (!neededItemPrefab)
                return;

            NeededItemName = neededItemPrefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_name;
            Icon = AssetStorage.AssetStorage.PlaceholderGamblerIcon;
            if (obj)
            {
                if (obj.GetComponent<Character>())
                {
                    PhotoManager.__instance.MakeSprite(obj, 0.6f, 0.25f);
                    Icon = PhotoManager.__instance.GetSprite(obj.name, AssetStorage.AssetStorage.PlaceholderMonsterIcon,
                        1);
                }
                else if (obj.GetComponent<ItemDrop>())
                {
                    Icon = obj.GetComponent<ItemDrop>().m_itemData.GetIcon();
                }
            }
            else
            {
                if (AssetStorage.AssetStorage.GlobalCachedSprites.TryGetValue(SpritePrefab, out Sprite sprite))
                {
                    Icon = sprite;
                }
            }


            NeededItemIcon = neededItemPrefab.GetComponent<ItemDrop>().m_itemData.GetIcon();

            StartEffect = ZNetScene.instance.GetPrefab(StartEffectPrefab);

            ObjectDB odb = ObjectDB.instance;
            if (!odb.m_StatusEffects.Find(se => se.name == UniqueName))
            {
                Main = ScriptableObject.CreateInstance<BufferMain>();
                Main.name = UniqueName;
                Main.Data = this;
                Main.m_icon = Icon;
                Main.m_name = Name;
                Main.m_ttl = Duration;
                Main.m_tooltip = Buffer_UI.GetFlagsText(this);
                if (StartEffect)
                    Main.m_startEffects = new EffectList
                    {
                        m_effectPrefabs = new[]
                        {
                            new EffectList.EffectData()
                            {
                                m_attach = true, m_enabled = true, m_inheritParentRotation = true,
                                m_inheritParentScale = true,
                                m_prefab = StartEffect, m_randomRotation = false, m_scale = true
                            }
                        }
                    };
                odb.m_StatusEffects.Add(Main);
                IsValid = true;
            }
        }

        public Sprite GetIcon() => Icon;
        public Sprite GetIconNeeded() => NeededItemIcon;

        public bool CanTake()
        {
            return Player.m_localPlayer && !string.IsNullOrEmpty(BuffGroup) && !Player.m_localPlayer.GetSEMan()
                .m_statusEffects
                .Any(s => s is BufferMain main && main.Data.BuffGroup == BuffGroup) && EnoughMaterials();
        }

        private bool EnoughMaterials()
        {
            if (!Player.m_localPlayer) return false;
            return Player.m_localPlayer.m_inventory.CountItems(NeededItemName) >= NeededPrefabCount;
        }
    }

    public class BufferMain : StatusEffect
    {
        public BufferBuffData Data;

        public override void ModifyAttack(Skills.SkillType skill, ref HitData hitData)
        {
            hitData.ApplyModifier(Data.ModifyAttack);
        }

        public override void ModifyHealthRegen(ref float regenMultiplier)
        {
            regenMultiplier *= Data.ModifyHealthRegen;
        }

        public override void ModifyStaminaRegen(ref float staminaRegen)
        {
            staminaRegen *= Data.ModifyStaminaRegen;
        }

        public override void ModifyRaiseSkill(Skills.SkillType skill, ref float value)
        {
            value *= Data.ModifyRaiseSkills;
        }

        public override void ModifySpeed(float baseSpeed, ref float speed)
        {
            speed *= Data.MofidySpeed;
        }

        public override void ModifyNoise(float baseNoise, ref float noise)
        {
            noise *= Data.ModifyNoise;
        }

        public override void ModifyStealth(float baseStealth, ref float stealth)
        {
            stealth *= Data.MofidyStealth;
        }

        public override void ModifyMaxCarryWeight(float baseLimit, ref float limit)
        {
            limit += Data.ModifyMaxCarryWeight;
        }

        public override void ModifyRunStaminaDrain(float baseDrain, ref float drain)
        {
            drain *= Data.RunStaminaDrain;
        }

        public override void ModifyJumpStaminaUsage(float baseStaminaUse, ref float staminaUse)
        {
            staminaUse *= Data.ModifyJumpStaminaUsage;
        }

        public override void OnDamaged(HitData hit, Character attacker)
        {
            float mod = Mathf.Clamp01(1f - Data.DamageReduction);
            hit.ApplyModifier(mod);
        }
    }
}
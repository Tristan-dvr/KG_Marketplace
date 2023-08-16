namespace Marketplace.Modules.NPC;

public class NPC_DataTypes
{
    public struct NPC_Fashion : ISerializableParameter
    {
        public string LeftItem;
        public string RightItem;
        public string HelmetItem;
        public string ChestItem;
        public string LegsItem;
        public string CapeItem;
        public string HairItem;
        public string HairColor;
        public string ModelScale;
        public string LeftItemHidden;
        public string RightItemHidden;
        public string InteractAnimation;
        public string GreetAnimation;
        public string ByeAnimation;
        public string GreetText;
        public string ByeText;
        public string SkinColor;
        public string CraftingAnimation;
        public string BeardItem;
        public string BeardColor;
        public string InteractAudioClip;
        public string TextSize;
        public string TextHeight;
        public string PeriodicAnimation;
        public string PeriodicAnimationTime;
        public string PeriodicSound;
        public string PeriodicSoundTime;

        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write(LeftItem ?? "");
            pkg.Write(RightItem ?? "");
            pkg.Write(HelmetItem ?? "");
            pkg.Write(ChestItem ?? "");
            pkg.Write(LegsItem ?? "");
            pkg.Write(CapeItem ?? "");
            pkg.Write(HairItem ?? "");
            pkg.Write(HairColor ?? "");
            pkg.Write(ModelScale ?? "");
            pkg.Write(LeftItemHidden ?? "");
            pkg.Write(RightItemHidden ?? "");
            pkg.Write(InteractAnimation ?? "");
            pkg.Write(GreetAnimation ?? "");
            pkg.Write(ByeAnimation ?? "");
            pkg.Write(GreetText ?? "");
            pkg.Write(ByeText ?? "");
            pkg.Write(SkinColor ?? "");
            pkg.Write(CraftingAnimation ?? "");
            pkg.Write(BeardItem ?? "");
            pkg.Write(BeardColor ?? "");
            pkg.Write(InteractAudioClip ?? "");
            pkg.Write(TextSize ?? "");
            pkg.Write(TextHeight ?? "");
            pkg.Write(PeriodicAnimation ?? "");
            pkg.Write(PeriodicAnimationTime ?? "");
            pkg.Write(PeriodicSound ?? "");
            pkg.Write(PeriodicSoundTime ?? "");
        }

        public void Deserialize(ref ZPackage pkg)
        {
            LeftItem = pkg.ReadString();
            RightItem = pkg.ReadString();
            HelmetItem = pkg.ReadString();
            ChestItem = pkg.ReadString();
            LegsItem = pkg.ReadString();
            CapeItem = pkg.ReadString();
            HairItem = pkg.ReadString();
            HairColor = pkg.ReadString();
            ModelScale = pkg.ReadString();
            LeftItemHidden = pkg.ReadString();
            RightItemHidden = pkg.ReadString();
            InteractAnimation = pkg.ReadString();
            GreetAnimation = pkg.ReadString();
            ByeAnimation = pkg.ReadString();
            GreetText = pkg.ReadString();
            ByeText = pkg.ReadString();
            SkinColor = pkg.ReadString();
            CraftingAnimation = pkg.ReadString();
            BeardItem = pkg.ReadString();
            BeardColor = pkg.ReadString();
            InteractAudioClip = pkg.ReadString();
            TextSize = pkg.ReadString();
            TextHeight = pkg.ReadString();
            PeriodicAnimation = pkg.ReadString();
            PeriodicAnimationTime = pkg.ReadString();
            PeriodicSound = pkg.ReadString();
            PeriodicSoundTime = pkg.ReadString();
        }
    }
    
    public class NpcData
    {
        public string? PrefabOverride;
        public string? LeftItem;
        public string? RightItem;
        public string? HelmetItem;
        public string? ChestItem;
        public string? LegsItem;
        public string? CapeItem;
        public string? HairItem;
        public string? HairItemColor;
        public string? ModelScale;
        public string? LeftItemHidden;
        public string? RightItemHidden;
        public string? NPCinteractAnimation;
        public string? NPCgreetAnimation;
        public string? NPCbyeAnimation;
        public string? NPCgreetText;
        public string? NPCbyeText;
        public string? SkinColor;
        public string? NPCcraftingAnimation;
        public string? BeardItem;
        public string? BeardItemColor;
        public string? InteractAudioClip;
        public string? TextSize;
        public string? TextHeight;
        public string? PeriodicAnimation;
        public string? PeriodicAnimationTime;
        public string PeriodicSound = "";
        public string PeriodicSoundTime = "0";
        public string? IMAGE;
    }
}
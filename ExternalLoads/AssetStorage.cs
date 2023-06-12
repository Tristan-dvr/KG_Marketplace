using BepInEx.Bootstrap;
using Marketplace.Base64;
using Marketplace.Paths;
using UnityEngine.Networking;

namespace Marketplace.AssetStorage;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Both, Market_Autoload.Priority.First, "OnInit")]
public static class AssetStorage
{
    public static AssetBundle asset;

    public static readonly Dictionary<string, Sprite> GlobalCachedSprites = new();
    public static readonly Dictionary<string, AudioClip> NPC_AudioClips = new();
    public static GameObject MarketplaceQuestTargetIcon;
    public static GameObject MarketplaceQuestQuestionIcon;
    public static Sprite PlaceholderMonsterIcon;
    public static Sprite PlaceholderGamblerIcon;
    public static Sprite EpicMMO_Exp;
    public static Sprite Cozyheim_Exp;
    public static Sprite Battlepass_EXP_Icon;
    public static Sprite MH_Exp_Icon;
    public static Sprite NullSprite;
    public static Texture WoodTex;
    public static AudioClip OpenUI_Sound;
    public static AudioClip CloseUI_Sound;
    public static GameObject FreeTakeEffect;
    public static GameObject PremiumTakeEffect;
    public static AudioClip TypeClip;
    public static GameObject Teleporter_VFX1;
    public static Sprite PortalIconDefault;
    public static GameObject Teleporter_VFX2;

    public static AudioSource AUsrc;
    

    // ReSharper disable once UnusedMember.Global
    private static void OnInit()
    {
        asset = GetAssetBundle("kgmarketplacemod");
        if (Marketplace.WorkingAsType is Marketplace.WorkingAs.Server) return;
        TypeClip = asset.LoadAsset<AudioClip>("TypeKeySoundMP");
        NullSprite = asset.LoadAsset<Sprite>("NullSpriteMP");
        OpenUI_Sound = asset.LoadAsset<AudioClip>("UI_InventoryShow_MPASN");
        CloseUI_Sound = asset.LoadAsset<AudioClip>("UI_InventoryHide_MPASN");
        Battlepass_EXP_Icon = asset.LoadAsset<Sprite>("premiumuser");
        WoodTex = asset.LoadAsset<Texture>("cbimage");
        Teleporter_VFX1 = asset.LoadAsset<GameObject>("MarketplaceTP_1");
        Teleporter_VFX2 = asset.LoadAsset<GameObject>("MarketplaceTP_2");
        MarketplaceQuestTargetIcon = asset.LoadAsset<GameObject>("MarketplaceQuestTargetIcon");
        MarketplaceQuestQuestionIcon = asset.LoadAsset<GameObject>("MarketplaceQuestionMark");
        Texture2D epicMMOTex = new Texture2D(1, 1);
        epicMMOTex.LoadImage(Convert.FromBase64String(ResourcesBase64.EpicMMO_Icon));
        EpicMMO_Exp = Sprite.Create(epicMMOTex, new Rect(0, 0, epicMMOTex.width, epicMMOTex.height), Vector2.zero);
        Texture2D mh_texture = new Texture2D(1, 1);
        mh_texture.LoadImage(Convert.FromBase64String(ResourcesBase64.MagicHeim_Icon));
        MH_Exp_Icon = Sprite.Create(mh_texture, new Rect(0, 0, mh_texture.width, mh_texture.height), Vector2.zero);
        Texture2D gamblerTex = new Texture2D(1, 1);
        gamblerTex.LoadImage(Convert.FromBase64String(ResourcesBase64.PlacholderGambler));
        Texture2D cozyheimTex = new Texture2D(1, 1);
        cozyheimTex.LoadImage(Convert.FromBase64String(ResourcesBase64.Cozyheim_LevelingSystem_Icon));
        Cozyheim_Exp = Sprite.Create(cozyheimTex, new Rect(0, 0, cozyheimTex.width, cozyheimTex.height), Vector2.zero);
        PlaceholderGamblerIcon = Sprite.Create(gamblerTex, new Rect(0, 0, gamblerTex.width, gamblerTex.height),
            Vector2.zero);
        Texture2D monsterTex = new Texture2D(1, 1);
        monsterTex.LoadImage(Convert.FromBase64String(ResourcesBase64.PlaceholderMonster));
        PlaceholderMonsterIcon = Sprite.Create(monsterTex, new Rect(0, 0, monsterTex.width, monsterTex.height),
            Vector2.zero);
        Texture2D def = new Texture2D(1, 1);
        def.LoadImage(Convert.FromBase64String(ResourcesBase64.defaultPortal));
        PortalIconDefault = Sprite.Create(def, new Rect(0, 0, def.width, def.height), new Vector2(0, 0));
        FreeTakeEffect = asset.LoadAsset<GameObject>("BattlepassEffectFree");
        PremiumTakeEffect = asset.LoadAsset<GameObject>("BattlepassEffectPremium");
        foreach (string file in Directory.GetFiles(Market_Paths.CachedImagesFolder, "*.png", SearchOption.TopDirectoryOnly))
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            byte[] data = File.ReadAllBytes(file);
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(data);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0));
            GlobalCachedSprites[fileName] = sprite;
        }
        GlobalCachedSprites.Add("teleporter_default", PortalIconDefault);
        Marketplace._thistype.StartCoroutine(LoadSoundsCoroutine(Market_Paths.NPC_SoundsPath));
    }


    private static IEnumerator LoadSoundsCoroutine(string path)
    {
        foreach (string file in Directory.GetFiles(path))
        {
            using (UnityWebRequest stream = UnityWebRequestMultimedia.GetAudioClip(file, AudioType.MPEG))
            {
                yield return stream.SendWebRequest();
                try
                {
                    AudioClip newClip = DownloadHandlerAudioClip.GetContent(stream);
                    NPC_AudioClips[Path.GetFileNameWithoutExtension(file)] = newClip;
                }
                catch (Exception ex)
                {
                    Utils.print($"Can't load clip: {file}. Reason:\n{ex}", ConsoleColor.Red);
                }
            }
        }
    }

    private static AssetBundle GetAssetBundle(string filename)
    {
        Assembly execAssembly = Assembly.GetExecutingAssembly();
        string resourceName = execAssembly.GetManifestResourceNames().Single(str => str.EndsWith(filename));
        using Stream stream = execAssembly.GetManifestResourceStream(resourceName);
        return AssetBundle.LoadFromStream(stream);
    }

    [HarmonyPatch(typeof(AudioMan), nameof(AudioMan.Awake))]
    [ClientOnlyPatch]
    private static class AudioMan_Awake_Patch
    {
        private static void Postfix(AudioMan __instance)
        {
            AUsrc = Chainloader.ManagerObject.AddComponent<AudioSource>();
            AUsrc.clip = asset.LoadAsset<AudioClip>("MarketClick");
            AUsrc.reverbZoneMix = 0;
            AUsrc.spatialBlend = 0;
            AUsrc.bypassListenerEffects = true;
            AUsrc.bypassEffects = true;
            AUsrc.volume = 0.8f;
            AUsrc.outputAudioMixerGroup = __instance.m_masterMixer.outputAudioMixerGroup;
            foreach (GameObject allAsset in asset.LoadAllAssets<GameObject>())
            {
                foreach (AudioSource audioSource in allAsset.GetComponentsInChildren<AudioSource>(true))
                {
                    audioSource.outputAudioMixerGroup = __instance.m_masterMixer.outputAudioMixerGroup;
                }
            }
        }
    }
}
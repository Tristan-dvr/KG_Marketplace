using BepInEx.Bootstrap;
using Marketplace.Paths;
using UnityEngine.Networking;

namespace Marketplace.ExternalLoads;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Both, Market_Autoload.Priority.First)]
public static class AssetStorage
{
    public static AssetBundle asset = null!;

    public static readonly Dictionary<string, Sprite> GlobalCachedSprites = new();
    public static readonly Dictionary<string, AudioClip> NPC_AudioClips = new();
    public static GameObject MarketplaceQuestTargetIcon = null!;
    public static GameObject MarketplaceQuestQuestionIcon = null!;
    public static Sprite PlaceholderMonsterIcon = null!;
    public static Sprite PlaceholderGamblerIcon = null!;
    public static Sprite NPC_MapControl = null!;
    public static Sprite EpicMMO_Exp = null!;
    public static Sprite Cozyheim_Exp = null!;
    public static Sprite MH_Exp_Icon = null!;
    public static Sprite NullSprite = null!;
    public static Sprite CustomValue_Icon = null!;
    public static Sprite QuestCompleteIcon = null!;
    public static Sprite PortalIconDefault = null!;
    public static Texture WoodTex = null!;
    public static AudioClip TypeClip = null!;
    public static GameObject Teleporter_VFX1 = null!;
    public static GameObject Teleporter_VFX2 = null!;

    public static AudioSource AUsrc = null!;

    [UsedImplicitly]
    private static void OnInit()
    {
        asset = GetAssetBundle("kgmarketplacemod");
        if (Marketplace.WorkingAsType is Marketplace.WorkingAs.Server) return;
        TypeClip = asset.LoadAsset<AudioClip>("TypeKeySoundMP");
        NullSprite = asset.LoadAsset<Sprite>("NullSpriteMP");
        WoodTex = asset.LoadAsset<Texture>("cbimage");
        Teleporter_VFX1 = asset.LoadAsset<GameObject>("MarketplaceTP_1");
        Teleporter_VFX2 = asset.LoadAsset<GameObject>("MarketplaceTP_2");
        MarketplaceQuestTargetIcon = asset.LoadAsset<GameObject>("MarketplaceQuestTargetIcon");
        MarketplaceQuestQuestionIcon = asset.LoadAsset<GameObject>("MarketplaceQuestionMark");
        QuestCompleteIcon = asset.LoadAsset<Sprite>("questcomplete");
        EpicMMO_Exp = asset.LoadAsset<Sprite>("EpicMMOIcon");
        CustomValue_Icon = asset.LoadAsset<Sprite>("CustomValueIcon");
        MH_Exp_Icon = asset.LoadAsset<Sprite>("magicheim_icon");
        Cozyheim_Exp = asset.LoadAsset<Sprite>("cozyheim_icon");
        PlaceholderGamblerIcon = asset.LoadAsset<Sprite>("placeholdergambler_icon");
        PlaceholderMonsterIcon = asset.LoadAsset<Sprite>("placeholdermonster_icon");
        PortalIconDefault = asset.LoadAsset<Sprite>("default_portal_icon");
        NPC_MapControl = asset.LoadAsset<Sprite>("NPC_MapControl");

        GlobalCachedSprites["teleporter_default"] = PortalIconDefault;

        ReloadImages();
        ReloadSounds();
    }


    private static IEnumerator LoadSoundsCoroutine(string path)
    {
        foreach (string file in Directory.GetFiles(path, "*.mp3", SearchOption.AllDirectories))
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

    public static void ReloadSounds() =>
        Marketplace._thistype.StartCoroutine(LoadSoundsCoroutine(Market_Paths.NPC_SoundsPath));

    public static void ReloadImages()
    {
        foreach (string file in Directory.GetFiles(Market_Paths.CachedImagesFolder, "*.png",
                     SearchOption.AllDirectories))
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            byte[] data = File.ReadAllBytes(file);
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(data);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0));
            GlobalCachedSprites[fileName] = sprite;
        }
    }

    private static AssetBundle GetAssetBundle(string filename)
    {
        Assembly execAssembly = Assembly.GetExecutingAssembly();
        string resourceName = execAssembly.GetManifestResourceNames().Single(str => str.EndsWith(filename));
        using Stream stream = execAssembly.GetManifestResourceStream(resourceName)!;
        return AssetBundle.LoadFromStream(stream);
    }

    [HarmonyPatch(typeof(AudioMan), nameof(AudioMan.Awake))]
    [ClientOnlyPatch]
    private static class AudioMan_Awake_Patch
    {
        [UsedImplicitly]
        private static void Postfix(AudioMan __instance)
        {
            var SFXgroup = __instance.m_masterMixer.FindMatchingGroups("SFX")[0];
            AUsrc = Chainloader.ManagerObject.AddComponent<AudioSource>();
            AUsrc.clip = asset.LoadAsset<AudioClip>("MarketClick");
            AUsrc.reverbZoneMix = 0;
            AUsrc.spatialBlend = 0;
            AUsrc.bypassListenerEffects = true;
            AUsrc.bypassEffects = true;
            AUsrc.volume = 0.8f;
            AUsrc.outputAudioMixerGroup = AudioMan.instance.m_masterMixer.outputAudioMixerGroup;
            foreach (GameObject go in asset.LoadAllAssets<GameObject>())
                foreach (AudioSource audioSource in go.GetComponentsInChildren<AudioSource>(true))
                    audioSource.outputAudioMixerGroup = SFXgroup;
        }
    }
}
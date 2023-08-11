using System.Text.RegularExpressions;
using API;
using Marketplace.Modules.Banker;
using Marketplace.Modules.Buffer;
using Marketplace.Modules.Feedback;
using Marketplace.Modules.Gambler;
using Marketplace.Modules.Marketplace_NPC;
using Marketplace.Modules.NPC_Dialogues;
using Marketplace.Modules.Quests;
using Marketplace.Modules.ServerInfo;
using Marketplace.Modules.Teleporter;
using Marketplace.Modules.Trader;
using Marketplace.Modules.Transmogrification;
using UnityEngine.EventSystems;
using Image = UnityEngine.UI.Image;
using Object = UnityEngine.Object;

namespace Marketplace.Modules.NPC;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Both, Market_Autoload.Priority.Normal)]
public static class Market_NPC
{
    private static GameObject NPC;
    private static GameObject PinnedNPC;
    private static readonly string[] TypeNames = Enum.GetNames(typeof(NPCType));
    private static readonly int SkinColor = Shader.PropertyToID("_SkinColor");
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");
    private static readonly int SkinBumpMap = Shader.PropertyToID("_SkinBumpMap");
    private static readonly int ChestTex = Shader.PropertyToID("_ChestTex");
    private static readonly int ChestBumpMap = Shader.PropertyToID("_ChestBumpMap");
    private static readonly int ChestMetal = Shader.PropertyToID("_ChestMetal");
    private static readonly int LegsTex = Shader.PropertyToID("_LegsTex");
    private static readonly int LegsBumpMap = Shader.PropertyToID("_LegsBumpMap");
    private static readonly int LegsMetal = Shader.PropertyToID("_LegsMetal");
    private static readonly int Wakeup = Animator.StringToHash("wakeup");
    private static readonly int Crafting = Animator.StringToHash("crafting");
    private static readonly int Stagger = Animator.StringToHash("stagger");

    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Awake))]
    [ClientOnlyPatch]
    private static class FejdStartup_Awake_Patch
    {
        private static bool done;

        private static void Postfix(FejdStartup __instance)
        {
            if (!done)
            {
                done = true;
                if (__instance.transform.Find("StartGame/Panel/JoinPanel/serverCount")
                        ?.GetComponent<TextMeshProUGUI>() is { } tmp)
                {
                    NPC.transform.Find("TMP").GetComponent<TMP_Text>().font = tmp.font;
                    NPC.transform.Find("TMP").GetComponent<TMP_Text>().outlineWidth = 0.075f;
                    Utils.print("Replaced TMP for NPC");
                }
            }
        }
    }

    public enum NPCType
    {
        None,
        Trader,
        Info,
        Teleporter,
        Feedback,
        Banker,
        Gambler,
        Quests,
        Buffer,
        Transmog,
        Marketplace
    }

    // ReSharper disable once UnusedMember.Global
    private static void OnInit()
    {
        NPC = AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("MarketPlaceNPC");
        if (Marketplace.WorkingAsType is Marketplace.WorkingAs.Server) return;
        NPC.AddComponent<NPCcomponent>();
        NPC.transform.Find("TMP").gameObject.AddComponent<TextComponent>();
        foreach (Animator animator in NPC.GetComponentsInChildren<Animator>(true))
        {
            if (animator.gameObject.name == "WomanNPC")
            {
                animator.gameObject.AddComponent<CustomLookAt>();
                SkinnedMeshRenderer mesh = animator.gameObject.GetComponentInChildren<SkinnedMeshRenderer>(true);
                foreach (Material material in mesh.materials)
                {
                    material.shader = Shader.Find("Custom/Creature");
                }
            }
        }
        NPCUI.Init();
        Marketplace.Global_Updator += UpdateNPCGUI;
    }

    private static void UpdateNPCGUI(float dt)
    {
        if (Input.GetKeyDown(KeyCode.Escape) && NPCUI.IsVisible())
        {
            NPCUI.Hide();
            Menu.instance.OnClose();
        }
    }

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    private static class NPCZnet
    {
        private static bool firstInit;

        private static void Postfix(ZNetScene __instance)
        {
            if (!firstInit)
            {
                firstInit = true;
                GameObject inactive = new GameObject("InactiveMPASN");
                Object.DontDestroyOnLoad(inactive);
                inactive.SetActive(false);
                PinnedNPC = Object.Instantiate(NPC, inactive.transform);
                PinnedNPC.name = "MarketPlaceNPCpinned";
                PinnedNPC.GetComponent<Piece>().m_name += " <color=#00FFFF>(Map Pin Visible)</color>";
            }

            __instance.m_namedPrefabs.Add(NPC.name.GetStableHashCode(), NPC);
            __instance.m_namedPrefabs.Add(PinnedNPC.name.GetStableHashCode(), PinnedNPC);
            GameObject goblin = AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("Marketplace_GOBLIN");
            __instance.m_namedPrefabs.Add(goblin.name.GetStableHashCode(), goblin);
            GameObject skeleton = AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("Marketplace_SKELETON");
            __instance.m_namedPrefabs.Add(skeleton.name.GetStableHashCode(), skeleton);
            GameObject questboard = AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("Marketplace_QUESTBOARD");
            __instance.m_namedPrefabs.Add(questboard.name.GetStableHashCode(), questboard);
            GameObject teleporter = AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("Marketplace_TELEPORTER");
            __instance.m_namedPrefabs.Add(teleporter.name.GetStableHashCode(), teleporter);
            GameObject defaultnpc = AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("Marketplace_DEFAULTNPC");
            foreach (Material material in defaultnpc.GetComponentInChildren<SkinnedMeshRenderer>().materials)
            {
                material.shader = Shader.Find("Custom/Creature");
            }

            __instance.m_namedPrefabs.Add(defaultnpc.name.GetStableHashCode(), defaultnpc);
        }
    }

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    [ClientOnlyPatch]
    private static class OdinFix
    {
        private static void Postfix(ZNetScene __instance)
        {
            GameObject odin = __instance.GetPrefab("odin");
            if (odin?.GetComponent<Animator>() is not { } anim) return;
            Object.Destroy(anim);
        }
    }

    [HarmonyPatch(typeof(Menu), nameof(Menu.IsVisible))]
    [ClientOnlyPatch]
    private static class IsVisiblePatch
    {
        private static void Postfix(ref bool __result)
        {
            if (NPCUI.IsVisible()) __result = true;
        }
    }

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
    [ClientOnlyPatch]
    private static class Chat_InputText_Patch_AdminCommands
    {
        private static void Postfix()
        {
            new Terminal.ConsoleCommand("npc remove", "Remove NPCs in range of 5 meters", (args) =>
            {
                if (!Utils.IsDebug) return;
                IEnumerable<NPCcomponent> FindNPCsInRange = NPCcomponent.ALL.Where(x =>
                    global::Utils.DistanceXZ(Player.m_localPlayer.transform.position, x.transform.position) <= 5f);
                int c = 0;
                string total = "";
                foreach (NPCcomponent npc in FindNPCsInRange)
                {
                    ++c;
                    string name = npc.GetClearNPCName();
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        name = Localization.instance.Localize("$mpasn_" + npc._currentNpcType);
                    }

                    total += $"\n{c + 1}) {name} {npc.transform.position}";
                    ZDOMan.instance.m_destroySendList.Add(npc.znv.m_zdo.m_uid);
                }

                args.Context.AddString($"Removed total {c} NPCs in range:{total}");
            });
            

            new Terminal.ConsoleCommand("idm", "Get Current Left/Right ItemDataManager values", (args) =>
            {
                if (!Utils.IsDebug) return;
                ItemDrop.ItemData weapon = Player.m_localPlayer.GetRightItem() != null
                    ? Player.m_localPlayer.GetRightItem()
                    : Player.m_localPlayer.GetLeftItem();
                if (weapon == null)
                {
                    args.Context.AddString("No weapon in hand");
                    return;
                }

                string IDM = JSON.ToNiceJSON(weapon.m_customData);
                args.Context.AddString($"IDM for {weapon.m_dropPrefab.name}:\n{IDM}");
            });

            new Terminal.ConsoleCommand("mpos", "Get Current Position",
                (args) =>
                {
                    args.Context.AddString($"<color=green>Position: {Player.m_localPlayer.transform.position}</color>");
                });

            new Terminal.ConsoleCommand("mfpslimit", "Set Fixed Update FPS", (args) =>
            {
                if (!Utils.IsDebug || args.Args.Length < 2) return;
                int fps = int.Parse(args.Args[1]);
                fps = Mathf.Clamp(fps, 50, 144);
                float time = 1f / fps;
                Time.fixedDeltaTime = time;
                args.Context.AddString($"<color=green>Fixed Update FPS set to {fps}</color>");
            });
        }
    }

    private class TextComponent : MonoBehaviour
    {
        private void Update()
        {
            Vector3 rot = Quaternion.LookRotation(GameCamera.instance.transform.forward).eulerAngles;
            rot.z = 0;
            transform.rotation = Quaternion.Euler(rot);
        }
    }

    public class HoverOnButton_WithImage : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private static readonly Dictionary<string, Sprite> Cache = new();
        private bool HasImage;
        private Image _image;
        private Sprite _sprite;
        private Transform _tt;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        public void Setup(string _name, Transform t, string base64)
        {
            _tt = t;
            try
            {
                if (Cache.TryGetValue(_name, out Sprite value))
                {
                    _sprite = value;
                    HasImage = true;
                    return;
                }

                byte[] bytes = Convert.FromBase64String(base64);
                Texture2D texture = new Texture2D(1, 1);
                texture.LoadImage(bytes);
                _sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                Cache.Add(_name, _sprite);
                HasImage = true;
            }
            catch
            {
                HasImage = false;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _image.color = Color.green;
            if (HasImage)
            {
                _tt.gameObject.SetActive(true);
                _tt.transform.GetChild(0).GetComponent<Image>().sprite = _sprite;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _image.color = Color.white;
            _tt.gameObject.SetActive(false);
        }
    }

    private class CustomLookAt : MonoBehaviour
    {
        private Animator animator;
        private Vector3 currentLookAt = Vector3.zero;
        private ZNetView _znet;

        private void Awake()
        {
            _znet = GetComponentInParent<ZNetView>();
            animator = GetComponent<Animator>();
            currentLookAt = transform.position + transform.forward * 4f + Vector3.up * 1.2f;
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (!_znet.IsValid()) return;
            if (animator && Player.m_localPlayer)
            {
                float speed = 2f;
                Vector3 target = Player.m_localPlayer.m_head.position;
                if (Vector3.Distance(target, transform.position) > 4f)
                {
                    target = transform.position + transform.forward * 3f + Vector3.up * 1.2f;
                    speed = 4f;
                }
                else
                {
                    Vector3 pVec = (transform.position - target).normalized;
                    float playerVec = Quaternion.LookRotation(pVec).eulerAngles.y;
                    float enemyVec = transform.rotation.eulerAngles.y;
                    float num = Mathf.Abs(Mathf.DeltaAngle(playerVec, enemyVec));
                    if (num < 100f)
                    {
                        target = transform.position + transform.forward * 3f + Vector3.up * 1.2f;
                    }
                }

                currentLookAt = Vector3.MoveTowards(currentLookAt, target, Time.deltaTime * speed);
                animator.SetLookAtPosition(currentLookAt);
                animator.SetLookAtWeight(1f, 0.1f, 1f, 0f);
            }
        }
    }


    public class NPCcomponent : MonoBehaviour, Hoverable, Interactable, IDestructible
    {
        private const string PatrolData = "KGmarket PatrolData";
        private const string LatestPatrolPoint = "KGmarket LatestPatrolPoint";
        private const string GoBackPatrol = "KGmarket GoBack";

        public static readonly List<NPCcomponent> ALL = new();
        public ZNetView znv;
        public GameObject pastOverrideModel;
        public NPCType _currentNpcType;

        public ZSyncAnimation zanim;
        private TMP_Text canvas;
        public AudioSource NPC_SoundSource;
        private bool WasClose;
        private Vector2[] PatrolArray;
        private float MaxSpeed;
        private float ForwardSpeed;
        private float PatrolTime;
        private float periodicAnimationTimer = 999f;
        private float periodicSoundTimer = 999f;


        [HarmonyPatch(typeof(MonoUpdaters), nameof(MonoUpdaters.Update))]
        [ClientOnlyPatch]
        private static class MonoUpdaters_Update_Patch
        {
            private static void Postfix()
            {
                foreach (NPCcomponent ccomponent in ALL)
                    ccomponent.CustomUpdate();
            }
        }

        [HarmonyPatch(typeof(MonoUpdaters), nameof(MonoUpdaters.FixedUpdate))]
        [ClientOnlyPatch]
        private static class MonoUpdaters_FixedUpdate_Patch
        {
            private static void Postfix()
            {
                foreach (NPCcomponent ccomponent in ALL)
                    ccomponent.CustomFixedUpdate();
            }
        }


        private void OnDestroy()
        {
            ALL.Remove(this);
        }

        private void InitPatrolData(string ForceData)
        {
            ForwardSpeed = 0f;
            if (zanim.enabled)
                zanim.SetFloat(Character.s_forwardSpeed, 0);
            if (string.IsNullOrEmpty(ForceData)) return;
            MaxSpeed = 1f;
            try
            {
                string[] maxSpeed = ForceData.Split('@');
                string[] split = maxSpeed[0].Replace(" ", "").Split(',');
                if (maxSpeed.Length == 2)
                {
                    MaxSpeed = Convert.ToSingle(maxSpeed[1], new CultureInfo("en-US"));
                }

                PatrolArray = new Vector2[split.Length / 2];
                for (int i = 0; i < split.Length; i += 2)
                {
                    float x = Convert.ToSingle(split[i], new CultureInfo("en-US"));
                    float y = Convert.ToSingle(split[i + 1], new CultureInfo("en-US"));
                    PatrolArray[i / 2] = new Vector2(x, y);
                }
            }
            catch
            {
                PatrolArray = null;
            }
        }

        private void CustomUpdate()
        {
            PatrolTime += Time.deltaTime;
            if (znv.m_zdo == null || !znv.IsOwner() || PatrolArray is not { Length: > 1 } || PatrolTime < 2f) return;
            Player closePlayer = Player.GetClosestPlayer(transform.position, 3f);
            ForwardSpeed = closePlayer && _currentNpcType is not NPCType.None
                ? Mathf.Clamp(ForwardSpeed - Time.deltaTime, 0, MaxSpeed)
                : Mathf.Clamp(ForwardSpeed + Time.deltaTime, 0, MaxSpeed);
            if (zanim.enabled)
                zanim.SetFloat(Character.s_forwardSpeed, ForwardSpeed * 1.5f);
            int currentPatrolPoint = znv.m_zdo.GetInt(LatestPatrolPoint);
            Vector2 currentPos = new Vector2(transform.position.x, transform.position.z);
            Vector2 move = Vector2.MoveTowards(currentPos, PatrolArray[currentPatrolPoint],
                Time.deltaTime * ForwardSpeed);
            Vector3 targetPoint = new Vector3(move.x, transform.position.y, move.y);
            Utils.CustomFindFloor(targetPoint, out float height);
            height = Mathf.Max(30, height, ZoneSystem.instance.GetGroundHeight(targetPoint));
            if (ForwardSpeed > 0)
            {
                Vector3 vecTest = (targetPoint - transform.position).normalized;
                if (vecTest != Vector3.zero)
                {
                    Quaternion newRotation = Quaternion.LookRotation(vecTest);
                    newRotation.x = 0;
                    transform.rotation =
                        Quaternion.RotateTowards(transform.rotation, newRotation, Time.deltaTime * 200f);
                }
            }

            transform.position = new Vector3(targetPoint.x, height, targetPoint.z);
            if (Vector2.Distance(PatrolArray[currentPatrolPoint],
                    new Vector2(transform.position.x, transform.position.z)) <= 0.25f)
            {
                bool goBack = znv.m_zdo.GetBool(GoBackPatrol);
                if (goBack)
                {
                    currentPatrolPoint--;

                    if (currentPatrolPoint < 0)
                    {
                        currentPatrolPoint = 1;
                        znv.m_zdo.Set(GoBackPatrol, false);
                    }

                    znv.m_zdo.Set(LatestPatrolPoint, currentPatrolPoint);
                    ForwardSpeed = MaxSpeed / 2.5f;
                }
                else
                {
                    currentPatrolPoint++;
                    if (currentPatrolPoint >= PatrolArray.Length)
                    {
                        currentPatrolPoint -= 2;
                        znv.m_zdo.Set(GoBackPatrol, true);
                    }

                    znv.m_zdo.Set(LatestPatrolPoint, currentPatrolPoint);
                    ForwardSpeed = MaxSpeed / 2.5f;
                }
            }
        }

        private void CustomFixedUpdate()
        {
            if (!znv || znv.m_zdo == null) return;
            if (!Player.m_localPlayer) return;
            Vector3 target = Player.m_localPlayer.m_head.position;
            Vector3 pVec = (transform.position - target).normalized;
            float playerVec = Quaternion.LookRotation(pVec).eulerAngles.y;
            float enemyVec = transform.rotation.eulerAngles.y;
            float num = Mathf.Abs(Mathf.DeltaAngle(playerVec, enemyVec));

            if (znv.IsOwner() && znv.m_zdo.GetFloat("KGperiodicAnimationTime") > 0)
            {
                periodicAnimationTimer += Time.fixedDeltaTime;
                if (periodicAnimationTimer >= znv.m_zdo.GetFloat("KGperiodicAnimationTime"))
                {
                    periodicAnimationTimer = 0;
                    if (!string.IsNullOrWhiteSpace(znv.m_zdo.GetString("KGperiodicAnimation")))
                    {
                        zanim.SetTrigger(znv.m_zdo.GetString("KGperiodicAnimation"));
                    }
                }
            }

            if (znv.m_zdo.GetFloat("KGperiodicSoundTime") > 0)
            {
                periodicSoundTimer += Time.fixedDeltaTime;
                if (periodicSoundTimer >= znv.m_zdo.GetFloat("KGperiodicSoundTime"))
                {
                    periodicSoundTimer = 0;
                    if (!string.IsNullOrWhiteSpace(znv.m_zdo.GetString("KGperiodicSound")))
                    {
                        if (AssetStorage.AssetStorage.NPC_AudioClips.TryGetValue(znv.m_zdo.GetString("KGperiodicSound"),
                                out AudioClip sound))
                            NPC_SoundSource.PlayOneShot(sound);
                    }
                }
            }


            if (!WasClose)
            {
                if (Vector3.Distance(target, transform.position) < 10f)
                {
                    WasClose = true;
                    if (num > 80f)
                    {
                        if (!string.IsNullOrWhiteSpace(znv.m_zdo.GetString("KGgreetingText")))
                        {
                            Chat.instance.SetNpcText(gameObject, Vector3.up * 4f, 20f, 3f, "",
                                znv.m_zdo.GetString("KGgreetingText"), false);
                        }

                        if (!string.IsNullOrWhiteSpace(znv.m_zdo.GetString("KGgreetingAnimation")))
                        {
                            zanim.SetTrigger(znv.m_zdo.GetString("KGgreetingAnimation"));
                        }
                    }
                }
            }
            else
            {
                if (Vector3.Distance(target, transform.position) > 12f)
                {
                    WasClose = false;
                    if (num > 80f)
                    {
                        if (!string.IsNullOrWhiteSpace(znv.m_zdo.GetString("KGbyeText")))
                        {
                            Chat.instance.SetNpcText(gameObject, Vector3.up * 4f, 20f, 3f, "",
                                znv.m_zdo.GetString("KGbyeText"), false);
                        }

                        if (!string.IsNullOrWhiteSpace(znv.m_zdo.GetString("KGbyeAnimation")))
                        {
                            zanim.SetTrigger(znv.m_zdo.GetString("KGbyeAnimation"));
                        }
                    }
                }
            }
        }

        private void Awake()
        {
            znv = GetComponent<ZNetView>();
            if (!znv || znv.m_zdo == null || Marketplace.WorkingAsType is Marketplace.WorkingAs.Server) return;
            NPC_SoundSource = gameObject.AddComponent<AudioSource>();
            NPC_SoundSource.spatialBlend = 1;
            NPC_SoundSource.volume = 0.8f;
            NPC_SoundSource.outputAudioMixerGroup = AudioMan.instance.m_masterMixer.outputAudioMixerGroup;
            NPC_SoundSource.maxDistance = 10f;
            NPC_SoundSource.bypassReverbZones = true;
            ALL.Add((this));
            zanim = GetComponent<ZSyncAnimation>();
            if (znv.m_zdo == null) return;
            canvas = transform.Find("TMP").GetComponent<TMP_Text>();
            int npcType = znv.m_zdo.GetInt("KGmarketNPC");
            if (npcType >= TypeNames.Length) npcType = 0;
            _currentNpcType = (NPCType)npcType;
            if (!znv.m_functions.ContainsKey("KGMarket changeNpcType".GetStableHashCode()))
            {
                znv.Register("KGMarket changeNpcType", new Action<long, int>(ChangeNpcType));
                znv.Register("KGMarket removeNpc", RemoveNPC);
                znv.Register("KGmarket snapandrotate", new Action<long, ZPackage>(SnapAndRotate));
                znv.Register("KGmarket changeprofile", new Action<long, string, string>(ChangeProfile));
                znv.Register("KGmarket overridename", new Action<long, string>(OverrideName));
                znv.Register("KGmarket overridemodel", new Action<long, string>(OverrideModel));
                znv.Register("KGmarket fashion", new Action<long, NPC_DataTypes.NPC_Fashion>(FashionApply));
                znv.Register("KGmarket GetPatrolData", new Action<long, string>(GetPatrolData));
            }

            OverrideModel(0, znv.m_zdo.GetString("KGnpcModelOverride"));
            GameObject go = Instantiate(AssetStorage.AssetStorage.MarketplaceQuestQuestionIcon, transform);
            go.transform.position += Vector3.up * 4.5f;
            go.name = "MPASNquest";
            go.SetActive(Quests_DataTypes.Quest.IsQuestTarget(GetNPCName()));
            InitPatrolData(znv.m_zdo.GetString(PatrolData));
        }


        private void GetPatrolData(long sender, string data)
        {
            if (znv.IsOwner())
            {
                znv.m_zdo.Set(PatrolData, data);
                znv.m_zdo.Set(LatestPatrolPoint, 0);
                znv.m_zdo.Set(GoBackPatrol, false);
            }

            InitPatrolData(data);
        }


        public string GetHoverText()
        {
            if (Menu.IsVisible()) return "";

            string admintext = Utils.IsDebug
                ? "\n" + Localization.instance.Localize("[<color=yellow><b>$KEY_AltPlace + $KEY_Use</b></color>]") +
                  " " + Localization.instance.Localize("$mpasn_opennnpcui") +
                  "\n" + Localization.instance.Localize("[<color=yellow><b>ALT + $KEY_Use</b></color>]") +
                  " " + Localization.instance.Localize("$mpasn_fashionmenu") +
                  "\n" + Localization.instance.Localize("[<color=red><b>DELETE + $KEY_Use</b></color>]") +
                  " " + Localization.instance.Localize("$mpasn_removenpc")
                : "";
            string text = Localization.instance.Localize("[<color=yellow><b>$KEY_Use</b></color>] ") +
                          Localization.instance.Localize("$mpasn_interact");

            return text + admintext;
        }

        public string GetHoverName()
        {
            return "";
        }

        public string GetNPCName()
        {
            return znv.m_zdo.GetString("KGnpcNameOverride");
        }

        public string GetClearNPCName()
        {
            return Regex.Replace(GetNPCName(), @"<\/?(?!color\b)\w+[^>]*>", "");
        }

        public void OpenUIForType(string type, string profile) =>
            OpenUIForType(type == null ? null : (NPCType)Enum.Parse(typeof(NPCType), type, true), profile);

        public void OpenUIForType(NPCType? type = null, string profile = null)
        {
            if (string.IsNullOrEmpty(profile))
                profile = znv.m_zdo.GetString("KGnpcProfile", "default");
            string npcName = znv.m_zdo.GetString("KGnpcNameOverride");
            profile = profile.ToLower();
            type ??= _currentNpcType;
            switch (type)
            {
                case NPCType.Marketplace:
                    if (!string.IsNullOrWhiteSpace(Global_Values._localUserID) && !ZNet.IsSinglePlayer)
                        Marketplace_UI.Show();
                    break;
                case NPCType.Info:
                    ServerInfo_UI.Show(profile, npcName);
                    break;
                case NPCType.Trader:
                    Trader_UI.Show(profile, npcName);
                    break;
                case NPCType.Banker:
                    Banker_UI.Show(profile, npcName);
                    break;
                case NPCType.Teleporter:
                    Teleporter_Main_Client.ShowTeleporterUI(profile);
                    break;
                case NPCType.Feedback:
                    Feedback_UI.Show();
                    break;
                case NPCType.Gambler:
                    Gambler_UI.Show(profile);
                    break;
                case NPCType.Quests:
                    Quests_UIs.QuestUI.Show(profile, npcName);
                    break;
                case NPCType.Buffer:
                    Buffer_UI.Show(profile, npcName);
                    break;
                case NPCType.Transmog:
                    Transmogrification_UI.Show(profile, npcName);
                    break;
            }
        }

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            if (!string.IsNullOrWhiteSpace(znv.m_zdo.GetString("KGinteractAnimation")))
            {
                zanim.SetTrigger(znv.m_zdo.GetString("KGinteractAnimation"));
            }

            string interactAudio = znv.m_zdo.GetString("KGinteractSound");
            NPC_SoundSource.Stop();
            if (!string.IsNullOrEmpty(interactAudio) &&
                AssetStorage.AssetStorage.NPC_AudioClips.TryGetValue(interactAudio, out AudioClip interactClip))
            {
                NPC_SoundSource.clip = interactClip;
                NPC_SoundSource.Play();
            }

            if (Input.GetKey(KeyCode.LeftAlt) && Utils.IsDebug)
            {
                NPCUI.ShowFashion(this.znv.m_zdo);
                return true;
            }

            if (alt && Utils.IsDebug)
            {
                NPCUI.ShowMain(this.znv.m_zdo);
                return true;
            }

            if (Input.GetKey(KeyCode.Delete) && Utils.IsDebug)
            {
                znv.InvokeRPC("KGMarket removeNpc");
                return true;
            }

            if (Quests_DataTypes.Quest.TryAddRewardTalk(GetClearNPCName()))
            {
                Quests_UIs.AcceptedQuestsUI.CheckQuests();
            }

            if (!string.IsNullOrWhiteSpace(znv.m_zdo.GetString("KGnpcDialogue")))
            {
                if (Dialogues_UI.LoadDialogue(this, znv.m_zdo.GetString("KGnpcDialogue")))
                    return true;
            }

            OpenUIForType();
            return true;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }

        private void FashionApply(long sender, NPC_DataTypes.NPC_Fashion fashion)
        {
            if (znv.m_zdo.IsOwner())
            {
                znv.m_zdo.Set("KGleftItem", fashion.LeftItem);
                znv.m_zdo.Set("KGrightItem", fashion.RightItem);
                znv.m_zdo.Set("KGhelmetItem", fashion.HelmetItem);
                znv.m_zdo.Set("KGchestItem", fashion.ChestItem);
                znv.m_zdo.Set("KGlegsItem", fashion.LegsItem);
                znv.m_zdo.Set("KGcapeItem", fashion.CapeItem);
                znv.m_zdo.Set("KGhairItem", fashion.HairItem);
                znv.m_zdo.Set("KGhairItemColor", fashion.HairColor);
                znv.m_zdo.Set("KGLeftItemBack", fashion.LeftItemHidden);
                znv.m_zdo.Set("KGRightItemBack", fashion.RightItemHidden);
                znv.m_zdo.Set("KGinteractAnimation", fashion.InteractAnimation);
                znv.m_zdo.Set("KGgreetingAnimation", fashion.GreetAnimation);
                znv.m_zdo.Set("KGbyeAnimation", fashion.ByeAnimation);
                znv.m_zdo.Set("KGgreetingText", fashion.GreetText);
                znv.m_zdo.Set("KGbyeText", fashion.ByeText);
                znv.m_zdo.Set("KGskinColor", fashion.SkinColor);
                znv.m_zdo.Set("KGcraftingAnimation",
                    int.TryParse(fashion.CraftingAnimation, out int craftingZDO) ? craftingZDO : 0);
                znv.m_zdo.Set("KGbeardItem", fashion.BeardItem);
                znv.m_zdo.Set("KGbeardColor", fashion.BeardColor);
                znv.m_zdo.Set("KGinteractSound", fashion.InteractAudioClip);
                znv.m_zdo.Set("KGtextSize",
                    float.TryParse(fashion.TextSize, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture,
                        out float textSizeZDO)
                        ? textSizeZDO
                        : 3);
                znv.m_zdo.Set("KGtextHeight",
                    float.TryParse(fashion.TextHeight, NumberStyles.Any, CultureInfo.InvariantCulture,
                        out float textHeightZDO)
                        ? textHeightZDO
                        : 0f);
                znv.m_zdo.Set("KGperiodicAnimation", fashion.PeriodicAnimation);
                znv.m_zdo.Set("KGperiodicAnimationTime",
                    float.TryParse(fashion.PeriodicAnimationTime, NumberStyles.AllowDecimalPoint,
                        CultureInfo.InvariantCulture,
                        out float periodicAnimationTimeZDO)
                        ? periodicAnimationTimeZDO
                        : 0f);
                znv.m_zdo.Set("KGperiodicSound", fashion.PeriodicSound);
                znv.m_zdo.Set("KGperiodicSoundTime",
                    float.TryParse(fashion.PeriodicSoundTime, NumberStyles.AllowDecimalPoint,
                        CultureInfo.InvariantCulture,
                        out float periodicSoundTimeZDO)
                        ? periodicSoundTimeZDO
                        : 0f);
                if (!float.TryParse(fashion.ModelScale, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture,
                        out float scaleFloat)) scaleFloat = 1f;

                znv.m_zdo.Set("KGnpcScale", scaleFloat);
            }

            periodicAnimationTimer = 999f;
            periodicSoundTimer = 999f;
            string prefab = znv.m_zdo.GetString("KGnpcModelOverride");
            if (TryOverrideModel(ref prefab, out bool isFemale, false))
            {
                pastOverrideModel.GetComponent<Animator>()?.SetBool(Wakeup, false);
                if (int.TryParse(fashion.CraftingAnimation, out int CRAFTING))
                {
                    pastOverrideModel.GetComponent<Animator>()?.SetInteger(Crafting, CRAFTING);
                }

                VisEquipment visuals = ZNetScene.instance.GetPrefab(prefab).GetComponent<VisEquipment>();
                if (visuals)
                {
                    pastOverrideModel.GetComponent<Animator>()?.SetBool(Wakeup, false);
                    SkinnedMeshRenderer skin = pastOverrideModel.GetComponentInChildren<SkinnedMeshRenderer>();
                    Transform leftArm = global::Utils.FindChild(pastOverrideModel.transform, visuals.m_leftHand?.name);
                    Transform rightgArm =
                        global::Utils.FindChild(pastOverrideModel.transform, visuals.m_rightHand?.name);
                    Transform helmetJoint =
                        global::Utils.FindChild(pastOverrideModel.transform, visuals.m_helmet?.name);
                    Transform leftBack =
                        global::Utils.FindChild(pastOverrideModel.transform, visuals.m_backShield?.name);
                    Transform rightBack =
                        global::Utils.FindChild(pastOverrideModel.transform, visuals.m_backMelee?.name);
                    string leftItem = fashion.LeftItem;
                    string rightItem = fashion.RightItem;
                    string helmetItem = fashion.HelmetItem;
                    string leftBackItem = fashion.LeftItemHidden;
                    string rightBackItem = fashion.RightItemHidden;
                    EquipItemsOnModel(leftArm, leftItem, skin);
                    EquipItemsOnModel(rightgArm, rightItem, skin);
                    if (ZNetScene.instance.GetPrefab(leftBackItem))
                    {
                        if (ZNetScene.instance.GetPrefab(leftBackItem).GetComponent<ItemDrop>().m_itemData
                            .IsWeapon())
                        {
                            EquipItemsOnModel(rightBack, leftBackItem, skin, true);
                        }
                        else
                        {
                            EquipItemsOnModel(leftBack, leftBackItem, skin);
                        }
                    }

                    EquipItemsOnModel(rightBack, rightBackItem, skin);
                    EquipItemsOnModel(helmetJoint, helmetItem, skin);
                    if (prefab == "Player")
                    {
                        if (pastOverrideModel.GetComponent<CustomLookAt>() == null && CRAFTING == 0)
                        {
                            pastOverrideModel.AddComponent<CustomLookAt>();
                        }

                        CapsuleCollider[] capsule =
                        {
                            pastOverrideModel.transform.Find("Armature/Hips/ClothCollider")
                                .GetComponent<CapsuleCollider>(),
                            pastOverrideModel.transform.Find("Armature/Hips/LeftUpLeg/ClothCollider")
                                .GetComponent<CapsuleCollider>(),
                            pastOverrideModel.transform.Find("Armature/Hips/RightUpLeg/ClothCollider")
                                .GetComponent<CapsuleCollider>(),
                            pastOverrideModel.transform.Find("Armature/Hips/Spine/Spine1/Spine2/ClothCollider (4)")
                                .GetComponent<CapsuleCollider>(),
                            pastOverrideModel.transform.Find("Armature/Hips/Spine/Spine1/ClothCollider (3)")
                                .GetComponent<CapsuleCollider>()
                        };
                        string chestItem = fashion.ChestItem;
                        string legsItem = fashion.LegsItem;
                        string capeItem = fashion.CapeItem;
                        EquipItemsOnModel(skin, capeItem, capsule);
                        EquipItemsOnModel(skin, chestItem, capsule);
                        EquipItemsOnModel(skin, legsItem, capsule);
                        string hairItem = "Hair" + fashion.HairItem;
                        GameObject hair = EquipItemsOnModel(helmetJoint, hairItem, skin);
                        ColorUtility.TryParseHtmlString(fashion.HairColor, out Color c);
                        if (hair)
                        {
                            Renderer[] componentsInChildren = hair.GetComponentsInChildren<Renderer>();
                            foreach (Renderer render in componentsInChildren)
                                render.material.SetColor(SkinColor, c);
                        }

                        string beardItem = "Beard" + fashion.BeardItem;
                        GameObject beard = EquipItemsOnModel(helmetJoint, beardItem, skin);
                        ColorUtility.TryParseHtmlString(fashion.BeardColor, out Color beardColor);
                        if (beard)
                        {
                            Renderer[] componentsInChildren = beard.GetComponentsInChildren<Renderer>();
                            foreach (Renderer render in componentsInChildren)
                                render.material.SetColor(SkinColor, beardColor);
                        }


                        if (isFemale)
                        {
                            skin.sharedMesh = visuals.m_models[1].m_mesh;
                            skin.materials[0].SetTexture(MainTex,
                                visuals.m_models[1].m_baseMaterial.GetTexture(MainTex));
                            skin.materials[0].SetTexture(SkinBumpMap,
                                visuals.m_models[1].m_baseMaterial.GetTexture(SkinBumpMap));
                        }

                        ColorUtility.TryParseHtmlString(fashion.SkinColor, out Color skinColor);
                        skin.materials[0].SetColor(SkinColor, skinColor);
                    }
                }
            }

            if (pastOverrideModel)
            {
                if (!float.TryParse(fashion.ModelScale, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture,
                        out float val)) val = 1f;
                if (val < 0.1f) val = 0.1f;
                pastOverrideModel.transform.localScale *= val;
            }

            float.TryParse(fashion.TextSize, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture,
                out float textSize);
            float.TryParse(fashion.TextHeight, NumberStyles.Any, CultureInfo.InvariantCulture,
                out float textAdditionalHeight);
            canvas.gameObject.transform.localScale = new Vector3(textSize, textSize, textSize);
            canvas.transform.localPosition = new Vector3(0, 3.4f, 0);
            canvas.transform.localPosition += new Vector3(0, textAdditionalHeight, 0);
        }


        private void OverrideModel(long sender, string newModelName)
        {
            if (znv.IsOwner()) znv.m_zdo.Set("KGnpcModelOverride", newModelName);
            foreach (string typeName in TypeNames)
                transform.Find(typeName).gameObject.SetActive(false);

            CheckNameOnIcons(znv.m_zdo.GetString("KGnpcNameOverride"));
            float KGtextSize = znv.m_zdo.GetFloat("KGtextSize", 3f);
            canvas.transform.localScale = new Vector3(KGtextSize, KGtextSize, KGtextSize);
            canvas.transform.localPosition = new Vector3(0, 3.4f, 0);
            float KGtextDistance = znv.m_zdo.GetFloat("KGtextHeight");
            canvas.transform.localPosition += new Vector3(0, KGtextDistance, 0);

            if (!TryOverrideModel(ref newModelName, out _))
            {
                Transform t = transform.Find(_currentNpcType.ToString());
                t.gameObject.SetActive(true);
                zanim.m_animator = t.GetComponentInChildren<Animator>(true);
            }
        }

        private void OverrideName(long sender, string newName)
        {
            if (znv.IsOwner()) znv.m_zdo.Set("KGnpcNameOverride", newName);
            CheckNameOnIcons(newName);
            transform.Find("MPASNquest").gameObject.SetActive(Quests_DataTypes.Quest.IsQuestTarget(newName));
        }

        private void ChangeProfile(long sender, string profile, string dialogue)
        {
            Utils.print($"Internal change profile set. Profile: {profile} Dialogue: {dialogue}");
            if (znv.IsOwner())
            {
                if (string.IsNullOrWhiteSpace(profile)) profile = "default";
                znv.m_zdo.Set("KGnpcProfile", profile.ToLower());

                if (string.IsNullOrWhiteSpace(dialogue)) dialogue = "";
                znv.m_zdo.Set("KGnpcDialogue", dialogue.ToLower());
            }
        }

        private void SnapAndRotate(long sender, ZPackage pkg)
        {
            Quaternion rotation = pkg.ReadQuaternion();
            ZoneSystem.instance.FindFloor(transform.position, out float height);
            transform.position = new Vector3(transform.position.x, height, transform.position.z);
            transform.rotation = rotation;
            if (znv.IsOwner())
            {
                znv.m_zdo.SetPosition(transform.position);
                znv.m_zdo.SetRotation(transform.rotation);
            }
        }

        private void CleanupInstance(GameObject instance)
        {
            Collider[] componentsInChildren = instance.GetComponentsInChildren<Collider>();
            foreach (Collider collider in componentsInChildren)
                collider.enabled = false;
        }

        private void EnableEquipedEffects(GameObject instance)
        {
            Transform find = instance.transform.Find("equiped");
            if (find) find.gameObject.SetActive(true);
        }

        private void AttachArmor(int itemHash, SkinnedMeshRenderer m_bodyModel, CapsuleCollider[] capsule,
            int variant = -1)
        {
            GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(itemHash);
            if (itemPrefab == null) return;
            int childCount = itemPrefab.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = itemPrefab.transform.GetChild(i);
                if (child.gameObject.name.StartsWith("attach_"))
                {
                    string text = child.gameObject.name.Substring(7);
                    GameObject gameObjectLocal;
                    if (text == "skin")
                    {
                        gameObjectLocal = Instantiate(child.gameObject, m_bodyModel.transform.position,
                            m_bodyModel.transform.parent.rotation, m_bodyModel.transform.parent);
                        gameObjectLocal.SetActive(true);
                        foreach (SkinnedMeshRenderer skinnedMeshRenderer in
                                 gameObjectLocal.GetComponentsInChildren<SkinnedMeshRenderer>())
                        {
                            skinnedMeshRenderer.rootBone = m_bodyModel.rootBone;
                            skinnedMeshRenderer.bones = m_bodyModel.bones;
                        }

                        foreach (Cloth cloth in gameObjectLocal.GetComponentsInChildren<Cloth>())
                            if (cloth.capsuleColliders.Length != 0)
                            {
                                List<CapsuleCollider> list2 = new List<CapsuleCollider>(capsule);
                                list2.AddRange(cloth.capsuleColliders);
                                cloth.capsuleColliders = list2.ToArray();
                            }
                            else
                            {
                                cloth.capsuleColliders = capsule;
                            }
                    }
                    else
                    {
                        Transform transformLocal = global::Utils.FindChild(pastOverrideModel.transform, text);
                        if (transformLocal == null) return;
                        gameObjectLocal = Instantiate(child.gameObject, transformLocal, true);
                        gameObjectLocal.SetActive(true);
                        gameObjectLocal.transform.localPosition = Vector3.zero;
                        gameObjectLocal.transform.localRotation = Quaternion.identity;
                    }

                    if (variant >= 0)
                    {
                        IEquipmentVisual componentInChildren =
                            gameObjectLocal.GetComponentInChildren<IEquipmentVisual>();
                        if (componentInChildren != null) componentInChildren.Setup(variant);
                    }

                    CleanupInstance(gameObjectLocal);
                    EnableEquipedEffects(gameObjectLocal);
                }
            }
        }

        private GameObject AttachItem(int itemHash, int variant, Transform joint, SkinnedMeshRenderer m_bodyModel,
            bool INVERT)
        {
            GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(itemHash);
            if (itemPrefab == null) return null;
            GameObject childGameObject = null;
            int childCount = itemPrefab.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = itemPrefab.transform.GetChild(i);
                if (child.gameObject.name is "attach" or "attach_skin")
                {
                    childGameObject = child.gameObject;
                    break;
                }
            }

            if (childGameObject == null) return null;
            GameObject gameObject2 = Instantiate(childGameObject);
            gameObject2.SetActive(true);
            CleanupInstance(gameObject2);
            EnableEquipedEffects(gameObject2);
            if (childGameObject.name == "attach_skin")
            {
                gameObject2.transform.SetParent(m_bodyModel.transform.parent);
                gameObject2.transform.localPosition = Vector3.zero;
                gameObject2.transform.localRotation = Quaternion.identity;
                if (INVERT && itemPrefab.GetComponent<ItemDrop>().m_itemData.IsWeapon())
                {
                    gameObject2.transform.localPosition = new Vector3(-0.003f, 0.00002f, 0.003f);
                    gameObject2.transform.localEulerAngles = new Vector3(0f, 80f, 0f);
                }

                foreach (SkinnedMeshRenderer skinnedMeshRenderer in gameObject2
                             .GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    skinnedMeshRenderer.rootBone = m_bodyModel.rootBone;
                    skinnedMeshRenderer.bones = m_bodyModel.bones;
                }
            }
            else
            {
                gameObject2.transform.SetParent(joint);
                gameObject2.transform.localPosition = Vector3.zero;
                gameObject2.transform.localRotation = Quaternion.identity;
                if (INVERT && itemPrefab.GetComponent<ItemDrop>().m_itemData.IsWeapon())
                {
                    gameObject2.transform.localPosition = new Vector3(-0.003f, 0.00002f, 0.003f);
                    gameObject2.transform.localEulerAngles = new Vector3(0f, 80f, 0f);
                }
            }

            IEquipmentVisual componentInChildren = gameObject2.GetComponentInChildren<IEquipmentVisual>();
            if (componentInChildren != null) componentInChildren.Setup(variant);
            return gameObject2;
        }


        private GameObject EquipItemsOnModel(Transform joint, string prefab, SkinnedMeshRenderer mesh,
            bool INVERT = false)
        {
            if (joint == null || ObjectDB.instance.GetItemPrefab(prefab.GetStableHashCode()) == null) return null;
            return AttachItem(prefab.GetStableHashCode(), 0, joint, mesh, INVERT);
        }

        private void EquipItemsOnModel(SkinnedMeshRenderer joint, string prefab, CapsuleCollider[] capsule)
        {
            if (joint == null || ObjectDB.instance.GetItemPrefab(prefab.GetStableHashCode()) == null) return;
            ItemDrop item = ObjectDB.instance.GetItemPrefab(prefab.GetStableHashCode()).GetComponent<ItemDrop>();
            if (item == null) return;
            if (item.m_itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Chest)
                if (item.m_itemData.m_shared.m_armorMaterial)
                {
                    joint.material.SetTexture(ChestTex,
                        item.m_itemData.m_shared.m_armorMaterial.GetTexture(ChestTex));
                    joint.material.SetTexture(ChestBumpMap,
                        item.m_itemData.m_shared.m_armorMaterial.GetTexture(ChestBumpMap));
                    joint.material.SetTexture(ChestMetal,
                        item.m_itemData.m_shared.m_armorMaterial.GetTexture(ChestMetal));
                }

            if (item.m_itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Legs)
                if (item.m_itemData.m_shared.m_armorMaterial)
                {
                    joint.material.SetTexture(LegsTex,
                        item.m_itemData.m_shared.m_armorMaterial.GetTexture(LegsTex));
                    joint.material.SetTexture(LegsBumpMap,
                        item.m_itemData.m_shared.m_armorMaterial.GetTexture(LegsBumpMap));
                    joint.material.SetTexture(LegsMetal,
                        item.m_itemData.m_shared.m_armorMaterial.GetTexture(LegsMetal));
                }

            AttachArmor(prefab.GetStableHashCode(), joint, capsule);
        }


        private bool TryOverrideModel(ref string prefab, out bool isFemale, bool EquipItems = true)
        {
            bool overrideHumanoid = false;

            if (pastOverrideModel) Destroy(pastOverrideModel);
            zanim.enabled = false;
            if (prefab.Contains("@humanoid"))
            {
                overrideHumanoid = true;
                prefab = prefab.Replace("@humanoid", "");
            }

            isFemale = false;
            if (prefab == "Player_Female")
            {
                prefab = "Player";
                isFemale = true;
            }

            GameObject original = ZNetScene.instance.GetPrefab(prefab);
            if (!original) return false;
            if (original.GetComponent<Character>() == null)
            {
                Collider col = original.GetComponent<Collider>() != null
                    ? original.GetComponent<Collider>()
                    : original.GetComponentInChildren<Collider>();
                if (col == null) return false;
                pastOverrideModel = new GameObject("OverrideModel")
                {
                    layer = LayerMask.NameToLayer("character"),
                    transform =
                    {
                        parent = transform,
                        localPosition = Vector3.zero,
                        localRotation = Quaternion.identity,
                        localScale = Vector3.one,
                    }
                };


                for (int i = 0; i < original.transform.childCount; ++i)
                {
                    GameObject currentChild = original.transform.GetChild(i).gameObject;
                    Instantiate(currentChild, pastOverrideModel.transform);
                }

                foreach (Collider inChild in pastOverrideModel.GetComponentsInChildren<Collider>())
                {
                    inChild.gameObject.layer = LayerMask.NameToLayer("character");
                }

                if (!pastOverrideModel.GetComponentInChildren<Collider>())
                {
                    Utils.CopyComponent(col, pastOverrideModel);
                }

                if (EquipItems)
                {
                    float val = znv.m_zdo.GetFloat("KGnpcScale", 1f);
                    if (val < 0.1f) val = 0.1f;
                    pastOverrideModel.transform.localScale *= val;
                }
            }
            else
            {
                Collider col = original.GetComponent<Collider>() != null
                    ? original.GetComponent<Collider>()
                    : original.GetComponentInChildren<Collider>();
                if (col == null) return false;
                original = original.GetComponentInChildren<Animator>()?.gameObject;
                if (!original) return false;
                pastOverrideModel = Instantiate(original, transform);
                foreach (ParticleSystem particleSystem in pastOverrideModel.GetComponentsInChildren<ParticleSystem>())
                {
                    ParticleSystem.EmissionModule em = particleSystem.emission;
                    em.enabled = false;
                }

                pastOverrideModel.layer = LayerMask.NameToLayer("character");
                Utils.CopyComponent(col, pastOverrideModel);
                pastOverrideModel.transform.localPosition = Vector3.zero;
                pastOverrideModel.transform.rotation = transform.rotation;
                if (overrideHumanoid)
                {
                    pastOverrideModel.GetComponent<Animator>().runtimeAnimatorController = ZNetScene.instance
                        .GetPrefab("Player").GetComponentInChildren<Animator>().runtimeAnimatorController;
                }

                zanim.m_animator = pastOverrideModel.GetComponent<Animator>();
                zanim.enabled = true;
                pastOverrideModel.GetComponent<Animator>().SetBool(Wakeup, false);
                pastOverrideModel.GetComponent<Animator>()
                    .SetInteger(Crafting, znv.m_zdo.GetInt("KGcraftingAnimation"));
                if (EquipItems)
                {
                    VisEquipment visuals = ZNetScene.instance.GetPrefab(prefab).GetComponent<VisEquipment>();
                    if (visuals)
                    {
                        SkinnedMeshRenderer skin = pastOverrideModel.GetComponentInChildren<SkinnedMeshRenderer>();
                        Transform leftArm =
                            global::Utils.FindChild(pastOverrideModel.transform, visuals.m_leftHand?.name);
                        Transform rightgArm =
                            global::Utils.FindChild(pastOverrideModel.transform, visuals.m_rightHand?.name);
                        Transform helmetJoint =
                            global::Utils.FindChild(pastOverrideModel.transform, visuals.m_helmet?.name);
                        Transform leftBack =
                            global::Utils.FindChild(pastOverrideModel.transform, visuals.m_backShield?.name);
                        Transform rightBack =
                            global::Utils.FindChild(pastOverrideModel.transform, visuals.m_backMelee?.name);
                        string leftItem = znv.m_zdo.GetString("KGleftItem");
                        string rightItem = znv.m_zdo.GetString("KGrightItem");
                        string helmetItem = znv.m_zdo.GetString("KGhelmetItem");
                        EquipItemsOnModel(leftArm, leftItem, skin);
                        EquipItemsOnModel(rightgArm, rightItem, skin);
                        string leftBackItem = znv.m_zdo.GetString("KGLeftItemBack");
                        string rightBackItem = znv.m_zdo.GetString("KGRightItemBack");
                        EquipItemsOnModel(leftArm, leftItem, skin);
                        EquipItemsOnModel(rightgArm, rightItem, skin);
                        EquipItemsOnModel(rightBack, rightBackItem, skin);
                        
                        

                        if (ZNetScene.instance.GetPrefab(leftBackItem))
                        {
                            if (ZNetScene.instance.GetPrefab(leftBackItem).GetComponent<ItemDrop>().m_itemData
                                .IsWeapon())
                            {
                                EquipItemsOnModel(rightBack, leftBackItem, skin, true);
                            }
                            else
                            {
                                EquipItemsOnModel(leftBack, leftBackItem, skin);
                            }
                        }


                        _ = EquipItemsOnModel(helmetJoint, helmetItem, skin) != null;
                        if (prefab == "Player")
                        {
                            if (pastOverrideModel.GetComponent<CustomLookAt>() == null &&
                                znv.m_zdo.GetInt("KGcraftingAnimation") == 0)
                            {
                                pastOverrideModel.AddComponent<CustomLookAt>();
                            }

                            CapsuleCollider[] capsule =
                            {
                                pastOverrideModel.transform.Find("Armature/Hips/ClothCollider")
                                    .GetComponent<CapsuleCollider>(),
                                pastOverrideModel.transform.Find("Armature/Hips/LeftUpLeg/ClothCollider")
                                    .GetComponent<CapsuleCollider>(),
                                pastOverrideModel.transform.Find("Armature/Hips/RightUpLeg/ClothCollider")
                                    .GetComponent<CapsuleCollider>(),
                                pastOverrideModel.transform.Find("Armature/Hips/Spine/Spine1/Spine2/ClothCollider (4)")
                                    .GetComponent<CapsuleCollider>(),
                                pastOverrideModel.transform.Find("Armature/Hips/Spine/Spine1/ClothCollider (3)")
                                    .GetComponent<CapsuleCollider>()
                            };
                            string chestItem = znv.m_zdo.GetString("KGchestItem");
                            string legsItem = znv.m_zdo.GetString("KGlegsItem");
                            string capeItem = znv.m_zdo.GetString("KGcapeItem");
                            string beardItem = "Beard" + znv.m_zdo.GetString("KGbeardItem");
                            EquipItemsOnModel(skin, capeItem, capsule);
                            EquipItemsOnModel(skin, chestItem, capsule);
                            EquipItemsOnModel(skin, legsItem, capsule);
                            string hairItem = "Hair" + znv.m_zdo.GetString("KGhairItem");
                            GameObject hair = EquipItemsOnModel(helmetJoint, hairItem, skin);
                            ColorUtility.TryParseHtmlString(znv.m_zdo.GetString("KGhairItemColor"), out Color c);
                            if (hair)
                            {
                                Renderer[] componentsInChildren = hair.GetComponentsInChildren<Renderer>();
                                foreach (Renderer render in componentsInChildren)
                                    render.material.SetColor(SkinColor, c);
                            }

                            GameObject beard = EquipItemsOnModel(helmetJoint, beardItem, skin);
                            ColorUtility.TryParseHtmlString(znv.m_zdo.GetString("KGbeardColor"), out Color beardColor);
                            if (beard)
                            {
                                Renderer[] componentsInChildren = beard.GetComponentsInChildren<Renderer>();
                                foreach (Renderer render in componentsInChildren)
                                    render.material.SetColor(SkinColor, beardColor);
                            }

                            if (isFemale)
                            {
                                skin.sharedMesh = visuals.m_models[1].m_mesh;
                                skin.materials[0].SetTexture(MainTex,
                                    visuals.m_models[1].m_baseMaterial.GetTexture(MainTex));
                                skin.materials[0].SetTexture(SkinBumpMap,
                                    visuals.m_models[1].m_baseMaterial.GetTexture(SkinBumpMap));
                            }

                            ColorUtility.TryParseHtmlString(znv.m_zdo.GetString("KGskinColor"), out Color skinColor);
                            skin.materials[0].SetColor(SkinColor, skinColor);
                        }
                    }

                    float val = znv.m_zdo.GetFloat("KGnpcScale", 1f);
                    if (val < 0.1f) val = 0.1f;
                    pastOverrideModel.transform.localScale *= val;
                }
            }

            return true;
        }

        private void RemoveNPC(long obj)
        {
            if (znv.IsOwner()) znv.Destroy();
        }


        private void ChangeNpcType(long sende, int index)
        {
            if (_currentNpcType == (NPCType)index) return;
            if (znv.IsOwner()) znv.m_zdo.Set("KGmarketNPC", index);
            _currentNpcType = (NPCType)index;
            OverrideModel(0, znv.m_zdo.GetString("KGnpcModelOverride"));
        }


        private void CheckNameOnIcons(string npcName)
        {
            SpriteRenderer icon = canvas.transform.Find("Icon").GetComponent<SpriteRenderer>();
            icon.gameObject.SetActive(false);
            if (string.IsNullOrWhiteSpace(npcName))
            {
                canvas.text = Localization.instance.Localize("$mpasn_" + _currentNpcType);
            }
            else
            {
                string[] split = npcName.Split(new[] { "<icon>" }, StringSplitOptions.RemoveEmptyEntries);
                canvas.text = split[0];
                if (split.Length == 2)
                {
                    split[1] = split[1].Replace("</icon>", "");
                    if (AssetStorage.AssetStorage.GlobalCachedSprites.ContainsKey(split[1]))
                    {
                        icon.sprite = AssetStorage.AssetStorage.GlobalCachedSprites[split[1]];
                        icon.gameObject.SetActive(true);
                        return;
                    }

                    GameObject prefab = ZNetScene.instance.GetPrefab(split[1]);
                    if (!prefab)
                    {
                        return;
                    }

                    if (prefab.GetComponent<ItemDrop>())
                    {
                        icon.sprite = prefab.GetComponent<ItemDrop>().m_itemData.GetIcon();
                        icon.gameObject.SetActive(true);
                    }

                    if (prefab.GetComponent<Character>())
                    {
                        PhotoManager.__instance.MakeSprite(prefab, 0.6f, 0.25f);
                        icon.sprite = PhotoManager.__instance.GetSprite(prefab.name,
                            AssetStorage.AssetStorage.PlaceholderMonsterIcon, 1);
                        icon.gameObject.SetActive(true);
                    }
                }
            }
        }

        public void Damage(HitData hit)
        {
            if (!znv.IsValid() || !zanim.m_animator) return;
            zanim?.SetTrigger("stagger");
        }

        public DestructibleType GetDestructibleType()
        {
            return DestructibleType.None;
        }
    }

    

    public static class NPCUI
    {
        private static Action _callback;
        private static GameObject UI;
        private static ZDO _currentNPC;
        private static GameObject MAIN;
        private static GameObject FASHION;
        private static Transform NPCTypeButtons;
        private static NPCType _currentType;
        private static InputField _npcprofile;
        private static InputField _npcname;
        private static InputField _npcmodel;
        private static InputField _patroldata;
        private static InputField _npcDialogue;

        private static InputField LeftItemFashion,
            RightItemFashion,
            HelmetItemFashion,
            ChestItemFashion,
            LegsItemFashion,
            RightItemHiddenFashion,
            LeftItemHiddenFashion,
            HairItemFashion,
            SkinColorFashion,
            HairItemFashionColor,
            ModelScaleFashion,
            CapeItemFashion,
            NPCinteractAnimation,
            NPCgreetAnimation,
            NPCgreetText,
            NPCbyeAnimation,
            NPCbyeText,
            NPCcraftingAnimation,
            BeardItemFashion,
            BeardItemFashionColor,
            InteractAudioClip,
            TextSize,
            TextHeight,
            PeriodicAnimation,
            PeriodicAnimationTime,
            PeriodicSound,
            PeriodicSoundTime;

        public static void Init()
        {
            UI = Object.Instantiate(AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("MarketplaceNPCUI"));
            Object.DontDestroyOnLoad(UI);
            UI.SetActive(false);
            NPCTypeButtons = UI.transform.Find("Canvas/MAIN/Pergament/NPCTYPE");
            MAIN = UI.transform.Find("Canvas/MAIN").gameObject;
            FASHION = UI.transform.Find("Canvas/FASHION").gameObject;
            _npcprofile = UI.transform.Find("Canvas/MAIN/Pergament/NPCPROFILE").GetComponent<InputField>();
            _npcname = UI.transform.Find("Canvas/MAIN/Pergament/NPCNAME").GetComponent<InputField>();
            _npcmodel = UI.transform.Find("Canvas/MAIN/Pergament/NPCMODEL").GetComponent<InputField>();
            _patroldata = UI.transform.Find("Canvas/MAIN/Pergament/PATROLDATA").GetComponent<InputField>();
            _npcDialogue = UI.transform.Find("Canvas/MAIN/Pergament/NPCDIALOGUE").GetComponent<InputField>();
            MAIN.SetActive(false);
            FASHION.SetActive(false);
            Localization.instance.Localize(UI.transform);

            int childCount = NPCTypeButtons.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = NPCTypeButtons.GetChild(i);
                int i1 = i;
                child.GetComponent<Button>().onClick.AddListener(() =>
                {
                    AssetStorage.AssetStorage.AUsrc.Play();
                    _currentType = (NPCType)i1;
                    CheckColors();
                });
            }

            UI.transform.Find("Canvas/MAIN/Pergament/Apply").GetComponent<Button>().onClick.AddListener(ApplyMain);
            UI.transform.Find("Canvas/MAIN/Pergament/Snap").GetComponent<Button>().onClick.AddListener(Snap);

            /* FASHION */
            LeftItemFashion = UI.transform.Find("Canvas/FASHION/Pergament/LeftItemFashion").GetComponent<InputField>();
            RightItemFashion = UI.transform.Find("Canvas/FASHION/Pergament/RightItemFashion")
                .GetComponent<InputField>();
            HelmetItemFashion = UI.transform.Find("Canvas/FASHION/Pergament/HelmetItemFashion")
                .GetComponent<InputField>();
            ChestItemFashion = UI.transform.Find("Canvas/FASHION/Pergament/ChestItemFashion")
                .GetComponent<InputField>();
            LegsItemFashion = UI.transform.Find("Canvas/FASHION/Pergament/LegsItemFashion").GetComponent<InputField>();
            RightItemHiddenFashion = UI.transform.Find("Canvas/FASHION/Pergament/RightItemHiddenFashion")
                .GetComponent<InputField>();
            LeftItemHiddenFashion = UI.transform.Find("Canvas/FASHION/Pergament/LeftItemHiddenFashion")
                .GetComponent<InputField>();
            HairItemFashion = UI.transform.Find("Canvas/FASHION/Pergament/HairItemFashion").GetComponent<InputField>();
            SkinColorFashion = UI.transform.Find("Canvas/FASHION/Pergament/SkinColorFashion")
                .GetComponent<InputField>();
            HairItemFashionColor = UI.transform.Find("Canvas/FASHION/Pergament/HairItemFashionColor")
                .GetComponent<InputField>();
            ModelScaleFashion = UI.transform.Find("Canvas/FASHION/Pergament/ModelScaleFashion")
                .GetComponent<InputField>();
            CapeItemFashion = UI.transform.Find("Canvas/FASHION/Pergament/CapeItemFashion").GetComponent<InputField>();
            NPCinteractAnimation = UI.transform.Find("Canvas/FASHION/Pergament/NPCinteractAnimation")
                .GetComponent<InputField>();
            NPCgreetAnimation = UI.transform.Find("Canvas/FASHION/Pergament/NPCgreetAnimation")
                .GetComponent<InputField>();
            NPCgreetText = UI.transform.Find("Canvas/FASHION/Pergament/NPCgreetText").GetComponent<InputField>();
            NPCbyeAnimation = UI.transform.Find("Canvas/FASHION/Pergament/NPCbyeAnimation").GetComponent<InputField>();
            NPCbyeText = UI.transform.Find("Canvas/FASHION/Pergament/NPCbyeText").GetComponent<InputField>();
            NPCcraftingAnimation = UI.transform.Find("Canvas/FASHION/Pergament/NPCcraftingAnimation")
                .GetComponent<InputField>();
            BeardItemFashion = UI.transform.Find("Canvas/FASHION/Pergament/BeardItemFashion")
                .GetComponent<InputField>();
            BeardItemFashionColor = UI.transform.Find("Canvas/FASHION/Pergament/BeardItemFashionColor")
                .GetComponent<InputField>();
            InteractAudioClip = UI.transform.Find("Canvas/FASHION/Pergament/InteractAudioClip")
                .GetComponent<InputField>();
            TextSize = UI.transform.Find("Canvas/FASHION/Pergament/TextSize").GetComponent<InputField>();
            TextHeight = UI.transform.Find("Canvas/FASHION/Pergament/TextHeight").GetComponent<InputField>();
            PeriodicAnimation = UI.transform.Find("Canvas/FASHION/Pergament/PeriodicAnimation")
                .GetComponent<InputField>();
            PeriodicAnimationTime = UI.transform.Find("Canvas/FASHION/Pergament/PeriodicAnimationTime")
                .GetComponent<InputField>();
            PeriodicSound = UI.transform.Find("Canvas/FASHION/Pergament/PeriodicSound").GetComponent<InputField>();
            PeriodicSoundTime = UI.transform.Find("Canvas/FASHION/Pergament/PeriodicSoundTime")
                .GetComponent<InputField>();
            UI.transform.Find("Canvas/FASHION/Pergament/Apply").GetComponent<Button>().onClick
                .AddListener(ApplyFashion);
        }

        private static void ApplyFashion()
        {
            AssetStorage.AssetStorage.AUsrc.Play();
            if (_currentNPC == null || !_currentNPC.IsValid())
            {
                Hide();
                return;
            }

            NPC_DataTypes.NPC_Fashion fashion = new()
            {
                LeftItem = LeftItemFashion.text,
                RightItem = RightItemFashion.text,
                HelmetItem = HelmetItemFashion.text,
                ChestItem = ChestItemFashion.text,
                LegsItem = LegsItemFashion.text,
                CapeItem = CapeItemFashion.text,
                HairItem = HairItemFashion.text,
                HairColor = HairItemFashionColor.text,
                ModelScale = ModelScaleFashion.text,
                LeftItemHidden = LeftItemHiddenFashion.text,
                RightItemHidden = RightItemHiddenFashion.text,
                InteractAnimation = NPCinteractAnimation.text,
                GreetAnimation = NPCgreetAnimation.text,
                ByeAnimation = NPCbyeAnimation.text,
                GreetText = NPCgreetText.text,
                ByeText = NPCbyeText.text,
                SkinColor = SkinColorFashion.text,
                CraftingAnimation = NPCcraftingAnimation.text,
                BeardItem = BeardItemFashion.text,
                BeardColor = BeardItemFashionColor.text,
                InteractAudioClip = InteractAudioClip.text,
                TextSize = TextSize.text,
                TextHeight = TextHeight.text,
                PeriodicAnimation = PeriodicAnimation.text,
                PeriodicAnimationTime = PeriodicAnimationTime.text,
                PeriodicSound = PeriodicSound.text,
                PeriodicSoundTime = PeriodicSoundTime.text
            };
            if (_currentNPC.HasOwner())
            {
                ZRoutedRpc.instance.InvokeRoutedRPC(ZNetView.Everybody, _currentNPC.m_uid, "KGmarket fashion", fashion);
            }
            else
            {
                _currentNPC.NPC_LeftItem(fashion.LeftItem);
                _currentNPC.NPC_RightItem(fashion.RightItem);
                _currentNPC.NPC_HelmetItem(fashion.HelmetItem);
                _currentNPC.NPC_ChestItem(fashion.ChestItem);
                _currentNPC.NPC_LegsItem(fashion.LegsItem);
                _currentNPC.NPC_CapeItem(fashion.CapeItem);
                _currentNPC.NPC_HairItem(fashion.HairItem);
                _currentNPC.NPC_HairColor(fashion.HairColor);
                _currentNPC.NPC_NPCScale(fashion.ModelScale);
                _currentNPC.NPC_LeftItemBack(fashion.LeftItemHidden);
                _currentNPC.NPC_RightItemBack(fashion.RightItemHidden);
                _currentNPC.NPC_InteractAnimation(fashion.InteractAnimation);
                _currentNPC.NPC_GreetingAnimation(fashion.GreetAnimation);
                _currentNPC.NPC_ByeAnimation(fashion.ByeAnimation);
                _currentNPC.NPC_GreetingText(fashion.GreetText);
                _currentNPC.NPC_ByeText(fashion.ByeText);
                _currentNPC.NPC_SkinColor(fashion.SkinColor);
                _currentNPC.NPC_CraftingAnimation(fashion.CraftingAnimation);
                _currentNPC.NPC_BeardItem(fashion.BeardItem);
                _currentNPC.NPC_BeardColor(fashion.BeardColor);
                _currentNPC.NPC_InteractSound(fashion.InteractAudioClip);
                _currentNPC.NPC_TextSize(fashion.TextSize);
                _currentNPC.NPC_TextHeight(fashion.TextHeight);
                _currentNPC.NPC_PeriodicAnimation(fashion.PeriodicAnimation);
                _currentNPC.NPC_PeriodicAnimationTime(fashion.PeriodicAnimationTime);
                _currentNPC.NPC_PeriodicSound(fashion.PeriodicSound);
                _currentNPC.NPC_PeriodicSoundTime(fashion.PeriodicSoundTime);
            }
            _callback?.Invoke();
            Hide();
        }

        private static void Snap()
        {
            AssetStorage.AssetStorage.AUsrc.Play();
            ZPackage pkg = new ZPackage();
            Quaternion rot = Quaternion.LookRotation(GameCamera.instance.transform.forward);
            rot.x = 0;
            rot.z = 0;
            pkg.Write(rot);
            ZRoutedRpc.instance.InvokeRoutedRPC(ZNetView.Everybody, _currentNPC.m_uid, "KGmarket snapandrotate", pkg);
            Hide();
        }

        private static void ApplyMain()
        {
            AssetStorage.AssetStorage.AUsrc.Play();
            if (_currentNPC == null || !_currentNPC.IsValid())
            {
                Hide();
                return;
            }

            if (_currentNPC.HasOwner())
            {
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, _currentNPC.m_uid, "KGMarket changeNpcType", (int)_currentType);
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, _currentNPC.m_uid, "KGmarket overridename", _npcname.text);
                ZRoutedRpc.instance.InvokeRoutedRPC(_currentNPC.GetOwner(),_currentNPC.m_uid, "KGmarket changeprofile",  _npcprofile.text, _npcDialogue.text);
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody,_currentNPC.m_uid, "KGmarket overridemodel",  _npcmodel.text);
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody,_currentNPC.m_uid, "KGmarket GetPatrolData",  _patroldata.text);
            }
            else
            {
                _currentNPC.NPC_Type((API.Marketplace_API.NPCType)_currentType);
                _currentNPC.NPC_Name(_npcname.text);
                _currentNPC.NPC_Profile(_npcprofile.text);
                _currentNPC.NPC_Dialogue(_npcDialogue.text);
                _currentNPC.NPC_Model(_npcmodel.text);
                _currentNPC.NPC_PatrolData(_patroldata.text);
            }
            _callback?.Invoke();
            Hide();
        }

        public static bool IsVisible()
        {
            return UI != null && UI.activeSelf;
        }

        public static void Hide()
        {
            UI.SetActive(false);
        }

        private static void CheckColors()
        {
            int childs = NPCTypeButtons.childCount;
            for (int i = 0; i < childs; i++)
            {
                Transform child = NPCTypeButtons.GetChild(i);
                child.GetComponent<Image>().color = i == (int)_currentType ? Color.green : Color.white;
            }
        }

        public static void ShowFashion(ZDO _npc, Action callback = null)
        {
            FASHION.SetActive(true);
            MAIN.SetActive(false);
            _currentNPC = _npc;
            LeftItemFashion.text = _npc.GetString("KGleftItem");
            LeftItemHiddenFashion.text = _npc.GetString("KGLeftItemBack");
            RightItemHiddenFashion.text = _npc.GetString("KGRightItemBack");
            RightItemFashion.text = _npc.GetString("KGrightItem");
            HelmetItemFashion.text = _npc.GetString("KGhelmetItem");
            ChestItemFashion.text = _npc.GetString("KGchestItem");
            LegsItemFashion.text = _npc.GetString("KGlegsItem");
            HairItemFashion.text = _npc.GetString("KGhairItem");
            HairItemFashionColor.text = _npc.GetString("KGhairItemColor");
            CapeItemFashion.text = _npc.GetString("KGcapeItem");
            ModelScaleFashion.text = _npc.GetFloat("KGnpcScale", 1f).ToString(CultureInfo.InvariantCulture);
            NPCinteractAnimation.text = _npc.GetString("KGinteractAnimation");
            NPCgreetAnimation.text = _npc.GetString("KGgreetingAnimation");
            NPCbyeAnimation.text = _npc.GetString("KGbyeAnimation");
            NPCgreetText.text = _npc.GetString("KGgreetingText");
            NPCbyeText.text = _npc.GetString("KGbyeText");
            SkinColorFashion.text = _npc.GetString("KGskinColor");
            NPCcraftingAnimation.text = _npc.GetInt("KGcraftingAnimation").ToString();
            BeardItemFashion.text = _npc.GetString("KGbeardItem");
            BeardItemFashionColor.text = _npc.GetString("KGbeardColor");
            InteractAudioClip.text = _npc.GetString("KGinteractSound");
            TextSize.text = _npc.GetFloat("KGtextSize", 3).ToString(CultureInfo.InvariantCulture);
            TextHeight.text = _npc.GetFloat("KGtextHeight").ToString(CultureInfo.InvariantCulture);
            PeriodicAnimation.text = _npc.GetString("KGperiodicAnimation");
            PeriodicAnimationTime.text = _npc.GetFloat("KGperiodicAnimationTime")
                .ToString(CultureInfo.InvariantCulture);
            PeriodicSound.text = _npc.GetString("KGperiodicSound");
            PeriodicSoundTime.text =
                _npc.GetFloat("KGperiodicSoundTime").ToString(CultureInfo.InvariantCulture);
            UI.SetActive(true);
            _callback = callback;
        }

        public static void ShowMain(ZDO _npc, Action callback = null)
        {
            FASHION.SetActive(false);
            MAIN.SetActive(true);
            int npcType = _npc.GetInt("KGmarketNPC");
            if (npcType >= TypeNames.Length) npcType = 0;
            _currentType = (NPCType)npcType;
            _npcprofile.text = _npc.GetString("KGnpcProfile", "default");
            _npcname.text = _npc.GetString("KGnpcNameOverride");
            _npcmodel.text = _npc.GetString("KGnpcModelOverride");
            _patroldata.text = _npc.GetString("KGmarket PatrolData");
            _npcDialogue.text = _npc.GetString("KGnpcDialogue");
            _currentNPC = _npc;
            CheckColors();
            UI.SetActive(true);
            _callback = callback;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.PlacePiece))]
    [ClientOnlyPatch]
    private static class DebugModeBuildCheck
    {
        private static bool Prefix(Piece piece)
        {
            if (piece.GetComponent<NPCcomponent>() && !Utils.IsDebug)
            {
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center,
                    $"<color=#00ff00>{Localization.instance.Localize("$mpasn_enabledebugmode")}</color>");
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(PieceTable), nameof(PieceTable.UpdateAvailable))]
    [ClientOnlyPatch]
    private static class PieceTable_UpdateAvailable_Patch
    {
        private static void Postfix(PieceTable __instance)
        {
            if (__instance.m_availablePieces.Count == 0) return;
            List<Piece> avaliablePieces = __instance.m_availablePieces[(int)Piece.PieceCategory.Misc];
            if (Utils.IsDebug)
            {
                if (!avaliablePieces.Contains(NPC.GetComponent<Piece>()))
                    avaliablePieces.Add(NPC.GetComponent<Piece>());
                if (!avaliablePieces.Contains(PinnedNPC.GetComponent<Piece>()))
                    avaliablePieces.Add(PinnedNPC.GetComponent<Piece>());
            }
            else
            {
                if (avaliablePieces.Contains(NPC.GetComponent<Piece>()))
                    avaliablePieces.Remove(NPC.GetComponent<Piece>());
                if (avaliablePieces.Contains(PinnedNPC.GetComponent<Piece>()))
                    avaliablePieces.Remove(PinnedNPC.GetComponent<Piece>());
            }
        }
    }
}
using Marketplace.Modules.Banker;
using Marketplace.Modules.Buffer;
using Marketplace.Modules.Feedback;
using Marketplace.Modules.Gambler;
using Marketplace.Modules.Marketplace_NPC;
using Marketplace.Modules.Quests;
using Marketplace.Modules.ServerInfo;
using Marketplace.Modules.Teleporter;
using Marketplace.Modules.Trader;
using Marketplace.Modules.Transmogrification;
using UnityEngine.EventSystems;
using Image = UnityEngine.UI.Image;
using Object = UnityEngine.Object;

namespace Marketplace.Modules.NPC;

[Market_Autoload(Market_Autoload.Type.Both, Market_Autoload.Priority.Normal, "OnInit")]
public static class Market_NPC
{
    private static bool EnableAI;
    private static GameObject NPC;
    private static GameObject PinnedNPC;
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

    public enum NPCType
    {
        Marketplace,
        Trader,
        Info,
        Teleporter,
        Feedback,
        Banker,
        Gambler,
        Quests,
        Buffer,
        Transmog,
        None
    }

    // ReSharper disable once UnusedMember.Global
    private static void OnInit()
    {
        NPC = AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("MarketPlaceNPC");
        NPC.AddComponent<NPCcomponent>();
        NPC.transform.Find("TMP").gameObject.AddComponent<TextComponent>();
        if (Utils.IsServer) return;
        NPCUI.Init();
        NPCLoader_UI.Init();
        Marketplace.Global_Updator += UpdateNPCGUI;
    }

    private static void UpdateNPCGUI()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && NPCLoader_UI.IsVisible())
        {
            NPCLoader_UI.Hide();
            Menu.instance.OnClose();
            return;
        }

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
                GameObject hammer = __instance.GetPrefab("Hammer");
                hammer.GetComponent<ItemDrop>().m_itemData.m_shared.m_buildPieces.m_pieces.Add(NPC);
                GameObject inactive = new GameObject("InactiveMPASN");
                Object.DontDestroyOnLoad(inactive);
                inactive.SetActive(false);
                PinnedNPC = Object.Instantiate(NPC, inactive.transform);
                PinnedNPC.name = "MarketPlaceNPCpinned";
                hammer.GetComponent<ItemDrop>().m_itemData.m_shared.m_buildPieces.m_pieces.Add(PinnedNPC);
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
            odin.transform.Find("visual").gameObject.AddComponent<Animator>();
        }
    }

    [HarmonyPatch(typeof(Menu), nameof(Menu.IsVisible))]
    [ClientOnlyPatch]
    private static class IsVisiblePatch
    {
        private static void Postfix(ref bool __result)
        {
            if (NPCLoader_UI.IsVisible() || NPCUI.IsVisible()) __result = true;
        }
    }

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.InputText))]
    [ClientOnlyPatch]
    private static class ConsolePatch
    {
        private static bool Prefix(Terminal __instance)
        {
            if (__instance.m_input.text.ToLower() == "pos")
            {
                __instance.AddString($"Position: {Player.m_localPlayer.transform.position}");
                return false;
            }

            if (__instance.m_input.text.ToLower() == "mpasn ai")
            {
                EnableAI = !EnableAI;
                __instance.AddString($"AI: {EnableAI}");
                return false;
            }

            if (__instance.m_input.text.ToLower() == "idm")
            {
                ItemDrop.ItemData weapon = Player.m_localPlayer.GetRightItem() != null
                    ? Player.m_localPlayer.GetRightItem()
                    : Player.m_localPlayer.GetLeftItem();
                if (weapon == null)
                {
                    __instance.AddString("No weapon in hand");
                    return false;
                }

                string IDM = JSON.ToNiceJSON(weapon.m_customData);
                __instance.AddString($"IDM for {weapon.m_dropPrefab}:\n{IDM}");
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Chat), nameof(Chat.InputText))]
    [ClientOnlyPatch]
    private static class Chat_InputText_Patch_AdminCommands
    {
        private static bool Prefix(Chat __instance)
        {
            if (__instance.m_input.text.ToLower() == "/npc remove" && Utils.IsDebug)
            {
                IEnumerable<NPCcomponent> FindNPCsInRange = NPCcomponent.ALL.Where(x =>
                    global::Utils.DistanceXZ(Player.m_localPlayer.transform.position, x.transform.position) <= 5f);
                int c = 0;
                string total = "";
                foreach (NPCcomponent npc in FindNPCsInRange)
                {
                    ++c;
                    string name = npc.GetNPCName();
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        name = Localization.instance.Localize("$mpasn_" + npc._currentNpcType);
                    }

                    total += $"\n{c + 1}) {name} {npc.transform.position}";
                    ZDOMan.instance.m_destroySendList.Add(npc.znv.m_zdo.m_uid);
                }

                __instance.AddString($"Removed total {c} NPCs in range:{total}");
                return false;
            }

            if (__instance.m_input.text.ToLower() == "/npc save" && Utils.IsDebug)
            {
                foreach (NPCcomponent npc in NPCcomponent.ALL)
                {
                    if (!npc.pastOverrideModel) continue;
                    NPCLoader_UI.Save(npc, out string text);
                    __instance.AddString(text);
                }
            }

            return true;
        }
    }

    private class TextComponent : MonoBehaviour
    {
        private Transform t;

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
                if (Cache.TryGetValue(_name, out var value))
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

        private void Awake()
        {
            animator = GetComponent<Animator>();
            currentLookAt = transform.position + transform.forward * 4f + Vector3.up * 1.2f;
        }

        private void OnAnimatorIK(int layerIndex)
        {
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
                animator.SetLookAtWeight(1f, 0.5f, 1f, 0f);
            }
        }
    }


    public class NPCcomponent : MonoBehaviour, Hoverable, Interactable, IDestructible
    {
        private const string PatrolData = "KGmarket PatrolData";
        private const string LatestPatrolPoint = "KGmarket LatestPatrolPoint";
        private const string GoBackPatrol = "KGmarket GoBack";
        private readonly string[] TypeNames = Enum.GetNames(typeof(NPCType));

        public static readonly List<NPCcomponent> ALL = new();
        public ZNetView znv;
        public GameObject pastOverrideModel;
        public NPCType _currentNpcType;

        private ZSyncAnimation zanim;
        private TMP_Text canvas;
        private AudioSource NPC_SoundSource;
        private bool WasClose;
        private Vector2[] PatrolArray;
        private float MaxSpeed;
        private float ForwardSpeed;
        private float PatrolTime;
        private float periodicAnimationTimer;

        private void OnDestroy()
        {
            ALL.Remove(this);
        }

        private void InitPatrolData(string ForceData)
        {
            ForwardSpeed = 0f;
            if (zanim.enabled)
                zanim.SetFloat(Character.forward_speed, 0);
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

        private void Update()
        {
            PatrolTime += Time.deltaTime;
            if (znv.m_zdo == null || !znv.IsOwner() || PatrolArray is not { Length: > 1 } || PatrolTime < 2f) return;
            Player closePlayer = Player.GetClosestPlayer(transform.position, 3f);
            ForwardSpeed = closePlayer && _currentNpcType is not NPCType.None
                ? Mathf.Clamp(ForwardSpeed - Time.deltaTime, 0, MaxSpeed)
                : Mathf.Clamp(ForwardSpeed + Time.deltaTime, 0, MaxSpeed);
            if (zanim.enabled)
                zanim.SetFloat(Character.forward_speed, ForwardSpeed * 1.5f);
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

        private void FixedUpdate()
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
                    if (pastOverrideModel && !string.IsNullOrWhiteSpace(znv.m_zdo.GetString("KGperiodicAnimation")))
                    {
                        pastOverrideModel.GetComponent<Animator>()
                            ?.SetTrigger(znv.m_zdo.GetString("KGperiodicAnimation"));
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

                        if (pastOverrideModel &&
                            !string.IsNullOrWhiteSpace(znv.m_zdo.GetString("KGgreetingAnimation")))
                        {
                            pastOverrideModel.GetComponent<Animator>()
                                ?.SetTrigger(znv.m_zdo.GetString("KGgreetingAnimation"));
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

                        if (pastOverrideModel && !string.IsNullOrWhiteSpace(znv.m_zdo.GetString("KGbyeAnimation")))
                        {
                            pastOverrideModel.GetComponent<Animator>()
                                ?.SetTrigger(znv.m_zdo.GetString("KGbyeAnimation"));
                        }
                    }
                }
            }
        }

        private void Awake()
        {
            znv = GetComponent<ZNetView>();
            if (!znv || znv.m_zdo == null || Utils.IsServer) return;
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
            int npcType = znv.m_zdo.GetInt("KGmarketNPC", (int)NPCType.None);
            if (npcType >= TypeNames.Length) npcType = 0;
            _currentNpcType = (NPCType)npcType;
            if (!znv.m_functions.ContainsKey("KGMarket changeNpcType".GetStableHashCode()))
            {
                znv.Register("KGMarket changeNpcType", new Action<long, int>(ChangeNpcType));
                znv.Register("KGMarket removeNpc", RemoveNPC);
                znv.Register("KGmarket snapandrotate", new Action<long, ZPackage>(SnapAndRotate));
                znv.Register("KGmarket changeprofile", new Action<long, string>(ChangeProfile));
                znv.Register("KGmarket overridename", new Action<long, string>(OverrideName));
                znv.Register("KGmarket overridemodel", new Action<long, string>(OverrideModel));
                znv.Register("KGmarket fashion", new Action<long, string>(FashionApply));
                znv.Register("KGmarket GetDamage", PlayStaggerAnimation);
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
            string admintext = Utils.IsDebug
                ? "\n" + Localization.instance.Localize("[<color=yellow><b>$KEY_AltPlace + $KEY_Use</b></color>]") +
                  " " + Localization.instance.Localize("$mpasn_opennnpcui") +
                  "\n" + Localization.instance.Localize("[<color=yellow><b>ALT + $KEY_Use</b></color>]") +
                  " " + Localization.instance.Localize("$mpasn_fashionmenu") +
                  "\n" + Localization.instance.Localize("[<color=yellow><b>C + $KEY_Use</b></color>]") +
                  " " + Localization.instance.Localize("$mpasn_npcsaveload") +
                  "\n" + Localization.instance.Localize("[<color=red><b>DELETE + $KEY_Use</b></color>]") +
                  " " + Localization.instance.Localize("$mpasn_removenpc")
                : "";
            string text = _currentNpcType switch
            {
                NPCType.Marketplace => Localization.instance.Localize("[<color=yellow><b>$KEY_Use</b></color>]") +
                                       " " + Localization.instance.Localize("$mpasn_openmarketplace"),
                NPCType.Info => Localization.instance.Localize("[<color=yellow><b>$KEY_Use</b></color>]") + " " +
                                Localization.instance.Localize("$mpasn_openinfo"),
                NPCType.Teleporter => Localization.instance.Localize("[<color=yellow><b>$KEY_Use</b></color>]") +
                                      " " + Localization.instance.Localize("$mpasn_openteleporthub"),
                NPCType.Feedback => Localization.instance.Localize("[<color=yellow><b>$KEY_Use</b></color>]") +
                                    " " + Localization.instance.Localize("$mpasn_openfeedback"),
                NPCType.Trader => Localization.instance.Localize("[<color=yellow><b>$KEY_Use</b></color>]") +
                                  " " + Localization.instance.Localize("$mpasn_opentrader"),
                NPCType.Banker => Localization.instance.Localize("[<color=yellow><b>$KEY_Use</b></color>]") +
                                  " " + Localization.instance.Localize("$mpasn_openbanker"),
                NPCType.Gambler => Localization.instance.Localize("[<color=yellow><b>$KEY_Use</b></color>]") +
                                   " " + Localization.instance.Localize("$mpasn_opengambler"),
                NPCType.Quests => Localization.instance.Localize("[<color=yellow><b>$KEY_Use</b></color>]") +
                                  " " + Localization.instance.Localize("$mpasn_openquests"),
                NPCType.Buffer => Localization.instance.Localize("[<color=yellow><b>$KEY_Use</b></color>]") +
                                  " " + Localization.instance.Localize("$mpasn_openbuffer"),
                NPCType.Transmog => Localization.instance.Localize("[<color=yellow><b>$KEY_Use</b></color>]") +
                                    " " + Localization.instance.Localize("$mpasn_opentransmog"),
                _ => ""
            };
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

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            if (pastOverrideModel && !string.IsNullOrWhiteSpace(znv.m_zdo.GetString("KGinteractAnimation")))
            {
                pastOverrideModel.GetComponent<Animator>()?.SetTrigger(znv.m_zdo.GetString("KGinteractAnimation"));
            }

            string interactAudio = znv.m_zdo.GetString("KGinteractSound");
            NPC_SoundSource.Stop();
            if (!string.IsNullOrEmpty(interactAudio) &&
                AssetStorage.AssetStorage.NPC_AudioClips.TryGetValue(interactAudio, out AudioClip interactClip))
            {
                NPC_SoundSource.clip = interactClip;
                NPC_SoundSource.Play();
            }

            if (Input.GetKey(KeyCode.C) && Utils.IsDebug)
            {
                NPCLoader_UI.Show(this);
                return true;
            }

            if (Input.GetKey(KeyCode.LeftAlt) && Utils.IsDebug)
            {
                NPCUI.ShowFashion(this);
                return true;
            }

            if (alt && Utils.IsDebug)
            {
                NPCUI.ShowMain(this);
                return true;
            }

            if (Input.GetKey(KeyCode.Delete) && Utils.IsDebug)
            {
                znv.InvokeRPC("KGMarket removeNpc");
                return true;
            }

            if (Quests_DataTypes.Quest.TryAddRewardTalk(GetNPCName()))
            {
                Quests_UIs.AcceptedQuestsUI.CheckQuests();
            }


            switch (_currentNpcType)
            {
                case NPCType.Marketplace:
                    if (!string.IsNullOrWhiteSpace(Global_Values._localUserID))
                        Marketplace_UI.Show();
                    break;
                case NPCType.Info:
                    ServerInfo_UI.Show(znv.m_zdo.GetString("KGnpcProfile", "default"),
                        znv.m_zdo.GetString("KGnpcNameOverride"));
                    break;
                case NPCType.Trader:
                    Trader_UI.Show(znv.m_zdo.GetString("KGnpcProfile", "default"),
                        gameObject,
                        znv.m_zdo.GetString("KGnpcNameOverride"));
                    break;
                case NPCType.Banker:
                    Banker_UI.Show(znv.m_zdo.GetString("KGnpcProfile", "default"),
                        znv.m_zdo.GetString("KGnpcNameOverride"));
                    break;
                case NPCType.Teleporter:
                    Teleporter_Main_Client.ShowTeleporterUI(znv.m_zdo.GetString("KGnpcProfile", "default"));
                    break;
                case NPCType.Feedback:
                    Feedback_UI.Show();
                    break;
                case NPCType.Gambler:
                    Gambler_UI.Show(znv.m_zdo.GetString("KGnpcProfile", "default"));
                    break;
                case NPCType.Quests:
                    Quests_UIs.QuestUI.Show(znv.m_zdo.GetString("KGnpcProfile", "default"),
                        znv.m_zdo.GetString("KGnpcNameOverride"));
                    break;
                case NPCType.Buffer:
                    Buffer_UI.Show(znv.m_zdo.GetString("KGnpcProfile", "default"),
                        znv.m_zdo.GetString("KGnpcNameOverride"));
                    break;
                case NPCType.Transmog:
                    Transmogrification_UI.Show(znv.m_zdo.GetString("KGnpcProfile", "default"),
                        znv.m_zdo.GetString("KGnpcNameOverride"));
                    break;
                default:
                    return false;
            }

            return true;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }

        private void FashionApply(long sender, string data)
        {
            string[] split = data.Split('|');
            if (znv.m_zdo.IsOwner())
            {
                znv.m_zdo.Set("KGleftItem", split[0]);
                znv.m_zdo.Set("KGrightItem", split[1]);
                znv.m_zdo.Set("KGhelmetItem", split[2]);
                znv.m_zdo.Set("KGchestItem", split[3]);
                znv.m_zdo.Set("KGlegsItem", split[4]);
                znv.m_zdo.Set("KGcapeItem", split[5]);
                znv.m_zdo.Set("KGhairItem", split[6]);
                znv.m_zdo.Set("KGhairItemColor", split[7]);
                znv.m_zdo.Set("KGLeftItemBack", split[9]);
                znv.m_zdo.Set("KGRightItemBack", split[10]);
                znv.m_zdo.Set("KGinteractAnimation", split[11]);
                znv.m_zdo.Set("KGgreetingAnimation", split[12]);
                znv.m_zdo.Set("KGbyeAnimation", split[13]);
                znv.m_zdo.Set("KGgreetingText", split[14]);
                znv.m_zdo.Set("KGbyeText", split[15]);
                znv.m_zdo.Set("KGskinColor", split[16]);
                znv.m_zdo.Set("KGcraftingAnimation", int.TryParse(split[17], out int craftingZDO) ? craftingZDO : 0);
                znv.m_zdo.Set("KGbeardItem", split[18]);
                znv.m_zdo.Set("KGbeardColor", split[19]);
                znv.m_zdo.Set("KGinteractSound", split[20]);
                znv.m_zdo.Set("KGtextSize",
                    float.TryParse(split[22], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture,
                        out float textSizeZDO)
                        ? textSizeZDO
                        : 3);
                znv.m_zdo.Set("KGtextHeight",
                    float.TryParse(split[23], NumberStyles.Any, CultureInfo.InvariantCulture,
                        out float textHeightZDO)
                        ? textHeightZDO
                        : 0f);
                znv.m_zdo.Set("KGperiodicAnimation", split[24]);
                znv.m_zdo.Set("KGperiodicAnimationTime",
                    float.TryParse(split[25], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture,
                        out float periodicAnimationTimeZDO)
                        ? periodicAnimationTimeZDO
                        : 0f);
                if (!float.TryParse(split[8], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture,
                        out float scaleFloat)) scaleFloat = 1f;

                znv.m_zdo.Set("KGnpcScale", scaleFloat);
            }

            periodicAnimationTimer = 0f;
            string prefab = znv.m_zdo.GetString("KGnpcModelOverride");
            if (TryOverrideModel(ref prefab, out bool isFemale, false))
            {
                pastOverrideModel.GetComponent<Animator>()?.SetBool(Wakeup, false);
                if (int.TryParse(split[17], out int CRAFTING))
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
                    string leftItem = split[0];
                    string rightItem = split[1];
                    string helmetItem = split[2];
                    string leftBackItem = split[9];
                    string rightBackItem = split[10];
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
                        string chestItem = split[3];
                        string legsItem = split[4];
                        string capeItem = split[5];
                        EquipItemsOnModel(skin, capeItem, capsule);
                        EquipItemsOnModel(skin, chestItem, capsule);
                        EquipItemsOnModel(skin, legsItem, capsule);
                        string hairItem = "Hair" + split[6];
                        GameObject hair = EquipItemsOnModel(helmetJoint, hairItem, skin);
                        ColorUtility.TryParseHtmlString(split[7], out Color c);
                        if (hair)
                        {
                            Renderer[] componentsInChildren = hair.GetComponentsInChildren<Renderer>();
                            foreach (Renderer render in componentsInChildren)
                                render.material.SetColor(SkinColor, c);
                        }

                        string beardItem = "Beard" + split[18];
                        GameObject beard = EquipItemsOnModel(helmetJoint, beardItem, skin);
                        ColorUtility.TryParseHtmlString(split[19], out Color beardColor);
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

                        ColorUtility.TryParseHtmlString(split[16], out Color skinColor);
                        skin.materials[0].SetColor(SkinColor, skinColor);
                    }
                }
            }

            if (pastOverrideModel)
            {
                if (!float.TryParse(split[8], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture,
                        out float val)) val = 1f;
                if (val < 0.1f) val = 0.1f;
                pastOverrideModel.transform.localScale *= val;
            }

            float.TryParse(split[22], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture,
                out float textSize);
            float.TryParse(split[23], NumberStyles.Any, CultureInfo.InvariantCulture,
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
                transform.Find(_currentNpcType.ToString()).gameObject.SetActive(true);
        }

        private void OverrideName(long sender, string newName)
        {
            if (znv.IsOwner()) znv.m_zdo.Set("KGnpcNameOverride", newName);
            CheckNameOnIcons(newName);
            transform.Find("MPASNquest").gameObject.SetActive(Quests_DataTypes.Quest.IsQuestTarget(newName));
        }

        private void ChangeProfile(long sender, string text)
        {
            if (znv.IsOwner())
            {
                if (string.IsNullOrWhiteSpace(text)) text = "default";
                znv.m_zdo.Set("KGnpcProfile", text.ToLower());
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

        private bool _isPlayerModel;

        private bool TryOverrideModel(ref string prefab, out bool isFemale, bool EquipItems = true)
        {
            bool overrideHumanoid = false;
            _isPlayerModel = false;

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
                    particleSystem.enableEmission = false;
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
                            _isPlayerModel = true;
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
            znv.InvokeRPC(ZNetView.Everybody, "KGmarket GetDamage", new object[] { null });
        }

        private void PlayStaggerAnimation(long sender)
        {
            if (!pastOverrideModel) return;
            Animator anim = pastOverrideModel.GetComponent<Animator>();
            if (!anim) return;
            anim.speed = 1f;
            anim.SetTrigger(Stagger);
        }

        public DestructibleType GetDestructibleType()
        {
            return DestructibleType.None;
        }
    }

    
    public class NpcData
    {
        public string PrefabOverride;
        public string LeftItem;
        public string RightItem;
        public string HelmetItem;
        public string ChestItem;
        public string LegsItem;
        public string CapeItem;
        public string HairItem;
        public string HairItemColor;
        public string ModelScale;
        public string LeftItemHidden;
        public string RightItemHidden;
        public string NPCinteractAnimation;
        public string NPCgreetAnimation;
        public string NPCbyeAnimation;
        public string NPCgreetText;
        public string NPCbyeText;
        public string SkinColor;
        public string NPCcraftingAnimation;
        public string BeardItem;
        public string BeardItemColor;
        public string InteractAudioClip;
        public string TextSize;
        public string TextHeight;
        public string PeriodicAnimation;
        public string PeriodicAnimationTime;
        public string IMAGE;
    }

    private static class NPCLoader_UI
    {
        private static GameObject UI;
        private static GameObject Element_GO;
        private static Transform Content;
        private static InputField Input;
        private static Transform ShowImage;
        private static readonly List<GameObject> Elements = new();

        private static NPCcomponent _currentNPC;

        public static bool IsVisible()
        {
            return UI && UI.activeSelf;
        }


        public static void Init()
        {
            UI = Object.Instantiate(AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("LoadNPC_UI"));
            Element_GO = AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("LoadNPC_Element");
            Content = UI.transform.Find("Canvas/View_Profiles/CraftView/Scroll View/Viewport/Content");
            Input = UI.transform.Find("Canvas/View_Profiles/InputField").GetComponent<InputField>();
            Object.DontDestroyOnLoad(UI);
            UI.SetActive(false);
            UI.transform.Find("Canvas/View_Profiles/Button").GetComponent<Button>().onClick.AddListener(SaveProfile);
            ShowImage = UI.transform.Find("Canvas/View_Profiles/ShowImage");
            Localization.instance.Localize(UI.transform);
        }

        private static void LoadProfiles()
        {
            string folderPath = Path.Combine(BepInEx.Paths.ConfigPath, "SavedNPCs");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            string[] files = Directory.GetFiles(folderPath, "*.json");
            Dictionary<string, NpcData> dictionary = new();
            foreach (string file in files)
            {
                try
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string json = File.ReadAllText(file);
                    NpcData data = JSON.ToObject<NpcData>(json);
                    dictionary[fileName] = data;
                }
                catch
                {
                    // ignored
                }
            }

            foreach (GameObject element in Elements)
                Object.Destroy(element);
            Elements.Clear();
            foreach (KeyValuePair<string, NpcData> pair in dictionary)
            {
                GameObject element = Object.Instantiate(Element_GO, Content);
                element.transform.Find("Text").GetComponent<Text>().text = pair.Key;
                element.GetComponent<Button>().onClick.AddListener(() => LoadProfile(pair.Value));
                Elements.Add(element);
                element.AddComponent<HoverOnButton_WithImage>()
                    .Setup(pair.Key, ShowImage, pair.Value.IMAGE);
            }
        }

        private static void LoadProfile(NpcData data)
        {
            AssetStorage.AssetStorage.AUsrc.Play();
            if (_currentNPC == null || _currentNPC.znv == null || !_currentNPC.znv.IsValid())
            {
                Hide();
                return;
            }

            _currentNPC.znv.InvokeRPC(ZNetView.Everybody, "KGmarket overridemodel", data.PrefabOverride);
            string combine =
                $"{data.LeftItem}|{data.RightItem}|{data.HelmetItem}|{data.ChestItem}|{data.LegsItem}|{data.CapeItem}|" +
                $"{data.HairItem}|{data.HairItemColor}|{data.ModelScale}|{data.LeftItemHidden}|{data.RightItemHidden}|" +
                $"{data.NPCinteractAnimation}|{data.NPCgreetAnimation}|{data.NPCbyeAnimation}|{data.NPCgreetText}|{data.NPCbyeText}|{data.SkinColor}|{data.NPCcraftingAnimation}|" +
                $"{data.BeardItem}|{data.BeardItemColor}|{data.InteractAudioClip}|FONT|{data.TextSize}|{data.TextHeight}|{data.PeriodicAnimation}|{data.PeriodicAnimationTime}";
            _currentNPC.znv.InvokeRPC(ZNetView.Everybody, "KGmarket fashion", combine);
            Hide();
        }

        public static void Save(NPCcomponent npc, out string text)
        {
            text = "";
            string NPCName = npc.GetNPCName();
            if (string.IsNullOrEmpty(NPCName))
            {
                NPCName = Localization.instance.Localize("$mpasn_" + npc._currentNpcType);
            }

            NPCName += $" {npc.transform.position}";
            string folderPath = Path.Combine(BepInEx.Paths.ConfigPath, "SavedNPCs");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            string fileName = NPCName + ".json";
            string filePath = Path.Combine(folderPath, fileName);
            if (!File.Exists(filePath)) File.Create(filePath).Close();
            NpcData newData = new()
            {
                PrefabOverride = npc.znv.m_zdo.GetString("KGnpcModelOverride"),
                LeftItem = npc.znv.m_zdo.GetString("KGleftItem"),
                LeftItemHidden = npc.znv.m_zdo.GetString("KGLeftItemBack"),
                RightItemHidden = npc.znv.m_zdo.GetString("KGRightItemBack"),
                RightItem = npc.znv.m_zdo.GetString("KGrightItem"),
                HelmetItem = npc.znv.m_zdo.GetString("KGhelmetItem"),
                ChestItem = npc.znv.m_zdo.GetString("KGchestItem"),
                LegsItem = npc.znv.m_zdo.GetString("KGlegsItem"),
                HairItem = npc.znv.m_zdo.GetString("KGhairItem"),
                HairItemColor = npc.znv.m_zdo.GetString("KGhairItemColor"),
                CapeItem = npc.znv.m_zdo.GetString("KGcapeItem"),
                ModelScale = npc.znv.m_zdo.GetFloat("KGnpcScale", 1f).ToString(CultureInfo.InvariantCulture),
                NPCinteractAnimation = npc.znv.m_zdo.GetString("KGinteractAnimation"),
                NPCgreetAnimation = npc.znv.m_zdo.GetString("KGgreetingAnimation"),
                NPCbyeAnimation = npc.znv.m_zdo.GetString("KGbyeAnimation"),
                NPCgreetText = npc.znv.m_zdo.GetString("KGgreetingText"),
                NPCbyeText = npc.znv.m_zdo.GetString("KGbyeText"),
                SkinColor = npc.znv.m_zdo.GetString("KGskinColor"),
                NPCcraftingAnimation = npc.znv.m_zdo.GetInt("KGcraftingAnimation").ToString(),
                BeardItem = npc.znv.m_zdo.GetString("KGbeardItem"),
                BeardItemColor = npc.znv.m_zdo.GetString("KGbeardColor"),
                InteractAudioClip = npc.znv.m_zdo.GetString("KGinteractSound"),
                TextSize = npc.znv.m_zdo.GetFloat("KGtextSize", 3).ToString(CultureInfo.InvariantCulture),
                TextHeight = npc.znv.m_zdo.GetFloat("KGtextHeight").ToString(CultureInfo.InvariantCulture),
                PeriodicAnimation = npc.znv.m_zdo.GetString("KGperiodicAnimation"),
                PeriodicAnimationTime = npc.znv.m_zdo.GetFloat("KGperiodicAnimationTime")
                    .ToString(CultureInfo.InvariantCulture),
                IMAGE = PhotoManager.__instance.NPC_Photo(npc)
            };
            string json = JSON.ToNiceJSON(newData);
            File.WriteAllText(filePath, json);
            text = $"Saved {NPCName}";
        }


        private static void SaveProfile()
        {
            AssetStorage.AssetStorage.AUsrc.Play();
            if (_currentNPC == null || _currentNPC.znv == null || !_currentNPC.znv.IsValid())
            {
                Hide();
                return;
            }

            if (string.IsNullOrWhiteSpace(Input.text)) return;
            string folderPath = Path.Combine(BepInEx.Paths.ConfigPath, "SavedNPCs");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            string fileName = Input.text + ".json";
            Input.text = "";
            string filePath = Path.Combine(folderPath, fileName);
            if (!File.Exists(filePath)) File.Create(filePath).Close();
            NpcData newData = new()
            {
                PrefabOverride = _currentNPC.znv.m_zdo.GetString("KGnpcModelOverride"),
                LeftItem = _currentNPC.znv.m_zdo.GetString("KGleftItem"),
                LeftItemHidden = _currentNPC.znv.m_zdo.GetString("KGLeftItemBack"),
                RightItemHidden = _currentNPC.znv.m_zdo.GetString("KGRightItemBack"),
                RightItem = _currentNPC.znv.m_zdo.GetString("KGrightItem"),
                HelmetItem = _currentNPC.znv.m_zdo.GetString("KGhelmetItem"),
                ChestItem = _currentNPC.znv.m_zdo.GetString("KGchestItem"),
                LegsItem = _currentNPC.znv.m_zdo.GetString("KGlegsItem"),
                HairItem = _currentNPC.znv.m_zdo.GetString("KGhairItem"),
                HairItemColor = _currentNPC.znv.m_zdo.GetString("KGhairItemColor"),
                CapeItem = _currentNPC.znv.m_zdo.GetString("KGcapeItem"),
                ModelScale = _currentNPC.znv.m_zdo.GetFloat("KGnpcScale", 1f).ToString(CultureInfo.InvariantCulture),
                NPCinteractAnimation = _currentNPC.znv.m_zdo.GetString("KGinteractAnimation"),
                NPCgreetAnimation = _currentNPC.znv.m_zdo.GetString("KGgreetingAnimation"),
                NPCbyeAnimation = _currentNPC.znv.m_zdo.GetString("KGbyeAnimation"),
                NPCgreetText = _currentNPC.znv.m_zdo.GetString("KGgreetingText"),
                NPCbyeText = _currentNPC.znv.m_zdo.GetString("KGbyeText"),
                SkinColor = _currentNPC.znv.m_zdo.GetString("KGskinColor"),
                NPCcraftingAnimation = _currentNPC.znv.m_zdo.GetInt("KGcraftingAnimation").ToString(),
                BeardItem = _currentNPC.znv.m_zdo.GetString("KGbeardItem"),
                BeardItemColor = _currentNPC.znv.m_zdo.GetString("KGbeardColor"),
                InteractAudioClip = _currentNPC.znv.m_zdo.GetString("KGinteractSound"),
                TextSize = _currentNPC.znv.m_zdo.GetFloat("KGtextSize", 3).ToString(CultureInfo.InvariantCulture),
                TextHeight = _currentNPC.znv.m_zdo.GetFloat("KGtextHeight").ToString(CultureInfo.InvariantCulture),
                PeriodicAnimation = _currentNPC.znv.m_zdo.GetString("KGperiodicAnimation"),
                PeriodicAnimationTime = _currentNPC.znv.m_zdo.GetFloat("KGperiodicAnimationTime")
                    .ToString(CultureInfo.InvariantCulture),
                IMAGE = PhotoManager.__instance.NPC_Photo(_currentNPC)
            };
            string json = JSON.ToNiceJSON(newData);
            File.WriteAllText(filePath, json);
            LoadProfiles();
        }


        private static void Default()
        {
            Input.text = "";
            Elements.ForEach(Object.Destroy);
            Elements.Clear();
            ShowImage.gameObject.SetActive(false);
        }

        public static void Hide()
        {
            _currentNPC = null;
            Default();
            UI.SetActive(false);
        }

        public static void Show(NPCcomponent npc)
        {
            if (!npc) return;
            _currentNPC = npc;
            Default();
            UI.SetActive(true);
            LoadProfiles();
        }
    }

    private static class NPCUI
    {
        private static GameObject UI;
        private static NPCcomponent _currentNPC;
        private static GameObject MAIN;
        private static GameObject FASHION;
        private static Transform NPCTypeButtons;
        private static NPCType _currentType;
        private static InputField _npcprofile;
        private static InputField _npcname;
        private static InputField _npcmodel;
        private static InputField _patroldata;

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
            PeriodicAnimationTime;

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
            UI.transform.Find("Canvas/FASHION/Pergament/Apply").GetComponent<Button>().onClick
                .AddListener(ApplyFashion);
        }

        private static void ApplyFashion()
        {
            AssetStorage.AssetStorage.AUsrc.Play();
            if (!_currentNPC)
            {
                Hide();
                return;
            }

            string combine =
                $"{LeftItemFashion.text}|{RightItemFashion.text}|{HelmetItemFashion.text}|{ChestItemFashion.text}|{LegsItemFashion.text}|{CapeItemFashion.text}|" +
                $"{HairItemFashion.text}|{HairItemFashionColor.text}|{ModelScaleFashion.text}|{LeftItemHiddenFashion.text}|{RightItemHiddenFashion.text}|" +
                $"{NPCinteractAnimation.text}|{NPCgreetAnimation.text}|{NPCbyeAnimation.text}|{NPCgreetText.text}|{NPCbyeText.text}|{SkinColorFashion.text}|{NPCcraftingAnimation.text}|" +
                $"{BeardItemFashion.text}|{BeardItemFashionColor.text}|{InteractAudioClip.text}|FONT|{TextSize.text}|{TextHeight.text}|{PeriodicAnimation.text}|{PeriodicAnimationTime.text}";
            _currentNPC.znv.InvokeRPC(ZNetView.Everybody, "KGmarket fashion", combine);
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
            _currentNPC.znv.InvokeRPC(ZNetView.Everybody, "KGmarket snapandrotate", pkg);
            Hide();
        }

        private static void ApplyMain()
        {
            AssetStorage.AssetStorage.AUsrc.Play();
            if (!_currentNPC)
            {
                Hide();
                return;
            }

            _currentNPC.znv.InvokeRPC(ZNetView.Everybody, "KGMarket changeNpcType", (int)_currentType);
            _currentNPC.znv.InvokeRPC(ZNetView.Everybody, "KGmarket overridename", _npcname.text);
            _currentNPC.znv.InvokeRPC("KGmarket changeprofile", _npcprofile.text);
            _currentNPC.znv.InvokeRPC(ZNetView.Everybody, "KGmarket overridemodel", _npcmodel.text);
            _currentNPC.znv.InvokeRPC(ZNetView.Everybody, "KGmarket GetPatrolData", _patroldata.text);
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

        public static void ShowFashion(NPCcomponent _npc)
        {
            FASHION.SetActive(true);
            MAIN.SetActive(false);
            _currentNPC = _npc;
            LeftItemFashion.text = _npc.znv.m_zdo.GetString("KGleftItem");
            LeftItemHiddenFashion.text = _npc.znv.m_zdo.GetString("KGLeftItemBack");
            RightItemHiddenFashion.text = _npc.znv.m_zdo.GetString("KGRightItemBack");
            RightItemFashion.text = _npc.znv.m_zdo.GetString("KGrightItem");
            HelmetItemFashion.text = _npc.znv.m_zdo.GetString("KGhelmetItem");
            ChestItemFashion.text = _npc.znv.m_zdo.GetString("KGchestItem");
            LegsItemFashion.text = _npc.znv.m_zdo.GetString("KGlegsItem");
            HairItemFashion.text = _npc.znv.m_zdo.GetString("KGhairItem");
            HairItemFashionColor.text = _npc.znv.m_zdo.GetString("KGhairItemColor");
            CapeItemFashion.text = _npc.znv.m_zdo.GetString("KGcapeItem");
            ModelScaleFashion.text = _npc.znv.m_zdo.GetFloat("KGnpcScale", 1f).ToString(CultureInfo.InvariantCulture);
            NPCinteractAnimation.text = _npc.znv.m_zdo.GetString("KGinteractAnimation");
            NPCgreetAnimation.text = _npc.znv.m_zdo.GetString("KGgreetingAnimation");
            NPCbyeAnimation.text = _npc.znv.m_zdo.GetString("KGbyeAnimation");
            NPCgreetText.text = _npc.znv.m_zdo.GetString("KGgreetingText");
            NPCbyeText.text = _npc.znv.m_zdo.GetString("KGbyeText");
            SkinColorFashion.text = _npc.znv.m_zdo.GetString("KGskinColor");
            NPCcraftingAnimation.text = _npc.znv.m_zdo.GetInt("KGcraftingAnimation").ToString();
            BeardItemFashion.text = _npc.znv.m_zdo.GetString("KGbeardItem");
            BeardItemFashionColor.text = _npc.znv.m_zdo.GetString("KGbeardColor");
            InteractAudioClip.text = _npc.znv.m_zdo.GetString("KGinteractSound");
            TextSize.text = _npc.znv.m_zdo.GetFloat("KGtextSize", 3).ToString(CultureInfo.InvariantCulture);
            TextHeight.text = _npc.znv.m_zdo.GetFloat("KGtextHeight").ToString(CultureInfo.InvariantCulture);
            PeriodicAnimation.text = _npc.znv.m_zdo.GetString("KGperiodicAnimation");
            PeriodicAnimationTime.text = _npc.znv.m_zdo.GetFloat("KGperiodicAnimationTime")
                .ToString(CultureInfo.InvariantCulture);
            UI.SetActive(true);
        }

        public static void ShowMain(NPCcomponent _npc)
        {
            FASHION.SetActive(false);
            MAIN.SetActive(true);
            _currentType = _npc._currentNpcType;
            _npcprofile.text = _npc.znv.m_zdo.GetString("KGnpcProfile", "default");
            _npcname.text = _npc.znv.m_zdo.GetString("KGnpcNameOverride");
            _npcmodel.text = _npc.znv.m_zdo.GetString("KGnpcModelOverride");
            _patroldata.text = _npc.znv.m_zdo.GetString("KGmarket PatrolData");
            _currentNPC = _npc;
            CheckColors();
            UI.SetActive(true);
        }

        [HarmonyPatch(typeof(InputField), "OnPointerDown")]
        [ClientOnlyPatch]
        private static class InputField__Patch
        {
            private static void Prefix(InputField __instance)
            {
                if (__instance.lineType != InputField.LineType.SingleLine) return;
                if (IsVisible() && !string.IsNullOrEmpty(__instance.text))
                    AccessTools.Field(typeof(InputField), "m_AllowInput").SetValue(__instance, true);
            }
        }
    }
}
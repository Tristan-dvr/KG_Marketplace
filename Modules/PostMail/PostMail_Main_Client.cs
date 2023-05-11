namespace Marketplace.Modules.PostMail;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Normal, "OnInit")]
public static class PostMail_Main_Client
{
    private static GameObject PostMail_Prefab;
    private static Sprite Icon;
    private const string PrefabToSearch = "MarketplacePostMail";
    private const Minimap.PinType PinType = (Minimap.PinType)174;
    private static readonly List<Minimap.PinData> TempPins = new();
    private static readonly Dictionary<Vector3, ZDO> PinMapper = new();

    private static void OnInit()
    {
        PostMail_UI.Init();
        PostMail_Prefab = AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("MarketplacePostMail");
        PostMail_Prefab.AddComponent<PostMailComponent>();
        Icon = PostMail_Prefab.GetComponent<Piece>().m_icon;
        Marketplace.Global_Updator += Update;
        Global_Values._container.ValueChanged += ResetMailpostRecipe;
    }

    private static void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && (PostMail_UI.IsVisible() || PostMail_UI.IsSendVisible()))
        {
            PostMail_UI.HideMain();
            PostMail_UI.HideSend();
            Menu.instance.OnClose();
        }
    }


    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    [ClientOnlyPatch]
    private static class ZNetScene_Awake_Patch
    {
        private static void Postfix(ZNetScene __instance)
        {
            __instance.m_namedPrefabs[PostMail_Prefab.name.GetStableHashCode()] = PostMail_Prefab;
            GameObject hammer = __instance.GetPrefab("Hammer");
            hammer.GetComponent<ItemDrop>().m_itemData.m_shared.m_buildPieces.m_pieces.Add(PostMail_Prefab);
            ResetMailpostRecipe();
        }
    }
    
    [HarmonyPatch(typeof(Piece), nameof(Piece.CanBeRemoved))]
    [ClientOnlyPatch]
    static class Piece_CanBeRemoved_Patch
    {
        static void Postfix(Piece __instance, ref bool __result)
        {
            if (global::Utils.GetPrefabName(__instance.gameObject) == PostMail_Prefab.name && !Utils.IsDebug)
            {
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Debug Mode Please");
                __result = false;
            }
        }
    }

    private static void ResetMailpostRecipe()
    {
        if(!ZNetScene.instance) return;
        try
        {
            List<Piece.Requirement> reqs = new();
            string recipeStr = Global_Values._container.Value._mailPostRecipe.Replace(" ","");
            string[] split = recipeStr.Split(',');
            for (int i = 0; i < split.Length; i += 2)
            {
                string name = split[i];
                int amount = int.Parse(split[i + 1]);
                reqs.Add(new Piece.Requirement()
                {
                    m_amount = amount,
                    m_resItem = ObjectDB.instance.GetItemPrefab(name.GetStableHashCode()).GetComponent<ItemDrop>()
                });
            }
            PostMail_Prefab.GetComponent<Piece>().m_resources = reqs.ToArray();
        }
        catch
        {
            PostMail_Prefab.GetComponent<Piece>().m_resources = new[]
            {
                new Piece.Requirement()
                {
                    m_amount = 1,
                    m_resItem = ObjectDB.instance.GetItemPrefab("SwordCheat").GetComponent<ItemDrop>()
                }
            };
        }
    }

    public class PostMailComponent : MonoBehaviour, Interactable, Hoverable
    {
        private ZNetView _znv;

        public static List<PostMail_DataTypes.MailData> TryReadMailFromZDO(ZNetView znv) =>
            TryReadMailFromZDO(znv.m_zdo);

        public static List<PostMail_DataTypes.MailData> TryReadMailFromZDO(ZDO zdo)
        {
            string json = zdo.GetString("MarketplacePostMaiLJSON");
            if (string.IsNullOrEmpty(json)) return new();
            try
            {
                List<PostMail_DataTypes.MailData> data = JSON.ToObject<List<PostMail_DataTypes.MailData>>(json);
                return data;
            }
            catch
            {
                return new();
            }
        }

        public static void WriteMailToZDO(ZDO zdo, List<PostMail_DataTypes.MailData> data)
        {
            string json = JSON.ToJSON(data);
            zdo.Set("MarketplacePostMaiLJSON", json);
        }

        private string Mail_Name
        {
            get => _znv.m_zdo.GetString("MarketplacePostMailName");
            set => _znv.m_zdo.Set("MarketplacePostMailName", value);
        }

        private void Awake()
        {
            _znv = GetComponent<ZNetView>();
            if (!_znv.IsValid()) return;
            if (_znv.IsOwner() && string.IsNullOrEmpty(Mail_Name))
                Mail_Name = $"<color=green>{Player.m_localPlayer.GetPlayerName()}</color> <color=red>Mailbox</color>";

            _znv.Register<string>("WriteMail", WriteMail);
            _znv.Register<int>("RemoveMail", RemoveMail);
        }

        private void RemoveMail(long sender, int uid)
        {
            if (!_znv.IsOwner()) return;
            List<PostMail_DataTypes.MailData> data = TryReadMailFromZDO(_znv.m_zdo);
            data.RemoveAll(x => x.UID == uid);
            WriteMailToZDO(_znv.m_zdo, data);
        }

        private void WriteMail(long obj, string newMailJSON)
        {
            if (!_znv.IsOwner()) return;
            List<PostMail_DataTypes.MailData> data = TryReadMailFromZDO(_znv.m_zdo);
            data.Add(JSON.ToObject<PostMail_DataTypes.MailData>(newMailJSON));
            WriteMailToZDO(_znv.m_zdo, data);
        }

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            if (alt)
            {
                ShowMapWithPins();
            }
            else
            {
                if (GetComponent<Piece>().IsCreator())
                    PostMail_UI.ShowMain(_znv);
            }

            return true;
        }

        private void ShowMapWithPins()
        {
            InventoryGui.instance.Hide();
            foreach (Minimap.PinData obj in TempPins) Minimap.instance.RemovePin(obj);
            TempPins.Clear();
            PinMapper.Clear();
            Minimap.instance.SetMapMode(Minimap.MapMode.Large);
            List<ZDO> AllMails = new();
            ZDOMan.instance.GetAllZDOsWithPrefab(PrefabToSearch, AllMails);
            if (AllMails.Count == 0) return;
            foreach (var mail in AllMails)
            {
                if (!mail.IsValid() || mail.m_uid == _znv.m_zdo.m_uid) continue;
                Minimap.PinData pinData = new Minimap.PinData
                {
                    m_type = PinType,
                    m_name = mail.GetString("MarketplacePostMailName", "<color=#32CD32>Mailbox</color>"),
                    m_pos = new Vector3(mail.m_position.x, mail.m_position.y, mail.m_position.z),
                    m_doubleSize = true,
                    m_animate = false,
                    m_icon = Icon,
                    m_save = false,
                    m_checked = false,
                    m_ownerID = 0L
                };
                if (!string.IsNullOrEmpty(pinData.m_name))
                {
                    pinData.m_NamePinData = new Minimap.PinNameData(pinData);
                }

                TempPins.Add(pinData);
                PinMapper.Add(pinData.m_pos, mail);
                Minimap.instance.m_pins.Add(pinData);
            }

            Minimap.instance.m_largeZoom = 1f;
            Minimap.instance.CenterMap(Vector3.zero);
        }

        [HarmonyPatch(typeof(Minimap), nameof(Minimap.SetMapMode))]
        [ClientOnlyPatch]
        private static class PatchRemovePinsMinimap
        {
            private static void Prefix(Minimap.MapMode mode)
            {
                if (mode != Minimap.MapMode.Large)
                {
                    foreach (Minimap.PinData obj in TempPins) Minimap.instance.RemovePin(obj);
                    TempPins.Clear();
                    PinMapper.Clear();
                }
            }
        }

        [HarmonyPatch(typeof(Minimap), nameof(Minimap.OnMapLeftClick))]
        [ClientOnlyPatch]
        private static class PatchClickIconMinimap
        {
            private static bool Prefix(Minimap __instance)
            {
                Vector3 pos = __instance.ScreenToWorldPoint(Input.mousePosition);
                Minimap.PinData closestPin = Utils.GetCustomPin(PinType, pos,
                    __instance.m_removeRadius * (__instance.m_largeZoom * 2f));
                if (closestPin != null && PinMapper.TryGetValue(closestPin.m_pos, out var zdo))
                {
                    PostMail_UI.ShowSend(zdo);
                    __instance.SetMapMode(Minimap.MapMode.Small);
                    return false;
                }

                return true;
            }
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }

        public string GetHoverText()
        {
            return Localization.instance.Localize("[<color=yellow><b>ALT + $KEY_Use</b></color>] Open Mailbox") + "\n" +
                   Localization.instance.Localize("[<color=yellow><b>L.Shift + $KEY_Use</b></color>] Send Mail");
        }

        public string GetHoverName()
        {
            return Mail_Name;
        }
        
    }
}
using Marketplace.Modules.Marketplace_NPC;

namespace Marketplace.Modules.PostMail;

public static class PostMail_UI
{
    private static GameObject UI;
    private static GameObject Send_UI;
    private static GameObject Element;

    private static Transform Content;

    private static ZNetView _currentlyProcessedMail;
    private static ZDO _currentProcessedTargetPost;
    private static readonly Dictionary<int, GameObject> MailElements = new();

    private static InputField Send_UI_Message;
    private static ItemDrop.ItemData CurrentSendItem;

    public static bool IsVisible()
    {
        return UI && UI.activeSelf;
    }

    public static bool IsSendVisible()
    {
        return Send_UI && Send_UI.activeSelf;
    }

    public static void Init()
    {
        UI = UnityEngine.Object.Instantiate(
            AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("MarketplacePostMailUI"));
        Send_UI = UnityEngine.Object.Instantiate(
            AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("MarketplacePostMailUI_Send"));
        UnityEngine.Object.DontDestroyOnLoad(UI);
        UnityEngine.Object.DontDestroyOnLoad(Send_UI);
        Element = AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("MarketplacePostMailElement");
        UI.SetActive(false);
        Send_UI.SetActive(false);

        Content = UI.transform.Find("GO/Scroll Rect/Viewport/Content");
        Send_UI_Message = Send_UI.transform.Find("GO/Message").GetComponent<InputField>();
        Send_UI.transform.Find("GO/Apply").GetComponent<Button>().onClick.AddListener(() =>
        {
            SendItem();
            HideSend();
        });
    }

    private static void FillMail()
    {
        if (_currentlyProcessedMail == null || !_currentlyProcessedMail.IsValid()) return;
        List<PostMail_DataTypes.MailData> data = PostMail_Main_Client.PostMailComponent.TryReadMailFromZDO(_currentlyProcessedMail);
        foreach (Transform child in Content)
        {
            UnityEngine.Object.Destroy(child.gameObject);
        }

        MailElements.Clear();

        foreach (PostMail_DataTypes.MailData mailData in data)
        {
            GameObject element = UnityEngine.Object.Instantiate(Element, Content);
            element.transform.Find("From").GetComponent<Text>().text =
                "From: " + "<color=yellow>" + mailData.Sender + "</color>";
            element.transform.Find("Message").GetComponent<Text>().text = "Message: " + mailData.Message;
            MailElements.Add(mailData.UID, element);
            Button button = element.transform.Find("Button").GetComponent<Button>();
            button.onClick.AddListener(() => RemoveMail(mailData));
            if (!mailData.HasAttachedItem)
            {
                button.transform.Find("fill").gameObject.SetActive(false);
                button.transform.Find("frame").GetComponent<Image>().color = Color.green;
                button.transform.Find("Text").GetComponent<Text>().text = "Delete";
                element.transform.Find("Content").gameObject.SetActive(false);
            }
            else
            {
                long currentTime = (long)EnvMan.instance.m_totalSeconds;
                long mailTime = mailData.TotalSeconds;
                long timeLeft = mailTime - currentTime;
                if (timeLeft <= 0)
                {
                    button.transform.Find("fill").gameObject.SetActive(false);
                    button.transform.Find("frame").GetComponent<Image>().color = Color.green;
                    button.transform.Find("Text").GetComponent<Text>().text = "Take";
                }
                else
                {
                    button.transform.Find("fill").gameObject.SetActive(true);
                    button.transform.Find("frame").GetComponent<Image>().color = Color.red;
                    button.transform.Find("Text").GetComponent<Text>().text = +timeLeft + "s";
                }

                if (ZNetScene.instance.GetPrefab(mailData.AttachedItem.ItemPrefab) is not { } item) return;
                string localizedName =
                    Localization.instance.Localize(item.GetComponent<ItemDrop>().m_itemData.m_shared.m_name);
                string stars = item.GetComponent<ItemDrop>().m_itemData.m_shared.m_maxQuality > 1
                    ? $" (<color=#00ff00>{mailData.AttachedItem.Quality}★</color>)"
                    : "";
                element.transform.Find("Content").GetComponent<Text>().text =
                    $"Attached Item: <color=yellow>x{mailData.AttachedItem.Count} {localizedName}</color>{stars}";
                element.transform.Find("Content/Icon/img").GetComponent<Image>().sprite =
                    item.GetComponent<ItemDrop>().m_itemData.GetIcon();
            }
        }
    }

    private static void RemoveMail(PostMail_DataTypes.MailData data)
    {
        if (_currentlyProcessedMail == null || !_currentlyProcessedMail.IsValid()) return;
        AssetStorage.AssetStorage.AUsrc.Play();
        if (!data.HasAttachedItem)
        {
            _currentlyProcessedMail.InvokeRPC("RemoveMail", data.UID);
            if (MailElements.TryGetValue(data.UID, out GameObject go2)) UnityEngine.Object.Destroy(go2);
            return;
        }

        long currentTime = (long)EnvMan.instance.m_totalSeconds;
        long mailTime = data.TotalSeconds;
        long timeLeft = mailTime - currentTime;
        if (timeLeft > 0) return;
        _currentlyProcessedMail.InvokeRPC("RemoveMail", data.UID);
        if (MailElements.TryGetValue(data.UID, out GameObject go)) UnityEngine.Object.Destroy(go);
        
        if (ZNetScene.instance.GetPrefab(data.AttachedItem.ItemPrefab) is not { } itemPrefab) return;
        ItemDrop item = itemPrefab.GetComponent<ItemDrop>();
        Dictionary<string, string> NewCustomData =
            JSON.ToObject<Dictionary<string, string>>(data.AttachedItem.CUSTOMdata);
        Player p = Player.m_localPlayer;
        int stack = data.AttachedItem.Count;
        while (stack > 0)
        {
            if (p.m_inventory.FindEmptySlot(false) is { x: >= 0 } pos)
            {
                int addStack = Math.Min(stack, item.m_itemData.m_shared.m_maxStackSize);
                stack -= addStack;
                p.m_inventory.AddItem(data.AttachedItem.ItemPrefab, addStack,
                    item.m_itemData.GetMaxDurability(data.AttachedItem.Quality), pos,
                    false, data.AttachedItem.Quality, data.AttachedItem.Variant, data.AttachedItem.CrafterID,
                    data.AttachedItem.CrafterName,
                    NewCustomData);
            }
            else
            {
                break;
            }
        }

        if (stack <= 0) return;
        while (stack > 0)
        {
            int addStack = Math.Min(stack, item.m_itemData.m_shared.m_maxStackSize);
            stack -= addStack;
            Transform transform = p.transform;
            Vector3 position = transform.position;
            ItemDrop itemDrop = UnityEngine.Object.Instantiate(itemPrefab, position + Vector3.up, transform.rotation)
                .GetComponent<ItemDrop>();
            itemDrop.m_itemData.m_customData = NewCustomData;
            itemDrop.m_itemData.m_stack = addStack;
            itemDrop.m_itemData.m_crafterName = data.AttachedItem.CrafterName;
            itemDrop.m_itemData.m_crafterID = data.AttachedItem.CrafterID;
            itemDrop.Save();
            itemDrop.OnPlayerDrop();
            itemDrop.GetComponent<Rigidbody>().velocity = (transform.forward + Vector3.up);
            p.m_dropEffects.Create(position, Quaternion.identity);
        }
    }

    public static void ShowMain(ZNetView znv)
    {
        znv.ClaimOwnership();
        if (!znv.IsValid()) return;
        _currentlyProcessedMail = znv;
        FillMail();
        UI.SetActive(true);
    }

    public static void HideMain()
    {
        UI.SetActive(false);
    }

    public static void HideSend()
    {
        Send_UI_Message.text = "";
        Send_UI.transform.Find("GO/AttachItem/img").GetComponent<Image>().sprite = AssetStorage.AssetStorage.NullSprite;
        CurrentSendItem = null;
        Send_UI.SetActive(false);
    }

    public static void ShowSend(ZDO zdo)
    {
        _currentProcessedTargetPost = zdo;
        Send_UI.SetActive(true);
        Send_UI.transform.Find("GO/From_Text").GetComponent<Text>().text = $"Sending message to: {_currentProcessedTargetPost.GetString("MarketplacePostMailName","Mail Post")}";
        Send_UI_Message.text = "";
        Send_UI.transform.Find("GO/AttachItem/img").GetComponent<Image>().sprite = AssetStorage.AssetStorage.NullSprite;
        CurrentSendItem = null;
        InventoryGui.instance.Show(null);
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.SetupDragItem))]
    [ClientOnlyPatch]
    static class InventoryGui_SetupDragItem_Patch
    {
        static void Postfix(InventoryGui __instance)
        {
            if (__instance.m_dragGo && __instance.m_dragItem != null)
            {
                if (IsSendVisible())
                {
                    CurrentSendItem = __instance.m_dragItem;
                    Send_UI.transform.Find("GO/AttachItem/img").GetComponent<Image>().sprite =
                        __instance.m_dragItem.m_dropPrefab.GetComponent<ItemDrop>().m_itemData.GetIcon();
                    InventoryGui.instance.SetupDragItem(null, null, 1);
                }
            }
        }
    }

    private static void SendItem()
    {
        if (_currentProcessedTargetPost == null || !_currentProcessedTargetPost.IsValid()) return;
        ItemDrop.ItemData item = CurrentSendItem;
        if (!Player.m_localPlayer.m_inventory.m_inventory.Contains(item)) item = null;
        Marketplace_DataTypes.ClientMarketSendData newData = item != null
            ? new Marketplace_DataTypes.ClientMarketSendData
            {
                ItemPrefab = item.m_dropPrefab.name,
                Count = item.m_stack,
                Quality = item.m_quality,
                Variant = item.m_variant,
                CUSTOMdata = JSON.ToJSON(item.m_customData),
                CrafterID = item.m_crafterID,
                CrafterName = item.m_crafterName
            }
            : null;

        PostMail_DataTypes.MailData toSend = new()
        {
            UID = UnityEngine.Random.Range(int.MinValue, int.MaxValue),
            Sender = Player.m_localPlayer.GetPlayerName(),
            Message = Send_UI_Message.text,
            AttachedItem = newData,
            HasAttachedItem = newData != null,
            TotalSeconds = (long)EnvMan.instance.m_totalSeconds + 60 * 5
        };
        Player.m_localPlayer.m_inventory.RemoveItem(item);
        if (_currentProcessedTargetPost.HasOwner())
        {
            var json = JSON.ToJSON(toSend);
            var owner = _currentProcessedTargetPost.m_owner;
            var zdoid = _currentProcessedTargetPost.m_uid;
            ZRoutedRpc.instance.InvokeRoutedRPC(owner, zdoid, "WriteMail", json);
        }
        else
        {
            List<PostMail_DataTypes.MailData> zdoMails = PostMail_Main_Client.PostMailComponent.TryReadMailFromZDO(_currentProcessedTargetPost);
            zdoMails.Add(toSend);
            PostMail_Main_Client.PostMailComponent.WriteMailToZDO(_currentProcessedTargetPost, zdoMails);
        }

        MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Mail Sent");
    }


    [HarmonyPatch(typeof(Menu), nameof(Menu.IsVisible))]
    [ClientOnlyPatch]
    private static class Menu_IsVisible_Patch
    {
        private static void Postfix(ref bool __result)
        {
            __result |= IsVisible() || IsSendVisible();
        }
    }
}
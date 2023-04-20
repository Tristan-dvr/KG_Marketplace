using Jewelcrafting;
using Jewelcrafting.GemEffects;
namespace Marketplace.Modules.Marketplace_NPC;
public static class JC_API_Class
{
    private static class JC_GemHandler
    {
        static JC_GemHandler()
        {
            Type idm = Type.GetType("ItemDataManager.ItemInfo, Jewelcrafting");
            Type idmExtentions = Type.GetType("ItemDataManager.ItemExtensions, Jewelcrafting");
            GetMethod = AccessTools.Method(idm, "Get", new[] { typeof(string) })
                .MakeGenericMethod(typeof(ItemContainer));
            DataExtention = AccessTools.Method(idmExtentions, "Data", new[] { typeof(ItemDrop.ItemData) });
        }

        private static readonly MethodInfo GetMethod;
        private static readonly MethodInfo DataExtention;


        public static object GetContainer(ItemDrop.ItemData item)
        {
            object ItemInfo = DataExtention.Invoke(null, new object[] { item });
            return GetMethod.Invoke(ItemInfo, new object[] { "" });
        }
    }


    public static void JC_Api_Tooltip(ItemDrop.ItemData data, List<Transform> JC_Api)
    {
        foreach (Transform transform1 in JC_Api)
        {
            transform1.gameObject.SetActive(false);
            for (int i = 0; i < transform1.childCount; ++i)
            {
                transform1.GetChild(i).gameObject.SetActive(false);
            }
        }

        if (JC_GemHandler.GetContainer(data) is not Socketable sockets) return;
        JC_Api[0].gameObject.SetActive(true);
        JC_Api[1].gameObject.SetActive(true);
        int numSockets = 0;
        if (sockets is not Box { progress: >= 100 })
        {
            numSockets = sockets.socketedGems.Count;
        }

        foreach (Transform transform in JC_Api)
        {
            for (int i = 1; i <= 5; ++i)
            {
                if (transform.Find($"Transmute_Text_{i}") is { } transmute)
                {
                    transmute.gameObject.SetActive(i <= numSockets);
                    if (i <= numSockets)
                    {
                        string socket = sockets.socketedGems[i - 1].Name;
                        string text = "$jc_empty_socket_text";
                        Sprite sprite = AssetStorage.AssetStorage.NullSprite;
                        if (ZNetScene.instance.GetPrefab(socket) is { } gameObject)
                        {
                            if (sockets is not Box)
                            {
                                if (Jewelcrafting.Jewelcrafting.EffectPowers.TryGetValue(socket.GetStableHashCode(),
                                        out Dictionary<GemLocation, List<EffectPower>> locationPowers) &&
                                    locationPowers.TryGetValue(Jewelcrafting.Utils.GetGemLocation(data.m_shared),
                                        out List<EffectPower> effectPowers))
                                {
                                    text = string.Join("\n",
                                        effectPowers.Select(gem =>
                                            $"$jc_effect_{EffectDef.EffectNames[gem.Effect].ToLower()} {gem.Power}"));
                                }
                                else
                                {
                                    text = "$jc_effect_no_effect";
                                }
                            }
                            else
                            {
                                text = gameObject.GetComponent<ItemDrop>().m_itemData.m_shared.m_name;
                            }

                            sprite = gameObject.GetComponent<ItemDrop>().m_itemData.GetIcon();
                        }

                        transmute.GetComponent<Text>().text = Localization.instance.Localize(text);
                        transmute.Find("Transmute_1").GetComponent<Image>().sprite = sprite;
                    }
                }
            }
        }
    }
}
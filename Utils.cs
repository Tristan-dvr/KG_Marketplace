using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using BepInEx.Logging;
using Marketplace.ExternalLoads;
using Marketplace.Modules.Buffer;
using Marketplace.Modules.NPC;
using Marketplace.Modules.Quests;
using Marketplace.Modules.TerritorySystem;
using Marketplace.Modules.Trader;
using UnityEngine.Networking;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace Marketplace;

public static class Utils
{
    public static bool IsDebug => Player.m_debugMode;

    public static void print(object obj, ConsoleColor color = ConsoleColor.DarkGreen)
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            ConsoleManager.SetConsoleColor(color);
            ConsoleManager.StandardOutStream.WriteLine($"[{DateTime.Now}] [kg.Marketplace] {obj}");
            ConsoleManager.SetConsoleColor(ConsoleColor.White);
            foreach (ILogListener logListener in BepInEx.Logging.Logger.Listeners)
                if (logListener is DiskLogListener { LogWriter: not null } bepinexlog)
                    bepinexlog.LogWriter.WriteLine($"[{DateTime.Now}] [kg.Marketplace] {obj}");
        }
        else
        {
            MonoBehaviour.print($"[{DateTime.Now}] [kg.Marketplace] " + obj);
        }
    }

    public static void arr_print(IEnumerable arr, ConsoleColor color = ConsoleColor.DarkGreen)
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            ConsoleManager.SetConsoleColor(color);
            ConsoleManager.StandardOutStream.WriteLine($"[Marketplace] printing array: {arr}");
            int c = 0;
            foreach (object item in arr)
            {
                foreach (ILogListener logListener in BepInEx.Logging.Logger.Listeners)
                    if (logListener is DiskLogListener { LogWriter: not null } bepinexlog)
                        bepinexlog.LogWriter.WriteLine($"[{c++}] {item}");
            }

            ConsoleManager.SetConsoleColor(ConsoleColor.White);
        }
        else
        {
            MonoBehaviour.print("[Marketplace] " + arr);
            int c = 0;
            foreach (object item in arr)
            {
                MonoBehaviour.print($"[{c++}] {item}");
            }
        }
    }

    private static IEnumerator DelayReloadConfigFile(ConfigFile file, Action action)
    {
        yield return new WaitForSecondsRealtime(1.5f);
        file.Reload();
        yield return new WaitForSecondsRealtime(1.5f);
        action?.Invoke();
    }

    private static IEnumerator DelayedActionRoutine(Action action)
    {
        yield return new WaitForSecondsRealtime(1.5f);
        action?.Invoke();
    }

    public static void DelayedAction(Action action)
    {
        if (action != null)
        {
            Marketplace._thistype.StartCoroutine(DelayedActionRoutine(action));
        }
    }

    public static void DelayReloadConfig(ConfigFile file, Action action = null)
    {
        Marketplace._thistype.StartCoroutine(DelayReloadConfigFile(file, action));
    }

    public static void CopyComponent<T>(T original, GameObject destination) where T : Component
    {
        Type type = original.GetType();
        Component copy = destination.AddComponent(type);
        try
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                       BindingFlags.Default | BindingFlags.DeclaredOnly;
            PropertyInfo[] pinfos = type.GetProperties(flags);
            foreach (PropertyInfo pinfo in pinfos)
                if (pinfo.CanWrite)
                    pinfo.SetValue(copy, pinfo.GetValue(original, null), null);

            FieldInfo[] fields = type.GetFields(flags);
            foreach (FieldInfo field in fields) field.SetValue(copy, field.GetValue(original));
        }
        catch
        {
            // ignored
        }
    }

    public static void CustomFindFloor(Vector3 p, out float height, float offset = 1f)
    {
        height = Physics.SphereCast(p + Vector3.up * offset, 0.05f, Vector3.down, out RaycastHit hitInfo, 100f,
            ZoneSystem.instance.m_solidRayMask)
            ? hitInfo.point.y
            : 0.0f;
    }

    public static string NoRichText(this string source)
    {
        return Regex.Replace(source, @"<[^>]*>", "");
    }

    public static string RemoveRichTextDynamicTag(string input, string tag)
    {
        while (true)
        {
            int index = input.IndexOf($"<{tag}=", StringComparison.Ordinal);
            if (index != -1)
            {
                int endIndex = input.Substring(index, input.Length - index).IndexOf('>');
                if (endIndex > 0)
                    input = input.Remove(index, endIndex + 1);
                continue;
            }

            input = RemoveRichTextTag(input, tag, false);
            return input;
        }
    }

    private static string RemoveRichTextTag(string input, string tag, bool isStart = true)
    {
        while (true)
        {
            int index = input.IndexOf(isStart ? $"<{tag}>" : $"</{tag}>", StringComparison.Ordinal);
            if (index != -1)
            {
                input = input.Remove(index, 2 + tag.Length + (!isStart).GetHashCode());
                continue;
            }

            if (isStart)
                input = RemoveRichTextTag(input, tag, false);
            return input;
        }
    }

    public static string RichTextFormatting(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";
        input = RemoveRichTextDynamicTag(input, "size");
        string[] split = input.Split(new[] { "<icon>" }, StringSplitOptions.RemoveEmptyEntries);
        if (split.Length == 0) return "";
        return split[0];
    }

    public static int CustomCountItems(string prefab, int level)
    {
        int num = 0;
        foreach (ItemDrop.ItemData itemData in Player.m_localPlayer.m_inventory.m_inventory)
        {
            if (itemData.m_dropPrefab.name == prefab && level == itemData.m_quality)
            {
                num += itemData.m_stack;
            }
        }

        return num;
    }

    public static Minimap.PinData GetCustomPin(Minimap.PinType type, Vector3 pos, float radius)
    {
        Minimap.PinData pinData = null;
        float num = 999999f;
        foreach (Minimap.PinData pinData2 in Minimap.instance.m_pins)
            if (pinData2.m_type == type)
            {
                float num2 = global::Utils.DistanceXZ(pos, pinData2.m_pos);
                if (num2 < radius && (num2 < num || pinData == null))
                {
                    pinData = pinData2;
                    num = num2;
                }
            }

        return pinData;
    }

    public static Texture2D CustomSize(this Texture2D texture2D, int targetX, int targetY)
    {
        RenderTexture rt = new RenderTexture(targetX, targetY, 24);
        RenderTexture.active = rt;
        Graphics.Blit(texture2D, rt);
        Texture2D result = new Texture2D(targetX, targetY);
        result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
        result.Apply();
        return result;
    }

    public static string AsBase64(this string s) => Convert.ToBase64String(Encoding.UTF8.GetBytes(s));

    public static int CustomCountItemsNoLevel(string prefab)
    {
        int num = 0;
        foreach (ItemDrop.ItemData itemData in Player.m_localPlayer.m_inventory.m_inventory)
        {
            if (itemData.m_dropPrefab.name == prefab)
            {
                num += itemData.m_stack;
            }
        }

        return num;
    }

    public static void CustomRemoveItems(string prefab, int amount, int level)
    {
        foreach (ItemDrop.ItemData itemData in Player.m_localPlayer.m_inventory.m_inventory)
        {
            if (itemData.m_dropPrefab.name == prefab && itemData.m_quality == level)
            {
                int num = Mathf.Min(itemData.m_stack, amount);
                itemData.m_stack -= num;
                amount -= num;
                if (amount <= 0)
                    break;
            }
        }

        Player.m_localPlayer.m_inventory.m_inventory.RemoveAll(x => x.m_stack <= 0);
        Player.m_localPlayer.m_inventory.Changed();
    }

    public static string SizeSuffix(this int value, int decimalPlaces = 1)
    {
        switch (value)
        {
            case < 0:
                return "-" + SizeSuffix(-value, decimalPlaces);
            case 0:
                return string.Format("{0:n" + decimalPlaces + "} bytes", 0);
        }

        int mag = (int)Math.Log(value, 1024);
        decimal adjustedSize = (decimal)value / (1L << (mag * 10));
        if (Math.Round(adjustedSize, decimalPlaces) < 1000)
            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        mag += 1;
        adjustedSize /= 1024;
        return string.Format("{0:n" + decimalPlaces + "} {1}",
            adjustedSize,
            SizeSuffixes[mag]);
    }

    public static void Decompress(this ZPackage pkg)
    {
        byte[] decompress = global::Utils.Decompress(pkg.GetArray());
        pkg.Clear();
        pkg.m_writer.Write(decompress);
        pkg.m_stream.Position = 0L;
    }

    public static void WriteFile(this string path, string data)
    {
        File.WriteAllText(path, data);
    }

    public static string ReadFile(this string path)
    {
        return File.ReadAllText(path);
    }

    private static readonly string[] SizeSuffixes =
        { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

    public static void Compress(this ZPackage pkg)
    {
        byte[] array = pkg.GetArray();
        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionLevel.Optimal))
                gzipStream.Write(array, 0, array.Length);
            byte[] compress = memoryStream.ToArray();
            pkg.Clear();
            pkg.m_writer.Write(compress);
        }
    }

    public static void InstantiateItem(GameObject main, int count, int rewardLevel)
    {
        Player p = Player.m_localPlayer;
        if (!p || main == null) return;

        if (main.GetComponent<ItemDrop>())
        {
            GameObject go = UnityEngine.Object.Instantiate(main,
                p.transform.position + p.transform.forward * 1.5f + Vector3.up * 1.5f,
                Quaternion.identity);
            ItemDrop itemDrop = go.GetComponent<ItemDrop>();
            itemDrop.m_itemData.m_stack = count;
            itemDrop.m_itemData.m_durability = itemDrop.m_itemData.GetMaxDurability();
            itemDrop.m_itemData.m_quality = rewardLevel;
            itemDrop.Save();
            if (p.m_inventory.CanAddItem(go))
            {
                p.m_inventory.AddItem(itemDrop.m_itemData);
                ZNetScene.instance.Destroy(go);
            }

            return;
        }

        if (main.GetComponent<Character>())
        {
            for (int i = 0; i < count; i++)
            {
                GameObject monster = UnityEngine.Object.Instantiate(main,
                    p.transform.position + p.transform.forward * 1.5f + Vector3.up * 1.5f, Quaternion.identity);
                monster.GetComponent<Character>().SetLevel(rewardLevel);
                if (monster.GetComponent<Tameable>())
                {
                    monster.GetComponent<Tameable>().Tame();
                }
            }
        }
    }

    public static Market_NPC.NPCcomponent GetClosestNPC(Vector3 pos)
    {
        List<Market_NPC.NPCcomponent> all = Market_NPC.NPCcomponent.ALL;
        Market_NPC.NPCcomponent result = null;
        float current = 999999f;
        foreach (Market_NPC.NPCcomponent npc in all)
        {
            float distance = Vector3.Distance(pos, npc.transform.position);
            if (distance >= current) continue;
            current = distance;
            result = npc;
        }

        return result;
    }

    public static Sprite TryFindIcon(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        if (AssetStorage.GlobalCachedSprites.TryGetValue(name, out Sprite img))
        {
            return img;
        }

        if (ZNetScene.instance.GetPrefab(name) is { } prefab)
        {
            if (prefab.GetComponent<ItemDrop>() is { } item)
                return item.m_itemData.GetIcon();
            if (prefab.GetComponent<Piece>() is { } piece)
                return piece.m_icon;
        }

        return null;
    }

    public static float GetPlayerSkillLevelCustom(string skillName)
    {
        if (!Enum.TryParse(skillName, out Skills.SkillType skill))
        {
            Skills.SkillDef SkillDef =
                Player.m_localPlayer.m_skills.GetSkillDef(
                    (Skills.SkillType)Mathf.Abs(skillName.GetStableHashCode()));
            if (SkillDef == null)
            {
                return -1;
            }

            skill = SkillDef.m_skill;
        }

        return Player.m_localPlayer.m_skills.GetSkillLevel(skill);
    }

    public static string LocalizeSkill(string name)
    {
        return Enum.TryParse(name, out Skills.SkillType _)
            ? Localization.instance.Localize("$skill_" + name.ToLower())
            : Localization.instance.Localize($"$skill_" + Mathf.Abs(name.GetStableHashCode()));
    }

    public static Sprite GetSkillIcon(string name)
    {
        if (!Enum.TryParse(name, out Skills.SkillType skill))
        {
            Skills.SkillDef SkillDef =
                Player.m_localPlayer.m_skills.GetSkillDef((Skills.SkillType)Mathf.Abs(name.GetStableHashCode()));
            return SkillDef == null ? AssetStorage.NullSprite : SkillDef.m_icon;
        }
        else
        {
            Skills.Skill SkillDef = Player.m_localPlayer.m_skills.GetSkill(skill);
            return SkillDef.m_info.m_icon;
        }
    }

    public static void LoadImageFromWEB(string url, Action<Sprite> callback)
    {
        if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out _)) return;
        if (!AssetStorage.GlobalCachedSprites.TryGetValue(url, out Sprite sprite))
        {
            Marketplace._thistype.StartCoroutine(_Internal_LoadImage(url, callback));
        }
        else
        {
            callback?.Invoke(sprite);
        }
    }

    private static IEnumerator _Internal_LoadImage(string url, Action<Sprite> callback)
    {
        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if (request.result is UnityWebRequest.Result.Success)
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            if (texture == null || texture.width == 0 || texture.height == 0) yield break;
            Texture2D newTempTexture = new Texture2D(texture.width, texture.height);
            newTempTexture.SetPixels(texture.GetPixels());
            newTempTexture.Apply();
            AssetStorage.GlobalCachedSprites[url] = Sprite.Create(newTempTexture,
                new Rect(0, 0, newTempTexture.width, newTempTexture.height), Vector2.zero);
            callback?.Invoke(AssetStorage.GlobalCachedSprites[url]);
        }
    }

    public static void IncreaseSkillEXP(string name, float expToAdd)
    {
        Skills.Skill skill;
        if (!Enum.TryParse(name, out Skills.SkillType found))
        {
            skill = Player.m_localPlayer.m_skills.GetSkill(
                (Skills.SkillType)Mathf.Abs(name.GetStableHashCode()));
        }
        else
        {
            skill = Player.m_localPlayer.m_skills.GetSkill(found);
        }

        if (skill != null)
        {
            while (expToAdd > 0)
            {
                float nextLevelRequirement = skill.GetNextLevelRequirement();
                if (skill.m_accumulator + expToAdd >= nextLevelRequirement)
                {
                    expToAdd -= nextLevelRequirement - skill.m_accumulator;
                    skill.m_accumulator = 0;
                    skill.m_level++;
                    skill.m_level = Mathf.Clamp(skill.m_level, 0f, 100f);
                }
                else
                {
                    skill.m_accumulator += expToAdd;
                    expToAdd = 0;
                }
            }
        }
    }

    public static void SetGOColors(GameObject go, Color color)
    {
        foreach (Renderer renderer in go.GetComponentsInChildren<MeshRenderer>()
                     .Concat(go.GetComponentsInChildren<SkinnedMeshRenderer>().Cast<Renderer>()))
        {
            foreach (Material mat in renderer.materials)
            {
                mat.color = color;
            }
        }
    }

    private const string CustomValue_Prefix = "kgMarketplaceValue@";

    public static void SetCustomValue(this Player p, string key, int value)
    {
        string toCheck = CustomValue_Prefix + key;
        p.m_customData[toCheck] = value.ToString();
    }

    public static void AddCustomValue(this Player p, string key, int value)
    {
        string toCheck = CustomValue_Prefix + key;
        if (p.m_customData.TryGetValue(toCheck, out string val))
        {
            if (int.TryParse(val, out int valInt))
            {
                p.m_customData[toCheck] = (valInt + value).ToString();
            }
            else
            {
                p.m_customData[toCheck] = value.ToString();
            }
        }
        else
        {
            p.m_customData[toCheck] = value.ToString();
        }
    }

    public static int GetCustomValue(this Player p, string key)
    {
        string toCheck = CustomValue_Prefix + key;
        if (p.m_customData.TryGetValue(toCheck, out string val))
        {
            if (int.TryParse(val, out int valInt))
            {
                return valInt;
            }
        }

        return 0;
    }

    public static void RemoveCustomValue(this Player p, string key)
    {
        string toCheck = CustomValue_Prefix + key;
        p.m_customData.Remove(toCheck);
    }


    public static bool HasFlagFast(this Quests_DataTypes.SpecialQuestTag value,
        Quests_DataTypes.SpecialQuestTag flag)
    {
        return (value & flag) != 0;
    }

    public static List<Trader_DataTypes.TraderItem> ToList(
        this Trader_DataTypes.TraderItem item)
    {
        return new List<Trader_DataTypes.TraderItem> { item };
    }

    public static bool HasFlagFast(this Buffer_DataTypes.WhatToModify flag,
        Buffer_DataTypes.WhatToModify other)
    {
        return (flag & other) != 0;
    }

    public static bool HasFlagFast(this TerritorySystem_DataTypes.AdditionalTerritoryFlags flag,
        TerritorySystem_DataTypes.AdditionalTerritoryFlags other)
    {
        return (flag & other) != 0;
    }

    public static bool HasFlagFast(this TerritorySystem_DataTypes.TerritoryFlags flag,
        TerritorySystem_DataTypes.TerritoryFlags other)
    {
        return (flag & other) != 0;
    }

    public static string RandomSplitSpace(this string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        string[] split = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (split.Length == 0) return "";
        return split[UnityEngine.Random.Range(0, split.Length)];
    }

    public static string ToTime(this int seconds)
    {
        TimeSpan t = TimeSpan.FromSeconds(seconds);
        string result = "";
        if (t.Days > 0) result += $"{t.Days:D2}d ";
        if (t.Hours > 0) result += $"{t.Hours:D2}h ";
        if (t.Minutes > 0) result += $"{t.Minutes:D2}m ";
        result += $"{t.Seconds:D2}s";
        return result;
    }

    public static string ToTime(this long seconds)
    {
        TimeSpan t = TimeSpan.FromSeconds(seconds);
        string result = "";
        if (t.Days > 0) result += $"{t.Days:D2}d ";
        if (t.Hours > 0) result += $"{t.Hours:D2}h ";
        if (t.Minutes > 0) result += $"{t.Minutes:D2}m ";
        result += $"{t.Seconds:D2}s";
        return result;
    }

    public static string ToTimeNoS(this long seconds)
    {
        TimeSpan t = TimeSpan.FromSeconds(seconds);
        string result = "";
        if (t.Days > 0) result += $"{t.Days:D2}d ";
        if (t.Hours > 0) result += $"{t.Hours:D2}h ";
        if (t.Minutes > 0) result += $"{t.Minutes:D2}m ";
        return result;
    }

    public static string Localize(this string text)
    {
        return string.IsNullOrEmpty(text) ? string.Empty : Localization.instance.Localize(text);
    }
}
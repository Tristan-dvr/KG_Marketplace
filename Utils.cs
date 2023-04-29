using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using BepInEx.Configuration;
using Marketplace.Modules.Buffer;
using Marketplace.Modules.NPC;
using Marketplace.Modules.Quests;
using Marketplace.Modules.TerritorySystem;
using Marketplace.Modules.Trader;
using UnityEngine.Rendering;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace Marketplace;

public static class Utils
{
    private static bool? _internal_isServer;
    internal static bool IsServer => _internal_isServer ??= SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
    public static bool IsDebug => Player.m_debugMode;
    
    public static void print(object obj, ConsoleColor color = ConsoleColor.DarkGreen)
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            ConsoleManager.SetConsoleColor(color);
            ConsoleManager.StandardOutStream.WriteLine($"[{DateTime.Now}] [kg.Marketplace] {obj}");
            ConsoleManager.SetConsoleColor(ConsoleColor.White);
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
                ConsoleManager.StandardOutStream.WriteLine($"[{c++}] {item}");
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

    public static void CustomFindFloor(Vector3 p, out float height)
    {
        height = Physics.SphereCast(p + Vector3.up * 1f, 0.05f, Vector3.down, out RaycastHit hitInfo, 100f,
            ZoneSystem.instance.m_solidRayMask) ? hitInfo.point.y : 0.0f;
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

    private static int CustomCountItemsNoLevel(string prefab)
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

    public static void CustomRemoveItems(string name, int amount, int level)
    {
        foreach (ItemDrop.ItemData itemData in Player.m_localPlayer.m_inventory.m_inventory)
        {
            if (itemData.m_dropPrefab.name == name && itemData.m_quality == level)
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
    
     public static void DecryptOldData(this string path)
    {
        if (File.Exists(path)) return;
        File.Create(path).Dispose();
        string encryptedOldData = path.Replace(".json", "");
        if (!File.Exists(encryptedOldData)) return;
        string decryptedData = encryptedOldData.ReadObfuscated();
        path.WriteClear(decryptedData);
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

    public static void WriteClear(this string path, string data)
    {
        File.WriteAllText(path, data);
    }

    public static string ReadClear(this string path)
    {
        return File.ReadAllText(path);
    }

    private static readonly string[] SizeSuffixes =
        { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

    private static string ReadObfuscated(this string path)
    {
        return Deobf(File.ReadAllText(path));
    }

    private static string Deobf(string data)
    {
        if (string.IsNullOrEmpty(data)) return "";
        byte[] baseBytes = Convert.FromBase64String(data);
        baseBytes = CreateEnc().CreateDecryptor().TransformFinalBlock(baseBytes, 0, baseBytes.Length);
        return Encoding.UTF8.GetString(baseBytes);
    }
    
    private static Aes CreateEnc()
    {
        Aes myAes = Aes.Create();
        myAes.Key = new byte[] { 205, 120, 49, 128, 197, 196, 75, 24, 63, 192, 191, 190, 189, 188, 187, 186 };
        myAes.IV = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        myAes.Padding = PaddingMode.ISO10126;
        myAes.Mode = CipherMode.ECB;
        return myAes;
    }


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
        if (AssetStorage.AssetStorage.GlobalCachedSprites.TryGetValue(name, out var img))
        {
            return img;
        }
        if (ZNetScene.instance.GetPrefab(name) is { } prefab && prefab.GetComponent<ItemDrop>() is { } item)
        {
            return item.m_itemData.GetIcon();
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

  
}
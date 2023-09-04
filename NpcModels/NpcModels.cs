using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace NpcModels
{
    [BepInPlugin(GUID, GUID, VERSION)]
    public class NpcModels : BaseUnityPlugin
    {
        private const string GUID = "kg.MarketplaceNpcModels";
        private const string VERSION = "1.0.0";

        private static List<GameObject> ToLoad;

        private static AssetBundle GetAssetBundle(string filename)
        {
            Assembly execAssembly = Assembly.GetExecutingAssembly();
            string resourceName = execAssembly.GetManifestResourceNames().Single(str => str.EndsWith(filename));
            using Stream stream = execAssembly.GetManifestResourceStream(resourceName)!;
            return AssetBundle.LoadFromStream(stream);
        }

        private void Awake()
        {
            ToLoad = GetAssetBundle("npcmodels").LoadAllAssets<GameObject>().ToList();
            new Harmony(GUID).PatchAll();
        }

        [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
        private static class ZNetScene_Awake_Patch
        {
            [UsedImplicitly]
            private static void Postfix(ZNetScene __instance)
            {
                ToLoad.ForEach(p =>
                {
                    __instance.m_namedPrefabs[p.name.GetStableHashCode()] = p;
                    IEnumerable<Renderer> mesh = p.GetComponentsInChildren<MeshRenderer>(true).Cast<Renderer>().Concat(p.GetComponentsInChildren<SkinnedMeshRenderer>());
                    foreach (Material material in mesh.SelectMany(m => m.materials))
                        material.shader = Shader.Find("Custom/Creature");
                });
            }
        }
    }
}
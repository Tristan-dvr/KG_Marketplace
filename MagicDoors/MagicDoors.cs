using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Speech.Recognition;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Windows.Speech;

namespace MagicDoors
{
    [BepInPlugin(GUID, GUID, VERSION)]
    public class MagicDoors : BaseUnityPlugin
    {
        private const string GUID = "kg.MagicDoors";
        private const string VERSION = "1.0.0";
        private static AssetBundle asset;
        private static DictationRecognizer Recognizer;
        private static GameObject MagicDoor;
        private static GameObject Activate_VFX;
        private static GameObject Deactivate_VFX;
        private static readonly int MainColor = Shader.PropertyToID("_MainColor");

        private void Awake()
        {
            asset = GetAssetBundle("kgmagicdoor");
            MagicDoor = asset.LoadAsset<GameObject>("kgMagicDoor");
            Activate_VFX = asset.LoadAsset<GameObject>("MagicDoor_Activate");
            Deactivate_VFX = asset.LoadAsset<GameObject>("MagicDoor_Deactivate");
            MagicDoor.AddComponent<MagicDoorComponent>();
            try
            {
                Recognizer = new DictationRecognizer(ConfidenceLevel.Low);
                Recognizer.AutoSilenceTimeoutSeconds = 0f;
                Recognizer.InitialSilenceTimeoutSeconds = 0f;
                Recognizer.DictationResult += OnVoiceRegognize;
                new Harmony(GUID).PatchAll();
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
                HarmonyMethod harmonyMethod = new HarmonyMethod(AccessTools.Method(typeof(ZNetScene_Awake_Patch),
                    nameof(ZNetScene_Awake_Patch.Postfix)));
                new Harmony(GUID).Patch(AccessTools.Method(typeof(ZNetScene), nameof(ZNetScene.Awake)),
                    postfix: harmonyMethod);
            }
        }

        private static string CurrentCommand = "";
        
        private static void OnVoiceRegognize(string text, ConfidenceLevel confidence)
        {
            if(Recognizer.Status != SpeechSystemStatus.Running) return;
            CurrentCommand += text;
            string limited = CurrentCommand.Length > 20 ? CurrentCommand.Substring(0, 20) : CurrentCommand;
            MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, limited + "...");
        }

        private static AssetBundle GetAssetBundle(string filename)
        {
            Assembly execAssembly = Assembly.GetExecutingAssembly();
            string resourceName = execAssembly.GetManifestResourceNames().Single(str => str.EndsWith(filename));
            using Stream stream = execAssembly.GetManifestResourceStream(resourceName);
            return AssetBundle.LoadFromStream(stream);
        }

        [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
        private static class ZNetScene_Awake_Patch
        {
            public static void Postfix(ZNetScene __instance)
            {
                __instance.m_namedPrefabs[MagicDoor.name.GetStableHashCode()] = MagicDoor;
                __instance.m_namedPrefabs[Activate_VFX.name.GetStableHashCode()] = Activate_VFX;
                __instance.m_namedPrefabs[Deactivate_VFX.name.GetStableHashCode()] = Deactivate_VFX;
                GameObject hammer = __instance.GetPrefab("Hammer");
                hammer.GetComponent<ItemDrop>().m_itemData.m_shared.m_buildPieces.m_pieces.Add(MagicDoor);
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.SetLocalPlayer))]
        private static class Player_Start_Patch
        {
            public static GameObject PlayerSpeaker;

            private static void Postfix()
            {
                PlayerSpeaker = Instantiate(asset.LoadAsset<GameObject>("magicdoorplayericon"),
                    Player.m_localPlayer.transform);
                PlayerSpeaker.SetActive(false);
                PlayerSpeaker.transform.localPosition += Vector3.up * 1.5f;
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.Update))]
        private static class Player_Update_Patch
        {
            public static bool GetAll;
            
            private static void Postfix(Player __instance)
            {
                if (__instance != Player.m_localPlayer || MagicDoorComponent.AllMagicDoors.Count == 0) return;
                if (!Player.m_localPlayer.TakeInput()) return;
                if (Input.GetKeyDown(KeyCode.V) && Recognizer.Status != SpeechSystemStatus.Running)
                {
                    CurrentCommand = "";
                    GetAll = false;
                    Player_Start_Patch.PlayerSpeaker.SetActive(true);
                    Recognizer.Start();
                }
                else if (Input.GetKeyUp(KeyCode.V) && Recognizer.Status == SpeechSystemStatus.Running)
                {
                    CurrentCommand = CurrentCommand.ToLower().Replace(" ", "");
                    var door = MagicDoorComponent.GetClosest(Player.m_localPlayer.transform.position);
                    if (door != null && door.CloseEnough(Player.m_localPlayer.transform.position))
                    {
                        door.CheckCommand(CurrentCommand);
                    }
                    CurrentCommand = "";
                    Player_Start_Patch.PlayerSpeaker.SetActive(false);
                    Recognizer.Stop();
                }

                if (Input.GetKeyDown(KeyCode.B) && Recognizer.Status != SpeechSystemStatus.Running)
                {
                    GetAll = true;
                    CurrentCommand = "";
                    Player_Start_Patch.PlayerSpeaker.SetActive(true);
                    Recognizer.Start();
                }
                else if (Input.GetKeyUp(KeyCode.B) && Recognizer.Status == SpeechSystemStatus.Running)
                {
                    CurrentCommand = CurrentCommand.ToLower().Replace(" ", "");
                    foreach (var door in MagicDoorComponent.AllMagicDoors.Where(door =>
                                 door.CloseEnough(Player.m_localPlayer.transform.position)))
                    {
                        door.CheckCommand(CurrentCommand);
                    }
                    CurrentCommand = "";
                    Player_Start_Patch.PlayerSpeaker.SetActive(false);
                    Recognizer.Stop();
                }
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.OnDeath))]
        private static class Player_OnDeath_Patch
        {
            private static void Postfix()
            {
                Player_Start_Patch.PlayerSpeaker?.SetActive(false);
                Recognizer.Stop();
                CurrentCommand = "";
            }
        }

        public class MagicDoorComponent : MonoBehaviour, TextReceiver
        {
            public static readonly List<MagicDoorComponent> AllMagicDoors = new List<MagicDoorComponent>();
            private ZNetView _znv;
            private LineRenderer LR;
            private Transform Gate;

            public static MagicDoorComponent GetClosest(Vector3 pos) => AllMagicDoors
                .OrderBy(x => Vector3.Distance(x.transform.position, pos)).FirstOrDefault();

            public bool CloseEnough(Vector3 pos) => Vector3.Distance(pos, transform.position) < 15f;

            private string OpenCommand
            {
                get => _znv.GetZDO().GetString("opencommand");
                set => _znv.GetZDO().Set("opencommand", value);
            }

            private string CloseCommand
            {
                get => _znv.GetZDO().GetString("closecommand");
                set => _znv.GetZDO().Set("closecommand", value);
            }

            private Color Color
            {
                get => Utils.Vec3ToColor(_znv.GetZDO().GetVec3("color", Vector3.one));
                set => _znv.GetZDO().Set("color", Utils.ColorToVec3(value));
            }

            private bool IsOpened
            {
                get => _znv.GetZDO().GetBool("isopened");
                set => _znv.GetZDO().Set("isopened", value);
            }

            private void OnDestroy()
            {
                AllMagicDoors.Remove(this);
            }

            private void Awake()
            {
                _znv = GetComponent<ZNetView>();
                if (!_znv.IsValid()) return;
                LR = transform.Find("LR").GetComponent<LineRenderer>();
                AllMagicDoors.Add(this);
                Gate = transform.Find("effect");
                Gate.gameObject.SetActive(!IsOpened);
                _znv.Register<int>("MagicDoor_Op", RPC_Open);
                if (string.IsNullOrEmpty(OpenCommand))
                {
                    TextInput.instance.RequestText(this, "Enter open command", 20);
                }
            }

            enum Op
            {
                Open,
                Close
            }

            public void CheckCommand(string text)
            {
                if (text.Contains(OpenCommand))
                {
                    Operation(Op.Open);
                }
                else if (text.Contains(CloseCommand))
                {
                    Operation(Op.Close);
                }
            }

            private void Operation(Op op)
            {
                _znv.InvokeRPC(ZNetView.Everybody, "MagicDoor_Op", (int)op);
            }

            private void RPC_Open(long sender, int i_op)
            {
                Op op = (Op)i_op;
                if (_znv.IsOwner())
                {
                    IsOpened = op == Op.Open;
                    Instantiate(op == Op.Open ? Activate_VFX : Deactivate_VFX, Gate.position, Quaternion.identity);
                }

                Gate.gameObject.SetActive(op == Op.Close);
            }


            private void LateUpdate()
            {
                if (!_znv.IsValid() || Recognizer == null) return;
                if (!Player.m_localPlayer)
                {
                    LR.enabled = false;
                    return;
                }

                if (Player_Update_Patch.GetAll)
                {
                    if (CloseEnough(Player.m_localPlayer.transform.position) &&
                        Recognizer.Status == SpeechSystemStatus.Running)
                    {
                        LR.enabled = true;
                        LR.SetPosition(0, transform.position + new Vector3(0, 2.6f, 0));
                        LR.SetPosition(1, Player.m_localPlayer.transform.position + Vector3.up * 4.1f);
                    }
                    else
                    {
                        LR.enabled = false;
                    }
                }
                else
                {
                    if (GetClosest(Player.m_localPlayer.transform.position) == this &&
                        CloseEnough(Player.m_localPlayer.transform.position) &&
                        Recognizer.Status == SpeechSystemStatus.Running)
                    {
                        LR.enabled = true;
                        LR.SetPosition(0, transform.position + new Vector3(0, 2.6f, 0));
                        LR.SetPosition(1, Player.m_localPlayer.transform.position + Vector3.up * 4.1f);
                    }
                    else
                    {
                        LR.enabled = false;
                    }
                }
            }

            public string GetText()
            {
                return "Enter Password";
            }

            private IEnumerator RequestCloseCommandNextFrame()
            {
                yield return new WaitForEndOfFrame();
                TextInput.instance.RequestText(this, "Enter close command", 20);
            }

            public void SetText(string text)
            {
                if (string.IsNullOrEmpty(OpenCommand))
                {
                    OpenCommand = text.ToLower().Replace(" ", "");
                    StartCoroutine(RequestCloseCommandNextFrame());
                }
                else
                {
                    CloseCommand = text.ToLower().Replace(" ", "");
                }
            }
        }
    }
}
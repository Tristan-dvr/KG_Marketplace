using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using fastJSON;
using HarmonyLib;
using UnityEngine;

namespace kg.ServerControl
{
    [BepInPlugin(GUID, GUID, VERSION)]
    public class ServerControl : BaseUnityPlugin
    {
        private const string GUID = "kg.ServerControl_WEB";
        private const string VERSION = "1.1.0";
        private const string POST_REQUEST = "https://kg-dev.xyz/API/RCON.php";
        private readonly ConcurrentQueue<ToInvoke> _queue = new ConcurrentQueue<ToInvoke>();
        private ConfigEntry<string> IDENTIFIER;
        private ConfigEntry<int> SECONDS_BETWEEN_REQUESTS;
        private FileSystemWatcher FSW;

        private enum Result : byte
        {
            Pending = 0,
            Sent = 1,
            Success = 2,
            Failed = 3,
            Timeout = 4
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        private struct ToInvoke
        {
            public long ID;
            public CommandExecution.Command Command;
            public string Arguments;

            public override string ToString()
            {
                return $"UID: {ID}, Action: {Command}, Args: {Arguments}";
            }
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        private struct Response
        {
            public long ID;
            public Result Result;
            public string Message;

            private Response(long uid, byte result, string message)
            {
                ID = uid;
                Result = (Result)result;
                Message = message;
            }

            public static Response Failed(long uid, string message)
            {
                return new(uid, (byte)Result.Failed, message);
            }

            public static Response Success(long uid, string message)
            {
                return new(uid, (byte)Result.Success, message);
            }

            public override string ToString()
            {
                return $"ID: {ID}, Result: {Result}, Message: {Message}";
            }
        }

        private void Awake()
        {
            JSON.Parameters = new JSONParameters
            {
                UseExtensions = false,
                SerializeNullValues = false,
                DateTimeMilliseconds = false,
                UseUTCDateTime = true,
                UseOptimizedDatasetSchema = true,
                UseValuesOfEnums = true,
            };
            CommandExecution.Init();
            IDENTIFIER = Config.Bind("ServerControl", "ID", "Put ID Here", "ServerControl ID (32 symbols)");
            SECONDS_BETWEEN_REQUESTS = Config.Bind("RCON", "Seconds Between Requests", 4, "Seconds between requests to the RCON server");
            SECONDS_BETWEEN_REQUESTS.Value = Mathf.Clamp(SECONDS_BETWEEN_REQUESTS.Value, 2, 20);
            new Harmony(GUID).PatchAll();
            FSW = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(Config.ConfigFilePath),
                Filter = Path.GetFileName(Config.ConfigFilePath),
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };
            FSW.Changed += (_, _) =>
            {
                if (LastConfigReload.AddSeconds(3) > DateTime.Now) return;
                LastConfigReload = DateTime.Now;
                print("Config changed, reloading...");
                Config.Reload();
                SECONDS_BETWEEN_REQUESTS.Value = Mathf.Clamp(SECONDS_BETWEEN_REQUESTS.Value, 2, 20);
            };
        }

        private DateTime LastConfigReload = DateTime.Now;

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            UpdateGet(dt);
            QueueProcess();
        }

        private void QueueProcess()
        {
            if (IDENTIFIER.Value.Length != 32 || !ZNet.instance) return;
            if (_queue.Count > 0)
            {
                List<Response> results = new List<Response>();
                while (_queue.TryDequeue(out ToInvoke invoke))
                {
                    Response resp = ProcessInvoke(invoke);
                    results.Add(resp);
                }

                if (results.Count > 0)
                {
                    string json = JSON.ToJSON(results);
                    Task.Run(async () =>
                    {
                        HttpClient client = new HttpClient();
                        Dictionary<string, string> values = new Dictionary<string, string>
                        {
                            { "id", IDENTIFIER.Value },
                            { "type", "update" },
                            { "json", json }
                        };
                        FormUrlEncodedContent content = new FormUrlEncodedContent(values);
                        await client.PostAsync(POST_REQUEST, content);
                    });
                }
            }
        }

        private float _timerGET;

        private void UpdateGet(float dt)
        {
            if (IDENTIFIER.Value.Length != 32 || !ZNet.instance) return;
            _timerGET += dt;
            if (_timerGET >= SECONDS_BETWEEN_REQUESTS.Value)
            {
                _timerGET = 0;
                Task.Run(async () =>
                {
                    HttpClient client = new HttpClient();
                    HttpResponseMessage response =
                        await client.GetAsync(POST_REQUEST + "?id=" + IDENTIFIER.Value + "&type=valheim");
                    string responseString = await response.Content.ReadAsStringAsync();
                    try
                    {
                        JSON.ToObject<List<ToInvoke>>(responseString).ForEach(_queue.Enqueue);
                    }
                    catch (Exception ex)
                    {
                        //print($"Error while parsing response: {ex}", ConsoleColor.Red);
                    }
                });
            }
        }

        private Response ProcessInvoke(ToInvoke invoke)
        {
            string args = invoke.Arguments ?? string.Empty;
            if (CommandExecution.Execute(invoke.Command, args, out string msg))
                return Response.Success(invoke.ID, msg);
            return Response.Failed(invoke.ID, msg);
        }

        public static void print(object obj, ConsoleColor color = ConsoleColor.DarkGreen)
        {
            if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                ConsoleManager.SetConsoleColor(color);
                ConsoleManager.StandardOutStream.WriteLine($"[{DateTime.Now}] [kg.ServerControl_WEB] {obj}");
                ConsoleManager.SetConsoleColor(ConsoleColor.White);
                foreach (ILogListener logListener in BepInEx.Logging.Logger.Listeners)
                    if (logListener is DiskLogListener { LogWriter: not null } bepinexlog)
                        bepinexlog.LogWriter.WriteLine($"[{DateTime.Now}] [kg.ServerControl_WEB] {obj}");
            }
            else
            {
                MonoBehaviour.print($"[{DateTime.Now}] [kg.ServerControl_WEB] " + obj);
            }
        }
    }
}
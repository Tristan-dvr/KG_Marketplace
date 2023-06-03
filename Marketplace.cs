using LocalizationManager;
using Marketplace.Paths;
using Microsoft.CSharp;
using UnityEngine.Rendering;

namespace Marketplace
{
    [BepInPlugin(GUID, PluginName, PluginVersion)]
    [BepInDependency("org.bepinex.plugins.groups", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("org.bepinex.plugins.jewelcrafting", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("Soulcatcher", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInIncompatibility("org.bepinex.plugins.valheim_plus")]
    public class Marketplace : BaseUnityPlugin
    {
        private const string GUID = "MarketplaceAndServerNPCs";
        private const string PluginName = "MarketplaceAndServerNPCs";
        public const string PluginVersion = "8.7.0";
        internal static Marketplace _thistype;
        private static readonly Harmony _harmony = new(GUID);
        private static FileSystemWatcher FSW;
        public static Action Global_Updator;
        public static Action Global_FixedUpdator;
        public static Action Global_OnGUI_Updator;
        public static Type TempJewelcraftingType;
        public static Type TempProfessionsType;

        public static readonly ConfigSync configSync = new(GUID)
        {
            DisplayName = GUID, ModRequired = true, MinimumRequiredVersion = PluginVersion,
            CurrentVersion = PluginVersion
        };

        public enum WorkingAs
        {
            Client,
            Server,
            Both
        }

        public static WorkingAs WorkingAsType;

        private void Awake()
        {
            WorkingAsType = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null
                ? WorkingAs.Server
                : Config.Bind("General", "Use Marketplace Locally", false, "Enable Market Local Usage").Value
                    ? WorkingAs.Both
                    : WorkingAs.Client;
            Utils.print($"Marketplace Working As: {WorkingAsType}");
            _thistype = this;
            Type.GetType("Groups.Initializer, kg.Marketplace")!.GetMethod("Init")!.Invoke(null, null);
            HarmonyLib.Tools.Logger.ChannelFilter = HarmonyLib.Tools.Logger.LogChannel.Error;
            Localizer.Load();
            TempJewelcraftingType = Type.GetType("Jewelcrafting.Jewelcrafting, Jewelcrafting")!;
            TempProfessionsType = Type.GetType("Professions.Professions, Professions")!;
            JSON.Parameters = new JSONParameters
            {
                UseExtensions = false,
                SerializeNullValues = false,
                DateTimeMilliseconds = false,
                UseUTCDateTime = true,
                UseOptimizedDatasetSchema = true,
                UseValuesOfEnums = true,
            };
            IEnumerable<KeyValuePair<Market_Autoload, Type>> toAutoload = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttribute<Market_Autoload>() != null)
                .Select(x => new KeyValuePair<Market_Autoload, Type>(x.GetCustomAttribute<Market_Autoload>(), x))
                .OrderBy(x => x.Key.priority).Where(x => WorkingAsType switch
                {
                    WorkingAs.Client => x.Key.type != Market_Autoload.Type.Server,
                    WorkingAs.Server => x.Key.type != Market_Autoload.Type.Client,
                    _ => true
                });
            foreach (KeyValuePair<Market_Autoload, Type> autoload in toAutoload)
            {
                if (autoload.Key.OnWatcherNames != null && autoload.Key.OnWatcherMethods != null &&
                    autoload.Key.OnWatcherNames.Length == autoload.Key.OnWatcherMethods.Length)
                {
                    for (int i = 0; i < autoload.Key.OnWatcherNames.Length; i++)
                    {
                        MethodInfo configWatcherMethod = autoload.Value.GetMethod(autoload.Key.OnWatcherMethods[i],
                            BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
                        if (configWatcherMethod == null)
                            Utils.print(
                                $"Error loading {autoload.Value.Name} class, method {autoload.Key.OnWatcherMethods[i]} not found",
                                ConsoleColor.Red);
                        else
                            FSW_Lookup.Add(autoload.Key.OnWatcherNames[i],
                                () => configWatcherMethod.Invoke(null, null));
                    }
                }

                MethodInfo method = autoload.Value.GetMethod(autoload.Key.InitMethod ?? "None",
                    BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
                if (method == null)
                {
                    Utils.print(
                        $"Error loading {autoload.Value.Name} class, method {autoload.Key.InitMethod} not found",
                        ConsoleColor.Red);
                    continue;
                }

                try
                {
                    method.Invoke(null, null);
                }
                catch (Exception ex)
                {
                    Utils.print($"Autoload exception on method {method}\n:{ex}", ConsoleColor.Red);
                }
            }

            InitFSW(Market_Paths.MainPath);
            AccessTools.GetTypesFromAssembly(Assembly.GetExecutingAssembly())
                .Where(t => WorkingAsType switch
                {
                    WorkingAs.Client => t.GetCustomAttribute<ServerOnlyPatch>() == null,
                    WorkingAs.Server => t.GetCustomAttribute<ClientOnlyPatch>() == null,
                    _ => true
                })
                .Where(t => t.GetCustomAttribute<ConditionalPatch>() is not { } cond || cond.Check(t))
                .Do(type => _harmony.CreateClassProcessor(type).Patch());
        }

        private void Update() => Global_Updator?.Invoke();
        private void OnGUI() => Global_OnGUI_Updator?.Invoke();
        private void FixedUpdate() => Global_FixedUpdator?.Invoke();

        private static void InitFSW(string folderPath)
        {
            if (WorkingAsType is WorkingAs.Client) return;
            try
            {
                FSW = new FileSystemWatcher(folderPath)
                {
                    Filter = "*.*",
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = true,
                    SynchronizingObject = ThreadingHelper.SynchronizingObject
                };
                FSW.Changed += MarketplaceConfigChanged;
            }
            catch (Exception ex)
            {
                Utils.print($"Can't start FSW, error: {ex}", ConsoleColor.Red);
            }
        }

        private static readonly Dictionary<string, Action> FSW_Lookup = new();
        private static readonly Dictionary<string, DateTime> LastConfigChangeTime = new();

        private static void MarketplaceConfigChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed) return;

            string fName = Path.GetFileName(e.Name)!;

            string folderPath = Path.GetDirectoryName(e.FullPath)!;
            if (folderPath.Contains(Market_Paths.AdditionalConfigsQuestsFolder))
                fName = Path.GetFileName(Market_Paths.QuestDatabasePath);
            else if (folderPath.Contains(Market_Paths.AdditionalConfigsDialoguesFolder))
                fName = Path.GetFileName(Market_Paths.NpcDialoguesConfig);
            else if (folderPath.Contains(Market_Paths.AdditionalCondfigsTerritoriesFolder))
                fName = Path.GetFileName(Market_Paths.TerritoriesConfigPath);

            if (!FSW_Lookup.TryGetValue(fName, out Action action)) return;
            if (!ZNet.instance || !ZNet.instance.IsServer())
            {
                Utils.print($"FSW: Not a server, ignoring ({e.Name})", ConsoleColor.Red);
                return;
            }
            if (!LastConfigChangeTime.ContainsKey(fName)) LastConfigChangeTime[fName] = DateTime.MinValue;
            if (LastConfigChangeTime[fName] > DateTime.Now.AddSeconds(-5)) return;
            LastConfigChangeTime[fName] = DateTime.Now;
            Utils.DelayedAction(action);
        }
    }
}
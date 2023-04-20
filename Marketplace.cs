using LocalizationManager;
using Marketplace.Paths;
using ServerSync;

namespace Marketplace
{
    [BepInPlugin(GUID, PluginName, PluginVersion)]
    [BepInDependency("org.bepinex.plugins.groups", BepInDependency.DependencyFlags.SoftDependency)]
    [VerifyKey("KGvalheim/Marketplace", LicenseMode.DedicatedServer)]
    public class Marketplace : BaseUnityPlugin
    {
        private const string GUID = "MarketplaceAndServerNPCs";
        private const string PluginName = "MarketplaceAndServerNPCs";
        private const string PluginVersion = "8.3.5";
        internal static Marketplace _thistype;
        private static readonly Harmony _harmony = new(GUID);
        private static FileSystemWatcher FSW;
        public static Action Global_Updator;
        public static Action Global_FixedUpdator;
        public static Action Global_OnGUI_Updator;
        public static Type TempJewelcraftingType;

        public static readonly ConfigSync configSync = new(GUID)
        {
            DisplayName = GUID, ModRequired = true, MinimumRequiredVersion = PluginVersion,
            CurrentVersion = PluginVersion
        };
        
        private void Awake()
        {
            _thistype = this;
            Type.GetType("Groups.Initializer, kg.Marketplace")!.GetMethod("Init")!.Invoke(null, null);
            HarmonyLib.Tools.Logger.ChannelFilter = HarmonyLib.Tools.Logger.LogChannel.Error;
            Localizer.Load();
            TempJewelcraftingType = Type.GetType("Jewelcrafting.Jewelcrafting, Jewelcrafting");
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
                .OrderBy(x => x.Key.priority)
                .Where(x => Utils.IsServer
                    ? x.Key.type != Market_Autoload.Type.Client
                    : x.Key.type != Market_Autoload.Type.Server);
            foreach (var autoload in toAutoload)
            {
                if (autoload.Key.OnWatcherNames != null && autoload.Key.OnWatcherMethods != null &&
                    autoload.Key.OnWatcherNames.Length == autoload.Key.OnWatcherMethods.Length)
                {
                    for (int i = 0; i < autoload.Key.OnWatcherNames.Length; i++)
                    {
                        MethodInfo configWatcherMethod = autoload.Value.GetMethod(autoload.Key.OnWatcherMethods[i],
                            BindingFlags.NonPublic | BindingFlags.Static);
                        if (configWatcherMethod == null)
                            Utils.print(
                                $"Error loading {autoload.Value.Name} class, method {autoload.Key.OnWatcherMethods[i]} not found",
                                ConsoleColor.Red);
                        else
                            FSW_Lookup.Add(autoload.Key.OnWatcherNames[i],
                                () => configWatcherMethod.Invoke(null, null));
                    }
                }

                MethodInfo method = autoload.Value.GetMethod(autoload.Key.InitMethod ?? "None", BindingFlags.NonPublic | BindingFlags.Static);
                if (method == null) 
                {
                    Utils.print(
                        $"Error loading {autoload.Value.Name} class, method {autoload.Key.InitMethod} not found",
                        ConsoleColor.Red);
                    continue;
                }

                method.Invoke(null, null);
            }
            InitFSW(Market_Paths.MainPath);
            AccessTools.GetTypesFromAssembly(Assembly.GetExecutingAssembly())
                .Where(t => Utils.IsServer
                    ? t.GetCustomAttribute<ClientOnlyPatch>() == null
                    : t.GetCustomAttribute<ServerOnlyPatch>() == null)
                .Do(type => _harmony.CreateClassProcessor(type).Patch());
        }

        private void Update() => Global_Updator?.Invoke();
        private void OnGUI() => Global_OnGUI_Updator?.Invoke();
        private void FixedUpdate() => Global_FixedUpdator?.Invoke();
        
        private void InitFSW(string folderPath)
        {
            if (!Utils.IsServer) return;
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
                Utils.print("FSW started");
            }
            catch (Exception ex)
            {
                Utils.print($"Can't start FSW, error: {ex}", ConsoleColor.Red);
            }
        }

        private static readonly Dictionary<string, Action> FSW_Lookup = new();
        private static DateTime LastConfigChangeTime;

        private static void MarketplaceConfigChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed || !ZNetScene.instance) return;
            if (LastConfigChangeTime > DateTime.Now.AddSeconds(-5)) return;
            LastConfigChangeTime = DateTime.Now;
            string fileName = Path.GetFileName(e.Name);
            if (FSW_Lookup.TryGetValue(fileName, out var action))
            {
                Utils.DelayedAction(action);
            }
        }
    }
}
using Object = UnityEngine.Object;
namespace Marketplace.Modules.TerritorySystem;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Client)]
public static class ZoneVisualizer
{
    private static GameObject ZoneVisualizer_Square, ZoneVisualizer_Circle;
    private static readonly List<GameObject> Visualizers = new();

    private static void OnInit()
    {
        ZoneVisualizer_Square = AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("SquareZoneVisualizer");
        ZoneVisualizer_Circle = AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("CircleZoneVisualizer");
    }

    private static void On()
    {
        Visualizers.ForEach(x => { if(x) Object.Destroy(x); });
        Visualizers.Clear();

        foreach (var territory in TerritorySystem_DataTypes.TerritoriesData.Value)
        {
            GameObject visualizer = Object.Instantiate(territory.Type is TerritorySystem_DataTypes.TerritoryType.Circle
                ? ZoneVisualizer_Circle
                : ZoneVisualizer_Square);
            
            visualizer.transform.position = territory.Type switch
            {
                TerritorySystem_DataTypes.TerritoryType.Circle => territory.Pos3D(),
                TerritorySystem_DataTypes.TerritoryType.Square => territory.Pos3D(),
                TerritorySystem_DataTypes.TerritoryType.Custom => territory.Pos3D() + new Vector3(territory.Xlength / 2f, 0, territory.Ylength / 2f),
                _ => territory.Pos3D()
            };
            
            float heightScale = territory.HeightBounds.Item2 - territory.HeightBounds.Item1;
            visualizer.transform.localScale = territory.Type switch
            {
                TerritorySystem_DataTypes.TerritoryType.Circle => new Vector3(territory.Radius * 2f, heightScale, territory.Radius * 2f),
                TerritorySystem_DataTypes.TerritoryType.Square => new Vector3(territory.Radius * 2f, heightScale, territory.Radius * 2f),
                TerritorySystem_DataTypes.TerritoryType.Custom => new Vector3(territory.Xlength, heightScale, territory.Ylength),
                _ => new Vector3(territory.Radius, heightScale, territory.Radius),
            };
            
            Color color = territory.GetColor();
            color.a = 0.2f;
            visualizer.GetComponentInChildren<MeshRenderer>().material.color = color;
            Visualizers.Add(visualizer);
        }
        EnvMan.instance?.transform.Find("Clouds").gameObject.SetActive(false);
    }

    private static void Off()
    {
        Visualizers.ForEach(x => { if(x) Object.Destroy(x); });
        Visualizers.Clear();
        EnvMan.instance?.transform.Find("Clouds").gameObject.SetActive(false);
    }
    
    [HarmonyPatch(typeof(FejdStartup),nameof(FejdStartup.Awake))]
    private static class FejdStartup_Awake_Patch
    {
        private static void Postfix() => Off();
    }
    
    [HarmonyPatch(typeof(Terminal),nameof(Terminal.InitTerminal))]
    [ClientOnlyPatch]
    private static class Terminal_InputText_Patch
    {
        private static void Postfix()
        {
            new Terminal.ConsoleCommand("zonevisualizer", "Toggle zone visualizer", (_) =>
            {
                if (Visualizers.Count == 0)
                    On();
                else
                    Off();
            });
        }
    }
    
}
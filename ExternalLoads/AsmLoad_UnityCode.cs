namespace Marketplace.AssetStorage;

[Market_Autoload(Market_Autoload.Type.Both, Market_Autoload.Priority.Init, "OnInit")]
public static class AsmLoad_UnityCode
{
    // ReSharper disable once UnusedMember.Global
    private static void OnInit()
    {
        LoadAsm("Marketplace_TransmogCode");
        LoadAsm("Marketplace_UnityCode");
    }
    private static void LoadAsm(string name)
    {
        Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Marketplace.Resources." + name + ".dll");
        byte[] buffer = new byte[stream!.Length];
        stream.Read(buffer, 0, buffer.Length); 
        try
        {
            Assembly.Load(buffer);
            stream.Dispose();
        }
        catch(Exception ex)
        {
            Utils.print($"Error loading {name} assembly\n:{ex}", ConsoleColor.Red);
        }
    }
}
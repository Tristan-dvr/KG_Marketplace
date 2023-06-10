using UnityEngine.Networking;

namespace Marketplace.Modules.ServerInfo;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Normal, "OnInit")]
public static class ServerInfo_Main_Client
{
    private static void OnInit()
    {
        ServerInfo_UI.Init();
        ServerInfo_DataTypes.ServerInfoData.ValueChanged += OnInfoUpdate;
        Marketplace.Global_Updator += Update;
        GameEvents.OnPlayerFirstSpawn += OnPlayerFirstSpawn;
    }

    private static void OnPlayerFirstSpawn()
    {
        if (ServerInfo_DataTypes.ServerInfoData.Value.ContainsKey("onplayerfirstspawn"))
            ServerInfo_UI.Show("onplayerfirstspawn", "");
    }

    private static void Update()
    { 
        if (!Input.GetKeyDown(KeyCode.Escape) || !ServerInfo_UI.IsPanelVisible()) return;
        ServerInfo_UI.Hide();
        Menu.instance.OnClose();
    }

    private static Coroutine LoadImagesRoutine;

    private static void OnInfoUpdate()
    {
        if (LoadImagesRoutine != null)
            Marketplace._thistype.StopCoroutine(LoadImagesRoutine);
        LoadImagesRoutine = Marketplace._thistype.StartCoroutine(LoadQuestImages(
            ServerInfo_DataTypes.ServerInfoData.Value.Values.SelectMany(x => x.infoQueue)
                .Where(x => x.Type == ServerInfo_DataTypes.ServerInfoQueue.Info.InfoType.Image)));

        if (ServerInfo_UI.IsPanelVisible()) ServerInfo_UI.Reload();
    }

    private static IEnumerator LoadQuestImages(IEnumerable<ServerInfo_DataTypes.ServerInfoQueue.Info> urls)
    {
        yield return new WaitForSeconds(3f);
        foreach (ServerInfo_DataTypes.ServerInfoQueue.Info url in urls)
        {
            if (string.IsNullOrEmpty(url.Text)) continue;
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url.Text);
            yield return request.SendWebRequest();
            if (request.result is UnityWebRequest.Result.Success)
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                if (texture == null || texture.width == 0 || texture.height == 0) continue;
                Texture2D newTempTexture = new Texture2D(texture.width, texture.height);
                newTempTexture.SetPixels(texture.GetPixels());
                newTempTexture.Apply();
                Sprite sprite = Sprite.Create(newTempTexture,
                    new Rect(0, 0, newTempTexture.width, newTempTexture.height), Vector2.zero);
                url.SetSprite(sprite);
            }
        }
    }
}
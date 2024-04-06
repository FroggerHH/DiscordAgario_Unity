using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public static class Utils
{
    private static Camera m_camera;

    public static Camera Camera => m_camera ??= Camera.main;

    public static Vector3 GetRandomPos()
    {
        var halfMap = GetMapSize() / 2.2f;
        return new(Random.Range(-halfMap, halfMap), Random.Range(-halfMap, halfMap) * 0.9f, 0);
    }

    public static int GetMapSize() => 50;

    public static void Info(string msg) => UnityEngine.Debug.Log($"[<color=#B6D344>INFO</color>] {msg}");

    public static void Success(string msg) => UnityEngine.Debug.Log($"[<color=green>SUCCESS</color>] {msg}");

    public static void Fail(string msg) => UnityEngine.Debug.Log($"[<color=purple>FAIL</color>] {msg}");

    public static void Error(string msg) => UnityEngine.Debug.LogError($"[<color=red>ERROR</color>] {msg}");

    public static void Debug(string msg) => UnityEngine.Debug.Log($"[<color=#02B295>DEBUG</color>] {msg}");
    
    public static void LoadImageFromWEB(string url, Action<Sprite> callback)
    {
        if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out _)) return;

        LobbyUI.Instance.StartCoroutine(LoadImage_IEnumerator(url, callback));
    }

    private static IEnumerator LoadImage_IEnumerator(string url, Action<Sprite> callback)
    {
        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if (request.result is UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            if (texture.width == 0 || texture.height == 0) yield break;
            Texture2D temp = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
            temp.SetPixels(texture.GetPixels());
            temp.Apply();
            var sprite = Sprite.Create(texture, new(0, 0, texture.width, texture.height), new(0.5f, 0.5f));
            callback?.Invoke(sprite);
        }
    }
}
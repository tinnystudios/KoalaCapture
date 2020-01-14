using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Android;

public class CaptureController : MonoBehaviour
{
    public Action OnCaptureBegin;
    public Action OnCaptureEnd;

    private void Start()
    {
        // External storage write permission actually allows you to modify public folders within the phone.
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
    }
    public void Capture()
    {
        StartCoroutine(CaptureRoutine());
    }

    private IEnumerator CaptureRoutine()
    {
        OnCaptureBegin?.Invoke();

        var fileName = $"KoalaCapture-{Guid.NewGuid()}.png";
        ScreenCapture.CaptureScreenshot(fileName);

        var path = Application.persistentDataPath;
        path = path.Replace("Android/data/com.jay.koalacapture/files", "");
        var tempPath = $"{Application.persistentDataPath}/{fileName}";

        yield return new WaitUntil(() => File.Exists(tempPath));

        var fullPath = path + $"DCIM/Camera/{fileName}";

        Debug.Log(Application.persistentDataPath);
        Debug.Log("Save Path: " + fullPath);

        var bytes = File.ReadAllBytes(tempPath);
        File.WriteAllBytes(fullPath, bytes);

        yield return new WaitUntil(() => File.Exists(fullPath));
        RefreshFile(fullPath);

        OnCaptureEnd?.Invoke();
    }

    /// <summary>
    /// Android Media Gallery requires the file to be indexed into order for it to show up in Media.
    /// </summary>
    /// <param name="fullPath"></param>
    public void RefreshFile(string fullPath) 
    {
        AndroidJavaClass classPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject objActivity = classPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaClass classUri = new AndroidJavaClass("android.net.Uri");
        AndroidJavaObject objIntent = new AndroidJavaObject("android.content.Intent", new object[2] { "android.intent.action.MEDIA_SCANNER_SCAN_FILE", classUri.CallStatic<AndroidJavaObject>("parse", "file://" + fullPath) });
        objActivity.Call("sendBroadcast", objIntent);
    }
}

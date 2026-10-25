using System.IO;
using UnityEditor;
using UnityEngine;

// ============================================================
// filename   : ScreenshotTool.cs
// description: 플레이 중 게임뷰 화면만(UI 포함, 에디터 창 제외) PNG로 저장한다.
//              - 단축키 F12 (Tools ▸ Vertex ▸ 스크린샷)
//              - 디버그 패널 맨 위의 버튼 · 지연 촬영
//              저장 위치: 프로젝트 루트/Screenshots/yyyyMMdd_HHmmss.png (git에는 올라가지 않는다)
// ============================================================
public static class ScreenshotTool
{
    public static string Folder => Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Screenshots");
    private static double _pendingAt = -1;

    [MenuItem("Tools/Vertex/스크린샷 _F12")]
    public static void Capture()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[스크린샷] 플레이 중에만 찍을 수 있어요."); return; }
        Directory.CreateDirectory(Folder);
        string file = Path.Combine(Folder, System.DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".png");
        ScreenCapture.CaptureScreenshot(file); // 게임뷰 해상도 그대로, 그 프레임이 끝날 때 저장
        Debug.Log($"[스크린샷] {file}");
    }

    // 몇 초 뒤에 찍는다 — 연출·배너를 기다렸다 찍을 때
    public static void CaptureAfter(float seconds)
    {
        _pendingAt = EditorApplication.timeSinceStartup + Mathf.Max(0f, seconds);
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
    }

    public static bool Pending => _pendingAt > 0;
    public static float Remaining => Pending ? Mathf.Max(0f, (float)(_pendingAt - EditorApplication.timeSinceStartup)) : 0f;

    private static void Tick()
    {
        if (_pendingAt < 0) { EditorApplication.update -= Tick; return; }
        if (EditorApplication.timeSinceStartup < _pendingAt) return;
        _pendingAt = -1;
        EditorApplication.update -= Tick;
        Capture();
    }

    public static void Reveal()
    {
        Directory.CreateDirectory(Folder);
        EditorUtility.RevealInFinder(Folder);
    }
}

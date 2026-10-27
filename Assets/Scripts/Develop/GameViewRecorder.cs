#if UNITY_EDITOR
using System.Collections;
using System.IO;
using UnityEditor.Media;
using UnityEngine;

// 에디터 플레이 중 게임뷰를 MP4로 녹화한다 (Unity Recorder 패키지 없이, 에디터 내장 MediaEncoder 사용).
// 켜고 끄는 곳: F10 / 디버그 패널 맨 위 (Editor/ScreenshotTool.cs). 저장: 프로젝트 루트/Recordings/yyyyMMdd_HHmmss.mp4 (git 제외)
//  - 매 프레임 끝에 화면을 읽어 고정 fps로 넣는다. 게임이 느려지면 같은 프레임을 반복해 실제 시간과 길이를 맞춘다
//  - 소리는 녹음하지 않는다. 플레이를 끝내면 자동으로 멈추고 파일을 닫는다
public class GameViewRecorder : MonoBehaviour
{
    public static GameViewRecorder Active { get; private set; }
    public static bool IsRecording => Active != null;
    public static float Elapsed => Active != null ? Time.unscaledTime - Active._startTime : 0f;
    public static string Folder => Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Recordings");

    private const int Fps = 30;
    private MediaEncoder _encoder;
    private Texture2D _frame;
    private string _path;
    private float _startTime;
    private int _written;
    private bool _stopping;

    public static void Toggle()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[녹화] 플레이 중에만 녹화할 수 있어요."); return; }
        if (IsRecording) Active.Stop();
        else
        {
            var go = new GameObject("__GameViewRecorder") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<GameViewRecorder>().Begin();
        }
    }

    private void Begin()
    {
        int w = Screen.width & ~1, h = Screen.height & ~1; // H.264는 짝수 크기만
        Directory.CreateDirectory(Folder);
        _path = Path.Combine(Folder, System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".mp4");
        var video = new VideoTrackAttributes
        {
            frameRate = new MediaRational(Fps),
            width = (uint)w,
            height = (uint)h,
            includeAlpha = false,
            bitRateMode = UnityEditor.VideoBitrateMode.High,
        };
        _encoder = new MediaEncoder(_path, video);
        _frame = new Texture2D(w, h, TextureFormat.RGBA32, false);
        _startTime = Time.unscaledTime;
        _written = 0;
        Active = this;
        StartCoroutine(Capture());
        Debug.Log($"[녹화] 시작 {w}×{h} · {Fps}fps → {_path}");
    }

    private IEnumerator Capture()
    {
        var eof = new WaitForEndOfFrame();
        while (!_stopping)
        {
            yield return eof;
            if (_stopping) break;
            // 지금까지 흐른 시간만큼 프레임이 들어가 있어야 한다 — 모자라면 이번 화면으로 채운다
            int due = Mathf.FloorToInt((Time.unscaledTime - _startTime) * Fps) + 1;
            if (_written >= due) continue;
            _frame.ReadPixels(new Rect(0, 0, _frame.width, _frame.height), 0, 0, false);
            _frame.Apply(false);
            while (_written < due) { _encoder.AddFrame(_frame); _written++; }
        }
    }

    public void Stop()
    {
        if (_stopping) return;
        _stopping = true;
        Finish();
        Destroy(gameObject);
    }

    private void Finish()
    {
        if (_encoder != null)
        {
            _encoder.Dispose();
            _encoder = null;
            Debug.Log($"[녹화] 저장 {_path} ({_written / (float)Fps:0.0}초)");
        }
        if (_frame != null) { Destroy(_frame); _frame = null; }
        if (Active == this) Active = null;
    }

    // 플레이를 끝내면 여기로 온다
    private void OnDestroy() => Finish();
}
#endif

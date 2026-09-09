using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

// ============================================================
// filename   : UISpriteSheetAnimator.cs
// description: uGUI Image 기반 스프라이트 시트 애니메이션 재생기.
//              machina_idle_sprite_sheet(32프레임) 등의 연속 프레임을
//              설정한 FPS에 맞춰 부드럽게 루프 재생합니다.
// ============================================================
[RequireComponent(typeof(Image))]
public class UISpriteSheetAnimator : MonoBehaviour
{
    [Header("스프라이트 프레임")]
    [Tooltip("재생할 스프라이트 프레임 배열 (순서대로 배치)")]
    [SerializeField] private Sprite[] frames;

    [Header("재생 설정")]
    [Tooltip("초당 재생 프레임 수 (FPS). 12~16 FPS 권장")]
    [Range(1f, 60f)]
    [SerializeField] private float frameRate = 12f;

    [Tooltip("무한 반복 여부")]
    [SerializeField] private bool loop = true;

    [Tooltip("활성화 시 자동 재생")]
    [SerializeField] private bool playOnAwake = true;

    [Tooltip("Time.timeScale에 영향을 받지 않고 재생할지 여부")]
    [SerializeField] private bool unscaledTime = false;

    [Tooltip("프레임 변경 시 자동으로 SetNativeSize() 호출 여부")]
    [SerializeField] private bool autoNativeSize = false;

    private Image _image;
    private int _currentFrame = 0;
    private float _timer = 0f;
    private bool _isPlaying = false;

    public event Action OnAnimationFinished;
    public event Action<int> OnLoopCompleted;
    private int _loopCount = 0;

    public bool IsPlaying => _isPlaying;
    public int CurrentFrame => _currentFrame;
    public int TotalFrames => frames != null ? frames.Length : 0;
    public float FrameRate
    {
        get => frameRate;
        set => frameRate = Mathf.Max(1f, value);
    }

    private void Awake()
    {
        _image = GetComponent<Image>();
        EnsureFrames();
    }

    private void Start()
    {
        if (playOnAwake && frames != null && frames.Length > 0)
        {
            Play();
        }
    }

    private void OnEnable()
    {
        if (playOnAwake && frames != null && frames.Length > 0 && !_isPlaying)
        {
            Play();
        }
    }

    private void Update()
    {
        if (!_isPlaying || frames == null || frames.Length <= 1) return;

        float dt = unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        _timer += dt;

        float interval = 1f / frameRate;
        if (_timer >= interval)
        {
            int framesToAdvance = Mathf.FloorToInt(_timer / interval);
            _timer %= interval;

            _currentFrame += framesToAdvance;

            if (_currentFrame >= frames.Length)
            {
                if (loop)
                {
                    _currentFrame %= frames.Length;
                    _loopCount++;
                    OnLoopCompleted?.Invoke(_loopCount);
                }
                else
                {
                    _currentFrame = frames.Length - 1;
                    _isPlaying = false;
                    ApplyFrame();
                    OnAnimationFinished?.Invoke();
                    return;
                }
            }

            ApplyFrame();
        }
    }

    public void Play()
    {
        _isPlaying = true;
        _timer = 0f;
        ApplyFrame();
    }

    public void Pause()
    {
        _isPlaying = false;
    }

    public void Resume()
    {
        _isPlaying = true;
    }

    public void Stop()
    {
        _isPlaying = false;
        _currentFrame = 0;
        _timer = 0f;
        ApplyFrame();
    }

    public void SetFrames(Sprite[] newFrames, bool resetToStart = true)
    {
        frames = newFrames;
        if (resetToStart)
        {
            _currentFrame = 0;
            _timer = 0f;
        }
        ApplyFrame();
    }

    public void Configure(Sprite[] newFrames, float fps, bool autoPlay = true)
    {
        frames = newFrames;
        frameRate = Mathf.Max(1f, fps);
        _currentFrame = 0;
        _timer = 0f;
        if (autoPlay && frames != null && frames.Length > 0)
            Play();
        else
            Stop();
    }

    private void ApplyFrame()
    {
        if (_image == null) _image = GetComponent<Image>();
        if (_image == null || frames == null || frames.Length == 0) return;

        if (_currentFrame >= 0 && _currentFrame < frames.Length)
        {
            Sprite s = frames[_currentFrame];
            if (s != null)
            {
                _image.sprite = s;
                if (autoNativeSize)
                {
                    _image.SetNativeSize();
                }
            }
        }
    }

    public void EnsureFrames()
    {
    }

#if UNITY_EDITOR
    [ContextMenu("Machina Idle 스프라이트 수동 로드")]
    public void LoadMachinaFramesFromEditor()
    {
        string assetPath = "Assets/Art/Blessing/Machina/machina_idle_sprite_64_sheet.png";
        UnityEngine.Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        if (subAssets == null || subAssets.Length == 0)
        {
            assetPath = "Assets/Art/Blessing/Machina/machina_idle_sprite_sheet.png";
            subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        }

        var list = new List<Sprite>();
        foreach (var obj in subAssets)
        {
            if (obj is Sprite s) list.Add(s);
        }

        if (list.Count > 0)
        {
            list.Sort((a, b) =>
            {
                int ia = ExtractIndex(a.name);
                int ib = ExtractIndex(b.name);
                return ia.CompareTo(ib);
            });

            if (list.Count == 64)
            {
                list = list.GetRange(0, 62);
            }

            frames = list.ToArray();
            frameRate = 16f;
            Debug.Log($"[UISpriteSheetAnimator] {assetPath}에서 {frames.Length}종의 프레임을 번호순으로 자동 로드했습니다.");
            EditorUtility.SetDirty(this);
            ApplyFrame();
        }
    }

    private int ExtractIndex(string name)
    {
        int lastUnder = name.LastIndexOf('_');
        if (lastUnder >= 0 && int.TryParse(name.Substring(lastUnder + 1), out int val))
            return val;
        return 0;
    }

    private void Reset()
    {
        _image = GetComponent<Image>();
    }
#endif
}

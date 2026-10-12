using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 노상강도(Highwayman) 등 근접 공격 시트 위에 방망이 휘두르기(참격/스윙) 이펙트를 덧그린다.
// 원본 시트(노상강도_attack)는 프레임 왼쪽 경계에서 방망이 끝이 잘려 있어서,
// 휘두르는 타이밍(triggerFrame)에 맞춰 몽둥이 끝 궤적을 따라가는 스미어·충격 섬광·파편 시트(BatSwing_Sheet)를 재생한다.
// 시트 원본: ArtSource/VFX/gen-batswing.js (칸 980×1020, 피벗 = 공격 프레임 픽셀 anchorPixel). 공격 시트와 같은 fps로 한 칸씩 맞춘다.
// 적 캐릭터 스탠딩 Image(Enemy Sprite)에 함께 붙인다.
public class BatSwingOverlay : MonoBehaviour
{
    [Header("대상 공격 시트")]
    [Tooltip("이 텍스처의 프레임이 재생될 때만 동작한다")]
    [SerializeField] private Texture2D attackSheet;
    [SerializeField] private int triggerFrame = 16;

    [Header("배트 궤적 기준점 (원본 프레임 픽셀, 왼쪽 위 원점)")]
    [Tooltip("노상강도_attack 기준 좌측 경계 충격점 (x=0, y=560)")]
    [SerializeField] private Vector2 anchorPixel = new(0f, 560f);
    [SerializeField] private Vector2 sourceFrameSize = new(832f, 1216f);

    [Header("스윙 참격 시트")]
    [SerializeField] private Sprite[] swingFrames;
    [SerializeField] private float swingFps = 18f;
    [Tooltip("이펙트 크기 배율. 1이면 원본 공격 프레임과 같은 픽셀 밀도")]
    [SerializeField] private float swingScale = 1f;

    private PoseSequencePlayer _player;
    private RectTransform _rect;
    private Image _image;
    private Image _swing;
    private Coroutine _playing;

    private void Awake()
    {
        _rect = (RectTransform)transform;
        _image = GetComponent<Image>();
        EnsurePlayerBinding();
    }

    private void OnEnable()
    {
        EnsurePlayerBinding();
    }

    private void OnDisable()
    {
        if (_player != null)
            _player.OnSpriteFrameShown -= HandleFrame;
        StopSwing();
    }

    public void EnsurePlayerBinding()
    {
        if (_player == null)
            _player = GetComponent<PoseSequencePlayer>();

        if (_player != null)
        {
            _player.OnSpriteFrameShown -= HandleFrame;
            _player.OnSpriteFrameShown += HandleFrame;
        }
    }

    private void HandleFrame(Sprite[] frames, int index)
    {
        if (frames == null || index < 0 || index >= frames.Length) return;
        Sprite current = frames[index];
        if (attackSheet == null || current == null || current.texture != attackSheet) return;

        int sheetFrame = SheetFrameNumber(current);
        if (sheetFrame == triggerFrame)
        {
            PlaySwing();
        }
    }

    private static int SheetFrameNumber(Sprite sprite)
    {
        if (sprite == null) return -1;
        string name = sprite.name;
        int underscore = name.LastIndexOf('_');
        return underscore >= 0 && int.TryParse(name.Substring(underscore + 1), out int n) ? n : -1;
    }

    public void PlaySwing()
    {
        if (swingFrames == null || swingFrames.Length == 0) return;
        EnsureSwingImage();
        PlaceAtAnchor();
        if (_playing != null) StopCoroutine(_playing);
        _playing = StartCoroutine(PlaySwingRoutine());
    }

    private IEnumerator PlaySwingRoutine()
    {
        _swing.enabled = true;
        float delay = 1f / Mathf.Max(1f, swingFps);
        foreach (var frame in swingFrames)
        {
            _swing.sprite = frame;
            yield return new WaitForSeconds(delay);
        }
        StopSwing();
    }

    public void StopSwing()
    {
        if (_playing != null) StopCoroutine(_playing);
        _playing = null;
        if (_swing != null) _swing.enabled = false;
    }

    private void EnsureSwingImage()
    {
        if (_swing != null) return;
        var child = transform.Find("BatSwingVFX");
        GameObject go;
        if (child != null)
        {
            go = child.gameObject;
        }
        else
        {
            go = new GameObject("BatSwingVFX", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
        }
        _swing = go.GetComponent<Image>();
        _swing.raycastTarget = false;
        _swing.enabled = false;
    }

    private void PlaceAtAnchor()
    {
        if (_rect == null) _rect = (RectTransform)transform;
        // Image가 preserveAspect로 프레임을 rect 안에 맞춰 그리므로, 실제 그려진 영역 기준으로 픽셀을 옮긴다
        Rect r = _rect.rect;
        if (_image != null && _image.preserveAspect && r.height > 0f)
        {
            float frameAspect = sourceFrameSize.x / sourceFrameSize.y;
            if (r.width / r.height > frameAspect)
            {
                float w = r.height * frameAspect;
                r = new Rect(r.center.x - w * 0.5f, r.yMin, w, r.height);
            }
            else
            {
                float h = r.width / frameAspect;
                r = new Rect(r.xMin, r.center.y - h * 0.5f, r.width, h);
            }
        }
        float unitsPerPixel = r.height / sourceFrameSize.y;
        Vector2 local = new(
            r.xMin + anchorPixel.x * unitsPerPixel,
            r.yMax - anchorPixel.y * unitsPerPixel);

        var sr = _swing.rectTransform;
        Sprite first = swingFrames[0];
        sr.anchorMin = sr.anchorMax = new Vector2(0.5f, 0.5f);
        sr.pivot = new Vector2(first.pivot.x / first.rect.width, first.pivot.y / first.rect.height);
        sr.sizeDelta = first.rect.size * unitsPerPixel * swingScale;
        sr.localPosition = local;
        sr.localRotation = Quaternion.identity;
        sr.localScale = Vector3.one;
        sr.SetAsLastSibling();
    }
}

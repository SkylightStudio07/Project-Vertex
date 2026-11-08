using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 플레이어 사격 스프라이트 시트 위에 총구 섬광을 덧그린다.
// 원본 시트(player_f_attack_handgun)의 섬광 프레임은 그림이 프레임 오른쪽 경계에서 잘려 있어서,
// 그 구간(replaceFrom~replaceTo)은 총을 뻗은 깨끗한 프레임(replacementFrame)으로 바꿔 보여주고
// 총구 위치에 칠한 섬광 낱장을 바꿔 끼우며 재생한다 (ArtDirection/BattleFxMockup/MuzzleFlash).
// 캐릭터 스탠딩 Image(PoseSequencePlayer가 붙은 오브젝트)에 함께 붙인다.
[RequireComponent(typeof(PoseSequencePlayer))]
public class MuzzleFlashOverlay : MonoBehaviour
{
    [Header("대상 공격 시트")]
    [Tooltip("이 텍스처의 프레임이 재생될 때만 동작한다 (다른 무기 모션에는 영향 없음)")]
    [SerializeField] private Texture2D attackSheet;
    // 아래 번호는 재생 배열의 인덱스가 아니라 "시트 안 프레임 번호"(스프라이트 이름 끝 _N)다.
    // 공격 모션은 시트 일부(현재 36~50)만 골라 재생하므로 배열 인덱스로 잡으면 어긋난다.
    [SerializeField] private int replaceFrom = 43;       // 잘린 섬광이 그려진 첫 프레임
    [SerializeField] private int replaceTo = 47;         // 잘린 섬광/파편이 남아 있는 마지막 프레임
    [SerializeField] private int replacementFrame = 48;  // 대신 보여줄 깨끗한 프레임(총을 뻗은 자세)

    [Header("총구 위치 (원본 프레임 픽셀, 왼쪽 위 원점)")]
    [SerializeField] private Vector2 muzzlePixel = new(744f, 290f);
    [SerializeField] private Vector2 sourceFrameSize = new(832f, 1216f);
    [Tooltip("총열 각도 (도, 위쪽이 +)")]
    [SerializeField] private float muzzleAngle;

    [Header("칠한 섬광 (가산 합성) — 그림의 피벗이 총구 원점")]
    [Tooltip("터지는 순간의 낱장. 사격마다 서로 다른 두 장을 골라 이어 붙인다")]
    [SerializeField] private Sprite[] burstSprites;
    [Tooltip("사라지는 순간의 낱장. 순서대로 재생한다")]
    [SerializeField] private Sprite[] fadeSprites;
    [SerializeField] private Material flashMaterial;
    [Tooltip("섬광 그림 한 변의 화면 크기 (1920×1080 기준 px)")]
    [SerializeField] private float flashScreenSize = 200f;
    [Tooltip("낱장 하나를 보여주는 시간 (초)")]
    [SerializeField] private float flashStep = 0.033f;

    [Header("다른 공격 시트 (attackClips) — 시트에 그려진 섬광은 지웠고, 사격 프레임마다 여기서 덧그린다")]
    [SerializeField] private System.Collections.Generic.List<ClipSheetShots> clipSheets = new();

    [System.Serializable]
    public class ClipSheetShots
    {
        public Texture2D sheet;
        [Tooltip("프레임 한 장의 픽셀 크기")]
        public Vector2 frameSize = new(794f, 1160f);
        public Shot[] shots;
        [Tooltip("사격 뒤 프레임의 총구 위치. 반동으로 총이 움직여도 섬광이 총구를 따라가게 한다")]
        public Shot[] track;
    }

    [System.Serializable]
    public class Shot
    {
        [Tooltip("시트 안 프레임 번호 (스프라이트 이름 끝 _N)")]
        public int frame;
        [Tooltip("총구 위치 (프레임 픽셀, 왼쪽 위 원점)")]
        public Vector2 muzzle;
        [Tooltip("총열 각도 (도, 위쪽이 +)")]
        public float angle;
    }

    [Header("파티클 (불티 · 불씨 · 연기 · 빛 번짐)")]
    [Tooltip("UIParticle 프리팹. 사격마다 총구 위치로 옮겨 처음부터 다시 재생한다")]
    [SerializeField] private Coffee.UIExtensions.UIParticle particlesPrefab;
    private Coffee.UIExtensions.UIParticle _particles;

    private PoseSequencePlayer _player;
    private RectTransform _rect;
    private Image _image;
    private Image _flash;
    private Coroutine _playing;
    private int _lastBurst = -1;

    // 낱장이 바뀌는 지점의 크기 · 밝기. 첫 장은 작게 터져서 빠르게 커지고, 마지막 장에서 꺼진다
    private static readonly float[] StepScale = { 0.85f, 1f, 1.08f, 1.12f, 1.16f };
    private static readonly float[] StepAlpha = { 1f, 1f, 0.9f, 0.75f, 0f };

    private void Awake()
    {
        _player = GetComponent<PoseSequencePlayer>();
        _rect = (RectTransform)transform;
        _image = GetComponent<Image>();
    }

    private void OnEnable() => _player.OnSpriteFrameShown += HandleFrame;

    private void OnDestroy()
    {
        if (_particles != null) Destroy(_particles.gameObject);
    }

    private void OnDisable()
    {
        _player.OnSpriteFrameShown -= HandleFrame;
        StopFlash();
    }

    private void HandleFrame(Sprite[] frames, int index)
    {
        Sprite current = frames[index];
        if (current == null) return;

        foreach (var entry in clipSheets)
        {
            if (entry == null || entry.sheet != current.texture || entry.shots == null) continue;
            int n = SheetFrameNumber(current);
            foreach (var shot in entry.shots)
                if (shot != null && shot.frame == n) PlayFlash(shot.muzzle, entry.frameSize, shot.angle);
            if (_playing != null && entry.track != null)
                foreach (var point in entry.track)
                    if (point != null && point.frame == n) MoveToMuzzle(point.muzzle, entry.frameSize, point.angle);
            return;
        }

        if (attackSheet == null || current.texture != attackSheet) return;
        int sheetFrame = SheetFrameNumber(current);
        if (sheetFrame < replaceFrom || sheetFrame > replaceTo) return;

        foreach (var f in frames)
            if (f != null && SheetFrameNumber(f) == replacementFrame) { _image.sprite = f; break; }

        if (sheetFrame == replaceFrom) PlayFlash(muzzlePixel, sourceFrameSize, muzzleAngle);
    }

    // 슬라이스 스프라이트 이름 끝의 번호 ("..._43" → 43). 번호가 없으면 -1.
    private static int SheetFrameNumber(Sprite sprite)
    {
        string name = sprite.name;
        int underscore = name.LastIndexOf('_');
        return underscore >= 0 && int.TryParse(name.Substring(underscore + 1), out int n) ? n : -1;
    }

    private void PlayFlash(Vector2 muzzle, Vector2 frameSize, float angle)
    {
        PlayParticles(muzzle, frameSize, angle);
        if (burstSprites == null || burstSprites.Length == 0) return;
        EnsureFlashImage();
        PlaceAtMuzzle(muzzle, frameSize, angle);
        if (_playing != null) StopCoroutine(_playing);
        _playing = StartCoroutine(PlayFlashRoutine(PickSequence()));
    }

    // 터지는 낱장 두 장(바로 전 사격의 첫 장은 피한다) + 사라지는 낱장
    private System.Collections.Generic.List<Sprite> PickSequence()
    {
        var seq = new System.Collections.Generic.List<Sprite>(4);
        int n = burstSprites.Length;
        int first = Random.Range(0, n);
        if (n > 1 && first == _lastBurst) first = (first + 1) % n;
        _lastBurst = first;
        seq.Add(burstSprites[first]);
        if (n > 1) seq.Add(burstSprites[(first + Random.Range(1, n)) % n]);
        if (fadeSprites != null) seq.AddRange(fadeSprites);
        return seq;
    }

    private IEnumerator PlayFlashRoutine(System.Collections.Generic.List<Sprite> seq)
    {
        _flash.enabled = true;
        // 표의 뒤쪽 구간부터 쓴다 — 낱장이 모자라도 마지막에는 꺼지게
        int offset = Mathf.Max(0, StepScale.Length - 1 - seq.Count);
        for (int i = 0; i < seq.Count; i++)
        {
            int k = Mathf.Min(i + offset, StepScale.Length - 2);
            _flash.sprite = seq[i];
            float t0 = Time.time;
            while (true)
            {
                float p = flashStep > 0f ? Mathf.Clamp01((Time.time - t0) / flashStep) : 1f;
                _flash.rectTransform.localScale = Vector3.one * Mathf.Lerp(StepScale[k], StepScale[k + 1], p);
                _flash.color = new Color(1f, 1f, 1f, Mathf.Lerp(StepAlpha[k], StepAlpha[k + 1], p));
                if (p >= 1f) break;
                yield return null;
            }
        }
        StopFlash();
    }

    private void StopFlash()
    {
        if (_playing != null) StopCoroutine(_playing);
        _playing = null;
        if (_flash != null) _flash.enabled = false;
    }

    private void EnsureFlashImage()
    {
        if (_flash != null) return;
        var go = new GameObject("MuzzleFlash", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(transform, false);
        _flash = go.GetComponent<Image>();
        _flash.raycastTarget = false;
        _flash.material = flashMaterial;
        _flash.enabled = false;
    }

    // 공격 프레임은 PoseSequencePlayer가 원본 비율로 rect를 맞추므로, 원본 픽셀 → rect 로컬 좌표로 그대로 환산된다.
    // 파티클은 캐릭터 그림의 형제로 둔다. 그림은 재생 중에만 커졌다가 돌아오므로, 자식이면 연기가 모션 끝에 같이 튄다
    private void PlayParticles(Vector2 muzzle, Vector2 frameSize, float angle)
    {
        if (particlesPrefab == null) return;
        if (_particles == null)
        {
            _particles = Instantiate(particlesPrefab, _rect.parent, false);
            _particles.name = "GunshotParticles";
        }
        _particles.transform.SetSiblingIndex(_rect.GetSiblingIndex() + 1);
        _particles.transform.position = _rect.TransformPoint(MuzzleLocal(muzzle, frameSize));
        _particles.transform.rotation = _rect.rotation * Quaternion.Euler(0f, 0f, angle);
        _particles.Play();
    }

    private Vector2 MuzzleLocal(Vector2 muzzle, Vector2 frameSize)
    {
        Rect r = _rect.rect;
        float unitsPerPixel = r.height / frameSize.y;
        return new Vector2(r.xMin + muzzle.x * unitsPerPixel, r.yMax - muzzle.y * unitsPerPixel);
    }

    private void MoveToMuzzle(Vector2 muzzle, Vector2 frameSize, float angle)
    {
        if (_flash == null) return;
        _flash.rectTransform.localPosition = MuzzleLocal(muzzle, frameSize);
        _flash.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    // 그림은 재생 중에 배율이 걸려 있으므로, 그 배율을 나눠서 화면 크기를 일정하게 맞춘다
    private void PlaceAtMuzzle(Vector2 muzzle, Vector2 frameSize, float angle)
    {
        var fr = _flash.rectTransform;
        Sprite first = burstSprites[0];
        fr.anchorMin = fr.anchorMax = new Vector2(0.5f, 0.5f);
        fr.pivot = new Vector2(first.pivot.x / first.rect.width, first.pivot.y / first.rect.height);
        float parentScale = Mathf.Max(0.001f, Mathf.Abs(_rect.localScale.x));
        fr.sizeDelta = Vector2.one * (flashScreenSize / parentScale);
        fr.localPosition = MuzzleLocal(muzzle, frameSize);
        fr.localRotation = Quaternion.Euler(0f, 0f, angle);
        fr.localScale = Vector3.one;
    }
}

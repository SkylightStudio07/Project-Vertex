using System.Collections;
using DG.Tweening;
using UnityEngine;

// 상점 주인(리깅 초상 RiggingV2Portrait)이 입장할 때 손을 흔든다.
// 뼈를 돌리면 같은 뼈에 묶인 몸통(topwear)까지 같이 휘므로, 소매 파츠(handwear_1) 하나만 떼어 통째로 돌린다:
//   흔드는 동안 그 파츠의 SpriteSkin(뼈 변형)을 끄고, 파츠 안의 한 점(어깨 쪽 소매 끝)을 축으로 부채꼴로 흔든 뒤 다시 켠다.
// 리그의 _1/_2 · l/r은 화면 기준이라 handwear_1(화면 왼쪽) = 토냐의 오른팔.
[RequireComponent(typeof(RectTransform))]
public class ShopkeeperWave : MonoBehaviour
{
    [Header("손 흔들기 — 파츠 하나만 돌린다")]
    [Tooltip("흔들 파츠 이름 (아바타 안의 SpriteRenderer)")]
    [SerializeField] private string partName = "handwear_1";
    [Tooltip("돌리는 축 — 파츠 그림 안의 점 (0~1, 좌하단 원점). 어깨에 붙은 소매 끝")]
    [SerializeField] private Vector2 pivot = new(0.78f, 0.1f);
    [Tooltip("좌우로 흔들리는 폭(도)")]
    [SerializeField] private float degrees = 9f;
    [SerializeField] private float waveSpeed = 7f;   // 초당 흔들림 위상(라디안)
    [SerializeField] private int waves = 4;          // 왕복 횟수
    [Tooltip("흔드는 동안 이 파츠를 frontOf 파츠 바로 앞에 그린다. 원래 순서로는 소매가 가운(topwear) 뒤로 들어간다. 얼굴 · 머리보다는 뒤")]
    [SerializeField] private bool bringToFront = true;
    [SerializeField] private string frontOf = "topwear";
    [Tooltip("켜면 입장할 때마다 흔든다. 끄면 처음 한 번만 (저장 플래그)")]
    [SerializeField] private bool everyVisit = true;

    private const string FlagId = "shop_first_wave";
    private Transform _part;
    private SpriteRenderer _renderer;
    private Behaviour _skin;
    private int _restOrder;
    private Vector3 _restPos;
    private Quaternion _restRot;
    private float _weight, _phase;
    private bool _active;
    private Coroutine _routine;

    // 입장 때 부른다 (ShopView.Open)
    public void PlayOnEnter()
    {
        var flags = BlessingAffinityManager.Instance;
        if (!everyVisit && flags != null && flags.HasFlag(FlagId)) return;
        if (flags != null && !everyVisit) flags.SetFlag(FlagId, true);
        Play();
    }

    public void Play()
    {
        if (_routine != null) StopCoroutine(_routine);
        Restore();
        _routine = StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        // 리그가 아바타를 만들 때까지 기다린다 (OnEnable → BuildStage)
        float wait = 0f;
        while (!FindPart() && wait < 3f) { wait += Time.unscaledDeltaTime; yield return null; }
        if (_part == null) yield break;
        yield return new WaitForSecondsRealtime(0.35f); // 대사창이 뜨고 나서 흔들기 시작

        _restPos = _part.localPosition;
        _restRot = _part.localRotation;
        if (_skin != null) _skin.enabled = false; // 뼈 변형을 끄면 쉬는 자세 그대로의 그림이 된다
        if (_renderer != null)
        {
            _restOrder = _renderer.sortingOrder;
            if (bringToFront) _renderer.sortingOrder = FrontOrder();
        }
        _active = true; _phase = 0f; _weight = 0f;

        float duration = waves * Mathf.PI * 2f / waveSpeed;
        DOTween.To(() => _weight, w => _weight = w, 1f, 0.25f).SetUpdate(true).SetEase(Ease.OutQuad).SetLink(gameObject);
        yield return new WaitForSecondsRealtime(Mathf.Max(0.5f, duration - 0.3f));
        DOTween.To(() => _weight, w => _weight = w, 0f, 0.3f).SetUpdate(true).SetEase(Ease.InQuad).SetLink(gameObject);
        yield return new WaitForSecondsRealtime(0.32f);
        Restore();
        _routine = null;
    }

    private bool FindPart()
    {
        var portrait = GetComponent("RiggingV2Portrait");
        if (portrait == null) return false;
        var prop = portrait.GetType().GetProperty("AvatarInstance");
        var avatar = prop != null ? prop.GetValue(portrait) as GameObject : null;
        if (avatar == null) return false;
        _part = FindDeep(avatar.transform, partName);
        if (_part == null) return false;
        _renderer = _part.GetComponent<SpriteRenderer>();
        _skin = null;
        foreach (var b in _part.GetComponents<Behaviour>())
            if (b.GetType().Name == "SpriteSkin") { _skin = b; break; }
        return true;
    }

    private static Transform FindDeep(Transform root, string name)
    {
        if (root.name == name) return root;
        foreach (Transform c in root)
        {
            var r = FindDeep(c, name);
            if (r != null) return r;
        }
        return null;
    }

    private void LateUpdate()
    {
        if (!_active || _part == null) return;
        _phase += Time.unscaledDeltaTime * waveSpeed;
        float angle = Mathf.Sin(_phase) * _weight * degrees;

        // 쉬는 자세로 되돌린 뒤, 파츠 안의 축 점을 중심으로 돌린다
        _part.localPosition = _restPos;
        _part.localRotation = _restRot;
        Vector3 axis = PivotWorld();
        _part.RotateAround(axis, _part.forward, angle);
    }

    // frontOf 파츠(가운) 바로 앞 — 얼굴 · 머리 파츠보다는 뒤에 남는다
    private int FrontOrder()
    {
        var target = FindDeep(_part.root, frontOf);
        var r = target != null ? target.GetComponent<SpriteRenderer>() : null;
        return r != null ? Mathf.Max(_renderer.sortingOrder, r.sortingOrder + 1) : _renderer.sortingOrder;
    }

    private Vector3 PivotWorld()
    {
        if (_renderer == null || _renderer.sprite == null) return _part.position;
        Bounds b = _renderer.sprite.bounds; // 파츠 로컬 공간
        var local = new Vector3(Mathf.Lerp(b.min.x, b.max.x, pivot.x), Mathf.Lerp(b.min.y, b.max.y, pivot.y), 0f);
        return _part.TransformPoint(local);
    }

    private void Restore()
    {
        if (_active && _part != null)
        {
            _part.localPosition = _restPos;
            _part.localRotation = _restRot;
        }
        if (_active && _renderer != null) _renderer.sortingOrder = _restOrder;
        _active = false;
        _weight = 0f;
        if (_skin != null) _skin.enabled = true;
    }

    private void OnDisable()
    {
        if (_routine != null) { StopCoroutine(_routine); _routine = null; }
        Restore();
    }
}

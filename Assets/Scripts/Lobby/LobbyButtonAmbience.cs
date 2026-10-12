using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// 로비 메인 화면이 가만히 있지 않도록 버튼에 주기적으로 넣는 대기 연출.
// - 빛 쓸기: 버튼 모양(스프라이트 알파)으로 가려진 사선 빛 막대가 주기적으로 한 번씩 쓸고 지나간다 (버튼끼리 시차)
// - 불빛 일렁임: 모닥불 그림 위의 따뜻한 빛이 불규칙하게 밝아졌다 어두워진다
// 등장 연출이 끝난 뒤(firstDelay) 시작하고, 메인 화면이 꺼지면 멈춘다. 빛 막대는 실행 중에 만든다.
public class LobbyButtonAmbience : MonoBehaviour
{
    [Header("빛 쓸기")]
    [SerializeField] private List<Image> shineTargets = new();
    [SerializeField] private Sprite shineSprite;                     // 부드러운 원형 그라데이션(세로로 늘려 막대로 쓴다)
    [SerializeField, Min(0.5f)] private float shineInterval = 5f;
    [SerializeField, Min(0f)] private float shineStagger = 0.45f;   // 버튼끼리 시차
    [SerializeField, Min(0.1f)] private float shineDuration = 0.9f;
    [SerializeField, Min(0f)] private float firstDelay = 1.6f;      // 등장 연출이 끝난 뒤 시작
    [SerializeField, Min(1f)] private float shineWidth = 90f;
    [SerializeField] private Color shineColor = new(0.35f, 0.85f, 1f, 0.45f);

    [Header("불빛 일렁임")]
    [SerializeField] private Graphic fireGlow;
    [SerializeField, Range(0f, 1f)] private float glowMin = 0.25f;
    [SerializeField, Range(0f, 1f)] private float glowMax = 0.7f;
    [SerializeField, Min(0.1f)] private float flickerSpeed = 2.2f;

    private readonly List<RectTransform> _bands = new();
    private Sequence _loop;
    private float _glowBaseAlpha = 1f;

    private void Awake()
    {
        foreach (var target in shineTargets)
        {
            if (target == null) continue;
            // 버튼 스프라이트 알파로 자식을 가린다 (투명한 모서리 밖으로 빛이 새지 않게)
            if (!target.TryGetComponent<Mask>(out var mask)) mask = target.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = true;

            var go = new GameObject("ShineBand", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(target.transform, false);
            go.transform.SetSiblingIndex(Mathf.Min(1, target.transform.childCount - 1)); // 배경 바로 위, 글자 아래
            var rt = (RectTransform)go.transform;
            var size = target.rectTransform.rect.size;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(shineWidth, size.y * 1.8f);
            rt.localRotation = Quaternion.Euler(0f, 0f, -22f);
            var img = go.GetComponent<Image>();
            img.sprite = shineSprite; img.color = shineColor; img.raycastTarget = false;
            // 끄지 않고 버튼 밖(마스크 바깥)에 대기시킨다. 꺼 둔 사이 부모 CanvasGroup 알파가 바뀌면 다시 켜도 투명하게 남는다
            rt.anchoredPosition = new Vector2(-shineWidth * 2f, 0f);
            _bands.Add(rt);
        }
        if (fireGlow != null) _glowBaseAlpha = fireGlow.color.a;
    }

    private void OnEnable()
    {
        _loop?.Kill();
        _loop = DOTween.Sequence().SetLink(gameObject).AppendInterval(firstDelay).OnComplete(StartShineLoop);
    }

    private void OnDisable()
    {
        _loop?.Kill();
        foreach (var band in _bands) if (band != null) { band.DOKill(); band.anchoredPosition = new Vector2(-shineWidth * 2f, 0f); }
    }

    private void StartShineLoop()
    {
        _loop = DOTween.Sequence().SetLink(gameObject);
        for (int i = 0; i < _bands.Count; i++)
        {
            var band = _bands[i];
            if (band == null) continue;
            float width = ((RectTransform)band.parent).rect.width;
            float at = i * shineStagger;
            _loop.InsertCallback(at, () => band.anchoredPosition = new Vector2(-shineWidth * 2f, 0f));
            _loop.Insert(at, band.DOAnchorPosX(width + shineWidth * 2f, shineDuration).SetEase(Ease.InOutSine));
        }
        _loop.AppendInterval(Mathf.Max(0f, shineInterval - shineDuration));
        _loop.SetLoops(-1);
    }

    private void Update()
    {
        if (fireGlow == null) return;
        // 두 개의 노이즈를 겹쳐 규칙적이지 않게 일렁인다
        float t = Time.time * flickerSpeed;
        float n = Mathf.PerlinNoise(t, 0.37f) * 0.7f + Mathf.PerlinNoise(t * 2.3f, 5.1f) * 0.3f;
        var c = fireGlow.color;
        c.a = _glowBaseAlpha * Mathf.Lerp(glowMin, glowMax, n);
        fireGlow.color = c;
    }
}

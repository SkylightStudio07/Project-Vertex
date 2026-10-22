using UnityEngine;
using UnityEngine.UI;

// 부드러운 조명이 촛불처럼 천천히 밝아졌다 어두워진다 (아카이브 붉은 조명).
// 느린 사인파에 펄린 노이즈로 작은 흔들림을 섞는다. 크기도 살짝 같이 숨쉰다.
[RequireComponent(typeof(Graphic))]
public class UIGlowBreath : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float minAlpha = 0.1f;
    [SerializeField, Range(0f, 1f)] private float maxAlpha = 0.22f;
    [SerializeField, Min(0.05f)] private float period = 4f;
    [SerializeField, Range(0f, 1f)] private float flicker = 0.25f;  // 0이면 매끈한 숨쉬기만
    [SerializeField, Range(0f, 0.2f)] private float scalePulse = 0.04f;
    [SerializeField] private float phase;

    private Graphic _graphic;
    private Vector3 _baseScale;

    private void Awake()
    {
        _graphic = GetComponent<Graphic>();
        _baseScale = transform.localScale;
    }

    private void OnDisable() => transform.localScale = _baseScale;

    private void Update()
    {
        float t = Time.unscaledTime + phase;
        float breath = 0.5f + 0.5f * Mathf.Sin(t * Mathf.PI * 2f / period);
        float noise = Mathf.PerlinNoise(t * 1.7f, phase) - 0.5f;
        float k = Mathf.Clamp01(breath + noise * flicker);
        var c = _graphic.color;
        c.a = Mathf.Lerp(minAlpha, maxAlpha, k);
        _graphic.color = c;
        transform.localScale = _baseScale * (1f + scalePulse * (k - 0.5f));
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 나비처럼 날갯짓하며 제자리 근처를 떠다니는 UI 그림 (아카이브 나비).
// 프레임을 0-1-2-1 순서로 돌리고, 펄린 노이즈로 처음 자리 둘레를 천천히 돈다. 진행 방향으로 살짝 기운다.
[RequireComponent(typeof(Image))]
public class UIFlutter : MonoBehaviour
{
    [SerializeField] private List<Sprite> frames = new();
    [SerializeField, Min(0.02f)] private float frameDuration = 0.09f;
    [SerializeField] private Vector2 wander = new(60f, 40f);      // 처음 자리에서 벗어나는 최대 거리
    [SerializeField, Min(0.01f)] private float wanderSpeed = 0.18f;
    [SerializeField, Range(0f, 45f)] private float tilt = 18f;
    [SerializeField] private float phase;                          // 나비끼리 엇갈리게

    private static readonly int[] Loop = { 0, 1, 2, 1 };
    private Image _image;
    private RectTransform _rect;
    private Vector2 _home;
    private float _seed;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _rect = (RectTransform)transform;
        _home = _rect.anchoredPosition;
        _seed = Random.Range(0f, 100f);
    }

    private void OnDisable() => _rect.anchoredPosition = _home;

    private void Update()
    {
        float t = Time.unscaledTime + phase;
        if (frames.Count > 0)
        {
            int i = Loop[(int)(t / frameDuration) % Loop.Length];
            _image.sprite = frames[Mathf.Min(i, frames.Count - 1)];
        }

        float w = t * wanderSpeed;
        var offset = new Vector2((Mathf.PerlinNoise(_seed, w) - 0.5f) * 2f * wander.x,
                                 (Mathf.PerlinNoise(_seed + 7.3f, w) - 0.5f) * 2f * wander.y);
        var prev = _rect.anchoredPosition;
        _rect.anchoredPosition = _home + offset;
        float dx = _rect.anchoredPosition.x - prev.x;
        _rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Clamp(-dx * 6f, -tilt, tilt));
    }
}

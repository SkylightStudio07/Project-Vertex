using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// UI 모서리에서 작은 청록 사각형(정사각형·직사각형)이 계속 생겨나 위로 떠오르며 사라지는 효과.
// 데이터 조각이 부글거리는 느낌. 붙인 RectTransform의 anchor 지점(기본 오른쪽 아래) 근처에서 생긴다.
// 사각형은 미리 만들어 둔 풀에서 돌려 쓰고, 오브젝트를 켜고 끄지 않고 알파로만 숨긴다.
public class UIPixelBubbles : MonoBehaviour
{
    [SerializeField] private Vector2 anchor = new(1f, 0f);             // 부모 영역 기준 생성 위치 (0~1)
    [SerializeField] private Vector2 spawnArea = new(70f, 14f);        // anchor 주변 생성 범위 (왼쪽·위로 퍼짐)
    [SerializeField, Min(1)] private int poolSize = 18;
    [SerializeField, Min(0.01f)] private float spawnInterval = 0.12f;
    [SerializeField] private Vector2 sizeRange = new(3f, 9f);
    [SerializeField, Range(0f, 1f)] private float rectangleChance = 0.4f; // 직사각형(가로로 긴) 비율
    [SerializeField] private Vector2 riseRange = new(14f, 40f);          // 수명 동안 떠오르는 거리
    [SerializeField] private Vector2 lifeRange = new(0.6f, 1.3f);
    [SerializeField] private Color color = new(0.25f, 0.8f, 1f, 0.9f);

    private class Bit
    {
        public RectTransform rect;
        public Image image;
        public Vector2 start;
        public float rise, life, age = -1f;
    }

    private readonly List<Bit> _bits = new();
    private float _timer;

    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var go = new GameObject("Bit", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            var img = go.GetComponent<Image>();
            img.raycastTarget = false;
            img.color = new Color(color.r, color.g, color.b, 0f);
            _bits.Add(new Bit { rect = rt, image = img });
        }
    }

    private void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            _timer = spawnInterval * Random.Range(0.6f, 1.4f);
            Spawn();
        }

        foreach (var b in _bits)
        {
            if (b.age < 0f) continue;
            b.age += Time.deltaTime;
            float t = b.age / b.life;
            if (t >= 1f)
            {
                b.age = -1f;
                b.image.color = new Color(color.r, color.g, color.b, 0f);
                continue;
            }
            // 위로 떠오르며, 처음엔 빠르게 나타나 끝으로 갈수록 흐려진다. 가끔 깜빡인다
            b.rect.anchoredPosition = b.start + new Vector2(0f, b.rise * (1f - (1f - t) * (1f - t)));
            float alpha = Mathf.Min(1f, t * 6f) * (1f - t);
            if (Random.value < 0.04f) alpha *= 0.3f;
            b.image.color = new Color(color.r, color.g, color.b, color.a * alpha);
        }
    }

    private void Spawn()
    {
        foreach (var b in _bits)
        {
            if (b.age >= 0f) continue;
            float s = Random.Range(sizeRange.x, sizeRange.y);
            bool rect = Random.value < rectangleChance;
            b.rect.sizeDelta = rect ? new Vector2(s * Random.Range(1.8f, 3f), s * 0.6f) : new Vector2(s, s);
            // 모서리에 가까울수록 많이 생기도록 치우쳐 뽑는다
            float dx = Mathf.Pow(Random.value, 1.6f) * spawnArea.x;
            float dy = Random.value * spawnArea.y;
            b.start = new Vector2(-dx, dy);
            b.rect.anchoredPosition = b.start;
            b.rise = Random.Range(riseRange.x, riseRange.y);
            b.life = Random.Range(lifeRange.x, lifeRange.y);
            b.age = 0f;
            return;
        }
    }
}

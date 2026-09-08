using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

// ============================================================
// filename   : PetalFloatingEffect.cs
// description: BlessingView 배경 위에 꽃잎이 바람을 타고 살랑거리며
//              3D 텀블링(뒤집힘)과 함께 흩날리는 uGUI 전용 파티클 이펙트
// ============================================================
public class PetalFloatingEffect : MonoBehaviour
{
    [Header("꽃잎 스프라이트 (4종)")]
    [Tooltip("Petals 스프라이트 시트에서 슬라이스된 4종류의 꽃잎")]
    [SerializeField] private Sprite[] petalSprites;

    [Header("스폰 설정")]
    [Tooltip("화면에 동시에 떠다닐 꽃잎 개수")]
    [Range(5, 60)]
    [SerializeField] private int petalCount = 25;

    [Tooltip("꽃잎 크기 범위 (최소, 최대)")]
    [SerializeField] private Vector2 minMaxScale = new Vector2(0.25f, 0.65f);

    [Tooltip("꽃잎 투명도 범위 (최소, 최대)")]
    [SerializeField] private Vector2 minMaxAlpha = new Vector2(0.4f, 0.95f);

    [Tooltip("꽃잎 기본 색상 틴트 (약간의 푸른 달빛 광택)")]
    [SerializeField] private Color baseTint = new Color(0.92f, 0.97f, 1f, 1f);

    [Header("바람 및 낙하 속도")]
    [Tooltip("기본 이동 속도 (X: 우측 바람 drift, Y: 하강 속도)")]
    [SerializeField] private Vector2 windVelocity = new Vector2(45f, -65f);

    [Tooltip("꽃잎별 속도 편차 (0.0 = 동일, 0.5 = ±50%)")]
    [Range(0f, 0.8f)]
    [SerializeField] private float speedVariation = 0.35f;

    [Header("살랑거림 (Sway) & 3D 텀블링 (Flip)")]
    [Tooltip("바람에 좌우로 흔들리는 진폭 (픽셀 단위)")]
    [SerializeField] private float swayAmplitude = 28f;

    [Tooltip("좌우 흔들림 주기")]
    [SerializeField] private float swayFrequency = 1.6f;

    [Tooltip("Z축 회전 속도 (도/초)")]
    [SerializeField] private float rotationSpeed = 25f;

    [Tooltip("3D 앞뒤 뒤집힘(텀블링) 주기")]
    [SerializeField] private float tumbleFrequency = 1.3f;

    [Header("영역 설정")]
    [Tooltip("꽃잎이 떠다닐 영역. 비워두면 현재 오브젝트의 RectTransform 사용")]
    [SerializeField] private RectTransform containerRect;

    private RectTransform _petalContainer;
    private readonly List<PetalItem> _petals = new List<PetalItem>();
    private bool _isInitialized = false;

    private class PetalItem
    {
        public RectTransform rect;
        public Image image;
        public float baseScale;
        public float speedMult;
        public float swayAmp;
        public float swayFreq;
        public float swayOffset;
        public float tumbleFreq;
        public float tumblePhase;
        public float rotSpeed;
        public float currentAngle;
        public Vector2 localPos;
    }

    private void Awake()
    {
        if (containerRect == null)
            containerRect = GetComponent<RectTransform>();

        EnsurePetalSprites();
        InitializePetals();
    }

    private void OnEnable()
    {
        if (_isInitialized)
        {
            // 뷰가 다시 켜졌을 때 화면 전체에 자연스럽게 흩뿌려진 상태로 시작
            PrewarmPetals();
        }
    }

    private void Update()
    {
        if (!_isInitialized || _petals.Count == 0) return;

        float dt = Time.deltaTime;
        float time = Time.time;
        Vector2 bounds = GetBounds();
        float halfW = bounds.x * 0.5f;
        float halfH = bounds.y * 0.5f;
        float margin = 80f;

        for (int i = 0; i < _petals.Count; i++)
        {
            var p = _petals[i];

            // 1. 기본 바람 이동
            p.localPos += windVelocity * (p.speedMult * dt);

            // 2. 좌우 살랑거림 (Sway)
            float sway = Mathf.Sin(time * p.swayFreq + p.swayOffset) * p.swayAmp;
            p.rect.anchoredPosition = new Vector2(p.localPos.x + sway, p.localPos.y);

            // 3. 3D 텀블링 (Flip: X축 스케일을 Cos으로 뒤집어 입체감 부여)
            float flip = Mathf.Cos(time * p.tumbleFreq + p.tumblePhase);
            p.rect.localScale = new Vector3(p.baseScale * flip, p.baseScale, 1f);

            // 4. Z축 회전
            p.currentAngle += p.rotSpeed * dt;
            p.rect.localEulerAngles = new Vector3(0f, 0f, p.currentAngle);

            // 5. 화면 경계 체크 및 리스폰 (화면 아래로 떨어지거나 우측 밖으로 나갔을 때)
            if (p.localPos.y < -halfH - margin || (windVelocity.x > 0 && p.localPos.x > halfW + margin) || (windVelocity.x < 0 && p.localPos.x < -halfW - margin))
            {
                RespawnAtTop(p, bounds);
            }
        }
    }

    private void InitializePetals()
    {
        if (_isInitialized) return;

        // 꽃잎들을 담을 투명 컨테이너 생성 (Background 바로 뒤/앞 순서 보장)
        GameObject containerObj = new GameObject("PetalContainer", typeof(RectTransform));
        _petalContainer = containerObj.GetComponent<RectTransform>();
        _petalContainer.SetParent(containerRect, false);
        _petalContainer.anchorMin = Vector2.zero;
        _petalContainer.anchorMax = Vector2.one;
        _petalContainer.sizeDelta = Vector2.zero;
        _petalContainer.anchoredPosition = Vector2.zero;

        // "Background"라는 이름의 자식이 있으면 그 바로 다음(앞)으로 인덱스 배치
        Transform bg = transform.Find("Background");
        if (bg != null)
        {
            _petalContainer.SetSiblingIndex(bg.GetSiblingIndex() + 1);
        }

        Vector2 bounds = GetBounds();

        for (int i = 0; i < petalCount; i++)
        {
            GameObject petalObj = new GameObject($"Petal_{i}", typeof(RectTransform), typeof(Image));
            RectTransform pRect = petalObj.GetComponent<RectTransform>();
            pRect.SetParent(_petalContainer, false);

            Image pImage = petalObj.GetComponent<Image>();
            pImage.raycastTarget = false; // 클릭 방해 금지

            PetalItem item = new PetalItem
            {
                rect = pRect,
                image = pImage
            };

            RandomizePetalProperties(item);
            // 초기 스폰 시에는 화면 전체에 무작위로 분포시켜 빈 화면 방지
            float randX = Random.Range(-bounds.x * 0.5f, bounds.x * 0.5f);
            float randY = Random.Range(-bounds.y * 0.5f, bounds.y * 0.5f);
            item.localPos = new Vector2(randX, randY);
            item.currentAngle = Random.Range(0f, 360f);

            _petals.Add(item);
        }

        _isInitialized = true;
    }

    private void PrewarmPetals()
    {
        Vector2 bounds = GetBounds();
        for (int i = 0; i < _petals.Count; i++)
        {
            var p = _petals[i];
            RandomizePetalProperties(p);
            float randX = Random.Range(-bounds.x * 0.5f, bounds.x * 0.5f);
            float randY = Random.Range(-bounds.y * 0.5f, bounds.y * 0.5f);
            p.localPos = new Vector2(randX, randY);
            p.currentAngle = Random.Range(0f, 360f);
        }
    }

    private void RespawnAtTop(PetalItem p, Vector2 bounds)
    {
        RandomizePetalProperties(p);

        float halfW = bounds.x * 0.5f;
        float halfH = bounds.y * 0.5f;
        float margin = 60f;

        // 바람이 우측으로 불면 좌측 상단 쪽에 더 많은 스폰 기회 부여
        float spawnX;
        if (windVelocity.x > 0)
        {
            // 바람이 우측으로 불 때는 좌측 외곽 ~ 우측 상단 범위에서 등장
            spawnX = Random.Range(-halfW - margin, halfW * 0.7f);
        }
        else
        {
            spawnX = Random.Range(-halfW * 0.7f, halfW + margin);
        }

        float spawnY = halfH + Random.Range(10f, margin);
        p.localPos = new Vector2(spawnX, spawnY);
        p.currentAngle = Random.Range(0f, 360f);
    }

    private void RandomizePetalProperties(PetalItem p)
    {
        // 4가지 꽃잎 스프라이트 중 하나를 균등하게 무작위 선택
        if (petalSprites != null && petalSprites.Length > 0)
        {
            int spriteIndex = Random.Range(0, petalSprites.Length);
            p.image.sprite = petalSprites[spriteIndex];
        }

        // 크기(원근감) & 투명도
        p.baseScale = Random.Range(minMaxScale.x, minMaxScale.y);
        float alpha = Random.Range(minMaxAlpha.x, minMaxAlpha.y);
        p.image.color = new Color(baseTint.r, baseTint.g, baseTint.b, alpha);

        // 원본 스프라이트 종횡비에 맞게 기본 크기 설정 (대략 80x80px 기준)
        if (p.image.sprite != null)
        {
            float ratio = p.image.sprite.rect.width / p.image.sprite.rect.height;
            float baseSize = 80f;
            p.rect.sizeDelta = new Vector2(baseSize * ratio, baseSize);
        }
        else
        {
            p.rect.sizeDelta = new Vector2(80f, 80f);
        }

        // 개별 물리/모션 편차
        float varFactor = Random.Range(-speedVariation, speedVariation);
        p.speedMult = 1f + varFactor;

        p.swayAmp = swayAmplitude * Random.Range(0.7f, 1.3f);
        p.swayFreq = swayFrequency * Random.Range(0.8f, 1.2f);
        p.swayOffset = Random.Range(0f, Mathf.PI * 2f);

        p.tumbleFreq = tumbleFrequency * Random.Range(0.7f, 1.3f);
        p.tumblePhase = Random.Range(0f, Mathf.PI * 2f);

        float rotDir = Random.value > 0.5f ? 1f : -1f;
        p.rotSpeed = rotationSpeed * Random.Range(0.5f, 1.5f) * rotDir;
    }

    private Vector2 GetBounds()
    {
        if (containerRect != null && containerRect.rect.width > 0 && containerRect.rect.height > 0)
        {
            return new Vector2(containerRect.rect.width, containerRect.rect.height);
        }
        return new Vector2(Screen.width > 0 ? Screen.width : 1920f, Screen.height > 0 ? Screen.height : 1080f);
    }

    private void EnsurePetalSprites()
    {
#if UNITY_EDITOR
        if (petalSprites == null || petalSprites.Length == 0)
        {
            string assetPath = "Assets/Art/Blessing/Machina/Effects/Petals.png";
            Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            var list = new List<Sprite>();
            foreach (var obj in subAssets)
            {
                if (obj is Sprite s)
                    list.Add(s);
            }

            if (list.Count > 0)
            {
                petalSprites = list.ToArray();
                Debug.Log($"[PetalFloatingEffect] Petals.png에서 {petalSprites.Length}종의 꽃잎 스프라이트를 자동으로 로드했습니다.");
            }
        }
#endif
    }

#if UNITY_EDITOR
    private void Reset()
    {
        containerRect = GetComponent<RectTransform>();
        EnsurePetalSprites();
    }
#endif
}

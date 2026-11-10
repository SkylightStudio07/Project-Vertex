using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// 전투마다 배경(BattleStageData)을 하나 골라 깐다: 배경 그림 · 유닛 조명(림 방향 · 색, 색 보정) · 분위기 층.
// Canvas/Background에 붙인다. 같은 노드에 다시 들어가면 같은 배경 (층 · 노드로 결정).
// 막 개념이 들어오면 stages를 actNumber로 거르면 된다.
[RequireComponent(typeof(Image))]
public class BattleStageView : MonoBehaviour
{
    [SerializeField] private List<BattleStageData> stages = new();
    [Tooltip("배경 바로 위 · 유닛 아래에 깔리는 분위기 층")]
    [SerializeField] private BattleAmbienceFx ambience;
    [Tooltip("배경 밝기에 맞춰 색을 바꿀 HUD 장식판 (밝은 배경용 흰 그라데이션 등)")]
    [SerializeField] private List<Graphic> hudDecor = new();
    [Tooltip("앞쪽 레이어(전경 소품)는 따로 캔버스를 두지 않고 유닛과 같은 캔버스에서 이 형제 바로 앞에 끼운다 (유닛 뒤 · END TURN 앞)")]
    [SerializeField] private string foregroundBefore = "EndTurnButton";

    private Image _image;
    private BattleStageData _current;
    private Material _ally, _enemy;
    private RectTransform _foreground;

    public BattleStageData Current => _current;
    // 지금 깔린 배경 (아군 진형 오프셋 등 다른 화면이 읽는다)
    public static BattleStageData Active { get; private set; }

    private void Awake() => _image = GetComponent<Image>();

    private void OnEnable()
    {
        if (BattleManager.Instance != null) BattleManager.Instance.OnBattleStarted += OnBattleStarted;
    }

    private void Start()
    {
        // OnEnable 때 BattleManager가 아직 없었을 수 있다
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleStarted -= OnBattleStarted;
            BattleManager.Instance.OnBattleStarted += OnBattleStarted;
        }
        if (_current == null) Apply(Pick());
    }

    private void OnDisable()
    {
        if (BattleManager.Instance != null) BattleManager.Instance.OnBattleStarted -= OnBattleStarted;
    }

    private void OnBattleStarted() => Apply(Pick());

    private BattleStageData Pick()
    {
        if (stages == null || stages.Count == 0) return null;

        // 전용 배경: 이번 전투의 적이 featuredEnemies에 들어 있으면 그 배경
        var enemies = BattleManager.Instance != null ? BattleManager.Instance.State?.Enemies : null;
        if (enemies != null)
            foreach (var stage in stages)
                if (stage != null && stage.featuredEnemies.Count > 0 && enemies.Exists(e => e?.Data != null && stage.featuredEnemies.Contains(e.Data)))
                    return stage;

        // 일반 배경 풀 (전용 배경은 뺀다)
        int shownFloor = (RunData.Instance != null ? RunData.Instance.currentFloor : 0) + 1; // 화면에 보이는 층
        var pool = stages.FindAll(s => s != null && s.featuredEnemies.Count == 0
            && (s.minFloor == 0 && s.maxFloor == 0 || shownFloor >= s.minFloor && shownFloor <= (s.maxFloor > 0 ? s.maxFloor : int.MaxValue)));
        if (pool.Count == 0) pool = stages.FindAll(s => s != null && s.featuredEnemies.Count == 0); // 층에 맞는 배경이 없으면 아무거나
        if (pool.Count == 0) return null;
        int floor = RunData.Instance != null ? RunData.Instance.currentFloor : 0;
        int node = RunData.Instance != null ? RunData.Instance.currentNodeIndex : 0;
        int i = Mathf.Abs((floor * 73856093) ^ (node * 19349663)) % pool.Count;
        return pool[i];
    }

    public void Apply(BattleStageData stage)
    {
        if (stage == null) return;
        _current = stage;
        Active = stage;
        if (_image == null) _image = GetComponent<Image>();
        if (stage.background != null) _image.sprite = stage.background;

        // 유닛 머티리얼: 기본값(Resources)을 복사해 이 배경의 빛에 맞춘다. 원본 에셋은 건드리지 않는다
        _ally = Tune(_ally, "BattleUnit/BattleUnit_Ally", stage.allyGrade, stage.allyBrightness, -1f, stage);
        _enemy = Tune(_enemy, "BattleUnit/BattleUnit_Enemy", stage.enemyGrade, stage.enemyBrightness, stage.enemySaturation, stage);
        BattleUnitLook.SetStageMaterials(_ally, _enemy);

        foreach (var g in hudDecor) if (g != null) g.color = stage.hudDecorTint;
        if (ambience != null) ambience.Apply(stage);
        BuildForeground(stage);
    }

    // 앞쪽 레이어: 배경과 같은 캔버스 아래에 따로 정렬 순서를 가진 층을 만들고, 소품을 화면 아래에 붙인다
    private void BuildForeground(BattleStageData stage)
    {
        if (_foreground == null)
        {
            var go = new GameObject("StageForeground", typeof(RectTransform));
            go.layer = gameObject.layer;
            _foreground = (RectTransform)go.transform;
            var root = transform.parent;
            _foreground.SetParent(root, false);
            // 유닛과 같은 캔버스 안에서 계층 순서로 그린다: 유닛(PartyViewer · 동료) 뒤, END TURN 앞
            var hud = root.Find(foregroundBefore);
            _foreground.SetSiblingIndex(hud != null ? hud.GetSiblingIndex() : transform.GetSiblingIndex() + 1);
            _foreground.anchorMin = Vector2.zero; _foreground.anchorMax = Vector2.one;
            _foreground.offsetMin = _foreground.offsetMax = Vector2.zero;
        }
        for (int i = _foreground.childCount - 1; i >= 0; i--) Destroy(_foreground.GetChild(i).gameObject);
        if (stage.foreground == null) return;

        float screenH = _foreground.rect.height > 0f ? _foreground.rect.height : 1080f;
        foreach (var prop in stage.foreground)
        {
            if (prop == null || prop.sprite == null) continue;
            var go = new GameObject("FG_" + prop.sprite.name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.layer = gameObject.layer;
            var rt = (RectTransform)go.transform;
            rt.SetParent(_foreground, false);
            rt.anchorMin = rt.anchorMax = prop.position;
            rt.pivot = prop.pivot;
            float h = screenH * prop.height;
            rt.sizeDelta = new Vector2(h * prop.sprite.rect.width / prop.sprite.rect.height, h);
            rt.anchoredPosition = Vector2.zero;
            if (prop.flip) rt.localScale = new Vector3(-1f, 1f, 1f);
            var image = go.GetComponent<Image>();
            image.sprite = prop.sprite;
            image.color = prop.tint;
            image.raycastTarget = false;
            if (prop.sway > 0f)
            {
                // 바람: 아래 끝을 축으로 아주 조금 기울었다 돌아온다
                float period = Random.Range(3.2f, 4.6f);
                rt.localRotation = Quaternion.Euler(0f, 0f, -prop.sway * 0.5f);
                rt.DOLocalRotate(new Vector3(0f, 0f, prop.sway * 0.5f), period).SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo).SetDelay(Random.value * period).SetLink(go);
            }
        }
    }

    private static Material Tune(Material current, string resource, Color grade, float brightness, float saturation, BattleStageData stage)
    {
        if (current == null)
        {
            var src = Resources.Load<Material>(resource);
            if (src == null) return null;
            current = new Material(src) { name = src.name + " (Stage)", hideFlags = HideFlags.DontSave };
        }
        current.SetColor("_Grade", grade);
        current.SetFloat("_Brightness", brightness);
        if (saturation >= 0f) current.SetFloat("_Saturation", saturation);
        current.SetColor("_TopLight", stage.topLight);
        current.SetFloat("_TopFalloff", stage.topFalloff);
        current.SetFloat("_BottomShade", stage.bottomShade);
        current.SetColor("_RimColor", stage.rimColor);
        current.SetVector("_RimDir", new Vector4(stage.lightFrom.x, stage.lightFrom.y, 0, 0));
        return current;
    }
}

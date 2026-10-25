using System.Collections.Generic;
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

    private Image _image;
    private BattleStageData _current;
    private Material _ally, _enemy;

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

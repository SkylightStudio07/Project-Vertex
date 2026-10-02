using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 런 종료 화면. 두 경우에 뜬다.
//   - 런 클리어: 마지막 막(ActData.isFinalAct) 보스를 격파하고 보상을 닫으면 (GameManager.CompleteAct → Open)
//   - 패배: 플레이어가 쓰러지면 "작전 실패" 배너(BattleOperationFx)가 끝난 뒤 (OpenDefeat)
//   - 훈련장: 이기든 지든 (HandleVictory / HandleDefeat — 훈련 중이면 보상 화면 대신 여기)
// 버튼은 "기지로 귀환" — 씬 전환 막(귀환 문구)을 거쳐 로비 씬으로 돌아간다. 로비 씬이 빌드에 없으면 예전처럼 현재 씬을 다시 연다.
// 판: 종이 "작전 보고서" (조각 Resources/BattleFx/Result, 원본 ArtDirection/BattleFxMockup/Extracted/Result).
//   기록 칸 4개 — 실전: 도달 층 / 격퇴 / 획득 카드 / 진행 턴 (RunStats), 훈련: 진행 턴 / 격퇴 / 남은 HP / 덱
//   클리어·훈련 완료는 청록, 실패·훈련 종료는 회색 강조선.
// 평소엔 켜 둔 채 투명·입력 차단 없음으로 두고, 열 때만 페이드 인한다(비활성 오브젝트는 Awake가 안 돌아 Instance가 비기 때문).
[RequireComponent(typeof(CanvasGroup))]
public class RunClearView : MonoBehaviour
{
    public static RunClearView Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private TextMeshProUGUI englishText;
    [SerializeField] private List<TextMeshProUGUI> statLabels = new();
    [SerializeField] private List<TextMeshProUGUI> statValues = new();
    [Header("상태 강조선 (클리어 = 청록 / 실패 = 회색)")]
    [SerializeField] private Image titleAccent;
    [SerializeField] private List<Image> statAccents = new();
    [SerializeField] private Image cornerAccent;
    [SerializeField] private Sprite accentClear;
    [SerializeField] private Sprite accentDefeat;
    [SerializeField] private Sprite statAccentClear;
    [SerializeField] private Sprite statAccentDefeat;
    [SerializeField] private Sprite cornerClear;
    [SerializeField] private Sprite cornerDefeat;
    [SerializeField] private Button restartButton;
    [SerializeField, Min(0f)] private float fadeDuration = 0.8f;
    [Tooltip("귀환할 로비 씬 이름 (Build Settings에 있어야 한다)")]
    [SerializeField] private string lobbySceneName = "Lobby";
    [Tooltip("패배 후 이 화면이 뜨기까지 기다리는 시간 (작전 실패 배너가 끝나는 시점)")]
    [SerializeField, Min(0f)] private float defeatDelay = 2.2f;

    private CanvasGroup _group;
    private BattleManager _battle;
    private bool _open;

    private void Awake()
    {
        Instance = this;
        _group = GetComponent<CanvasGroup>();
        SetVisible(false);
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(ReturnToLobby);
        }
    }

    private void Start()
    {
        _battle = BattleManager.Instance;
        if (_battle != null)
        {
            _battle.OnBattleDefeat += HandleDefeat;
            _battle.OnBattleVictory += HandleVictory;
        }
    }

    private void OnDestroy()
    {
        if (_battle != null)
        {
            _battle.OnBattleDefeat -= HandleDefeat;
            _battle.OnBattleVictory -= HandleVictory;
        }
        if (Instance == this) Instance = null;
    }

    public void Open(ActData clearedAct)
    {
        QuestManager.Instance.SettleRun(); // 런 클리어 → 의뢰 정산 (보상은 로비에서 수령)
        Show("런 클리어",
             clearedAct != null && !string.IsNullOrWhiteSpace(clearedAct.actName)
                 ? $"{clearedAct.actNumber}막 · {clearedAct.actName} 돌파"
                 : "모든 작전을 완료했습니다", success: true);
    }

    // 실전 승리는 보상 화면(RewardsView)이 받는다. 훈련장만 여기서 결과를 띄운다.
    private void HandleVictory(BattleReward reward)
    {
        if (!TrainingSession.IsActive) return;
        DOVirtual.DelayedCall(defeatDelay, () => Show("훈련 완료", $"{TrainingTargetName()} 격파", success: true), ignoreTimeScale: true)
                 .SetLink(gameObject);
    }

    private static string TrainingTargetName()
    {
        var encounter = TrainingSession.Encounter;
        if (encounter == null) return "훈련 상대";
        if (!string.IsNullOrWhiteSpace(encounter.bossTitle)) return encounter.bossTitle;
        var first = encounter.enemies != null && encounter.enemies.Count > 0 ? encounter.enemies[0] : null;
        return first != null ? first.enemyName : encounter.name;
    }

    private void HandleDefeat()
    {
        if (TrainingSession.IsActive)
        {
            DOVirtual.DelayedCall(defeatDelay, () => Show("훈련 종료", $"{TrainingTargetName()}에게 패배했습니다", success: false), ignoreTimeScale: true)
                     .SetLink(gameObject);
            return;
        }
        QuestManager.Instance.SettleRun(); // 패배도 런 종료 — 이미 정산했으면 무시된다
        int floor = RunData.Instance != null ? RunData.Instance.currentFloor + 1 : 0;
        int chapter = GameManager.Instance != null ? GameManager.Instance.Chapter : 1;
        DOVirtual.DelayedCall(defeatDelay, () => Show("작전 실패", $"{chapter}막 {floor}층에서 쓰러졌습니다", success: false), ignoreTimeScale: true)
                 .SetLink(gameObject);
    }

    private void Show(string title, string subtitle, bool success)
    {
        if (_open) return;
        _open = true;
        if (titleText != null) titleText.text = title;
        if (subtitleText != null) subtitleText.text = subtitle;
        if (englishText != null) englishText.text = TrainingSession.IsActive ? "RESULT  /  TRAINING REPORT" : "RESULT  /  OPERATION REPORT";
        FillStats();
        ApplyAccent(success);

        SetVisible(true);
        _group.alpha = 0f;
        _group.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad).SetUpdate(true).SetLink(gameObject);
    }

    private void FillStats()
    {
        (string label, string value)[] stats;
        if (TrainingSession.IsActive)
        {
            int hp = GameManager.Instance != null ? GameManager.Instance.PlayerHP : 0;
            int deck = DeckManager.Instance != null ? DeckManager.Instance.PlayerDeck.Count : 0;
            stats = new[] { ("진행 턴", Two(RunStats.Turns)), ("격퇴", Two(RunStats.EnemiesDefeated)), ("남은 HP", hp.ToString()), ("덱", Two(deck)) };
        }
        else
        {
            int floor = RunData.Instance != null ? RunData.Instance.currentFloor + 1 : 0;
            stats = new[] { ("도달 층", Two(floor)), ("격퇴", Two(RunStats.EnemiesDefeated)), ("획득 카드", Two(RunStats.CardsObtained)), ("진행 턴", Two(RunStats.Turns)) };
        }
        for (int i = 0; i < stats.Length; i++)
        {
            if (i < statLabels.Count && statLabels[i] != null) statLabels[i].text = stats[i].label;
            if (i < statValues.Count && statValues[i] != null) statValues[i].text = stats[i].value;
        }
    }

    private static string Two(int n) => n.ToString("00");

    private void ApplyAccent(bool success)
    {
        if (titleAccent != null) titleAccent.sprite = success ? accentClear : accentDefeat;
        foreach (var a in statAccents) if (a != null) a.sprite = success ? statAccentClear : statAccentDefeat;
        if (cornerAccent != null) cornerAccent.sprite = success ? cornerClear : cornerDefeat;
    }

    private void SetVisible(bool visible)
    {
        _group.alpha = visible ? _group.alpha : 0f;
        _group.blocksRaycasts = visible;
        _group.interactable = visible;
    }

    private void ReturnToLobby()
    {
        if (restartButton != null) restartButton.interactable = false;
        TrainingSession.End(); // 훈련장 출격 정보는 귀환과 함께 비운다
        if (!string.IsNullOrWhiteSpace(lobbySceneName) && Application.CanStreamedLevelBeLoaded(lobbySceneName))
        {
            SceneTransition.Load(lobbySceneName, "귀환", "기지로 복귀합니다", "RETURN  //  BASE");
            return;
        }

        // 로비가 빌드에 없으면 현재 씬을 다시 열어 새 런을 시작한다
        var scene = SceneManager.GetActiveScene();
#if UNITY_EDITOR
        UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(scene.path, new LoadSceneParameters(LoadSceneMode.Single));
#else
        SceneManager.LoadScene(scene.buildIndex);
#endif
    }
}

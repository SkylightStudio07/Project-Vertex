using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 런 종료 화면. 두 경우에 뜬다.
//   - 런 클리어: 마지막 막(ActData.isFinalAct) 보스를 격파하고 보상을 닫으면 (GameManager.CompleteAct → Open)
//   - 패배: 플레이어가 쓰러지면 "작전 실패" 배너(BattleOperationFx)가 끝난 뒤 (OpenDefeat)
// 버튼은 "기지로 귀환" — 씬 전환 막(귀환 문구)을 거쳐 로비 씬으로 돌아간다. 로비 씬이 빌드에 없으면 예전처럼 현재 씬을 다시 연다.
// 평소엔 켜 둔 채 투명·입력 차단 없음으로 두고, 열 때만 페이드 인한다(비활성 오브젝트는 Awake가 안 돌아 Instance가 비기 때문).
[RequireComponent(typeof(CanvasGroup))]
public class RunClearView : MonoBehaviour
{
    public static RunClearView Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;
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
            var label = restartButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null) label.text = "기지로 귀환";
        }
    }

    private void Start()
    {
        _battle = BattleManager.Instance;
        if (_battle != null) _battle.OnBattleDefeat += HandleDefeat;
    }

    private void OnDestroy()
    {
        if (_battle != null) _battle.OnBattleDefeat -= HandleDefeat;
        if (Instance == this) Instance = null;
    }

    public void Open(ActData clearedAct)
    {
        QuestManager.Instance.SettleRun(); // 런 클리어 → 의뢰 정산 (보상은 로비에서 수령)
        Show("런 클리어",
             clearedAct != null && !string.IsNullOrWhiteSpace(clearedAct.actName)
                 ? $"{clearedAct.actNumber}막 · {clearedAct.actName} 돌파"
                 : "버텍스 돌파");
    }

    private void HandleDefeat()
    {
        QuestManager.Instance.SettleRun(); // 패배도 런 종료 — 이미 정산했으면 무시된다
        int floor = RunData.Instance != null ? RunData.Instance.currentFloor + 1 : 0;
        int chapter = GameManager.Instance != null ? GameManager.Instance.Chapter : 1;
        DOVirtual.DelayedCall(defeatDelay, () => Show("작전 실패", $"{chapter}막 {floor}층에서 쓰러졌습니다"), ignoreTimeScale: true)
                 .SetLink(gameObject);
    }

    private void Show(string title, string subtitle)
    {
        if (_open) return;
        _open = true;
        if (titleText != null) titleText.text = title;
        if (subtitleText != null) subtitleText.text = subtitle;

        SetVisible(true);
        _group.alpha = 0f;
        _group.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad).SetUpdate(true).SetLink(gameObject);
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

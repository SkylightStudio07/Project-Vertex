using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 마지막 막(ActData.isFinalAct) 보스를 격파하고 보상을 닫으면 뜨는 런 클리어 화면.
// 평소엔 켜 둔 채 투명·입력 차단 없음으로 두고, Open 때만 페이드 인한다(비활성 오브젝트는 Awake가 안 돌아 Instance가 비기 때문).
// "다시 시작"은 지금 씬을 다시 불러 새 런을 연다. 로비가 연결되면 로비 복귀로 바꾼다.
[RequireComponent(typeof(CanvasGroup))]
public class RunClearView : MonoBehaviour
{
    public static RunClearView Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private Button restartButton;
    [SerializeField, Min(0f)] private float fadeDuration = 0.8f;

    private CanvasGroup _group;

    private void Awake()
    {
        Instance = this;
        _group = GetComponent<CanvasGroup>();
        SetVisible(false);
        if (restartButton != null) restartButton.onClick.AddListener(Restart);
    }

    public void Open(ActData clearedAct)
    {
        if (titleText != null) titleText.text = "런 클리어";
        if (subtitleText != null)
            subtitleText.text = clearedAct != null && !string.IsNullOrWhiteSpace(clearedAct.actName)
                ? $"{clearedAct.actNumber}막 · {clearedAct.actName} 돌파"
                : "버텍스 돌파";

        SetVisible(true);
        _group.alpha = 0f;
        _group.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad).SetLink(gameObject);
    }

    private void SetVisible(bool visible)
    {
        _group.alpha = visible ? _group.alpha : 0f;
        _group.blocksRaycasts = visible;
        _group.interactable = visible;
    }

    private void Restart()
    {
        var scene = SceneManager.GetActiveScene();
#if UNITY_EDITOR
        // 에디터에서는 빌드 세팅에 없는 씬도 다시 불러올 수 있게 경로로 로드한다
        UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(scene.path, new LoadSceneParameters(LoadSceneMode.Single));
#else
        SceneManager.LoadScene(scene.buildIndex);
#endif
    }
}

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 로비 시설 담당 캐릭터의 말: 열 때 인사(호감도 +, 등급이 오르면 등급업 한마디) · 누르면 다른 대사 · 가만히 있으면 혼잣말.
// 글자는 한 자씩 찍히고, 찍히는 중에 누르면 끝까지 바로 보인다. 시설 화면(LobbyShopView 등)의 상황 대사도 Say로 넘긴다.
public class LobbyKeeperTalk : MonoBehaviour
{
    [SerializeField] private LobbyKeeperData data;
    [SerializeField] private TextMeshProUGUI lineText;
    [SerializeField] private TextMeshProUGUI nameText;
    [Tooltip("누르면 대사가 바뀌는 곳 (캐릭터 그림 · 대사창)")]
    [SerializeField] private List<Graphic> clickTargets = new();
    [SerializeField, Min(5f)] private float charsPerSecond = 40f;

    // 로비에 들어올 때마다 시설별 방문 호감도는 한 번만
    private static readonly HashSet<string> s_visitedThisLobby = new();
    private static bool s_hooked;

    private float _idle;
    private float _typed;
    private bool _typing;
    private string _current;

    public LobbyKeeperData Data => data;

    private void Awake()
    {
        if (!s_hooked) { SceneManager.sceneLoaded += (_, _) => s_visitedThisLobby.Clear(); s_hooked = true; }
        foreach (var g in clickTargets)
        {
            if (g == null) continue;
            g.raycastTarget = true;
            var b = g.GetComponent<Button>();
            if (b == null) b = g.gameObject.AddComponent<Button>();
            b.transition = Selectable.Transition.None;
            b.onClick.AddListener(OnClicked);
        }
    }

    private static int Tier(string id) => BlessingAffinityManager.Instance != null ? BlessingAffinityManager.Instance.GetAffinityTier(id) : 0;

    private void OnEnable()
    {
        if (data == null) return;
        if (nameText != null) nameText.text = data.displayName;
        int before = Tier(data.entityId);
        if (BlessingAffinityManager.Instance != null && s_visitedThisLobby.Add(data.entityId))
            BlessingAffinityManager.Instance.AddAffinity(data.entityId, data.visitAffinityGain);
        int after = Tier(data.entityId);
        string tierUp = after > before ? data.TierUpLine(after) : null;
        Say(tierUp ?? LobbyKeeperData.Pick(data.greetingLines, after, _current));
    }

    // 다시 첫 화면으로 돌아왔을 때 (안쪽 화면에서 복귀)
    public void Greet()
    {
        if (data != null) Say(LobbyKeeperData.Pick(data.greetingLines, Tier(data.entityId), _current));
    }

    public void Say(string line)
    {
        if (lineText == null || string.IsNullOrEmpty(line)) return;
        _current = line;
        lineText.text = line;
        lineText.maxVisibleCharacters = 0;
        _typed = 0f;
        _typing = true;
        _idle = 0f;
    }

    private void OnClicked()
    {
        if (data == null) return;
        if (_typing) { FinishTyping(); return; }
        Say(LobbyKeeperData.Pick(data.clickLines, Tier(data.entityId), _current));
    }

    private void FinishTyping()
    {
        _typing = false;
        lineText.maxVisibleCharacters = 99999;
    }

    private void Update()
    {
        if (data == null || lineText == null) return;
        float dt = Time.unscaledDeltaTime;
        if (_typing)
        {
            _typed += dt * charsPerSecond;
            lineText.maxVisibleCharacters = Mathf.FloorToInt(_typed);
            if (_typed >= lineText.text.Length) FinishTyping();
            return;
        }
        if (!lineText.gameObject.activeInHierarchy) return; // 안쪽 화면을 보는 동안엔 혼잣말 안 함
        _idle += dt;
        if (_idle >= data.idleInterval) Say(LobbyKeeperData.Pick(data.idleLines, Tier(data.entityId), _current));
    }
}

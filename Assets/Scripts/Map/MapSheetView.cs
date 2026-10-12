using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 맵 v1 (작전 지도) 판의 정보 표시. MapView에 붙인다.
//   - 제목 줄: "1막" + 막 이름, 오른쪽 "BOSS · 남은 층 NN"
//   - 층 눈금: 스크롤 내용(MapContent) 위쪽에 층마다 한 칸 (지나온 층 / 현재 층 청록 / 남은 층 / BOSS). 스크롤과 함께 움직인다
//   - 스크롤 막대: 트랙 위 손잡이 위치
//   - 맵이 열려 있는 동안 전투 HUD(HP·자원 모듈·END TURN)를 숨긴다
// 노드·연결선 상태는 MapNodeView / MapConnectionLine이 맡는다. 조각: Assets/Art/Maps/v1 (원본 ArtDirection/MapMockup/Extracted)
public class MapSheetView : MonoBehaviour
{
    [SerializeField] private MapUIController controller;

    [Header("제목 줄")]
    [SerializeField] private TextMeshProUGUI actNumberText;
    [SerializeField] private TextMeshProUGUI actNameText;
    [SerializeField] private TextMeshProUGUI remainingText;

    [Header("층 눈금 (스크롤 내용 위쪽)")]
    [SerializeField] private Sprite tickPast, tickCurrent, tickFuture, tickBoss;
    [SerializeField] private TMP_FontAsset tickFont;
    [SerializeField] private Vector2 tickSize = new(200f, 28f);

    [Header("스크롤 막대")]
    [SerializeField] private RectTransform scrollTrack;
    [SerializeField] private RectTransform scrollHandle;

    [Header("맵이 열려 있는 동안 숨길 전투 HUD")]
    [SerializeField] private List<GameObject> hideWhileOpen = new();

    private static readonly Color Ink = new(0.086f, 0.094f, 0.106f, 1f);
    private static readonly Color Faint = new(0.55f, 0.58f, 0.62f, 1f);

    private readonly List<GameObject> _hidden = new();
    private readonly List<(Image img, TextMeshProUGUI label)> _ticks = new();
    private RectTransform _tickRoot;
    private MapData _builtFor;

    private void OnEnable()
    {
        _hidden.Clear();
        foreach (var go in hideWhileOpen)
            if (go != null && go.activeSelf) { go.SetActive(false); _hidden.Add(go); }
        if (controller != null) controller.MapRefreshed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        foreach (var go in _hidden) if (go != null) go.SetActive(true);
        _hidden.Clear();
        if (controller != null) controller.MapRefreshed -= Refresh;
    }

    private void Update()
    {
        if (scrollTrack == null || scrollHandle == null || controller == null || controller.ScrollRect == null) return;
        float t = Mathf.Clamp01(controller.ScrollRect.horizontalNormalizedPosition);
        float travel = scrollTrack.rect.width - scrollHandle.rect.width;
        scrollHandle.anchoredPosition = new Vector2(scrollTrack.anchoredPosition.x + travel * t, scrollHandle.anchoredPosition.y);
    }

    public void Refresh()
    {
        var run = RunData.Instance;
        var map = run != null ? run.mapData : null;
        var gm = GameManager.Instance;
        var act = gm != null ? gm.CurrentAct : null;
        int chapter = gm != null ? gm.Chapter : 1;

        if (actNumberText != null) actNumberText.text = $"{chapter}막";
        if (actNameText != null) actNameText.text = act != null ? act.actName : "";

        if (map == null || map.floors == null) return;
        int floors = map.floors.Count;
        int cur = run.currentFloor;
        if (remainingText != null) remainingText.text = $"BOSS  ·  남은 층 <b>{Mathf.Max(0, floors - 1 - cur)}</b>";

        if (_builtFor != map || _ticks.Count != floors) BuildTicks(floors);
        for (int f = 0; f < _ticks.Count; f++)
        {
            var (img, label) = _ticks[f];
            bool boss = f == floors - 1;
            bool current = f == cur;
            img.sprite = current ? tickCurrent : boss ? tickBoss : f < cur ? tickPast : tickFuture;
            label.text = boss ? "BOSS" : (f + 1).ToString("00");
            label.color = current ? Color.white : f < cur || boss ? Ink : Faint;
        }
    }

    private void BuildTicks(int floors)
    {
        _builtFor = RunData.Instance.mapData;
        _ticks.Clear();
        var content = controller != null ? controller.MapContent : null;
        if (content == null) return;
        if (_tickRoot == null)
        {
            var existing = content.Find("FloorTicks");
            _tickRoot = existing != null ? (RectTransform)existing : NewRect("FloorTicks", content);
        }
        _tickRoot.anchorMin = new Vector2(0f, 1f); _tickRoot.anchorMax = new Vector2(1f, 1f);
        _tickRoot.pivot = new Vector2(0.5f, 1f); _tickRoot.sizeDelta = new Vector2(0f, tickSize.y); _tickRoot.anchoredPosition = Vector2.zero;
        _tickRoot.SetSiblingIndex(Mathf.Min(1, content.childCount - 1)); // 지형 바로 위, 선·노드 아래
        for (int i = _tickRoot.childCount - 1; i >= 0; i--) Destroy(_tickRoot.GetChild(i).gameObject);

        for (int f = 0; f < floors; f++)
        {
            var rt = NewRect("Tick_" + f, _tickRoot);
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f); rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = tickSize;
            rt.anchoredPosition = new Vector2(f * controller.FloorSpacing + controller.HorizontalPadding, 0f);
            rt.gameObject.AddComponent<CanvasRenderer>();
            var img = rt.gameObject.AddComponent<Image>(); img.raycastTarget = false;
            var lrt = NewRect("Label", rt);
            lrt.anchorMin = new Vector2(0.5f, 1f); lrt.anchorMax = new Vector2(0.5f, 1f); lrt.pivot = new Vector2(0.5f, 1f);
            lrt.sizeDelta = new Vector2(90f, 20f); lrt.anchoredPosition = new Vector2(0f, -1f);
            lrt.gameObject.AddComponent<CanvasRenderer>();
            var t = lrt.gameObject.AddComponent<TextMeshProUGUI>();
            if (tickFont != null) t.font = tickFont;
            t.fontSize = 15f; t.alignment = TextAlignmentOptions.Center; t.characterSpacing = 6f; t.raycastTarget = false;
            t.textWrappingMode = TextWrappingModes.NoWrap; t.fontStyle = FontStyles.Bold;
            _ticks.Add((img, t));
        }
    }

    private static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        return rt;
    }
}

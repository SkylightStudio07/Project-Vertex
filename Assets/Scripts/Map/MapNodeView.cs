using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// 노드가 현재 어떤 상태인지 열거형
// MapUIController가 이 상태를 계산해서 SetState()로 전달함
public enum MapNodeState
{
    Locked,     // 아직 갈 수 없는 노드 (회색). 디폴트
    Accessible, // 현재 위치에서 이동 가능한 노드 (흰색, 클릭 가능) - 살짝 백색 색조 더하기
    Current,    // 플레이어가 현재 있는 노드 (시안)
    Visited     // 이미 방문한 노드 - 회색
}

// 노드 하나하나 시각적 처리 클래스. MapUIController가 프리팹을 Instantiate한 뒤 Setup()으로 초기화하고, 맵 이동 시마다 SetState()로 상태 갱신.
public class MapNodeView : MonoBehaviour
{
    // NodeType별 스프라이트 매핑 - 어차피 9개니까 선형으로 무식하게 해도 문제없음.
    [Serializable]
    public struct NodeSprite
    {
        public NodeType nodeType;
        public Sprite sprite;
        public bool hasIntegratedFrame;
    }

    [SerializeField] private List<NodeSprite> nodeSprites;

    [SerializeField] private Image iconImage; // 노드 아이콘 표시용 Image 컴포넌트 (프리팹에서 연결)
    [SerializeField] private Image nodeBackdrop;
    [SerializeField] private Button button; 
    [SerializeField] private GameObject currentMarker;    // 현재 위치 마커

    // 각 상태별 색상 (iconImage에 tinting으로 적용) - 클로드야 고마워
    private static readonly Color ColorLocked     = new(0.35f, 0.35f, 0.35f, 1f);
    private static readonly Color ColorAccessible = Color.white;
    private static readonly Color ColorCurrent    = new(0.65f, 0.92f, 1f, 1f);
    private static readonly Color ColorVisited    = new(0.55f, 0.55f, 0.55f, 1f);

    // 이 View가 표현하는 MapNode 데이터
    public MapNode Data { get; private set; }
    private bool hasIntegratedFrame;

    // 상태 연출: 현재 위치는 마커가 천천히 숨 쉬듯 커졌다 작아지고, 이동 가능한 노드는 아이콘이 살짝 떠오른다.
    private const float PulseScale = 1.12f;
    private const float PulsePeriod = 1.1f;
    private const float FloatHeight = 4f;
    private const float FloatPeriod = 0.9f;

    private Vector3 _markerBaseScale = Vector3.one;
    private Vector2 _iconBasePos;
    private bool _hasBase;
    private MapNodeState _state;

    private void CaptureBase()
    {
        if (_hasBase) return;
        if (currentMarker != null) _markerBaseScale = currentMarker.transform.localScale;
        _iconBasePos = iconImage.rectTransform.anchoredPosition;
        _hasBase = true;
    }

    // 맵을 열 때 MapUIController가 층 순서대로 지연을 줘서 호출한다.
    public void PlayReveal(float delay, float duration)
    {
        transform.DOKill();
        transform.localScale = Vector3.zero;
        transform.DOScale(1f, duration).SetDelay(delay).SetEase(Ease.OutBack)
                 .SetUpdate(true).SetLink(gameObject);
    }

    // 루프 트윈은 맵 패널이 꺼지면 멈췄다가, 다시 켜질 때 현재 상태 기준으로 재시작한다.
    private void OnEnable()
    {
        if (Data != null) PlayStateMotion(_state);
    }

    private void OnDisable() => StopStateMotion();

    private void StopStateMotion()
    {
        if (!_hasBase) return;
        iconImage.rectTransform.DOKill();
        iconImage.rectTransform.anchoredPosition = _iconBasePos;
        if (currentMarker != null)
        {
            currentMarker.transform.DOKill();
            currentMarker.transform.localScale = _markerBaseScale;
        }
    }

    private void PlayStateMotion(MapNodeState state)
    {
        CaptureBase();
        StopStateMotion();
        if (!isActiveAndEnabled) return;

        if (state == MapNodeState.Current && currentMarker != null)
        {
            currentMarker.transform.DOScale(_markerBaseScale * PulseScale, PulsePeriod * 0.5f)
                         .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);
        }
        else if (state == MapNodeState.Accessible)
        {
            iconImage.rectTransform.DOAnchorPos(_iconBasePos + new Vector2(0f, FloatHeight), FloatPeriod * 0.5f)
                     .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);
        }
    }

    // 노드 초기화. MapUIController가 프리팹을 생성한 직후 호출함.
    // Action<MapNode>: "MapNode를 인자로 받는 함수"를 변수처럼 전달하는 C# 문법.
    // 클릭 시 이 함수를 호출해서 MapUIController 쪽에 "어떤 노드가 눌렸는지" 알려줌.
    public void Setup(MapNode data, Action<MapNode> onClick)
    {
        Data = data;
        var presentation = GetNodeSprite(data.nodeType);
        iconImage.sprite = presentation.sprite;
        hasIntegratedFrame = presentation.hasIntegratedFrame;
        if (nodeBackdrop != null) nodeBackdrop.enabled = !hasIntegratedFrame;

        // 중복 등록 방지 후 클릭 리스너 등록
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke(Data));
    }

    // 노드의 시각 상태를 변경. MapUIController가 맵 이동 시마다 전체 노드에 호출함.
    public void SetState(MapNodeState state)
    {
        // switch expression: state 값에 따라 색상을 결정하는 간결한 분기 문법
        iconImage.color = state switch
        {
            MapNodeState.Locked     => hasIntegratedFrame ? new Color(0.72f, 0.72f, 0.72f, 1f) : ColorLocked,
            MapNodeState.Accessible => ColorAccessible,
            MapNodeState.Current    => ColorCurrent,
            MapNodeState.Visited    => hasIntegratedFrame ? new Color(0.86f, 0.86f, 0.86f, 1f) : ColorVisited,
            _                       => Color.white  // 예외 케이스 (발생하지 않음)
        };

        // Accessible 상태일 때만 버튼 클릭 가능
        button.interactable = state == MapNodeState.Accessible;

        // 현재 위치 마커는 Current 상태일 때만 표시
        if (currentMarker != null)
            currentMarker.SetActive(state == MapNodeState.Current);

        _state = state;
        PlayStateMotion(state);
    }

    // nodeType에 맞는 스프라이트 반환. 일반 선형 탐색.
    private NodeSprite GetNodeSprite(NodeType type)
    {
        foreach (var entry in nodeSprites)
            if (entry.nodeType == type) return entry;
        return default;
    }
}

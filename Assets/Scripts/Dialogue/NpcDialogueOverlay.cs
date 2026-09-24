using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// 화면 하단 대사창 오버레이(DialogueUI_WhiteVertex_PinkKey_Transparent)로 BlessingDialogueSequence를 재생한다.
// 상점 주인처럼 "NPC 한 명이 몇 줄 말하고 끝나는" 방문 대사용. 분기·다인 연출이 필요한 스토리 대화는 DialogueView(JSON) 담당.
//
// 단계 진행 규칙:
//   - playerAnswerText가 비어 있으면: 화면 클릭(또는 스페이스/엔터)으로 다음 줄
//   - 채워져 있으면: 타이핑이 끝난 뒤 답변 버튼이 뜨고, 그 버튼을 눌러야 다음 줄 (호감도 등급업 이벤트용)
// 타이핑 중 클릭은 문장을 즉시 완성하는 데만 쓰인다(DialogueView와 같은 관례).
public class NpcDialogueOverlay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI lineText;
    [Tooltip("화면 전체를 덮는 클릭 영역 — 뒤쪽 UI 입력도 함께 막는다")]
    [SerializeField] private Button advanceArea;
    [SerializeField] private GameObject nextIndicator;
    [SerializeField] private Button answerButton;
    [SerializeField] private TextMeshProUGUI answerText;
    [SerializeField] private float typewriterCharsPerSecond = 40f;
    [Tooltip("이 오버레이 Canvas의 정렬 순서. 상점(Nodes 40)·MAP/DECK(55) 위, 스토리 대화(60)·덱 목록(99) 아래")]
    [SerializeField] private int sortingOrder = 58;

    private BlessingDialogueSequence _sequence;
    private string _defaultSpeaker;
    private int _index;
    private Action _onComplete;
    private TypewriterPrinter _typewriter;
    private bool _waitingAnswer;

    public bool IsPlaying => gameObject.activeSelf;

    private void Awake()
    {
        _typewriter = new TypewriterPrinter(this);
        if (advanceArea != null) advanceArea.onClick.AddListener(OnAdvance);
        if (answerButton != null) answerButton.onClick.AddListener(OnAnswer);
    }

    // 비활성 오브젝트의 Canvas는 에디터에서 overrideSorting을 켜도 저장되지 않아, 켜질 때마다 런타임에 지정한다
    private void OnEnable()
    {
        if (!TryGetComponent<Canvas>(out var canvas)) canvas = gameObject.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = sortingOrder;
        if (!TryGetComponent<GraphicRaycaster>(out _)) gameObject.AddComponent<GraphicRaycaster>();
    }

    public void Play(BlessingDialogueSequence sequence, string defaultSpeaker, Action onComplete)
    {
        if (sequence == null || sequence.steps == null || sequence.steps.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        // 비활성으로 저장된 오브젝트는 켜지는 순간 Awake가 돈다 — 그 전에 _typewriter를 쓰지 않도록 먼저 켠다
        gameObject.SetActive(true);
        _typewriter ??= new TypewriterPrinter(this);
        _sequence = sequence;
        _defaultSpeaker = defaultSpeaker;
        _onComplete = onComplete;
        _index = 0;
        ShowStep();
    }

    private void ShowStep()
    {
        var step = _sequence.steps[_index];
        if (nameText != null) nameText.text = string.IsNullOrEmpty(step.speakerName) ? _defaultSpeaker : step.speakerName;
        _typewriter.Play(lineText, step.npcDialogue, typewriterCharsPerSecond);

        _waitingAnswer = !string.IsNullOrWhiteSpace(step.playerAnswerText);
        if (answerText != null) answerText.text = step.playerAnswerText;
        if (answerButton != null) answerButton.gameObject.SetActive(false); // 타이핑이 끝나면 Update에서 켠다
        if (nextIndicator != null) nextIndicator.SetActive(false);
    }

    private void Update()
    {
        // 타이핑이 끝난 순간 다음 입력 수단(답변 버튼 또는 ▶ 표시)을 보여준다
        if (!_typewriter.IsTyping)
        {
            if (_waitingAnswer && answerButton != null && !answerButton.gameObject.activeSelf)
                answerButton.gameObject.SetActive(true);
            else if (!_waitingAnswer && nextIndicator != null && !nextIndicator.activeSelf)
                nextIndicator.SetActive(true);
        }

        if (Keyboard.current == null) return;
        if (Keyboard.current[Key.Space].wasPressedThisFrame || Keyboard.current[Key.Enter].wasPressedThisFrame)
        {
            if (_waitingAnswer && !_typewriter.IsTyping) OnAnswer();
            else OnAdvance();
        }
    }

    // 화면 클릭: 타이핑 중이면 완성, 답변이 필요한 줄이면 무시, 아니면 다음 줄
    private void OnAdvance()
    {
        if (_typewriter.CompleteImmediately()) return;
        if (_waitingAnswer) return;
        Next();
    }

    private void OnAnswer()
    {
        if (_typewriter.CompleteImmediately()) return;
        Next();
    }

    private void Next()
    {
        _index++;
        if (_index < _sequence.steps.Count)
        {
            ShowStep();
            return;
        }

        if (!string.IsNullOrEmpty(_sequence.setEventFlagOnComplete))
            BlessingAffinityManager.Instance.SetFlag(_sequence.setEventFlagOnComplete, true);

        var done = _onComplete;
        _onComplete = null;
        _sequence = null;
        gameObject.SetActive(false);
        done?.Invoke();
    }
}

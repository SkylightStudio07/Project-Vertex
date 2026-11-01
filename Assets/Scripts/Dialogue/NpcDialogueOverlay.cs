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
// 답변은 대화 선택지 v2(DialogueChoiceView.ShowStaged)로 띄운다 — 화면이 어두워지고 오른쪽에 고심하는 주인공, 말하는 NPC 스탠딩만 밝게.
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
    [Tooltip("대화창 v1: 이름 + 소속 + 고유색 선 + 초상 + 넘기기 표시 (비우면 이름 글자만)")]
    [SerializeField] private DialogueNameplate nameplate;
    [Tooltip("이 오버레이 Canvas의 정렬 순서. 상점(Nodes 40)·MAP/DECK(55) 위, 스토리 대화(60)·덱 목록(99) 아래")]
    [SerializeField] private int sortingOrder = 58;
    [Tooltip("켜면 답변을 대화 선택지 v2로 띄운다. 끄면 예전 답변 버튼")]
    [SerializeField] private bool useChoiceView = true;

    private BlessingDialogueSequence _sequence;
    private string _defaultSpeaker;
    private int _index;
    private Action _onComplete;
    private TypewriterPrinter _typewriter;
    private bool _waitingAnswer;
    private Sprite _portrait;
    private string _affiliation;
    private Color? _accent;
    private GameObject _standing;
    private DialogueChoiceView _choice;

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

    // portrait·affiliation·accent: 대화창 v1 초상 칸 / 소속 글자 / 이름 아래 선 색 (비우면 기본)
    // standing: 화면에 서 있는 NPC 그림 — 답변 선택지가 뜰 때 어둠 위로 꺼낸다 (비우면 전부 어둡게)
    public void Play(BlessingDialogueSequence sequence, string defaultSpeaker, Action onComplete,
                     Sprite portrait = null, string affiliation = null, Color? accent = null, GameObject standing = null)
    {
        _standing = standing;
        _portrait = portrait;
        _affiliation = affiliation;
        _accent = accent;
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
        string speaker = string.IsNullOrEmpty(step.speakerName) ? _defaultSpeaker : step.speakerName;
        if (nameplate != null) nameplate.Set(speaker, _affiliation, _accent, _portrait);
        else if (nameText != null) nameText.text = speaker;
        _typewriter.Play(lineText, step.npcDialogue, typewriterCharsPerSecond);

        _waitingAnswer = !string.IsNullOrWhiteSpace(step.playerAnswerText);
        if (answerText != null) answerText.text = step.playerAnswerText;
        if (answerButton != null) answerButton.gameObject.SetActive(false); // 타이핑이 끝나면 Update에서 켠다
        if (nextIndicator != null) nextIndicator.SetActive(false);
        if (nameplate != null) nameplate.SetAdvanceVisible(false);
    }

    private void Update()
    {
        // 타이핑이 끝난 순간 다음 입력 수단(답변 버튼 또는 ▶ 표시)을 보여준다
        if (!_typewriter.IsTyping)
        {
            if (_waitingAnswer && useChoiceView)
            {
                if (_choice == null) ShowAnswerChoice();
            }
            else if (_waitingAnswer && answerButton != null && !answerButton.gameObject.activeSelf)
                answerButton.gameObject.SetActive(true);
            else if (!_waitingAnswer && nameplate != null) nameplate.SetAdvanceVisible(true);
            else if (!_waitingAnswer && nextIndicator != null && !nextIndicator.activeSelf)
                nextIndicator.SetActive(true);
        }

        if (Keyboard.current == null || _choice != null) return; // 선택지가 떠 있으면 클릭으로만 고른다
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

    // 답변 한 줄을 선택지로. "[방랑자] …" 같은 화자 머리표는 떼어 낸다 — 오른쪽에 선 주인공이 곧 화자다
    private void ShowAnswerChoice()
    {
        string text = answerText != null && !string.IsNullOrEmpty(answerText.text) ? answerText.text : _sequence.steps[_index].playerAnswerText;
        text = text.Trim();
        if (text.StartsWith("[") && text.IndexOf(']') > 0) text = text.Substring(text.IndexOf(']') + 1).Trim();
        var stage = new DialogueChoiceView.Stage
        {
            DimOrder = sortingOrder - 2,
            ChoiceOrder = sortingOrder + 1,
            KeepBright = _standing != null ? new[] { _standing } : null,
        };
        _choice = DialogueChoiceView.ShowStaged((RectTransform)transform, stage,
            new[] { new DialogueChoiceView.Option(text) }, _ => { _choice = null; Next(); });
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

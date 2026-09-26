using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// 분기 가능한 다중 캐릭터 다이얼로그 재생기.
// 포맷 설명: Assets/Data/Dialogue/지침.md
// 이벤트 노드뿐 아니라 모닥불 대화, 합류 컷신 등에서도 재사용 가능하도록
// TextAsset + 완료 콜백만으로 동작한다 (EventView 등 호출부에 의존하지 않음).
public class DialogueView : MonoBehaviour
{
    [Header("캐릭터 슬롯 (좌→우, 최대 4명)")]
    [SerializeField] private CharacterSlotView[] characterSlots;

    [Header("화자 그림 (캐릭터 슬롯 대신 한 명만 크게)")]
    // 화자 ID가 협력자 charID(예: Cp_01)면 그 캐릭터 아트를 띄운다(성소 전신 아트 → 스탠딩 → 초상화 순).
    // 지문(화자 없음)이나 협력자가 아닌 화자일 때는 직전 그림을 어둡게 남긴다.
    [SerializeField] private Image speakerImage;
    [SerializeField] private GameObject speakerNameplate; // 지문일 때 숨길 이름표 (비우면 이름 텍스트만 비움)

    [Header("대사 UI")]
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI lineText;
    [SerializeField] private Button advanceButton;
    [SerializeField] private float typewriterCharsPerSecond = 40f; // 0 이하면 즉시 전체 표시(연출 끄기)

    [Header("선택지 UI")]
    [SerializeField] private Transform choiceContainer;
    [SerializeField] private Button choiceButtonPrefab;

    private DialogueScriptData _script;
    private Dictionary<string, DialogueNodeData> _nodeMap;
    private DialogueNodeData _currentNode;
    private int _lineIndex;
    private Action _onComplete;
    private readonly List<Button> _choiceButtons = new();
    private TypewriterPrinter typewriter;

    private void Awake()
    {
        // 주의: 이 오브젝트는 씬에서 비활성 상태로 시작해야 함.
        // 여기서 SetActive(false)를 부르면 Play()가 호출하는 최초 SetActive(true)를
        // 같은 프레임에서 취소해버려 첫 호출만 패널이 안 뜨는 버그가 생긴다 (EventView와 동일 이슈).
        advanceButton.onClick.AddListener(OnAdvanceClicked);
        typewriter = new TypewriterPrinter(this);
    }

    public void Play(TextAsset json, Action onComplete)
    {
        _onComplete = onComplete;

        if (json == null)
        {
            Debug.LogWarning("[Dialogue] 재생할 JSON이 없음. 즉시 완료 처리.");
            _onComplete?.Invoke();
            return;
        }

        _script = JsonUtility.FromJson<DialogueScriptData>(json.text);

        if (_script == null || _script.nodes == null || string.IsNullOrEmpty(_script.startNode))
        {
            Debug.LogWarning($"[Dialogue] '{json.name}' 파싱 실패 또는 nodes/startNode 누락. 즉시 완료 처리.");
            _onComplete?.Invoke();
            return;
        }

        // 새 대화마다 화자 그림을 비운다 (이전 대화 캐릭터가 남지 않게)
        if (speakerImage != null) { speakerImage.sprite = null; speakerImage.enabled = false; }

        _nodeMap = new Dictionary<string, DialogueNodeData>();
        foreach (var node in _script.nodes)
            _nodeMap[node.id] = node;

        SetupCharacterSlots();

        gameObject.SetActive(true);
        GoToNode(_script.startNode);
    }

    private void SetupCharacterSlots()
    {
        if (characterSlots == null || characterSlots.Length == 0)
        {
            // 화자 그림(speakerImage) 방식이면 슬롯이 없는 게 정상
            if (speakerImage == null) Debug.LogWarning("[Dialogue] characterSlots가 비어있음. Inspector 연결 확인 필요.");
            return;
        }

        foreach (var slot in characterSlots)
            slot.gameObject.SetActive(false);

        if (_script.characters == null) return;

        foreach (var character in _script.characters)
        {
            if (character.slot < 0 || character.slot >= characterSlots.Length)
            {
                Debug.LogWarning($"[Dialogue] 캐릭터 '{character.id}'의 slot({character.slot})이 슬롯 범위를 벗어남.");
                continue;
            }
            characterSlots[character.slot].Bind(character);
        }
    }

    private void GoToNode(string nodeId)
    {
        if (!_nodeMap.TryGetValue(nodeId, out _currentNode))
        {
            Debug.LogWarning($"[Dialogue] 노드 '{nodeId}'를 찾을 수 없음.");
            Finish();
            return;
        }
        _lineIndex = 0;
        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        HideChoices();

        if (_currentNode.lines == null || _lineIndex >= _currentNode.lines.Length)
        {
            Finish();
            return;
        }

        var line = _currentNode.lines[_lineIndex];
        switch (line.type)
        {
            case "line":
                ShowLine(line);
                break;
            case "choice":
                ShowChoices(line);
                break;
            case "end":
                Finish();
                break;
            default:
                Debug.LogWarning($"[Dialogue] 알 수 없는 line type '{line.type}'.");
                Finish();
                break;
        }
    }

    private void ShowLine(DialogueLineData line)
    {
        advanceButton.gameObject.SetActive(true);
        bool narration = string.IsNullOrEmpty(line.speaker);
        speakerNameText.text = narration ? string.Empty : GetCharacterName(line.speaker);
        if (speakerNameplate != null) speakerNameplate.SetActive(!narration);
        typewriter.Play(lineText, line.text, typewriterCharsPerSecond);
        UpdateSpeakerHighlight(line.speaker, line.emotion);
        UpdateSpeakerImage(line.speaker);
    }

    private static readonly Color SpeakerActive = Color.white;
    private static readonly Color SpeakerDimmed = new(0.45f, 0.47f, 0.5f, 1f);

    private void UpdateSpeakerImage(string speakerId)
    {
        if (speakerImage == null) return;

        Sprite art = null;
        if (CooperationManager.Instance != null && CooperationManager.Instance.TryGetCoopCharData(speakerId, out var data))
            art = data.sanctuaryFullArt != null ? data.sanctuaryFullArt
                : data.standingSprite != null ? data.standingSprite
                : data.charImage;

        if (art != null)
        {
            speakerImage.sprite = art;
            speakerImage.enabled = true;
            speakerImage.preserveAspect = true;
            speakerImage.color = SpeakerActive;
        }
        else if (speakerImage.sprite != null)
        {
            speakerImage.color = SpeakerDimmed; // 지문·다른 화자: 직전 캐릭터를 어둡게
        }
    }

    private void UpdateSpeakerHighlight(string speakerId, string emotion)
    {
        foreach (var slot in characterSlots)
        {
            if (!slot.gameObject.activeSelf) continue;

            bool isSpeaker = slot.CharacterId == speakerId;
            slot.SetHighlighted(isSpeaker);
            if (isSpeaker) slot.SetEmotion(emotion);
        }
    }

    private string GetCharacterName(string id)
    {
        if (_script.characters == null) return id;
        foreach (var c in _script.characters)
            if (c.id == id) return c.name;
        return id;
    }

    private void ShowChoices(DialogueLineData line)
    {
        advanceButton.gameObject.SetActive(false);

        if (line.options == null || line.options.Length == 0)
        {
            Debug.LogWarning("[Dialogue] choice 노드에 options가 없음. 대화를 종료함.");
            Finish();
            return;
        }

        foreach (var option in line.options)
        {
            var btn = Instantiate(choiceButtonPrefab, choiceContainer);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = option.text;
            var capturedOption = option;
            btn.onClick.AddListener(() => OnChoiceSelected(capturedOption));
            _choiceButtons.Add(btn);
        }
    }

    private void OnChoiceSelected(DialogueChoiceOption option)
    {
        HideChoices();

        if (option == null) return;

        if (option.effects != null)
        {
            var ctx = new CardContext();
            EffectRunner.ExecuteImmediate(option.effects, ctx);
        }

        // Dictionary.TryGetValue는 key가 null이면 ArgumentNullException을 던진다 (못 찾는 것과 다름).
        // JSON 작성 시 "next"를 빠뜨리면 option.next가 null로 들어와 GoToNode에서 바로 크래시 나므로 가드 필요.
        if (string.IsNullOrEmpty(option.next))
        {
            Debug.LogWarning("[Dialogue] 선택지의 'next' 노드 ID가 비어 있음.");
            Finish();
            return;
        }

        GoToNode(option.next);
    }

    private void HideChoices()
    {
        foreach (var b in _choiceButtons)
            Destroy(b.gameObject);
        _choiceButtons.Clear();
    }

    // 선택지가 표시 중일 때는 advanceButton이 비활성화돼 있으므로, 그 상태를 그대로
    // "지금 스페이스/엔터로 넘겨도 되는 시점인지" 가드로 재사용한다 (별도 플래그 불필요).
    // GameObject가 비활성(대화 안 하는 중)일 때는 Update 자체가 호출되지 않으므로 따로 체크 안 해도 됨.
    private void Update()
    {
        // interactable도 같이 체크 — 향후 타이프라이터 효과 등으로 버튼을 잠가도
        // 키보드 입력이 그걸 우회해서 넘어가버리는 비일관성을 막기 위함.
        if (!advanceButton.gameObject.activeSelf || !advanceButton.interactable) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current[Key.Space].wasPressedThisFrame || Keyboard.current[Key.Enter].wasPressedThisFrame)
            OnAdvanceClicked();
    }

    private void OnAdvanceClicked()
    {
        if (typewriter.CompleteImmediately()) return; // 타이핑 중이었으면 이번 클릭은 완성 처리로 소비

        _lineIndex++;
        ShowCurrentLine();
    }

    private void Finish()
    {
        gameObject.SetActive(false);
        _onComplete?.Invoke();
    }
}

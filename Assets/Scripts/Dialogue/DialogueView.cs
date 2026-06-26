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

    [Header("대사 UI")]
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI lineText;
    [SerializeField] private Button advanceButton;

    [Header("선택지 UI")]
    [SerializeField] private Transform choiceContainer;
    [SerializeField] private Button choiceButtonPrefab;

    private DialogueScriptData _script;
    private Dictionary<string, DialogueNodeData> _nodeMap;
    private DialogueNodeData _currentNode;
    private int _lineIndex;
    private Action _onComplete;
    private readonly List<Button> _choiceButtons = new();

    private void Awake()
    {
        // 주의: 이 오브젝트는 씬에서 비활성 상태로 시작해야 함.
        // 여기서 SetActive(false)를 부르면 Play()가 호출하는 최초 SetActive(true)를
        // 같은 프레임에서 취소해버려 첫 호출만 패널이 안 뜨는 버그가 생긴다 (EventView와 동일 이슈).
        advanceButton.onClick.AddListener(OnAdvanceClicked);
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
            Debug.LogWarning("[Dialogue] characterSlots가 비어있음. Inspector 연결 확인 필요.");
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
        speakerNameText.text = GetCharacterName(line.speaker);
        lineText.text = line.text;
        UpdateSpeakerHighlight(line.speaker, line.emotion);
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

        if (option.effects != null)
        {
            var ctx = new CardContext();
            foreach (var effect in option.effects)
                if (effect != null) effect.Execute(ctx);
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
        if (!advanceButton.gameObject.activeSelf) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current[Key.Space].wasPressedThisFrame || Keyboard.current[Key.Enter].wasPressedThisFrame)
            OnAdvanceClicked();
    }

    private void OnAdvanceClicked()
    {
        _lineIndex++;
        ShowCurrentLine();
    }

    private void Finish()
    {
        gameObject.SetActive(false);
        _onComplete?.Invoke();
    }
}

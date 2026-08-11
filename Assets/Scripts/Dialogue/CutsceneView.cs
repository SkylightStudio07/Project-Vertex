using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// 풀스크린 CG + 검정 페이드 전환으로 재생하는 컷씬 재생기.
// DialogueView와 같은 원칙 — TextAsset + 완료 콜백만으로 동작해서 호출부(PrologueDirector 등)에
// 의존하지 않는다. 분기가 없어 노드 구조 없이 beats를 순서대로 재생하고, 화살표/스페이스/엔터로
// 한 비트씩 진행한다(DialogueView.advanceButton과 동일한 관례).
public class CutsceneView : MonoBehaviour
{
    [System.Serializable]
    public class BackgroundEntry
    {
        public string key;
        public Sprite sprite;
    }

    [Header("화면")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image fadeOverlay; // 풀스크린 검정 이미지. 알파만 코루틴으로 조절.
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("자막")]
    [SerializeField] private TextMeshProUGUI captionText;
    [SerializeField] private float typewriterCharsPerSecond = 40f;

    [Header("진행")]
    [SerializeField] private Button advanceButton;

    [Header("배경 키 → 스프라이트")]
    [SerializeField] private List<BackgroundEntry> backgrounds;

    private CutsceneScriptData _script;
    private int _beatIndex;
    private Action _onComplete;
    private TypewriterPrinter typewriter;

    private void Awake()
    {
        // 주의: 이 오브젝트는 씬에서 비활성 상태로 시작해야 함(DialogueView/EventView와 동일 이슈 —
        // 여기서 SetActive(false)를 부르면 Play()의 최초 SetActive(true)를 같은 프레임에서 취소해버림).
        advanceButton.onClick.AddListener(OnAdvanceClicked);
        typewriter = new TypewriterPrinter(this);
        if (fadeOverlay != null) fadeOverlay.raycastTarget = false;
    }

    public void Play(TextAsset json, Action onComplete)
    {
        _onComplete = onComplete;

        if (json == null)
        {
            Debug.LogWarning("[Cutscene] 재생할 JSON이 없음. 즉시 완료 처리.");
            _onComplete?.Invoke();
            return;
        }

        _script = JsonUtility.FromJson<CutsceneScriptData>(json.text);

        if (_script == null || _script.beats == null || _script.beats.Length == 0)
        {
            Debug.LogWarning($"[Cutscene] '{json.name}' 파싱 실패 또는 beats 누락. 즉시 완료 처리.");
            _onComplete?.Invoke();
            return;
        }

        _beatIndex = 0;
        gameObject.SetActive(true);
        SetOverlayAlpha(1f); // 컷씬은 항상 암전에서 시작 — 첫 비트가 보통 fade:"in"으로 열림
        ShowCurrentBeat();
    }

    private void ShowCurrentBeat()
    {
        if (_script.beats == null || _beatIndex >= _script.beats.Length)
        {
            Finish();
            return;
        }
        StartCoroutine(PlayBeat(_script.beats[_beatIndex]));
    }

    private IEnumerator PlayBeat(CutsceneBeatData beat)
    {
        advanceButton.interactable = false; // 전환 중엔 못 넘기게

        if (!string.IsNullOrEmpty(beat.background))
        {
            Sprite sprite = FindBackground(beat.background);
            if (sprite != null) backgroundImage.sprite = sprite;
            else Debug.LogWarning($"[Cutscene] 배경 키 '{beat.background}'를 찾을 수 없음. backgrounds 등록 확인 필요.");
        }

        switch (beat.fade)
        {
            case "in":
                yield return FadeOverlay(1f, 0f);
                break;
            case "out":
                yield return FadeOverlay(0f, 1f);
                break;
            default:
                SetOverlayAlpha(0f); // cut: 페이드 없이 바로 노출
                break;
        }

        if (!string.IsNullOrEmpty(beat.caption))
        {
            captionText.gameObject.SetActive(true);
            typewriter.Play(captionText, beat.caption, typewriterCharsPerSecond);
        }
        else
        {
            captionText.gameObject.SetActive(false);
        }

        advanceButton.interactable = true;
    }

    private IEnumerator FadeOverlay(float from, float to)
    {
        SetOverlayAlpha(from);
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            SetOverlayAlpha(Mathf.Lerp(from, to, Mathf.Clamp01(t / fadeDuration)));
            yield return null;
        }
        SetOverlayAlpha(to);
    }

    private void SetOverlayAlpha(float a)
    {
        if (fadeOverlay == null) return;
        Color c = fadeOverlay.color;
        c.a = a;
        fadeOverlay.color = c;
    }

    private Sprite FindBackground(string key)
    {
        if (backgrounds == null) return null;
        foreach (var entry in backgrounds)
            if (entry != null && entry.key == key) return entry.sprite;
        return null;
    }

    // DialogueView.Update()와 동일한 관례 — advanceButton 비활성/비상호작용 중엔 키보드 입력 무시.
    private void Update()
    {
        if (!advanceButton.gameObject.activeSelf || !advanceButton.interactable) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current[Key.Space].wasPressedThisFrame || Keyboard.current[Key.Enter].wasPressedThisFrame)
            OnAdvanceClicked();
    }

    private void OnAdvanceClicked()
    {
        if (typewriter.CompleteImmediately()) return; // 타이핑 중이었으면 이번 클릭은 완성 처리로 소비

        _beatIndex++;
        ShowCurrentBeat();
    }

    private void Finish()
    {
        gameObject.SetActive(false);
        _onComplete?.Invoke();
    }
}

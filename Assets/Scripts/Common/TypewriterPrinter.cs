using System.Collections;
using TMPro;
using UnityEngine;

// TMP_Text 하나에 문자 단위로 점진적으로 텍스트를 채워 넣는 재생기.
// DialogueView/EventView 공용 — 둘 다 "한 줄/페이지씩 진행 + 타이핑 중 클릭하면 즉시 완성" 패턴이
// 똑같아서 공용으로 뺐다. MonoBehaviour가 아님 — 코루틴을 돌릴 숙주(host)만 생성자로 받는다.
public class TypewriterPrinter
{
    public bool IsTyping { get; private set; }

    private readonly MonoBehaviour host;
    private TMP_Text target;
    private Coroutine running;

    public TypewriterPrinter(MonoBehaviour host)
    {
        this.host = host;
    }

    // 타이핑 시작. charsPerSecond가 0 이하면 연출 없이 즉시 전체 표시.
    public void Play(TMP_Text target, string fullText, float charsPerSecond)
    {
        if (running != null) host.StopCoroutine(running);
        this.target = target;
        target.text = fullText ?? string.Empty;

        if (charsPerSecond <= 0f)
        {
            target.maxVisibleCharacters = int.MaxValue;
            IsTyping = false;
            return;
        }

        running = host.StartCoroutine(TypeRoutine(charsPerSecond));
    }

    // 타이핑 도중이면 즉시 끝까지 채운다.
    // 진행 버튼 클릭 시 "타이핑 중이면 완성만 하고, 이미 다 됐으면 진짜로 다음으로" 분기에 사용 —
    // 반환값이 true면 이번 클릭은 완성 처리로 소비된 것이니 호출측은 다음 단계로 넘어가면 안 된다.
    public bool CompleteImmediately()
    {
        if (!IsTyping) return false;
        if (running != null) host.StopCoroutine(running);
        target.maxVisibleCharacters = int.MaxValue;
        IsTyping = false;
        running = null;
        return true;
    }

    private IEnumerator TypeRoutine(float charsPerSecond)
    {
        IsTyping = true;
        target.maxVisibleCharacters = 0;
        target.ForceMeshUpdate();
        int totalChars = target.textInfo.characterCount;
        float interval = 1f / charsPerSecond;
        float t = 0f;

        while (target.maxVisibleCharacters < totalChars)
        {
            t += Time.deltaTime;
            target.maxVisibleCharacters = Mathf.Min(Mathf.FloorToInt(t / interval), totalChars);
            yield return null;
        }

        IsTyping = false;
        running = null;
    }
}

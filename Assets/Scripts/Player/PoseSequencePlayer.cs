using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// AttackKeyframe[] 하나를 순서대로 재생하는 재생기. 대상 Image/RectTransform은
// 자기 GameObject에서 직접 가져온다(캐릭터 스탠딩 Image에 붙여서 사용).
// 데미지 적용을 기다리지 않는 "구경용" 연출 — BattleManager.OnCardPlayed 구독 쪽(PartyView)에서
// 호출만 하고 결과를 기다리지 않는다. 재생 중 다시 호출되면 이전 재생은 중단하고 새로 시작한다.
[RequireComponent(typeof(Image))]
public class PoseSequencePlayer : MonoBehaviour
{
    private Image image;
    private RectTransform rt;

    private Sprite restingSprite;
    private Vector2 restingAnchoredPosition;
    private Vector3 restingScale; // localScale은 Vector3 — Vector2로 다루면 대입 시 z가 0으로 날아간다
    private float restingRotation;
    private Vector2 restingSizeDelta;
    private bool hasResting; // restingXxx가 실제 대기 포즈를 담고 있는지(트랜지션 중간값이 아닌지)

    private Coroutine playing;

    private void Awake()
    {
        image = GetComponent<Image>();
        rt = GetComponent<RectTransform>();

        // 포즈마다 원본 스프라이트의 픽셀 비율이 다를 수 있어(예: 대기 포즈는 세로로 긴 초상,
        // 공격 포즈는 정사각형에 가까움) — preserveAspect 없이 고정 박스에 늘려 넣으면
        // 비율이 깨져서 찌그러들어 보인다. 포즈 전환 시 박스 자체를 원본 비율에 맞게
        // 다시 잡아주는 것과 세트로 써야 한다(PlaySequence 참고).
        image.preserveAspect = true;
    }

    public void Play(AttackKeyframe[] frames)
    {
        if (frames == null || frames.Length == 0) return;

        if (playing != null)
        {
            StopCoroutine(playing);
            // 이전 재생이 트랜지션 도중 끊겼을 수 있으므로, 마지막으로 기록된 진짜 대기 포즈로
            // 먼저 되돌린 뒤 새로 시작한다 — 안 그러면 아래에서 트랜지션 중간값을 대기 포즈로
            // 잘못 캡처해서, 반복 재생할수록 캐릭터 위치가 조금씩 밀리는 버그가 생긴다.
            RestoreResting();
        }

        playing = StartCoroutine(PlaySequence(frames));
    }

    private void RestoreResting()
    {
        if (!hasResting) return;
        image.sprite = restingSprite;
        rt.anchoredPosition = restingAnchoredPosition;
        rt.localScale = restingScale;
        rt.localEulerAngles = new Vector3(0, 0, restingRotation);
        rt.sizeDelta = restingSizeDelta;
    }

    private IEnumerator PlaySequence(AttackKeyframe[] frames)
    {
        // 대기 포즈 기록 — 시퀀스 끝나면 여기로 복귀.
        restingSprite = image.sprite;
        restingAnchoredPosition = rt.anchoredPosition;
        restingScale = rt.localScale;
        restingRotation = rt.localEulerAngles.z;
        restingSizeDelta = rt.sizeDelta;
        hasResting = true;

        foreach (var frame in frames)
        {
            if (frame == null) continue;

            Vector2 fromPos = rt.anchoredPosition;
            Vector3 fromScale = rt.localScale;
            float fromRot = rt.localEulerAngles.z;

            Vector2 toPos = restingAnchoredPosition + frame.positionOffset;
            Vector3 toScale = Vector3.Scale(restingScale, new Vector3(frame.scale.x, frame.scale.y, 1f));
            float toRot = restingRotation + frame.rotation;

            if (frame.pose != null)
            {
                image.sprite = frame.pose;

                // 대기 포즈 기준 높이는 고정하고, 폭만 이 포즈의 실제 픽셀 비율대로 다시 잡는다.
                // (세로 높이가 흔들리면 "캐릭터가 갑자기 작아진 것"처럼 보이기 쉬워서 높이를 기준으로 삼음)
                Rect r = frame.pose.rect;
                if (r.height > 0f)
                {
                    float aspect = r.width / r.height;
                    rt.sizeDelta = new Vector2(restingSizeDelta.y * aspect, restingSizeDelta.y);
                }
            }

            if (frame.moveDuration <= 0f)
            {
                rt.anchoredPosition = toPos;
                rt.localScale = toScale;
                rt.localEulerAngles = new Vector3(0, 0, toRot);
            }
            else
            {
                float t = 0f;
                while (t < frame.moveDuration)
                {
                    t += Time.deltaTime;
                    float p = Mathf.Clamp01(t / frame.moveDuration);
                    rt.anchoredPosition = Vector2.Lerp(fromPos, toPos, p);
                    rt.localScale = Vector3.Lerp(fromScale, toScale, p);
                    rt.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(fromRot, toRot, p));
                    yield return null;
                }
            }

            if (frame.holdDuration > 0f)
                yield return new WaitForSeconds(frame.holdDuration);
        }

        RestoreResting();
        playing = null;
    }
}

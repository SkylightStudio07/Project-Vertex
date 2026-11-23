using System.Collections;
using UnityEngine;

// 잠식된 시체 같은 대기: 몇 초마다 그림이 잠깐 멈췄다가(정지) 옆으로 툭 꺾이며 경련하고 천천히 제자리로 돌아온다.
// EnemyData.idleTwitch를 켠 적만 EnemyView가 붙인다. 공격 모션(PoseSequencePlayer) 중에는 쉬고,
// 위치 · 회전은 "더했다 빼는" 방식이라 피격 밀림 · 돌진 · 화면 흔들림과 섞여도 제자리로 돌아온다.
public class EnemyIdleTwitch : MonoBehaviour
{
    [SerializeField] private Vector2 interval = new(1.8f, 4.2f);   // 경련 사이 간격 (초)
    [SerializeField] private Vector2 freeze = new(0.12f, 0.3f);    // 경련 직전 그림이 멈추는 시간
    [SerializeField] private Vector2 angle = new(2.5f, 4.5f);      // 꺾이는 각도 (도)
    [SerializeField] private Vector2 shift = new(6f, 12f);         // 옆으로 튀는 거리 (px)
    [SerializeField] private float settle = 0.32f;                 // 제자리로 돌아오는 시간

    private RectTransform _rt;
    private UISpriteSheetAnimator _anim;
    private PoseSequencePlayer _pose;
    private Coroutine _loop;
    private float _appliedAngle;
    private Vector2 _appliedShift;

    private void OnEnable()
    {
        _rt = (RectTransform)transform;
        _anim = GetComponent<UISpriteSheetAnimator>();
        _pose = GetComponent<PoseSequencePlayer>();
        _loop = StartCoroutine(Loop());
    }

    private void OnDisable()
    {
        if (_loop != null) StopCoroutine(_loop);
        _loop = null;
        Apply(0f, Vector2.zero);
        if (_anim != null && !_anim.IsPlaying && _anim.enabled) _anim.Resume();
    }

    private IEnumerator Loop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(interval.x, interval.y));
            if (_pose == null) _pose = GetComponent<PoseSequencePlayer>();
            if (_pose != null && _pose.IsPlaying) continue; // 공격 중에는 쉬기

            // 1) 멈춤: 숨이 끊긴 것처럼 그림이 잠깐 정지
            bool paused = _anim != null && _anim.IsPlaying;
            if (paused) _anim.Pause();
            yield return new WaitForSeconds(Random.Range(freeze.x, freeze.y));
            if (paused && _anim != null) _anim.Resume();

            // 2) 경련: 한 프레임에 툭 꺾이고 옆으로 튄다 (가끔 반대로 한 번 더)
            float side = Random.value < 0.5f ? -1f : 1f;
            Apply(side * Random.Range(angle.x, angle.y), new Vector2(side * Random.Range(shift.x, shift.y), Random.Range(-3f, 3f)));
            yield return new WaitForSeconds(0.05f);
            if (Random.value < 0.4f)
            {
                Apply(-side * Random.Range(angle.x, angle.y) * 0.6f, new Vector2(-side * Random.Range(shift.x, shift.y) * 0.5f, 0f));
                yield return new WaitForSeconds(0.04f);
            }

            // 3) 천천히 제자리로
            float a0 = _appliedAngle; Vector2 s0 = _appliedShift;
            for (float t = 0f; t < settle; t += Time.deltaTime)
            {
                float k = 1f - Mathf.SmoothStep(0f, 1f, t / settle);
                Apply(a0 * k, s0 * k);
                yield return null;
            }
            Apply(0f, Vector2.zero);
        }
    }

    private void Apply(float deg, Vector2 offset)
    {
        if (_rt == null) return;
        _rt.localRotation = Quaternion.Euler(0f, 0f, _rt.localEulerAngles.z - _appliedAngle + deg);
        _rt.anchoredPosition += offset - _appliedShift;
        _appliedAngle = deg;
        _appliedShift = offset;
    }
}

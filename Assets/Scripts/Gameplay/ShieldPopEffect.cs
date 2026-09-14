using DG.Tweening;
using UnityEngine;

// 방어도 획득 시 캐릭터 위치에 스폰되는 방패 실루엣 연출.
// 스케일 0.6 → 1.2 → 1.0으로 튀어나왔다가(펀치感), 잠깐 유지 후 페이드아웃하며 자동 파괴된다.
// HitEffectSpawner가 월드 스페이스에 부모 없이 스폰하므로(Spawn 주석 참고),
// 이 컴포넌트가 자기 수명을 스스로 관리한다 — 파티클처럼 Stop Action으로 정리되는 게 아니라
// 이쪽은 SpriteRenderer라 코드로 직접 Destroy 해야 한다.
[RequireComponent(typeof(SpriteRenderer))]
public class ShieldPopEffect : MonoBehaviour
{
    [SerializeField] private float popDuration = 0.18f;   // 0.6 → 1.2배로 튀어나오는 시간
    [SerializeField] private float settleDuration = 0.1f;  // 1.2 → 1.0배로 안착하는 시간
    [SerializeField] private float holdDuration = 0.15f;   // 완전 불투명으로 유지되는 시간
    [SerializeField] private float fadeDuration = 0.35f;   // 페이드아웃 시간

    private SpriteRenderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        transform.localScale = Vector3.one * 0.6f;

        Color c = _renderer.color;
        c.a = 1f;
        _renderer.color = c;

        // 시퀀스: 튀어나옴 → 안착 → 유지 → 페이드아웃 → 파괴.
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(1.2f, popDuration).SetEase(Ease.OutBack));
        seq.Append(transform.DOScale(1.0f, settleDuration).SetEase(Ease.InOutSine));
        seq.AppendInterval(holdDuration);
        seq.Append(_renderer.DOFade(0f, fadeDuration));
        seq.OnComplete(() => Destroy(gameObject));
    }
}

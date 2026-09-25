using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// 피격 시 스프라이트가 마인크래프트처럼 잠깐 붉게 물들었다가 원래 색으로 돌아오는 연출.
// 적(EnemyView)과 파티원(PartyView) 스프라이트 Image에 필요할 때 자동으로 붙는다: HitFlash.Play(image).
// 원래 색은 처음 붙을 때 한 번 기억해 두고, 연타로 겹쳐 맞아도 그 색으로 돌아온다.
[RequireComponent(typeof(Graphic))]
public class HitFlash : MonoBehaviour
{
    public static readonly Color Tint = new(1f, 0.55f, 0.55f, 1f); // Image 색에 곱해지므로 붉은 필터처럼 보인다
    public const float Hold = 0.08f;
    public const float Fade = 0.25f;

    private Graphic _graphic;
    private Color _baseColor;

    private void Awake()
    {
        _graphic = GetComponent<Graphic>();
        _baseColor = _graphic.color;
    }

    public static void Play(Graphic target)
    {
        if (target == null || !target.isActiveAndEnabled) return;
        if (!target.TryGetComponent<HitFlash>(out var flash)) flash = target.gameObject.AddComponent<HitFlash>();
        flash.Flash();
    }

    private void Flash()
    {
        _graphic.DOKill();
        var tinted = _baseColor * Tint;
        tinted.a = _baseColor.a;
        _graphic.color = tinted;
        _graphic.DOColor(_baseColor, Fade).SetDelay(Hold).SetEase(Ease.OutQuad).SetLink(gameObject);
    }

    // 연출 도중 꺼지면 붉은 채 남지 않게
    private void OnDisable()
    {
        if (_graphic == null) return;
        _graphic.DOKill();
        _graphic.color = _baseColor;
    }
}

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
    public const float WhiteHold = 0.035f;
    public const float WhiteFade = 0.09f;
    private static readonly int WhiteId = Shader.PropertyToID("_HitFlash");

    private Graphic _graphic;
    private Color _baseColor;

    private void Awake()
    {
        _graphic = GetComponent<Graphic>();
        _baseColor = _graphic.color;
    }

    // whitePeak: 맞은 순간 희게 번쩍이는 세기. 그림에 UI/BattleUnit 재질이 있을 때만 (유닛마다 복사본이라 다른 유닛에 번지지 않는다)
    public static void Play(Graphic target, float whitePeak = 0f)
    {
        if (target == null || !target.isActiveAndEnabled) return;
        if (!target.TryGetComponent<HitFlash>(out var flash)) flash = target.gameObject.AddComponent<HitFlash>();
        flash.Flash();
        flash.White(whitePeak);
    }

    private void Flash()
    {
        _graphic.DOKill();
        var tinted = _baseColor * Tint;
        tinted.a = _baseColor.a;
        _graphic.color = tinted;
        _graphic.DOColor(_baseColor, Fade).SetDelay(Hold).SetEase(Ease.OutQuad).SetLink(gameObject);
    }

    private Tween _whiteTween;

    private void White(float peak)
    {
        if (peak <= 0f) return;
        var material = _graphic.material;
        if (material == null || !material.HasProperty(WhiteId)) return;
        _whiteTween?.Kill();
        material.SetFloat(WhiteId, peak);
        _whiteTween = DOTween.To(() => material.GetFloat(WhiteId), v => material.SetFloat(WhiteId, v), 0f, WhiteFade)
            .SetDelay(WhiteHold).SetUpdate(true).SetLink(gameObject);
    }

    // 연출 도중 꺼지면 붉은 채 남지 않게
    private void OnDisable()
    {
        if (_graphic == null) return;
        _graphic.DOKill();
        _graphic.color = _baseColor;
    }
}

using System.Collections.Generic;
using Coffee.UIEffects;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// UIEffect(Dissolve)를 이용한 디졸브 연출 공용 헬퍼.
// UIEffect는 붙은 Graphic 하나에만 먹고 자식에는 번지지 않는다. 그래서 여러 요소로 된 UI(카드, 적 등)는
// 이미지마다 UIEffect를 붙여 디졸브하고, 글자(TMP)는 모양이 깨져 보여서 알파 페이드로만 처리한다.
//
// noise(노이즈 텍스처)가 비어 있으면 모든 픽셀이 같은 임계값을 가져서 "번지며 사라지는" 대신
// 통째로 테두리 색으로 바뀌어 버린다. 그 경우엔 디졸브 대신 페이드로 대체한다.
public static class UIDissolve
{
    // 아트 디렉션상 시안은 제한적으로 — 얇은 테두리에만 쓴다.
    public static readonly Color DefaultEdge = new(0.55f, 0.85f, 0.95f, 1f);

    // graphic 하나를 디졸브 준비 상태로 만든다. 이미 UIEffect가 있으면 그 설정(텍스처·색)을 존중한다.
    public static UIEffect Prepare(Graphic graphic, Texture noise, Color edge,
                                   TransitionFilter filter = TransitionFilter.Dissolve)
    {
        if (graphic == null || noise == null) return null;

        if (!graphic.TryGetComponent<UIEffect>(out var fx))
        {
            fx = graphic.gameObject.AddComponent<UIEffect>();
            fx.transitionTexture = noise;
            fx.transitionWidth = 0.06f;
            fx.transitionSoftness = 0.5f;
            fx.transitionColorFilter = ColorFilter.Replace;
            fx.transitionColor = edge;
        }
        fx.transitionFilter = filter;
        return fx;
    }

    // graphic 하나를 디졸브로 나타나게(rate 1→0) 한다.
    public static Tween In(Graphic graphic, Texture noise, float duration, Color edge)
    {
        var fx = Prepare(graphic, noise, edge);
        if (fx == null) return FadeFrom0(graphic, duration);

        fx.transitionRate = 1f;
        return DOTween.To(() => fx.transitionRate, r => fx.transitionRate = r, 0f, duration)
                      .SetEase(Ease.OutQuad).SetLink(graphic.gameObject);
    }

    // root 아래 보이는 요소 전부를 디졸브로 사라지게(rate 0→1) 한다. exclude에 든 Graphic은 건드리지 않는다.
    // filter: 기본 Dissolve. 카드 제거처럼 불타 없어지는 연출은 Burn.
    public static Sequence Out(GameObject root, Texture noise, float duration, Color edge,
                               ICollection<Graphic> exclude = null, TransitionFilter filter = TransitionFilter.Dissolve)
    {
        var seq = DOTween.Sequence().SetLink(root);

        foreach (var g in root.GetComponentsInChildren<Graphic>())
        {
            if (!g.enabled || (exclude != null && exclude.Contains(g))) continue;
            g.raycastTarget = false; // 사라지는 중인 UI가 클릭·타게팅을 가로채지 않도록

            // 글자, 또는 노이즈가 없을 때는 페이드로 처리
            var fx = g is TMP_Text ? null : Prepare(g, noise, edge, filter);
            if (fx == null)
            {
                seq.Join(FadeOut(g, duration * 0.6f));
                continue;
            }

            fx.transitionRate = 0f;
            seq.Join(DOTween.To(() => fx.transitionRate, r => fx.transitionRate = r, 1f, duration)
                            .SetEase(Ease.InQuad));
        }
        return seq;
    }

    // Graphic.color 알파가 아니라 CanvasRenderer 알파를 내린다. 커스텀 Graphic(BlessingPanelGraphic 등)은
    // 메시 정점색을 자체 계산해 color.a를 무시하는 경우가 있는데, CanvasRenderer 알파는 어떤 Graphic에도 곱해진다.
    private static Tween FadeOut(Graphic g, float duration)
    {
        var cr = g.canvasRenderer;
        return DOTween.To(() => cr.GetAlpha(), a => cr.SetAlpha(a), 0f, duration);
    }

    private static Tween FadeFrom0(Graphic graphic, float duration)
    {
        if (graphic == null) return null;
        float target = graphic.color.a;
        var c = graphic.color;
        graphic.color = new Color(c.r, c.g, c.b, 0f);
        return graphic.DOFade(target, duration).SetLink(graphic.gameObject);
    }
}

using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// 땅이 파이는 충격 연출 (적 공격 프레임에 맞춰 터뜨린다):
//   구덩이·균열 자국(남는다) · 먼지 덩이(8프레임으로 피어오른다) · 튀는 돌 조각
// 그림은 Resources/GroundImpact (Crater_A · Dust_Puff_00~07 · Debris_01~06). 없으면 코드가 만든 임시 도형을 쓴다.
// 쓰는 곳: EnemyView.PlayLungeMotion → EnemyData.groundFxShots (총구별 프레임 · 탄착점).
public static class GroundImpactFx
{
    private static Sprite _soft, _crack, _chip;
    private static Sprite[] _craters, _debris, _dust;
    private static bool _loaded;

    private static Sprite[] LoadSet(params string[] names)
    {
        var list = new System.Collections.Generic.List<Sprite>();
        foreach (var n in names)
        {
            var sp = Resources.Load<Sprite>("GroundImpact/" + n);
            if (sp != null) list.Add(sp);
        }
        return list.ToArray();
    }

    private static void EnsureArt()
    {
        if (_loaded) return;
        _loaded = true;
        _craters = LoadSet("Crater_A"); // 구덩이는 한 종류만
        _debris = LoadSet("Debris_01", "Debris_02", "Debris_03", "Debris_04", "Debris_05", "Debris_06");
        _dust = LoadSet("Dust_Puff_00", "Dust_Puff_01", "Dust_Puff_02", "Dust_Puff_03", "Dust_Puff_04", "Dust_Puff_05", "Dust_Puff_06", "Dust_Puff_07");
    }

    // ── 코드로 만드는 스프라이트 ──
    private static Sprite Make(int w, int h, System.Func<float, float, float> alphaAt)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
        var px = new Color32[w * h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float a = Mathf.Clamp01(alphaAt((x + 0.5f) / w, (y + 0.5f) / h));
                px[y * w + x] = new Color32(255, 255, 255, (byte)(a * 255f));
            }
        tex.SetPixels32(px); tex.Apply(false, true);
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
    }

    private static Sprite Soft => _soft ??= Make(64, 64, (u, v) =>
    {
        float d = Mathf.Sqrt((u - 0.5f) * (u - 0.5f) + (v - 0.5f) * (v - 0.5f)) * 2f;
        return Mathf.SmoothStep(1f, 0f, d);
    });

    private static Sprite Chip => _chip ??= Make(16, 16, (u, v) => (v < 1f - Mathf.Abs(u - 0.5f) * 1.6f) ? 1f : 0f);

    // 가운데에서 갈라져 나가는 균열 + 어두운 구덩이
    private static Sprite Crack => _crack ??= BuildCrack();
    private static Sprite BuildCrack()
    {
        const int w = 256, h = 128;
        var rnd = new System.Random(7);
        var a = new float[w * h];
        // 구덩이: 납작한 타원, 가장자리는 부드럽게
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float dx = (x + 0.5f) / w * 2f - 1f, dy = ((y + 0.5f) / h * 2f - 1f) * 1.7f;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                a[y * w + x] = Mathf.SmoothStep(0.55f, 0f, d) * 0.9f;
            }
        // 균열: 중심에서 바깥으로 꺾이며 뻗는 선
        for (int i = 0; i < 11; i++)
        {
            float ang = (i / 11f) * Mathf.PI * 2f + (float)rnd.NextDouble() * 0.4f;
            float px = w * 0.5f, py = h * 0.5f, len = 38f + (float)rnd.NextDouble() * 70f;
            for (float s = 0; s < len; s += 1f)
            {
                ang += ((float)rnd.NextDouble() - 0.5f) * 0.35f;
                px += Mathf.Cos(ang); py += Mathf.Sin(ang) * 0.55f; // 세로는 납작하게 (원근)
                float fade = 1f - s / len;
                for (int oy = -1; oy <= 1; oy++)
                    for (int ox = -1; ox <= 1; ox++)
                    {
                        int xx = Mathf.RoundToInt(px) + ox, yy = Mathf.RoundToInt(py) + oy;
                        if (xx < 0 || yy < 0 || xx >= w || yy >= h) continue;
                        float k = (ox == 0 && oy == 0 ? 1f : 0.45f) * (0.35f + 0.65f * fade);
                        a[yy * w + xx] = Mathf.Max(a[yy * w + xx], k);
                    }
            }
        }
        return Make(w, h, (u, v) => a[Mathf.Clamp((int)(v * h), 0, h - 1) * w + Mathf.Clamp((int)(u * w), 0, w - 1)]);
    }

    private static Image Spawn(Transform parent, Sprite sprite, Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject("GroundFx", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        var img = go.GetComponent<Image>();
        img.sprite = sprite; img.color = color; img.raycastTarget = false;
        return img;
    }

    // worldPos: 땅이 파이는 자리(월드). parent: 이펙트를 담을 RectTransform(보통 적 뷰). scale: 크기 배율. dust: 먼지 색
    public static void Play(RectTransform parent, Vector3 worldPos, float scale, Color dust)
    {
        if (parent == null) return;
        Vector2 p = parent.InverseTransformPoint(worldPos);
        // 한 번의 충격이 하나의 묶음 — 끝나면 통째로 지운다
        var holder = new GameObject("GroundFxHolder", typeof(RectTransform));
        var hrt = (RectTransform)holder.transform;
        hrt.SetParent(parent, false);
        hrt.anchorMin = hrt.anchorMax = hrt.pivot = new Vector2(0.5f, 0.5f);
        hrt.anchoredPosition = Vector2.zero; hrt.sizeDelta = Vector2.zero;
        hrt.SetAsLastSibling();
        UnityEngine.Object.Destroy(holder, 2.6f);

        float s = Mathf.Max(0.2f, scale);
        var seq = DOTween.Sequence().SetLink(holder);

        // 1) 구덩이 · 균열 (오래 남는다)
        EnsureArt();
        bool art = _craters.Length > 0;
        // 그림은 1920×1080 화면 기준 크기(380×190) 그대로, 임시 도형은 절반 크기
        var crater = art
            ? Spawn(holder.transform, _craters[Random.Range(0, _craters.Length)], p, new Vector2(380f, 190f) * s, new Color(1f, 1f, 1f, 0f))
            : Spawn(holder.transform, Crack, p, new Vector2(190f, 95f) * s, new Color(0.05f, 0.05f, 0.07f, 0f));
        crater.rectTransform.localScale = new Vector3(0.55f, 0.55f, 1f);
        seq.Join(crater.rectTransform.DOScale(1f, 0.14f).SetEase(Ease.OutCubic));
        seq.Join(crater.DOFade(art ? 1f : 0.85f, 0.05f));
        seq.Insert(0.7f, crater.DOFade(0f, 0.6f));

        // 3) 먼지 덩이
        for (int i = 0; i < 3; i++)
        {
            float dir = Random.Range(-1f, 1f);
            bool dustArt = _dust.Length > 0;
            var puff = Spawn(holder.transform, dustArt ? _dust[0] : Soft, p + new Vector2(dir * 26f * s, Random.Range(-4f, 6f)),
                (dustArt ? new Vector2(110f, 110f) : new Vector2(36f, 36f)) * s, new Color(dust.r, dust.g, dust.b, 0f));
            float end = (dustArt ? Random.Range(150f, 220f) : Random.Range(110f, 170f)) * s, dur = Random.Range(0.7f, 1.05f), delay = Random.Range(0f, 0.08f);
            seq.Insert(delay, puff.DOFade(dust.a * 0.75f, 0.08f));
            if (dustArt)
            {
                var frames = _dust; var target = puff;
                seq.Insert(delay, DOVirtual.Float(0f, frames.Length - 0.01f, dur, v => { if (target != null) target.sprite = frames[Mathf.Clamp((int)v, 0, frames.Length - 1)]; }).SetEase(Ease.Linear));
                if (Random.value < 0.5f) puff.rectTransform.localScale = new Vector3(-1f, 1f, 1f);
            }
            seq.Insert(delay, puff.rectTransform.DOSizeDelta(dustArt ? new Vector2(end, end) : new Vector2(end, end * 0.8f), dur).SetEase(Ease.OutQuad));
            seq.Insert(delay, puff.rectTransform.DOAnchorPos(puff.rectTransform.anchoredPosition + new Vector2(dir * 70f * s, Random.Range(34f, 90f) * s), dur).SetEase(Ease.OutQuad));
            seq.Insert(delay + 0.08f, puff.DOFade(0f, dur - 0.08f).SetEase(Ease.InQuad));
        }

        // 4) 튀는 돌 조각
        for (int i = 0; i < 3; i++)
        {
            bool chipArt = _debris.Length > 0;
            var chipSprite = chipArt ? _debris[Random.Range(0, _debris.Length)] : Chip;
            float sz = chipArt ? chipSprite.rect.width * Random.Range(0.55f, 0.9f) * s : Random.Range(7f, 15f) * s;
            var chip = Spawn(holder.transform, chipSprite, p + new Vector2(Random.Range(-14f, 14f) * s, 0f), new Vector2(sz, sz),
                chipArt ? Color.white : new Color(0.2f, 0.18f, 0.17f, 1f));
            float dx = Random.Range(-150f, 150f) * s, up = Random.Range(70f, 190f) * s, dur = Random.Range(0.45f, 0.75f);
            Vector2 a0 = chip.rectTransform.anchoredPosition;
            seq.Insert(0f, chip.rectTransform.DOAnchorPosX(a0.x + dx, dur).SetEase(Ease.Linear));
            seq.Insert(0f, chip.rectTransform.DOAnchorPosY(a0.y + up, dur * 0.45f).SetEase(Ease.OutQuad));
            seq.Insert(dur * 0.45f, chip.rectTransform.DOAnchorPosY(a0.y - 6f, dur * 0.55f).SetEase(Ease.InQuad));
            seq.Insert(0f, chip.rectTransform.DORotate(new Vector3(0, 0, Random.Range(-540f, 540f)), dur, RotateMode.FastBeyond360).SetEase(Ease.Linear));
            seq.Insert(dur * 0.7f, chip.DOFade(0f, dur * 0.3f + 0.15f));
        }
    }
}

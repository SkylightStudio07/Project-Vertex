using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// BattleImpactFx 중 적 행동 · 상태 연출.
//   - 상태 부여: 상태가 붙거나 쌓이면 대상 위에 아이콘과 "취약 +2"가 튄다 (디버프 붉은 보라 · 버프 금색 · 그 밖 회색)
//   - 회복: 플레이어 위에 초록 "+N"
//   - 적 사격 (EnemyData.attackFx = Gunshot): 총구 섬광 + 플레이어 쪽으로 그어지는 탄도선
//   - 곡사 포격 (Artillery): 하늘에서 포탄 꼬리가 떨어지고, 플레이어 발밑에서 화구 · 폭연이 솟는다 (예전 targetBlastFrames 대신)
//   - 적 버프 행동: 적 몸 뒤로 붉은 기운이 피어오른다
//   - 적 디버프 행동: 저주 구체가 플레이어에게 날아가 보라 안개로 터진다
// 그림: Resources/BattleFx/EnemyAction/ (원본 ArtDirection/BattleFxMockup/EnemyActionFx). 그림이 없으면 그 연출만 건너뛴다.
public partial class BattleImpactFx
{
    [Header("상태 부여 · 회복 표시")]
    [SerializeField] private Color debuffPopColor = new(0.86f, 0.30f, 0.46f, 1f);
    [SerializeField] private Color buffPopColor = new(0.96f, 0.72f, 0.26f, 1f);
    [SerializeField] private Color neutralPopColor = new(0.62f, 0.66f, 0.72f, 1f);
    [SerializeField] private Color healColor = new(0.36f, 0.86f, 0.52f, 1f);
    [SerializeField] private float statusPopSize = 30f;

    [Header("적 행동 이펙트 (Resources/BattleFx/EnemyAction)")]
    [SerializeField] private float tracerThickness = 26f;
    [SerializeField] private float tracerDuration = 0.09f;
    [SerializeField] private float enemyFlashSize = 230f;
    [SerializeField] private float shellFallTime = 0.32f;
    [SerializeField] private float shellRiseTime = 0.34f;
    [Tooltip("발사된 포탄이 솟는 방향 (포신 방향)")]
    [SerializeField] private Vector2 launchDirection = new(-0.55f, 1f);
    [Tooltip("떨어지는 포탄의 기울기 (높이 1당 가로). 양수면 오른쪽 위에서 온다")]
    [SerializeField] private float fallSlant = 0.28f;
    [SerializeField] private float shellLength = 420f;
    [SerializeField] private float blastHeight = 560f;
    [Tooltip("버프 행동: 적 실루엣이 번지는 색 (알파 = 최대 밝기)")]
    [SerializeField] private Color buffGlowColor = new(1f, 0.74f, 0.32f, 0.55f);
    [SerializeField] private float buffGhostScale = 1.1f;
    [SerializeField] private float curseFlightTime = 0.34f;
    [SerializeField] private float curseBurstSize = 360f;

    private const string EnemyActionPath = "BattleFx/EnemyAction/";
    private readonly Dictionary<string, Sprite[]> _actionSprites = new();
    private readonly Dictionary<object, (float time, int count)> _popStacks = new();

    // ───────── 구독 ─────────

    private void WatchEnemyExtras(EnemyInstance enemy)
    {
        var e = enemy;
        if (e.Statuses != null) e.Statuses.OnStatusAdded += (status, delta) => HandleStatusAdded(e, status, delta);
    }

    private void WatchPlayerExtras(PlayerCombatant player)
    {
        if (player.Statuses != null) player.Statuses.OnStatusAdded += HandlePlayerStatusAdded;
        player.OnHealed += HandlePlayerHealed;
    }

    private void UnwatchPlayerExtras(PlayerCombatant player)
    {
        if (player.Statuses != null) player.Statuses.OnStatusAdded -= HandlePlayerStatusAdded;
        player.OnHealed -= HandlePlayerHealed;
    }

    private void HandlePlayerStatusAdded(StatusInstance status, int delta) => HandleStatusAdded(null, status, delta);

    // ───────── 상태 부여 · 회복 ─────────

    private void HandleStatusAdded(EnemyInstance enemy, StatusInstance status, int delta)
    {
        if (!isActiveAndEnabled || status?.Definition == null || delta == 0) return;
        if (_battle == null || _battle.State == null) return; // 전투 밖(미리보기 등)
        Vector2 at;
        object key;
        if (enemy != null)
        {
            var view = FindView(enemy);
            if (view == null) return;
            at = PointInRect(view.SpriteRect, new Vector2(0.5f, 0.5f)); // 몸 가운데 (머리 위는 행동 이름 · 의도 아이콘 자리)
            key = enemy;
        }
        else
        {
            at = PlayerPoint() + new Vector2(0f, 110f); // 피해 · 회복 숫자 위
            key = this;
        }

        var def = status.Definition;
        Color color = def.GetDisposition(status.Stacks) switch
        {
            StatusDisposition.Debuff => debuffPopColor,
            StatusDisposition.Buff => buffPopColor,
            _ => neutralPopColor,
        };
        string text = $"{def.DisplayName} {(delta > 0 ? "+" : "−")}{Mathf.Abs(delta)}";
        SpawnStatusPop(key, at, def.Icon, text, color);
    }

    private void HandlePlayerHealed(int amount)
    {
        if (amount <= 0 || !isActiveAndEnabled) return;
        SpawnNumber(PlayerPoint(), "+" + amount, numberSize, healColor);
    }

    // 아이콘 + 글자 한 줄. 같은 대상에 연달아 붙으면 위로 쌓는다
    private void SpawnStatusPop(object key, Vector2 at, Sprite icon, string text, Color color)
    {
        int n = _popStacks.TryGetValue(key, out var prev) && Time.unscaledTime - prev.time < 0.6f ? prev.count + 1 : 0;
        _popStacks[key] = (Time.unscaledTime, n);
        at += new Vector2(0f, n * (statusPopSize + 12f));

        var go = new GameObject("StatusPop", typeof(RectTransform));
        go.layer = gameObject.layer;
        var rt = (RectTransform)go.transform;
        rt.SetParent(_root, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(420f, statusPopSize + 16f);
        rt.anchoredPosition = at;
        var group = go.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;

        var label = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer)).AddComponent<TextMeshProUGUI>();
        label.gameObject.layer = gameObject.layer;
        label.rectTransform.SetParent(rt, false);
        label.rectTransform.anchorMin = label.rectTransform.anchorMax = label.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        if (font != null) label.font = font;
        label.text = text;
        label.fontSize = statusPopSize;
        label.fontStyle = FontStyles.Bold;
        label.color = color;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.outlineWidth = 0.18f;
        label.outlineColor = new Color32(14, 16, 20, 230);
        float textW = label.preferredWidth;
        label.rectTransform.sizeDelta = new Vector2(textW + 8f, statusPopSize + 10f);

        if (icon != null)
        {
            float side = statusPopSize * 1.35f;
            var img = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer)).AddComponent<Image>();
            img.gameObject.layer = gameObject.layer;
            img.rectTransform.SetParent(rt, false);
            img.rectTransform.anchorMin = img.rectTransform.anchorMax = img.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            img.rectTransform.sizeDelta = Vector2.one * side;
            img.sprite = icon;
            img.preserveAspect = true;
            img.raycastTarget = false;
            float total = side + 8f + textW;
            img.rectTransform.anchoredPosition = new Vector2(-total * 0.5f + side * 0.5f, 0f);
            label.rectTransform.anchoredPosition = new Vector2(-total * 0.5f + side + 8f + textW * 0.5f, 0f);
            img.rectTransform.localScale = Vector3.one * 1.8f;
            img.rectTransform.DOScale(1f, 0.22f).SetEase(Ease.OutBack).SetUpdate(true).SetLink(go);
        }

        rt.localScale = new Vector3(1.25f, 1.25f, 1f);
        group.alpha = 0f;
        DOTween.Sequence().SetUpdate(true).SetLink(go)
            .Append(group.DOFade(1f, 0.1f))
            .Join(rt.DOScale(1f, 0.2f).SetEase(Ease.OutBack))
            .Append(rt.DOAnchorPosY(at.y + 46f, 0.8f).SetEase(Ease.OutCubic))
            .Insert(0.75f, group.DOFade(0f, 0.35f))
            .OnComplete(() => Destroy(go));
    }

    // ───────── 적 행동 ─────────

    private void HandleEnemyActionStarting(EnemyInstance enemy, EnemyAction action, float landingIn)
    {
        if (!isActiveAndEnabled || enemy == null || action == null) return;
        var view = FindView(enemy);
        if (view == null) return;
        var data = enemy.Data;

        switch (action.intentType)
        {
            case IntentType.Attack:
                if (data == null) break;
                if (data.attackFx == EnemyAttackFx.Gunshot)
                {
                    // 시트의 발사 프레임마다 한 발씩 (점사). 지정이 없으면 피해가 들어가는 순간 한 발
                    var frames = data.muzzleFireFrames;
                    if (frames != null && frames.Length > 0 && data.attackFrameRate > 0f)
                        foreach (int f in frames)
                            DOVirtual.DelayedCall(f / data.attackFrameRate, () => { if (this != null && view != null) FireGunshot(view, data); }, false).SetLink(gameObject);
                    else
                        DOVirtual.DelayedCall(Mathf.Max(0f, landingIn - 0.02f), () => { if (this != null && view != null) FireGunshot(view, data); }, false).SetLink(gameObject);
                }
                else if (data.attackFx == EnemyAttackFx.Artillery && DrawsArtillery(enemy))
                {
                    // 포구에서 솟아 화면 위로 나갔다가 (발사 프레임), 착탄 직전에 플레이어 머리 위에서 내리꽂힌다
                    if (data.muzzleFireFrames != null && data.muzzleFireFrames.Length > 0 && data.attackFrameRate > 0f)
                    {
                        float fireAt = data.muzzleFireFrames[0] / data.attackFrameRate;
                        DOVirtual.DelayedCall(fireAt, () => { if (this != null && view != null) LaunchShell(view, data); }, false).SetLink(gameObject);
                    }
                    DOVirtual.DelayedCall(Mathf.Max(0f, landingIn - shellFallTime), () => { if (this != null) FireArtillery(); }, false).SetLink(gameObject);
                }
                break;
            case IntentType.Buff:
                PlayBuffAura(view);
                break;
            case IntentType.Debuff:
                FireCurse(view, Mathf.Max(0.2f, landingIn));
                break;
        }
    }

    private bool DrawsArtillery(EnemyInstance enemy)
        => enemy?.Data != null && enemy.Data.attackFx == EnemyAttackFx.Artillery && ActionSprites("Blast").Length > 0;

    // 사격: 총구 섬광 + 탄도선 (적은 왼쪽을 보고 쏜다)
    private void FireGunshot(EnemyView view, EnemyData data)
    {
        Vector2 muzzle = PointInRect(view.SpriteRect, data.muzzleAnchor);
        Vector2 target = PlayerPoint() + new Vector2(Random.Range(-20f, 20f), Random.Range(-70f, -20f));

        var flashes = ActionSprites("Flash");
        if (flashes.Length > 0 && !data.sheetDrawsMuzzleFlash)
        {
            var flash = MakeImage("EnemyMuzzleFlash", flashes[Random.Range(0, flashes.Length)], true);
            var rt = flash.rectTransform;
            rt.pivot = SpritePivot(flash.sprite, new Vector2(24f / 512f, 0.5f)); // 총구 원점
            rt.sizeDelta = Vector2.one * enemyFlashSize;
            rt.anchoredPosition = muzzle;
            Vector2 dir = target - muzzle;
            rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            rt.localScale = new Vector3(0.8f, Random.value < 0.5f ? 0.8f : -0.8f, 1f);
            DOTween.Sequence().SetUpdate(true).SetLink(flash.gameObject)
                .Append(rt.DOScale(new Vector3(1.1f, 1.1f * Mathf.Sign(rt.localScale.y), 1f), 0.1f))
                .Join(flash.DOFade(0f, 0.08f).SetDelay(0.03f))
                .OnComplete(() => Destroy(flash.gameObject));
        }

        var tracers = ActionSprites("Tracer");
        if (tracers.Length > 0)
        {
            var tracer = MakeImage("EnemyTracer", tracers[Random.Range(0, tracers.Length)], true);
            var rt = tracer.rectTransform;
            rt.pivot = new Vector2(0f, SpritePivot(tracer.sprite, new Vector2(0f, 0.5f)).y); // 꼬리를 총구에, 머리가 플레이어에 닿게 늘인다
            Vector2 dir = target - muzzle;
            float len = dir.magnitude;
            rt.sizeDelta = new Vector2(len, tracerThickness);
            rt.anchoredPosition = muzzle;
            rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            rt.localScale = new Vector3(0.15f, 1f, 1f);
            DOTween.Sequence().SetUpdate(true).SetLink(tracer.gameObject)
                .Append(rt.DOScaleX(1f, tracerDuration * 0.45f).SetEase(Ease.OutQuad))
                .Append(tracer.DOFade(0f, tracerDuration))
                .Join(rt.DOScaleY(0.4f, tracerDuration))
                .OnComplete(() => Destroy(tracer.gameObject));
        }
    }

    // 발사: 포구에서 포신 방향(왼쪽 위)으로 솟아 화면 위 밖으로 사라진다
    private void LaunchShell(EnemyView view, EnemyData data)
    {
        var shells = ActionSprites("Shell");
        if (shells.Length == 0) return;
        Vector2 muzzle = PointInRect(view.SpriteRect, data.muzzleAnchor);
        float top = _root.rect.yMax + shellLength * 0.6f;
        Vector2 dir = launchDirection.normalized;
        Vector2 exit = muzzle + dir * ((top - muzzle.y) / Mathf.Max(0.2f, dir.y));
        var shell = MakeImage("ArtilleryShellUp", shells[Random.Range(0, shells.Length)], true);
        var rt = shell.rectTransform;
        rt.pivot = SpritePivot(shell.sprite, new Vector2(1f, 0.5f));
        rt.sizeDelta = new Vector2(shellLength, shellLength / SpriteAspect(shell.sprite, 6f));
        rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        rt.anchoredPosition = muzzle;
        rt.localScale = new Vector3(0.3f, 0.6f, 1f); // 포구에서 꼬리가 자라며 나간다
        DOTween.Sequence().SetUpdate(true).SetLink(shell.gameObject)
            .Append(rt.DOAnchorPos(exit, shellRiseTime).SetEase(Ease.InQuad))
            .Join(rt.DOScale(new Vector3(0.85f, 0.85f, 1f), shellRiseTime * 0.6f))
            .OnComplete(() => Destroy(shell.gameObject));
    }

    // 곡사 포격: 플레이어 머리 위 화면 밖에서 포탄이 내리꽂혀 발밑에 닿는 순간 화구 · 폭연 · 땅 충격
    private void FireArtillery()
    {
        if (!TryPlayerFeet(out Vector3 feetWorld, out Vector2 feet)) return;
        var shells = ActionSprites("Shell");
        float top = _root.rect.yMax + shellLength * 0.6f;
        Vector2 from = new Vector2(feet.x + (top - feet.y) * fallSlant, top);
        if (shells.Length > 0)
        {
            var shell = MakeImage("ArtilleryShell", shells[Random.Range(0, shells.Length)], true);
            var rt = shell.rectTransform;
            rt.pivot = SpritePivot(shell.sprite, new Vector2(1f, 0.5f)); // 포탄 머리
            float aspect = SpriteAspect(shell.sprite, 6f);
            rt.sizeDelta = new Vector2(shellLength, shellLength / aspect);
            Vector2 dir = feet - from;
            rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            rt.anchoredPosition = from;
            DOTween.Sequence().SetUpdate(true).SetLink(shell.gameObject)
                .Append(rt.DOAnchorPos(feet, shellFallTime).SetEase(Ease.InQuad))
                .Append(shell.DOFade(0f, 0.05f))
                .OnComplete(() => Destroy(shell.gameObject));
        }
        DOVirtual.DelayedCall(shellFallTime, () => { if (this != null) Explode(feetWorld, feet); }, true).SetLink(gameObject);
    }

    private void Explode(Vector3 feetWorld, Vector2 feet)
    {
        GroundImpactFx.Play(_root, feetWorld, 1.5f, GroundImpactFx.StageDustColor());

        var smokes = ActionSprites("Smoke");
        if (smokes.Length > 0)
        {
            var smoke = MakeImage("ArtillerySmoke", smokes[Random.Range(0, smokes.Length)], false);
            var rt = smoke.rectTransform;
            rt.pivot = SpritePivot(smoke.sprite, new Vector2(0.5f, 0.1f)); // 연기 밑동
            float aspect = SpriteAspect(smoke.sprite, 1f);
            rt.sizeDelta = new Vector2(blastHeight * 0.9f * aspect, blastHeight * 0.9f);
            rt.anchoredPosition = feet;
            rt.localScale = new Vector3(Random.value < 0.5f ? -0.6f : 0.6f, 0.6f, 1f);
            var c = smoke.color; c.a = 0f; smoke.color = c;
            float sx = Mathf.Sign(rt.localScale.x);
            DOTween.Sequence().SetUpdate(true).SetLink(smoke.gameObject)
                .Insert(0.08f, smoke.DOFade(0.92f, 0.18f))
                .Insert(0.08f, rt.DOScale(new Vector3(1.15f * sx, 1.15f, 1f), 1.4f).SetEase(Ease.OutCubic))
                .Insert(0.08f, rt.DOAnchorPosY(feet.y + 90f, 1.4f).SetEase(Ease.OutCubic))
                .Insert(0.7f, smoke.DOFade(0f, 0.8f).SetEase(Ease.InQuad))
                .OnComplete(() => Destroy(smoke.gameObject));
        }

        var blasts = ActionSprites("Blast");
        if (blasts.Length > 0)
        {
            var blast = MakeImage("ArtilleryBlast", blasts[Random.Range(0, blasts.Length)], true);
            var rt = blast.rectTransform;
            rt.pivot = SpritePivot(blast.sprite, new Vector2(0.5f, 0.05f)); // 불기둥이 땅에 닿는 점
            float aspect = SpriteAspect(blast.sprite, 0.75f);
            rt.sizeDelta = new Vector2(blastHeight * aspect, blastHeight);
            rt.anchoredPosition = feet;
            float sx = Random.value < 0.5f ? -1f : 1f;
            rt.localScale = new Vector3(0.45f * sx, 0.35f, 1f);
            DOTween.Sequence().SetUpdate(true).SetLink(blast.gameObject)
                .Append(rt.DOScale(new Vector3(1f * sx, 1f, 1f), 0.12f).SetEase(Ease.OutCubic))
                .Append(rt.DOScale(new Vector3(1.12f * sx, 1.18f, 1f), 0.45f).SetEase(Ease.OutQuad))
                .Insert(0.18f, blast.DOFade(0f, 0.4f).SetEase(Ease.InQuad))
                .OnComplete(() => Destroy(blast.gameObject));
        }
    }

    // 버프: 적 자신의 실루엣이 금빛으로 두 번 부풀며 번진다 (불 · 점프 없이 "힘이 차오른다"만 보이게)
    private void PlayBuffAura(EnemyView view)
    {
        var src = view.SpriteRect.GetComponent<Image>();
        if (src == null || src.sprite == null) return;
        SpawnBuffGhost(view, src, 0f, buffGhostScale);
        SpawnBuffGhost(view, src, 0.16f, buffGhostScale * 1.06f);
    }

    private void SpawnBuffGhost(EnemyView view, Image src, float delay, float peakScale)
    {
        var body = view.SpriteRect;
        var go = new GameObject("EnemyBuffGhost", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = body.gameObject.layer;
        var rt = (RectTransform)go.transform;
        rt.SetParent(body.parent, false);
        rt.SetSiblingIndex(body.GetSiblingIndex() + 1); // 몸 바로 위 (같은 캔버스 안)
        rt.anchorMin = body.anchorMin; rt.anchorMax = body.anchorMax; rt.pivot = body.pivot;
        rt.sizeDelta = body.sizeDelta; rt.anchoredPosition = body.anchoredPosition;
        rt.localRotation = body.localRotation;
        Vector3 baseScale = body.localScale;
        rt.localScale = baseScale;
        var img = go.GetComponent<Image>();
        img.sprite = src.sprite;
        img.preserveAspect = src.preserveAspect;
        img.raycastTarget = false;
        if (impactAdditiveMaterial != null) img.material = impactAdditiveMaterial;
        img.color = new Color(buffGlowColor.r, buffGlowColor.g, buffGlowColor.b, 0f);
        DOTween.Sequence().SetUpdate(true).SetLink(go).SetDelay(delay)
            .Append(img.DOFade(buffGlowColor.a, 0.07f))
            .Join(rt.DOScale(baseScale * peakScale, 0.5f).SetEase(Ease.OutCubic))
            .Insert(0.1f, img.DOFade(0f, 0.42f).SetEase(Ease.InQuad))
            .OnComplete(() => Destroy(go));
    }

    // 디버프: 적에게서 저주 구체가 날아가 플레이어를 감싸며 터진다
    private void FireCurse(EnemyView view, float flight)
    {
        Vector2 from = PointInRect(view.SpriteRect, new Vector2(0.3f, 0.62f));
        Vector2 to = PlayerPoint() + new Vector2(0f, -40f);
        flight = Mathf.Min(flight, curseFlightTime);
        var orbs = ActionSprites("Orb");
        if (orbs.Length > 0)
        {
            var orb = MakeImage("CurseOrb", orbs[Random.Range(0, orbs.Length)], true);
            var rt = orb.rectTransform;
            rt.pivot = SpritePivot(orb.sprite, new Vector2(0.85f, 0.5f)); // 구체 중심
            float len = 300f;
            rt.sizeDelta = new Vector2(len, len / SpriteAspect(orb.sprite, 4f));
            Vector2 dir = to - from;
            rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            rt.anchoredPosition = from;
            rt.localScale = Vector3.one * 0.6f;
            Vector2 mid = (from + to) * 0.5f + new Vector2(0f, 120f);
            DOTween.Sequence().SetUpdate(true).SetLink(orb.gameObject)
                .Append(DOVirtual.Float(0f, 1f, flight, t =>
                {
                    if (rt == null) return;
                    Vector2 a = Vector2.Lerp(from, mid, t), b = Vector2.Lerp(mid, to, t);
                    Vector2 p = Vector2.Lerp(a, b, t), d = b - a;
                    rt.anchoredPosition = p;
                    if (d.sqrMagnitude > 0.01f) rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
                }).SetEase(Ease.InQuad))
                .Join(rt.DOScale(1f, flight))
                .Append(orb.DOFade(0f, 0.06f))
                .OnComplete(() => Destroy(orb.gameObject));
        }
        DOVirtual.DelayedCall(flight, () => { if (this != null) CurseBurst(to); }, true).SetLink(gameObject);
    }

    private void CurseBurst(Vector2 at)
    {
        var bursts = ActionSprites("Curse");
        if (bursts.Length == 0) return;
        var burst = MakeImage("CurseBurst", bursts[Random.Range(0, bursts.Length)], true);
        var rt = burst.rectTransform;
        rt.pivot = SpritePivot(burst.sprite, new Vector2(0.5f, 0.5f));
        rt.sizeDelta = new Vector2(curseBurstSize * SpriteAspect(burst.sprite, 1f), curseBurstSize);
        rt.anchoredPosition = at;
        rt.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        rt.localScale = Vector3.one * 0.4f;
        DOTween.Sequence().SetUpdate(true).SetLink(burst.gameObject)
            .Append(rt.DOScale(1f, 0.16f).SetEase(Ease.OutCubic))
            .Join(rt.DOLocalRotate(new Vector3(0f, 0f, rt.localEulerAngles.z + 25f), 0.6f))
            .Append(rt.DOScale(1.15f, 0.4f))
            .Join(burst.DOFade(0f, 0.4f).SetEase(Ease.InQuad))
            .OnComplete(() => Destroy(burst.gameObject));
    }

    // ───────── 도우미 ─────────

    private Sprite[] ActionSprites(string prefix)
    {
        if (_actionSprites.TryGetValue(prefix, out var cached)) return cached;
        var list = new List<Sprite>();
        foreach (var s in Resources.LoadAll<Sprite>(EnemyActionPath))
            if (s.name.StartsWith(prefix + "_") || s.name == prefix) list.Add(s);
        var arr = list.ToArray();
        _actionSprites[prefix] = arr;
        return arr;
    }

    private Image MakeImage(string name, Sprite sprite, bool additive)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = gameObject.layer;
        var rt = (RectTransform)go.transform;
        rt.SetParent(_root, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        var image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.raycastTarget = false;
        if (additive && impactAdditiveMaterial != null) image.material = impactAdditiveMaterial;
        return image;
    }

    // 조각마다 정해 둔 기준점 (스프라이트 피벗, 0–1)
    private static Vector2 SpritePivot(Sprite sprite, Vector2 fallback)
        => sprite != null && sprite.rect.width > 0f ? new Vector2(sprite.pivot.x / sprite.rect.width, sprite.pivot.y / sprite.rect.height) : fallback;

    private static float SpriteAspect(Sprite sprite, float fallback)
        => sprite != null && sprite.rect.height > 0f ? sprite.rect.width / sprite.rect.height : fallback;

    private Vector2 PointInRect(RectTransform target, Vector2 normalized)
    {
        var corners = new Vector3[4];
        target.GetWorldCorners(corners);
        Vector3 bottom = Vector3.Lerp(corners[0], corners[3], normalized.x);
        Vector3 top = Vector3.Lerp(corners[1], corners[2], normalized.x);
        return (Vector2)_root.InverseTransformPoint(Vector3.Lerp(bottom, top, normalized.y));
    }

    private bool TryPlayerFeet(out Vector3 world, out Vector2 local)
    {
        world = default; local = default;
        PlayerPoint();
        if (_playerAnchor == null) return false;
        var corners = new Vector3[4];
        _playerAnchor.GetWorldCorners(corners);
        world = (corners[0] + corners[3]) * 0.5f;
        var body = _playerAnchor.GetComponentInChildren<Image>();
        if (body != null && BattleUnitLook.TryVisibleFeetWorld(body, out Vector3 visibleFeet)) world = visibleFeet;
        local = _root.InverseTransformPoint(world);
        return true;
    }
}

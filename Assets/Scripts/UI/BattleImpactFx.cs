using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 전투 타격감. 피해가 들어가는 순간을 묵직하게 만든다. 입력은 막지 않는다(전부 raycastTarget off).
//   - 피해 숫자: 맞은 적 위로 숫자가 튀어 오른다. 큰 피해일수록 크게. 플레이어가 맞으면 HP 바 위에(붉게), 방어도로 막은 양은 작게 "막음 N"
//   - 히트스톱: 맞는 순간 시간을 아주 잠깐 거의 멈춘다. 피해에 비례(최대 hitStopMax). 처치 시 조금 더 길게
//   - 화면 흔들림: 전장(배경·적·캐릭터)만 흔든다. HUD는 흔들지 않는다. 피해 shakeThreshold 이상·처치·플레이어 피격 때
//   - 플레이어 피격: 화면 가장자리 붉은 비네트
//   - 효과음: 클립을 넣으면 재생 (피격 / 큰 피격 / 처치 / 플레이어 피격 / 막음)
// 마지막 적을 쓰러뜨리는 타격은 히트스톱 대신 작전 종료 슬로모션(BattleOperationFx)에 맡긴다.
// 세기는 전부 인스펙터에서 조절. UI 요소는 Awake에서 코드로 만든다.
public class BattleImpactFx : MonoBehaviour
{
    [SerializeField] private TMP_FontAsset font;
    [SerializeField] private int sortingOrder = 95; // 손패(5)·카드 컷인(90) 위, 배너(210) 아래

    [Header("피해 숫자")]
    [SerializeField] private float numberSize = 46f;
    [SerializeField] private float bigNumberSize = 64f;
    [SerializeField, Min(1)] private int bigDamage = 15;           // 이 이상이면 큰 숫자
    [SerializeField] private Color numberColor = new(0.086f, 0.094f, 0.106f, 1f);   // 먹색 (밝은 전장 위)
    [SerializeField] private Color bigNumberColor = new(0.051f, 0.722f, 0.949f, 1f);
    [SerializeField] private Color playerNumberColor = new(0.93f, 0.33f, 0.31f, 1f);
    [SerializeField] private Color blockNumberColor = new(0.36f, 0.38f, 0.41f, 1f);
    [SerializeField] private string playerAnchorName = "PlayerDraft"; // 플레이어 피해 숫자를 띄울 캐릭터 (Canvas 바로 아래 이름)

    [Header("히트스톱 (실제 시간, 초)")]
    [SerializeField] private bool hitStop = true;
    [SerializeField] private float hitStopBase = 0.035f;
    [SerializeField] private float hitStopPerDamage = 0.003f;
    [SerializeField] private float hitStopMax = 0.11f;
    [SerializeField] private float hitStopKillBonus = 0.05f;
    [SerializeField, Range(0f, 0.5f)] private float hitStopScale = 0.04f;

    [Header("화면 흔들림")]
    [SerializeField] private string[] shakeTargetNames = { "Background", "EnemyArea", "PlayerDraft", "CoopDraft", "PlayerShadow", "PartyViewer", "CoopTransform1", "CoopTransform2", "CoopTransform3" };
    [SerializeField, Min(0)] private int shakeThreshold = 8;
    [SerializeField] private float shakeStrength = 9f;      // 기준 피해(shakeThreshold)일 때 픽셀
    [SerializeField] private float shakeStrengthMax = 26f;
    [SerializeField] private float shakeDuration = 0.22f;

    [Header("플레이어 피격 비네트")]
    [SerializeField] private Color vignetteColor = new(0.78f, 0.12f, 0.14f, 0.38f);

    [Header("효과음 (비우면 재생 안 함)")]
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip heavyHitClip;
    [SerializeField] private AudioClip killClip;
    [SerializeField] private AudioClip playerHitClip;
    [SerializeField] private AudioClip playerBlockClip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.8f;

    private BattleManager _battle;
    private RectTransform _root;
    private Image _vignette;
    private AudioSource _audio;
    private readonly List<RectTransform> _shakeTargets = new();
    private readonly Dictionary<RectTransform, (Tween tween, Vector2 applied)> _offsets = new();
    private readonly HashSet<EnemyInstance> _watched = new();
    private PlayerCombatant _player;
    private Coroutine _stopCo;
    private float _stopTimeScale = -1f; // 히트스톱이 건 값 (다른 연출이 timeScale을 바꿨으면 되돌리지 않는다)
    private float _lastNumberTime;
    private int _stack;

    private void Awake()
    {
        _root = (RectTransform)transform;
        var canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = sortingOrder;
        if (font == null) font = TMP_Settings.defaultFontAsset;
        _audio = gameObject.AddComponent<AudioSource>();
        _audio.playOnAwake = false;
        BuildVignette();
    }

    private void Start()
    {
        _battle = BattleManager.Instance;
        CollectShakeTargets();
        if (_battle == null) return;
        _battle.OnBattleStarted += Rewire;
        _battle.OnEnemiesChanged += Rewire;
        Rewire();
    }

    private void OnDestroy()
    {
        if (_battle != null)
        {
            _battle.OnBattleStarted -= Rewire;
            _battle.OnEnemiesChanged -= Rewire;
        }
        Unwatch();
        RestoreTime();
        foreach (var kv in _offsets) { kv.Value.tween?.Kill(); if (kv.Key != null) kv.Key.anchoredPosition -= kv.Value.applied; }
        _offsets.Clear();
    }

    private void OnDisable() => RestoreTime();

    // ───────── 이벤트 연결 ─────────

    private void Rewire()
    {
        var state = _battle != null ? _battle.State : null;
        if (state == null) return;

        if (_player != state.Player)
        {
            if (_player != null) { _player.OnDamaged -= HandlePlayerDamaged; _player.OnBlocked -= HandlePlayerBlocked; }
            _player = state.Player;
            if (_player != null) { _player.OnDamaged += HandlePlayerDamaged; _player.OnBlocked += HandlePlayerBlocked; }
        }

        _watched.RemoveWhere(e => !state.Enemies.Contains(e));
        foreach (var enemy in state.Enemies)
        {
            if (enemy == null || !_watched.Add(enemy)) continue;
            var e = enemy;
            e.OnDamaged += amount => HandleEnemyDamaged(e, amount);
        }
    }

    private void Unwatch()
    {
        if (_player != null) { _player.OnDamaged -= HandlePlayerDamaged; _player.OnBlocked -= HandlePlayerBlocked; }
        _player = null;
        _watched.Clear(); // 적 인스턴스는 전투마다 새로 만들어지므로 람다 구독은 같이 버려진다
    }

    // ───────── 적 피격 ─────────

    private void HandleEnemyDamaged(EnemyInstance enemy, int amount)
    {
        if (amount <= 0 || !isActiveAndEnabled) return;
        var view = FindView(enemy);
        bool killed = enemy.IsDead;
        bool lastKill = killed && AllEnemiesDead();
        bool big = amount >= bigDamage;

        if (view != null)
            SpawnNumber(PointAbove(view.SpriteRect), amount.ToString(), big ? bigNumberSize : numberSize, big ? bigNumberColor : numberColor);

        if (!lastKill) HitStop(amount, killed);
        if (amount >= shakeThreshold || killed) Shake(amount, killed);
        Play(killed ? (killClip != null ? killClip : heavyHitClip) : big && heavyHitClip != null ? heavyHitClip : hitClip);
    }

    private bool AllEnemiesDead()
    {
        foreach (var e in _battle.Enemies) if (e != null && !e.IsDead) return false;
        return true;
    }

    private static EnemyView FindView(EnemyInstance enemy)
    {
        foreach (var view in FindObjectsByType<EnemyView>(FindObjectsSortMode.None))
            if (view.Instance == enemy) return view;
        return null;
    }

    // ───────── 플레이어 피격 ─────────

    private void HandlePlayerDamaged(int amount)
    {
        if (amount <= 0 || !isActiveAndEnabled) return;
        SpawnNumber(PlayerPoint(), amount.ToString(), amount >= bigDamage ? bigNumberSize : numberSize, playerNumberColor);
        HitStop(amount, false);
        Shake(Mathf.Max(amount, shakeThreshold), false);
        FlashVignette(amount);
        Play(playerHitClip);
    }

    private void HandlePlayerBlocked(int absorbed)
    {
        if (absorbed <= 0 || !isActiveAndEnabled) return;
        SpawnNumber(PlayerPoint() + new Vector2(-110f, -40f), $"막음 {absorbed}", numberSize * 0.62f, blockNumberColor);
        Play(playerBlockClip);
    }

    // ───────── 피해 숫자 ─────────

    private void SpawnNumber(Vector2 at, string text, float size, Color color)
    {
        // 연타는 조금씩 위로 쌓고 좌우로 흩는다
        _stack = Time.unscaledTime - _lastNumberTime < 0.35f ? _stack + 1 : 0;
        _lastNumberTime = Time.unscaledTime;
        at += new Vector2((_stack % 2 == 0 ? -1f : 1f) * 26f + Random.Range(-10f, 10f), _stack * 40f);

        var go = new GameObject("DamageNumber", typeof(RectTransform), typeof(CanvasRenderer));
        go.layer = gameObject.layer;
        var rt = (RectTransform)go.transform;
        rt.SetParent(_root, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(300f, 90f);
        rt.anchoredPosition = at;
        var t = go.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.text = text;
        t.fontSize = size;
        t.fontStyle = FontStyles.Bold;
        t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        t.textWrappingMode = TextWrappingModes.NoWrap;

        rt.localScale = Vector3.one * 1.6f;
        DOTween.Sequence().SetUpdate(true).SetLink(go)
            .Append(rt.DOScale(1f, 0.16f).SetEase(Ease.OutBack))
            .Join(rt.DOAnchorPosY(at.y + 34f, 0.16f).SetEase(Ease.OutQuad))
            .Append(rt.DOAnchorPosY(at.y + 70f, 0.55f).SetEase(Ease.OutCubic))
            .Insert(0.42f, t.DOFade(0f, 0.3f))
            .OnComplete(() => Destroy(go));
    }

    private Vector2 PointAbove(RectTransform target)
    {
        var corners = new Vector3[4];
        target.GetWorldCorners(corners);
        Vector3 top = (corners[1] + corners[2]) * 0.5f;
        Vector3 center = (corners[0] + corners[2]) * 0.5f;
        Vector3 world = Vector3.Lerp(center, top, 0.55f);
        return (Vector2)_root.InverseTransformPoint(world);
    }

    private RectTransform _playerAnchor;
    private Vector2 PlayerPoint()
    {
        if (_playerAnchor == null)
            _playerAnchor = GetComponentInParent<Canvas>().rootCanvas.transform.Find(playerAnchorName) as RectTransform;
        return _playerAnchor != null ? PointAbove(_playerAnchor) : new Vector2(-560f, 0f);
    }

    // ───────── 히트스톱 ─────────

    private void HitStop(int amount, bool killed)
    {
        if (!hitStop) return;
        float duration = Mathf.Min(hitStopMax, hitStopBase + amount * hitStopPerDamage) + (killed ? hitStopKillBonus : 0f);
        if (_stopCo != null) StopCoroutine(_stopCo);
        else if (Time.timeScale < 0.99f) return; // 다른 연출(슬로모션 등)이 시간을 바꿔 둔 상태면 건드리지 않는다
        _stopCo = StartCoroutine(StopRoutine(duration));
    }

    private IEnumerator StopRoutine(float duration)
    {
        _stopTimeScale = hitStopScale;
        Time.timeScale = hitStopScale;
        yield return new WaitForSecondsRealtime(duration);
        RestoreTime();
    }

    private void RestoreTime()
    {
        if (_stopTimeScale >= 0f && Mathf.Approximately(Time.timeScale, _stopTimeScale)) Time.timeScale = 1f;
        _stopTimeScale = -1f;
        _stopCo = null;
    }

    // ───────── 화면 흔들림 ─────────

    private void CollectShakeTargets()
    {
        _shakeTargets.Clear();
        var canvas = GetComponentInParent<Canvas>().rootCanvas.transform;
        foreach (var name in shakeTargetNames)
            if (canvas.Find(name) is RectTransform rt) _shakeTargets.Add(rt);
    }

    private void Shake(int amount, bool killed)
    {
        if (_shakeTargets.Count == 0 || shakeStrength <= 0f) return;
        float strength = Mathf.Min(shakeStrengthMax, shakeStrength * Mathf.Max(1f, (float)amount / Mathf.Max(1, shakeThreshold)) * (killed ? 1.3f : 1f));
        foreach (var rt in _shakeTargets)
        {
            if (rt == null) continue;
            Offset(rt, shakeDuration, t => Random.insideUnitCircle * strength * (1f - t) * (1f - t));
        }
    }

    // 대상 위치에 오프셋을 "더했다 빼는" 방식으로 흔든다. 대상의 다른 트윈(등장·공격 모션)을 끄거나 위치를 덮어쓰지 않는다.
    private void Offset(RectTransform rt, float duration, System.Func<float, Vector2> curve)
    {
        if (_offsets.TryGetValue(rt, out var prev))
        {
            prev.tween?.Kill();
            if (rt != null) rt.anchoredPosition -= prev.applied;
        }
        _offsets[rt] = (null, Vector2.zero);
        var tween = DOVirtual.Float(0f, 1f, duration, t =>
        {
            if (rt == null) return;
            var cur = _offsets[rt];
            Vector2 next = t >= 1f ? Vector2.zero : curve(t);
            rt.anchoredPosition += next - cur.applied;
            _offsets[rt] = (cur.tween, next);
        }).SetUpdate(true).SetEase(Ease.Linear).SetLink(rt.gameObject);
        _offsets[rt] = (tween, _offsets[rt].applied); // 생성 중에 이미 한 번 적용됐을 수 있으니 적용량은 유지
    }

    // ───────── 비네트 ─────────

    private void BuildVignette()
    {
        var go = new GameObject("PlayerHitVignette", typeof(RectTransform), typeof(CanvasRenderer));
        go.layer = gameObject.layer;
        var rt = (RectTransform)go.transform;
        rt.SetParent(_root, false);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        _vignette = go.AddComponent<Image>();
        _vignette.sprite = MakeVignetteSprite();
        _vignette.raycastTarget = false;
        _vignette.color = new Color(vignetteColor.r, vignetteColor.g, vignetteColor.b, 0f);
    }

    private static Sprite MakeVignetteSprite()
    {
        const int w = 128, h = 72;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
        var px = new Color32[w * h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float dx = Mathf.Abs(x / (w - 1f) - 0.5f) * 2f, dy = Mathf.Abs(y / (h - 1f) - 0.5f) * 2f;
                float d = Mathf.Clamp01((Mathf.Max(dx, dy) * 0.7f + Mathf.Sqrt(dx * dx + dy * dy) * 0.3f - 0.78f) / 0.3f);
                px[y * w + x] = new Color32(255, 255, 255, (byte)(d * d * 255));
            }
        tex.SetPixels32(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
    }

    private void FlashVignette(int amount)
    {
        if (_vignette == null) return;
        float peak = vignetteColor.a * Mathf.Lerp(0.6f, 1f, Mathf.InverseLerp(1, bigDamage, amount));
        _vignette.DOKill();
        _vignette.color = new Color(vignetteColor.r, vignetteColor.g, vignetteColor.b, 0f);
        DOTween.Sequence().SetUpdate(true).SetLink(_vignette.gameObject)
            .Append(_vignette.DOFade(peak, 0.06f))
            .Append(_vignette.DOFade(0f, 0.45f).SetEase(Ease.InQuad));
    }

    // ───────── 효과음 ─────────

    private void Play(AudioClip clip)
    {
        if (clip == null || _audio == null) return;
        _audio.pitch = Random.Range(0.95f, 1.05f);
        _audio.PlayOneShot(clip, volume);
    }
}

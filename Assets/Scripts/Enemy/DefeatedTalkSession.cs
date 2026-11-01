using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 격퇴 후 대화 화면. 보상 화면에서 "진행"을 누른 뒤, 맵으로 가기 전에 한 번 끼어든다.
//   - 쓰러뜨린 적(DefeatedTalkData가 있는 적)은 전투 그림 그대로 서 있고, 클릭하면 스탠딩 + 대사 (NpcDialogueOverlay)
//   - 대사가 끝나면 호감도 +, 합류 후보이고 호감도가 충분하면 "데려간다 / 두고 간다"
//   - 오른쪽 아래 "계속"을 누르면 그대로 맵으로 (대화는 선택)
// UI는 코드로 만든다 (전투 캔버스 위 별도 캔버스). 흐름: EnemyView.HandleDied → Register, RewardsView.OnProceedClicked → TryShow
public class DefeatedTalkSession : MonoBehaviour
{
    private class Entry
    {
        public EnemyView View;
        public RectTransform Sprite;
        public DefeatedTalkData Data;
        public bool Talked;
        public RectTransform Hint;
    }

    private static readonly List<Entry> s_pending = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => s_pending.Clear();

    public static bool HasPending
    {
        get
        {
            s_pending.RemoveAll(e => e.View == null);
            return s_pending.Count > 0;
        }
    }

    // 격퇴 순간 (EnemyView). 호감도는 여기서 바로 쌓는다 — 대화하지 않고 지나가도 격퇴 몫은 남는다
    public static void Register(EnemyView view, RectTransform sprite, DefeatedTalkData data)
    {
        if (view == null || data == null) return;
        s_pending.Add(new Entry { View = view, Sprite = sprite, Data = data });
        var aff = BlessingAffinityManager.Instance;
        if (aff != null && data.defeatAffinityGain > 0f) aff.AddAffinity(data.entityId, data.defeatAffinityGain);
    }

    // 보상 화면 진행 버튼에서. 남은 대상이 있으면 이 화면을 띄우고 true (맵은 "계속"을 누를 때 연다)
    public static bool TryShow(Action onLeave)
    {
        if (!HasPending) return false;
        var anchor = s_pending[0].View.GetComponentInParent<Canvas>();
        if (anchor == null) return false;
        var go = new GameObject("DefeatedTalkSession", typeof(RectTransform));
        go.transform.SetParent(anchor.rootCanvas.transform, false);
        go.AddComponent<DefeatedTalkSession>().Build(onLeave);
        return true;
    }

    // ── 화면 ──
    private const int SortingOrder = 56; // 대사창(NpcDialogueOverlay 58)보다 아래, 전투 HUD보다 위
    private Action _onLeave;
    private readonly List<Entry> _entries = new();
    private RectTransform _root;
    private TMP_FontAsset _font;
    private GameObject _stage;      // 스탠딩 + 어둡게 깐 막 (대화 중)
    private Image _standing;
    private GameObject _choice;
    private Entry _current;
    private NpcDialogueOverlay _overlay;

    private void Build(Action onLeave)
    {
        _onLeave = onLeave;
        _entries.AddRange(s_pending);
        s_pending.Clear();
        _font = TMP_Settings.defaultFontAsset;
        _overlay = FindFirstObjectByType<NpcDialogueOverlay>(FindObjectsInactive.Include);

        _root = (RectTransform)transform;
        Stretch(_root);
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = SortingOrder;
        gameObject.AddComponent<GraphicRaycaster>();

        // 서 있는 적 위를 덮는 투명 클릭 판 + "대화" 표시
        foreach (var e in _entries)
        {
            var hit = NewImage("Hit_" + e.Data.entityId, _root, new Color(1, 1, 1, 0));
            Cover(hit.rectTransform, e.Sprite);
            var b = hit.gameObject.AddComponent<Button>();
            var captured = e;
            b.onClick.AddListener(() => Talk(captured));

            var hint = NewText("Hint", _root, "▼  대화", 26, new Color(0.95f, 0.95f, 0.96f, 1f), TextAlignmentOptions.Center);
            hint.rectTransform.sizeDelta = new Vector2(220, 40);
            hint.raycastTarget = false;
            Vector3[] c = new Vector3[4];
            e.Sprite.GetWorldCorners(c);
            hint.rectTransform.position = (c[1] + c[2]) * 0.5f;
            hint.rectTransform.anchoredPosition += new Vector2(0, 30);
            hint.rectTransform.DOAnchorPosY(hint.rectTransform.anchoredPosition.y + 10f, 0.7f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetLink(hint.gameObject);
            e.Hint = hint.rectTransform;
        }

        // 계속 (오른쪽 아래)
        var leave = NewButton("Leave", _root, "계속  →", "CONTINUE", OnLeave);
        var lr = (RectTransform)leave.transform;
        lr.anchorMin = lr.anchorMax = lr.pivot = new Vector2(1, 0);
        lr.sizeDelta = new Vector2(340, 96);
        lr.anchoredPosition = new Vector2(-48, 420); // 오른쪽 아래 무기 · 에너지 패널 위
    }

    private void Talk(Entry e)
    {
        if (_stage != null) return;
        _current = e;
        var d = e.Data;
        if (e.Hint != null) e.Hint.gameObject.SetActive(false);

        // 어둡게 깐 막 + 스탠딩 (오른쪽, 적 자리 쪽)
        _stage = new GameObject("Stage", typeof(RectTransform));
        var sr = (RectTransform)_stage.transform;
        sr.SetParent(_root, false);
        Stretch(sr);
        var dim = NewImage("Dim", sr, new Color(0.02f, 0.03f, 0.05f, 0f));
        Stretch(dim.rectTransform);
        dim.DOFade(0.72f, 0.25f).SetLink(dim.gameObject);
        if (d.standing != null)
        {
            _standing = NewImage("Standing", sr, Color.white);
            _standing.sprite = d.standing;
            _standing.preserveAspect = true;
            _standing.raycastTarget = false;
            var r = _standing.rectTransform;
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(1, 0);
            float h = 1000f;
            r.sizeDelta = new Vector2(h * d.standing.rect.width / d.standing.rect.height, h);
            r.anchoredPosition = new Vector2(-150, -40);
            _standing.color = new Color(1, 1, 1, 0);
            _standing.DOFade(1f, 0.3f).SetLink(_standing.gameObject);
            r.DOAnchorPosX(-100, 0.35f).From(new Vector2(-40, -40)).SetEase(Ease.OutCubic).SetLink(_standing.gameObject);
        }

        var aff = BlessingAffinityManager.Instance;
        float affinity = aff != null ? aff.GetAffinity(d.entityId) : 0f;
        var seq = d.Pick(affinity, id => CooperationManager.Instance != null && CooperationManager.Instance.IsJoinedInRun(id),
                         aff != null ? aff.GetAllFlags() : new HashSet<string>());
        if (_overlay != null && seq != null) _overlay.Play(seq, d.displayName, OnTalkDone, d.portrait, d.affiliation, d.themeColor,
                                                             _standing != null ? _standing.gameObject : null);
        else OnTalkDone();
    }

    private void OnTalkDone()
    {
        var e = _current;
        if (e == null) return;
        var d = e.Data;
        if (!e.Talked)
        {
            e.Talked = true;
            var aff = BlessingAffinityManager.Instance;
            if (aff != null && d.talkAffinityGain > 0f) aff.AddAffinity(d.entityId, d.talkAffinityGain);
        }

        if (CanRecruit(d)) ShowChoice(d);
        else CloseStage();
    }

    private static bool CanRecruit(DefeatedTalkData d)
    {
        var coop = CooperationManager.Instance;
        var aff = BlessingAffinityManager.Instance;
        if (d.recruit == null || coop == null || aff == null) return false;
        if (coop.IsJoinedInRun(d.recruit.charID) || coop.IsPartyFull) return false;
        return aff.GetAffinityTier(d.entityId) >= d.recruitMinTier; // 호감도가 낮으면 아예 따라오지 않는다
    }

    private void ShowChoice(DefeatedTalkData d)
    {
        var options = new[]
        {
            new DialogueChoiceView.Option(d.recruitYes, "RECRUIT"),
            new DialogueChoiceView.Option(d.recruitNo),
        };
        var view = DialogueChoiceView.Show(_root, d.recruitPrompt, options, i =>
        {
            _choice = null; // 선택지는 스스로 사라진다
            if (i == 0) Recruit(d);
            else CloseStage();
        });
        _choice = view.gameObject;
    }

    private void Recruit(DefeatedTalkData d)
    {
        CooperationManager.Instance.SelectChar(d.recruit.charID);
        if (!string.IsNullOrEmpty(d.recruitedFlag)) BlessingAffinityManager.Instance?.SetFlag(d.recruitedFlag, true);
        Debug.Log($"[격퇴 후 대화] {d.displayName} 합류");
        CloseStage();
    }

    private void CloseStage()
    {
        if (_choice != null) Destroy(_choice);
        _choice = null;
        if (_stage != null) Destroy(_stage);
        _stage = null;
        _standing = null;
        if (_current?.Hint != null && !_current.Talked) _current.Hint.gameObject.SetActive(true);
        _current = null;
    }

    private void OnLeave()
    {
        if (_overlay != null && _overlay.IsPlaying) return;
        foreach (var e in _entries) if (e.View != null) Destroy(e.View.gameObject);
        var leave = _onLeave;
        Destroy(gameObject);
        leave?.Invoke();
    }

    // ── 만들기 ──
    private static void Stretch(RectTransform r)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero; r.pivot = new Vector2(0.5f, 0.5f);
    }

    private void Cover(RectTransform r, RectTransform target)
    {
        Vector3[] c = new Vector3[4];
        target.GetWorldCorners(c);
        Vector2 a = _root.InverseTransformPoint(c[0]), b = _root.InverseTransformPoint(c[2]);
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.pivot = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = (a + b) * 0.5f;
        r.sizeDelta = new Vector2(Mathf.Abs(b.x - a.x), Mathf.Abs(b.y - a.y));
    }

    private static Image NewImage(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = color;
        return img;
    }

    private TextMeshProUGUI NewText(string name, Transform parent, string text, float size, Color color, TextAlignmentOptions align)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        if (_font != null) t.font = _font;
        t.text = text; t.fontSize = size; t.color = color; t.alignment = align;
        t.textWrappingMode = TextWrappingModes.Normal;
        return t;
    }

    // 먹색 판 + 큰 글자 + 작은 영문 (전투 HUD의 END TURN 버튼과 같은 톤)
    private Button NewButton(string name, Transform parent, string label, string sub, Action onClick)
    {
        var bg = NewImage(name, parent, new Color(0.07f, 0.08f, 0.1f, 0.92f));
        var btn = bg.gameObject.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.75f, 0.92f, 1f, 1f);
        colors.pressedColor = new Color(0.6f, 0.8f, 0.9f, 1f);
        btn.colors = colors;
        btn.onClick.AddListener(() => onClick?.Invoke());

        var line = NewImage("Accent", bg.transform, new Color(0.05f, 0.72f, 0.95f, 1f));
        var lr = line.rectTransform;
        lr.anchorMin = new Vector2(0, 0); lr.anchorMax = new Vector2(1, 0); lr.pivot = new Vector2(0.5f, 0);
        lr.sizeDelta = new Vector2(0, 3); lr.anchoredPosition = Vector2.zero;
        line.raycastTarget = false;

        var t = NewText("Label", bg.transform, label, 30, new Color(0.95f, 0.95f, 0.96f, 1f), TextAlignmentOptions.Center);
        var tr = t.rectTransform;
        tr.anchorMin = new Vector2(0, 0.35f); tr.anchorMax = new Vector2(1, 1); tr.offsetMin = tr.offsetMax = Vector2.zero;
        t.raycastTarget = false;
        var s = NewText("Sub", bg.transform, sub, 13, new Color(0.65f, 0.72f, 0.8f, 1f), TextAlignmentOptions.Center);
        var srt = s.rectTransform;
        srt.anchorMin = new Vector2(0, 0); srt.anchorMax = new Vector2(1, 0.38f); srt.offsetMin = srt.offsetMax = Vector2.zero;
        s.characterSpacing = 12f;
        s.raycastTarget = false;
        return btn;
    }
}

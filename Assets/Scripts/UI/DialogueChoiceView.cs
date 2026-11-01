using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 대화 선택지 (v1 — 사선 컷 판이 기울어진 채 계단식으로 박힌다). 조각 · 수치: ArtDirection/DialogueMockup/Choice_v1/Extracted_Draft/layout.json
//   등장: 줄마다 0.06초 간격으로 왼쪽 밖에서 날아와 12px 지나쳤다 돌아온다 (01번 줄만 잔상 두 겹)
//   마우스 올림: 먹색 반전 + 청록 테두리, 오른쪽 18px, 그림자 14px
//   선택: 고른 줄 1.2배 · 오른쪽 50 / 위 35px + 청록 띠, 나머지는 왼쪽으로 빠지며 반투명 → 0.25초 뒤 전부 사라지고 onPick
// 좌표는 1920×1080 화면 기준(좌상단 원점)을 그대로 쓰고, 부모 크기에 맞춰 통째로 축소 · 확대한다.
//
// v2 (Stage를 넘기면) — NPC 대화 선택지. ArtDirection/DialogueMockup/Choice_v2/
//   화면 전체를 먹색으로 누르고, 오른쪽에 고심하는 주인공이 미끄러져 들어온다. 말하는 NPC 스탠딩(KeepBright)만 어둠 위로 꺼낸다.
//   줄마다 주인공 쪽을 가리키는 꼬리가 붙는다. 정렬: 어둠 · 주인공(DimOrder) < NPC(DimOrder+1) < 대사창 < 선택지(ChoiceOrder)
//
// 판 조각은 v3 (ArtDirection/DialogueMockup/Redesign_v3/Extracted/layout.json): 흰 종이 판 + 왼쪽 위 먹색 모서리 + 오른쪽 회청 사선 면,
//   마우스 올림 · 선택은 먹색 잉크 판 + 청록 테두리. 판 높이 104, 너비 720~960 (9-slice), 꼬리는 판 오른쪽 위 (-6, 38)
public class DialogueChoiceView : MonoBehaviour
{
    public struct Option
    {
        public string Label;
        public string Tag; // 오른쪽 끝 작은 영문 라벨 — 없으면 비운다
        public Option(string label, string tag = null) { Label = label; Tag = tag; }
    }

    public class Stage
    {
        public Sprite PlayerPose;         // 비우면 Resources/DialogueChoice/Player_Ponder
        public int DimOrder = 56;         // 어둠 + 주인공. 대사창(NpcDialogueOverlay 58)보다 아래
        public int ChoiceOrder = 59;      // 선택지. 대사창보다 위
        public GameObject[] KeepBright;   // 어둠 위로 꺼낼 NPC 스탠딩 (DimOrder+1로 잠깐 올린다)
    }

    private const float W = 1920f, H = 1080f;
    private const float RowH = 104f, MaxRowW = 960f;
    private const float LabelLeft = 108f, LabelRight = 120f, TagW = 96f; // 문구 자리: 판 너비 - 228 (태그가 있으면 태그 폭만큼 더 줄인다)

    // 줄 3개의 자리 (회전 전 화면 rect x, y, w, 시계방향 각도)
    private static readonly Vector4[] Rows =
    {
        new(476, 304, 724, -4),
        new(506, 436, 724, 2),
        new(476, 558, 736, -2),
    };
    // v2 — 오른쪽 끝이 주인공 앞에 모이게, 대사창 위로 (v3 정본 Choice_v3.png의 reference_rows)
    private static readonly Vector4[] RowsV2 =
    {
        new(732, 385, 720, -1.5f),
        new(711, 492, 720, 2.5f),
        new(744, 604, 720, -1.5f),
    };
    private static readonly Rect PlayerRect = new(1210, 20, 780, 1170); // 1024×1536 원본 비율, 대사창 아래로 잘린다. 꼬리 끝(x≈1500)이 얼굴에 걸리지 않게

    private static readonly Color Ink = Hex("#11191E");
    private static readonly Color TagNormal = Hex("#68737D"), TagHover = Hex("#D3DCE2");
    private static readonly Color ShadowTint = new(1f, 1f, 1f, 0.65f); // 그림자 조각이 이미 먹색
    private static readonly Color GhostTint = Hex("#D5DCE2");

    private class Row
    {
        public RectTransform Root;
        public CanvasGroup Group;
        public Image Panel, Shadow;
        public RectTransform ShadowRt, BandMask;
        public TextMeshProUGUI Number, Label, Tag;
        public Image NumberLine, Arrow, Tail;
        public Vector2 Home;      // 정착 위치 (anchoredPosition)
        public float AngleZ;      // Unity Z (= -화면 시계방향 각도)
        public float Width;
    }

    private readonly List<Row> _rows = new();
    private RectTransform _frame;
    private Action<int> _onPick;
    private bool _ready, _picked;
    private TMP_FontAsset _font;
    private Stage _stage;
    private Image _dim, _player;
    private readonly List<(GameObject go, Canvas canvas, bool added, bool prevOverride, int prevOrder)> _raised = new();

    private static readonly Dictionary<string, Sprite> s_sprites = new();
    private static Sprite Spr(string name)
    {
        if (!s_sprites.TryGetValue(name, out var s) || s == null)
            s_sprites[name] = s = Resources.Load<Sprite>("DialogueChoice/" + name);
        return s;
    }

    // parent: 화면을 덮는 RectTransform. prompt는 머리글 위 한 줄 (비우면 생략)
    public static DialogueChoiceView Show(RectTransform parent, string prompt, IList<Option> options, Action<int> onPick)
    {
        var go = new GameObject("DialogueChoice", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var v = go.AddComponent<DialogueChoiceView>();
        v.Build(prompt, options, onPick, null);
        return v;
    }

    // v2 — NPC 대화 선택지 (어둠 + 주인공 + 꼬리). prompt 없음 — NPC의 마지막 대사가 대사창에 남아 있다
    public static DialogueChoiceView ShowStaged(RectTransform parent, Stage stage, IList<Option> options, Action<int> onPick)
    {
        var go = new GameObject("DialogueChoice_v2", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var v = go.AddComponent<DialogueChoiceView>();
        v.Build(null, options, onPick, stage ?? new Stage());
        return v;
    }

    private Vector4[] Slots => _stage != null ? RowsV2 : Rows;

    private void Build(string prompt, IList<Option> options, Action<int> onPick, Stage stage)
    {
        _onPick = onPick;
        _stage = stage;
        _font = TMP_Settings.defaultFontAsset;

        var root = (RectTransform)transform;
        StretchFull(root);
        if (_stage != null)
        {
            SetCanvas(gameObject, _stage.ChoiceOrder);
            gameObject.AddComponent<GraphicRaycaster>();
            BuildStage();
        }

        // 1920×1080 판을 부모 안에 맞춰 넣는다
        _frame = NewFrame("Frame", root);

        int count = Mathf.Clamp(options.Count, 1, Rows.Length);

        // 머리글 — RESPONSE / 0N + 가는 선, 그 위에 질문 한 줄
        if (!string.IsNullOrEmpty(prompt))
        {
            var p = NewText("Prompt", _frame, prompt, 30, new Color(0.95f, 0.95f, 0.96f, 1f), TextAlignmentOptions.Left);
            p.fontStyle = FontStyles.Bold;
            p.enableAutoSizing = true; p.fontSizeMin = 22f; p.fontSizeMax = 30f; // 오른쪽 스탠딩 얼굴에 걸리지 않게 폭을 묶는다
            Place(p.rectTransform, 509, 196, 760, 50);
            FadeIn(p, 0f);
        }
        float hx = _stage != null ? 750f : 509f, hy = _stage != null ? 318f : 256f;
        float delay = _stage != null ? 0.15f : 0f;
        var head = NewText("Header", _frame, $"RESPONSE / {count:00}", 18, Color.white, TextAlignmentOptions.Left);
        head.fontStyle = FontStyles.Bold;
        Place(head.rectTransform, hx, hy, 240, 28);
        FadeIn(head, delay);

        for (int i = 0; i < count; i++) _rows.Add(BuildRow(i, options[i]));
        PlayEntrance(delay);
    }

    // v2 무대: 어둠 + 주인공 (둘 다 대사창 아래 정렬), NPC 스탠딩은 어둠 바로 위로
    private void BuildStage()
    {
        var back = NewRect("Stage", transform);
        StretchFull(back);
        SetCanvas(back.gameObject, _stage.DimOrder);

        _dim = NewImage("Dim", back, null, new Color(0.02f, 0.03f, 0.05f, 0f));
        _dim.raycastTarget = true; // 뒤쪽 상점 · 맵 입력을 막는다
        StretchFull(_dim.rectTransform);
        _dim.DOFade(0.65f, 0.15f).SetLink(_dim.gameObject);

        var frame = NewFrame("PlayerFrame", back);
        _player = NewImage("Player", frame, _stage.PlayerPose != null ? _stage.PlayerPose : Spr("Player_Ponder"), new Color(1, 1, 1, 0));
        _player.preserveAspect = true;
        var pr = _player.rectTransform;
        Place(pr, PlayerRect.x, PlayerRect.y, PlayerRect.width, PlayerRect.height);
        var home = pr.anchoredPosition;
        pr.anchoredPosition = home + new Vector2(240f, 0f);
        pr.DOAnchorPos(home, 0.2f).SetEase(Ease.OutCubic).SetLink(_player.gameObject);
        _player.DOFade(1f, 0.15f).SetLink(_player.gameObject);

        if (_stage.KeepBright != null)
            foreach (var go in _stage.KeepBright)
            {
                if (go == null) continue;
                bool added = !go.TryGetComponent<Canvas>(out var c);
                if (added) c = go.AddComponent<Canvas>();
                _raised.Add((go, c, added, c.overrideSorting, c.sortingOrder));
                c.overrideSorting = true;
                c.sortingOrder = _stage.DimOrder + 1;
            }
    }

    private void RestoreRaised()
    {
        foreach (var (go, c, added, prevOverride, prevOrder) in _raised)
        {
            if (go == null || c == null) continue;
            if (added) Destroy(c);
            else { c.overrideSorting = prevOverride; c.sortingOrder = prevOrder; }
        }
        _raised.Clear();
    }

    private void OnDestroy() => RestoreRaised();

    private static void SetCanvas(GameObject go, int order)
    {
        var c = go.AddComponent<Canvas>();
        c.overrideSorting = true;
        c.sortingOrder = order;
    }

    // 부모를 덮는 1920×1080 판 (부모 크기에 맞춰 통째로 축소 · 확대)
    private RectTransform NewFrame(string name, Transform parent)
    {
        var f = NewRect(name, parent);
        f.anchorMin = f.anchorMax = f.pivot = new Vector2(0.5f, 0.5f);
        f.sizeDelta = new Vector2(W, H);
        var pr = ((RectTransform)transform.parent).rect;
        float scale = Mathf.Min(pr.width / W, pr.height / H);
        if (scale > 0f) f.localScale = Vector3.one * scale;
        return f;
    }

    // ── 줄 하나 ──
    private Row BuildRow(int i, Option opt)
    {
        var slot = Slots[i];
        bool hasTag = !string.IsNullOrEmpty(opt.Tag);
        string label = opt.Label ?? "";

        var r = new Row { AngleZ = -slot.w };
        r.Root = NewRect($"Row{i + 1:00}", _frame);
        r.Group = r.Root.gameObject.AddComponent<CanvasGroup>();

        // 그림자 — 화면 기준 오른쪽 아래로 깔리게, 회전을 거꾸로 돌려 로컬 오프셋으로
        r.Shadow = NewImage("Shadow", r.Root, Spr("Choice_Shadow"), ShadowTint, sliced: true);
        r.ShadowRt = r.Shadow.rectTransform;
        StretchFull(r.ShadowRt);
        if (_stage != null) PlaceTail(NewImage("TailShadow", r.ShadowRt, Spr("Choice_Tail_Shadow"), Color.white));

        r.Panel = NewImage("Panel", r.Root, Spr("Choice_Panel_Normal"), Color.white, sliced: true);
        r.Panel.raycastTarget = true;
        StretchFull(r.Panel.rectTransform);

        // v2 꼬리 — 판 오른쪽 위 기준 (-6, 38), 왼쪽 6px는 판 테두리 위에 겹친다
        if (_stage != null)
        {
            r.Tail = NewImage("Tail", r.Root, Spr("Choice_Tail_Normal"), Color.white);
            r.Tail.raycastTarget = true;
            PlaceTail(r.Tail);
        }

        // 선택 띠 — 왼쪽 아래 컷 끝점에서 오른쪽으로 그어진다 (마스크 폭으로 드러냄)
        r.BandMask = NewRect("BandMask", r.Root);
        r.BandMask.gameObject.AddComponent<RectMask2D>();
        r.BandMask.anchorMin = r.BandMask.anchorMax = r.BandMask.pivot = new Vector2(0, 1);
        r.BandMask.anchoredPosition = new Vector2(28, -101);
        r.BandMask.sizeDelta = new Vector2(0, 3);
        var band = NewImage("Band", r.BandMask, Spr("Choice_SelectBand"), Color.white, sliced: true);
        band.rectTransform.anchorMin = band.rectTransform.anchorMax = band.rectTransform.pivot = new Vector2(0, 1);
        band.rectTransform.anchoredPosition = Vector2.zero;

        // 왼쪽 묶음 (번호 · 밑줄 · 문구 · 문구 밑 짧은 선) — 왼쪽 고정. 선 조각은 회색 그대로 쓴다
        float labelRight = LabelRight + (hasTag ? TagW : 0f);
        r.Number = NewText("Number", r.Root, $"{i + 1:00}", 20, Ink, TextAlignmentOptions.Left);
        r.Number.fontStyle = FontStyles.Bold;
        PlaceLocal(r.Number.rectTransform, 36, 34, 40, 28, false);
        r.NumberLine = NewImage("NumberLine", r.Root, Spr("Choice_NumberLine"), Color.white);
        PlaceLocal(r.NumberLine.rectTransform, 36, 68, 22, 2, false);
        var div = NewImage("Divider", r.Root, Spr("Choice_Divider"), Color.white);
        PlaceLocal(div.rectTransform, LabelLeft, 82, 24, 2, false);

        r.Label = NewText("Label", r.Root, label, 32, Ink, TextAlignmentOptions.Left);
        r.Label.fontStyle = FontStyles.Bold;
        r.Label.textWrappingMode = TextWrappingModes.NoWrap;
        r.Label.overflowMode = TextOverflowModes.Ellipsis;
        var lr = r.Label.rectTransform;
        lr.anchorMin = new Vector2(0, 1); lr.anchorMax = new Vector2(1, 1); lr.pivot = new Vector2(0, 1);
        lr.offsetMin = new Vector2(LabelLeft, -27 - 44);
        lr.offsetMax = new Vector2(-labelRight, -27);

        // 오른쪽 묶음 (태그 · 화살표) — 오른쪽 고정
        r.Tag = NewText("Tag", r.Root, opt.Tag ?? "", 13, TagNormal, TextAlignmentOptions.Right);
        r.Tag.characterSpacing = 2f;
        r.Tag.textWrappingMode = TextWrappingModes.NoWrap;
        r.Tag.overflowMode = TextOverflowModes.Ellipsis;
        PlaceLocal(r.Tag.rectTransform, 70 + 12 + 84, 34, 84, 28, true);
        r.Tag.gameObject.SetActive(hasTag);
        r.Arrow = NewImage("Arrow", r.Root, Spr("Choice_Arrow"), Ink); // 흰 원본 — 평소 먹색, 마우스 올림 · 선택은 흰색
        PlaceLocal(r.Arrow.rectTransform, 70, 35, 32, 22, true);

        // 폭 — 문구가 길면 오른쪽 끝을 고정한 채 왼쪽으로 늘린다 (최대 960)
        float w = slot.z;
        float need = LabelLeft + r.Label.GetPreferredValues(label).x + labelRight;
        if (need > MaxRowW) { r.Label.enableAutoSizing = true; r.Label.fontSizeMin = 20f; r.Label.fontSizeMax = 32f; } // 960도 넘치면 글자를 줄인다
        w = Mathf.Min(MaxRowW, Mathf.Max(w, need));
        float x = slot.x + slot.z - w;
        r.Width = w;
        r.Root.anchorMin = r.Root.anchorMax = new Vector2(0, 1);
        r.Root.pivot = new Vector2(0.5f, 0.5f);
        r.Root.sizeDelta = new Vector2(w, RowH);
        r.Home = new Vector2(x + w * 0.5f, -(slot.y + RowH * 0.5f));
        r.Root.anchoredPosition = r.Home;
        r.Root.localRotation = Quaternion.Euler(0, 0, r.AngleZ);
        SetShadow(r, 10f, 1f);

        // 입력
        var idx = i;
        var et = r.Panel.gameObject.AddComponent<EventTrigger>();
        AddTrigger(et, EventTriggerType.PointerEnter, () => Hover(idx, true));
        AddTrigger(et, EventTriggerType.PointerExit, () => Hover(idx, false));
        AddTrigger(et, EventTriggerType.PointerClick, () => Pick(idx));
        if (r.Tail != null)
        {
            var tet = r.Tail.gameObject.AddComponent<EventTrigger>();
            AddTrigger(tet, EventTriggerType.PointerEnter, () => Hover(idx, true));
            AddTrigger(tet, EventTriggerType.PointerExit, () => Hover(idx, false));
            AddTrigger(tet, EventTriggerType.PointerClick, () => Pick(idx));
        }
        return r;
    }

    private static void PlaceTail(Image tail)
    {
        var t = tail.rectTransform;
        t.anchorMin = t.anchorMax = new Vector2(1, 1);
        t.pivot = new Vector2(0, 1);
        t.sizeDelta = new Vector2(56, 32);
        t.anchoredPosition = new Vector2(-6, -38);
    }

    // ── 연출 ──
    private void PlayEntrance(float baseDelay)
    {
        float frameLeft = 0f;
        for (int i = 0; i < _rows.Count; i++)
        {
            var r = _rows[i];
            float delay = baseDelay + i * 0.06f;
            // 회전한 줄 전체가 화면 왼쪽 밖에 있도록
            float halfAabb = 0.5f * (r.Width * Mathf.Abs(Mathf.Cos(r.AngleZ * Mathf.Deg2Rad)) + RowH * Mathf.Abs(Mathf.Sin(r.AngleZ * Mathf.Deg2Rad)));
            var start = new Vector2(frameLeft - halfAabb - 24f, r.Home.y + 36f);
            r.Root.anchoredPosition = start;
            r.Group.alpha = 0f;

            if (i == 0) SpawnGhosts(r, delay);

            var seq = DOTween.Sequence().SetLink(r.Root.gameObject);
            seq.AppendInterval(delay);
            seq.AppendCallback(() => r.Group.alpha = 1f);
            seq.Append(r.Root.DOAnchorPos(r.Home + new Vector2(12f, 0f), 0.18f).SetEase(Ease.OutCubic));
            seq.Append(r.Root.DOAnchorPos(r.Home, 0.08f).SetEase(Ease.OutQuad));
        }
        DOVirtual.DelayedCall(baseDelay + 0.06f * (_rows.Count - 1) + 0.26f, () => _ready = true).SetLink(gameObject);
    }

    // 01번 줄 착지 순간에만 보이는 외곽선 잔상 두 겹
    private void SpawnGhosts(Row r, float delay)
    {
        var offs = new[] { new Vector2(-12f, 12f), new Vector2(-24f, 24f) };
        var alphas = new[] { 0.22f, 0.12f };
        for (int g = 0; g < 2; g++)
        {
            // 잔상은 판 모양 그대로 — 그림자 조각을 밝게 칠해 쓴다
            var img = NewImage("Ghost", _frame, Spr("Choice_Shadow"), new Color(GhostTint.r, GhostTint.g, GhostTint.b, 0f), sliced: true);
            var gr = img.rectTransform;
            gr.SetSiblingIndex(r.Root.GetSiblingIndex());
            gr.anchorMin = gr.anchorMax = new Vector2(0, 1);
            gr.pivot = new Vector2(0.5f, 0.5f);
            gr.sizeDelta = r.Root.sizeDelta;
            gr.localRotation = r.Root.localRotation;
            gr.anchoredPosition = r.Home + offs[g];
            float a = alphas[g];
            var seq = DOTween.Sequence().SetLink(img.gameObject);
            seq.AppendInterval(delay + 0.12f);
            seq.Append(img.DOFade(a, 0.04f));
            seq.AppendInterval(0.06f);
            seq.Append(img.DOFade(0f, 0.12f));
            seq.OnComplete(() => Destroy(img.gameObject));
        }
    }

    private void Hover(int i, bool on)
    {
        if (!_ready || _picked) return;
        var r = _rows[i];
        r.Root.DOKill();
        r.Root.DOAnchorPos(r.Home + (on ? new Vector2(18f, 0f) : Vector2.zero), 0.09f).SetEase(Ease.OutCubic).SetLink(r.Root.gameObject);
        SetShadow(r, on ? 14f : 10f, 1f);
        ApplyLook(r, on ? "Choice_Panel_Hover" : "Choice_Panel_Normal", on);
    }

    private void Pick(int i)
    {
        if (!_ready || _picked) return;
        _picked = true;
        var sel = _rows[i];
        sel.Root.DOKill();
        sel.Root.SetAsLastSibling();
        ApplyLook(sel, "Choice_Panel_Selected", true);
        sel.Root.DOAnchorPos(sel.Home + new Vector2(50f, 35f), 0.1f).SetEase(Ease.OutBack).SetLink(sel.Root.gameObject);
        sel.Root.DOScale(1.2f, 0.1f).SetEase(Ease.OutBack).SetLink(sel.Root.gameObject);
        SetShadow(sel, 18f, 1.2f);
        float bandW = sel.Width - 56f;
        ((RectTransform)sel.BandMask.GetChild(0)).sizeDelta = new Vector2(bandW, 3f);
        sel.BandMask.DOSizeDelta(new Vector2(bandW, 3f), 0.08f).SetDelay(0.04f).SetLink(sel.BandMask.gameObject);

        // 나머지 — 위에서부터 왼쪽으로 빠지며 반투명
        var outOffs = new[] { new Vector2(-95f, -12f), new Vector2(-145f, -24f) };
        var outAlpha = new[] { 0.3f, 0.25f };
        int k = 0;
        for (int j = 0; j < _rows.Count; j++)
        {
            if (j == i) continue;
            var r = _rows[j];
            r.Root.DOKill();
            ApplyLook(r, "Choice_Panel_Normal", false);
            SetShadow(r, 10f, 1f);
            r.Root.DOAnchorPos(r.Home + outOffs[Mathf.Min(k, 1)], 0.12f).SetEase(Ease.OutCubic).SetLink(r.Root.gameObject);
            r.Group.DOFade(outAlpha[Mathf.Min(k, 1)], 0.12f).SetLink(r.Root.gameObject);
            k++;
        }

        var group = gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = true;
        var exit = DOTween.Sequence().SetLink(gameObject);
        exit.AppendInterval(0.25f);
        if (_player != null)
        {
            // v2 — 선택지와 주인공이 함께 빠지고 어둠이 걷힌다
            var pr = _player.rectTransform;
            exit.Append(pr.DOAnchorPos(pr.anchoredPosition + new Vector2(240f, 0f), 0.2f).SetEase(Ease.InCubic));
            exit.Join(group.DOFade(0f, 0.2f));
        }
        else exit.Append(group.DOFade(0f, 0.1f));
        exit.OnComplete(() =>
        {
            var cb = _onPick;
            RestoreRaised();
            Destroy(gameObject);
            cb?.Invoke(i);
        });
    }

    // 판 · 글자 · 장식 색을 상태에 맞춘다
    private static void ApplyLook(Row r, string panel, bool dark)
    {
        r.Panel.sprite = Spr(panel);
        if (r.Tail != null) r.Tail.sprite = Spr(panel.Replace("Panel", "Tail"));
        r.Number.color = dark ? Color.white : Ink;
        r.Label.color = dark ? Color.white : Ink;
        r.Tag.color = dark ? TagHover : TagNormal;
        r.Arrow.color = dark ? Color.white : Ink;
    }

    // 화면 기준 (0.8d, d) 오른쪽 아래 오프셋을 줄 로컬 좌표로 (회전 · 확대를 거꾸로). 평소 d 10 = layout.json의 (8, 10)
    private static void SetShadow(Row r, float d, float scale)
    {
        var screen = new Vector2(d * 0.8f, -d);
        var local = (Vector2)(Quaternion.Euler(0, 0, -r.AngleZ) * screen) / scale;
        r.ShadowRt.DOKill();
        r.ShadowRt.DOAnchorPos(local, 0.09f).SetLink(r.ShadowRt.gameObject);
    }

    // ── 만들기 ──
    private static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    private static Image NewImage(string name, Transform parent, Sprite sprite, Color color, bool sliced = false)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        img.raycastTarget = false;
        return img;
    }

    private TextMeshProUGUI NewText(string name, Transform parent, string text, float size, Color color, TextAlignmentOptions align)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        if (_font != null) t.font = _font;
        t.text = text; t.fontSize = size; t.color = color;
        t.alignment = align == TextAlignmentOptions.Right ? TextAlignmentOptions.MidlineRight : TextAlignmentOptions.MidlineLeft;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.raycastTarget = false;
        return t;
    }

    // 1920×1080 화면 rect (좌상단 원점)
    private static void Place(RectTransform r, float x, float y, float w, float h)
    {
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1);
        r.sizeDelta = new Vector2(w, h);
        r.anchoredPosition = new Vector2(x, -y);
    }

    // 판 기준 로컬 rect. fromRight면 x는 오른쪽 끝에서 잰 거리 (요소 왼쪽 변까지)
    private static void PlaceLocal(RectTransform r, float x, float y, float w, float h, bool fromRight)
    {
        float ax = fromRight ? 1f : 0f;
        r.anchorMin = r.anchorMax = new Vector2(ax, 1);
        r.pivot = new Vector2(0, 1);
        r.sizeDelta = new Vector2(w, h);
        r.anchoredPosition = new Vector2(fromRight ? -x : x, -y);
    }

    private static void StretchFull(RectTransform r)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
        r.pivot = new Vector2(0.5f, 0.5f);
    }

    private static void FadeIn(Graphic g, float delay)
    {
        var c = g.color;
        g.color = new Color(c.r, c.g, c.b, 0f);
        g.DOFade(c.a, 0.15f).SetDelay(delay).SetLink(g.gameObject);
    }

    private static void AddTrigger(EventTrigger et, EventTriggerType type, Action a)
    {
        var e = new EventTrigger.Entry { eventID = type };
        e.callback.AddListener(_ => a());
        et.triggers.Add(e);
    }

    private static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;
}

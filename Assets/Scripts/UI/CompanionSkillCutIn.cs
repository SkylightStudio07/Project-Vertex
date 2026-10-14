using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using K = CompanionSkillUIKit;

// 동료 스킬 컷인 (LoL 전투사관학교 궁극기 구도). 기획: Docs/기획/동료_스킬.md
//   화면 왼쪽 위 삼각 영역에 동료 눈매가 사선 방향으로 밀려 들어오고(눈매는 반 박자 늦게 흐름),
//   경계선 바깥에 선을 따라 대사 태그 "ORDER · 스킬 ─ 대사"가 붙는다. 약 1.5초 뒤 같은 방향으로 빠진다.
//   인연 강화: 청록 이중 경계선 + Vertex Mark 워터마크 + 태그 위 청록 선.
// 여러 스킬을 연달아 쓰면 앞의 컷인이 빠진 뒤 다음 것이 들어온다. 입력은 막지 않는다. 게임은 멈추지 않는다.
// 좌표는 layout.json(1920×1080 좌상단) 그대로. 눈매 그림은 CompanionSkillData.cutinArt (규격 1920×912, 화면 rect [-80,-38,960,456]).
public class CompanionSkillCutIn : MonoBehaviour
{
    [SerializeField] private TMP_FontAsset font;
    [SerializeField] private int sortingOrder = 90; // 카드 컷인과 같은 층
    [SerializeField] private float holdSeconds = 1.5f;

    private const float EdgeDeg = 25.4077f;              // 사선 경계 각도 (Unity Z)
    private static readonly Vector2 Normal = new(0.4289f, -0.9034f); // 삼각 바깥(전투 쪽)으로 향하는 단위 벡터 (anchored 좌표)
    private static readonly Vector2 Along = new(0.9034f, 0.4289f);   // 경계선을 따라 오른쪽 위로

    private BattleManager _battle;
    private CanvasGroup _group;
    private RectTransform _tri, _eyeRt, _tag;
    private Image _eye, _edge, _edgeEnhanced, _watermark, _tagBack;
    private TextMeshProUGUI _skillText, _lineText;
    private RectTransform _divider;
    private Sequence _seq;
    private readonly Queue<CompanionSkillSlot> _queue = new();

    private void Awake()
    {
        var canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = sortingOrder;
        if (font == null) font = TMP_Settings.defaultFontAsset;
        _group = gameObject.GetComponent<CanvasGroup>();
        if (_group == null) _group = gameObject.AddComponent<CanvasGroup>();
        _group.blocksRaycasts = false;
        _group.interactable = false;
        Build();
        _group.alpha = 0f;
    }

    private void Start()
    {
        _battle = BattleManager.Instance;
        if (_battle != null) _battle.OnCompanionSkillUsed += Enqueue;
    }

    private void OnDestroy()
    {
        if (_battle != null) _battle.OnCompanionSkillUsed -= Enqueue;
    }

    private void Build()
    {
        var root = (RectTransform)transform;

        _tri = K.Group("Triangle", root, stretch: false);
        var mask = K.Pic(_tri, "Mask", "CutIn_Mask", 0, 0, 800, 380);
        mask.gameObject.AddComponent<Mask>().showMaskGraphic = false;
        _eye = K.Pic(mask.transform, "Eye", null, -80, -38, 960, 456);
        _eyeRt = _eye.rectTransform;
        _watermark = K.Pic(mask.transform, "Watermark", "CutIn_Watermark", 566, 4, 54, 54);
        _edge = K.Pic(_tri, "Edge", "CutIn_Edge", -16, -16, 832, 412);
        _edgeEnhanced = K.Pic(_tri, "EdgeEnhanced", "CutIn_Edge_Enhanced", -16, -16, 832, 412);

        _tag = K.Group("Tag", root, stretch: false);
        K.TopLeft(_tag, 120, 342, 570, 44);
        _tag.localEulerAngles = new Vector3(0f, 0f, EdgeDeg);
        _tagBack = K.Pic(_tag, "Back", "Tag_Back", 0, 0, 570, 44, sliced: true);
        K.Text(_tag, "Order", font, 30, 9, 52, 24, 13, K.Paper, TextAlignmentOptions.Left).text = "ORDER";
        _skillText = K.Text(_tag, "Skill", font, 88, 7, 58, 28, 20, K.Paper, TextAlignmentOptions.Left, FontStyles.Bold);
        _divider = K.Pic(_tag, "Divider", "Tag_Divider", 154, 22, 22, 1).rectTransform;
        _lineText = K.Text(_tag, "Line", font, 190, 8, 350, 26, 18, K.Paper, TextAlignmentOptions.Left);
    }

    private void Enqueue(CompanionSkillSlot slot)
    {
        if (slot?.Skill == null) return;
        _queue.Enqueue(slot);
        if (_seq == null || !_seq.IsActive()) PlayNext();
    }

    private void PlayNext()
    {
        if (_queue.Count == 0) return;
        Play(_queue.Dequeue());
    }

    private void Play(CompanionSkillSlot slot)
    {
        var skill = slot.Skill;
        bool enhanced = slot.Enhanced;

        _eye.sprite = skill.cutinArt;
        _eye.enabled = skill.cutinArt != null;
        _edge.gameObject.SetActive(!enhanced);
        _edgeEnhanced.gameObject.SetActive(enhanced);
        _watermark.gameObject.SetActive(enhanced);
        _tagBack.sprite = K.Load(enhanced ? "Tag_Back_Enhanced" : "Tag_Back");

        // 태그 글자: 스킬 이름 길이에 맞춰 구분선·대사를 밀고, 대사 길이에 맞춰 띠 폭을 정한다
        _skillText.text = skill.skillName;
        float skillW = Mathf.Max(40f, _skillText.GetPreferredValues(skill.skillName).x);
        _skillText.rectTransform.sizeDelta = new Vector2(skillW, 28);
        float divX = 88 + skillW + 8;
        _divider.anchoredPosition = new Vector2(divX, -22);
        string line = skill.CutinLineOf(enhanced);
        _lineText.text = line;
        float lineX = divX + 22 + 14;
        _lineText.rectTransform.anchoredPosition = new Vector2(lineX, -8);
        _divider.gameObject.SetActive(!string.IsNullOrEmpty(line));
        float lineW = string.IsNullOrEmpty(line) ? 0f : _lineText.GetPreferredValues(line).x;
        _tag.sizeDelta = new Vector2(Mathf.Max(300f, lineX + lineW + 40f), 44);
        _tagBack.rectTransform.sizeDelta = _tag.sizeDelta;

        // 움직임
        _seq?.Kill();
        Vector2 triIn = Vector2.zero, triOut = -Normal * 380f;
        Vector2 eyeBase = new(-80f, 38f);
        Vector2 tagBase = new(120f, -342f);
        _tri.anchoredPosition = triOut;
        _eyeRt.anchoredPosition = eyeBase - Normal * 70f;
        _tag.anchoredPosition = tagBase - Along * 140f;
        if (!_tag.TryGetComponent<CanvasGroup>(out var tagGroup)) tagGroup = _tag.gameObject.AddComponent<CanvasGroup>();
        tagGroup.alpha = 0f;
        _group.alpha = 1f;

        _seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject)
            .Append(_tri.DOAnchorPos(triIn, 0.24f).SetEase(Ease.OutCubic))
            .Insert(0.06f, _eyeRt.DOAnchorPos(eyeBase, 0.42f).SetEase(Ease.OutCubic))
            .Insert(0.48f, _eyeRt.DOAnchorPos(eyeBase + Along * 10f, holdSeconds).SetEase(Ease.Linear))
            .Insert(0.14f, _tag.DOAnchorPos(tagBase, 0.3f).SetEase(Ease.OutCubic))
            .Insert(0.14f, tagGroup.DOFade(1f, 0.2f))
            .Insert(0.24f + holdSeconds, _tri.DOAnchorPos(triOut, 0.26f).SetEase(Ease.InCubic))
            .Insert(0.24f + holdSeconds, tagGroup.DOFade(0f, 0.18f))
            .Insert(0.24f + holdSeconds, _tag.DOAnchorPos(tagBase + Along * 60f, 0.2f).SetEase(Ease.InCubic))
            .AppendCallback(() => { _group.alpha = 0f; _seq = null; PlayNext(); });
    }
}

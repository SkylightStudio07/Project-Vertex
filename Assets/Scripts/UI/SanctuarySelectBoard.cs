using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 성소 후보 선택 v3 (아트: ArtDirection/SanctuaryMockup/Redesign_v2, layout.json 1920×1080 좌상단 기준)
// 넓은 칸에는 언제나 지금 고른 후보가 선다. 다른 합류 가능 후보는 좁은 칸에 컬러로, 남는 칸은 잠긴 후보 실루엣.
// 좁은 칸의 후보를 누르면 넓은 칸과 자리를 바꾸고, 넓은 칸을 누르면 상세로 간다.
// 상세 · 합류 흐름은 SelectCoopCharUI가 그대로 맡는다.
public class SanctuarySelectBoard : MonoBehaviour
{
    [System.Serializable]
    public class Side
    {
        public SanctuarySlotHit hit;
        public Image character;
        public RectTransform characterArea;
        public TextMeshProUGUI number;
        [Tooltip("자물쇠 + \"잠긴 후보\"")]
        public GameObject lockGroup;
        public TextMeshProUGUI condition;
        [Tooltip("합류 가능 후보일 때: 이름 + 테마색 선 + 소속")]
        public GameObject nameGroup;
        public TextMeshProUGUI nameText;
        public Image nameRule;
        public TextMeshProUGUI factionText;
        [HideInInspector] public string charID;
    }

    [Header("넓은 칸 (고른 후보)")]
    [SerializeField] private SanctuarySlotHit mainHit;
    [SerializeField] private Image mainPanel;
    [SerializeField] private Sprite mainPanelNormal, mainPanelHover;
    [SerializeField] private Image mainEdge;
    [SerializeField] private Sprite mainEdgeNormal, mainEdgeHover;
    [SerializeField] private Image mainCharacter;
    [SerializeField] private RectTransform mainCharacterArea;
    [SerializeField] private TextMeshProUGUI mainNumber;
    [Tooltip("청록 점 + \"선택 중\"")]
    [SerializeField] private GameObject selectedMark;
    [SerializeField] private TextMeshProUGUI nameText, factionText, availabilityText;
    [SerializeField] private Image nameRule;
    [SerializeField] private string availableText = "합류 가능";
    [SerializeField] private string partyFullText = "동행 인원이 가득 찼습니다";

    [Header("좁은 칸")]
    [SerializeField] private List<Side> sides = new();

    [Header("잠긴 후보 · 고르지 않은 후보")]
    [SerializeField] private Material silhouetteMaterial;
    [SerializeField] private Color silhouetteColor = new(0.14f, 0.19f, 0.24f, 0.2f);
    [SerializeField] private string lockedHint = "구출 의뢰 · 구조 신호";
    [Tooltip("동행 인원이 가득 차 고를 수 없을 때 넓은 칸 그림 색")]
    [SerializeField] private Color unavailableTint = new(0.62f, 0.63f, 0.65f, 1f);
    [Tooltip("좁은 칸에 선 합류 가능 후보 (고르지 않음)")]
    [SerializeField] private Color sideTint = new(0.82f, 0.84f, 0.86f, 1f);
    [SerializeField] private Color sideHoverTint = Color.white;
    [Tooltip("좁은 칸 후보 그림 크기 (이름 블록이 칸 위쪽에 들어가도록 줄인다)")]
    [SerializeField, Range(0.5f, 1f)] private float sideArtScale = 0.8f;

    private readonly List<string> _candidates = new();
    private readonly List<CoopCharData> _locked = new();
    private SelectCoopCharUI _owner;
    private bool _partyFull;
    private int _selected = -1;

    public string SelectedCharID => _selected >= 0 && _selected < _candidates.Count ? _candidates[_selected] : null;
    public int Capacity => 1 + sides.Count;
    public int NumberOf(string charID) => Mathf.Max(0, _candidates.IndexOf(charID)) + 1;

    public void Show(SelectCoopCharUI owner, List<string> candidates, List<CoopCharData> lockedSources, bool partyFull)
    {
        _owner = owner;
        _partyFull = partyFull;
        _candidates.Clear();
        _candidates.AddRange(candidates);
        _locked.Clear();
        if (lockedSources != null) _locked.AddRange(lockedSources);
        _selected = _candidates.Count > 0 ? 0 : -1;
        SetMainHover(false);
        Refresh();
    }

    public void OnSlotClicked(SanctuarySlotHit hit)
    {
        if (hit == mainHit)
        {
            if (!_partyFull && SelectedCharID != null && _owner != null) _owner.OpenDetails(SelectedCharID);
            return;
        }
        var side = sides.Find(s => s.hit == hit);
        if (side == null || string.IsNullOrEmpty(side.charID)) return;
        _selected = _candidates.IndexOf(side.charID);
        Refresh();
        if (_owner != null) _owner.RefreshBoardSelection();
    }

    public void OnSlotHover(SanctuarySlotHit hit, bool on)
    {
        if (hit == mainHit) { SetMainHover(on && SelectedCharID != null && !_partyFull); return; }
        var side = sides.Find(s => s.hit == hit);
        if (side == null || string.IsNullOrEmpty(side.charID)) return;
        side.character.color = on ? sideHoverTint : sideTint;
    }

    private void SetMainHover(bool on)
    {
        if (mainPanel != null) mainPanel.sprite = on && mainPanelHover != null ? mainPanelHover : mainPanelNormal;
        if (mainEdge != null) mainEdge.sprite = on && mainEdgeHover != null ? mainEdgeHover : mainEdgeNormal;
    }

    private void Refresh()
    {
        // ── 넓은 칸 ──
        CoopCharData main = Data(SelectedCharID);
        mainNumber.text = $"{Mathf.Max(0, _selected) + 1:00}";
        if (selectedMark != null) selectedMark.SetActive(main != null && !_partyFull);
        nameText.text = main != null ? DisplayName(main) : "";
        factionText.text = main != null ? main.affiliation : "";
        Color theme = main != null ? main.themeColor : Color.clear;
        factionText.color = theme;
        if (nameRule != null) { nameRule.color = theme; nameRule.enabled = main != null; }
        availabilityText.text = main == null ? "" : _partyFull ? partyFullText : availableText;
        ShowArt(mainCharacter, mainCharacterArea, Art(main), false);
        if (main != null) mainCharacter.color = _partyFull ? unavailableTint : Color.white;
        if (mainHit != null) mainHit.Interactable = main != null && !_partyFull;

        // ── 좁은 칸: 고르지 않은 후보 → 잠긴 후보 ──
        var others = new List<string>();
        foreach (var id in _candidates) if (id != SelectedCharID) others.Add(id);
        int lockedIndex = 0;
        for (int i = 0; i < sides.Count; i++)
        {
            var side = sides[i];
            bool candidate = i < others.Count;
            int number = candidate ? _candidates.IndexOf(others[i]) + 1 : _candidates.Count + lockedIndex + 1;
            side.number.text = $"{number:00}";
            side.lockGroup.SetActive(!candidate);
            side.condition.gameObject.SetActive(!candidate);
            side.nameGroup.SetActive(candidate);

            if (candidate)
            {
                side.charID = others[i];
                var data = Data(side.charID);
                side.nameText.text = DisplayName(data);
                side.factionText.text = data != null ? data.affiliation : "";
                Color c = data != null ? data.themeColor : Color.white;
                side.factionText.color = c;
                side.nameRule.color = c;
                ShowArt(side.character, side.characterArea, Art(data), false, sideArtScale);
                side.character.color = sideTint;
            }
            else
            {
                side.charID = null;
                side.condition.text = lockedHint;
                CoopCharData source = lockedIndex < _locked.Count ? _locked[lockedIndex] : null;
                lockedIndex++;
                ShowArt(side.character, side.characterArea, Art(source), true);
            }
            if (side.hit != null) side.hit.Interactable = candidate && !_partyFull;
        }
    }

    private CoopCharData Data(string id)
        => string.IsNullOrEmpty(id) || CooperationManager.Instance == null ? null : CooperationManager.Instance.GetCoopCharData(id);

    private static string DisplayName(CoopCharData data)
        => data == null ? "" : string.IsNullOrWhiteSpace(data.charName) ? data.charID : data.charName;

    // 투명 배경이 보장된 전신 순서. 초상화(charImage)는 배경이 칠해진 그림일 수 있어 맨 뒤
    private static Sprite Art(CoopCharData data)
    {
        if (data == null) return null;
        if (data.sanctuarySelectionArt != null) return data.sanctuarySelectionArt;
        if (data.RosterFullArt != null) return data.RosterFullArt;
        if (data.standingSprite != null) return data.standingSprite;
        return data.charImage;
    }

    // 비율 유지로 칸 안에 담고 발을 칸 아래쪽에 붙인다 (기울이거나 자르지 않는다)
    private void ShowArt(Image image, RectTransform area, Sprite art, bool silhouette, float extraScale = 1f)
    {
        image.sprite = art;
        image.enabled = art != null;
        image.material = silhouette ? silhouetteMaterial : null;
        image.color = silhouette ? silhouetteColor : Color.white;
        if (art == null || area == null) return;

        Vector2 box = area.rect.size;
        float scale = Mathf.Min(box.x / art.rect.width, box.y / art.rect.height) * extraScale;
        RectTransform rt = image.rectTransform;
        rt.SetParent(area, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = art.rect.size * scale;
        rt.localScale = Vector3.one;
    }
}

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 훈련장 상대 목록 한 줄: 썸네일 · 이름 · 종류 배지 · 격퇴 점 3개. 잠긴 상대는 썸네일을 실루엣으로 칠한다.
// 일반 적은 한 마리 단위(왼쪽 클릭 추가 · 오른쪽 클릭 빼기), 엘리트 · 보스는 조우 단위로 묶는다.
public class TrainingTargetRow : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Button button;
    [SerializeField] private Image frame;
    [SerializeField] private UICroppedArt thumbnail;
    [SerializeField] private Image lockIcon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image typeBadge;
    [SerializeField] private TextMeshProUGUI typeLabel;
    [SerializeField] private List<Image> pips = new();
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("스프라이트")]
    [SerializeField] private Sprite frameNormal;
    [SerializeField] private Sprite frameHover;
    [SerializeField] private Sprite frameSelected;
    [SerializeField] private Sprite frameLocked;
    [SerializeField] private Sprite badgeNormal;
    [SerializeField] private Sprite badgeElite;
    [SerializeField] private Sprite badgeBoss;
    [SerializeField] private Sprite pipFilled;
    [SerializeField] private Sprite pipEmpty;

    [Header("잠김")]
    [SerializeField] private Material silhouetteMaterial;
    [SerializeField] private Color silhouetteColor = new(0.62f, 0.64f, 0.67f, 1f);
    [SerializeField] private Color inkColor = new(0.086f, 0.094f, 0.106f, 1f);
    [SerializeField] private Color lockedTextColor = new(0.55f, 0.57f, 0.6f, 1f);
    [Tooltip("썸네일 크롭 — 적 그림의 윗부분(머리)을 남긴다")]
    [SerializeField] private Vector2 thumbnailFocus = new(0.5f, 0.8f);
    [SerializeField, Min(1f)] private float thumbnailZoom = 1.15f;

    private Action _onClick;
    private Action _onRightClick;

    private void Awake() => button.onClick.AddListener(() => _onClick?.Invoke());

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right) _onRightClick?.Invoke();
    }

    public void Bind(EnemyEncounter encounter, string displayName, int defeats, int required, bool unlocked, bool selected, Action onClick)
        => BindCore(Thumbnail(encounter.enemies), encounter.encounterType, displayName, defeats, required, unlocked, selected, onClick, null);

    public void BindEnemy(EnemyData enemy, string displayName, int defeats, int required, bool unlocked, bool selected, Action onClick, Action onRightClick)
        => BindCore(Thumbnail(new[] { enemy }), EnemyEncounterType.Normal, displayName, defeats, required, unlocked, selected, onClick, onRightClick);

    private void BindCore(Sprite art, EnemyEncounterType type, string displayName, int defeats, int required, bool unlocked, bool selected, Action onClick, Action onRightClick)
    {
        _onClick = unlocked ? onClick : null;
        _onRightClick = unlocked ? onRightClick : null;

        frame.sprite = !unlocked ? frameLocked : selected ? frameSelected : frameNormal;
        var ss = button.spriteState;
        ss.highlightedSprite = unlocked && !selected ? frameHover : null;
        ss.pressedSprite = ss.highlightedSprite;
        button.spriteState = ss;
        button.interactable = unlocked;

        var thumbRaw = thumbnail.GetComponent<RawImage>();
        thumbnail.SetSprite(art, thumbnailFocus, thumbnailZoom);
        thumbRaw.material = unlocked ? null : silhouetteMaterial;
        thumbRaw.color = unlocked ? Color.white : silhouetteColor;
        lockIcon.enabled = !unlocked;

        nameText.text = displayName;
        nameText.color = unlocked ? inkColor : lockedTextColor;

        typeBadge.sprite = type switch
        {
            EnemyEncounterType.Elite => badgeElite,
            EnemyEncounterType.Boss  => badgeBoss,
            _                        => badgeNormal,
        };
        typeLabel.text = type switch
        {
            EnemyEncounterType.Elite => "엘리트",
            EnemyEncounterType.Boss  => "보스",
            _                        => "일반",
        };
        typeLabel.color = type == EnemyEncounterType.Normal ? inkColor : Color.white;

        // 점 개수 = 필요한 격퇴 수 (일반 3 · 엘리트 2 · 보스 1)
        for (int i = 0; i < pips.Count; i++)
        {
            pips[i].gameObject.SetActive(i < required);
            pips[i].sprite = i < defeats ? pipFilled : pipEmpty;
        }
        progressText.gameObject.SetActive(!unlocked);
        progressText.text = $"격퇴 {Mathf.Min(defeats, required)} / {required}";
    }

    // 첫 적 그림 (대기 시트만 있는 적은 첫 프레임)
    private static Sprite Thumbnail(IEnumerable<EnemyData> enemies)
    {
        if (enemies == null) return null;
        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;
            if (enemy.enemyImage != null) return enemy.enemyImage;
            if (enemy.idleFrames != null && enemy.idleFrames.Length > 0) return enemy.idleFrames[0];
        }
        return null;
    }
}

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using K = CompanionSkillUIKit;

// 진행 중인 의뢰 추적 (명일방주: 엔드필드 미션 추적 UI 문법). 전투 화면 왼쪽 위.
//   [STORY]                          ← 아주 작은 먹색 태그 칩
//   (◎) 블랙박스 회수                  ← 유형 아이콘 + 흰 제목. 이 줄 뒤에만 얇은 먹색 띠(오른쪽으로 사라짐)
//    ▫ 스카웃 유닛을 처치해 획득        ← 작은 빈 사각 + 흰 목표 문장, 핵심 단어만 청록
//      물품 확보 대기                   ← 진행도 (청록 작은 글자)
// 판은 없다. 제목 줄 뒤 얇은 먹색 띠, 목표 줄 뒤 더 옅은 띠(우리 전장은 밝아서), 글자에는 먹색 복사본 그림자.
// 의뢰마다 한 묶음씩 아래로 쌓는다. 진행 중인 의뢰가 없거나 런 밖(훈련장 등)이면 숨긴다.
// 처치 기록(BattleOperationFx)은 Bottom 아래로 내려 겹치지 않게 한다. 좌표는 1920×1080 좌상단 기준.
public class QuestTrackerView : MonoBehaviour
{
    [SerializeField] private TMP_FontAsset font;
    [SerializeField] private int sortingOrder = 4; // 전장 위, 손패(5)·컷인 아래
    [SerializeField] private Vector2 origin = new(96f, 108f); // 왼쪽 위 별 장식(SYSTEM ONLINE) 오른쪽
    [SerializeField] private Color bandColor = new(0.05f, 0.055f, 0.06f, 0.9f);   // 제목 줄 뒤 띠 (짙게)
    [SerializeField] private Color lineBandColor = new(0.05f, 0.055f, 0.06f, 0.42f); // 목표 줄 뒤 옅은 띠 (밝은 전장 위 가독성)
    [SerializeField] private Color textShadow = new(0.05f, 0.055f, 0.06f, 0.8f);

    private const float QuestGap = 22f;
    private static readonly Color White = new(0.96f, 0.97f, 0.98f, 1f);
    private const string Key = "#0DB8F2"; // 핵심 단어 강조 (청록)

    // 패널 맨 아래 y (1920×1080 좌상단 기준). 숨겨져 있으면 0
    public static float Bottom { get; private set; }

    private RectTransform _root;
    private Sprite _band, _titleBand;
    private string _lastKey;

    private void Awake()
    {
        var canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = sortingOrder;
        if (font == null) font = TMP_Settings.defaultFontAsset;
        _band = MakeBandSprite(0.45f);
        _titleBand = MakeBandSprite(0.6f); // 제목 띠는 짙은 구간을 더 길게
        _root = K.Group("TrackerRoot", transform, stretch: false);
        K.TopLeft(_root, origin.x, origin.y, 600, 0);
        _root.gameObject.SetActive(false);
    }

    private void OnDisable() => Bottom = 0f;

    private void Update()
    {
        var quests = Active();
        bool show = quests.Count > 0;
        if (_root.gameObject.activeSelf != show) _root.gameObject.SetActive(show);
        if (!show) { Bottom = 0f; _lastKey = null; return; }

        var qm = QuestManager.Instance;
        var sb = new System.Text.StringBuilder();
        foreach (var q in quests)
            sb.Append(q.questId).Append(qm.IsDoneThisRun(q)).Append(qm.HoldsQuestItem(q)).Append(qm.DefeatProgress(q)).Append('|');
        string key = sb.ToString();
        if (key == _lastKey) return;
        _lastKey = key;
        Rebuild(quests);
    }

    // 런 중이고(훈련장 제외) 수주한 의뢰
    private static List<QuestData> Active()
    {
        var list = new List<QuestData>();
        var qm = QuestManager.Instance;
        if (qm == null || !qm.IsRunActive || TrainingSession.IsActive) return list;
        foreach (var q in qm.Accepted) if (q != null) list.Add(q);
        return list;
    }

    private void Rebuild(List<QuestData> quests)
    {
        for (int i = _root.childCount - 1; i >= 0; i--) Destroy(_root.GetChild(i).gameObject);
        var qm = QuestManager.Instance;
        var skin = QuestRunSkin.Instance;
        float y = 0f;
        foreach (var q in quests)
        {
            bool done = qm.IsDoneThisRun(q);

            // 태그 칩
            string chip = q.tag == QuestTag.Story ? "STORY" : q.tag == QuestTag.Rescue ? "RESCUE" : "REQUEST";
            var chipBack = K.Pic(_root, "Chip", null, 0, y, 62, 14);
            chipBack.color = new Color(0.05f, 0.055f, 0.06f, 0.75f);
            var chipText = K.Text(chipBack.transform, "Label", font, 0, 0, 62, 14, 9, q.tag == QuestTag.None ? White : K.Accent, TextAlignmentOptions.Center, FontStyles.Bold);
            chipText.text = chip;
            chipText.characterSpacing = 4f;
            y += 20f;

            // 제목 줄: 뒤에 얇은 띠
            var band = K.Pic(_root, "Band", null, -8, y, 420, 34);
            band.sprite = _titleBand;
            band.color = bandColor;
            var icon = K.Pic(_root, "Icon", null, 2, y + 5, 24, 24);
            icon.sprite = skin != null ? skin.Tag(q.IconKind, true) : null;
            icon.enabled = icon.sprite != null;
            icon.preserveAspect = true;
            ShadowedText("Title", 36, y + 3, 380, 28, 21, done ? new Color(0.7f, 0.73f, 0.76f, 1f) : White,
                done ? $"{q.title}  <size=70%><color={Key}>달성</color></size>" : q.title, FontStyles.Normal);
            y += 42f;

            // 목표 줄
            foreach (var (text, progress, check) in Lines(q, qm, done))
            {
                float lineH = string.IsNullOrEmpty(progress) ? 25f : 47f;
                var lineBand = K.Pic(_root, "LineBand", null, -8, y - 2, 420, lineH);
                lineBand.sprite = _band;
                lineBand.color = lineBandColor;
                var box = K.Pic(_root, "Box", null, 10, y + 7, 10, 10);
                box.color = check ? K.Accent : White;
                if (!check)
                {
                    var hole = K.Pic(box.transform, "Hole", null, 2, 2, 6, 6);
                    hole.color = new Color(0.12f, 0.13f, 0.14f, 0.85f);
                }
                ShadowedText("Objective", 30, y, 420, 24, 17, check ? new Color(0.78f, 0.8f, 0.82f, 1f) : White, text, FontStyles.Normal);
                y += 25f;
                if (!string.IsNullOrEmpty(progress))
                {
                    ShadowedText("Progress", 30, y, 300, 20, 15, K.Accent, progress, FontStyles.Bold);
                    y += 22f;
                }
            }
            y += QuestGap;
        }
        Bottom = origin.y + y - QuestGap;
    }

    // (목표 문장, 진행도, 달성) — 핵심 단어는 청록
    private static List<(string, string, bool)> Lines(QuestData q, QuestManager qm, bool done)
    {
        var lines = new List<(string, string, bool)>();
        bool holding = qm.HoldsQuestItem(q);
        string item = q.questItem != null ? q.questItem.ItemName : "의뢰 물품";

        // 물품을 얻는 계기
        if (q.itemTrigger == QuestItemTrigger.DefeatEnemy && q.triggerEnemy != null)
            lines.Add(($"<color={Key}>{q.triggerEnemy.enemyName}</color>{Obj(q.triggerEnemy.enemyName)} 처치해 <color={Key}>{item}</color> 확보", holding || done ? "확보 완료" : null, holding || done));
        else if (q.itemTrigger == QuestItemTrigger.ReachFloor)
            lines.Add(($"<color={Key}>{q.triggerChapter}막 {q.triggerFloor + 1}층</color>에서 <color={Key}>{item}</color> 확보", holding || done ? "확보 완료" : null, holding || done));

        switch (q.goal)
        {
            case QuestGoal.HoldItemAtRunEnd:
                lines.Add(($"<color={Key}>{item}</color>{Obj(item)} 지닌 채 귀환", null, done));
                break;
            case QuestGoal.DeliverToNode:
                lines.Add(($"<color={Key}>{QuestData.NodeName(q.deliverNodeType)}</color>에 <color={Key}>{item}</color> 전달", null, done));
                break;
            case QuestGoal.DefeatCount:
                int n = Mathf.Min(qm.DefeatProgress(q), q.goalCount);
                string enemy = q.goalEnemy != null ? q.goalEnemy.enemyName : "대상";
                lines.Add(($"<color={Key}>{enemy}</color> 처치", $"{n} / {q.goalCount}", done || n >= q.goalCount));
                break;
        }
        return lines;
    }

    // 글자 + 아주 옅은 그림자(먹색 복사본을 1.5px 오른쪽 아래에)
    private void ShadowedText(string name, float x, float y, float w, float h, float size, Color color, string text, FontStyles style)
    {
        var shadow = K.Text(_root, name + "Shadow", font, x + 1.5f, y + 2f, w, h, size, textShadow, TextAlignmentOptions.Left, style);
        shadow.text = System.Text.RegularExpressions.Regex.Replace(text, "<color=[^>]*>|</color>", "");
        var t = K.Text(_root, name, font, x, y, w, h, size, color, TextAlignmentOptions.Left, style);
        t.text = text;
    }

    // 제목 줄 뒤 띠: 왼쪽이 짙고 오른쪽으로 사라짐, 위아래만 살짝 부드럽게
    private static Sprite MakeBandSprite(float fadeStart)
    {
        const int w = 128, h = 16;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
        var px = new Color32[w * h];
        for (int yy = 0; yy < h; yy++)
            for (int xx = 0; xx < w; xx++)
            {
                float u = xx / (w - 1f), v = yy / (h - 1f);
                float a = (1f - Edge(fadeStart, 1f, u)) * Edge(0f, 0.12f, v) * Edge(0f, 0.12f, 1f - v);
                px[yy * w + xx] = new Color32(255, 255, 255, (byte)(a * 255));
            }
        tex.SetPixels32(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
    }

    // 목적격 조사: 마지막 글자에 받침이 있으면 "을", 없으면 "를" (한글이 아니면 "을(를)")
    private static string Obj(string word)
    {
        if (string.IsNullOrEmpty(word)) return "을(를)";
        char c = word[word.Length - 1];
        if (c < '가' || c > '힣') return "을(를)";
        return (c - '가') % 28 != 0 ? "을" : "를";
    }

    // 경계 함수 (Mathf.SmoothStep은 두 값 사이 보간이라 다르다)
    private static float Edge(float e0, float e1, float x)
    {
        float t = Mathf.Clamp01((x - e0) / (e1 - e0));
        return t * t * (3f - 2f * t);
    }
}

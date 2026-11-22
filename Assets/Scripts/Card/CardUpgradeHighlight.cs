using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

// 강화된 카드에서 "강화로 달라진 부분"만 강조색으로 칠한다 (예: 사격해, 6 → 9 대미지).
// 설명문 템플릿은 기본/강화 공용이고 수치 토큰만 상태별로 바뀌므로, 강화 전·후 설명문을 조각 단위로 비교해
// 강화 후에만 있는 조각을 칠한다. 숫자는 조사와 따로 떼어 비교해서 "9만큼"이면 "9"만 칠해진다.
//
// 키워드(선천성·휘발성·소멸·보존·특효)는 CardData.GetDescription이 상태 플래그로 양쪽 설명에 자동으로 붙이므로
// 강화로 새로 생긴 키워드도 같은 비교로 칠해지고, 없어진 키워드는 강화 설명에 아예 나오지 않는다.
public static class CardUpgradeHighlight
{
    public const string Color = "#FF5247";

    public static string Describe(CardData card, BattleState state = null, EnemyInstance target = null)
    {
        if (card == null) return string.Empty;
        if (!card.isUpgraded) return card.GetFullDescription(state, target);

        string before = card.GetDescription(false, state, target);
        string after = card.GetDescription(true, state, target);
        return Diff(before, after);
    }

    public static string Wrap(string text) => $"<color={Color}>{text}</color>";

    // 조각: 숫자 덩어리 / 공백 / 쉼표·마침표 / 그 밖의 글자 덩어리
    // 구두점을 따로 떼어야 "선천성." → "선천성, 휘발성."에서 원래 있던 "선천성"이 칠해지지 않는다
    private static readonly Regex TokenRegex = new(@"\d+|\s+|[.,]|[^\d\s.,]+", RegexOptions.Compiled);

    private static List<string> Tokenize(string s)
    {
        var list = new List<string>();
        foreach (Match m in TokenRegex.Matches(s)) list.Add(m.Value);
        return list;
    }

    // 최장 공통 부분열(LCS)로 강화 후에만 있는 조각을 찾아 칠한다. 공백 조각은 칠하지 않는다.
    private static string Diff(string before, string after)
    {
        if (before == after) return after;
        var a = Tokenize(before);
        var b = Tokenize(after);
        int n = a.Count, m = b.Count;
        var dp = new int[n + 1, m + 1];
        for (int i = n - 1; i >= 0; i--)
            for (int j = m - 1; j >= 0; j--)
                dp[i, j] = a[i] == b[j] ? dp[i + 1, j + 1] + 1 : System.Math.Max(dp[i + 1, j], dp[i, j + 1]);

        var sb = new StringBuilder();
        int x = 0, y = 0;
        var pending = new StringBuilder(); // 연속으로 바뀐 조각은 한 번에 감싼다
        void Flush()
        {
            if (pending.Length == 0) return;
            string s = pending.ToString();
            string trimmed = s.TrimEnd();
            sb.Append(Wrap(trimmed)).Append(s.Substring(trimmed.Length));
            pending.Clear();
        }
        while (y < m)
        {
            if (x < n && a[x] == b[y]) { Flush(); sb.Append(b[y]); x++; y++; }
            else if (x < n && dp[x + 1, y] >= dp[x, y + 1]) { x++; }
            else
            {
                if (pending.Length == 0 && string.IsNullOrWhiteSpace(b[y])) sb.Append(b[y]);
                else pending.Append(b[y]);
                y++;
            }
        }
        Flush();
        return sb.ToString();
    }
}

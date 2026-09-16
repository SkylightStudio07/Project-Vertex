using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

// Read-only view of runtime statuses and player card piles. Never invokes passive/preview hooks.
public sealed class CombatStatusWindow : EditorWindow
{
    private const double RefreshInterval = 0.2;
    [SerializeField] private string search = "";
    [SerializeField] private int dispositionFilter;
    [SerializeField] private bool showDead = true;
    [SerializeField] private bool showEmpty = true;
    [SerializeField] private Vector2 scroll;
    [SerializeField] private bool showDrawPile = true;
    [SerializeField] private bool showDiscardPile = true;
    [SerializeField] private Vector2 drawPileScroll;
    [SerializeField] private Vector2 discardPileScroll;

    private readonly HashSet<StatusInstance> expandedStatuses = new();
    private readonly HashSet<ICombatant> collapsedCombatants = new();
    private readonly List<CombatantRow> combatants = new();
    private readonly List<string> drawPileCards = new();
    private readonly List<string> discardPileCards = new();
    private BattleState observedState;
    private bool refreshRequested = true;
    private double nextRefresh;
    private string battleSummary = "전투 데이터를 기다리는 중입니다.";
    private string notice;

    [MenuItem("Tools/Combat/Live Status Monitor")]
    public static void Open()
    {
        var window = GetWindow<CombatStatusWindow>("실시간 상태");
        window.minSize = new Vector2(780, 420);
        window.Show();
    }

    private void OnEnable()
    {
        minSize = new Vector2(780, 420);
        EditorApplication.update += Tick;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        refreshRequested = true;
    }

    private void OnDisable()
    {
        EditorApplication.update -= Tick;
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        ClearSnapshot();
    }

    private void OnPlayModeChanged(PlayModeStateChange change)
    {
        ClearSnapshot();
        refreshRequested = true;
        Repaint();
    }

    private void Tick()
    {
        if (EditorApplication.timeSinceStartup < nextRefresh) return;
        nextRefresh = EditorApplication.timeSinceStartup + RefreshInterval;
        refreshRequested = true;
        Repaint();
    }

    private void ClearSnapshot()
    {
        observedState = null;
        combatants.Clear();
        drawPileCards.Clear();
        discardPileCards.Clear();
        expandedStatuses.Clear();
        collapsedCombatants.Clear();
    }

    private void OnGUI()
    {
        // Keep the same row structure between Layout and Repaint, even when a turn removes statuses.
        if (Event.current.type == EventType.Layout && refreshRequested)
        {
            CaptureSnapshot();
            refreshRequested = false;
        }

        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            GUILayout.Label("상태 검색", GUILayout.Width(60));
            search = GUILayout.TextField(search ?? "", EditorStyles.toolbarSearchField, GUILayout.MinWidth(140));
            dispositionFilter = EditorGUILayout.Popup(dispositionFilter, new[] { "전체 종류", "버프", "디버프", "중립" }, GUILayout.Width(90));
            showDead = GUILayout.Toggle(showDead, "사망 포함", EditorStyles.toolbarButton, GUILayout.Width(76));
            showEmpty = GUILayout.Toggle(showEmpty, "빈 목록 포함", EditorStyles.toolbarButton, GUILayout.Width(88));
            if (GUILayout.Button("모두 펼치기", EditorStyles.toolbarButton, GUILayout.Width(82)))
            {
                collapsedCombatants.Clear();
                foreach (var actor in combatants)
                    foreach (var status in actor.Statuses) expandedStatuses.Add(status.Instance);
            }
            if (GUILayout.Button("접기", EditorStyles.toolbarButton, GUILayout.Width(42))) expandedStatuses.Clear();
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField(battleSummary, EditorStyles.boldLabel);
        EditorGUILayout.LabelField("읽기 전용 · 0.2초 간격 갱신 · 에디터 일시정지 중에도 조회 가능", EditorStyles.miniLabel);
        if (!string.IsNullOrEmpty(notice)) EditorGUILayout.HelpBox(notice, MessageType.Info);
        if (observedState == null) return;

        using (var view = new EditorGUILayout.ScrollViewScope(scroll))
        {
            scroll = view.scrollPosition;
            EditorGUILayout.LabelField("플레이어 카드 더미", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawCardPile("뽑기 더미", drawPileCards, ref showDrawPile, ref drawPileScroll);
                DrawCardPile("버리기 더미", discardPileCards, ref showDiscardPile, ref discardPileScroll);
            }
            EditorGUILayout.Space(6);
            int visible = 0;
            foreach (var actor in combatants)
            {
                if (!showDead && actor.Dead) continue;
                if (!showEmpty && actor.Statuses.Count == 0 && actor.OtherPassives.Count == 0) continue;
                var rows = actor.Statuses.FindAll(row => Matches(actor.Name, row));
                bool filtering = !string.IsNullOrWhiteSpace(search) || dispositionFilter != 0;
                if (filtering && rows.Count == 0) continue;
                DrawCombatant(actor, rows);
                visible++;
            }
            if (visible == 0) EditorGUILayout.HelpBox("현재 검색·필터에 해당하는 전투원/상태가 없습니다.", MessageType.None);
        }
    }

    private void CaptureSnapshot()
    {
        var manager = EditorApplication.isPlaying ? BattleManager.Instance : null;
        var state = manager != null ? manager.State : null;
        if (!ReferenceEquals(observedState, state)) ClearSnapshot();
        observedState = state;
        combatants.Clear();
        drawPileCards.Clear();
        discardPileCards.Clear();
        if (state == null)
        {
            battleSummary = EditorApplication.isPlaying ? "전투 데이터를 기다리는 중입니다." : "플레이 모드에서 사용할 수 있습니다.";
            notice = "게임을 실행하고 전투에 진입하면 플레이어와 각 적의 현재 Status가 표시됩니다.";
            return;
        }

        string phase = state.Phase == BattlePhase.PlayerTurn ? "플레이어 턴" : "적 턴";
        battleSummary = $"턴 {state.TurnNumber} · {phase} · 에너지 {state.Energy}/{state.MaxEnergy} · 탄약 {state.Ammo}";
        if (EditorApplication.isPaused) battleSummary += " · 일시정지";
        notice = manager.IsInBattle ? null : "전투가 진행 중이 아닙니다. BattleManager에 남아 있는 마지막 전투 상태를 표시합니다.";

        CaptureCardPile(state.DrawPile, drawPileCards);
        CaptureCardPile(state.DiscardPile, discardPileCards);

        if (state.Player != null) combatants.Add(CaptureCombatant(state.Player, "플레이어", state));
        if (state.Enemies != null)
            for (int i = 0; i < state.Enemies.Count; i++)
            {
                var enemy = state.Enemies[i];
                if (enemy != null) combatants.Add(CaptureCombatant(enemy, EnemyName(enemy, i), state));
            }

        // Drop expansion keys for expired statuses instead of retaining runtime instances indefinitely.
        var liveStatuses = new HashSet<StatusInstance>();
        var liveActors = new HashSet<ICombatant>();
        foreach (var actor in combatants)
        {
            liveActors.Add(actor.Combatant);
            foreach (var status in actor.Statuses) liveStatuses.Add(status.Instance);
        }
        expandedStatuses.RemoveWhere(status => !liveStatuses.Contains(status));
        collapsedCombatants.RemoveWhere(actor => !liveActors.Contains(actor));
    }

    private static void CaptureCardPile(IReadOnlyList<CardData> pile, List<string> rows)
    {
        if (pile == null) return;
        foreach (var card in pile)
        {
            if (card == null) { rows.Add("(카드 참조 없음)"); continue; }
            string name = string.IsNullOrWhiteSpace(card.CardName) ? card.name : card.CardName;
            rows.Add(card.isUpgraded ? name + " [강화]" : name);
        }
    }

    private static void DrawCardPile(string title, List<string> cards, ref bool expanded, ref Vector2 pileScroll)
    {
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.ExpandWidth(true)))
        {
            expanded = EditorGUILayout.Foldout(expanded, $"{title} · {cards.Count}장", true, EditorStyles.foldoutHeader);
            if (!expanded) return;
            if (cards.Count == 0)
            {
                EditorGUILayout.LabelField("비어 있습니다.", EditorStyles.centeredGreyMiniLabel);
                return;
            }
            float height = Mathf.Min(180, cards.Count * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) + 6);
            using (var view = new EditorGUILayout.ScrollViewScope(pileScroll, GUILayout.Height(height)))
            {
                pileScroll = view.scrollPosition;
                foreach (string name in cards)
                    EditorGUILayout.LabelField(new GUIContent(name, name));
            }
        }
    }

    private static CombatantRow CaptureCombatant(ICombatant actor, string name, BattleState state)
    {
        // Player HP delegates to GameManager; it may already have been destroyed during a scene change.
        bool hasHp = actor is PlayerCombatant ? GameManager.Instance != null : actor is not EnemyInstance enemy || enemy.Data != null;
        var row = new CombatantRow
        {
            Combatant = actor, Name = name, Block = actor.Block,
            HpLabel = hasHp ? $"HP {actor.HP}/{actor.MaxHP}" : "HP 조회 불가",
            Dead = hasHp && actor.IsDead
        };
        var entries = actor.Statuses?.Entries;
        if (entries == null) return row;
        foreach (var passive in entries)
        {
            if (passive is not StatusInstance status)
            {
                row.OtherPassives.Add(passive == null ? "빈 패시브 참조" : passive.GetType().Name);
                continue;
            }
            var definition = status.Definition;
            var behaviors = new StringBuilder();
            if (definition != null && definition.Behaviors != null)
                foreach (var behavior in definition.Behaviors)
                {
                    if (behaviors.Length > 0) behaviors.AppendLine();
                    if (behavior == null) { behaviors.Append("빈 Behavior 참조"); continue; }
                    behaviors.Append(ObjectNames.NicifyVariableName(behavior.GetType().Name));
                    if (behavior is DamageModifierStatusBehavior modifier && modifier.oncePerTurn)
                        behaviors.Append(status.WasUsedThisTurn(behavior) ? " — 이번 턴 사용 완료" : " — 이번 턴 미사용");
                }
            row.Statuses.Add(new StatusRow
            {
                Instance = status, Definition = definition,
                Name = definition != null ? definition.DisplayName : "정의 누락 (StatusInstance)",
                Id = definition != null ? definition.Id : "", Description = definition != null ? definition.Description : "",
                Stacks = status.Stacks, Potency = status.Potency, SecondaryPotency = status.SecondaryPotency,
                Disposition = definition != null ? definition.GetDisposition(status.Stacks) : StatusDisposition.Neutral,
                Duration = definition != null ? DurationLabel(definition.DurationPolicy) : "정의 누락",
                MaxStacks = definition != null ? definition.MaxStacks : 0,
                StackPolicy = definition != null ? definition.StackPolicy.ToString() : "—",
                Dispellable = definition != null && definition.Dispellable,
                Source = SourceName(status.Source, state), Behaviors = behaviors.ToString()
            });
        }
        return row;
    }

    private void DrawCombatant(CombatantRow actor, List<StatusRow> rows)
    {
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            string title = $"{actor.Name}{(actor.Dead ? " [사망]" : "")}  ·  {actor.HpLabel}  ·  방어도 {actor.Block}  ·  상태 {actor.Statuses.Count}개";
            bool expanded = !collapsedCombatants.Contains(actor.Combatant);
            expanded = EditorGUILayout.Foldout(expanded, title, true, EditorStyles.foldoutHeader);
            if (expanded) collapsedCombatants.Remove(actor.Combatant);
            else { collapsedCombatants.Add(actor.Combatant); return; }

            if (rows.Count == 0) EditorGUILayout.LabelField("현재 적용된 상태가 없습니다.", EditorStyles.centeredGreyMiniLabel);
            foreach (var row in rows) DrawStatus(row);
            if (actor.OtherPassives.Count > 0)
                EditorGUILayout.HelpBox("Status 외 패시브 (스택 정보 없음): " + string.Join(", ", actor.OtherPassives), MessageType.None);
        }
        EditorGUILayout.Space(4);
    }

    private void DrawStatus(StatusRow row)
    {
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                bool expanded = expandedStatuses.Contains(row.Instance);
                expanded = EditorGUILayout.Foldout(expanded, new GUIContent(row.Name, row.Description), true);
                if (expanded) expandedStatuses.Add(row.Instance); else expandedStatuses.Remove(row.Instance);
                var style = new GUIStyle(EditorStyles.boldLabel);
                style.normal.textColor = row.Disposition switch
                {
                    StatusDisposition.Buff => new Color(0.25f, 0.72f, 0.4f),
                    StatusDisposition.Debuff => new Color(0.95f, 0.4f, 0.35f),
                    _ => EditorStyles.label.normal.textColor
                };
                GUILayout.Label(DispositionLabel(row.Disposition), style, GUILayout.Width(54));
                GUILayout.Label($"스택 {row.Stacks}", EditorStyles.boldLabel, GUILayout.Width(100));
                GUILayout.Label(row.Duration, GUILayout.Width(142));
                using (new EditorGUI.DisabledScope(row.Definition == null))
                    if (GUILayout.Button("원본 선택", GUILayout.Width(72)))
                    {
                        Selection.activeObject = row.Definition;
                        EditorGUIUtility.PingObject(row.Definition);
                    }
            }
            if (row.Stacks == 0) EditorGUILayout.LabelField("스택 0 · 만료/제거 대기", EditorStyles.miniLabel);
            if (!expandedStatuses.Contains(row.Instance)) return;

            EditorGUILayout.LabelField("ID", string.IsNullOrEmpty(row.Id) ? "(미지정)" : row.Id);
            EditorGUILayout.LabelField("설명", EditorStyles.miniBoldLabel);
            GUILayout.Label(string.IsNullOrWhiteSpace(row.Description) ? "(작성된 설명 없음)" : row.Description, EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("위력", $"Potency {row.Potency}  /  SecondaryPotency {row.SecondaryPotency}");
            EditorGUILayout.LabelField("적용 출처", row.Source);
            EditorGUILayout.LabelField("스택 규칙", $"{row.StackPolicy} · 최대 {(row.MaxStacks > 0 ? row.MaxStacks.ToString() : "제한 없음")} · {(row.Dispellable ? "해제 가능" : "해제 불가")}");
            EditorGUILayout.LabelField("Behavior", EditorStyles.miniBoldLabel);
            GUILayout.Label(string.IsNullOrEmpty(row.Behaviors) ? "(없음)" : row.Behaviors, EditorStyles.wordWrappedLabel);
        }
    }

    private bool Matches(string actor, StatusRow status)
    {
        if (dispositionFilter == 1 && status.Disposition != StatusDisposition.Buff) return false;
        if (dispositionFilter == 2 && status.Disposition != StatusDisposition.Debuff) return false;
        if (dispositionFilter == 3 && status.Disposition != StatusDisposition.Neutral) return false;
        string term = (search ?? "").Trim();
        return term.Length == 0 || Contains(actor, term) || Contains(status.Name, term) || Contains(status.Id, term)
            || Contains(status.Description, term) || Contains(status.Behaviors, term);
    }

    private static bool Contains(string text, string term) => text?.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
    private static string EnemyName(EnemyInstance enemy, int index)
    {
        string name = enemy.Data != null ? enemy.Data.enemyName : null;
        if (string.IsNullOrWhiteSpace(name)) name = enemy.Data != null ? enemy.Data.name : "정의 없는 적";
        return $"적 #{index + 1} · {name}";
    }
    private static string SourceName(ICombatant source, BattleState state)
    {
        if (source == null) return "없음 / 미기록";
        if (ReferenceEquals(source, state.Player)) return "플레이어";
        if (source is EnemyInstance enemy)
        {
            int index = state.Enemies?.IndexOf(enemy) ?? -1;
            return index >= 0 ? EnemyName(enemy, index) : $"{enemy.Data?.enemyName ?? "적"} (현재 전투 목록 밖)";
        }
        return source.GetType().Name;
    }
    private static string DispositionLabel(StatusDisposition disposition) => disposition switch
    {
        StatusDisposition.Buff => "버프", StatusDisposition.Debuff => "디버프",
        StatusDisposition.Neutral => "중립", _ => disposition.ToString()
    };
    private static string DurationLabel(StatusDurationPolicy duration) => duration switch
    {
        StatusDurationPolicy.DecreaseOnTurnStart => "턴 시작: 0 쪽으로 1",
        StatusDurationPolicy.DecreaseOnTurnEnd => "턴 종료: 0 쪽으로 1",
        StatusDurationPolicy.Permanent => "턴 감소 없음",
        StatusDurationPolicy.ConsumeOnly => "발동 시 소비",
        _ => duration.ToString()
    };

    private sealed class CombatantRow
    {
        internal ICombatant Combatant;
        internal string Name, HpLabel;
        internal int Block;
        internal bool Dead;
        internal readonly List<StatusRow> Statuses = new();
        internal readonly List<string> OtherPassives = new();
    }
    private sealed class StatusRow
    {
        internal StatusInstance Instance;
        internal StatusDefinition Definition;
        internal string Name, Id, Description, Duration, StackPolicy, Source, Behaviors;
        internal int Stacks, Potency, SecondaryPotency, MaxStacks;
        internal bool Dispellable;
        internal StatusDisposition Disposition;
    }
}

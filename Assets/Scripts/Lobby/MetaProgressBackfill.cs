// 재화·거점 개발도(MetaProgress)가 생기기 전의 기록을 개발도에 한 번 소급한다. 로비 진입 때 한 번(LobbyManager).
//   - 동료: 호감도 Lv.1 이상 = 합류한 적 있음 → 첫 합류, Lv.2~ → 랭크업
//   - 막 클리어 플래그(actN_cleared) → 막 보스 첫 격파 (아티팩트·상점 재고 포함)
//   - 완료한 스토리·구출 의뢰 → 개발도 + 아티팩트
// 마테리얼 편(격퇴 수)은 적 등급을 알아야 해서 소급하지 않는다. 이후 격퇴부터 센다.
public static class MetaProgressBackfill
{
    private const string Key = "backfill:v1";

    public static void Run()
    {
        if (MetaProgress.HasMilestone(Key)) return;

        foreach (var (id, level) in PlayerRecord.AllCoop())
        {
            if (level < 1) continue;
            MetaProgress.AwardDevelopment("join:" + id, MetaProgress.DevJoin);
            for (int l = 2; l <= level; l++) MetaProgress.AwardDevelopment($"rank:{id}:{l}", MetaProgress.DevRankUp);
        }

        var flags = BlessingAffinityManager.Instance;
        if (flags != null)
            for (int act = 1; act <= 4; act++)
                if (flags.HasFlag($"act{act}_cleared")) MetaProgress.OnActBossCleared(act);

        var quests = QuestManager.Instance;
        if (quests != null)
            foreach (var q in quests.History())
                if (q != null && q.tag != QuestTag.None
                    && MetaProgress.AwardDevelopment("quest:" + q.questId, MetaProgress.DevStoryQuest))
                    MetaProgress.AddArtifacts(1);

        MetaProgress.MarkMilestone(Key);
    }
}

using System.Collections.Generic;
using UnityEngine;

// 게임에 존재하는 모든 의뢰 목록. Resources/QuestDatabase.asset 하나만 둔다 (로비·런 씬 어디서든 불러오도록).
[CreateAssetMenu(fileName = "QuestDatabase", menuName = "Game Asset/Quest Database")]
public class QuestDatabase : ScriptableObject
{
    public List<QuestData> quests = new();

    public QuestData Find(string questId) => quests.Find(q => q != null && q.questId == questId);
}

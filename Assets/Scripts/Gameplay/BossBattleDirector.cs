using System;
using System.Collections;
using UnityEngine;

// 보스전 진입 연출: 등장 배너("N막 보스 / 이름") → 스토리 진행도에 맞는 등장 대사 → 첫 턴 시작.
// GameManager가 보스 노드 전투를 시작할 때 PlayIntro를 부르고, 끝나면 onDone으로 첫 턴을 연다.
// 페이즈 전환 연출은 적마다 EnemyView가, 격파 후 막 클리어 흐름은 GameManager.CompleteAct가 맡는다.
public class BossBattleDirector : MonoBehaviour
{
    public static BossBattleDirector Instance { get; private set; }

    [SerializeField] private ActTitleBanner banner;     // 막 이름 배너를 같이 쓴다
    [SerializeField] private DialogueView dialogueView;
    [Tooltip("배너가 떠오른 뒤 대사로 넘어가기까지 (배너가 사라지는 중에 대사가 시작된다)")]
    [SerializeField, Min(0f)] private float bannerLeadTime = 2.4f;

    private void Awake() => Instance = this;

    public void PlayIntro(EnemyEncounter encounter, Action onDone)
    {
        StopAllCoroutines();
        StartCoroutine(IntroRoutine(encounter, onDone));
    }

    private IEnumerator IntroRoutine(EnemyEncounter encounter, Action onDone)
    {
        if (banner != null && encounter != null)
        {
            int chapter = GameManager.Instance != null ? GameManager.Instance.Chapter : 1;
            string label = !string.IsNullOrWhiteSpace(encounter.bossLabel) ? encounter.bossLabel : $"{chapter}막 보스";
            string title = !string.IsNullOrWhiteSpace(encounter.bossTitle) ? encounter.bossTitle
                         : encounter.enemies != null && encounter.enemies.Count > 0 && encounter.enemies[0] != null ? encounter.enemies[0].enemyName
                         : null;
            if (!string.IsNullOrWhiteSpace(title))
            {
                banner.PlayText(label, title);
                yield return new WaitForSeconds(bannerLeadTime);
            }
        }

        var dialogue = encounter != null ? StoryDialogue.Pick(encounter.introDialogues) : null;
        if (dialogue != null && dialogueView != null)
        {
            bool finished = false;
            dialogueView.Play(dialogue.dialogueJson, () => finished = true);
            while (!finished) yield return null;
            dialogue.MarkPlayed();
        }

        onDone?.Invoke();
    }

    // 씬을 다시 불러와도(재출정) 파괴된 이전 씬의 뷰가 Instance로 남지 않게 (비활성으로 시작하면 Awake가 늦게 돈다)
    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}

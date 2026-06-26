using System.Collections.Generic;
using UnityEngine;

// 휴식 노드 UI 컨트롤러
public class RestView : MonoBehaviour
{
    [Header("복귀")]
    [SerializeField] private MapUIController mapUIController;

    [Header("행동")]
    [SerializeField] private HealEffect restHealEffect;        // 휴식 회복용 효과 SO


    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void OnRestClicked()
    {
        if (restHealEffect != null)
        {
            restHealEffect.Execute(new CardContext());
        }
        Finish();
    }

    private void Finish()
    {
        gameObject.SetActive(false);
        mapUIController.OpenMap();
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;

public class FadeController : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    private GameObject currentSelectedObj;

    [SerializeField] private PlayableDirector fadeDirector;
    [SerializeField] private PlayableAsset fadeInAsset;
    [SerializeField] private PlayableAsset fadeOutAsset;

    // fadeIn 실행 메소드
    public void FadeIn()
    {
        if (fadeInAsset != null)
        {
            fadeDirector.playableAsset = fadeInAsset;
            fadeDirector.Play();
        }
    }

    // fadeOut 실행 메소드
    public void FadeOut()
    {
        if (fadeOutAsset != null)
        {
            fadeDirector.playableAsset = fadeOutAsset;
            fadeDirector.Play();
        }
    }

    // Fade 연출이 시작될 때 상호작용을 차단하는 메소드
    public void StartFade()
    {
        currentSelectedObj = EventSystem.current.currentSelectedGameObject;
        fadeCanvasGroup.interactable = false;
    }

    // Fade 연출이 끝날 때 상호작용을 재개하는 메소드
    public void EndFade()
    {
        EventSystem.current.SetSelectedGameObject(currentSelectedObj);
        currentSelectedObj = null;
        fadeCanvasGroup.interactable = true;
    }

}

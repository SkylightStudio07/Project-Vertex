using UnityEngine;
using UnityEngine.UI;

// 스프라이트 리그(SpriteLab RiggingV2 등 SpriteRenderer + 뼈대)를 UI에 보여주는 RawImage.
// 화면 밖에 둔 리그를 전용 카메라가 RenderTexture에 그리고, 이 RawImage가 그 텍스처를 표시한다.
// UI 배치·버튼 클릭·패널 페이드를 그대로 쓸 수 있고, 이 UI가 보일 때만 카메라와 리그를 켠다.
[RequireComponent(typeof(RawImage))]
public class RigPortraitView : MonoBehaviour
{
    [SerializeField] private Camera rigCamera;
    [SerializeField] private GameObject rigRoot;

    private void OnEnable() => SetRigActive(true);
    private void OnDisable() => SetRigActive(false);

    private void SetRigActive(bool active)
    {
        if (rigCamera != null) rigCamera.enabled = active;
        if (rigRoot != null) rigRoot.SetActive(active);
    }
}

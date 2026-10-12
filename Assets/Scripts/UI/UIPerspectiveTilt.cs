using UnityEngine;
using UnityEngine.InputSystem;

// 명일방주 로비 오른쪽 메뉴 같은 2.5D 기울기. 붙인 RectTransform(메뉴 묶음)을 Y·X축으로 살짝 돌리고,
// 마우스 위치를 따라 각도를 조금 더 흔들어 시차를 준다.
// 원근이 보이려면 캔버스가 Screen Space - Camera(원근 카메라)여야 한다. 오버레이 캔버스에서는 납작하게만 돈다.
// 클릭 판정은 GraphicRaycaster가 기울어진 면 그대로 계산하므로 따로 손볼 것이 없다.
[RequireComponent(typeof(RectTransform))]
public class UIPerspectiveTilt : MonoBehaviour
{
    [Tooltip("기본 각도. y가 음수면 오른쪽이 안쪽으로 물러난다")]
    [SerializeField] private Vector2 baseAngle = new(2f, -14f);       // (x, y)
    [Tooltip("마우스가 화면 끝에 있을 때 더해지는 각도")]
    [SerializeField] private Vector2 parallaxAngle = new(1.5f, 2.5f);
    [SerializeField, Min(0f)] private float smoothing = 6f;
    [SerializeField] private bool useUnscaledTime = true;

    private RectTransform _rect;
    private Vector2 _current;

    private void Awake() => _rect = (RectTransform)transform;

    private void OnEnable()
    {
        _current = baseAngle;
        Apply();
    }

    private void OnDisable() => _rect.localRotation = Quaternion.identity;

    private void LateUpdate()
    {
        Vector2 target = baseAngle;
        if (parallaxAngle != Vector2.zero && Mouse.current != null && Screen.width > 0 && Screen.height > 0)
        {
            // -1 ~ 1 (화면 가운데 0). 마우스가 오른쪽에 가면 메뉴가 그쪽으로 조금 돌아본다
            Vector2 m = Mouse.current.position.ReadValue();
            float nx = Mathf.Clamp(m.x / Screen.width * 2f - 1f, -1f, 1f);
            float ny = Mathf.Clamp(m.y / Screen.height * 2f - 1f, -1f, 1f);
            target += new Vector2(-ny * parallaxAngle.x, nx * parallaxAngle.y);
        }
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        _current = smoothing > 0f ? Vector2.Lerp(_current, target, 1f - Mathf.Exp(-smoothing * dt)) : target;
        Apply();
    }

    private void Apply() => _rect.localRotation = Quaternion.Euler(_current.x, _current.y, 0f);
}

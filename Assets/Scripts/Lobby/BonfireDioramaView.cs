using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the static Bonfire diorama objects placed in the scene.
/// The room, camera, and sprite planes are authored in Lobby.unity; this component only
/// toggles that world with the Bonfire UI and animates the vortex layer.
/// </summary>
public class BonfireDioramaView : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private GameObject worldRoot;
    [SerializeField] private Camera dioramaCamera;
    [SerializeField] private Renderer vortexLoopRenderer;
    [SerializeField] private Transform vortexLoopTransform;
    [SerializeField] private Transform bonfireTransform;
    [SerializeField] private SpriteRenderer bonfireRenderer;

    [Header("Vortex Loop")]
    [SerializeField] private Vector2 vortexTextureScale = new Vector2(1.25f, 1.25f);
    [SerializeField] private Vector2 vortexScrollSpeed = new Vector2(0.018f, 0.045f);
    [SerializeField, Range(0f, 0.2f)] private float vortexPulseAmount = 0f;
    [SerializeField, Min(0f)] private float vortexPulseSpeed = 0.8f;
    [SerializeField] private float vortexRotationSpeed = 24f;

    [Header("Bonfire Loop")]
    [SerializeField, Min(0f)] private float bonfireFlickerSpeed = 8f;
    [SerializeField, Range(0f, 0.2f)] private float bonfireFlickerScale = 0.04f;
    [SerializeField, Range(0f, 0.4f)] private float bonfireFlickerAlpha = 0.12f;

    private Image hostBackground;
    private Material vortexLoopMaterial;
    private Vector3 vortexLoopBaseScale;
    private Quaternion vortexLoopBaseRotation = Quaternion.identity;
    private Vector3 bonfireBaseScale;
    private Color bonfireBaseColor = Color.white;
    private bool hasVortexLoopBaseRotation;
    private bool hasBonfireBaseColor;

    private void Awake()
    {
        HideUiBackgroundImage();
        CacheVortexMaterial();
        CacheBonfireState();
        SetDioramaActive(false);
    }

    private void OnEnable()
    {
        HideUiBackgroundImage();
        CacheVortexMaterial();
        CacheBonfireState();
        SetDioramaActive(true);
    }

    private void OnDisable()
    {
        SetDioramaActive(false);
    }

    private void OnDestroy()
    {
        if (vortexLoopMaterial != null)
        {
            Destroy(vortexLoopMaterial);
            vortexLoopMaterial = null;
        }
    }

    private void Update()
    {
        AnimateVortex();
        AnimateBonfire();
    }

    private void HideUiBackgroundImage()
    {
        hostBackground = hostBackground != null ? hostBackground : GetComponent<Image>();
        if (hostBackground == null)
            return;

        Color color = hostBackground.color;
        color.a = 0f;
        hostBackground.color = color;
        hostBackground.raycastTarget = false;
    }

    private void CacheVortexMaterial()
    {
        if (vortexLoopRenderer == null)
            return;

        if (vortexLoopMaterial == null)
        {
            vortexLoopMaterial = vortexLoopRenderer.material;
            ApplyTextureScale(vortexLoopMaterial, vortexTextureScale);
        }

        if (vortexLoopTransform != null && vortexLoopBaseScale == Vector3.zero)
            vortexLoopBaseScale = vortexLoopTransform.localScale;

        if (vortexLoopTransform != null && !hasVortexLoopBaseRotation)
        {
            vortexLoopBaseRotation = vortexLoopTransform.localRotation;
            hasVortexLoopBaseRotation = true;
        }
    }

    private void CacheBonfireState()
    {
        if (bonfireTransform != null && bonfireBaseScale == Vector3.zero)
            bonfireBaseScale = bonfireTransform.localScale;

        if (bonfireRenderer != null && !hasBonfireBaseColor)
        {
            bonfireBaseColor = bonfireRenderer.color;
            hasBonfireBaseColor = true;
        }
    }

    private void AnimateVortex()
    {
        float time = Time.time;

        if (vortexLoopMaterial != null)
        {
            Vector2 offset = vortexScrollSpeed * time;
            ApplyTextureOffset(vortexLoopMaterial, offset);
        }

        if (vortexLoopTransform == null)
            return;

        if (!Mathf.Approximately(vortexRotationSpeed, 0f))
            vortexLoopTransform.localRotation = vortexLoopBaseRotation * Quaternion.Euler(0f, 0f, time * vortexRotationSpeed);

        if (vortexPulseAmount <= 0f)
            return;

        float pulse = 1f + Mathf.Sin(time * vortexPulseSpeed) * vortexPulseAmount;
        vortexLoopTransform.localScale = new Vector3(vortexLoopBaseScale.x * pulse, vortexLoopBaseScale.y * pulse, vortexLoopBaseScale.z);
    }

    private void AnimateBonfire()
    {
        if (bonfireTransform == null && bonfireRenderer == null)
            return;

        float time = Time.time * bonfireFlickerSpeed;
        float noise = Mathf.PerlinNoise(time, 0.37f);
        float shimmer = Mathf.Sin(time * 1.7f) * 0.5f + 0.5f;
        float flicker = Mathf.Lerp(noise, shimmer, 0.35f);

        if (bonfireTransform != null && bonfireBaseScale != Vector3.zero && bonfireFlickerScale > 0f)
        {
            float width = 1f + (flicker - 0.5f) * bonfireFlickerScale;
            float height = 1f + (flicker - 0.5f) * bonfireFlickerScale * 1.7f;
            bonfireTransform.localScale = new Vector3(bonfireBaseScale.x * width, bonfireBaseScale.y * height, bonfireBaseScale.z);
        }

        if (bonfireRenderer != null && bonfireFlickerAlpha > 0f)
        {
            Color color = bonfireBaseColor;
            color.a = Mathf.Clamp01(bonfireBaseColor.a * (1f - bonfireFlickerAlpha + flicker * bonfireFlickerAlpha));
            bonfireRenderer.color = color;
        }
    }

    private void SetDioramaActive(bool active)
    {
        if (worldRoot != null)
            worldRoot.SetActive(active);

        if (dioramaCamera != null)
            dioramaCamera.enabled = active;
    }

    private static void ApplyTextureScale(Material material, Vector2 scale)
    {
        if (material == null)
            return;

        if (material.HasProperty("_MainTex"))
            material.SetTextureScale("_MainTex", scale);
        if (material.HasProperty("_BaseMap"))
            material.SetTextureScale("_BaseMap", scale);
    }

    private static void ApplyTextureOffset(Material material, Vector2 offset)
    {
        if (material == null)
            return;

        if (material.HasProperty("_MainTex"))
            material.SetTextureOffset("_MainTex", offset);
        if (material.HasProperty("_BaseMap"))
            material.SetTextureOffset("_BaseMap", offset);
    }
}

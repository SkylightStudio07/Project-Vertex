using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserExpSliderInspector : MonoBehaviour
{
    [SerializeField] private Slider expSlider;
    [SerializeField] private TextMeshProUGUI expText;

    private UserExpManager userExpManager;

    private void OnEnable()
    {
        userExpManager = UserExpManager.Instance;
        if (userExpManager == null)
        {
            Logger.LogWarning(this, "UserExpManager instance does not exist.");
            return;
        }
        userExpManager.OnExperienceChanged += Refresh;
        Refresh(userExpManager.Experience);
    }
    private void OnDisable()
    {
        if (userExpManager == null)
            return;

        userExpManager.OnExperienceChanged -= Refresh;
        userExpManager = null;
    }

    private void Refresh(int experience)
    {
        UserExpManager manager = userExpManager != null ? userExpManager : UserExpManager.Instance;
        if (manager == null)
            return;

        int maxExperience = Mathf.Max(1, manager.MaxExperience);
        int clampedExperience = Mathf.Clamp(experience, 0, maxExperience);

        if (expSlider != null)
        {
            expSlider.minValue = 0;
            expSlider.maxValue = maxExperience;
            expSlider.value = clampedExperience;
        }

        if (expText != null)
            expText.text = $"{experience} / {maxExperience}";
    }

    // Debug buttons for testing user experience slider updates in the Inspector.
    [ContextMenu("Debug/Refresh Slider")]
    private void DebugRefreshSlider()
    {
        if (UserExpManager.Instance == null)
        {
            Logger.LogWarning(this, "UserExpManager instance does not exist.");
            return;
        }

        Refresh(UserExpManager.Instance.Experience);
        Logger.Log(this, $"Experience slider refreshed to {UserExpManager.Instance.Experience}.");
    }
}

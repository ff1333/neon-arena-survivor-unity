using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudController : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider experienceSlider;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text killText;
    [SerializeField] private TMP_Text stateText;

    private Health health;
    private PlayerProgress progress;

    public void Initialize(Health playerHealth, PlayerProgress playerProgress)
    {
        health = playerHealth;
        progress = playerProgress;

        health.Changed += HandleHealthChanged;
        progress.ExperienceChanged += HandleExperienceChanged;
        progress.LevelChanged += HandleLevelChanged;
        
        HandleHealthChanged(health.Current, health.Max);
        HandleExperienceChanged(progress.Experience, progress.ExperienceToNextLevel);
        HandleLevelChanged(progress.Level);
        SetKills(0);
        SetTimer(0f);
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.Changed -= HandleHealthChanged;
        }
        if (progress != null)
        {
            progress.ExperienceChanged -= HandleExperienceChanged;
            progress.LevelChanged -= HandleLevelChanged;
        }
    }

    public void SetTimer(float seconds)
    {
        int totalSeconds = Mathf.FloorToInt(seconds);
        int minutes = totalSeconds / 60;
        int remainingSeconds = totalSeconds % 60;
        timerText.text = $"{minutes:00}:{remainingSeconds:00}";
    }

    public void SetKills(int kills)
    {
        killText.text = $"Kills {kills}";
    }

    public void SetState(string value)
    {
        stateText.text = value;
    }

    private void HandleHealthChanged(float current, float max)
    {
        healthSlider.maxValue = max;
        healthSlider.value = current;
    }

    private void HandleExperienceChanged(int current, int required)
    {
        experienceSlider.maxValue = required;
        experienceSlider.value = current;
    }

    private void HandleLevelChanged(int level)
    {
        levelText.text = $"Level {level}";
    }

}

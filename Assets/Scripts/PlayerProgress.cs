using System;
using UnityEngine;

public class PlayerProgress : MonoBehaviour
{
    [SerializeField, Min(1)] private int firstLevelRequirement = 5;

    public int Level { get; private set; } = 1;
    public int Experience { get; private set; }
    public int ExperienceToNextLevel { get; private set; }

    public event Action<int, int> ExperienceChanged;
    public event Action<int> LevelChanged;
    public event Action LevelUpRequested;

    private void Awake()
    {
        ExperienceToNextLevel = firstLevelRequirement;
    }

    private void Start()
    {
        ExperienceChanged?.Invoke(Experience, ExperienceToNextLevel);
        LevelChanged?.Invoke(Level);
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0) 
        { 
            return; 
        }
        Experience += amount;
        
        if (Experience >= ExperienceToNextLevel)
        {
            Experience -= ExperienceToNextLevel;
            Level++;
            ExperienceToNextLevel = Mathf.CeilToInt(ExperienceToNextLevel * 1.35f);
            LevelChanged?.Invoke(Level);
            Debug.Log($"Level up: {Level}");
            LevelUpRequested?.Invoke();
        }

        ExperienceChanged?.Invoke(Experience, ExperienceToNextLevel);
    }

    // Boss reward replaces the two normal choice screens with one six-choice reward.
    public void GrantBossLevels()
    {
        for (var i = 0; i < 2; i++)
        {
            Level++;
            ExperienceToNextLevel = Mathf.CeilToInt(ExperienceToNextLevel * 1.35f);
            LevelChanged?.Invoke(Level);
        }
        ExperienceChanged?.Invoke(Experience, ExperienceToNextLevel);
    }

#if UNITY_EDITOR
    [ContextMenu("Debug/Add One Level")]
    private void DebugAddOneLevel()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        AddExperience(ExperienceToNextLevel - Experience);
    }
#endif
}

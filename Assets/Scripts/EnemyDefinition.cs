using UnityEngine;

public enum ExperiencePickupShape
{
    Diamond,
    Arrow,
    Hexagon
}

[CreateAssetMenu(menuName = "Neon Arena/Enemy Definition")]
public class EnemyDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string displayName;
    [SerializeField] private EnemyBehaviorType behaviorType;
    [SerializeField] private Color displayColor = Color.red;
    [SerializeField] private Color spawnWarningColor = Color.red;
    [SerializeField, Min(0.5f)] private float scaleMultiplier = 1f;

    [Header("Base Combat")]
    [SerializeField, Min(1f)] private float maxHealth = 100f;
    [SerializeField, Min(0.1f)] private float moveSpeed = 2f;
    [SerializeField, Min(0f)] private float contactDamage = 10f;
    [SerializeField, Min(1)] private int experienceReward = 1;

    [Header("Experience Drop")]
    [SerializeField] private ExperiencePickupShape experiencePickupShape;
    [SerializeField] private Color experiencePickupColor = Color.green;
    [SerializeField, Min(0.5f)] private float experiencePickupScale = 1f;

    [Header("Dasher Only")]
    [SerializeField, Min(0.1f)] private float dashInterval = 2.4f;
    [SerializeField, Min(0.05f)] private float dashTelegraphDuration = 0.45f;
    [SerializeField, Min(0.05f)] private float dashDuration = 0.32f;
    [SerializeField, Min(1f)] private float dashSpeedMultiplier = 4f;
    [SerializeField] private Color dashTelegraphColor = Color.yellow;

    [Header("Berserker Only")]
    [SerializeField, Range(0.1f, 0.9f)] private float enrageHealthRatio = 0.5f;
    [SerializeField, Min(1f)] private float enragedSpeedMultiplier = 1.9f;
    [SerializeField] private Color enragedColor = Color.magenta;

    public string DisplayName => displayName;
    public EnemyBehaviorType BehaviorType => behaviorType;
    public Color DisplayColor => displayColor;
    public Color SpawnWarningColor => spawnWarningColor;
    public float ScaleMultiplier => scaleMultiplier;
    public float MaxHealth => maxHealth;
    public float MoveSpeed => moveSpeed;
    public float ContactDamage => contactDamage;
    public int ExperienceReward => experienceReward;
    public ExperiencePickupShape ExperiencePickupShape =>
        experiencePickupShape;
    public Color ExperiencePickupColor => experiencePickupColor;
    public float ExperiencePickupScale => experiencePickupScale;
    public float DashInterval => dashInterval;
    public float DashTelegraphDuration => dashTelegraphDuration;
    public float DashDuration => dashDuration;
    public float DashSpeedMultiplier => dashSpeedMultiplier;
    public Color DashTelegraphColor => dashTelegraphColor;
    public float EnrageHealthRatio => enrageHealthRatio;
    public float EnragedSpeedMultiplier => enragedSpeedMultiplier;
    public Color EnragedColor => enragedColor;
}

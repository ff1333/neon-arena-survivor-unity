using UnityEngine;

[CreateAssetMenu(menuName = "Neon Arena/Weapon Definition")]
public class WeaponDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private WeaponType weaponType;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField] private Sprite projectileSprite;
    [SerializeField] private Color displayColor = Color.white;

    [Header("Combat")]
    [SerializeField, Min(1f)] private float damage = 25f;
    [SerializeField, Min(0.05f)] private float fireInterval = 0.45f;
    [SerializeField, Min(0.5f)] private float range = 5.5f;
    [SerializeField, Min(0.1f)] private float projectileSpeed = 14f;

    public WeaponType WeaponType => weaponType;
    public string DisplayName => displayName;
    private Sprite presentationIcon;
    public Sprite Icon => presentationIcon != null ? presentationIcon
        : presentationIcon = Resources.Load<Sprite>("Polish/Weapon" + weaponType) ?? icon;
    public Sprite ProjectileSprite => projectileSprite;
    public Color DisplayColor => displayColor;
    public float Damage => damage;
    public float FireInterval => fireInterval;
    public float Range => range;
    public float ProjectileSpeed => projectileSpeed;
}

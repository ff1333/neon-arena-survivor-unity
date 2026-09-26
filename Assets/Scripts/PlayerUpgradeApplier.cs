using UnityEngine;

public class PlayerUpgradeApplier : MonoBehaviour
{
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerShooter shooter;
    [SerializeField] private Health health;

    public bool CanApply(PlayerUpgradeData data, int level)
    {
        if (data == null)
        {
            return false;
        }

        switch (data.EffectType)
        {
            case UpgradeEffectType.AttackRange:
                return shooter.CanIncreaseRange;
            case UpgradeEffectType.ProjectileCount:
                return level % 3 == 0 && shooter.CanIncreaseProjectileCount;
            default:
                return true;
        }
    }

    public void Apply(PlayerUpgradeData data)
    {
        switch (data.EffectType)
        {
            case UpgradeEffectType.Damage:
                shooter.AddDamage(data.Value);
                break;
            case UpgradeEffectType.FireRate:
                shooter.ReduceFireInterval(data.Value);
                break;
            case UpgradeEffectType.Heal:
                health.Heal(data.Value);
                break;
            case UpgradeEffectType.MoveSpeed:
                movement.AddMoveSpeed(data.Value);
                break;
            case UpgradeEffectType.MaxHealth:
                health.AddMaxHealth(data.Value);
                break;
            case UpgradeEffectType.AttackRange:
                shooter.AddRange(data.Value);
                break;
            case UpgradeEffectType.ProjectileCount:
                shooter.AddProjectileCount(Mathf.RoundToInt(data.Value));
                break;
        }
    }
}
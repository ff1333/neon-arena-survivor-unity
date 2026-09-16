using UnityEngine;

public class PlayerUpgradeApplier : MonoBehaviour
{
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerShooter shooter;
    [SerializeField] private Health health;

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
        }
    }
}

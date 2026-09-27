using UnityEngine;

public class PlayerUpgradeApplier : MonoBehaviour
{
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerShooter shooter;
    [SerializeField] private Health health;

    public bool CanApply(PlayerUpgradeData data)
    {
        if (data == null)
        {
            return false;
        }

        switch (data.EffectType)
        {
            case UpgradeEffectType.EquipWeapon:
                return shooter.CanEquip(data.Weapon);
            case UpgradeEffectType.Heal:
            case UpgradeEffectType.MoveSpeed:
            case UpgradeEffectType.MaxHealth:
                return true;
            default:
                return false;
        }
    }

    public void Apply(PlayerUpgradeData data)
    {
        switch (data.EffectType)
        {
            case UpgradeEffectType.EquipWeapon:
                shooter.EquipWeapon(data.Weapon);
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

    public int GetWeaponCount(PlayerUpgradeData data)
    {
        return data != null && data.Weapon != null
            ? shooter.GetEquippedCount(data.Weapon.WeaponType)
            : 0;
    }
}
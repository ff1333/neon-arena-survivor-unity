using System.Collections.Generic;
using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    private sealed class EquippedWeapon
    {
        public WeaponDefinition Definition;
        public Transform Slot;
        public float NextFireTime;
    }

    [Header("References")]
    [SerializeField] private GameObjectPool projectilePool;
    [SerializeField] private Transform[] weaponSlots;

    [Header("Targeting")]
    [SerializeField, Min(0f)] private float priorityBandWidth = 1.5f;
    [SerializeField, Min(0.02f)] private float idleScanInterval = 0.1f;

    [Header("Global Upgrades")]
    [SerializeField, Min(1f)] private float damageMultiplier = 1f;
    [SerializeField, Min(1f)] private float maximumDamageMultiplier = 2f;
    [SerializeField, Min(1f)] private float rangeMultiplier = 1f;
    [SerializeField, Min(1f)] private float maximumRangeMultiplier = 1.5f;
    [SerializeField, Min(1f)] private float attackSpeedMultiplier = 1f;
    [SerializeField, Min(1f)] private float maximumAttackSpeedMultiplier = 2f;

    private readonly List<EquippedWeapon> equippedWeapons =
        new List<EquippedWeapon>(6);

    public int EquippedCount => equippedWeapons.Count;
    public int WeaponSlotCapacity => weaponSlots?.Length ?? 0;
    public float DamageMultiplier => damageMultiplier;
    public float RangeMultiplier => rangeMultiplier;
    public float AttackSpeedMultiplier => attackSpeedMultiplier;
    public bool CanIncreaseDamage =>
        damageMultiplier < maximumDamageMultiplier - 0.001f;
    public bool CanIncreaseRange =>
        rangeMultiplier < maximumRangeMultiplier - 0.001f;
    public bool CanIncreaseAttackSpeed =>
        attackSpeedMultiplier < maximumAttackSpeedMultiplier - 0.001f;

    private void Awake()
    {
        maximumDamageMultiplier = Mathf.Max(1f, maximumDamageMultiplier);
        maximumRangeMultiplier = Mathf.Max(1f, maximumRangeMultiplier);
        maximumAttackSpeedMultiplier =
            Mathf.Max(1f, maximumAttackSpeedMultiplier);
        damageMultiplier = Mathf.Clamp(
            damageMultiplier, 1f, maximumDamageMultiplier);
        rangeMultiplier = Mathf.Clamp(
            rangeMultiplier, 1f, maximumRangeMultiplier);
        attackSpeedMultiplier = Mathf.Clamp(
            attackSpeedMultiplier, 1f, maximumAttackSpeedMultiplier);

        if (projectilePool == null ||
            weaponSlots == null || weaponSlots.Length != 6)
        {
            Debug.LogError(
                "PlayerShooter requires a projectile pool and 6 slots.",
                this);
            enabled = false;
            return;
        }

        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] == null ||
                !weaponSlots[i].TryGetComponent(out SpriteRenderer slotRenderer))
            {
                Debug.LogError(
                    $"Weapon slot {i} requires a SpriteRenderer.",
                    this);
                enabled = false;
                return;
            }

            slotRenderer.enabled = false;
        }
    }

    private void Update()
    {
        for (int i = 0; i < equippedWeapons.Count; i++)
        {
            EquippedWeapon weapon = equippedWeapons[i];
            if (Time.time < weapon.NextFireTime)
            {
                continue;
            }

            float effectiveRange =
                weapon.Definition.Range * rangeMultiplier;
            EnemyController target = TargetSelector.FindPriorityTarget(
                weapon.Slot.position,
                effectiveRange,
                priorityBandWidth);

            if (target == null)
            {
                weapon.NextFireTime = Time.time + idleScanInterval;
                continue;
            }

            Vector2 direction =
                (target.transform.position - weapon.Slot.position).normalized;
            weapon.Slot.right = direction;

            GameObject projectileObject = projectilePool.Get(
                weapon.Slot.position,
                Quaternion.identity);
            projectileObject.GetComponent<Projectile>().Fire(
                direction,
                weapon.Definition,
                damageMultiplier,
                rangeMultiplier);

            CombatFeedback.Instance?.PlayShot(
                weapon.Definition.WeaponType);

            weapon.NextFireTime = Time.time +
                weapon.Definition.FireInterval / attackSpeedMultiplier;
        }
    }

    public bool CanEquip(WeaponDefinition definition)
    {
        return definition != null &&
               weaponSlots != null &&
               equippedWeapons.Count < weaponSlots.Length;
    }

    public bool EquipWeapon(WeaponDefinition definition)
    {
        if (!CanEquip(definition))
        {
            return false;
        }

        int slotIndex = equippedWeapons.Count;
        Transform slot = weaponSlots[slotIndex];
        SpriteRenderer slotRenderer = slot.GetComponent<SpriteRenderer>();
        slotRenderer.sprite = definition.Icon;
        slotRenderer.color = definition.DisplayColor;
        slotRenderer.enabled = true;

        equippedWeapons.Add(new EquippedWeapon
        {
            Definition = definition,
            Slot = slot,
            NextFireTime = Time.time +
                definition.FireInterval / attackSpeedMultiplier
        });

        return true;
    }

    public int GetEquippedCount(WeaponType weaponType)
    {
        int count = 0;

        for (int i = 0; i < equippedWeapons.Count; i++)
        {
            if (equippedWeapons[i].Definition.WeaponType == weaponType)
            {
                count++;
            }
        }

        return count;
    }

    public void AddDamageMultiplier(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        damageMultiplier = Mathf.Min(
            maximumDamageMultiplier,
            damageMultiplier + amount);
    }

    public void AddRangeMultiplier(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        rangeMultiplier = Mathf.Min(
            maximumRangeMultiplier,
            rangeMultiplier + amount);
    }

    public void AddAttackSpeedMultiplier(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        attackSpeedMultiplier = Mathf.Min(
            maximumAttackSpeedMultiplier,
            attackSpeedMultiplier + amount);
    }
}

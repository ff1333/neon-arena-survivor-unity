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
    [SerializeField] private WeaponDefinition startingWeapon;
    [SerializeField] private Transform[] weaponSlots;

    [Header("Targeting")]
    [SerializeField, Min(0f)] private float priorityBandWidth = 1.5f;
    [SerializeField, Min(0.02f)] private float idleScanInterval = 0.1f;
    [SerializeField, Range(1, 2)] private int maximumCopiesPerType = 2;

    private readonly List<EquippedWeapon> equippedWeapons =
        new List<EquippedWeapon>(6);

    public int EquippedCount => equippedWeapons.Count;

    private void Awake()
    {
        if (projectilePool == null || startingWeapon == null ||
            weaponSlots == null || weaponSlots.Length != 6)
        {
            Debug.LogError(
                "PlayerShooter requires a projectile pool, starting weapon and 6 slots.",
                this);
            enabled = false;
            return;
        }

        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] == null ||
                !weaponSlots[i].TryGetComponent(out SpriteRenderer slotRenderer))
            {
                Debug.LogError($"Weapon slot {i} requires a SpriteRenderer.", this);
                enabled = false;
                return;
            }

            slotRenderer.enabled = false;
        }

        EquipWeapon(startingWeapon);
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

            EnemyController target = TargetSelector.FindPriorityTarget(
                weapon.Slot.position,
                weapon.Definition.Range,
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
                weapon.Definition);

            weapon.NextFireTime = Time.time + weapon.Definition.FireInterval;
        }
    }

    public bool CanEquip(WeaponDefinition definition)
    {
        return definition != null &&
               equippedWeapons.Count < weaponSlots.Length &&
               GetEquippedCount(definition.WeaponType) < maximumCopiesPerType;
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
            NextFireTime = Time.time + definition.FireInterval
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
}
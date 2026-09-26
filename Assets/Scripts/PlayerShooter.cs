using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private GameObjectPool projectilePool;
    [SerializeField] private float fireInterval = 0.4f;
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float damage = 25f;

    [Header("Targeting")]
    [SerializeField, Min(0.5f)] private float range = 3f;
    [SerializeField, Min(0.5f)] private float maximumRange = 4.5f;
    [SerializeField, Min(0f)] private float priorityBandWidth = 1.5f;

    [Header("Multishot")]
    [SerializeField, Range(1, 5)] private int projectileCount = 1;
    [SerializeField, Range(1, 5)] private int maximumProjectileCount = 5;
    [SerializeField, Min(0f)] private float spreadAngle = 12f;

    private Camera mainCamera;
    private float nextFireTime;

    public bool CanIncreaseRange => range < maximumRange - 0.001f;
    public bool CanIncreaseProjectileCount => projectileCount < maximumProjectileCount;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("PlayerShooter requires a Main Camera.", this);
        }
    }

    private void Update()
    {
        if (Time.time < nextFireTime)
        {
            return;
        }

        GameObject target = FindPriorityTarget();
        if (target == null)
        {
            return;
        }

        nextFireTime = Time.time + fireInterval;
        Vector2 baseDirection = target.transform.position - transform.position;
        FireVolley(baseDirection.normalized);
    }

    private GameObject FindPriorityTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float rangeSqr = range * range;
        float nearestDistanceSqr = float.PositiveInfinity;

        foreach (GameObject enemy in enemies)
        {
            if (!enemy.TryGetComponent(out Health _) || !IsInsideCamera(enemy.transform.position))
            {
                continue;
            }

            float distanceSqr = (enemy.transform.position - transform.position).sqrMagnitude;
            if (distanceSqr <= rangeSqr && distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
            }
        }

        if (float.IsPositiveInfinity(nearestDistanceSqr))
        {
            return null;
        }

        float bandLimit = Mathf.Sqrt(nearestDistanceSqr) + priorityBandWidth;
        float bandLimitSqr = Mathf.Min(rangeSqr, bandLimit * bandLimit);
        GameObject bestTarget = null;
        float lowestHealth = float.PositiveInfinity;
        float bestDistanceSqr = float.PositiveInfinity;

        foreach (GameObject enemy in enemies)
        {
            if (!enemy.TryGetComponent(out Health health) || !IsInsideCamera(enemy.transform.position))
            {
                continue;
            }

            float distanceSqr = (enemy.transform.position - transform.position).sqrMagnitude;
            if (distanceSqr > bandLimitSqr)
            {
                continue;
            }

            if (health.Current < lowestHealth ||
                (Mathf.Approximately(health.Current, lowestHealth) && distanceSqr < bestDistanceSqr))
            {
                bestTarget = enemy;
                lowestHealth = health.Current;
                bestDistanceSqr = distanceSqr;
            }
        }

        return bestTarget;
    }

    private bool IsInsideCamera(Vector3 worldPosition)
    {
        if (mainCamera == null)
        {
            return false;
        }

        Vector3 viewport = mainCamera.WorldToViewportPoint(worldPosition);
        return viewport.z > 0f &&
               viewport.x >= 0f && viewport.x <= 1f &&
               viewport.y >= 0f && viewport.y <= 1f;
    }

    private void FireVolley(Vector2 baseDirection)
    {
        for (int index = 0; index < projectileCount; index++)
        {
            float offset = (index - (projectileCount - 1) * 0.5f) * spreadAngle;
            Vector3 rotated = Quaternion.Euler(0f, 0f, offset) * (Vector3)baseDirection;
            GameObject projectileObject = projectilePool.Get(transform.position, Quaternion.identity);
            projectileObject.GetComponent<Projectile>().Fire(rotated, projectileSpeed, damage);
        }
    }

    public void AddDamage(float amount)
    {
        damage = Mathf.Max(1f, damage + amount);
    }

    public void ReduceFireInterval(float amount)
    {
        fireInterval = Mathf.Max(0.08f, fireInterval - amount);
    }

    public void AddRange(float amount)
    {
        range = Mathf.Clamp(range + amount, 0.5f, maximumRange);
    }

    public void AddProjectileCount(int amount)
    {
        projectileCount = Mathf.Clamp(
            projectileCount + Mathf.Max(0, amount),
            1,
            maximumProjectileCount);
    }
}
using UnityEngine;

public static class TargetSelector
{
    public static EnemyController FindPriorityTarget(
        Vector2 origin,
        float range,
        float priorityBandWidth)
    {
        var enemies = EnemyRegistry.ActiveEnemies;
        float rangeSqr = range * range;
        float nearestDistanceSqr = float.PositiveInfinity;

        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyController enemy = enemies[i];
            if (!IsValid(enemy))
            {
                continue;
            }

            float distanceSqr = ((Vector2)enemy.transform.position - origin).sqrMagnitude;
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
        EnemyController bestTarget = null;
        float lowestHealth = float.PositiveInfinity;
        float bestDistanceSqr = float.PositiveInfinity;

        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyController enemy = enemies[i];
            if (!IsValid(enemy))
            {
                continue;
            }

            float distanceSqr = ((Vector2)enemy.transform.position - origin).sqrMagnitude;
            if (distanceSqr > bandLimitSqr)
            {
                continue;
            }

            float currentHealth = enemy.Health.Current;
            if (currentHealth < lowestHealth ||
                (Mathf.Approximately(currentHealth, lowestHealth) &&
                 distanceSqr < bestDistanceSqr))
            {
                bestTarget = enemy;
                lowestHealth = currentHealth;
                bestDistanceSqr = distanceSqr;
            }
        }

        return bestTarget;
    }

    private static bool IsValid(EnemyController enemy)
    {
        return enemy != null &&
               enemy.isActiveAndEnabled &&
               enemy.Health != null &&
               !enemy.Health.IsDead;
    }
}
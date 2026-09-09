using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float fireInterval = 0.4f;
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float range = 12f;

    private float nextFireTime;

    private void Update()
    {
        if (Time.time < nextFireTime)
        {
            return;
        }
        GameObject target = FindNearestEnemy();
        if (target == null)
        {
            return;
        }
        nextFireTime = Time.time + fireInterval;
        Vector2 direction = target.transform.position - transform.position;
        GameObject projectileObject = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        projectileObject.GetComponent<Projectile>().Fire(direction, projectileSpeed, damage);
    }

    private GameObject FindNearestEnemy()
    {
        GameObject nearest = null;
        float nearestDistance = range * range;

        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            float distance = (enemy.transform.position - transform.position).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = enemy;
            }
        }


        return nearest;
    }
}

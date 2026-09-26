using UnityEngine;

[RequireComponent(typeof(PoolMember))]
public class EnemySpawnWarning : MonoBehaviour
{
    private PoolMember poolMember;
    private Transform playerTarget;
    private GameObjectPool enemyPool;
    private GameObjectPool experiencePool;
    private float spawnTime;
    private bool isConfigured;

    private void Awake()
    {
        poolMember = GetComponent<PoolMember>();
    }

    private void OnEnable()
    {
        isConfigured = false;
    }

    public void Configure(
        Transform newPlayerTarget,
        GameObjectPool newEnemyPool,
        GameObjectPool newExperiencePool,
        float delay)
    {
        playerTarget = newPlayerTarget;
        enemyPool = newEnemyPool;
        experiencePool = newExperiencePool;
        spawnTime = Time.time + delay;
        isConfigured = true;
    }

    private void Update()
    {
        if (!isConfigured || Time.time < spawnTime)
        {
            return;
        }

        isConfigured = false;

        if (playerTarget != null && enemyPool != null)
        {
            GameObject enemyObject = enemyPool.Get(transform.position, Quaternion.identity);
            enemyObject.GetComponent<EnemyController>().Spawn(playerTarget, experiencePool);
        }

        poolMember.Release();
    }
}

using UnityEngine;

[RequireComponent(typeof(PoolMember), typeof(SpriteRenderer))]
public class EnemySpawnWarning : MonoBehaviour
{
    private PoolMember poolMember;
    private SpriteRenderer spriteRenderer;
    private Transform playerTarget;
    private GameObjectPool enemyPool;
    private GameObjectPool experiencePool;
    private EnemyDefinition enemyDefinition;
    private Color authoredColor;
    private float spawnTime;
    private bool isConfigured;

    private void Awake()
    {
        poolMember = GetComponent<PoolMember>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        authoredColor = spriteRenderer.color;
    }

    private void OnEnable()
    {
        isConfigured = false;
    }

    private void OnDisable()
    {
        spriteRenderer.color = authoredColor;
        enemyDefinition = null;
    }

    public void Configure(
        Transform newPlayerTarget,
        GameObjectPool newEnemyPool,
        GameObjectPool newExperiencePool,
        EnemyDefinition newEnemyDefinition,
        float delay)
    {
        playerTarget = newPlayerTarget;
        enemyPool = newEnemyPool;
        experiencePool = newExperiencePool;
        enemyDefinition = newEnemyDefinition;
        spawnTime = Time.time + delay;
        spriteRenderer.color = enemyDefinition.SpawnWarningColor;
        isConfigured = true;
    }

    private void Update()
    {
        if (!isConfigured || Time.time < spawnTime)
        {
            return;
        }

        isConfigured = false;

        if (playerTarget != null && enemyPool != null &&
            experiencePool != null && enemyDefinition != null)
        {
            GameObject enemyObject = enemyPool.Get(
                transform.position,
                Quaternion.identity);
            enemyObject.GetComponent<EnemyController>().Spawn(
                playerTarget,
                experiencePool,
                enemyDefinition);
        }

        poolMember.Release();
    }
}
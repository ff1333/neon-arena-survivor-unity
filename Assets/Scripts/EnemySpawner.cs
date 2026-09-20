using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Pools")]
    [SerializeField] private GameObjectPool enemyPool;
    [SerializeField] private GameObjectPool experiencePool;

    [Header("Difficulty")]
    [SerializeField, Min(0.1f)] private float startingInterval = 1.5f;
    [SerializeField, Min(0.1f)] private float minimumInterval = 0.55f;
    [SerializeField, Min(0f)] private float intervalDecreasePerSecond = 0.004f;
    [SerializeField, Min(10f)] private float extraEnemyEverySeconds = 45f;
    [SerializeField, Min(1)] private int maximumEnemiesPerWave = 3;

    [Header("Spawn Area")]
    [SerializeField, Min(0f)] private float edgePadding = 0.6f;
    [SerializeField, Min(0f)] private float minimumDistanceFromPlayer = 4f;
    [SerializeField, Range(1, 32)] private int positionAttempts = 12;


    private Transform player;
    private Camera mainCamera;
    private float nextSpawnTime;
    private float runStartTime;

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogError("EnemySpawner: No player found with tag 'Player'.", this);
            enabled = false;
            return;
        }

        player = playerObject.transform;
        mainCamera = Camera.main;
        if (mainCamera == null || !mainCamera.orthographic)
        {
            Debug.LogError("EnemySpawner requires an orthographic main camera.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        runStartTime = Time.time;
        nextSpawnTime = Time.time;
    }

    private void Update()
    {
        if (Time.time < nextSpawnTime || player == null)
        {
            return;
        }
        float elapsed = Time.time - runStartTime;
        float currentInterval = Mathf.Max(minimumInterval, startingInterval - elapsed * intervalDecreasePerSecond);
        nextSpawnTime = Time.time + currentInterval;
        
        int enemiesThisWave = Mathf.Clamp(1 + Mathf.FloorToInt(elapsed / extraEnemyEverySeconds), 1, maximumEnemiesPerWave);

        for (int i = 0; i < enemiesThisWave; i++)
        {
            SpawnOne();
        }
    }

    private void SpawnOne()
    {
        Vector3 spawnPosition = ChooseSpawnPosition();
        GameObject enemyObject = enemyPool.Get(spawnPosition, Quaternion.identity);
        enemyObject.GetComponent<EnemyController>().Spawn(player, experiencePool);
    }

    private Vector3 ChooseSpawnPosition()
    {
        Vector2 center = mainCamera.transform.position;
        float halfHeight = Mathf.Max(0.1f, mainCamera.orthographicSize - edgePadding);
        float halfWidth = Mathf.Max(0.1f, mainCamera.orthographicSize * mainCamera.aspect - edgePadding);
        float minimumDistanceSqr = minimumDistanceFromPlayer * minimumDistanceFromPlayer;
        for (int attempt = 0; attempt < positionAttempts; attempt++)
        {
            Vector2 candidate = RandomPointOnPerimeter(center, halfWidth, halfHeight);
            if ((candidate - (Vector2)player.position).sqrMagnitude >= minimumDistanceSqr)
            {
                return candidate;
            }
        }
        
        float fallbackX = player.position.x < center.x ? center.x + halfWidth : center.x - halfWidth;
        float fallbackY = player.position.y < center.y ? center.y + halfHeight : center.y - halfHeight;
        return new Vector3(fallbackX, fallbackY, 0f);
    }

    private static Vector2 RandomPointOnPerimeter(Vector2 center, float halfWidth, float halfHeight)
    {
        switch (Random.Range(0, 4))
        {
            case 0:
                return new Vector2(center.x - halfWidth, Random.Range(center.y - halfHeight, center.y + halfHeight));
            case 1:
                return new Vector2(center.x + halfWidth, Random.Range(center.y - halfHeight, center.y + halfHeight));
            case 2:
                return new Vector2(Random.Range(center.x - halfWidth, center.x + halfWidth), center.y - halfHeight);
            default:
                return new Vector2(Random.Range(center.x - halfWidth, center.x + halfWidth), center.y + halfHeight);
        }
    }
}

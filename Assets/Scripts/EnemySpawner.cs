using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Pools")]
    [SerializeField] private GameObjectPool enemyPool;
    [SerializeField] private GameObjectPool experiencePool;
    [SerializeField] private GameObjectPool warningPool;

    [Header("Difficulty")]
    [SerializeField, Min(0.1f)] private float startingInterval = 1.5f;
    [SerializeField, Min(0.1f)] private float minimumInterval = 0.65f;
    [SerializeField, Min(0f)] private float intervalDecreasePerSecond = 0.004f;

    [Header("Spawn Area")]
    [SerializeField] private ArenaBounds arenaBounds;
    [SerializeField, Min(0f)] private float arenaPadding = 0.75f;
    [SerializeField, Min(0f)] private float minimumDistanceFromPlayer = 5f;
    [SerializeField, Range(1, 128)] private int positionAttempts = 64;

    [Header("Warning")]
    [SerializeField, Min(0f)] private float warningDuration = 1.5f;

    private Transform player;
    private float nextSpawnTime;
    private float runStartTime;

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogError("EnemySpawner could not find the Player tag.", this);
            enabled = false;
            return;
        }

        player = playerObject.transform;

        if (arenaBounds == null || enemyPool == null ||
            experiencePool == null || warningPool == null)
        {
            Debug.LogError("EnemySpawner requires ArenaBounds and all three pools.", this);
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
        if (player == null || Time.time < nextSpawnTime)
        {
            return;
        }

        float elapsed = Time.time - runStartTime;
        float currentInterval = Mathf.Max(
            minimumInterval,
            startingInterval - elapsed * intervalDecreasePerSecond);

        nextSpawnTime = Time.time + currentInterval;
        SpawnOne();
    }

    private void SpawnOne()
    {
        Vector3 warningPosition = ChooseSpawnPosition();
        GameObject warningObject = warningPool.Get(warningPosition, Quaternion.identity);
        warningObject.GetComponent<EnemySpawnWarning>().Configure(
            player,
            enemyPool,
            experiencePool,
            warningDuration);
    }

    private Vector3 ChooseSpawnPosition()
    {
        Vector2 min = arenaBounds.Minimum + Vector2.one * arenaPadding;
        Vector2 max = arenaBounds.Maximum - Vector2.one * arenaPadding;
        float minimumDistanceSqr = minimumDistanceFromPlayer * minimumDistanceFromPlayer;

        for (int attempt = 0; attempt < positionAttempts; attempt++)
        {
            Vector2 candidate = new Vector2(
                Random.Range(min.x, max.x),
                Random.Range(min.y, max.y));

            if ((candidate - (Vector2)player.position).sqrMagnitude >= minimumDistanceSqr)
            {
                return candidate;
            }
        }

        return ChooseFarthestCorner(min, max);
    }

    private Vector2 ChooseFarthestCorner(Vector2 min, Vector2 max)
    {
        Vector2[] corners =
        {
            new Vector2(min.x, min.y),
            new Vector2(min.x, max.y),
            new Vector2(max.x, min.y),
            new Vector2(max.x, max.y)
        };

        Vector2 farthest = corners[0];
        float farthestDistanceSqr = (farthest - (Vector2)player.position).sqrMagnitude;

        for (int i = 1; i < corners.Length; i++)
        {
            float distanceSqr = (corners[i] - (Vector2)player.position).sqrMagnitude;
            if (distanceSqr > farthestDistanceSqr)
            {
                farthest = corners[i];
                farthestDistanceSqr = distanceSqr;
            }
        }

        return farthest;
    }
}
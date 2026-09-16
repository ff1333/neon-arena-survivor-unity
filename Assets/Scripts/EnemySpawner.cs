using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObjectPool enemyPool;
    [SerializeField] private GameObjectPool experiencePool;
    [SerializeField] private float interval = 2f;
    [SerializeField] private float radius = 10f;

    private Transform player;
    private float nextSpawnTime;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (Time.time < nextSpawnTime)
        {
            return;
        }
        nextSpawnTime = Time.time + interval;
        float angle = Random.Range(0f, Mathf.PI * 2f);
        Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
        GameObject enemyObject = enemyPool.Get(player.position + offset, Quaternion.identity);
        enemyObject.GetComponent<EnemyController>().Spawn(player, experiencePool);
    }
}

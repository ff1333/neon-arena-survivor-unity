using UnityEngine;

[RequireComponent(typeof(PoolMember))]
public class ExperiencePickup : MonoBehaviour
{
    [SerializeField, Min(1)] private int value = 1;
    [SerializeField] private float magnetRadius = 3f;
    [SerializeField] private float moveSpeed = 7f;

    private PoolMember poolMember;
    private Transform player;
    private bool collected;

    private void Awake()
    {
        poolMember = GetComponent<PoolMember>();
    }

    private void OnEnable()
    {
        collected = false;
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        player = playerObject != null ? playerObject.transform : null;
    }

    public void Configure(int newValue)
    {
        value = Mathf.Max(1, newValue);
    }

    private void Update()
    {
        if (player == null) 
        { 
            return; 
        }
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= magnetRadius)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player"))
        {
            return;
        }
        if (other.TryGetComponent(out PlayerProgress progress))
        {
            collected = true;
            progress.AddExperience(value);
            poolMember.Release();
        }
    }
}

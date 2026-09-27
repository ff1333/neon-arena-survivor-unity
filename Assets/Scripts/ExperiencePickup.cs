using UnityEngine;

[RequireComponent(typeof(PoolMember))]
public class ExperiencePickup : MonoBehaviour
{
    [SerializeField, Min(1)] private int value = 1;
    [SerializeField, Min(0.1f)] private float fallbackMagnetRadius = 3f;
    [SerializeField, Min(0.1f)] private float moveSpeed = 7f;

    private PoolMember poolMember;
    private Transform player;
    private PlayerPickupRange pickupRange;
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
        pickupRange = playerObject != null
            ? playerObject.GetComponent<PlayerPickupRange>()
            : null;
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

        float radius = pickupRange != null
            ? pickupRange.CurrentRadius
            : fallbackMagnetRadius;
        Vector2 offset = player.position - transform.position;

        if (offset.sqrMagnitude <= radius * radius)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                moveSpeed * Time.deltaTime);
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
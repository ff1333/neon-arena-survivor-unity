using System.Runtime.CompilerServices;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D), typeof(Health))]
public class EnemyController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float attackInterval = 1f;

    private Rigidbody2D body;
    private Transform target;
    private float nextAttackTime;

    private Health health;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        health.Died += HandleDied;
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
    }

    private void FixedUpdate()
    {
        if (target == null)
        {
            return;
        }
        Vector2 direction = (target.position - transform.position).normalized;
        body.MovePosition(body.position + direction * moveSpeed * Time.fixedDeltaTime);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || Time.time < nextAttackTime)
        {
            return;
        } 
        nextAttackTime = Time.time + attackInterval;
        if (other.TryGetComponent(out Health playerHealth))
        {
            playerHealth.TakeDamage(contactDamage);
            Debug.Log($"Player Hp: {playerHealth.Current}");
        }

    }
    private void OnDestroy()
    {
        if (health != null)
        {
            health.Died -= HandleDied;
        }
    }

    private void HandleDied()
    {
        Destroy(gameObject);
    }
}

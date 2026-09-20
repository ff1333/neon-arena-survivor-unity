using System;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D), typeof(Health), typeof(PoolMember))]
public class EnemyController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float attackInterval = 1f;

    private Rigidbody2D body;
    private Transform target;
    private float nextAttackTime;

    private Health health;
    private PoolMember poolMember;
    private GameObjectPool experiencePool;
    public static event Action DiedGlobally;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        poolMember = GetComponent<PoolMember>();
        health.Died += HandleDied;
    }

    private void OnEnable()
    {
        nextAttackTime = 0f;
        health.RestoreFull();
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.Died -= HandleDied;
        }
        
    }

    public void Spawn(Transform newTarget, GameObjectPool newExperiencePool)
    {
        target = newTarget;
        experiencePool = newExperiencePool;
        health.RestoreFull();
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

    private void HandleDied()
    {
        if (experiencePool != null)
        {
            // Spawn experience pickup at enemy's location
            GameObject pickupObject = experiencePool.Get(transform.position, Quaternion.identity);
            pickupObject.GetComponent<ExperiencePickup>().Configure(1);
        }
        DiedGlobally?.Invoke();
        poolMember.Release();
    }
}

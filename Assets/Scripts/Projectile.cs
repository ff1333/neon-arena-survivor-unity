using UnityEngine;

[RequireComponent(typeof(PoolMember))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float lifeTime = 2f;

    private PoolMember poolMember;
    private Vector2 direction;
    private float speed;
    private float damage;
    private float releaseTime;

    private void Awake()
    {
        poolMember = GetComponent<PoolMember>();
    }

    public void Fire(Vector2 newDirection, float newSpeed, float newDamage)
    {
        direction = newDirection.normalized;
        speed = newSpeed;
        damage = newDamage;
        releaseTime = Time.time + lifeTime;
    }

    public void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        if (Time.time >= releaseTime)
        {
            poolMember.Release();
        }
    
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy"))
        {
            return;
        }
        if (other.TryGetComponent(out Health enemyHealth))
        {
            enemyHealth.TakeDamage(damage);
        }

        poolMember.Release();
    }
}

using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float lifeTime = 2f;

    private Vector2 direction;
    private float speed;
    private float damage;

    public void Fire(Vector2 newDirection, float newSpeed, float newDamage)
    {
        direction = newDirection.normalized;
        speed = newSpeed;
        damage = newDamage;
        Destroy(gameObject, lifeTime);
    }

    public void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
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

        Destroy(gameObject);
    }
}

using UnityEngine;

[RequireComponent(typeof(PoolMember))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float lifeTime = 2f;
    [SerializeField, Min(0f)] private float viewportMargin = 0.02f;

    private Camera mainCamera;

    private PoolMember poolMember;
    private Vector2 direction;
    private float speed;
    private float damage;
    private float releaseTime;

    private void Awake()
    {
        poolMember = GetComponent<PoolMember>();
        mainCamera = Camera.main;
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

        if (Time.time >= releaseTime || IsOutsideCamera())
        {
            poolMember.Release();
        }
    
    }

    private bool IsOutsideCamera()
    {
        if (mainCamera == null)
        {
            return false;
        }
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);
        return viewportPosition.z < 0f ||
               viewportPosition.x < -viewportMargin || viewportPosition.x > 1f + viewportMargin ||
               viewportPosition.y < -viewportMargin || viewportPosition.y > 1f + viewportMargin;
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

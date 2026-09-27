using UnityEngine;

[RequireComponent(typeof(PoolMember))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float lifeTime = 2f;
    [SerializeField, Min(0f)] private float viewportMargin = 0.02f;

    private Camera mainCamera;
    private CameraFollow cameraFollow;
    private PoolMember poolMember;
    private Vector2 direction;
    private float speed;
    private float damage;
    private float releaseTime;

    private void Awake()
    {
        poolMember = GetComponent<PoolMember>();
        FindCameraFeedback();
    }

    public void Fire(Vector2 newDirection, float newSpeed, float newDamage)
    {
        direction = newDirection.normalized;
        speed = newSpeed;
        damage = newDamage;
        releaseTime = Time.time + lifeTime;
    }

    private void Update()
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
            FindCameraFeedback();
        }

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

        if (other.TryGetComponent(out Health enemyHealth) && !enemyHealth.IsDead)
        {
            if (other.TryGetComponent(out EnemyHitFlash hitFlash))
            {
                hitFlash.Flash();
            }

            enemyHealth.TakeDamage(damage);
            ShakeCamera(enemyHealth.IsDead ? 1.6f : 1f);
        }

        poolMember.Release();
    }

    private void FindCameraFeedback()
    {
        mainCamera = Camera.main;
        cameraFollow = mainCamera != null
            ? mainCamera.GetComponent<CameraFollow>()
            : null;
    }

    private void ShakeCamera(float strengthMultiplier)
    {
        if (cameraFollow == null)
        {
            FindCameraFeedback();
        }

        cameraFollow?.Shake(strengthMultiplier);
    }
}
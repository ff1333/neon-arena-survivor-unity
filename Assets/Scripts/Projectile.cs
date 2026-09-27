using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(PoolMember))]
public class Projectile : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float lifeTime = 2f;

    private SpriteRenderer spriteRenderer;
    private CameraFollow cameraFollow;
    private PoolMember poolMember;
    private Sprite defaultSprite;
    private Color defaultColor;
    private Vector2 direction;
    private float speed;
    private float damage;
    private float remainingDistance;
    private float releaseTime;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        poolMember = GetComponent<PoolMember>();
        defaultSprite = spriteRenderer.sprite;
        defaultColor = spriteRenderer.color;
        FindCameraFeedback();
    }

    private void OnDisable()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.sprite = defaultSprite;
        spriteRenderer.color = defaultColor;
    }

    public void Fire(Vector2 newDirection, WeaponDefinition weapon)
    {
        if (weapon == null)
        {
            Debug.LogError("Projectile requires a weapon definition.", this);
            poolMember.Release();
            return;
        }

        direction = newDirection.normalized;
        speed = weapon.ProjectileSpeed;
        damage = weapon.Damage;
        remainingDistance = weapon.Range;
        releaseTime = Time.time + lifeTime;

        spriteRenderer.sprite = weapon.ProjectileSprite;
        spriteRenderer.color = weapon.DisplayColor;
        transform.right = direction;
    }

    private void Update()
    {
        if (remainingDistance <= 0f || Time.time >= releaseTime)
        {
            poolMember.Release();
            return;
        }

        float moveDistance = Mathf.Min(
            speed * Time.deltaTime,
            remainingDistance);
        transform.position += (Vector3)(direction * moveDistance);
        remainingDistance -= moveDistance;

        if (remainingDistance <= 0.0001f || Time.time >= releaseTime)
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
        Camera mainCamera = Camera.main;
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
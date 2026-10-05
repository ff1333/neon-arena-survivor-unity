using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Health), typeof(PoolMember))]
[RequireComponent(typeof(SpriteRenderer), typeof(EnemyHitFlash))]
public class EnemyController : MonoBehaviour
{
    private Rigidbody2D body;
    private Transform target;
    private Health health;
    private PoolMember poolMember;
    private SpriteRenderer spriteRenderer;
    private EnemyHitFlash hitFlash;
    private GameObjectPool experiencePool;
    private CameraFollow cameraFollow;
    private EnemyDefinition definition;
    private Vector3 authoredScale;
    private Color currentColor;
    private bool hasHitPlayer;
    private bool isPreparingDash;
    private bool isDashing;
    private bool isEnraged;
    private float nextDashTime;
    private float dashPreparationEndTime;
    private float dashEndTime;
    private Vector2 dashDirection;
    private BossAgent boss;

    public static event Action DiedGlobally;
    public Health Health => health;
    public Color CurrentColor => currentColor;
    public void SpawnBoss(BossAgent owner, float maximum, Color color)
    {
        boss = owner;
        currentColor = color;
        health.ResetHealth(maximum);
        spriteRenderer.color = color;
        hitFlash.SetBaseAppearance(color, transform.localScale);
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        poolMember = GetComponent<PoolMember>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        hitFlash = GetComponent<EnemyHitFlash>();
        authoredScale = transform.localScale;

        health.Changed += HandleHealthChanged;
        health.Died += HandleDied;
        FindCameraFeedback();
    }

    private void OnEnable()
    {
        hasHitPlayer = false;
        isPreparingDash = false;
        isDashing = false;
        isEnraged = false;
        EnemyRegistry.Register(this);
    }

    private void OnDisable()
    {
        EnemyRegistry.Unregister(this);
        target = null;
        experiencePool = null;
        definition = null;
    }

    private void OnDestroy()
    {
        EnemyRegistry.Unregister(this);

        if (health != null)
        {
            health.Changed -= HandleHealthChanged;
            health.Died -= HandleDied;
        }
    }

    public void Spawn(
        Transform newTarget,
        GameObjectPool newExperiencePool,
        EnemyDefinition newDefinition)
    {
        if (newTarget == null || newExperiencePool == null ||
            newDefinition == null)
        {
            Debug.LogError("Enemy spawn requires target, experience pool and definition.", this);
            poolMember.Release();
            return;
        }

        target = newTarget;
        experiencePool = newExperiencePool;
        definition = newDefinition;
        var appearance = Resources.Load<Sprite>("Polish/" + definition.BehaviorType);
        if (appearance != null) spriteRenderer.sprite = appearance;
        hasHitPlayer = false;
        isPreparingDash = false;
        isDashing = false;
        isEnraged = false;
        nextDashTime = Time.time + definition.DashInterval;
        health.ResetHealth(definition.MaxHealth);
        ApplyAppearance(definition.DisplayColor, 1f);
    }

    private void FixedUpdate()
    {
        if (target == null || definition == null)
        {
            return;
        }

        if (definition.BehaviorType == EnemyBehaviorType.Dasher &&
            UpdateDashMovement())
        {
            return;
        }

        Vector2 direction =
            ((Vector2)target.position - body.position).normalized;
        float speedMultiplier = isEnraged
            ? definition.EnragedSpeedMultiplier
            : 1f;

        Move(direction, definition.MoveSpeed * speedMultiplier);
    }

    private bool UpdateDashMovement()
    {
        if (isDashing)
        {
            if (Time.time < dashEndTime)
            {
                Move(
                    dashDirection,
                    definition.MoveSpeed * definition.DashSpeedMultiplier);
                return true;
            }

            isDashing = false;
            nextDashTime = Time.time + definition.DashInterval;
            ApplyAppearance(definition.DisplayColor, 1f);
            return false;
        }

        if (isPreparingDash)
        {
            if (Time.time < dashPreparationEndTime)
            {
                return true;
            }

            isPreparingDash = false;
            isDashing = true;
            dashEndTime = Time.time + definition.DashDuration;
            dashDirection =
                ((Vector2)target.position - body.position).normalized;
            ApplyAppearance(definition.DisplayColor, 1f);
            return true;
        }

        if (Time.time < nextDashTime)
        {
            return false;
        }

        isPreparingDash = true;
        dashPreparationEndTime =
            Time.time + definition.DashTelegraphDuration;
        ApplyAppearance(definition.DashTelegraphColor, 1.12f);
        CombatFeedback.Instance?.PlayEnemyCharge(
            transform.position,
            definition.DashTelegraphColor);
        return true;
    }

    private void Move(Vector2 direction, float speed)
    {
        body.MovePosition(
            body.position + direction * speed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (boss != null) return;
        if (hasHitPlayer || definition == null ||
            !other.CompareTag("Player"))
        {
            return;
        }

        if (!other.TryGetComponent(out Health playerHealth))
        {
            return;
        }

        hasHitPlayer = true;
        playerHealth.TakeDamage(definition.ContactDamage);
        CombatFeedback.Instance?.PlayPlayerHit(other.transform.position);

        if (!playerHealth.IsDead)
        {
            ShakeCamera(1.8f);
        }

        poolMember.Release();
    }

    private void HandleHealthChanged(float current, float maximum)
    {
        if (definition == null || current <= 0f || isEnraged ||
            definition.BehaviorType != EnemyBehaviorType.Berserker)
        {
            return;
        }

        if (current / maximum > definition.EnrageHealthRatio)
        {
            return;
        }

        isEnraged = true;
        ApplyAppearance(definition.EnragedColor, 1.08f);
        CombatFeedback.Instance?.PlayEnemyEnrage(
            transform.position,
            definition.EnragedColor);
    }

    private void HandleDied()
    {
        if (boss != null)
        {
            DiedGlobally?.Invoke();
            CombatFeedback.Instance?.PlayEnemyDeath(transform.position, currentColor);
            boss.Defeated();
            return;
        }
        if (definition == null)
        {
            poolMember.Release();
            return;
        }

        if (experiencePool != null)
        {
            GameObject pickupObject = experiencePool.Get(
                transform.position,
                Quaternion.identity);
            pickupObject.GetComponent<ExperiencePickup>().Configure(
                definition.ExperienceReward,
                definition.ExperiencePickupShape,
                definition.ExperiencePickupColor,
                definition.ExperiencePickupScale);
        }

        CombatFeedback.Instance?.PlayEnemyDeath(
            transform.position,
            currentColor);
        DiedGlobally?.Invoke();
        poolMember.Release();
    }

    private void ApplyAppearance(Color color, float stateScaleMultiplier)
    {
        currentColor = color;
        Vector3 scale = authoredScale *
            definition.ScaleMultiplier * stateScaleMultiplier;
        spriteRenderer.color = color;
        transform.localScale = scale;
        hitFlash.SetBaseAppearance(color, scale);
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

using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyHitFlash : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float flashDuration = 0.08f;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField, Range(1f, 1.5f)] private float punchScale = 1.12f;

    private SpriteRenderer spriteRenderer;
    private Color baseColor;
    private Vector3 baseScale;
    private float restoreTime;
    private bool isFlashing;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseColor = spriteRenderer.color;
        baseScale = transform.localScale;
    }

    private void OnEnable()
    {
        RestoreAppearance();
    }

    private void OnDisable()
    {
        RestoreAppearance();
    }

    private void Update()
    {
        if (!isFlashing || Time.time < restoreTime)
        {
            return;
        }

        RestoreAppearance();
    }

    public void SetBaseAppearance(Color color, Vector3 scale)
    {
        baseColor = color;
        baseScale = scale;

        if (!isFlashing)
        {
            spriteRenderer.color = baseColor;
            transform.localScale = baseScale;
        }
    }

    public void Flash()
    {
        spriteRenderer.color = flashColor;
        transform.localScale = baseScale * punchScale;
        restoreTime = Time.time + flashDuration;
        isFlashing = true;
    }

    private void RestoreAppearance()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.color = baseColor;
        transform.localScale = baseScale;
        isFlashing = false;
    }
}
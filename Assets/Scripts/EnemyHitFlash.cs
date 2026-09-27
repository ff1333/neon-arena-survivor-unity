using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyHitFlash : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float flashDuration = 0.08f;
    [SerializeField] private Color flashColor = Color.white;

    private SpriteRenderer spriteRenderer;
    private Color normalColor;
    private float restoreTime;
    private bool isFlashing;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        normalColor = spriteRenderer.color;
    }

    private void OnEnable()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.color = normalColor;
        isFlashing = false;
    }

    private void OnDisable()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }

        isFlashing = false;
    }

    private void Update()
    {
        if (!isFlashing || Time.time < restoreTime)
        {
            return;
        }

        spriteRenderer.color = normalColor;
        isFlashing = false;
    }

    public void Flash()
    {
        spriteRenderer.color = flashColor;
        restoreTime = Time.time + flashDuration;
        isFlashing = true;
    }
}
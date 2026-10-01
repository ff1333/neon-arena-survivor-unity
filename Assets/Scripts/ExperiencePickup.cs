using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PoolMember), typeof(SpriteRenderer))]
public class ExperiencePickup : MonoBehaviour
{
    private const int SpriteSize = 64;
    private const int SamplesPerAxis = 4;

    private static readonly Dictionary<ExperiencePickupShape, Sprite>
        GeneratedSprites = new Dictionary<ExperiencePickupShape, Sprite>();

    [SerializeField, Min(1)] private int value = 1;
    [SerializeField, Min(0.1f)] private float fallbackMagnetRadius = 3f;
    [SerializeField, Min(0.1f)] private float moveSpeed = 7f;

    private PoolMember poolMember;
    private SpriteRenderer spriteRenderer;
    private Transform player;
    private PlayerPickupRange pickupRange;
    private Vector3 authoredScale;
    private bool collected;

    private void Awake()
    {
        poolMember = GetComponent<PoolMember>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        authoredScale = transform.localScale;
    }

    private void OnEnable()
    {
        collected = false;
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        player = playerObject != null ? playerObject.transform : null;
        pickupRange = playerObject != null
            ? playerObject.GetComponent<PlayerPickupRange>()
            : null;
    }

    public void Configure(
        int newValue,
        ExperiencePickupShape shape,
        Color color,
        float scaleMultiplier)
    {
        value = Mathf.Max(1, newValue);
        spriteRenderer.sprite = GetOrCreateSprite(shape);
        spriteRenderer.color = color;
        transform.localScale = authoredScale * Mathf.Max(0.5f, scaleMultiplier);
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        float radius = pickupRange != null
            ? pickupRange.CurrentRadius
            : fallbackMagnetRadius;
        Vector2 offset = player.position - transform.position;

        if (offset.sqrMagnitude <= radius * radius)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                moveSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player"))
        {
            return;
        }

        if (other.TryGetComponent(out PlayerProgress progress))
        {
            collected = true;
            progress.AddExperience(value);
            poolMember.Release();
        }
    }

    private static Sprite GetOrCreateSprite(ExperiencePickupShape shape)
    {
        if (GeneratedSprites.TryGetValue(shape, out Sprite sprite))
        {
            return sprite;
        }

        Texture2D texture = new Texture2D(
            SpriteSize,
            SpriteSize,
            TextureFormat.RGBA32,
            false);
        texture.name = $"Experience {shape}";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        texture.hideFlags = HideFlags.HideAndDontSave;

        Color32[] pixels = new Color32[SpriteSize * SpriteSize];
        int sampleCount = SamplesPerAxis * SamplesPerAxis;

        for (int y = 0; y < SpriteSize; y++)
        {
            for (int x = 0; x < SpriteSize; x++)
            {
                int coveredSamples = 0;

                for (int sampleY = 0; sampleY < SamplesPerAxis; sampleY++)
                {
                    for (int sampleX = 0; sampleX < SamplesPerAxis; sampleX++)
                    {
                        float normalizedX = ((x +
                            (sampleX + 0.5f) / SamplesPerAxis) /
                            SpriteSize) * 2f - 1f;
                        float normalizedY = ((y +
                            (sampleY + 0.5f) / SamplesPerAxis) /
                            SpriteSize) * 2f - 1f;

                        if (IsInsideShape(shape, normalizedX, normalizedY))
                        {
                            coveredSamples++;
                        }
                    }
                }

                byte alpha = (byte)Mathf.RoundToInt(
                    255f * coveredSamples / sampleCount);
                pixels[y * SpriteSize + x] =
                    new Color32(255, 255, 255, alpha);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, true);

        sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, SpriteSize, SpriteSize),
            new Vector2(0.5f, 0.5f),
            SpriteSize);
        sprite.name = $"Experience {shape}";
        sprite.hideFlags = HideFlags.HideAndDontSave;
        GeneratedSprites.Add(shape, sprite);
        return sprite;
    }

    private static bool IsInsideShape(
        ExperiencePickupShape shape,
        float x,
        float y)
    {
        float absoluteX = Mathf.Abs(x);
        float absoluteY = Mathf.Abs(y);

        switch (shape)
        {
            case ExperiencePickupShape.Diamond:
                return absoluteX + absoluteY <= 0.82f;
            case ExperiencePickupShape.Arrow:
                bool shaft = y >= -0.78f && y <= 0.08f &&
                    absoluteX <= 0.22f;
                bool arrowHead = y >= -0.08f && y <= 0.82f &&
                    absoluteX <= (0.82f - y) * 0.82f;
                return shaft || arrowHead;
            case ExperiencePickupShape.Hexagon:
                return absoluteY <= 0.74f &&
                    absoluteX + absoluteY * 0.48f <= 0.82f;
            default:
                return false;
        }
    }
}

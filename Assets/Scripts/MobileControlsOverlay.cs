using UnityEngine;
using UnityEngine.UI;

public sealed class MobileControlsOverlay : MonoBehaviour
{
    private const float JoystickSize = 250f;
    private const float HandleSize = 118f;
    private const float MovementRange = 72f;

    private static MobileControlsOverlay instance;
    private static bool gameplayActive;
    private static Sprite circleSprite;

    private MobileJoystickTouchArea touchArea;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (!ShouldCreate() ||
            FindFirstObjectByType<MobileControlsOverlay>() != null)
        {
            return;
        }

        CreateOverlay();
    }

    public static void SetGameplayActive(bool active)
    {
        gameplayActive = active;
        if (instance != null)
        {
            if (!active)
            {
                instance.touchArea.CancelInput();
            }

            instance.gameObject.SetActive(active);
        }
    }

    private static bool ShouldCreate()
    {
#if UNITY_EDITOR
        return UnityEditor.EditorUserBuildSettings.activeBuildTarget ==
            UnityEditor.BuildTarget.Android;
#else
        return Application.isMobilePlatform;
#endif
    }

    private static void CreateOverlay()
    {
        GameObject canvasObject = new GameObject(
            "Mobile Controls",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler));

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GameObject backgroundObject = CreateUiObject(
            "Movement Joystick",
            canvasObject.transform);
        RectTransform background =
            backgroundObject.GetComponent<RectTransform>();
        background.anchorMin = new Vector2(0.5f, 0.5f);
        background.anchorMax = new Vector2(0.5f, 0.5f);
        background.pivot = new Vector2(0.5f, 0.5f);
        background.anchoredPosition = Vector2.zero;
        background.sizeDelta = Vector2.one * JoystickSize;

        Image backgroundImage = backgroundObject.AddComponent<Image>();
        backgroundImage.sprite = GetCircleSprite();
        backgroundImage.color = new Color(0.035f, 0.075f, 0.09f, 0.28f);
        backgroundImage.raycastTarget = false;
        backgroundImage.enabled = false;

        GameObject handleObject = CreateUiObject(
            "Handle",
            canvasObject.transform);
        RectTransform handle = handleObject.GetComponent<RectTransform>();
        handle.anchorMin = new Vector2(0.5f, 0.5f);
        handle.anchorMax = new Vector2(0.5f, 0.5f);
        handle.pivot = new Vector2(0.5f, 0.5f);
        handle.anchoredPosition = Vector2.zero;
        handle.sizeDelta = Vector2.one * HandleSize;

        Image handleImage = handleObject.AddComponent<Image>();
        handleImage.sprite = GetCircleSprite();
        handleImage.color = new Color(0.13f, 0.92f, 0.72f, 0.58f);
        handleImage.raycastTarget = false;
        handleImage.enabled = false;

        instance = canvasObject.AddComponent<MobileControlsOverlay>();
        instance.touchArea =
            canvasObject.AddComponent<MobileJoystickTouchArea>();
        instance.touchArea.Initialize(
            canvasObject.GetComponent<RectTransform>(),
            background,
            handle,
            backgroundImage,
            handleImage,
            MovementRange);
        canvasObject.SetActive(gameplayActive);
    }

    private static GameObject CreateUiObject(
        string objectName,
        Transform parent)
    {
        GameObject result = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer));
        result.layer = 5;
        result.transform.SetParent(parent, false);
        return result;
    }

    private static Sprite GetCircleSprite()
    {
        if (circleSprite != null)
        {
            return circleSprite;
        }

        const int size = 128;
        Texture2D texture = new Texture2D(
            size,
            size,
            TextureFormat.RGBA32,
            false);
        texture.name = "Runtime Joystick Circle";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Color32[] pixels = new Color32[size * size];
        Vector2 center = Vector2.one * (size - 1) * 0.5f;
        float radius = size * 0.49f;
        float feather = 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(
                    new Vector2(x, y),
                    center);
                byte alpha = (byte)Mathf.RoundToInt(
                    Mathf.Clamp01((radius - distance) / feather) * 255f);
                pixels[y * size + x] = new Color32(255, 255, 255, alpha);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        circleSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            100f);
        circleSprite.name = "Runtime Joystick Circle";
        return circleSprite;
    }
}

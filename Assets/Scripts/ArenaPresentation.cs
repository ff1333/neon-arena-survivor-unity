using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public sealed class ArenaPresentation : MonoBehaviour
{
    private static Material trailMaterial;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        SceneManager.sceneLoaded -= Loaded;
        SceneManager.sceneLoaded += Loaded;
    }

    private static void Loaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Main") return;
        new GameObject("Arena Presentation").AddComponent<ArenaPresentation>();
    }

    private void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Polish/Player");
            player.GetComponent<SpriteRenderer>().color = new Color(.46f,1f,.77f);
        }
        var arena = FindFirstObjectByType<ArenaBounds>();
        if (arena != null)
        {
            var oldFloor = GameObject.Find("ArenaFloor");
            if (oldFloor != null && oldFloor.TryGetComponent<SpriteRenderer>(out var oldRenderer)) oldRenderer.enabled = false;
            var floor = new GameObject("Arena Deck").AddComponent<SpriteRenderer>();
            floor.transform.SetParent(transform);
            floor.transform.position = (arena.Minimum + arena.Maximum) * .5f;
            floor.sprite = Resources.Load<Sprite>("Polish/Floor");
            floor.drawMode = SpriteDrawMode.Tiled;
            floor.size = arena.Maximum - arena.Minimum;
            floor.sortingOrder = -30;
        }
        foreach (var image in FindObjectsByType<Image>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            var canvas = image.GetComponentInParent<Canvas>();
            if (canvas != null && (canvas.name == "Boss Canvas" || canvas.name == "Settings Canvas")) continue;
            if (image.name == "ModalWindow") image.color = new Color(.065f,.075f,.09f,.98f);
            if (image.TryGetComponent<Button>(out var button))
            {
                image.color = image.name.Contains("Restart") ? new Color(.32f,.15f,.19f)
                    : image.name.Contains("Upgrade") ? new Color(.13f,.23f,.29f) : new Color(.10f,.30f,.30f);
                var colors = button.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(1.18f,1.18f,1.18f);
                colors.pressedColor = new Color(.75f,.75f,.75f);
                colors.selectedColor = new Color(1.08f,1.08f,1.08f);
                colors.disabledColor = new Color(.50f,.55f,.55f);
                button.colors = colors;
            }
        }
        foreach (var text in FindObjectsByType<TMP_Text>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            text.raycastTarget = false;
            if (text.name == "TitleText") text.characterSpacing = 0;
        }
        var startPanel = GameObject.Find("StartPanel");
        if (startPanel != null)
        {
            var window = startPanel.transform.Find("ModalWindow");
            if (window != null)
            {
                if (window.TryGetComponent<Image>(out var backdrop)) backdrop.color = Color.clear;
                if (window.TryGetComponent<Outline>(out var outline)) outline.enabled = false;
                var rect = window.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
                rect.offsetMin = rect.offsetMax = Vector2.zero;
                foreach (var title in window.GetComponentsInChildren<TMP_Text>())
                {
                    if (title.name == "TitleText")
                    {
                        Place(title.rectTransform,new Vector2(.08f,.57f),new Vector2(.87f,.80f));
                        title.alignment = TextAlignmentOptions.Left;
                        title.fontSize = 64f; title.enableAutoSizing = true; title.fontSizeMax = 64f;
                    }
                    if (title.name == "BestText")
                    {
                        Place(title.rectTransform,new Vector2(.08f,.47f),new Vector2(.80f,.56f));
                        title.alignment = TextAlignmentOptions.Left;
                    }
                }
                var start = window.GetComponentInChildren<Button>();
                if (start != null) Place(start.GetComponent<RectTransform>(),new Vector2(.08f,.30f),new Vector2(.38f,.40f));
            }
        }
    }

    private static void Place(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin=min; rect.anchorMax=max; rect.offsetMin=rect.offsetMax=Vector2.zero;
    }

    public static Material TrailMaterial
    {
        get
        {
            if (trailMaterial == null) trailMaterial = new Material(Resources.Load<Shader>("NeonVfx"));
            return trailMaterial;
        }
    }

    private void OnDestroy()
    {
        if (trailMaterial != null) Destroy(trailMaterial);
        trailMaterial = null;
    }
}

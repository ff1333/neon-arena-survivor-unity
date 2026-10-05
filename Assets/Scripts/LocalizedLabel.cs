using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Retain the source text so changing languages never translates a translation.
[DefaultExecutionOrder(10000)]
public sealed class LocalizedLabel : MonoBehaviour
{
    [Serializable] private class Entry { public string en; public string zh; }
    [Serializable] private class Catalog { public Entry[] entries; }
    private static Dictionary<string, string> translations;
    private static Regex matcher;
    private static Font font;
    private static TMP_FontAsset tmpFont;
    private Text legacy;
    private TMP_Text tmp;
    private string source;
    private string rendered;
    private bool lastChinese;
    public static Font Font => font != null ? font : font = Resources.Load<Font>("Localization/NotoSansCJKsc-Regular");
    public static string Translate(string value)
    {
        if (string.IsNullOrEmpty(value) || !PortfolioSettings.Chinese) return value;
        if (translations == null)
        {
            var asset = Resources.Load<TextAsset>("Localization/Strings");
            var entries = JsonUtility.FromJson<Catalog>(asset.text).entries;
            translations = entries.ToDictionary(item => item.en, item => item.zh);
            matcher = new Regex("(?<![A-Za-z])(?:" + string.Join("|", translations.Keys.OrderByDescending(key => key.Length).Select(Regex.Escape)) + ")(?![A-Za-z])");
        }
        return matcher.Replace(value, match => translations[match.Value]);
    }
    public static void Attach(GameObject target)
    {
        if (!target.TryGetComponent<LocalizedLabel>(out _)) target.AddComponent<LocalizedLabel>();
    }
    public static void AttachTree(Transform root)
    {
        foreach (var label in root.GetComponentsInChildren<Text>(true)) Attach(label.gameObject);
        foreach (var label in root.GetComponentsInChildren<TMP_Text>(true)) Attach(label.gameObject);
    }
    private void Awake()
    {
        legacy = GetComponent<Text>(); tmp = GetComponent<TMP_Text>();
        if (legacy != null) legacy.font = Font;
        if (tmp != null)
        {
            if (tmpFont == null)
            {
                tmpFont = TMP_FontAsset.CreateFontAsset(Font, 48, 5, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 2048, 2048);
                tmpFont.isMultiAtlasTexturesEnabled = true;
            }
            tmp.font = tmpFont;
        }
        Refresh();
    }
    private void LateUpdate() => Refresh();
    public void Refresh()
    {
        if (legacy == null && tmp == null) return;
        var current = legacy != null ? legacy.text : tmp.text;
        if (current != rendered) source = current;
        if (current == rendered && lastChinese == PortfolioSettings.Chinese) return;
        lastChinese = PortfolioSettings.Chinese;
        rendered = Translate(source);
        if (legacy != null) legacy.text = rendered; else tmp.text = rendered;
    }
}

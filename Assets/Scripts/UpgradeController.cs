using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeController : MonoBehaviour
{
    private const float ChoiceSpacing = 110f;

    [SerializeField] private PlayerProgress progress;
    [SerializeField] private PlayerUpgradeApplier applier;
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Button[] buttons;
    [SerializeField] private TMP_Text[] labels;
    [SerializeField] private Image[] weaponIcons;
    [SerializeField] private PlayerUpgradeData[] availableUpgrades;

    private readonly PlayerUpgradeData[] currentChoices =
        new PlayerUpgradeData[3];

    public bool IsOpen => upgradePanel != null && upgradePanel.activeSelf;

    private void Awake()
    {
        upgradePanel.SetActive(false);

        for (int i = 0; i < buttons.Length; i++)
        {
            int capturedIndex = i;
            buttons[i].onClick.AddListener(() => Select(capturedIndex));
        }
    }

    private void OnEnable()
    {
        progress.LevelUpRequested += ShowChoices;
    }

    private void OnDisable()
    {
        progress.LevelUpRequested -= ShowChoices;
    }

    private void ShowChoices()
    {
        if (buttons.Length != 3 || labels.Length != 3 ||
            weaponIcons.Length != 3)
        {
            Debug.LogError(
                "UpgradeController requires exactly 3 buttons, labels and icons.",
                this);
            return;
        }

        List<PlayerUpgradeData> weapons = new List<PlayerUpgradeData>();
        List<PlayerUpgradeData> utilities = new List<PlayerUpgradeData>();

        foreach (PlayerUpgradeData upgrade in availableUpgrades)
        {
            if (!applier.CanApply(upgrade))
            {
                continue;
            }

            if (upgrade.EffectType == UpgradeEffectType.EquipWeapon)
            {
                weapons.Add(upgrade);
            }
            else
            {
                utilities.Add(upgrade);
            }
        }

        List<PlayerUpgradeData> choices = new List<PlayerUpgradeData>(3);
        bool isWeaponLevel = progress.Level % 3 == 0;

        if (isWeaponLevel)
        {
            AddRandomChoices(weapons, choices, 3);
        }

        AddRandomChoices(utilities, choices, 3);

        if (choices.Count == 0)
        {
            Debug.LogError("No eligible upgrades are available.", this);
            return;
        }

        LayoutVisibleButtons(choices.Count);

        for (int i = 0; i < currentChoices.Length; i++)
        {
            bool hasChoice = i < choices.Count;
            buttons[i].gameObject.SetActive(hasChoice);
            currentChoices[i] = hasChoice ? choices[i] : null;

            if (!hasChoice)
            {
                continue;
            }

            PlayerUpgradeData choice = currentChoices[i];
            bool isWeapon = choice.EffectType == UpgradeEffectType.EquipWeapon;
            weaponIcons[i].gameObject.SetActive(isWeapon);

            if (isWeapon)
            {
                WeaponDefinition weapon = choice.Weapon;
                weaponIcons[i].sprite = weapon.Icon;
                weaponIcons[i].color = weapon.DisplayColor;
                int count = applier.GetWeaponCount(choice);
                labels[i].text =
                    $"{choice.Title}  {count}/2\n{choice.Description}";
            }
            else
            {
                labels[i].text =
                    $"{choice.Title}\n{choice.Description}";
            }
        }

        upgradePanel.transform.SetAsLastSibling();
        upgradePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void LayoutVisibleButtons(int visibleCount)
    {
        float startY = (visibleCount - 1) * ChoiceSpacing * 0.5f;

        for (int i = 0; i < visibleCount; i++)
        {
            RectTransform rect = buttons[i].GetComponent<RectTransform>();
            Vector2 position = rect.anchoredPosition;
            position.y = startY - i * ChoiceSpacing;
            rect.anchoredPosition = position;
        }
    }

    private static void AddRandomChoices(
        List<PlayerUpgradeData> source,
        List<PlayerUpgradeData> destination,
        int targetCount)
    {
        while (source.Count > 0 && destination.Count < targetCount)
        {
            int randomIndex = Random.Range(0, source.Count);
            destination.Add(source[randomIndex]);
            source.RemoveAt(randomIndex);
        }
    }

    private void Select(int index)
    {
        if (!IsOpen || index < 0 || index >= currentChoices.Length ||
            currentChoices[index] == null)
        {
            return;
        }

        applier.Apply(currentChoices[index]);
        upgradePanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
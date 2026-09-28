using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeController : MonoBehaviour
{
    [SerializeField] private PlayerProgress progress;
    [SerializeField] private PlayerUpgradeApplier applier;
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private TMP_Text headingText;
    [SerializeField] private RectTransform choicesContainer;
    [SerializeField] private Button[] buttons;
    [SerializeField] private TMP_Text[] labels;
    [SerializeField] private Image[] weaponIcons;
    [SerializeField] private PlayerUpgradeData[] availableUpgrades;

    private readonly PlayerUpgradeData[] currentChoices =
        new PlayerUpgradeData[3];

    public bool IsOpen =>
        upgradePanel != null && upgradePanel.activeSelf;

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

        headingText.text = $"LEVEL {progress.Level}  /  CHOOSE ONE";

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
            bool isWeapon =
                choice.EffectType == UpgradeEffectType.EquipWeapon;
            weaponIcons[i].gameObject.SetActive(isWeapon);

            string countText = string.Empty;
            if (isWeapon)
            {
                WeaponDefinition weapon = choice.Weapon;
                weaponIcons[i].sprite = weapon.Icon;
                weaponIcons[i].color = weapon.DisplayColor;
                countText = $"  {applier.GetWeaponCount(choice)}/2";
            }

            labels[i].text =
                $"<b>{choice.Title}</b>{countText}\n" +
                $"<size=22><color=#9FB0BC>{choice.Description}</color></size>";
        }

        upgradePanel.transform.SetAsLastSibling();
        upgradePanel.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(choicesContainer);
        Time.timeScale = 0f;
        SelectButton(buttons[0]);
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
        EventSystem.current?.SetSelectedGameObject(null);
    }

    private static void SelectButton(Button button)
    {
        if (EventSystem.current == null || button == null ||
            !button.gameObject.activeInHierarchy)
        {
            return;
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button.gameObject);
    }
}
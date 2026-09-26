using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeController : MonoBehaviour
{
    [SerializeField] private PlayerProgress progress;
    [SerializeField] private PlayerUpgradeApplier applier;
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Button[] buttons;
    [SerializeField] private TMP_Text[] labels;
    [SerializeField] private PlayerUpgradeData[] availableUpgrades;

    private readonly PlayerUpgradeData[] currentChoices = new PlayerUpgradeData[3];

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
        if (buttons.Length != 3 || labels.Length != 3)
        {
            Debug.LogError("UpgradeController requires exactly 3 buttons and 3 labels.", this);
            return;
        }

        List<PlayerUpgradeData> remaining = new List<PlayerUpgradeData>();
        PlayerUpgradeData guaranteedProjectileUpgrade = null;

        foreach (PlayerUpgradeData upgrade in availableUpgrades)
        {
            if (!applier.CanApply(upgrade, progress.Level))
            {
                continue;
            }

            if (upgrade.EffectType == UpgradeEffectType.ProjectileCount)
            {
                guaranteedProjectileUpgrade = upgrade;
            }
            else
            {
                remaining.Add(upgrade);
            }
        }

        int guaranteedSlot = guaranteedProjectileUpgrade != null ? Random.Range(0, 3) : -1;
        int randomChoicesNeeded = guaranteedProjectileUpgrade != null ? 2 : 3;

        if (remaining.Count < randomChoicesNeeded)
        {
            Debug.LogError("Not enough eligible upgrades to create 3 choices.", this);
            return;
        }

        for (int i = 0; i < 3; i++)
        {
            if (i == guaranteedSlot)
            {
                currentChoices[i] = guaranteedProjectileUpgrade;
            }
            else
            {
                int randomIndex = Random.Range(0, remaining.Count);
                currentChoices[i] = remaining[randomIndex];
                remaining.RemoveAt(randomIndex);
            }

            labels[i].text = $"{currentChoices[i].Title}\n{currentChoices[i].Description}";
        }

        upgradePanel.transform.SetAsLastSibling();
        upgradePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void Select(int index)
    {
        if (!IsOpen || index < 0 || index >= currentChoices.Length)
        {
            return;
        }

        applier.Apply(currentChoices[index]);
        upgradePanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
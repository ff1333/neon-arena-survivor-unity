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

        for ( int i = 0; i < buttons.Length; i++)
        {
            int CapturedIndex = i;
            buttons[i].onClick.AddListener(() => Select(CapturedIndex));
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
        if (availableUpgrades.Length < 3 || buttons.Length != 3 || labels.Length != 3)
        {
            Debug.LogError("UpgradeController needs at least 3 upgrades, 3 buttons and 3 labels.", this);
            return;
        }
        List<PlayerUpgradeData> remaining = new List<PlayerUpgradeData>(availableUpgrades);
        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, remaining.Count);
            currentChoices[i] = remaining[randomIndex];
            remaining.RemoveAt(randomIndex);
            labels[i].text = $"{currentChoices[i].Title}\n{currentChoices[i].Description}";
        }
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

using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Gameplay")]
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerShooter shooter;
    [SerializeField] private EnemySpawner spawner;
    [SerializeField] private Health playerHealth;
    [SerializeField] private PlayerProgress progress;
    [SerializeField] private UpgradeController upgradeController;

    [Header("HUD")]
    [SerializeField] private HudController hud;
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text bestText;
    [SerializeField] private TMP_Text finalStatsText;

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;

    private bool hasStarted;
    private bool isPaused;
    private bool isGameOver;
    private float elapsedTime;
    private int killCount;

    private void Awake()
    {
        Time.timeScale = 0f;
        movement.enabled = false;
        shooter.enabled = false;
        spawner.enabled = false;

        startPanel.SetActive(true);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);

        startButton.onClick.AddListener(StartRun);
        resumeButton.onClick.AddListener(ResumeRun);
        restartButton.onClick.AddListener(RestartRun);
    }

    private void Start()
    {
        hud.Initialize(playerHealth, progress);
        playerHealth.Died += HandlePlayerDied;
        EnemyController.DiedGlobally += HandleEnemyDied;

        float bestTime = PlayerPrefs.GetFloat("BestTime", 0f);
        int bestKills = PlayerPrefs.GetInt("BestKills", 0);
        bestText.text = $"Best Time: {FormatTime(bestTime)} | Best kills {bestKills}";
        hud.SetState("Press Enter or Start");
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.Died -= HandlePlayerDied;
        }
        EnemyController.DiedGlobally -= HandleEnemyDied;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (!hasStarted && keyboard != null && keyboard.enterKey.wasPressedThisFrame)
        {
            StartRun();
        }

        if (hasStarted && !isGameOver && !upgradeController.IsOpen && keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }

        if (isGameOver && keyboard != null && keyboard.rKey.wasPressedThisFrame)
        {
            RestartRun();
        }

        if (hasStarted && !isGameOver && !isPaused && !upgradeController.IsOpen)
        {
            elapsedTime += Time.unscaledDeltaTime;
            hud.SetTimer(elapsedTime);
        } 
    }

    public void StartRun()
    {
        if (hasStarted)
        {
            return;
        }
        hasStarted = true;
        Time.timeScale = 1f;
        movement.enabled = true;
        shooter.enabled = true;
        spawner.enabled = true;
        startPanel.SetActive(false);
        hud.SetState(string.Empty);
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeRun();
            return;
        }
        isPaused = true;
        pausePanel.SetActive(true);
        hud.SetState("Paused");
        Time.timeScale = 0f;
    }

    public void ResumeRun()
    {
        if (!isPaused || isGameOver)
        {
            return;
        }
        isPaused = false;
        pausePanel.SetActive(false);
        hud.SetState(string.Empty);
        Time.timeScale = 1f;
    }
    
    public void RestartRun()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void HandleEnemyDied()
    {
        if (!hasStarted || isGameOver)
        {
            return;
        }
        killCount++;
        hud.SetKills(killCount);
    }

    private void HandlePlayerDied()
    {
        if (isGameOver)
        {
            return;
        }
        isGameOver = true;
        movement.enabled = false;
        shooter.enabled = false;
        spawner.enabled = false;
        Time.timeScale = 0f;

        float bestTime = Mathf.Max(PlayerPrefs.GetFloat("BestTime", 0f), elapsedTime);
        int bestKills = Mathf.Max(PlayerPrefs.GetInt("BestKills", 0), killCount);
        PlayerPrefs.SetFloat("BestTime", bestTime);
        PlayerPrefs.SetInt("BestKills", bestKills);
        PlayerPrefs.Save();

        finalStatsText.text = $"Time: {FormatTime(elapsedTime)} | Kills: {killCount}\n" + $"Best {FormatTime(bestTime)} | Best kills {bestKills}";
        gameOverPanel.SetActive(true);
        hud.SetState("Press R or Restart");
    }

    private static string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.FloorToInt(seconds);
        return $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
    }
}

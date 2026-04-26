using CursedDungeon.CoreAI.CellularAutomata;
using HealthBehaviour = CursedDungeon.Health.Health;
using CursedDungeon.Player;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CursedDungeon.GameLoop
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField]
        private LevelGenerator levelGenerator;

        [SerializeField]
        private GameObject playerPrefab;

        [SerializeField]
        private CameraFollow cameraFollow;

        [SerializeField]
        private Image playerHealthFill;

        [SerializeField]
        private TMP_Text killCountText;

        [SerializeField]
        private Image bossHealthFill;

        [SerializeField]
        private GameObject pausePanel;

        [SerializeField]
        private GameObject winPanel;

        [SerializeField]
        private GameObject losePanel;

        [SerializeField]
        private string mainMenuSceneName;

        private bool isGameOver;
        private int killCount;
        private int totalEnemies;
        private bool paused;
        private HealthBehaviour playerHealth;
        private HealthBehaviour bossHealth;
        private GameObject playerInstance;

        public Transform PlayerTransform { get; private set; }
        public int KillCount => killCount;
        public int TotalEnemies => totalEnemies;

        private void Awake()
        {
            Instance = this;
            levelGenerator.OnLevelReady += HandleLevelReady;
        }

        private void OnDestroy()
        {
            levelGenerator.OnLevelReady -= HandleLevelReady;
            UnsubscribePlayerHealth();
            UnsubscribeBossHealth();
            Instance = null;
        }

        private void Update()
        {
            if (isGameOver) return;
            if (Keyboard.current.escapeKey.wasPressedThisFrame) TogglePause();
        }

        public void RegisterEnemy()
        {
            totalEnemies++;
            RefreshKillUi();
        }

        private void HandleLevelReady()
        {
            killCount = 0;
            totalEnemies = 0;
            UnsubscribePlayerHealth();
            if (playerInstance != null) Destroy(playerInstance);

            var go = Instantiate(playerPrefab, levelGenerator.PlayerSpawn, Quaternion.identity);
            playerInstance = go;
            PlayerTransform = go.transform;
            playerHealth = go.GetComponent<HealthBehaviour>();
            playerHealth.OnHealthChanged += OnPlayerHealthChanged;
            playerHealth.OnDied += OnPlayerDied;
            OnPlayerHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);

            cameraFollow.SetTarget(PlayerTransform);
            RefreshKillUi();
            if (bossHealthFill != null) bossHealthFill.gameObject.SetActive(false);
        }

        private void OnPlayerHealthChanged(int current, int max)
        {
            if (playerHealthFill == null || max <= 0) return;
            playerHealthFill.fillAmount = (float)current / max;
        }

        public void OnPlayerDied()
        {
            if (isGameOver) return;
            LoseGame();
        }

        public void OnEnemyKilled()
        {
            killCount++;
            RefreshKillUi();
        }

        private void RefreshKillUi()
        {
            if (killCountText == null) return;
            killCountText.text = totalEnemies > 0
                ? $"Kills: {killCount}/{totalEnemies}"
                : $"Kills: {killCount}";
        }

        public void OnBossKilled()
        {
            UnsubscribeBossHealth();
            if (bossHealthFill != null) bossHealthFill.gameObject.SetActive(false);
            OnEnemyKilled();
        }

        public void OnExitReached()
        {
            if (isGameOver) return;
            if (totalEnemies > 0 && killCount < totalEnemies) return;
            WinGame();
        }

        public void RegisterBossHealth(HealthBehaviour boss)
        {
            UnsubscribeBossHealth();
            bossHealth = boss;
            bossHealth.OnHealthChanged += OnBossHealthChanged;

            if (bossHealthFill != null)
            {
                bossHealthFill.gameObject.SetActive(true);
                OnBossHealthChanged(bossHealth.CurrentHealth, bossHealth.MaxHealth);
            }
        }

        private void OnBossHealthChanged(int current, int max)
        {
            if (bossHealthFill == null || max <= 0) return;
            bossHealthFill.fillAmount = (float)current / max;
        }

        private void UnsubscribeBossHealth()
        {
            if (bossHealth == null) return;
            bossHealth.OnHealthChanged -= OnBossHealthChanged;
            bossHealth = null;
        }

        public void TogglePause()
        {
            if (isGameOver) return;
            paused = !paused;
            Time.timeScale = paused ? 0 : 1;
            if (pausePanel != null) pausePanel.SetActive(paused);
        }

        private void WinGame()
        {
            if (isGameOver) return;
            Debug.Log("You won!");
            isGameOver = true;
            paused = false;
            Time.timeScale = 1;
            UnsubscribeBossHealth();

            if (bossHealthFill != null) bossHealthFill.gameObject.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (winPanel != null) winPanel.SetActive(true);
        }

        private void LoseGame()
        {
            if (isGameOver) return;
            isGameOver = true;
            paused = false;
            Time.timeScale = 1;
            UnsubscribeBossHealth();

            if (bossHealthFill != null) bossHealthFill.gameObject.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (losePanel != null) losePanel.SetActive(true);
        }

        public void RestartLevel()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void LoadMainMenu()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene("MainMenu");
        }

        private void UnsubscribePlayerHealth()
        {
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= OnPlayerHealthChanged;
                playerHealth.OnDied -= OnPlayerDied;
                playerHealth = null;
            }

            PlayerTransform = null;
        }
    }
}

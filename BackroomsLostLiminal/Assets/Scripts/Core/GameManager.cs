using UnityEngine;
using UnityEngine.SceneManagement;

namespace Backrooms.Core
{
    /// <summary>
    /// Gerenciador principal do jogo - Singleton que controla o estado global
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private string currentLevelName = "Level0";
        [SerializeField] private int currentLevelIndex = 0;
        
        [Header("Player Data")]
        [SerializeField] private float playerSanity = 100f;
        [SerializeField] private float playerHealth = 100f;
        [SerializeField] private float playerStamina = 100f;
        
        [Header("Game Settings")]
        [SerializeField] private GameDifficulty difficulty = GameDifficulty.Normal;
        [SerializeField] private bool hardcoreMode = false;
        [SerializeField] private bool permadeathEnabled = false;

        // Events
        public delegate void SanityChangedHandler(float newSanity);
        public event SanityChangedHandler OnSanityChanged;

        public delegate void HealthChangedHandler(float newHealth);
        public event HealthChangedHandler OnHealthChanged;

        public delegate void LevelChangedHandler(string newLevelName);
        public event LevelChangedHandler OnLevelChanged;

        public enum GameDifficulty
        {
            Easy,
            Normal,
            Hard,
            Hardcore
        }

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            InitializeGame();
        }

        /// <summary>
        /// Inicializa o jogo com configurações padrão
        /// </summary>
        public void InitializeGame()
        {
            playerSanity = 100f;
            playerHealth = 100f;
            playerStamina = 100f;
            currentLevelIndex = 0;
            currentLevelName = "Level0_Lobby";
            
            Debug.Log("[GameManager] Jogo inicializado - Bem-vindo às Backrooms");
        }

        /// <summary>
        /// Carrega uma cena específica
        /// </summary>
        public void LoadLevel(string levelName)
        {
            StartCoroutine(LoadLevelAsync(levelName));
        }

        /// <summary>
        /// Carrega cena de forma assíncrona com tela de loading
        /// </summary>
        private System.Collections.IEnumerator LoadLevelAsync(string levelName)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelName);
            
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            currentLevelName = levelName;
            OnLevelChanged?.Invoke(levelName);
            
            Debug.Log($"[GameManager] Nível carregado: {levelName}");
        }

        /// <summary>
        /// Modifica a sanidade do jogador
        /// </summary>
        public void ModifySanity(float amount)
        {
            playerSanity = Mathf.Clamp(playerSanity + amount, 0f, 100f);
            OnSanityChanged?.Invoke(playerSanity);

            if (playerSanity <= 0f)
            {
                TriggerInsanityEffect();
            }
        }

        /// <summary>
        /// Modifica a saúde do jogador
        /// </summary>
        public void ModifyHealth(float amount)
        {
            playerHealth = Mathf.Clamp(playerHealth + amount, 0f, 100f);
            OnHealthChanged?.Invoke(playerHealth);

            if (playerHealth <= 0f)
            {
                PlayerDeath();
            }
        }

        /// <summary>
        /// Modifica a stamina do jogador
        /// </summary>
        public void ModifyStamina(float amount)
        {
            playerStamina = Mathf.Clamp(playerStamina + amount, 0f, 100f);
        }

        /// <summary>
        /// Efeitos de insanidade quando sanidade chega a zero
        /// </summary>
        private void TriggerInsanityEffect()
        {
            Debug.LogWarning("[GameManager] Jogador atingiu insanidade total!");
            // TODO: Implementar alucinações visuais e auditivas
            // TODO: Controles invertidos temporariamente
            // TODO: Sombras se movendo
        }

        /// <summary>
        /// Lida com a morte do jogador
        /// </summary>
        private void PlayerDeath()
        {
            Debug.LogError("[GameManager] Jogador morreu!");
            
            if (permadeathEnabled || hardcoreMode)
            {
                DeleteAllSaveData();
                LoadLevel("MainMenu");
            }
            else
            {
                LoadLastCheckpoint();
            }
        }

        /// <summary>
        /// Salva o progresso em checkpoint
        /// </summary>
        public void SaveCheckpoint(string checkpointName)
        {
            SaveSystem.SaveCheckpoint(checkpointName, currentLevelName, playerSanity, playerHealth);
            Debug.Log($"[GameManager] Checkpoint salvo: {checkpointName}");
        }

        /// <summary>
        /// Carrega último checkpoint
        /// </summary>
        private void LoadLastCheckpoint()
        {
            SaveData data = SaveSystem.LoadLastCheckpoint();
            if (data != null)
            {
                currentLevelName = data.levelName;
                playerSanity = data.sanity;
                playerHealth = data.health;
                
                LoadLevel(currentLevelName);
            }
            else
            {
                LoadLevel("Level0_Lobby");
            }
        }

        /// <summary>
        /// Deleta todos os dados salvos (hardcore mode)
        /// </summary>
        private void DeleteAllSaveData()
        {
            SaveSystem.DeleteAllSaves();
            Debug.LogWarning("[GameManager] Todos os saves foram deletados (Permadeath)");
        }

        // Getters
        public float CurrentSanity => playerSanity;
        public float CurrentHealth => playerHealth;
        public float CurrentStamina => playerStamina;
        public string CurrentLevel => currentLevelName;
        public GameDifficulty Difficulty => difficulty;
        public bool IsHardcoreMode => hardcoreMode;
    }

    /// <summary>
    /// Dados de save para checkpoints
    /// </summary>
    [System.Serializable]
    public class SaveData
    {
        public string checkpointName;
        public string levelName;
        public float sanity;
        public float health;
        public float stamina;
        public string[] inventoryItems;
    }
}

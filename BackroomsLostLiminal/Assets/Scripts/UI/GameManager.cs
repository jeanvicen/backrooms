using UnityEngine;
using UnityEngine.SceneManagement;

namespace Backrooms.UI
{
    /// <summary>
    /// GameManager central que controla estado do jogo, transições de nível,
    /// pause menu, e coordena todos os sistemas principais.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Configurações do Jogo")]
        [SerializeField] private string gameTitle = "Backrooms: Lost in the Liminal";
        [SerializeField] private int startingSceneIndex = 0;
        
        [Header("Referências")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private SanitySystem sanitySystem;
        [SerializeField] private Flashlight flashlight;
        [SerializeField] private InteractionSystem interactionSystem;
        
        [Header("UI")]
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject hudCanvas;
        
        // Variáveis privadas
        private static GameManager instance;
        private bool isPaused = false;
        private bool isGameStarted = false;
        
        // Propriedades estáticas para acesso global
        public static GameManager Instance => instance;
        public static bool IsPaused { get; private set; }
        public static bool IsGameStarted { get; private set; }
        
        // Eventos globais
        public delegate void GameStateChangedHandler(bool isPaused);
        public static event GameStateChangedHandler OnGamePaused;
        public static event GameStateChangedHandler OnGameResumed;
        
        public delegate void SceneChangedHandler(string sceneName, int sceneIndex);
        public static event SceneChangedHandler OnSceneChanged;

        void Awake()
        {
            // Singleton pattern
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            Application.targetFrameRate = 60;
        }

        void Start()
        {
            // Inicia no menu principal ou cena inicial
            ShowMainMenu();
        }

        void Update()
        {
            HandleInput();
        }

        void HandleInput()
        {
            // Toggle pause com ESC
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isGameStarted && !isPaused)
                {
                    PauseGame();
                }
                else if (isPaused)
                {
                    ResumeGame();
                }
            }
        }

        #region Menu Management
        
        public void ShowMainMenu()
        {
            Time.timeScale = 1f;
            isGameStarted = false;
            IsGameStarted = false;
            isPaused = false;
            IsPaused = false;
            
            if (mainMenu != null)
            {
                mainMenu.SetActive(true);
            }
            
            if (hudCanvas != null)
            {
                hudCanvas.SetActive(false);
            }
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void StartNewGame()
        {
            LoadScene(startingSceneIndex);
            
            isGameStarted = true;
            IsGameStarted = true;
            isPaused = false;
            IsPaused = false;
            
            if (mainMenu != null)
            {
                mainMenu.SetActive(false);
            }
            
            if (hudCanvas != null)
            {
                hudCanvas.SetActive(true);
            }
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void ContinueGame()
        {
            ResumeGame();
        }

        #endregion

        #region Pause System
        
        public void PauseGame()
        {
            if (!isGameStarted) return;
            
            isPaused = true;
            IsPaused = true;
            Time.timeScale = 0f;
            
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(true);
            }
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            OnGamePaused?.Invoke(true);
        }

        public void ResumeGame()
        {
            if (!isPaused) return;
            
            isPaused = false;
            IsPaused = false;
            Time.timeScale = 1f;
            
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(false);
            }
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            OnGameResumed?.Invoke(false);
        }

        public void TogglePause()
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        #endregion

        #region Scene Management
        
        public void LoadScene(int sceneIndex)
        {
            StartCoroutine(LoadSceneAsync(sceneIndex));
        }

        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }

        System.Collections.IEnumerator LoadSceneAsync(int sceneIndex)
        {
            // Fade out (implementar sistema de fade)
            yield return new WaitForSeconds(0.5f);
            
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
            asyncLoad.allowSceneActivation = false;
            
            while (asyncLoad.progress < 0.9f)
            {
                yield return null;
            }
            
            // Carrega a cena
            asyncLoad.allowSceneActivation = true;
            
            yield return new WaitForSeconds(0.5f);
            
            // Fade in
            OnSceneChanged?.Invoke(SceneManager.GetActiveScene().name, sceneIndex);
        }

        System.Collections.IEnumerator LoadSceneAsync(string sceneName)
        {
            yield return LoadSceneAsync(SceneManager.GetSceneByName(sceneName).buildIndex);
        }

        public void LoadNextScene()
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex + 1;
            
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                LoadScene(nextSceneIndex);
            }
        }

        public void ReloadCurrentScene()
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            LoadScene(currentSceneIndex);
        }

        #endregion

        #region Game Over & Victory
        
        public void TriggerGameOver(string reason = "")
        {
            Debug.Log($"Game Over: {reason}");
            
            // Mostra tela de game over
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // Implementar UI de Game Over
        }

        public void TriggerVictory()
        {
            Debug.Log("Vitória! Você escapou das Backrooms!");
            
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // Implementar UI de Vitória / Créditos
        }

        #endregion

        #region Settings
        
        public void SetGraphicsQuality(int qualityLevel)
        {
            QualitySettings.SetQualityLevel(qualityLevel);
        }

        public void SetMasterVolume(float volume)
        {
            AudioListener.volume = Mathf.Clamp01(volume);
        }

        public void SetFullscreen(bool fullscreen)
        {
            Screen.fullScreen = fullscreen;
        }

        public void SetResolution(int width, int height)
        {
            Screen.SetResolution(width, height, Screen.fullScreen);
        }

        #endregion

        public void QuitGame()
        {
            Debug.Log("Saindo do jogo...");
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // Cleanup
        void OnDestroy()
        {
            Time.timeScale = 1f;
        }

        void OnApplicationQuit()
        {
            Time.timeScale = 1f;
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && isGameStarted && !isPaused)
            {
                PauseGame();
            }
        }
    }
}

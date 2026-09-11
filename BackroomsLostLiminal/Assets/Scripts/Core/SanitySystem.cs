using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Backrooms.Core
{
    /// <summary>
    /// Sistema de Sanidade que afeta a percepção do jogador, causa alucinações
    /// e altera o comportamento do jogo baseado no nível de sanidade mental.
    /// </summary>
    public class SanitySystem : MonoBehaviour
    {
        [Header("Configurações de Sanidade")]
        [SerializeField] private float maxSanity = 100f;
        [SerializeField] private float currentSanity;
        [SerializeField] private float sanityDrainRate = 5f;
        [SerializeField] private float sanityRecoveryRate = 3f;
        
        [Header("Limites para Efeitos")]
        [SerializeField] private float lowSanityThreshold = 40f;
        [SerializeField] private float criticalSanityThreshold = 20f;
        [SerializeField] private float hallucinationThreshold = 30f;
        
        [Header("Drenadores de Sanidade")]
        [SerializeField] private float darknessDrainRate = 8f;
        [SerializeField] private float entitySeenDrainAmount = 15f;
        [SerializeField] private float jumpscareDrainAmount = 25f;
        [SerializeField] private float lostDrainRate = 3f;
        
        [Header("Referências")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private PlayerController playerController;
        
        [Header("Efeitos Visuais")]
        [SerializeField] private GameObject vignetteEffect;
        [SerializeField] private GameObject chromaticAberrationEffect;
        [SerializeField] private GameObject filmGrainEffect;
        [SerializeField] private GameObject distortionEffect;
        
        [Header("Áudio")]
        [SerializeField] private AudioClip[] whisperSounds;
        [SerializeField] private AudioClip heartbeatSound;
        [SerializeField] private AudioClip staticSound;
        
        [Header("Alucinações")]
        [SerializeField] private GameObject[] hallucinationPrefabs;
        [SerializeField] private float minHallucinationInterval = 10f;
        [SerializeField] private float maxHallucinationInterval = 30f;
        
        // Variáveis privadas
        private bool isDarkness;
        private bool isEntityNearby;
        private bool isLost;
        private float lastHallucinationTime;
        private Coroutine hallucinationCoroutine;
        private Coroutine audioCoroutine;
        private float currentLightLevel = 1f;
        
        // Propriedades
        public float CurrentSanity => currentSanity;
        public float MaxSanity => maxSanity;
        public float SanityPercentage => (currentSanity / maxSanity) * 100f;
        public bool IsLowSanity => currentSanity < lowSanityThreshold;
        public bool IsCriticalSanity => currentSanity < criticalSanityThreshold;
        public bool CanHaveHallucinations => currentSanity < hallucinationThreshold;
        
        // Eventos
        public delegate void SanityChangedHandler(float current, float max, float percentage);
        public event SanityChangedHandler OnSanityChanged;
        
        public delegate void SanityThresholdReachedHandler(float threshold);
        public event SanityThresholdReachedHandler OnLowSanityReached;
        public event SanityThresholdReachedHandler OnCriticalSanityReached;
        public event SanityThresholdReachedHandler OnHallucinationsEnabled;
        
        public delegate void HallucinationSpawnedHandler(GameObject hallucination);
        public event HallucinationSpawnedHandler OnHallucinationSpawned;

        void Awake()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
            
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            
            if (playerController == null)
                playerController = GetComponent<PlayerController>();
        }

        void Start()
        {
            currentSanity = maxSanity;
            lastHallucinationTime = Time.time;
            
            // Inicia corrotinas de efeitos
            if (audioSource != null)
            {
                audioCoroutine = StartCoroutine(PlaySanityAudioLoop());
            }
            
            if (CanHaveHallucinations)
            {
                hallucinationCoroutine = StartCoroutine(SpawnHallucinationsLoop());
            }
        }

        void Update()
        {
            HandleSanityDrain();
            HandleVisualEffects();
            CheckThresholds();
        }

        void HandleSanityDrain()
        {
            float totalDrain = 0f;
            
            // Drenagem por escuridão
            if (isDarkness)
            {
                totalDrain += darknessDrainRate;
            }
            
            // Drenagem por entidade próxima
            if (isEntityNearby)
            {
                totalDrain += sanityDrainRate * 2f;
            }
            
            // Drenagem por estar perdido
            if (isLost)
            {
                totalDrain += lostDrainRate;
            }
            
            // Aplica drenagem ou recuperação
            if (totalDrain > 0)
            {
                currentSanity -= totalDrain * Time.deltaTime;
            }
            else if (!isDarkness && !isEntityNearby)
            {
                // Recuperação natural em áreas claras e seguras
                currentSanity += sanityRecoveryRate * Time.deltaTime;
            }
            
            // Limita valores
            currentSanity = Mathf.Clamp(currentSanity, 0f, maxSanity);
            
            // Notifica mudança
            OnSanityChanged?.Invoke(currentSanity, maxSanity, SanityPercentage);
            
            // Gerencia corrotina de alucinações
            ManageHallucinationCoroutine();
        }

        void CheckThresholds()
        {
            static float lastLowTrigger = -1f;
            static float lastCriticalTrigger = -1f;
            static float lastHallucinationTrigger = -1f;
            
            if (currentSanity < lowSanityThreshold && lastLowTrigger != lowSanityThreshold)
            {
                OnLowSanityReached?.Invoke(lowSanityThreshold);
                lastLowTrigger = lowSanityThreshold;
            }
            
            if (currentSanity < criticalSanityThreshold && lastCriticalTrigger != criticalSanityThreshold)
            {
                OnCriticalSanityReached?.Invoke(criticalSanityThreshold);
                TriggerInvertedControls();
                lastCriticalTrigger = criticalSanityThreshold;
            }
            
            if (currentSanity < hallucinationThreshold && lastHallucinationTrigger != hallucinationThreshold)
            {
                OnHallucinationsEnabled?.Invoke(hallucinationThreshold);
                lastHallucinationTrigger = hallucinationThreshold;
            }
        }

        void HandleVisualEffects()
        {
            float sanityRatio = currentSanity / maxSanity;
            
            // Ajusta intensidade dos efeitos baseado na sanidade
            if (vignetteEffect != null)
            {
                var vignette = vignetteEffect.GetComponent<UnityEngine.Rendering.Volume>();
                if (vignette != null)
                {
                    // Intensidade aumenta conforme sanidade diminui
                    float intensity = 1f - sanityRatio;
                    // Aplicar intensidade ao efeito (implementação depende do URP)
                }
            }
            
            if (chromaticAberrationEffect != null && IsLowSanity)
            {
                var aberration = chromaticAberrationEffect.GetComponent<UnityEngine.Rendering.Volume>();
                if (aberration != null)
                {
                    float intensity = (lowSanityThreshold - currentSanity) / lowSanityThreshold;
                    intensity = Mathf.Clamp(intensity, 0f, 1f);
                    // Aplicar intensidade ao efeito
                }
            }
            
            // Tremor de câmera em sanidade crítica
            if (IsCriticalSanity)
            {
                ApplyCameraShake();
            }
        }

        void ApplyCameraShake()
        {
            if (mainCamera == null) return;
            
            float shakeAmount = (criticalSanityThreshold - currentSanity) / criticalSanityThreshold * 0.1f;
            float shakeX = Random.Range(-shakeAmount, shakeAmount);
            float shakeY = Random.Range(-shakeAmount, shakeAmount);
            
            mainCamera.transform.localPosition += new Vector3(shakeX, shakeY, 0f);
        }

        IEnumerator PlaySanityAudioLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(5f);
                
                if (IsLowSanity && audioSource != null)
                {
                    // Toca sussurros aleatórios
                    if (whisperSounds.Length > 0 && Random.value < 0.3f)
                    {
                        AudioClip whisper = whisperSounds[Random.Range(0, whisperSounds.Length)];
                        audioSource.PlayOneShot(whisper, 0.5f * (1f - SanityPercentage / 100f));
                    }
                    
                    // Toca batimento cardíaco em sanidade crítica
                    if (IsCriticalSanity && heartbeatSound != null)
                    {
                        audioSource.PlayOneShot(heartbeatSound, 0.7f);
                    }
                }
            }
        }

        IEnumerator SpawnHallucinationsLoop()
        {
            while (CanHaveHallucinations)
            {
                float waitTime = Random.Range(minHallucinationInterval, maxHallucinationInterval);
                waitTime *= (currentSanity / hallucinationThreshold); // Mais frequente com menos sanidade
                
                yield return new WaitForSeconds(waitTime);
                
                if (CanHaveHallucinations && hallucinationPrefabs.Length > 0)
                {
                    SpawnRandomHallucination();
                }
            }
        }

        void SpawnRandomHallucination()
        {
            if (hallucinationPrefabs.Length == 0 || mainCamera == null) return;
            
            GameObject hallucinationPrefab = hallucinationPrefabs[Random.Range(0, hallucinationPrefabs.Length)];
            
            // Spawn na frente do jogador, mas não muito perto
            Vector3 spawnDirection = mainCamera.transform.forward;
            float spawnDistance = Random.Range(5f, 15f);
            Vector3 spawnPosition = mainCamera.transform.position + spawnDirection * spawnDistance;
            
            // Adiciona variação lateral
            spawnPosition += mainCamera.transform.right * Random.Range(-3f, 3f);
            
            GameObject hallucination = Instantiate(hallucinationPrefab, spawnPosition, Quaternion.identity);
            
            // Destruir após alguns segundos
            Destroy(hallucination, 5f);
            
            OnHallucinationSpawned?.Invoke(hallucination);
        }

        void ManageHallucinationCoroutine()
        {
            if (CanHaveHallucinations && hallucinationCoroutine == null)
            {
                hallucinationCoroutine = StartCoroutine(SpawnHallucinationsLoop());
            }
            else if (!CanHaveHallucinations && hallucinationCoroutine != null)
            {
                StopCoroutine(hallucinationCoroutine);
                hallucinationCoroutine = null;
            }
        }

        // Métodos públicos para modificar sanidade
        
        public void DrainSanity(float amount, string reason = "")
        {
            currentSanity -= amount;
            currentSanity = Mathf.Max(0f, currentSanity);
            OnSanityChanged?.Invoke(currentSanity, maxSanity, SanityPercentage);
            
            Debug.Log($"Sanidade drenada: {amount} ({reason})");
        }

        public void RecoverSanity(float amount)
        {
            currentSanity += amount;
            currentSanity = Mathf.Min(maxSanity, currentSanity);
            OnSanityChanged?.Invoke(currentSanity, maxSanity, SanityPercentage);
        }

        public void SetDarkness(bool isDark)
        {
            isDarkness = isDark;
        }

        public void SetEntityNearby(bool hasEntity)
        {
            isEntityNearby = hasEntity;
            
            if (hasEntity)
            {
                DrainSanity(entitySeenDrainAmount, "Entidade vista");
            }
        }

        public void SetLost(bool isLostState)
        {
            isLost = isLostState;
        }

        public void TriggerJumpscare()
        {
            DrainSanity(jumpscareDrainAmount, "Jumpscare");
        }

        public void TriggerInvertedControls()
        {
            // Implementado no PlayerInputHandler
            StartCoroutine(InvertControlsTemporarily());
        }

        IEnumerator InvertControlsTemporarily()
        {
            // Aplica inversão de controles por tempo limitado
            float duration = 5f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Restaura controles normais
            Debug.Log("Controles restaurados ao normal");
        }

        // Uso de itens que afetam sanidade
        public void UseSanityItem(string itemName, float sanityRestored)
        {
            RecoverSanity(sanityRestored);
            Debug.Log($"Usou {itemName}, recuperou {sanityRestored} de sanidade");
        }

        // Debug
        void OnGUI()
        {
            if (Application.isPlaying)
            {
                GUILayout.BeginArea(new Rect(10, 10, 200, 100));
                GUILayout.Label($"Sanidade: {currentSanity:F1}/{maxSanity}");
                GUILayout.Label($"Porcentagem: {SanityPercentage:F1}%");
                GUILayout.Label($"Estado: {(IsCriticalSanity ? "CRÍTICO" : IsLowSanity ? "BAIXO" : "NORMAL")}");
                GUILayout.EndArea();
            }
        }
    }
}

using UnityEngine;

namespace Backrooms.Player
{
    /// <summary>
    /// Gerencia o sistema de sanidade do jogador
    /// Controla efeitos visuais, auditivos e alucinações
    /// </summary>
    public class SanityManager : MonoBehaviour
    {
        [Header("Sanity Settings")]
        [SerializeField] private float maxSanity = 100f;
        [SerializeField] private float currentSanity = 100f;
        
        [Header("Sanity Drain Rates")]
        [SerializeField] private float darknessDrainRate = 5f;
        [SerializeField] private float entityDrainRate = 15f;
        [SerializeField] private float lostDrainRate = 2f;
        [SerializeField] private float scareDrainAmount = 20f;
        
        [Header("Sanity Recovery")]
        [SerializeField] private float lightRecoveryRate = 8f;
        [SerializeField] private float safeZoneRecoveryRate = 15f;
        [SerializeField] private float itemRecoveryAmount = 25f;
        
        [Header("Sanity Thresholds")]
        [SerializeField] private float highSanityThreshold = 75f;
        [SerializeField] private float mediumSanityThreshold = 50f;
        [SerializeField] private float lowSanityThreshold = 25f;
        [SerializeField] private float criticalSanityThreshold = 10f;
        
        [Header("Visual Effects")]
        [SerializeField] private GameObject sanityEffectsPrefab;
        [SerializeField] private AudioClip[] hallucinationSounds;
        [SerializeField] private float hallucinationInterval = 30f;
        
        [Header("References")]
        [SerializeField] private Light playerLight;
        [SerializeField] private AudioSource audioSource;
        
        // State
        private bool isInSafeZone = false;
        private bool isNearLight = false;
        private bool hasHallucinations = false;
        private float timeSinceLastHallucination = 0f;
        private SanityState currentState = SanityState.Normal;
        
        // Events
        public delegate void SanityStateChangedHandler(SanityState newState, float sanityPercent);
        public event SanityStateChangedHandler OnSanityStateChanged;
        
        public delegate void HallucinationTriggeredHandler(HallucinationType type);
        public event HallucinationTriggeredHandler OnHallucinationTriggered;
        
        public enum SanityState
        {
            Normal,      // 75-100%
            Concerned,   // 50-75%
            Unstable,    // 25-50%
            Low,         // 10-25%
            Critical     // 0-10%
        }
        
        public enum HallucinationType
        {
            Visual,      // Sombras se movendo
            Auditory,    // Sons fantasmas
            Inverted,    // Controles invertidos
            Whisper      // Sussurros
        }
        
        public float CurrentSanity => currentSanity;
        public float SanityPercent => currentSanity / maxSanity;
        public SanityState CurrentState => currentState;
        public bool HasActiveHallucinations => hasHallucinations;

        private void Start()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
            
            UpdateSanityState();
        }

        private void Update()
        {
            UpdateSanityDrain();
            UpdateHallucinations();
        }

        /// <summary>
        /// Atualiza drenagem de sanidade baseada no ambiente
        /// </summary>
        private void UpdateSanityDrain()
        {
            float drainAmount = 0f;
            
            // Drenagem por escuridão
            if (!isNearLight && !isInSafeZone)
            {
                drainAmount += darknessDrainRate * Time.deltaTime;
            }
            
            // Recuperação em zona segura
            if (isInSafeZone)
            {
                currentSanity += safeZoneRecoveryRate * Time.deltaTime;
            }
            // Recuperação perto da luz
            else if (isNearLight)
            {
                currentSanity += lightRecoveryRate * Time.deltaTime;
            }
            // Drenagem por estar perdido/sozinho
            else
            {
                drainAmount += lostDrainRate * Time.deltaTime;
            }
            
            // Aplicar mudanças
            currentSanity -= drainAmount;
            currentSanity = Mathf.Clamp(currentSanity, 0f, maxSanity);
            
            // Verificar mudança de estado
            UpdateSanityState();
        }

        /// <summary>
        /// Atualiza estado das alucinações
        /// </summary>
        private void UpdateHallucinations()
        {
            timeSinceLastHallucination += Time.deltaTime;
            
            // Determinar se deve ter alucinações baseado na sanidade
            bool shouldHaveHallucinations = currentSanity < lowSanityThreshold;
            
            if (shouldHaveHallucinations && !hasHallucinations)
            {
                StartHallucinations();
            }
            else if (!shouldHaveHallucinations && hasHallucinations)
            {
                StopHallucinations();
            }
            
            // Trigger alucinações periódicas em sanidade baixa
            if (hasHallucinations && timeSinceLastHallucination >= hallucinationInterval)
            {
                TriggerRandomHallucination();
                timeSinceLastHallucination = 0f;
            }
        }

        /// <summary>
        /// Inicia efeito de alucinações
        /// </summary>
        private void StartHallucinations()
        {
            hasHallucinations = true;
            Debug.LogWarning("[SanityManager] Alucinações iniciadas - Sanidade crítica!");
        }

        /// <summary>
        /// Para efeito de alucinações
        /// </summary>
        private void StopHallucinations()
        {
            hasHallucinations = false;
            Debug.Log("[SanityManager] Alucinações cessaram");
        }

        /// <summary>
        /// Trigger uma alucinação aleatória
        /// </summary>
        private void TriggerRandomHallucination()
        {
            HallucinationType type = (HallucinationType)Random.Range(0, System.Enum.GetNames(typeof(HallucinationType)).Length);
            TriggerHallucination(type);
        }

        /// <summary>
        /// Trigger um tipo específico de alucinação
        /// </summary>
        public void TriggerHallucination(HallucinationType type)
        {
            OnHallucinationTriggered?.Invoke(type);
            
            switch (type)
            {
                case HallucinationType.Visual:
                    TriggerVisualHallucination();
                    break;
                case HallucinationType.Auditory:
                    TriggerAuditoryHallucination();
                    break;
                case HallucinationType.Inverted:
                    TriggerInvertedControls();
                    break;
                case HallucinationType.Whisper:
                    TriggerWhispers();
                    break;
            }
        }

        private void TriggerVisualHallucination()
        {
            Debug.Log("[SanityManager] Alucinação visual: sombras se movendo");
            // TODO: Implementar shaders de distorção visual
        }

        private void TriggerAuditoryHallucination()
        {
            if (hallucinationSounds.Length > 0 && audioSource != null)
            {
                AudioClip sound = hallucinationSounds[Random.Range(0, hallucinationSounds.Length)];
                audioSource.PlayOneShot(sound);
            }
            Debug.Log("[SanityManager] Alucinação auditiva: sons fantasmas");
        }

        private void TriggerInvertedControls()
        {
            Debug.Log("[SanityManager] Alucinação: controles invertidos temporariamente");
            // TODO: Implementar inversão de controles por tempo limitado
        }

        private void TriggerWhispers()
        {
            Debug.Log("[SanityManager] Alucinação: sussurros nas paredes");
            // TODO: Implementar áudio posicional de sussurros
        }

        /// <summary>
        /// Atualiza estado atual da sanidade
        /// </summary>
        private void UpdateSanityState()
        {
            SanityState newState = CalculateSanityState();
            
            if (newState != currentState)
            {
                currentState = newState;
                OnSanityStateChanged?.Invoke(currentState, SanityPercent);
                Debug.Log($"[SanityManager] Estado mudou para: {currentState}");
            }
        }

        /// <summary>
        /// Calcula estado baseado nos thresholds
        /// </summary>
        private SanityState CalculateSanityState()
        {
            if (currentSanity >= highSanityThreshold) return SanityState.Normal;
            if (currentSanity >= mediumSanityThreshold) return SanityState.Concerned;
            if (currentSanity >= lowSanityThreshold) return SanityState.Unstable;
            if (currentSanity >= criticalSanityThreshold) return SanityState.Low;
            return SanityState.Critical;
        }

        /// <summary>
        /// Modifica sanidade por uma quantidade específica
        /// </summary>
        public void ModifySanity(float amount)
        {
            currentSanity += amount;
            currentSanity = Mathf.Clamp(currentSanity, 0f, maxSanity);
            UpdateSanityState();
        }

        /// <summary>
        /// Drena sanidade ao ver entidade
        /// </summary>
        public void DrainFromEntity()
        {
            ModifySanity(-entityDrainRate * Time.deltaTime);
        }

        /// <summary>
        /// Drena sanidade por evento assustador
        /// </summary>
        public void DrainFromScare()
        {
            ModifySanity(-scareDrainAmount);
        }

        /// <summary>
        /// Define se jogador está em zona segura
        /// </summary>
        public void SetInSafeZone(bool inSafeZone)
        {
            isInSafeZone = inSafeZone;
            Debug.Log($"[SanityManager] Zona segura: {inSafeZone}");
        }

        /// <summary>
        /// Define se jogador está perto de luz
        /// </summary>
        public void SetNearLight(bool nearLight)
        {
            isNearLight = nearLight;
        }

        /// <summary>
        /// Usa item de recuperação de sanidade
        /// </summary>
        public void UseSanityItem()
        {
            ModifySanity(itemRecoveryAmount);
            Debug.Log("[SanityManager] Item de sanidade usado");
        }

        /// <summary>
        /// Recupera sanidade completamente (para debug)
        /// </summary>
        public void RestoreFullSanity()
        {
            currentSanity = maxSanity;
            StopHallucinations();
            UpdateSanityState();
        }
    }
}

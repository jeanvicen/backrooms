using UnityEngine;

namespace Backrooms.Player
{
    /// <summary>
    /// Gerencia a lanterna do jogador e sistema de iluminação pessoal
    /// </summary>
    public class FlashlightManager : MonoBehaviour
    {
        [Header("Flashlight Settings")]
        [SerializeField] private Light flashlightLight;
        [SerializeField] private GameObject flashlightModel;
        [SerializeField] private KeyCode toggleKey = KeyCode.F;
        
        [Header("Battery System")]
        [SerializeField] private float maxBattery = 100f;
        [SerializeField] private float currentBattery = 100f;
        [SerializeField] private float batteryDrainRate = 5f;
        [SerializeField] private float batteryRechargeRate = 2f;
        [SerializeField] private bool canRecharge = true;
        
        [Header("Flicker Settings")]
        [SerializeField] private bool enableFlicker = true;
        [SerializeField] private float flickerMinInterval = 0.5f;
        [SerializeField] private float flickerMaxInterval = 2f;
        [SerializeField] private float flickerChance = 0.3f;
        
        [Header("Battery Warning")]
        [SerializeField] private float lowBatteryThreshold = 20f;
        [SerializeField] private AudioClip lowBatterySound;
        [SerializeField] private bool hasWarnedLowBattery = false;
        
        [Header("References")]
        [SerializeField] private AudioSource audioSource;
        
        // State
        private bool isFlashlightOn = false;
        private bool isFlickering = false;
        private float nextFlickerTime = 0f;
        private float originalIntensity;
        private float originalRange;
        
        // Events
        public delegate void FlashlightToggledHandler(bool isOn);
        public event FlashlightToggledHandler OnFlashlightToggled;
        
        public delegate void BatteryChangedHandler(float batteryPercent);
        public event BatteryChangedHandler OnBatteryChanged;
        
        public delegate void BatteryDepletedHandler();
        public event BatteryDepletedHandler OnBatteryDepleted;
        
        public bool IsFlashlightOn => isFlashlightOn;
        public float CurrentBattery => currentBattery;
        public float BatteryPercent => currentBattery / maxBattery;

        private void Start()
        {
            InitializeFlashlight();
        }

        private void Update()
        {
            HandleInput();
            UpdateBattery();
            UpdateFlicker();
        }

        /// <summary>
        /// Inicializa configurações da lanterna
        /// </summary>
        private void InitializeFlashlight()
        {
            if (flashlightLight != null)
            {
                originalIntensity = flashlightLight.intensity;
                originalRange = flashlightLight.range;
                flashlightLight.enabled = false;
            }
            
            if (flashlightModel != null)
            {
                flashlightModel.SetActive(false);
            }
            
            isFlashlightOn = false;
        }

        /// <summary>
        /// Processa input do jogador
        /// </summary>
        private void HandleInput()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleFlashlight();
            }
        }

        /// <summary>
        /// Alterna estado da lanterna
        /// </summary>
        public void ToggleFlashlight()
        {
            isFlashlightOn = !isFlashlightOn;
            
            if (flashlightLight != null)
            {
                flashlightLight.enabled = isFlashlightOn;
            }
            
            if (flashlightModel != null)
            {
                flashlightModel.SetActive(isFlashlightOn);
            }
            
            OnFlashlightToggled?.Invoke(isFlashlightOn);
            
            Debug.Log($"[Flashlight] Lanterna {(isFlashlightOn ? "ligada" : "desligada")}");
        }

        /// <summary>
        /// Liga a lanterna diretamente
        /// </summary>
        public void TurnOn()
        {
            if (!isFlashlightOn && currentBattery > 0)
            {
                ToggleFlashlight();
            }
        }

        /// <summary>
        /// Desliga a lanterna diretamente
        /// </summary>
        public void TurnOff()
        {
            if (isFlashlightOn)
            {
                ToggleFlashlight();
            }
        }

        /// <summary>
        /// Atualiza consumo de bateria
        /// </summary>
        private void UpdateBattery()
        {
            if (isFlashlightOn)
            {
                currentBattery -= batteryDrainRate * Time.deltaTime;
                
                // Verificar bateria baixa
                if (currentBattery <= lowBatteryThreshold && !hasWarnedLowBattery)
                {
                    WarnLowBattery();
                }
                
                // Verificar bateria esgotada
                if (currentBattery <= 0f)
                {
                    DepleteBattery();
                }
                
                OnBatteryChanged?.Invoke(BatteryPercent);
            }
            else if (canRecharge && currentBattery < maxBattery)
            {
                currentBattery += batteryRechargeRate * Time.deltaTime;
                currentBattery = Mathf.Min(currentBattery, maxBattery);
                OnBatteryChanged?.Invoke(BatteryPercent);
                
                // Resetar warning se recarregou o suficiente
                if (currentBattery > lowBatteryThreshold)
                {
                    hasWarnedLowBattery = false;
                }
            }
        }

        /// <summary>
        /// Aviso de bateria baixa
        /// </summary>
        private void WarnLowBattery()
        {
            hasWarnedLowBattery = true;
            
            if (lowBatterySound != null && audioSource != null)
            {
                audioSource.PlayOneShot(lowBatterySound);
            }
            
            Debug.LogWarning("[Flashlight] Bateria baixa!");
        }

        /// <summary>
        /// Bateria esgotada - desliga lanterna
        /// </summary>
        private void DepleteBattery()
        {
            currentBattery = 0f;
            
            if (isFlashlightOn)
            {
                TurnOff();
            }
            
            OnBatteryDepleted?.Invoke();
            Debug.LogError("[Flashlight] Bateria esgotada!");
        }

        /// <summary>
        /// Atualiza efeito de flicker da lanterna
        /// </summary>
        private void UpdateFlicker()
        {
            if (!enableFlicker || !isFlashlightOn || isFlickering) return;
            
            if (Time.time >= nextFlickerTime)
            {
                if (Random.value < flickerChance)
                {
                    StartCoroutine(FlickerRoutine());
                }
                
                nextFlickerTime = Time.time + Random.Range(flickerMinInterval, flickerMaxInterval);
            }
        }

        /// <summary>
        /// Rotina de flicker da lanterna
        /// </summary>
        private System.Collections.IEnumerator FlickerRoutine()
        {
            isFlickering = true;
            
            int flickers = Random.Range(2, 5);
            
            for (int i = 0; i < flickers; i++)
            {
                if (flashlightLight != null)
                {
                    flashlightLight.enabled = !flashlightLight.enabled;
                }
                
                yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
            }
            
            if (flashlightLight != null && isFlashlightOn)
            {
                flashlightLight.enabled = true;
            }
            
            isFlickering = false;
        }

        /// <summary>
        /// Adiciona bateria à lanterna
        /// </summary>
        public void AddBattery(float amount)
        {
            currentBattery += amount;
            currentBattery = Mathf.Min(currentBattery, maxBattery);
            OnBatteryChanged?.Invoke(BatteryPercent);
            
            Debug.Log($"[Flashlight] Bateria recarregada: {currentBattery}/{maxBattery}");
        }

        /// <summary>
        /// Substitui bateria completamente
        /// </summary>
        public void ReplaceBattery()
        {
            currentBattery = maxBattery;
            hasWarnedLowBattery = false;
            OnBatteryChanged?.Invoke(BatteryPercent);
            
            Debug.Log("[Flashlight] Bateria substituída");
        }

        /// <summary>
        /// Define intensidade da lanterna
        /// </summary>
        public void SetIntensity(float intensity)
        {
            if (flashlightLight != null)
            {
                flashlightLight.intensity = intensity;
            }
        }

        /// <summary>
        /// Define alcance da lanterna
        /// </summary>
        public void SetRange(float range)
        {
            if (flashlightLight != null)
            {
                flashlightLight.range = range;
            }
        }

        /// <summary>
        /// Recupera intensidade original
        /// </summary>
        public void ResetIntensity()
        {
            if (flashlightLight != null)
            {
                flashlightLight.intensity = originalIntensity;
            }
        }

        /// <summary>
        /// Força flicker (para eventos assustadores)
        /// </summary>
        public void ForceFlicker()
        {
            if (!isFlickering)
            {
                StartCoroutine(FlickerRoutine());
            }
        }
    }
}

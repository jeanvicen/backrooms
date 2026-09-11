using UnityEngine;
using System.Collections;

namespace Backrooms.Systems
{
    /// <summary>
    /// Sistema de lanterna com bateria, efeitos de luz dinâmicos,
    /// e mecânica de flicker para atmosfera assustadora.
    /// </summary>
    [RequireComponent(typeof(Spotlight))]
    [RequireComponent(typeof(AudioSource))]
    public class Flashlight : MonoBehaviour
    {
        [Header("Configurações da Lanterna")]
        [SerializeField] private float maxBattery = 100f;
        [SerializeField] private float batteryDrainRate = 5f; // Porcentagem por segundo
        [SerializeField] private float rechargeRate = 10f;
        [SerializeField] private bool canRecharge = true;
        
        [Header("Flicker (Piscar)")]
        [SerializeField] private float minFlickerInterval = 0.05f;
        [SerializeField] private float maxFlickerInterval = 0.2f;
        [SerializeField] private float flickerIntensityMin = 0.3f;
        [SerializeField] private float flickerIntensityMax = 1f;
        [SerializeField] private bool enableFlicker = true;
        
        [Header("Bateria Baixa")]
        [SerializeField] private float lowBatteryThreshold = 20f;
        [SerializeField] private float criticalBatteryThreshold = 5f;
        [SerializeField] private float lowBatteryFlickerRate = 2f;
        
        [Header("Referências")]
        [SerializeField] private Spotlight spotlight;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Light lightComponent;
        
        [Header("Áudio")]
        [SerializeField] private AudioClip turnOnSound;
        [SerializeField] private AudioClip turnOffSound;
        [SerializeField] private AudioClip flickerSound;
        [SerializeField] private AudioClip lowBatterySound;
        
        [Header("Efeitos")]
        [SerializeField] private GameObject flickerLightEffect;
        [SerializeField] private ParticleSystem sparkParticles;
        
        // Variáveis privadas
        private float currentBattery;
        private bool isOn = false;
        private bool isFlickering = false;
        private Coroutine flickerCoroutine;
        private Coroutine lowBatteryCoroutine;
        private float originalIntensity;
        private float originalRange;
        
        // Propriedades
        public float CurrentBattery => currentBattery;
        public float MaxBattery => maxBattery;
        public float BatteryPercentage => (currentBattery / maxBattery) * 100f;
        public bool IsOn => isOn;
        public bool IsLowBattery => currentBattery < lowBatteryThreshold;
        public bool IsCriticalBattery => currentBattery < criticalBatteryThreshold;
        
        // Eventos
        public delegate void BatteryChangedHandler(float current, float max, float percentage);
        public event BatteryChangedHandler OnBatteryChanged;
        
        public delegate void FlashlightStateChangedHandler(bool isOn);
        public event FlashlightStateChangedHandler OnFlashlightTurnedOn;
        public event FlashlightStateChangedHandler OnFlashlightTurnedOff;
        
        public delegate void LowBatteryHandler(float percentage);
        public event LowBatteryHandler OnLowBattery;
        public event LowBatteryHandler OnCriticalBattery;

        void Awake()
        {
            if (spotlight == null)
                spotlight = GetComponent<Spotlight>();
            
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            
            if (lightComponent == null)
                lightComponent = GetComponent<Light>();
        }

        void Start()
        {
            currentBattery = maxBattery;
            
            if (lightComponent != null)
            {
                originalIntensity = lightComponent.intensity;
                originalRange = lightComponent.range;
                
                // Inicia desligada
                lightComponent.enabled = false;
            }
        }

        void Update()
        {
            HandleInput();
            HandleBatteryDrain();
        }

        void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                Toggle();
            }
        }

        public void Toggle()
        {
            if (isOn)
            {
                TurnOff();
            }
            else
            {
                TurnOn();
            }
        }

        public void TurnOn()
        {
            if (isOn || currentBattery <= 0) return;
            
            isOn = true;
            
            if (lightComponent != null)
            {
                lightComponent.enabled = true;
                lightComponent.intensity = originalIntensity;
            }
            
            if (turnOnSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(turnOnSound, 0.7f);
            }
            
            // Inicia flicker se habilitado
            if (enableFlicker && flickerCoroutine == null)
            {
                flickerCoroutine = StartCoroutine(FlickerLoop());
            }
            
            OnFlashlightTurnedOn?.Invoke(isOn);
        }

        public void TurnOff()
        {
            if (!isOn) return;
            
            isOn = false;
            
            if (lightComponent != null)
            {
                lightComponent.enabled = false;
            }
            
            // Para flicker
            if (flickerCoroutine != null)
            {
                StopCoroutine(flickerCoroutine);
                flickerCoroutine = null;
            }
            
            // Para alerta de bateria baixa
            if (lowBatteryCoroutine != null)
            {
                StopCoroutine(lowBatteryCoroutine);
                lowBatteryCoroutine = null;
            }
            
            if (turnOffSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(turnOffSound, 0.5f);
            }
            
            OnFlashlightTurnedOff?.Invoke(isOn);
        }

        void HandleBatteryDrain()
        {
            if (isOn)
            {
                currentBattery -= batteryDrainRate * Time.deltaTime;
                currentBattery = Mathf.Max(0, currentBattery);
                
                OnBatteryChanged?.Invoke(currentBattery, maxBattery, BatteryPercentage);
                
                // Verifica thresholds de bateria
                CheckBatteryThresholds();
                
                // Desliga automaticamente se bateria acabar
                if (currentBattery <= 0 && isOn)
                {
                    TurnOff();
                }
            }
            else if (canRecharge && currentBattery < maxBattery)
            {
                // Recarrega quando desligada
                currentBattery += rechargeRate * Time.deltaTime;
                currentBattery = Mathf.Min(maxBattery, currentBattery);
                
                OnBatteryChanged?.Invoke(currentBattery, maxBattery, BatteryPercentage);
            }
        }

        void CheckBatteryThresholds()
        {
            static float lastLowTrigger = -1f;
            static float lastCriticalTrigger = -1f;
            
            if (currentBattery < lowBatteryThreshold && lastLowTrigger != lowBatteryThreshold)
            {
                OnLowBattery?.Invoke(BatteryPercentage);
                lastLowTrigger = lowBatteryThreshold;
                
                // Inicia flicker rápido de bateria baixa
                if (lowBatteryCoroutine == null)
                {
                    lowBatteryCoroutine = StartCoroutine(LowBatteryFlickerLoop());
                }
            }
            
            if (currentBattery < criticalBatteryThreshold && lastCriticalTrigger != criticalBatteryThreshold)
            {
                OnCriticalBattery?.Invoke(BatteryPercentage);
                lastCriticalTrigger = criticalBatteryThreshold;
                
                if (lowBatterySound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(lowBatterySound, 0.5f);
                }
            }
        }

        IEnumerator FlickerLoop()
        {
            while (isOn && enableFlicker)
            {
                yield return new WaitForSeconds(Random.Range(minFlickerInterval, maxFlickerInterval));
                
                // Chance de flicker baseada na bateria (mais flicker com menos bateria)
                float flickerChance = 1f - (currentBattery / maxBattery) * 0.5f;
                
                if (Random.value < flickerChance)
                {
                    DoFlicker();
                }
            }
        }

        IEnumerator LowBatteryFlickerLoop()
        {
            while (isOn && IsLowBattery)
            {
                yield return new WaitForSeconds(1f / lowBatteryFlickerRate);
                DoFlicker();
            }
        }

        void DoFlicker()
        {
            if (lightComponent == null) return;
            
            float targetIntensity = Random.Range(flickerIntensityMin, flickerIntensityMax) * originalIntensity;
            lightComponent.intensity = targetIntensity;
            
            // Toca som de flicker
            if (flickerSound != null && audioSource != null && Random.value < 0.3f)
            {
                audioSource.PlayOneShot(flickerSound, 0.3f);
            }
            
            // Spawn de partículas de faísca
            if (sparkParticles != null && Random.value < 0.1f)
            {
                sparkParticles.Play();
            }
        }

        public void AddBattery(float amount)
        {
            currentBattery += amount;
            currentBattery = Mathf.Min(maxBattery, currentBattery);
            OnBatteryChanged?.Invoke(currentBattery, maxBattery, BatteryPercentage);
        }

        public void DrainBattery(float amount)
        {
            currentBattery -= amount;
            currentBattery = Mathf.Max(0, currentBattery);
            OnBatteryChanged?.Invoke(currentBattery, maxBattery, BatteryPercentage);
            
            if (currentBattery <= 0 && isOn)
            {
                TurnOff();
            }
        }

        public void SetFlickerEnabled(bool enabled)
        {
            enableFlicker = enabled;
            
            if (!enabled && flickerCoroutine != null)
            {
                StopCoroutine(flickerCoroutine);
                flickerCoroutine = null;
                
                // Restaura intensidade normal
                if (lightComponent != null)
                {
                    lightComponent.intensity = originalIntensity;
                }
            }
        }

        // Método público para forçar estado (útil para cutscenes)
        public void ForceState(bool on, float batteryPercentage = -1f)
        {
            if (on)
            {
                if (batteryPercentage >= 0)
                {
                    currentBattery = (batteryPercentage / 100f) * maxBattery;
                }
                TurnOn();
            }
            else
            {
                TurnOff();
            }
        }

        // Debug
        void OnGUI()
        {
            if (Application.isPlaying && isOn)
            {
                GUILayout.BeginArea(new Rect(Screen.width - 160, 10, 150, 60));
                
                Color batteryColor = currentBattery > lowBatteryThreshold ? Color.green : 
                                   currentBattery > criticalBatteryThreshold ? Color.yellow : Color.red;
                
                GUI.backgroundColor = batteryColor;
                GUILayout.Box($"Bateria: {BatteryPercentage:F1}%");
                
                GUI.backgroundColor = Color.white;
                GUILayout.EndArea();
            }
        }
    }
}

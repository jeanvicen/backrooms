using UnityEngine;
using System.Collections;

namespace Backrooms.Systems
{
    /// <summary>
    /// Controla o flicker (piscar) de luzes fluorescentes para criar
    /// atmosfera assustadora e dinâmica nas Backrooms.
    /// </summary>
    public class LightFlickerController : MonoBehaviour
    {
        [Header("Configurações de Flicker")]
        [SerializeField] private bool startFlickering = true;
        [SerializeField] private float minFlickerInterval = 0.1f;
        [SerializeField] private float maxFlickerInterval = 2f;
        [SerializeField] private float minIntensity = 0.3f;
        [SerializeField] private float maxIntensity = 1.2f;
        [SerializeField] private float flickerDuration = 0.5f;
        
        [Header("Frequência")]
        [SerializeField] private float baseFlickerChance = 0.02f; // Chance por frame
        [SerializeField] private float intensityMultiplier = 1f;
        
        [Header("Tipos de Flicker")]
        [SerializeField] private bool enableStrobe = false;
        [SerializeField] private float strobeInterval = 0.1f;
        [SerializeField] private int strobeCount = 3;
        
        [Header("Áudio")]
        [SerializeField] private AudioClip buzzSound;
        [SerializeField] private AudioClip flickerSound;
        [SerializeField] private AudioSource audioSource;
        
        [Header("Referências")]
        [SerializeField] private Light lightComponent;
        [SerializeField] private Light[] additionalLights;
        
        // Variáveis privadas
        private Coroutine flickerCoroutine;
        private float originalIntensity;
        private bool isFlickering = false;
        private bool hasPower = true;
        
        // Propriedades
        public bool IsFlickering => isFlickering;
        public bool HasPower => hasPower;
        
        // Eventos
        public delegate void FlickerStartedHandler();
        public event FlickerStartedHandler OnFlickerStarted;
        
        public delegate void FlickerStoppedHandler();
        public event FlickerStoppedHandler OnFlickerStopped;
        
        public delegate void PowerChangedHandler(bool hasPower);
        public event PowerChangedHandler OnPowerChanged;

        void Awake()
        {
            if (lightComponent == null)
                lightComponent = GetComponent<Light>();
            
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }

        void Start()
        {
            if (lightComponent != null)
            {
                originalIntensity = lightComponent.intensity;
            }
            
            if (startFlickering && hasPower)
            {
                StartFlickering();
            }
        }

        public void StartFlickering()
        {
            if (isFlickering || !hasPower) return;
            
            isFlickering = true;
            
            if (enableStrobe)
            {
                flickerCoroutine = StartCoroutine(StrobeLoop());
            }
            else
            {
                flickerCoroutine = StartCoroutine(RandomFlickerLoop());
            }
            
            OnFlickerStarted?.Invoke();
        }

        public void StopFlickering()
        {
            if (!isFlickering) return;
            
            isFlickering = false;
            
            if (flickerCoroutine != null)
            {
                StopCoroutine(flickerCoroutine);
                flickerCoroutine = null;
            }
            
            // Restaura intensidade original
            SetLightIntensity(originalIntensity);
            
            OnFlickerStopped?.Invoke();
        }

        public void ToggleFlickering()
        {
            if (isFlickering)
            {
                StopFlickering();
            }
            else
            {
                StartFlickering();
            }
        }

        IEnumerator RandomFlickerLoop()
        {
            while (isFlickering && hasPower)
            {
                // Espera intervalo aleatório
                yield return new WaitForSeconds(Random.Range(minFlickerInterval, maxFlickerInterval));
                
                // Verifica se deve flicker
                if (Random.value < baseFlickerChance * 60f) // Ajusta para framerate
                {
                    DoFlicker();
                }
            }
        }

        IEnumerator StrobeLoop()
        {
            while (isFlickering && hasPower)
            {
                for (int i = 0; i < strobeCount; i++)
                {
                    SetLightIntensity(maxIntensity * intensityMultiplier);
                    yield return new WaitForSeconds(strobeInterval);
                    
                    SetLightIntensity(minIntensity * intensityMultiplier);
                    yield return new WaitForSeconds(strobeInterval);
                }
                
                // Pausa entre sequências de strobe
                yield return new WaitForSeconds(Random.Range(1f, 3f));
            }
        }

        void DoFlicker()
        {
            StartCoroutine(ExecuteFlicker());
        }

        IEnumerator ExecuteFlicker()
        {
            float elapsed = 0f;
            
            // Toca som de flicker
            if (flickerSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(flickerSound, 0.5f);
            }
            
            while (elapsed < flickerDuration)
            {
                float intensity = Random.Range(minIntensity, maxIntensity) * intensityMultiplier;
                SetLightIntensity(intensity);
                
                // Som de buzz durante flicker intenso
                if (buzzSound != null && audioSource != null && intensity > 0.8f)
                {
                    if (!audioSource.isPlaying)
                    {
                        audioSource.PlayOneShot(buzzSound, 0.3f);
                    }
                }
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Restaura intensidade normal
            SetLightIntensity(originalIntensity);
        }

        void SetLightIntensity(float intensity)
        {
            if (lightComponent != null && hasPower)
            {
                lightComponent.intensity = intensity;
            }
            
            // Aplica a luzes adicionais
            if (additionalLights != null)
            {
                foreach (Light light in additionalLights)
                {
                    if (light != null && hasPower)
                    {
                        light.intensity = intensity;
                    }
                }
            }
        }

        public void CutPower()
        {
            hasPower = false;
            SetLightIntensity(0f);
            StopFlickering();
            
            OnPowerChanged?.Invoke(false);
        }

        public void RestorePower()
        {
            hasPower = true;
            SetLightIntensity(originalIntensity);
            
            OnPowerChanged?.Invoke(true);
        }

        public void SetIntensity(float intensity)
        {
            originalIntensity = intensity;
            SetLightIntensity(intensity);
        }

        // Método estático para flicker instantâneo (útil para eventos)
        public static void TriggerInstantFlicker(Light light, float duration = 0.2f)
        {
            if (light == null) return;
            
            CoroutineHolder.Instance.StartCoroutine(InstantFlickerRoutine(light, duration));
        }

        static IEnumerator InstantFlickerRoutine(Light light, float duration)
        {
            float originalIntensity = light.intensity;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                light.intensity = Random.Range(0f, originalIntensity);
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            light.intensity = originalIntensity;
        }

        // Classe helper para corrotinas estáticas
        class CoroutineHolder : MonoBehaviour
        {
            private static CoroutineHolder _instance;
            public static CoroutineHolder Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("CoroutineHolder");
                        _instance = go.AddComponent<CoroutineHolder>();
                        DontDestroyOnLoad(go);
                    }
                    return _instance;
                }
            }
        }

        void OnDestroy()
        {
            if (flickerCoroutine != null)
            {
                StopCoroutine(flickerCoroutine);
            }
        }

        // Debug
        void OnDrawGizmosSelected()
        {
            Gizmos.color = isFlickering ? Color.yellow : Color.gray;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            
#if UNITY_EDITOR
            GUIStyle style = new GUIStyle();
            style.fontSize = 12;
            style.normal.textColor = Color.white;
            
            string status = hasPower ? (isFlickering ? "Flickering" : "Ativa") : "Sem Energia";
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1f, status, style);
#endif
        }
    }
}

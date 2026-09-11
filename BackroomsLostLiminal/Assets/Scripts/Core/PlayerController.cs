using UnityEngine;
using System.Collections;

namespace Backrooms.Core
{
    /// <summary>
    /// Controlador principal do jogador com sistema de movimento em primeira pessoa,
    /// stamina, agachar, correr e head bob realista.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(AudioSource))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Configurações de Movimento")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float runSpeed = 8f;
        [SerializeField] private float crouchSpeed = 2.5f;
        [SerializeField] private float crawlSpeed = 1.5f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float gravity = -9.81f * 2f;

        [Header("Configurações de Stamina")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float staminaDrainRate = 20f;
        [SerializeField] private float staminaRecoveryRate = 15f;
        [SerializeField] private float staminaDrainDelay = 1f;

        [Header("Head Bob")]
        [SerializeField] private float headBobFrequency = 10f;
        [SerializeField] private float headBobAmplitude = 0.05f;
        [SerializeField] private float headBobRunMultiplier = 2f;
        
        [Header("Câmera")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float standingHeight = 1.8f;
        [SerializeField] private float crouchingHeight = 1.0f;
        [SerializeField] private float crawlingHeight = 0.6f;
        [SerializeField] private float heightTransitionSpeed = 5f;

        [Header("Áudio")]
        [SerializeField] private AudioClip footstepCarpet;
        [SerializeField] private AudioClip footstepConcrete;
        [SerializeField] private AudioClip footstepMetal;
        [SerializeField] private AudioClip footstepWater;
        [SerializeField] private AudioClip breathingNormal;
        [SerializeField] private AudioClip breathingTired;

        [Header("Referências")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private AudioSource audioSource;

        // Variáveis privadas
        private Vector3 velocity;
        private bool isGrounded;
        private float currentStamina;
        private bool isRunning;
        private bool isCrouching;
        private bool isCrawling;
        private float targetHeight;
        private float headBobTimer;
        private Vector3 originalCameraPosition;
        private float lastStaminaUseTime;
        private bool isPlayingBreathing;

        // Propriedades
        public float CurrentStamina => currentStamina;
        public float MaxStamina => maxStamina;
        public bool IsRunning => isRunning;
        public bool IsCrouching => isCrouching;
        public bool IsCrawling => isCrawling;
        public Vector3 PlayerVelocity => velocity;

        // Eventos
        public delegate void StaminaChangedHandler(float current, float max);
        public event StaminaChangedHandler OnStaminaChanged;

        public delegate void MovementStateChangedHandler(bool running, bool crouching, bool crawling);
        public event MovementStateChangedHandler OnMovementStateChanged;

        void Awake()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();

            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();

            if (cameraTransform == null)
                cameraTransform = Camera.main.transform;

            // Bloqueia o cursor no centro da tela
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Start()
        {
            currentStamina = maxStamina;
            targetHeight = standingHeight;
            originalCameraPosition = cameraTransform.localPosition;
            
            // Inicia som de respiração
            if (breathingNormal != null)
                StartCoroutine(PlayBreathingLoop());
        }

        void Update()
        {
            HandleInput();
            HandleMovement();
            HandleHeightTransition();
            HandleHeadBob();
            HandleStamina();
            ApplyGravity();
        }

        void HandleInput()
        {
            // Entrada de movimento
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            // Correr (apenas se não estiver agachado ou rastejando)
            isRunning = Input.GetKey(KeyCode.LeftShift) && !isCrouching && !isCrawling && currentStamina > 0;

            // Agachar
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                if (isCrouching)
                {
                    isCrouching = false;
                    isCrawling = false;
                }
                else
                {
                    isCrouching = true;
                }
                OnMovementStateChanged?.Invoke(isRunning, isCrouching, isCrawling);
            }

            // Rastejar (agachar + correr em espaços baixos)
            isCrawling = isCrouching && Input.GetKey(KeyCode.LeftShift);

            // Pular (apenas se não estiver agachado)
            if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
            {
                velocity.y = jumpForce;
            }
        }

        void HandleMovement()
        {
            isGrounded = characterController.isGrounded;
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Pequena força para baixo para garantir que está no chão
            }

            // Determina velocidade baseada no estado
            float speed = GetMovementSpeed();

            // Move o jogador
            Vector3 move = transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical");
            characterController.Move(move.normalized * speed * Time.deltaTime);

            // Toca sons de passo
            if (move.magnitude > 0.1f && isGrounded)
            {
                PlayFootstep(speed);
            }
        }

        float GetMovementSpeed()
        {
            if (isCrawling) return crawlSpeed;
            if (isCrouching) return crouchSpeed;
            if (isRunning) return runSpeed;
            return walkSpeed;
        }

        void HandleHeightTransition()
        {
            // Define altura alvo baseada no estado
            if (isCrawling)
                targetHeight = crawlingHeight;
            else if (isCrouching)
                targetHeight = crouchingHeight;
            else
                targetHeight = standingHeight;

            // Transição suave de altura
            characterController.height = Mathf.Lerp(characterController.height, targetHeight, Time.deltaTime * heightTransitionSpeed);
            
            // Ajusta posição da câmera
            float cameraY = Mathf.Lerp(cameraTransform.localPosition.y, targetHeight - 0.2f, Time.deltaTime * heightTransitionSpeed);
            cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, cameraY, cameraTransform.localPosition.z);
        }

        void HandleHeadBob()
        {
            if (!isGrounded || characterController.velocity.magnitude < 0.1f)
            {
                headBobTimer = 0f;
                cameraTransform.localPosition = originalCameraPosition;
                return;
            }

            float frequency = headBobFrequency;
            float amplitude = headBobAmplitude;

            if (isRunning)
            {
                frequency *= headBobRunMultiplier;
                amplitude *= headBobRunMultiplier;
            }

            if (isCrouching)
            {
                frequency *= 0.8f;
                amplitude *= 0.7f;
            }

            headBobTimer += Time.deltaTime * frequency;
            
            float bobX = Mathf.Sin(headBobTimer) * amplitude;
            float bobY = Mathf.Abs(Mathf.Cos(headBobTimer * 0.5f)) * amplitude;

            cameraTransform.localPosition = new Vector3(
                originalCameraPosition.x + bobX,
                originalCameraPosition.y + bobY,
                originalCameraPosition.z
            );
        }

        void HandleStamina()
        {
            if (isRunning && !isCrawling)
            {
                currentStamina -= staminaDrainRate * Time.deltaTime;
                currentStamina = Mathf.Max(0, currentStamina);
                lastStaminaUseTime = Time.time;
                
                if (currentStamina <= 0)
                {
                    isRunning = false;
                }
            }
            else if (Time.time - lastStaminaUseTime > staminaDrainDelay)
            {
                currentStamina += staminaRecoveryRate * Time.deltaTime;
                currentStamina = Mathf.Min(maxStamina, currentStamina);
            }

            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }

        void ApplyGravity()
        {
            if (!isGrounded)
            {
                velocity.y += gravity * Time.deltaTime;
            }
            
            characterController.Move(velocity * Time.deltaTime);
        }

        void PlayFootstep(float speed)
        {
            // Implementação simplificada - em produção, usaria Raycast para detectar superfície
            float stepInterval = isRunning ? 0.4f : 0.6f;
            
            if (Time.time % stepInterval < Time.deltaTime)
            {
                AudioClip stepSound = footstepCarpet; // Default para carpete do Level 0
                
                // Em produção, detectaria o tipo de superfície via Raycast
                audioSource.PlayOneShot(stepSound, 0.5f);
            }
        }

        IEnumerator PlayBreathingLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(2f);
                
                if (currentStamina < maxStamina * 0.5f)
                {
                    // Respiração cansada
                    if (breathingTired != null && !audioSource.isPlaying)
                    {
                        audioSource.PlayOneShot(breathingTired, 0.3f);
                    }
                }
                else
                {
                    // Respiração normal
                    if (breathingNormal != null && !audioSource.isPlaying)
                    {
                        audioSource.PlayOneShot(breathingNormal, 0.2f);
                    }
                }
            }
        }

        // Método público para dano de stamina (ex: ser atingido por entidade)
        public void TakeStaminaDamage(float amount)
        {
            currentStamina -= amount;
            currentStamina = Mathf.Max(0, currentStamina);
            lastStaminaUseTime = Time.time;
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }

        // Método público para recuperar stamina
        public void RecoverStamina(float amount)
        {
            currentStamina += amount;
            currentStamina = Mathf.Min(maxStamina, currentStamina);
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }

        // Debug visual
        void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
            {
                // Desenha barra de stamina no editor
                Gizmos.color = currentStamina > 30 ? Color.green : Color.red;
                Vector3 pos = transform.position + Vector3.up * 2f;
                Gizmos.DrawCube(pos, new Vector3(1f, 0.1f, 0.1f));
            }
        }
    }
}

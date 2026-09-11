using UnityEngine;

namespace Backrooms.Player
{
    /// <summary>
    /// Controlador principal do jogador em primeira pessoa
    /// Gerencia movimento, câmera, stamina e interações
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(AudioSource))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float minVerticalAngle = -80f;
        [SerializeField] private float maxVerticalAngle = 80f;
        
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float runSpeed = 8f;
        [SerializeField] private float crouchSpeed = 2f;
        [SerializeField] private float crawlSpeed = 1f;
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float rotationSmoothTime = 0.03f;
        
        [Header("Stamina System")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float staminaDrainRate = 25f;
        [SerializeField] private float staminaRegenRate = 15f;
        [SerializeField] private float staminaRegenDelay = 2f;
        
        [Header("Head Bob")]
        [SerializeField] private bool enableHeadBob = true;
        [SerializeField] private float headBobSpeed = 10f;
        [SerializeField] private float headBobAmount = 0.05f;
        [SerializeField] private float headBobTilt = 0.02f;
        
        [Header("Crouch & Crawl")]
        [SerializeField] private float standingHeight = 1.8f;
        [SerializeField] private float crouchHeight = 1.0f;
        [SerializeField] private float crawlHeight = 0.6f;
        [SerializeField] private float heightTransitionSpeed = 5f;
        
        [Header("Jump")]
        [SerializeField] private bool canJump = true;
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float gravity = -9.81f * 2f;
        
        [Header("Ground Detection")]
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float groundDistance = 0.2f;
        
        [Header("Audio")]
        [SerializeField] private AudioClip footstepCarpet;
        [SerializeField] private AudioClip footstepConcrete;
        [SerializeField] private AudioClip footstepMetal;
        [SerializeField] private AudioClip footstepWater;
        [SerializeField] private AudioClip breathingClip;
        
        // Private variables
        private CharacterController characterController;
        private AudioSource audioSource;
        private Vector3 velocity;
        private float verticalRotation = 0f;
        private float currentStamina;
        private float lastStaminaUseTime;
        private bool isRunning = false;
        private bool isCrouching = false;
        private bool isCrawling = false;
        private bool isGrounded;
        private float headBobTimer = 0f;
        private Vector3 originalCameraOffset;
        private Transform groundCheck;
        
        // Ground check sphere
        private float groundCheckRadius = 0.2f;

        public bool IsRunning => isRunning;
        public bool IsCrouching => isCrouching;
        public float CurrentStamina => currentStamina;
        public float StaminaPercent => currentStamina / maxStamina;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            audioSource = GetComponent<AudioSource>();
            
            if (cameraTransform == null)
            {
                cameraTransform = Camera.main.transform;
            }
            
            // Setup ground check
            groundCheck = new GameObject("GroundCheck").transform;
            groundCheck.SetParent(transform);
            groundCheck.localPosition = Vector3.zero;
            
            // Lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Start()
        {
            currentStamina = maxStamina;
            originalCameraOffset = cameraTransform.localPosition;
        }

        private void Update()
        {
            HandleMouseLook();
            HandleMovement();
            HandleStamina();
            HandleHeadBob();
            ApplyGravity();
        }

        /// <summary>
        /// Processa entrada do mouse para rotação da câmera
        /// </summary>
        private void HandleMouseLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // Rotação horizontal (corpo inteiro)
            transform.Rotate(Vector3.up * mouseX);

            // Rotação vertical (apenas câmera)
            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);
            cameraTransform.localEulerAngles = Vector3.right * verticalRotation;
        }

        /// <summary>
        /// Processa entrada de movimento do jogador
        /// </summary>
        private void HandleMovement()
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);

            // Input de movimento
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            // Determinar estado de movimento
            isRunning = Input.GetKey(KeyCode.LeftShift) && currentStamina > 0;
            isCrouching = Input.GetKey(KeyCode.LeftControl);
            isCrawling = isCrouching && Input.GetKey(KeyCode.LeftShift);

            // Selecionar velocidade baseada no estado
            float targetSpeed = GetTargetSpeed();

            // Calcular direção de movimento
            Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
            moveDirection.Normalize();

            // Aplicar aceleração suave
            float currentSpeed = new Vector2(velocity.x, velocity.z).magnitude;
            float smoothSpeed = Mathf.Lerp(currentSpeed, targetSpeed * moveDirection.magnitude, acceleration * Time.deltaTime);

            velocity.x = moveDirection.x * smoothSpeed;
            velocity.z = moveDirection.z * smoothSpeed;

            // Pulo
            if (canJump && isGrounded && Input.GetKeyDown(KeyCode.Space))
            {
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            }

            // Mover CharacterController
            characterController.Move(velocity * Time.deltaTime);

            // Ajustar altura baseada no estado
            AdjustHeight();
        }

        /// <summary>
        /// Obtém a velocidade alvo baseada no estado atual
        /// </summary>
        private float GetTargetSpeed()
        {
            if (isCrawling) return crawlSpeed;
            if (isCrouching) return crouchSpeed;
            if (isRunning) return runSpeed;
            return walkSpeed;
        }

        /// <summary>
        /// Ajusta altura do personagem baseado em agachar/rastejar
        /// </summary>
        private void AdjustHeight()
        {
            float targetHeight = isCrawling ? crawlHeight : (isCrouching ? crouchHeight : standingHeight);
            
            characterController.height = Mathf.Lerp(characterController.height, targetHeight, heightTransitionSpeed * Time.deltaTime);
            
            // Ajustar offset da câmera
            float cameraHeight = isCrawling ? crawlHeight * 0.8f : (isCrouching ? crouchHeight * 0.9f : standingHeight * 0.9f);
            cameraTransform.localPosition = new Vector3(0, cameraHeight, 0);
        }

        /// <summary>
        /// Gerencia sistema de stamina
        /// </summary>
        private void HandleStamina()
        {
            if (isRunning && !isCrouching && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0))
            {
                currentStamina -= staminaDrainRate * Time.deltaTime;
                lastStaminaUseTime = Time.time;
                
                if (currentStamina <= 0)
                {
                    currentStamina = 0;
                    isRunning = false;
                }
            }
            else if (Time.time - lastStaminaUseTime > staminaRegenDelay)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina);
            }
        }

        /// <summary>
        /// Aplica efeito de head bob durante movimento
        /// </summary>
        private void HandleHeadBob()
        {
            if (!enableHeadBob) return;
            if (!isGrounded) return;

            float speed = GetTargetSpeed();
            bool isMoving = Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;

            if (isMoving && speed > 0.1f)
            {
                headBobTimer += Time.deltaTime * headBobSpeed * (speed / walkSpeed);

                float bobX = Mathf.Sin(headBobTimer) * headBobAmount;
                float bobY = Mathf.Cos(headBobTimer * 2f) * headBobAmount;
                float tilt = Mathf.Sin(headBobTimer) * headBobTilt;

                cameraTransform.localPosition = new Vector3(
                    originalCameraOffset.x + bobX,
                    originalCameraOffset.y + bobY,
                    originalCameraOffset.z
                );

                cameraTransform.localRotation = Quaternion.Euler(tilt, 0, 0);

                // Tocar passos
                PlayFootstep();
            }
            else
            {
                headBobTimer = 0f;
                cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, originalCameraOffset, Time.deltaTime * 5f);
            }
        }

        /// <summary>
        /// Toca som de passo baseado na superfície
        /// </summary>
        private void PlayFootstep()
        {
            if (headBobTimer % (Mathf.PI * 2) < 0.1f)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.down, out hit, standingHeight * 0.9f))
                {
                    AudioClip stepSound = GetFootstepSoundForSurface(hit.collider.gameObject.layer);
                    
                    if (stepSound != null && !audioSource.isPlaying)
                    {
                        audioSource.PlayOneShot(stepSound, 0.5f);
                    }
                }
            }
        }

        /// <summary>
        /// Retorna som de passo baseado na superfície
        /// </summary>
        private AudioClip GetFootstepSoundForSurface(int layer)
        {
            // TODO: Implementar lógica de camadas para diferentes superfícies
            return footstepConcrete; // Default
        }

        /// <summary>
        /// Aplica gravidade ao jogador
        /// </summary>
        private void ApplyGravity()
        {
            if (!isGrounded)
            {
                velocity.y += gravity * Time.deltaTime;
            }
            else if (velocity.y < 0)
            {
                velocity.y = -2f;
            }
        }

        /// <summary>
        /// Desenha gizmos para debug
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }

        /// <summary>
        /// Recupera stamina instantaneamente (para power-ups)
        /// </summary>
        public void RestoreStamina(float amount)
        {
            currentStamina = Mathf.Min(currentStamina + amount, maxStamina);
        }

        /// <summary>
        /// Drena stamina instantaneamente
        /// </summary>
        public void DrainStamina(float amount)
        {
            currentStamina = Mathf.Max(currentStamina - amount, 0f);
        }
    }
}

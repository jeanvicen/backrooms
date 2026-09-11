using UnityEngine;

namespace Backrooms.Core
{
    /// <summary>
    /// Sistema de interação por raycasting. Permite ao jogador interagir com
    /// objetos, portas, itens e outros elementos do cenário.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class InteractionSystem : MonoBehaviour
    {
        [Header("Configurações de Interação")]
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private float interactionCooldown = 0.5f;
        [SerializeField] private LayerMask interactableLayers;
        
        [Header("UI")]
        [SerializeField] private GameObject interactionPrompt;
        [SerializeField] private UnityEngine.UI.Text interactionText;
        
        [Header("Referências")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private AudioSource audioSource;
        
        [Header("Áudio")]
        [SerializeField] private AudioClip interactSuccessSound;
        [SerializeField] private AudioClip interactFailSound;
        [SerializeField] private AudioClip pickupSound;
        
        // Variáveis privadas
        private IInteractable currentInteractable;
        private float lastInteractionTime;
        private RaycastHit hitInfo;
        
        // Propriedades
        public float InteractionRange => interactionRange;
        public IInteractable CurrentInteractable => currentInteractable;
        public bool CanInteract => Time.time >= lastInteractionTime + interactionCooldown;
        
        // Eventos
        public delegate void InteractionStartedHandler(IInteractable interactable);
        public event InteractionStartedHandler OnInteractionStarted;
        
        public delegate void InteractionCompletedHandler(IInteractable interactable);
        public event InteractionCompletedHandler OnInteractionCompleted;
        
        public delegate void InteractionFailedHandler(IInteractable interactable, string reason);
        public event InteractionFailedHandler OnInteractionFailed;
        
        public delegate void LookAtChangedHandler(IInteractable newTarget, IInteractable oldTarget);
        public event LookAtChangedHandler OnLookAtChanged;

        void Awake()
        {
            if (mainCamera == null)
                mainCamera = GetComponent<Camera>();
            
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }

        void Update()
        {
            HandleRaycast();
            HandleInput();
            UpdatePromptVisibility();
        }

        void HandleRaycast()
        {
            Vector3 rayOrigin = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f));
            Vector3 rayDirection = mainCamera.transform.forward;
            
            if (Physics.Raycast(rayOrigin, rayDirection, out hitInfo, interactionRange, interactableLayers))
            {
                IInteractable interactable = GetInteractableFromHit(hitInfo);
                
                if (interactable != currentInteractable)
                {
                    IInteractable oldTarget = currentInteractable;
                    currentInteractable = interactable;
                    OnLookAtChanged?.Invoke(currentInteractable, oldTarget);
                }
            }
            else
            {
                if (currentInteractable != null)
                {
                    IInteractable oldTarget = currentInteractable;
                    currentInteractable = null;
                    OnLookAtChanged?.Invoke(null, oldTarget);
                }
            }
        }

        IInteractable GetInteractableFromHit(RaycastHit hit)
        {
            // Tenta pegar o componente IInteractable do objeto atingido
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            
            // Se não encontrou, procura nos pais
            if (interactable == null)
            {
                interactable = hit.collider.GetComponentInParent<IInteractable>();
            }
            
            return interactable;
        }

        void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.E) && CanInteract)
            {
                TryInteract();
            }
        }

        void TryInteract()
        {
            if (currentInteractable == null)
            {
                OnInteractionFailed?.Invoke(null, "Nada para interagir");
                PlayFailSound();
                return;
            }
            
            if (!currentInteractable.CanInteract)
            {
                OnInteractionFailed?.Invoke(currentInteractable, "Não pode interagir agora");
                PlayFailSound();
                return;
            }
            
            lastInteractionTime = Time.time;
            OnInteractionStarted?.Invoke(currentInteractable);
            
            // Executa a interação
            bool success = currentInteractable.Interact();
            
            if (success)
            {
                OnInteractionCompleted?.Invoke(currentInteractable);
                PlaySuccessSound();
            }
            else
            {
                OnInteractionFailed?.Invoke(currentInteractable, "Interação falhou");
                PlayFailSound();
            }
        }

        void UpdatePromptVisibility()
        {
            if (interactionPrompt == null) return;
            
            if (currentInteractable != null && CanInteract)
            {
                interactionPrompt.SetActive(true);
                
                if (interactionText != null)
                {
                    interactionText.text = currentInteractable.GetPromptText();
                }
            }
            else
            {
                interactionPrompt.SetActive(false);
            }
        }

        void PlaySuccessSound()
        {
            if (audioSource != null && interactSuccessSound != null)
            {
                audioSource.PlayOneShot(interactSuccessSound, 0.8f);
            }
        }

        void PlayFailSound()
        {
            if (audioSource != null && interactFailSound != null)
            {
                audioSource.PlayOneShot(interactFailSound, 0.5f);
            }
        }

        public void PlayPickupSound()
        {
            if (audioSource != null && pickupSound != null)
            {
                audioSource.PlayOneShot(pickupSound, 0.6f);
            }
        }

        // Método público para forçar interação (útil para puzzles)
        public void ForceInteract(IInteractable interactable)
        {
            if (interactable != null && interactable.CanInteract)
            {
                currentInteractable = interactable;
                TryInteract();
            }
        }

        // Debug visual do raycast
        void OnDrawGizmosSelected()
        {
            if (mainCamera == null) return;
            
            Vector3 rayOrigin = mainCamera.transform.position;
            Vector3 rayEnd = rayOrigin + mainCamera.transform.forward * interactionRange;
            
            // Desenha linha do raycast
            Gizmos.color = currentInteractable != null ? Color.green : Color.yellow;
            Gizmos.DrawLine(rayOrigin, rayEnd);
            
            // Desenha esfera no ponto de impacto
            if (hitInfo.collider != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(hitInfo.point, 0.1f);
            }
        }
    }

    /// <summary>
    /// Interface para todos os objetos interativos do jogo.
    /// Qualquer objeto que o jogador possa interagir deve implementar esta interface.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Executa a interação. Retorna true se foi bem-sucedida.
        /// </summary>
        bool Interact();
        
        /// <summary>
        /// Verifica se o objeto pode ser interagido neste momento.
        /// </summary>
        bool CanInteract { get; }
        
        /// <summary>
        /// Texto mostrado no prompt de interação.
        /// </summary>
        string GetPromptText();
    }
}

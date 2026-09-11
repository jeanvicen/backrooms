using UnityEngine;

namespace Backrooms.Interactables
{
    /// <summary>
    /// Sistema de interação do jogador com objetos do mundo
    /// Usa raycasting para detectar objetos olhados
    /// </summary>
    public class InteractionSystem : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private LayerMask interactionLayerMask;
        [SerializeField] private Camera interactionCamera;
        
        [Header("UI References")]
        [SerializeField] private GameObject interactionPrompt;
        [SerializeField] private UnityEngine.UI.Text interactionText;
        
        [Header("Audio")]
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip interactSound;
        [SerializeField] private AudioSource audioSource;
        
        // State
        private IInteractable currentInteractable = null;
        private RaycastHit hitInfo;
        private bool isHovering = false;
        
        // Events
        public delegate void InteractionStartedHandler(IInteractable interactable);
        public event InteractionStartedHandler OnInteractionStarted;
        
        public delegate void InteractionEndedHandler(IInteractable interactable);
        public event InteractionEndedHandler OnInteractionEnded;
        
        public delegate void HoverChangedHandler(bool isHovering, string objectName);
        public event HoverChangedHandler OnHoverChanged;

        private void Start()
        {
            if (interactionCamera == null)
            {
                interactionCamera = Camera.main;
            }
            
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }

        private void Update()
        {
            CheckForInteractable();
            HandleInput();
        }

        /// <summary>
        /// Verifica se há objeto interagível no centro da tela
        /// </summary>
        private void CheckForInteractable()
        {
            Ray ray = new Ray(interactionCamera.transform.position, interactionCamera.transform.forward);
            
            if (Physics.Raycast(ray, out hitInfo, interactionRange, interactionLayerMask))
            {
                IInteractable interactable = GetInteractableFromHit(hitInfo);
                
                if (interactable != null && interactable.CanInteract())
                {
                    if (currentInteractable != interactable)
                    {
                        // Mudou de objeto
                        EndHover();
                        StartHover(interactable);
                    }
                    
                    return;
                }
            }
            
            // Não há objeto válido
            if (currentInteractable != null)
            {
                EndHover();
            }
        }

        /// <summary>
        /// Obtém componente IInteractable do objeto atingido
        /// </summary>
        private IInteractable GetInteractableFromHit(RaycastHit hit)
        {
            // Tentar obter do próprio collider
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            
            if (interactable == null)
            {
                // Tentar obter dos pais
                interactable = hit.collider.GetComponentInParent<IInteractable>();
            }
            
            return interactable;
        }

        /// <summary>
        /// Inicia hover sobre objeto interagível
        /// </summary>
        private void StartHover(IInteractable interactable)
        {
            currentInteractable = interactable;
            isHovering = true;
            
            // Mostrar prompt de interação
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
                
                if (interactionText != null)
                {
                    interactionText.text = $"[{interactKey}] " + interactable.GetInteractionPrompt();
                }
            }
            
            // Tocar som de hover
            if (hoverSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hoverSound);
            }
            
            OnHoverChanged?.Invoke(true, interactable.GetObjectName());
            OnInteractionStarted?.Invoke(interactable);
            
            Debug.Log($"[Interaction] Hover: {interactable.GetObjectName()}");
        }

        /// <summary>
        /// Termina hover sobre objeto
        /// </summary>
        private void EndHover()
        {
            if (currentInteractable == null) return;
            
            isHovering = false;
            
            // Esconder prompt
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
            
            OnHoverChanged?.Invoke(false, "");
            OnInteractionEnded?.Invoke(currentInteractable);
            
            currentInteractable = null;
        }

        /// <summary>
        /// Processa input de interação
        /// </summary>
        private void HandleInput()
        {
            if (Input.GetKeyDown(interactKey) && currentInteractable != null)
            {
                PerformInteraction();
            }
        }

        /// <summary>
        /// Realiza interação com objeto atual
        /// </summary>
        private void PerformInteraction()
        {
            if (currentInteractable == null || !currentInteractable.CanInteract()) return;
            
            currentInteractable.Interact();
            
            // Tocar som de interação
            if (interactSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(interactSound);
            }
            
            Debug.Log($"[Interaction] Interagiu com: {currentInteractable.GetObjectName()}");
        }

        /// <summary>
        /// Força interação com objeto específico (para puzzles)
        /// </summary>
        public void ForceInteract(IInteractable interactable)
        {
            if (interactable != null && interactable.CanInteract())
            {
                interactable.Interact();
            }
        }

        /// <summary>
        /// Define alcance de interação
        /// </summary>
        public void SetInteractionRange(float range)
        {
            interactionRange = range;
            Debug.Log($"[Interaction] Alcance definido para: {range}m");
        }

        /// <summary>
        /// Mostra mensagem customizada no prompt
        /// </summary>
        public void ShowCustomPrompt(string message)
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }
            
            if (interactionText != null)
            {
                interactionText.text = message;
            }
        }

        /// <summary>
        /// Esconde o prompt de interação
        /// </summary>
        public void HidePrompt()
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Interface para todos os objetos interagíveis
    /// </summary>
    public interface IInteractable
    {
        void Interact();
        bool CanInteract();
        string GetInteractionPrompt();
        string GetObjectName();
    }
}

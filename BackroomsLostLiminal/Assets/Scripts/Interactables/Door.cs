using UnityEngine;

namespace Backrooms.Interactables
{
    /// <summary>
    /// Porta interagível que pode ser aberta, trancada ou exigir chave
    /// </summary>
    public class Door : MonoBehaviour, IInteractable
    {
        [Header("Door Settings")]
        [SerializeField] private bool isOpen = false;
        [SerializeField] private bool isLocked = false;
        [SerializeField] private bool autoClose = false;
        [SerializeField] private float autoCloseDelay = 3f;
        
        [Header("Animation")]
        [SerializeField] private Transform doorPivot;
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float openSpeed = 2f;
        [SerializeField] private AnimationCurve openCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Lock Requirements")]
        [SerializeField] private bool requiresKey = false;
        [SerializeField] private string requiredKeyName = "";
        [SerializeField] private KeyCode unlockKey = KeyCode.E;
        
        [Header("Audio")]
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;
        [SerializeField] private AudioClip lockedSound;
        [SerializeField] private AudioClip creakSound;
        [SerializeField] private AudioSource audioSource;
        
        [Header("Interaction")]
        [SerializeField] private string interactionPrompt = "Abrir porta";
        
        // State
        private float currentAngle = 0f;
        private float targetAngle = 0f;
        private bool isAnimating = false;
        private float animationTime = 0f;
        private System.Collections.IEnumerator autoCloseCoroutine;

        public bool IsOpen => isOpen;
        public bool IsLocked => isLocked;

        private void Start()
        {
            if (doorPivot == null)
            {
                doorPivot = transform;
            }
            
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
            
            UpdateTargetAngle();
        }

        private void Update()
        {
            if (isAnimating)
            {
                AnimateDoor();
            }
        }

        /// <summary>
        /// Interage com a porta (implementação IInteractable)
        /// </summary>
        public void Interact()
        {
            if (isLocked)
            {
                TryUnlock();
            }
            else
            {
                ToggleDoor();
            }
        }

        /// <summary>
        /// Verifica se pode interagir com a porta
        /// </summary>
        public bool CanInteract()
        {
            return !isAnimating;
        }

        /// <summary>
        /// Retorna prompt de interação
        /// </summary>
        public string GetInteractionPrompt()
        {
            if (isLocked) return "Trancada";
            return isOpen ? "Fechar" : "Abrir";
        }

        /// <summary>
        /// Retorna nome do objeto
        /// </summary>
        public string GetObjectName()
        {
            return gameObject.name;
        }

        /// <summary>
        /// Alterna estado da porta
        /// </summary>
        public void ToggleDoor()
        {
            if (isAnimating) return;
            
            isOpen = !isOpen;
            UpdateTargetAngle();
            StartAnimation();
            PlayDoorSound(isOpen ? openSound : closeSound);
            
            Debug.Log($"[Door] {(isOpen ? "Aberta" : "Fechada")}");
        }

        /// <summary>
        /// Abre a porta
        /// </summary>
        public void Open()
        {
            if (!isOpen && !isAnimating)
            {
                isOpen = true;
                UpdateTargetAngle();
                StartAnimation();
                PlayDoorSound(openSound);
            }
        }

        /// <summary>
        /// Fecha a porta
        /// </summary>
        public void Close()
        {
            if (isOpen && !isAnimating)
            {
                isOpen = false;
                UpdateTargetAngle();
                StartAnimation();
                PlayDoorSound(closeSound);
            }
        }

        /// <summary>
        /// Tenta destrancar a porta
        /// </summary>
        private void TryUnlock()
        {
            if (!requiresKey)
            {
                isLocked = false;
                ToggleDoor();
                return;
            }
            
            // Verificar se jogador tem a chave
            Inventory.InventoryManager inventory = FindObjectOfType<Inventory.InventoryManager>();
            
            if (inventory != null && inventory.HasItem(requiredKeyName))
            {
                isLocked = false;
                ToggleDoor();
                Debug.Log($"[Door] Porta destrancada com {requiredKeyName}");
            }
            else
            {
                PlayDoorSound(lockedSound);
                Debug.Log("[Door] Porta trancada - chave necessária");
            }
        }

        /// <summary>
        /// Tranca a porta
        /// </summary>
        public void Lock()
        {
            isLocked = true;
            if (isOpen)
            {
                Close();
            }
            Debug.Log("[Door] Porta trancada");
        }

        /// <summary>
        /// Destrava a porta
        /// </summary>
        public void Unlock()
        {
            isLocked = false;
            Debug.Log("[Door] Porta destrancada");
        }

        /// <summary>
        /// Atualiza ângulo alvo baseado no estado
        /// </summary>
        private void UpdateTargetAngle()
        {
            targetAngle = isOpen ? openAngle : 0f;
            
            if (autoClose && isOpen)
            {
                StartAutoClose();
            }
        }

        /// <summary>
        /// Inicia animação da porta
        /// </summary>
        private void StartAnimation()
        {
            isAnimating = true;
            animationTime = 0f;
        }

        /// <summary>
        /// Anima a porta
        /// </summary>
        private void AnimateDoor()
        {
            animationTime += Time.deltaTime * openSpeed;
            
            if (animationTime >= 1f)
            {
                animationTime = 1f;
                isAnimating = false;
            }
            
            float t = openCurve.Evaluate(animationTime);
            currentAngle = Mathf.Lerp(currentAngle, targetAngle, t);
            
            doorPivot.localEulerAngles = new Vector3(0, currentAngle, 0);
        }

        /// <summary>
        /// Inicia coroutine de fechamento automático
        /// </summary>
        private void StartAutoClose()
        {
            if (autoCloseCoroutine != null)
            {
                StopCoroutine(autoCloseCoroutine);
            }
            
            autoCloseCoroutine = AutoCloseRoutine();
            StartCoroutine(autoCloseCoroutine);
        }

        /// <summary>
        /// Rotina de fechamento automático
        /// </summary>
        private System.Collections.IEnumerator AutoCloseRoutine()
        {
            yield return new WaitForSeconds(autoCloseDelay);
            
            if (isOpen && !isAnimating)
            {
                Close();
            }
        }

        /// <summary>
        /// Toca som da porta
        /// </summary>
        private void PlayDoorSound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        /// <summary>
        /// Toca som de rangido (para portas assustadoras)
        /// </summary>
        public void PlayCreak()
        {
            if (creakSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(creakSound, 0.5f);
            }
        }

        /// <summary>
        /// Força estado da porta sem animação
        /// </summary>
        public void ForceState(bool shouldBeOpen)
        {
            isOpen = shouldBeOpen;
            currentAngle = shouldBeOpen ? openAngle : 0f;
            doorPivot.localEulerAngles = new Vector3(0, currentAngle, 0);
        }
    }
}

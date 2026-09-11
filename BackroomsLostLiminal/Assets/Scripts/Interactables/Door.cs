using UnityEngine;

namespace Backrooms.Interactables
{
    /// <summary>
    /// Porta interativa que pode ser aberta, trancada, ou exigir chaves.
    /// Usada em todo o jogo para progressão e bloqueio de áreas.
    /// </summary>
    public class Door : MonoBehaviour, IInteractable
    {
        [Header("Configurações da Porta")]
        [SerializeField] private bool isOpen = false;
        [SerializeField] private bool isLocked = false;
        [SerializeField] private bool requiresKey = false;
        [SerializeField] private string requiredKeyName = "";
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float openSpeed = 2f;
        [SerializeField] private float closeSpeed = 1f;
        
        [Header("Estado Atual")]
        [SerializeField] private float currentAngle = 0f;
        [SerializeField] private bool isMoving = false;
        [SerializeField] private bool openingDirection = true; // true = abre para fora
        
        [Header("Referências")]
        [SerializeField] private Transform doorPivot;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Collider doorCollider;
        
        [Header("Áudio")]
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;
        [SerializeField] private AudioClip lockedSound;
        [SerializeField] private AudioClip unlockSound;
        [SerializeField] private AudioClip creakSound;
        
        [Header("Interação")]
        [SerializeField] private KeyCode interactionKey = KeyCode.E;
        
        // Propriedades
        public bool IsOpen => isOpen;
        public bool IsLocked => isLocked;
        public bool CanInteract => !isMoving && (!isLocked || !requiresKey);
        
        // Eventos
        public delegate void DoorStateChangedHandler(Door door, bool isOpen);
        public event DoorStateChangedHandler OnDoorOpened;
        public event DoorStateChangedHandler OnDoorClosed;
        public event System.Action OnDoorLocked;
        public event System.Action OnDoorUnlocked;

        void Awake()
        {
            if (doorPivot == null)
                doorPivot = transform;
            
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            
            if (doorCollider == null)
                doorCollider = GetComponent<Collider>();
        }

        void Update()
        {
            if (isMoving)
            {
                HandleDoorMovement();
            }
        }

        public bool Interact()
        {
            if (isLocked)
            {
                PlayLockedSound();
                return false;
            }
            
            if (isMoving)
            {
                return false;
            }
            
            ToggleDoor();
            return true;
        }

        public void ToggleDoor()
        {
            if (isOpen)
            {
                CloseDoor();
            }
            else
            {
                OpenDoor();
            }
        }

        public void OpenDoor()
        {
            if (isLocked || isMoving) return;
            
            isMoving = true;
            openingDirection = true;
            
            if (openSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(openSound, 0.8f);
            }
            else if (creakSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(creakSound, 0.6f);
            }
        }

        public void CloseDoor()
        {
            if (isMoving) return;
            
            isMoving = true;
            openingDirection = false;
            
            if (closeSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(closeSound, 0.8f);
            }
            else if (creakSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(creakSound, 0.5f);
            }
        }

        void HandleDoorMovement()
        {
            float targetAngle = isOpen ? openAngle : 0f;
            float speed = isOpen ? closeSpeed : openSpeed;
            
            currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, speed * Time.deltaTime);
            
            // Aplica rotação ao pivot da porta
            if (openingDirection)
            {
                doorPivot.localRotation = Quaternion.Euler(0, currentAngle, 0);
            }
            else
            {
                doorPivot.localRotation = Quaternion.Euler(0, -currentAngle, 0);
            }
            
            // Verifica se terminou o movimento
            if (Mathf.Approximately(currentAngle, targetAngle))
            {
                isMoving = false;
                isOpen = !isOpen;
                
                if (isOpen)
                {
                    OnDoorOpened?.Invoke(this, true);
                    DisableCollision();
                }
                else
                {
                    OnDoorClosed?.Invoke(this, false);
                    EnableCollision();
                }
            }
        }

        public void Lock()
        {
            isLocked = true;
            OnDoorLocked?.Invoke();
        }

        public void Unlock()
        {
            isLocked = false;
            OnDoorUnlocked?.Invoke();
            
            if (unlockSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(unlockSound, 0.7f);
            }
        }

        public bool TryUnlockWithKey(string keyName)
        {
            if (!requiresKey) return false;
            
            if (keyName == requiredKeyName)
            {
                Unlock();
                return true;
            }
            
            return false;
        }

        public void ForceOpen()
        {
            isLocked = false;
            isOpen = true;
            currentAngle = openAngle;
            doorPivot.localRotation = Quaternion.Euler(0, currentAngle, 0);
            DisableCollision();
        }

        public void ForceClose()
        {
            isOpen = false;
            currentAngle = 0f;
            doorPivot.localRotation = Quaternion.identity;
            EnableCollision();
        }

        void DisableCollision()
        {
            if (doorCollider != null)
            {
                doorCollider.enabled = false;
            }
        }

        void EnableCollision()
        {
            if (doorCollider != null)
            {
                doorCollider.enabled = true;
            }
        }

        void PlayLockedSound()
        {
            if (lockedSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(lockedSound, 0.5f);
            }
        }

        public string GetPromptText()
        {
            if (isLocked)
            {
                return requiresKey ? $"Trancado (precisa: {requiredKeyName})" : "Trancado";
            }
            
            return isOpen ? "Fechar Porta" : "Abrir Porta";
        }

        // Método para ser chamado por eventos (ex: puzzle completado)
        public void OnEventTrigger(string eventType)
        {
            switch (eventType)
            {
                case "open":
                    OpenDoor();
                    break;
                case "close":
                    CloseDoor();
                    break;
                case "lock":
                    Lock();
                    break;
                case "unlock":
                    Unlock();
                    break;
                case "toggle":
                    ToggleDoor();
                    break;
            }
        }

        // Debug
        void OnDrawGizmosSelected()
        {
            // Desenha arco mostrando direção de abertura
            Vector3 pivotPos = doorPivot != null ? doorPivot.position : transform.position;
            
            Gizmos.color = isOpen ? Color.green : Color.red;
            Gizmos.DrawLine(pivotPos, pivotPos + transform.forward * 2f);
            
            // Texto de estado
#if UNITY_EDITOR
            GUIStyle style = new GUIStyle();
            style.fontSize = 14;
            style.normal.textColor = Color.white;
            
            string status = $"{(isOpen ? "Aberta" : "Fechada")} | {(isLocked ? "Trancada" : "Destrancada")}";
            UnityEditor.Handles.Label(pivotPos + Vector3.up * 2f, status, style);
#endif
        }
    }
}

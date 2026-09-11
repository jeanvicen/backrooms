using UnityEngine;

namespace Backrooms.Interactables
{
    /// <summary>
    /// Item coletável que pode ser pego e adicionado ao inventário.
    /// Suporta diferentes tipos: bateria, comida, água, chave, documento, etc.
    /// </summary>
    public class CollectibleItem : MonoBehaviour, IInteractable
    {
        public enum ItemType
        {
            Battery,
            Food,
            Water,
            Key,
            Document,
            Medicine,
            Flashlight,
            Tool,
            Artifact
        }

        [Header("Configurações do Item")]
        [SerializeField] private ItemType itemType = ItemType.Battery;
        [SerializeField] private string itemName = "Item";
        [SerializeField] private string description = "";
        [SerializeField] private int quantity = 1;
        [SerializeField] private float weight = 0.5f;
        
        [Header("Efeitos")]
        [SerializeField] private float staminaRestored = 0f;
        [SerializeField] private float sanityRestored = 0f;
        [SerializeField] private float healthRestored = 0f;
        
        [Header("Visual")]
        [SerializeField] private GameObject modelObject;
        [SerializeField] private ParticleSystem pickupEffect;
        [SerializeField] private AudioClip pickupSound;
        
        [Header("Referências")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Collider itemCollider;
        [SerializeField] private Renderer itemRenderer;
        
        // Variáveis privadas
        private bool isCollected = false;
        private bool canBePickedUp = true;
        
        // Propriedades
        public ItemType Type => itemType;
        public string Name => itemName;
        public string Description => description;
        public int Quantity => quantity;
        public float Weight => weight;
        public bool IsCollected => isCollected;
        public bool CanBePickedUp => canBePickedUp && !isCollected;
        
        // Eventos
        public delegate void ItemCollectedHandler(CollectibleItem item);
        public event ItemCollectedHandler OnItemCollected;
        
        public delegate void ItemUsedHandler(CollectibleItem item);
        public event ItemUsedHandler OnItemUsed;

        void Awake()
        {
            if (modelObject == null)
                modelObject = transform.GetChild(0)?.gameObject;
            
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            
            if (itemCollider == null)
                itemCollider = GetComponent<Collider>();
            
            if (itemRenderer == null)
                itemRenderer = GetComponent<Renderer>();
        }

        void Start()
        {
            // Adiciona rotação suave se for um item no chão
            if (CanRotate())
            {
                StartCoroutine(RotateSlowly());
            }
        }

        System.Collections.IEnumerator RotateSlowly()
        {
            while (!isCollected)
            {
                transform.Rotate(Vector3.up, 45f * Time.deltaTime);
                yield return null;
            }
        }

        bool CanRotate()
        {
            return itemType != ItemType.Document && itemType != ItemType.Key;
        }

        public bool Interact()
        {
            if (!CanBePickedUp)
            {
                return false;
            }
            
            Collect();
            return true;
        }

        public void Collect()
        {
            if (isCollected) return;
            
            isCollected = true;
            canBePickedUp = false;
            
            // Toca efeito de pickup
            PlayPickupEffect();
            
            // Notifica coleta
            OnItemCollected?.Invoke(this);
            
            Debug.Log($"Coletou: {itemName} x{quantity}");
        }

        public bool Use()
        {
            if (isCollected)
            {
                ApplyEffects();
                OnItemUsed?.Invoke(this);
                return true;
            }
            
            return false;
        }

        void ApplyEffects()
        {
            // Encontra sistemas do jogador
            PlayerController player = FindObjectOfType<PlayerController>();
            SanitySystem sanity = FindObjectOfType<SanitySystem>();
            
            // Aplica efeitos
            if (player != null && staminaRestored > 0)
            {
                player.RecoverStamina(staminaRestored);
            }
            
            if (sanity != null && sanityRestored > 0)
            {
                sanity.RecoverSanity(sanityRestored);
            }
            
            // Reduz quantidade
            quantity--;
            
            if (quantity <= 0)
            {
                DestroyItem();
            }
        }

        void PlayPickupEffect()
        {
            // Toca som
            if (pickupSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(pickupSound, 0.8f);
            }
            
            // Toca partículas
            if (pickupEffect != null)
            {
                pickupEffect.Play();
            }
            
            // Desativa visual
            if (modelObject != null)
            {
                modelObject.SetActive(false);
            }
            
            // Desativa collider
            if (itemCollider != null)
            {
                itemCollider.enabled = false;
            }
        }

        public void DestroyItem()
        {
            // Destrói o objeto após delay para efeitos
            Invoke(nameof(DestroyGameObject), 0.5f);
        }

        void DestroyGameObject()
        {
            Destroy(gameObject);
        }

        public string GetPromptText()
        {
            if (isCollected)
            {
                return "Coletado";
            }
            
            switch (itemType)
            {
                case ItemType.Battery:
                    return $"Pegar Bateria ({quantity})";
                case ItemType.Food:
                    return $"Pegar Comida ({quantity})";
                case ItemType.Water:
                    return $"Pegar Água ({quantity})";
                case ItemType.Key:
                    return $"Pegar Chave";
                case ItemType.Document:
                    return $"Ler Documento";
                case ItemType.Medicine:
                    return $"Pegar Remédio ({quantity})";
                case ItemType.Flashlight:
                    return $"Pegar Lanterna";
                case ItemType.Tool:
                    return $"Pegar Ferramenta";
                case ItemType.Artifact:
                    return $"Pegar Artefato";
                default:
                    return $"Pegar {itemName}";
            }
        }

        public bool CanInteract => CanBePickedUp;

        // Método para spawnar item dinamicamente
        public static CollectibleItem SpawnItem(ItemType type, Vector3 position, int quantity = 1)
        {
            // Em produção, carregaria de um prefab
            GameObject itemObj = new GameObject(type.ToString());
            itemObj.transform.position = position;
            
            CollectibleItem item = itemObj.AddComponent<CollectibleItem>();
            item.itemType = type;
            item.quantity = quantity;
            
            return item;
        }

        // Debug
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
            
#if UNITY_EDITOR
            GUIStyle style = new GUIStyle();
            style.fontSize = 12;
            style.normal.textColor = Color.white;
            
            string label = $"{itemName} x{quantity}\n{itemType}";
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, label, style);
#endif
        }
    }
}

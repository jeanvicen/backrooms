using UnityEngine;

namespace Backrooms.Entities
{
    /// <summary>
    /// Classe base para todas as entidades (inimigos, NPCs)
    /// Define comportamento comum e atributos básicos
    /// </summary>
    public abstract class BaseEntity : MonoBehaviour
    {
        [Header("Entity Settings")]
        [SerializeField] protected string entityName = "Unknown Entity";
        [SerializeField] protected float maxHealth = 100f;
        [SerializeField] protected float moveSpeed = 3f;
        [SerializeField] protected float runSpeed = 6f;
        
        [Header("Detection")]
        [SerializeField] protected float detectionRange = 15f;
        [SerializeField] protected float fieldOfView = 90f;
        [SerializeField] protected LayerMask playerLayer;
        [SerializeField] protected LayerMask obstacleLayer;
        
        [Header("Audio")]
        [SerializeField] protected AudioClip[] idleSounds;
        [SerializeField] protected AudioClip[] alertSounds;
        [SerializeField] protected AudioClip[] attackSounds;
        [SerializeField] protected AudioClip[] painSounds;
        [SerializeField] protected AudioSource audioSource;
        
        // State
        protected float currentHealth;
        protected bool isAlive = true;
        protected bool isAlerted = false;
        protected Transform target; // Normalmente o jogador
        
        // Events
        public delegate void EntityAlertedHandler(BaseEntity entity);
        public event EntityAlertedHandler OnEntityAlerted;
        
        public delegate void EntityDamagedHandler(BaseEntity entity, float damage);
        public event EntityDamagedHandler OnEntityDamaged;
        
        public delegate void EntityDeathHandler(BaseEntity entity);
        public event EntityDeathHandler OnEntityDeath;
        
        public string EntityName => entityName;
        public float HealthPercent => currentHealth / maxHealth;
        public bool IsAlive => isAlive;
        public bool IsAlerted => isAlerted;

        protected virtual void Start()
        {
            currentHealth = maxHealth;
            
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
            
            InitializeEntity();
        }

        /// <summary>
        /// Inicialização customizada para subclasses
        /// </summary>
        protected virtual void InitializeEntity()
        {
            // Override em subclasses
        }

        /// <summary>
        /// Loop principal de atualização da entidade
        /// </summary>
        protected virtual void Update()
        {
            if (!isAlive) return;
            
            UpdateDetection();
            UpdateBehavior();
        }

        /// <summary>
        /// Atualiza detecção do jogador
        /// </summary>
        protected virtual void UpdateDetection()
        {
            if (target == null)
            {
                target = FindPlayer();
            }
            
            if (target != null)
            {
                CheckIfCanSeeTarget();
            }
        }

        /// <summary>
        /// Encontra jogador mais próximo
        /// </summary>
        protected Transform FindPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            return player != null ? player.transform : null;
        }

        /// <summary>
        /// Verifica se pode ver o alvo
        /// </summary>
        protected virtual void CheckIfCanSeeTarget()
        {
            if (target == null) return;
            
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            
            if (distanceToTarget <= detectionRange)
            {
                Vector3 directionToTarget = (target.position - transform.position).normalized;
                float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
                
                if (angleToTarget < fieldOfView * 0.5f)
                {
                    // Raycast para verificar obstáculos
                    RaycastHit hit;
                    if (!Physics.Raycast(transform.position, directionToTarget, out hit, distanceToTarget, obstacleLayer))
                    {
                        if (!isAlerted)
                        {
                            BecomeAlerted();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Entidade fica alertada
        /// </summary>
        protected virtual void BecomeAlerted()
        {
            isAlerted = true;
            OnEntityAlerted?.Invoke(this);
            PlayAlertSound();
            Debug.Log($"[Entity] {entityName} alertado!");
        }

        /// <summary>
        /// Entidade perde alerta
        /// </summary>
        protected virtual void LoseAlert()
        {
            isAlerted = false;
            Debug.Log($"[Entity] {entityName} perdeu alerta");
        }

        /// <summary>
        /// Atualiza comportamento baseado no estado
        /// </summary>
        protected virtual void UpdateBehavior()
        {
            // Implementar em subclasses
        }

        /// <summary>
        /// Aplica dano à entidade
        /// </summary>
        public virtual void TakeDamage(float damage)
        {
            if (!isAlive) return;
            
            currentHealth -= damage;
            OnEntityDamaged?.Invoke(this, damage);
            
            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                PlayPainSound();
            }
            
            Debug.Log($"[Entity] {entityName} recebeu {damage} dano. Vida: {currentHealth}/{maxHealth}");
        }

        /// <summary>
        /// Cura a entidade
        /// </summary>
        public virtual void Heal(float amount)
        {
            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }

        /// <summary>
        /// Entidade morre
        /// </summary>
        protected virtual void Die()
        {
            isAlive = false;
            OnEntityDeath?.Invoke(this);
            Debug.Log($"[Entity] {entityName} morreu");
            
            // Destruir após delay ou desativar
            Destroy(gameObject, 3f);
        }

        /// <summary>
        /// Toca som de alerta
        /// </summary>
        protected virtual void PlayAlertSound()
        {
            if (alertSounds.Length > 0 && audioSource != null)
            {
                AudioClip sound = alertSounds[Random.Range(0, alertSounds.Length)];
                audioSource.PlayOneShot(sound);
            }
        }

        /// <summary>
        /// Toca som de dor
        /// </summary>
        protected virtual void PlayPainSound()
        {
            if (painSounds.Length > 0 && audioSource != null)
            {
                AudioClip sound = painSounds[Random.Range(0, painSounds.Length)];
                audioSource.PlayOneShot(sound, 0.7f);
            }
        }

        /// <summary>
        /// Toca som de ataque
        /// </summary>
        protected virtual void PlayAttackSound()
        {
            if (attackSounds.Length > 0 && audioSource != null)
            {
                AudioClip sound = attackSounds[Random.Range(0, attackSounds.Length)];
                audioSource.PlayOneShot(sound);
            }
        }

        /// <summary>
        /// Toca som idle aleatório
        /// </summary>
        protected virtual void PlayIdleSound()
        {
            if (idleSounds.Length > 0 && audioSource != null && !audioSource.isPlaying)
            {
                AudioClip sound = idleSounds[Random.Range(0, idleSounds.Length)];
                audioSource.PlayOneShot(sound, 0.3f);
            }
        }

        /// <summary>
        /// Desenha gizmos para debug
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
        {
            // Range de detecção
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            // Field of view
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, transform.forward * detectionRange);
        }
    }
}

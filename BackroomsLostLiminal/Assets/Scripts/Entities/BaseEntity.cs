using UnityEngine;
using UnityEngine.AI;

namespace Backrooms.Entities
{
    /// <summary>
    /// Classe base para todas as entidades das Backrooms.
    /// Implementa estados de IA: Patrulha, Perseguição, Procura, e Retorno.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(AudioSource))]
    public abstract class BaseEntity : MonoBehaviour
    {
        [Header("Configurações da Entidade")]
        [SerializeField] protected string entityName = "Entity";
        [SerializeField] protected float health = 100f;
        [SerializeField] protected float damage = 25f;
        
        [Header("Movimento")]
        [SerializeField] protected float patrolSpeed = 2f;
        [SerializeField] protected float chaseSpeed = 6f;
        [SerializeField] protected float detectionRange = 15f;
        [SerializeField] protected float attackRange = 2f;
        [SerializeField] protected float attackCooldown = 2f;
        
        [Header("Detecção")]
        [SerializeField] protected float fieldOfView = 90f;
        [SerializeField] protected float hearingRange = 20f;
        [SerializeField] protected LayerMask playerLayer;
        [SerializeField] protected LayerMask obstacleLayer;
        
        [Header("Patrulha")]
        [SerializeField] protected Transform[] patrolPoints;
        [SerializeField] protected float patrolWaitTime = 3f;
        [SerializeField] protected bool randomizePatrol = true;
        
        [Header("Áudio")]
        [SerializeField] protected AudioClip[] ambientSounds;
        [SerializeField] protected AudioClip chaseSound;
        [SerializeField] protected AudioClip attackSound;
        [SerializeField] protected AudioClip damageSound;
        [SerializeField] protected AudioClip deathSound;
        
        // Referências
        protected NavMeshAgent navMeshAgent;
        protected AudioSource audioSource;
        protected Animator animator;
        protected Transform playerTransform;
        protected PlayerController playerController;
        protected SanitySystem sanitySystem;
        
        // Estados
        protected enum EntityState { Idle, Patrol, Chase, Search, Attack, Return }
        protected EntityState currentState = EntityState.Idle;
        protected EntityState previousState = EntityState.Idle;
        
        protected int currentPatrolIndex = 0;
        protected float lastAttackTime;
        protected float lastAmbientSoundTime;
        protected float searchTimer;
        protected float returnTimer;
        protected Vector3 lastKnownPlayerPosition;
        protected Vector3 searchPosition;
        
        // Propriedades
        public string EntityName => entityName;
        public float Health => health;
        public bool IsAlive => health > 0;
        public EntityState CurrentState => currentState;
        public bool IsChasing => currentState == EntityState.Chase || currentState == EntityState.Attack;
        
        // Eventos
        public delegate void EntityStateChangedHandler(BaseEntity entity, EntityState newState, EntityState oldState);
        public event EntityStateChangedHandler OnStateChanged;
        
        public delegate void EntityDetectedPlayerHandler(BaseEntity entity);
        public event EntityDetectedPlayerHandler OnPlayerDetected;
        
        public delegate void EntityLostPlayerHandler(BaseEntity entity);
        public event EntityLostPlayerHandler OnPlayerLost;
        
        public delegate void EntityAttackedHandler(BaseEntity entity, float damage);
        public event EntityAttackedHandler OnEntityAttacked;
        
        public delegate void EntityDiedHandler(BaseEntity entity);
        public event EntityDiedHandler OnEntityDied;

        protected virtual void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            audioSource = GetComponent<AudioSource>();
            animator = GetComponent<Animator>();
            
            // Encontra o jogador
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                playerController = player.GetComponent<PlayerController>();
                sanitySystem = player.GetComponent<SanitySystem>();
            }
            
            // Configura NavMeshAgent
            if (navMeshAgent != null)
            {
                navMeshAgent.speed = patrolSpeed;
                navMeshAgent.stoppingDistance = attackRange;
            }
        }

        protected virtual void Start()
        {
            if (patrolPoints.Length > 0 && randomizePatrol)
            {
                currentPatrolIndex = Random.Range(0, patrolPoints.Length);
            }
            
            lastAmbientSoundTime = Time.time;
        }

        protected virtual void Update()
        {
            if (!IsAlive) return;
            
            CheckDetection();
            UpdateState();
            UpdateAnimations();
            PlayAmbientSounds();
        }

        protected virtual void CheckDetection()
        {
            if (playerTransform == null) return;
            
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            
            // Verifica se está dentro do range de detecção
            if (distanceToPlayer <= detectionRange)
            {
                // Verifica linha de visão
                if (HasLineOfSight(playerTransform.position))
                {
                    DetectPlayer();
                }
            }
            
            // Verifica sons (corrida do jogador)
            if (playerController != null && playerController.IsRunning && distanceToPlayer <= hearingRange)
            {
                if (HasLineOfSight(playerTransform.position))
                {
                    DetectPlayer();
                }
            }
        }

        protected virtual bool HasLineOfSight(Vector3 targetPosition)
        {
            Vector3 directionToTarget = (targetPosition - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
            
            // Verifica se está dentro do campo de visão
            if (angleToTarget > fieldOfView / 2f)
            {
                return false;
            }
            
            // Raycast para verificar obstáculos
            if (Physics.Raycast(transform.position, directionToTarget, out RaycastHit hit, detectionRange, obstacleLayer))
            {
                return false;
            }
            
            return true;
        }

        protected virtual void DetectPlayer()
        {
            if (currentState != EntityState.Chase && currentState != EntityState.Attack)
            {
                ChangeState(EntityState.Chase);
                OnPlayerDetected?.Invoke(this);
                
                // Drena sanidade do jogador ao ver entidade
                if (sanitySystem != null)
                {
                    sanitySystem.SetEntityNearby(true);
                }
            }
            
            lastKnownPlayerPosition = playerTransform.position;
        }

        protected virtual void LosePlayer()
        {
            if (currentState == EntityState.Chase || currentState == EntityState.Attack)
            {
                searchPosition = lastKnownPlayerPosition;
                searchTimer = 5f; // Tempo procurando
                ChangeState(EntityState.Search);
                OnPlayerLost?.Invoke(this);
            }
        }

        protected virtual void UpdateState()
        {
            switch (currentState)
            {
                case EntityState.Idle:
                    UpdateIdleState();
                    break;
                    
                case EntityState.Patrol:
                    UpdatePatrolState();
                    break;
                    
                case EntityState.Chase:
                    UpdateChaseState();
                    break;
                    
                case EntityState.Search:
                    UpdateSearchState();
                    break;
                    
                case EntityState.Attack:
                    UpdateAttackState();
                    break;
                    
                case EntityState.Return:
                    UpdateReturnState();
                    break;
            }
        }

        protected virtual void UpdateIdleState()
        {
            navMeshAgent.isStopped = true;
            
            // Transição para patrulha após tempo aleatório
            if (Random.value < 0.01f && patrolPoints.Length > 0)
            {
                ChangeState(EntityState.Patrol);
            }
        }

        protected virtual void UpdatePatrolState()
        {
            if (patrolPoints.Length == 0)
            {
                ChangeState(EntityState.Idle);
                return;
            }
            
            Transform targetPoint = patrolPoints[currentPatrolIndex];
            
            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                // Chegou no ponto, espera
                navMeshAgent.isStopped = true;
                
                if (Time.time % patrolWaitTime < Time.deltaTime)
                {
                    // Próximo ponto
                    if (randomizePatrol)
                    {
                        currentPatrolIndex = Random.Range(0, patrolPoints.Length);
                    }
                    else
                    {
                        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                    }
                    
                    navMeshAgent.isStopped = false;
                    SetDestination(patrolPoints[currentPatrolIndex].position);
                }
            }
            else
            {
                navMeshAgent.isStopped = false;
            }
        }

        protected virtual void UpdateChaseState()
        {
            if (playerTransform == null)
            {
                LosePlayer();
                return;
            }
            
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            
            if (distanceToPlayer <= attackRange)
            {
                ChangeState(EntityState.Attack);
            }
            else if (!HasLineOfSight(playerTransform.position))
            {
                LosePlayer();
            }
            else
            {
                SetDestination(playerTransform.position);
            }
        }

        protected virtual void UpdateSearchState()
        {
            searchTimer -= Time.deltaTime;
            
            if (searchTimer <= 0f)
            {
                // Desiste da procura
                if (patrolPoints.Length > 0)
                {
                    ChangeState(EntityState.Patrol);
                }
                else
                {
                    ChangeState(EntityState.Idle);
                }
                
                if (sanitySystem != null)
                {
                    sanitySystem.SetEntityNearby(false);
                }
            }
            else
            {
                // Procura na última posição conhecida
                if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                {
                    // Move aleatoriamente perto da última posição
                    Vector3 randomOffset = new Vector3(
                        Random.Range(-5f, 5f),
                        0,
                        Random.Range(-5f, 5f)
                    );
                    SetDestination(searchPosition + randomOffset);
                }
            }
        }

        protected virtual void UpdateAttackState()
        {
            if (playerTransform == null)
            {
                ChangeState(EntityState.Search);
                return;
            }
            
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            
            if (distanceToPlayer > attackRange * 1.5f)
            {
                ChangeState(EntityState.Chase);
                return;
            }
            
            // Ataca
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                PerformAttack();
            }
            
            // Mantém olhando para o jogador
            transform.LookAt(new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z));
        }

        protected virtual void UpdateReturnState()
        {
            returnTimer -= Time.deltaTime;
            
            if (returnTimer <= 0f)
            {
                if (patrolPoints.Length > 0)
                {
                    ChangeState(EntityState.Patrol);
                }
                else
                {
                    ChangeState(EntityState.Idle);
                }
            }
        }

        protected virtual void PerformAttack()
        {
            lastAttackTime = Time.time;
            
            // Toca som de ataque
            if (attackSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(attackSound);
            }
            
            // Aplica dano ao jogador
            if (playerController != null)
            {
                playerController.TakeStaminaDamage(damage);
                
                // Também afeta sanidade
                if (sanitySystem != null)
                {
                    sanitySystem.DrainSanity(10f, "Atacado por entidade");
                }
            }
            
            OnEntityAttacked?.Invoke(this, damage);
        }

        protected virtual void ChangeState(EntityState newState)
        {
            if (currentState == newState) return;
            
            previousState = currentState;
            currentState = newState;
            
            OnStateChanged?.Invoke(this, newState, previousState);
            
            // Ajusta velocidade baseado no estado
            switch (newState)
            {
                case EntityState.Patrol:
                    navMeshAgent.speed = patrolSpeed;
                    break;
                    
                case EntityState.Chase:
                case EntityState.Attack:
                    navMeshAgent.speed = chaseSpeed;
                    break;
                    
                default:
                    navMeshAgent.speed = patrolSpeed;
                    break;
            }
        }

        protected virtual void UpdateAnimations()
        {
            if (animator == null) return;
            
            // Atualiza parâmetros do animator
            animator.SetFloat("Speed", navMeshAgent.velocity.magnitude);
            animator.SetBool("IsChasing", IsChasing);
            animator.SetBool("IsAttacking", currentState == EntityState.Attack);
        }

        protected virtual void PlayAmbientSounds()
        {
            if (ambientSounds.Length == 0 || audioSource == null) return;
            
            // Toca sons ambientais aleatoriamente
            if (Time.time >= lastAmbientSoundTime + Random.Range(5f, 15f))
            {
                AudioClip sound = ambientSounds[Random.Range(0, ambientSounds.Length)];
                audioSource.PlayOneShot(sound, 0.3f);
                lastAmbientSoundTime = Time.time;
            }
        }

        protected virtual void SetDestination(Vector3 position)
        {
            if (navMeshAgent != null)
            {
                navMeshAgent.SetDestination(position);
            }
        }

        public virtual void TakeDamage(float amount)
        {
            health -= amount;
            
            if (damageSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(damageSound);
            }
            
            if (health <= 0)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            if (deathSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(deathSound);
            }
            
            OnEntityDied?.Invoke(this);
            
            // Desativa entidade (ou implementa sistema de respawn)
            gameObject.SetActive(false);
        }

        // Métodos públicos para controle externo
        public void SetPatrolPoints(Transform[] points)
        {
            patrolPoints = points;
        }
        
        public void ForceState(EntityState state)
        {
            ChangeState(state);
        }
        
        public void ResetEntity()
        {
            health = 100f;
            currentState = EntityState.Idle;
            navMeshAgent.enabled = true;
            gameObject.SetActive(true);
        }

        // Debug
        protected virtual void OnDrawGizmosSelected()
        {
            // Range de detecção
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            // Campo de visão
            Vector3 leftAngle = Quaternion.Euler(0, -fieldOfView / 2f, 0) * transform.forward;
            Vector3 rightAngle = Quaternion.Euler(0, fieldOfView / 2f, 0) * transform.forward;
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + leftAngle * detectionRange);
            Gizmos.DrawLine(transform.position, transform.position + rightAngle * detectionRange);
            
            // Pontos de patrulha
            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                Gizmos.color = Color.green;
                foreach (Transform point in patrolPoints)
                {
                    if (point != null)
                    {
                        Gizmos.DrawSphere(point.position, 0.5f);
                    }
                }
            }
        }
    }
}

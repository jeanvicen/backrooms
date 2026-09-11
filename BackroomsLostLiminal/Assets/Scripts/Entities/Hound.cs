using UnityEngine;

namespace Backrooms.Entities
{
    /// <summary>
    /// Hound - Criatura quadrúpede agressiva encontrada no Level 1.
    /// Rápida, ataca em grupo, e é fraca contra luz forte.
    /// </summary>
    public class Hound : BaseEntity
    {
        [Header("Configurações Específicas do Hound")]
        [SerializeField] private float lightRepelRange = 8f;
        [SerializeField] private float lightRepelForce = 5f;
        [SerializeField] private float groupAttackBonus = 0.2f; // 20% mais dano por hound próximo
        
        [Header("Referências")]
        [SerializeField] private LightDetector lightDetector;
        
        // Variáveis privadas
        private bool isRepelledByLight;
        private float lastGroupCheckTime;
        private int nearbyHoundsCount;
        
        protected override void Awake()
        {
            base.Awake();
            
            if (lightDetector == null)
                lightDetector = GetComponent<LightDetector>();
            
            entityName = "Hound";
        }

        protected override void Update()
        {
            if (!IsAlive) return;
            
            CheckLightRepel();
            CheckGroupBonus();
            
            base.Update();
        }

        void CheckLightRepel()
        {
            if (lightDetector == null) return;
            
            // Verifica se há luz forte próxima
            float closestLightIntensity = lightDetector.GetClosestLightIntensity(out Vector3 lightPosition);
            
            if (closestLightIntensity > 0.7f && Vector3.Distance(transform.position, lightPosition) < lightRepelRange)
            {
                isRepelledByLight = true;
                
                // Move-se para longe da luz
                Vector3 awayFromLight = (transform.position - lightPosition).normalized;
                navMeshAgent.SetDestination(transform.position + awayFromLight * lightRepelRange);
                
                // Reduz velocidade quando repelido
                navMeshAgent.speed = patrolSpeed * 0.5f;
            }
            else
            {
                isRepelledByLight = false;
                
                // Restaura velocidade normal
                if (currentState == EntityState.Chase || currentState == EntityState.Attack)
                {
                    navMeshAgent.speed = chaseSpeed;
                }
                else
                {
                    navMeshAgent.speed = patrolSpeed;
                }
            }
        }

        void CheckGroupBonus()
        {
            // Verifica outros hounds próximos a cada 2 segundos
            if (Time.time < lastGroupCheckTime + 2f) return;
            
            lastGroupCheckTime = Time.time;
            
            // Procura por outros hounds num raio de 10 metros
            Collider[] nearbyEntities = Physics.OverlapSphere(transform.position, 10f);
            nearbyHoundsCount = 0;
            
            foreach (Collider collider in nearbyEntities)
            {
                Hound otherHound = collider.GetComponent<Hound>();
                if (otherHound != null && otherHound != this && otherHound.IsAlive)
                {
                    nearbyHoundsCount++;
                }
            }
        }

        protected override void PerformAttack()
        {
            // Aumenta dano baseado no número de hounds próximos
            float bonusDamage = damage * (nearbyHoundsCount * groupAttackBonus);
            float totalDamage = damage + bonusDamage;
            
            base.PerformAttack();
            
            Debug.Log($"Hound atacou com {totalDamage:F1} de dano (bônus de grupo: {nearbyHoundsCount})");
        }

        protected override void ChangeState(EntityState newState)
        {
            // Não pode patrulhar se estiver sendo repelido por luz
            if (isRepelledByLight && newState == EntityState.Patrol)
            {
                return;
            }
            
            base.ChangeState(newState);
        }

        protected override void Die()
        {
            Debug.Log("Hound morreu!");
            base.Die();
        }

        // Debug
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            
            // Range de repulsão por luz
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, lightRepelRange);
            
            // Conta de hounds próximos
            if (Application.isPlaying)
            {
                GUIStyle style = new GUIStyle();
                style.fontSize = 16;
                style.normal.textColor = Color.white;
                
#if UNITY_EDITOR
                UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, 
                    $"Hounds próximos: {nearbyHoundsCount}", style);
#endif
            }
        }
    }

    /// <summary>
    /// Detector de luz para entidades. Usado para detectar fontes de luz
    /// e calcular intensidade luminosa em pontos específicos.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class LightDetector : MonoBehaviour
    {
        [Header("Configurações")]
        [SerializeField] private int lightSamples = 8;
        [SerializeField] private float sampleRadius = 2f;
        [SerializeField] private LayerMask lightObstacleLayer;
        
        /// <summary>
        /// Obtém a intensidade da luz mais próxima e sua posição.
        /// </summary>
        public float GetClosestLightIntensity(out Vector3 lightPosition)
        {
            lightPosition = transform.position;
            float closestIntensity = 0f;
            Transform closestLight = null;
            
            // Encontra todas as luzes na cena
            Light[] lights = FindObjectsOfType<Light>();
            
            foreach (Light light in lights)
            {
                float distance = Vector3.Distance(transform.position, light.transform.position);
                
                // Calcula intensidade baseada na distância e intensidade da luz
                float intensity = CalculateLightIntensity(light, distance);
                
                if (intensity > closestIntensity)
                {
                    closestIntensity = intensity;
                    closestLight = light.transform;
                }
            }
            
            if (closestLight != null)
            {
                lightPosition = closestLight.position;
            }
            
            return closestIntensity;
        }
        
        /// <summary>
        /// Calcula intensidade da luz em um ponto específico.
        /// </summary>
        public float CalculateLightIntensity(Light light, float distance)
        {
            // Intensidade base diminui com o quadrado da distância
            float intensity = light.intensity / (distance * distance);
            
            // Considera range da luz
            if (distance > light.range)
            {
                intensity = 0f;
            }
            
            // Verifica se há obstáculos entre a luz e este ponto
            if (Physics.Linecast(light.transform.position, transform.position, out RaycastHit hit, lightObstacleLayer))
            {
                intensity *= 0.3f; // Reduz intensidade se houver obstáculo
            }
            
            return Mathf.Clamp01(intensity);
        }
        
        /// <summary>
        /// Amostra intensidade de luz ao redor deste ponto.
        /// </summary>
        public float SampleAmbientLight()
        {
            float totalIntensity = 0f;
            
            for (int i = 0; i < lightSamples; i++)
            {
                float angle = (i / (float)lightSamples) * Mathf.PI * 2f;
                Vector3 sampleDirection = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                Vector3 samplePosition = transform.position + sampleDirection * sampleRadius;
                
                // Raycast para cima para detectar luz ambiente
                if (Physics.Raycast(samplePosition, Vector3.up, out RaycastHit hit, 10f))
                {
                    totalIntensity += 1f;
                }
            }
            
            return totalIntensity / lightSamples;
        }
    }
}

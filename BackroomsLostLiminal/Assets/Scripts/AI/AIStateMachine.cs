using UnityEngine;
using System.Collections.Generic;

namespace Backrooms.AI
{
    /// <summary>
    /// Sistema de IA baseado em máquina de estados finitos (FSM)
    /// Usado por todas as entidades para comportamento
    /// </summary>
    public class AIStateMachine : MonoBehaviour
    {
        [Header("State Settings")]
        [SerializeField] private AIState initialState;
        
        // State atual
        private AIState currentState;
        private Dictionary<System.Type, AIState> states = new Dictionary<System.Type, AIState>();
        
        // Events
        public delegate voidStateChangedHandler(AIState newState, AIState previousState);
        public event StateChangedHandler OnStateChanged;
        
        public AIState CurrentState => currentState;

        private void Start()
        {
            InitializeStates();
            
            if (initialState != null)
            {
                SetState(initialState);
            }
        }

        private void Update()
        {
            if (currentState != null && currentState.enabled)
            {
                currentState.ExecuteUpdate();
            }
        }

        /// <summary>
        /// Inicializa todos os estados e adiciona ao dicionário
        /// </summary>
        private void InitializeStates()
        {
            AIState[] allStates = GetComponentsInChildren<AIState>();
            
            foreach (AIState state in allStates)
            {
                state.Initialize(this);
                states[state.GetType()] = state;
                state.enabled = false;
            }
        }

        /// <summary>
        /// Muda para um estado específico
        /// </summary>
        public void SetState(AIState newState)
        {
            if (currentState == newState) return;
            if (newState == null) return;
            
            AIState previousState = currentState;
            
            // Sair do estado atual
            if (currentState != null)
            {
                currentState.OnExit();
                currentState.enabled = false;
            }
            
            // Entrar no novo estado
            currentState = newState;
            currentState.enabled = true;
            currentState.OnEnter();
            
            OnStateChanged?.Invoke(currentState, previousState);
            
            Debug.Log($"[AI] Estado mudou para: {currentState.StateName}");
        }

        /// <summary>
        /// Muda para estado por tipo
        /// </summary>
        public void SetState<T>() where T : AIState
        {
            System.Type stateType = typeof(T);
            
            if (states.ContainsKey(stateType))
            {
                SetState(states[stateType]);
            }
            else
            {
                Debug.LogError($"[AI] Estado {stateType.Name} não encontrado!");
            }
        }

        /// <summary>
        /// Retorna estado por tipo
        /// </summary>
        public T GetState<T>() where T : AIState
        {
            System.Type stateType = typeof(T);
            
            if (states.ContainsKey(stateType))
            {
                return (T)states[stateType];
            }
            
            return null;
        }

        /// <summary>
        /// Reverte para estado anterior
        /// </summary>
        public void RevertToPreviousState()
        {
            // Implementar se necessário
        }
    }

    /// <summary>
    /// Classe base para estados de IA
    /// </summary>
    public abstract class AIState : MonoBehaviour
    {
        protected AIStateMachine stateMachine;
        protected Transform entityTransform;
        protected AudioSource audioSource;
        
        [SerializeField] protected string stateName = "Base State";
        
        public string StateName => stateName;

        /// <summary>
        /// Inicializa o estado com referência à state machine
        /// </summary>
        public virtual void Initialize(AIStateMachine machine)
        {
            stateMachine = machine;
            entityTransform = transform;
            audioSource = GetComponent<AudioSource>();
        }

        /// <summary>
        /// Chamado quando entra neste estado
        /// </summary>
        public virtual void OnEnter()
        {
            enabled = true;
        }

        /// <summary>
        /// Chamado quando sai deste estado
        /// </summary>
        public virtual void OnExit()
        {
            enabled = false;
        }

        /// <summary>
        /// Atualizado a cada frame enquanto neste estado
        /// </summary>
        public virtual void ExecuteUpdate()
        {
            HandleBehavior();
            CheckTransitions();
        }

        /// <summary>
        /// Lógica principal do estado
        /// </summary>
        protected abstract void HandleBehavior();

        /// <summary>
        /// Verifica condições para transição de estado
        /// </summary>
        protected virtual void CheckTransitions()
        {
            // Override em subclasses
        }

        /// <summary>
        /// Muda para outro estado
        /// </summary>
        protected void ChangeState(AIState newState)
        {
            stateMachine.SetState(newState);
        }

        /// <summary>
        /// Muda para estado por tipo
        /// </summary>
        protected void ChangeState<T>() where T : AIState
        {
            stateMachine.SetState<T>();
        }
    }

    /// <summary>
    /// Enum para tipos de estados comuns
    /// </summary>
    public enum AIStateType
    {
        Idle,
        Patrol,
        Alert,
        Chase,
        Attack,
        Search,
        Return,
        Flee
    }
}

# 📘 Guia de Desenvolvimento - Backrooms: Lost in the Liminal

## 📁 Estrutura de Pastas

```
BackroomsLostLiminal/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/              # Sistemas principais do jogador
│   │   │   ├── PlayerController.cs      # Movimento, stamina, head bob
│   │   │   ├── SanitySystem.cs          # Sistema de sanidade mental
│   │   │   └── InteractionSystem.cs     # Sistema de interação por raycast
│   │   ├── Entities/          # IA das entidades
│   │   │   ├── BaseEntity.cs            # Classe base para todas as entidades
│   │   │   └── Hound.cs                 # Entidade Hound (Level 1)
│   │   ├── Interactables/     # Objetos interativos
│   │   │   ├── Door.cs                  # Portas interativas
│   │   │   └── CollectibleItem.cs       # Itens coletáveis
│   │   ├── Systems/           # Sistemas globais
│   │   │   ├── Flashlight.cs            # Sistema de lanterna
│   │   │   └── LightFlickerController.cs # Flicker de luzes
│   │   └── UI/                # Interface e gerenciamento
│   │       └── GameManager.cs           # Gerenciador principal do jogo
│   ├── Scenes/                # Cenas do jogo (a criar)
│   ├── Materials/             # Materiais URP (a criar)
│   ├── Textures/              # Texturas 4K (a criar)
│   ├── Audio/                 # Trilha sonora e SFX (a criar)
│   ├── Models/                # Modelos 3D (a criar)
│   └── Prefabs/               # Prefabs organizados (a criar)
├── ProjectSettings/           # Configurações da Unity
└── README.md
```

## 🔧 Scripts Implementados

### Core (Núcleo)

#### PlayerController.cs
- **Função:** Controla movimento em primeira pessoa
- **Features:**
  - Andar, correr, agachar, rastejar
  - Sistema de stamina
  - Head bob realista
  - Transição suave de altura
  - Sons de passo por superfície
- **Inputs:** WASD, Shift, Ctrl, Espaço, Mouse

#### SanitySystem.cs
- **Função:** Gerencia sanidade mental do jogador
- **Features:**
  - Drenagem por escuridão, entidades, estar perdido
  - Alucinações visuais e auditivas
  - Efeitos visuais (vignette, aberração cromática)
  - Inversão temporária de controles
  - Sussurros e batimentos cardíacos
- **Thresholds:** Baixa (<40%), Crítica (<20%), Alucinações (<30%)

#### InteractionSystem.cs
- **Função:** Sistema de interação com objetos
- **Features:**
  - Raycast a partir da câmera
  - Detecção de objetos olhados
  - Prompt de interação dinâmico
  - Interface IInteractable
- **Input:** Tecla E

### Entities (Entidades)

#### BaseEntity.cs
- **Função:** Classe base para IA de entidades
- **Estados:** Idle, Patrol, Chase, Search, Attack, Return
- **Features:**
  - Detecção por visão (cone) e áudio
  - Sistema de patrulha com waypoints
  - Perseguição ao jogador
  - Procura quando perde o jogador
  - Animações via Animator
- **Configurável:** Velocidade, range, campo de visão, dano

#### Hound.cs
- **Função:** Entidade específica do Level 1
- **Features:**
  - Repelido por luz forte
  - Bônus de dano em grupo
  - Mais rápido que outras entidades
- **Fraqueza:** Luzes intensas

### Interactables (Objetos Interativos)

#### Door.cs
- **Função:** Portas interativas
- **Features:**
  - Abrir/fechar com animação
  - Trancar/destrancar
  - Requerer chaves específicas
  - Eventos de estado
- **Configurável:** Velocidade, ângulo, sons

#### CollectibleItem.cs
- **Função:** Itens coletáveis
- **Tipos:** Bateria, Comida, Água, Chave, Documento, Remédio, etc.
- **Features:**
  - Rotação automática no chão
  - Efeitos ao coletar
  - Restaura stamina/sanidade
  - Sistema de quantidade

### Systems (Sistemas)

#### Flashlight.cs
- **Função:** Lanterna do jogador
- **Features:**
  - Sistema de bateria
  - Flicker aleatório
  - Alerta de bateria baixa
  - Recarga automática quando desligada
- **Input:** Tecla F

#### LightFlickerController.cs
- **Função:** Controla flicker de luzes ambientais
- **Features:**
  - Flicker aleatório
  - Modo strobe
  - Corte/restauração de energia
  - Sons de buzz elétrico
- **Configurável:** Intervalo, intensidade, duração

### UI (Interface)

#### GameManager.cs
- **Função:** Gerenciador principal do jogo
- **Features:**
  - Singleton persistente
  - Controle de pause/resume
  - Carregamento de cenas
  - Menu principal
  - Configurações de qualidade/volume
- **Input:** ESC para pause

## 🎮 Próximos Passos

### Fase 1: Protótipo Jogável ✅
- [x] Estrutura do projeto
- [x] Player Controller
- [x] Sistema de Sanidade
- [x] Sistema de Interação
- [x] Entity Base + Hound
- [x] Sistema de Lanterna
- [x] Portas e itens
- [ ] Cena de teste Level 0
- [ ] NavMesh configurado
- [ ] Iluminação URP básica

### Fase 2: Alpha
- [ ] Level 0 completo (geometria, texturas)
- [ ] Level 1 com Hounds
- [ ] Sistema de inventário
- [ ] Save/Load
- [ ] Áudio implementado
- [ ] UI completa (HUD, menus)

### Fase 3: Beta
- [ ] Todos os 7 níveis
- [ ] Todas as entidades
- [ ] Lore espalhada
- [ ] Otimização
- [ ] Testes de balanceamento

### Fase 4: Lançamento
- [ ] Polimento final
- [ ] Trailer
- [ ] Steam page
- [ ] Lançamento

## ⚙️ Configuração na Unity

### Requisitos
- Unity 2024.2 ou superior
- Universal Render Pipeline (URP)
- Input System (opcional, pode usar Input clássico)
- TextMeshPro (para UI)

### Configurar URP
1. Window > Package Manager
2. Instalar "Universal RP"
3. Edit > Project Settings > Graphics
4. Assign URP Asset em "Scriptable Render Pipeline Settings"

### Configurar Layers
Criar as seguintes layers:
- `Player` (6)
- `Enemy` (7)
- `Interactable` (8)
- `LightObstacle` (9)

### Configurar Tags
- `Player` - Jogador
- `Enemy` - Inimigos
- `Interactable` - Objetos interativos
- `Checkpoint` - Pontos de save

### Configurar Input (Edit > Project Settings > Input Manager)
- Horizontal: A/D, Left/Right Arrow
- Vertical: W/S, Up/Down Arrow
- Jump: Space
- Fire1: Left Ctrl
- Fire2: Left Alt
- Submit: E

## 🐛 Debug e Testes

### Comandos de Console
```csharp
// No PlayerController
player.RecoverStamina(100f);

// No SanitySystem
sanity.RecoverSanity(100f);
sanity.DrainSanity(50f, "teste");

// No Flashlight
flashlight.AddBattery(50f);
flashlight.ForceState(true, 100f);

// Na EntityManager
entity.TakeDamage(100f);
entity.ResetEntity();
```

### Atalhos de Debug
- `~` (til): Abre console
- `F1`: Mostra FPS (implementar)
- `F5`: Recarrega cena atual
- `ESC`: Pause menu

## 📊 Performance

### Otimizações Recomendadas
1. **LOD Group** em modelos distantes
2. **Occlusion Culling** ativado
3. **Light Baking** para luzes estáticas
4. **Texture Streaming** para reduzir RAM
5. **Audio Spatializer** para sons 3D

### Budget de Performance
- **FPS Alvo:** 60 FPS (mínimo), 144+ (recomendado)
- **Draw Calls:** < 1000 por frame
- **Triangles:** < 500k visíveis
- **Memory:** < 4GB RAM

## 📝 Convenções de Código

### Naming
- Classes: PascalCase (ex: `PlayerController`)
- Métodos: PascalCase (ex: `HandleMovement()`)
- Variáveis privadas: camelCase com _ (ex: `_currentHealth`)
- Variáveis públicas: PascalCase (ex: `CurrentHealth`)
- Constantes: UPPER_CASE (ex: `MAX_SPEED`)

### Organização
- Headers com `[Header("Categoria")]` no Inspector
- Regiões `#region` para agrupar métodos relacionados
- Comentários XML `///` para documentação
- Events para comunicação entre sistemas

### Padrões Utilizados
- **Singleton:** GameManager
- **Observer:** Eventos C# (delegate/event)
- **Strategy:** Estados de IA (enum + switch)
- **Command:** Sistema de interação (IInteractable)
- **Component:** Arquitetura ECS da Unity

---

*"A solidão é o maior inimigo... mas não é o único."*

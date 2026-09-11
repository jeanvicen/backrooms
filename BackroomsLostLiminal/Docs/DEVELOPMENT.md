# 📋 DOCUMENTAÇÃO DO PROJETO

## Backrooms: Lost in the Liminal

---

## 🎯 VISÃO GERAL

Este documento contém a documentação completa do jogo de terror psicológico baseado no universo das Backrooms.

---

## 📁 ESTRUTURA DE CÓDIGO

### Core (Núcleo)

#### GameManager.cs
**Responsabilidade:** Gerenciador principal singleton do jogo
- Controla estado global (sanidade, saúde, stamina)
- Gerencia transição entre níveis
- Sistema de checkpoints e saves
- Eventos para UI e outros sistemas

**Uso:**
```csharp
// Acessar instância singleton
GameManager.Instance.ModifySanity(-10f);
GameManager.Instance.LoadLevel("Level1_HabitableZone");

// Assinar eventos
GameManager.OnSanityChanged += OnSanityChanged;
```

#### SaveSystem.cs
**Responsabilidade:** Sistema de salvamento de dados
- Checkpoints automáticos
- Saves manuais (3 slots)
- Serialização JSON
- Permadeath support

---

### Player (Jogador)

#### FirstPersonController.cs
**Responsabilidade:** Controlador de movimento em primeira pessoa
- WASD + Mouse
- Sistema de stamina (correr, agachar)
- Head bob e inclinação de câmera
- Detecção de superfície para passos

**Configurações Principais:**
- Walk Speed: 5 m/s
- Run Speed: 8 m/s
- Crouch Speed: 2 m/s
- Crawl Speed: 1 m/s

#### SanityManager.cs
**Responsabilidade:** Sistema de sanidade mental
- Drenagem por escuridão, entidades, eventos
- Recuperação em luz/zonas seguras
- Alucinações visuais e auditivas
- Estados: Normal, Concerned, Unstable, Low, Critical

**Thresholds:**
- High: 75-100%
- Medium: 50-75%
- Low: 25-50%
- Critical: 0-25%

#### FlashlightManager.cs
**Responsabilidade:** Sistema da lanterna
- Toggle com tecla F
- Bateria drena com uso
- Flicker aleatório para atmosfera
- Recarga quando desligada

**Configurações:**
- Battery Drain: 5%/s
- Battery Recharge: 2%/s
- Flicker Chance: 30%

---

### Interactables (Objetos Interagíveis)

#### InteractionSystem.cs
**Responsabilidade:** Sistema de interação por raycast
- Detecta objetos olhados
- Prompt de interação [E]
- Interface IInteractable

#### Door.cs
**Responsabilidade:** Portas interagíveis
- Abre/fecha com animação
- Pode estar trancada
- Requer chaves específicas
- Auto-close opcional

---

### Inventory (Inventário)

#### InventoryItem.cs
**Responsabilidade:** Definição de itens
- Tipos: Consumable, Tool, KeyItem, Document, Weapon, Misc
- Suporte a stacking
- Dados customizados (bateria, durabilidade)

#### InventoryManager.cs
**Responsabilidade:** Gerenciamento de inventário
- 8 slots
- Adicionar/remover/usar itens
- Eventos para UI
- Drop system

---

### Entities (Entidades)

#### BaseEntity.cs
**Responsabilidade:** Classe base para todas as entidades
- Saúde e detecção
- Sistema de áudio
- Events de dano/morte
- Field of view e range

---

### AI (Inteligência Artificial)

#### AIStateMachine.cs
**Responsabilidade:** FSM para comportamento de IA
- Estados: Idle, Patrol, Alert, Chase, Attack, Search, Return, Flee
- Transições entre estados
- Events de mudança de estado

---

## 🎮 MECÂNICAS IMPLEMENTADAS

### ✅ Movimento
- [x] Andar (WASD)
- [x] Correr (Shift) - consome stamina
- [x] Agachar (Ctrl)
- [x] Rastejar (Ctrl + Shift)
- [x] Pulo limitado (Espaço)
- [x] Head bob realista
- [x] Inclinação de câmera

### ✅ Sanidade
- [x] Drenagem por escuridão
- [x] Drenagem por entidades
- [x] Recuperação em luz
- [x] Recuperação em zona segura
- [x] Alucinações (estrutura pronta)
- [x] Estados de sanidade

### ✅ Inventário
- [x] 8 slots
- [x] Stack de itens
- [x] Sistema de chaves
- [x] Eventos para UI
- [x] Drop de itens

### ✅ Interação
- [x] Raycast detection
- [x] Prompt visual
- [x] Portas animadas
- [x] Sistema de trancas

### ✅ Lanterna
- [x] Toggle F
- [x] Bateria
- [x] Flicker atmosférico
- [x] Recarga

### ✅ IA Básica
- [x] FSM structure
- [x] Detecção por visão
- [x] Patrol points (estrutura)
- [x] Chase behavior (estrutura)

---

## 🔧 PRÓXIMOS PASSOS

### Fase 1 - Protótipo Jogável
1. [ ] Criar cena Level 0 (geometria básica)
2. [ ] Configurar URP (Universal Render Pipeline)
3. [ ] Implementar sistema de áudio 3D
4. [ ] Criar UI básica (sanidade, stamina, inventory)
5. [ ] Implementar entidade Hound simples

### Fase 2 - Alpha
1. [ ] Texturas e materiais finalizados
2. [ ] Iluminação e sombras
3. [ ] Sistema de puzzles
4. [ ] Mais entidades (Smiler, Partygoer)
5. [ ] Sistema de notas/lore

### Fase 3 - Beta
1. [ ] Todos os 7 níveis
2. [ ] Otimização de performance
3. [ ] Testes de gameplay
4. [ ] Balanceamento

### Fase 4 - Lançamento
1. [ ] Polimento final
2. [ ] Marketing materials
3. [ ] Publicação Steam

---

## 📝 CONFIGURAÇÕES RECOMENDADAS UNITY

### Quality Settings (URP)
- Anti Aliasing: 4x MSAA
- Shadow Resolution: High
- Shadow Distance: 50m
- Ambient Occlusion: Enabled
- Screen Space Shadows: Enabled

### Lighting
- Realtime GI: Off
- Baked GI: On
- Lightmap Resolution: 40
- Lightmap Padding: 2

### Post Processing
- Film Grain: 0.3
- Vignette: 0.2
- Chromatic Aberration: 0.1
- Bloom: 0.5
- Color Adjustments: Desaturado (-20)

---

## 🎵 ÁUDIO

### Canais
1. Master
2. Music
3. SFX
4. Voice
5. Ambient

### Mixer Groups
- MainMixer
  - MusicBus
  - SFXBus
  - VoiceBus
  - AmbientBus

---

## 📊 PERFORMANCE TARGETS

| Plataforma | Resolução | FPS Target | Quality |
|------------|-----------|------------|---------|
| PC Low | 1080p | 60 | Medium |
| PC Medium | 1440p | 60 | High |
| PC High | 4K | 60+ | Ultra |
| Console | 1440p | 60 | High |

---

## ⚠️ NOTAS IMPORTANTES

1. **Não usar Unity Assets padrão** - Todos assets devem ser customizados para estética única
2. **Otimizar draw calls** - Usar GPU instancing sempre que possível
3. **Audio spatializer** - Configurar corretamente para imersão 3D
4. **Save system** - Testar em múltiplas plataformas
5. **Acessibilidade** - Implementar opções de daltonismo, legendas

---

*"Nas Backrooms, a solidão é o maior inimigo... mas não é o único."*

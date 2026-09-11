# Backrooms: Lost in the Liminal

🔦 **Um jogo de terror psicológico em primeira pessoa baseado no universo das Backrooms**

---

## 📋 VISÃO GERAL

**Título:** Backrooms: Lost in the Liminal  
**Gênero:** Horror psicológico / Survival / Exploração  
**Perspectiva:** Primeira pessoa  
**Engine:** Unity 2024+ com URP (Universal Render Pipeline)  
**Público-alvo:** Jogadores de terror que gostam de atmosfera, mistério e tensão

---

## 🎨 DIREÇÃO DE ARTE

### Estilo Visual
- Estética liminal: espaços vazios, repetitivos e inquietantes
- Iluminação dramática com sombras dinâmicas e luzes fluorescentes piscando
- Texturas de alta resolução (4K) para paredes, carpetes e tetos
- Efeitos de pós-processamento: grain de filme, vinheta, aberração cromática, bloom
- Paleta de cores: amarelo envelhecido, marrom, verde musgo, cinza concreto
- Neblina volumétrica para profundidade e atmosfera opressiva

### Qualidade Gráfica
- Ray tracing opcional para reflexos e sombras
- Iluminação global em tempo real (GI)
- Partículas para poeira flutuando no ar
- Shaders customizados para paredes úmidas e superfícies deterioradas
- Animações suaves de câmera (head bob, respiração, tremor de medo)

---

## 🏢 ESTRUTURA DOS NÍVEIS

| Nível | Nome | Ambiente | Duração | Ameaças |
|-------|------|----------|---------|---------|
| 0 | The Lobby | Corredores de escritório amarelo | 10-15 min | Nenhuma (tutorial) |
| 1 | Habitable Zone | Armazéns industriais | 30-45 min | Hounds, Dullers |
| 2 | Pipe Dreams | Túneis de manutenção | 45-60 min | Smilers, Partygoers |
| 3 | Electrical Station | Subestação elétrica | 60 min | Wretches |
| 4 | Abandoned Office | Escritório anos 90 | 15 min | Zona segura |
| 5 | Terror Hotel | Hotel vintage 1920s | 60-90 min | Partygoers, Arachnids |
| 6 | Lights Out | Escuridão total | 45 min | Todas + sombras |
| 7 | Thalassophobia | Piscina abandonada | 30 min | The Beast (chefão) |

---

## ⚙️ MECÂNICAS PRINCIPAIS

### Sistema de Movimento
- WASD: Movimento básico
- Shift: Correr (consome stamina)
- Ctrl: Agachar/Rastejar
- Mouse: Olhar ao redor
- Head bob realista e inclinação de câmera

### Sistema de Sanidade
- Diminui com escuridão, entidades e eventos assustadores
- Sanidade baixa causa alucinações e controles invertidos
- Recuperar com luz, áreas seguras e itens calmantes

### Sistema de Inventário
- 8 slots para itens
- Itens equipáveis: lanterna, rádio, mapa
- Arrastar e soltar para organizar

### Sistema de Interação
- Tecla E para interagir com objetos
- Portas trancadas, interruptores, alavancas
- Raycasting para detecção de objetos olhados

---

## 👾 ENTIDADES

### Hounds
- Quadrúpedes humanoides deformados
- Rápidos e agressivos, atacam em grupo
- **Fraqueza:** Luz forte os afasta

### Smilers
- Visíveis apenas no escuro (olhos e sorriso brilhantes)
- Teleportam se olhar diretamente
- **Fraqueza:** Lanterna constante

### Partygoers
- Humanóides com sorriso fixo, vestidos para festa
- Inteligentes, usam distrações e emboscadas
- **Fraqueza:** Evitar contato visual prolongado

### Wretches
- Humanos corrompidos, pele cinza, olhos pretos
- Rápidos, numerosos, atacam em horda
- **Fraqueza:** Barulhos altos os assustam

### The Beast (Chefão - Level 7)
- Criatura aquática gigante com tentáculos
- Persegue jogador na água
- **Fraqueza:** Não pode entrar em terra

---

## 🎯 OBJETIVOS

### Progressão
- Encontrar saída de cada nível
- Coletar notas e gravações para lore
- Sobreviver a encontros com entidades
- Gerenciar recursos (baterias, comida, sanidade)

### Finais
- **Final Ruim:** Morrer em qualquer nível
- **Final Normal:** Escapar do Level 7
- **Final Verdadeiro:** Coletar todos os artefatos secretos

---

## 🔧 CONTROLES (PC)

| Tecla | Ação |
|-------|------|
| W/A/S/D | Movimento |
| Shift | Correr |
| Ctrl | Agachar/Rastejar |
| Espaço | Pular (limitado) |
| Mouse | Olhar ao redor |
| Botão esquerdo | Atacar/Usar item |
| Botão direito | Mirar/Bloquear |
| E | Interagir |
| F | Lanterna |
| I | Inventário |
| M | Mapa |
| ESC | Pausa/Menu |

---

## 📁 ESTRUTURA DO PROJETO

```
BackroomsLostLiminal/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/           # Gerenciadores principais
│   │   ├── Player/         # Controlador do jogador
│   │   ├── Entities/       # Scripts de entidades
│   │   ├── AI/             # Comportamentos de IA
│   │   ├── UI/             # Interface do usuário
│   │   ├── Inventory/      # Sistema de inventário
│   │   ├── Audio/          # Sistemas de áudio
│   │   ├── Levels/         # Gerenciamento de níveis
│   │   ├── Interactables/  # Objetos interagíveis
│   │   └── Systems/        # Sistemas gerais
│   ├── Scenes/             # Cenas do jogo
│   ├── Materials/          # Materiais URP
│   ├── Textures/           # Texturas 4K
│   ├── Models/             # Modelos 3D
│   ├── Audio/              # Áudios (Música, SFX, Voz)
│   ├── Prefabs/            # Prefabs organizados
│   ├── ScriptableObjects/  # Dados configuráveis
│   ├── Animations/         # Animações
│   ├── Lighting/           # Configurações de luz
│   └── PostProcessing/     # Perfis de pós-processamento
├── Docs/                   # Documentação
└── README.md
```

---

## 🚀 REQUISITOS TÉCNICOS

### Mínimos
- **SO:** Windows 10 64-bit
- **Processador:** Intel Core i5-8400 / AMD Ryzen 5 2600
- **Memória:** 16 GB RAM
- **Placa de vídeo:** GTX 1060 6GB / RX 580 8GB
- **DirectX:** Versão 12
- **Armazenamento:** 50 GB disponível

### Recomendados
- **SO:** Windows 11 64-bit
- **Processador:** Intel Core i7-12700K / AMD Ryzen 7 5800X
- **Memória:** 32 GB RAM
- **Placa de vídeo:** RTX 3070 / RX 6800 XT
- **DirectX:** Versão 12
- **Armazenamento:** SSD NVMe 50 GB

---

## 🎵 TRILHA SONORA

### Música
- Ambiente: Drones graves, tons dissonantes, silêncio proposital
- Tensão: Cordas agudas, batidas cardíacas, ruídos industriais
- Entidades: Sons distorcidos, gritos abafados, risadas
- Chefão: Orquestra caótica, metais, percussão pesada

### Efeitos Sonoros
- Luzes fluorescentes: Zumbido constante (40-60Hz)
- Passos: Diferentes por superfície (carpete, concreto, água, metal)
- Portas: Rangidos, batidas, trancas
- Entidades: Respiração, rosnados, arrastar de pés, ossos estalando

---

## 💡 INSPIRAÇÕES

- **Jogos:** The Backrooms (Pie on Earth), Escape the Backrooms, Lethal Company, Outlast, Amnesia, P.T.
- **Estética:** Fotografias de espaços liminais, arquitetura brutalista, escritórios abandonados
- **Horror:** Tensão psicológica > jump scares, atmosfera > ação

---

## 📝 STATUS DO DESENVOLVIMENTO

### Fase 1: Protótipo ✅
- [x] Estrutura do projeto criada
- [ ] Player controller funcional
- [ ] Level 0 básico (geometria placeholder)
- [ ] Sistema de lanterna
- [ ] IA básica de entidade

### Fase 2: Alpha
- [ ] Level 0-2 completos
- [ ] Mecânicas principais implementadas
- [ ] Sistema de inventário e save
- [ ] 3-4 entidades diferentes
- [ ] Trilha sonora e SFX básicos

### Fase 3: Beta
- [ ] Todos os 7 níveis jogáveis
- [ ] Lore completa espalhada
- [ ] Otimização de performance
- [ ] Polimento de UI e menus
- [ ] Testes de bug e balanceamento

### Fase 4: Lançamento
- [ ] Finalização de assets
- [ ] Trailer e marketing
- [ ] Lançamento na Steam
- [ ] Suporte pós-lançamento

---

## 📄 LICENÇA

Este projeto é um jogo independente desenvolvido para fins educacionais e de entretenimento.

O universo das Backrooms é baseado em creepypasta de domínio público.

---

*"Nas Backrooms, a solidão é o maior inimigo... mas não é o único."*

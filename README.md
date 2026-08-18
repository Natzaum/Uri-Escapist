# Uri-Escapist 🎓⚔️

Um jogo em Unity 3D no estilo **dark fantasy acadêmico**, onde o jogador enfrenta os desafios da vida universitária para conquistar o diploma de **Ciência da Computação** na URI Erechim – Campus II.  
Cada fase representa uma disciplina e cada professor se torna um chefe de fase, com batalhas que testam tanto agilidade quanto conhecimento.

O projeto inclui um painel em **PHP + MySQL** para professores cadastrarem perguntas. A Unity busca as questões publicadas e as distribui entre os livros da fase. Veja a [instalação do painel web](web/README.md).

Para iniciar PHP, MySQL e phpMyAdmin juntos, siga o [guia de execução com Docker](DOCKER.md).

---

## 🎮 Visão Geral

- O jogador inicia nos **portões da URI Erechim – Campus II**.
- A primeira luta serve como **tutorial**.
- O **Prédio 8** funciona como base durante toda a aventura.
- Cada fase representa uma **disciplina específica**:
  - O professor da disciplina é o **adversário**.
  - O combate acontece por meio de **perguntas e respostas**.
  - **Respostas erradas** deixam o professor mais rápido e agressivo.
  - Há um **tempo limite** para cada pergunta.

---

## 🛠️ Mecânicas Principais

- **Sistema de Perguntas**  
  Perguntas de múltipla escolha com timer.  
  Feedback imediato de acerto ou erro.

- **Professores Inimigos**  
  Cada erro aumenta atributos de velocidade e agressividade.  
  Podem sofrer **debuffs** aplicados por itens.

- **Itens Espalhados pelo Mapa**  
  - Dicas (facilitam perguntas).  
  - Debuffs (reduzem velocidade, precisão ou ações do professor).  

- **Stress & Alucinações**  
  A cada fase superada, o estresse aumenta, trazendo efeitos visuais e sonoros que tornam a jornada mais intensa e imprevisível.

---

## 🏗️ Estrutura de Desenvolvimento

- **Engine**: Unity 2022+  
- **Linguagem**: C#  
- **Arquitetura**: ScriptableObjects para dados (perguntas, itens, fases).  
- **IA**: NavMesh + parâmetros escaláveis (velocidade, agressividade).  
- **UI**: Unity Canvas para HUD, menus e sistema de perguntas.  

---

## 👥 Equipe

- **Dev Líder (Unity/Programação)**: implementação da jogabilidade, IA, integração dos assets.  
- **Conteúdo (Banco de Perguntas)**: criação e organização das questões de cada disciplina.  
- **Arte (Design Visual/Assets)**: seleção de modelos, ícones, imagens e recursos visuais.  
- **Narrativa & Documentação**: enredo, roteiro de apresentação, escolha de trilha sonora e relatórios.

---

## 📅 Cronograma (4 Meses)

- **Mês 1**: protótipo jogável (player, professor, UI de perguntas, 1 fase).  
- **Mês 2**: sistema de itens + balanceamento da primeira fase.  
- **Mês 3**: novas fases (disciplinas), implementação de alucinações.  
- **Mês 4**: polimento, trilha sonora, testes e documentação final.  

---

## 🚀 Como Jogar (futuro)

1. Baixe a última versão em [Releases](#).  
2. Abra o jogo e inicie sua jornada nos portões da URI.  
3. Enfrente professores respondendo perguntas antes que o tempo acabe.  
4. Use itens estrategicamente para sobreviver.  
5. Supere todas as disciplinas e conquiste o diploma final!

---

## 📄 Licença

Projeto acadêmico, desenvolvido para fins de estudo.  
Todos os direitos reservados © 2025 — Equipe Uri-Escapist.
Projeto acadêmico, desenvolvido para fins de estudo.  
Todos os direitos reservados © 2025 — Equipe Uri-Escapist.

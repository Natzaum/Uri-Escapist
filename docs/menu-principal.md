# Menu principal

Abra a cena `Assets/Scenes/UriMenu.unity` e pressione **Play**. O script `MenuPrincipal`, já presente na cena, monta o Canvas automaticamente. O menu antigo é ocultado durante a execução.

O visual usa fundo preto, título serifado em azul URI, emblema original de livro e arco desenhado por código, brilho na opção selecionada e transições suaves. O Canvas se adapta à resolução mantendo a composição central.

- **Nova partida:** mantém a dificuldade escolhida e carrega `MainScene`.
- **Dificuldade:** Fácil, Normal ou Difícil, com descrição das regras dos dois andares atuais.
- **Configurações:** volume geral e multiplicador de sensibilidade do mouse.
- **Créditos:** identificação do projeto e atribuição da fonte.
- **Sair:** confirmação antes de fechar o jogo; no Editor, encerra o Play Mode.

Use mouse ou setas para selecionar e Enter para confirmar. Escape retorna dos submenus.

O layout, os textos e as cores ficam em `Assets/AssetsNata/Scripts/MenuPrincipal.cs`. O áudio continua em `MenuAmbientAudio.cs`. A interface não depende de imagens externas ou de uma conexão com a API para abrir.

## Fonte

Cinzel, The Cinzel Project Authors, distribuída sob SIL Open Font License 1.1. A fonte em `Assets/Resources/Fonts/Cinzel-Regular.ttf` é uma instância estática de peso 400 da fonte variável publicada no repositório Google Fonts:

https://github.com/google/fonts/tree/main/ofl/cinzel

A licença completa está em `Assets/Resources/Fonts/Cinzel-OFL.txt`.

## Telas da partida

Todas as telas são criadas automaticamente por `GameInterface`. Não é necessário adicionar objetos ou ligar botões manualmente nas cenas.

- **Pausa:** Esc abre a pausa, com continuar, configurações, reiniciar o andar e voltar ao menu. Reinício e retorno pedem confirmação porque descartam o progresso do andar. Esc cancela a confirmação ou retoma a partida.
- **Configurações:** disponíveis no menu principal e na pausa. Volume de 0 a 100% e sensibilidade de 0,2× a 3×, aplicados imediatamente. As preferências são salvas ao voltar ou trocar de cena e reutilizadas nas próximas sessões. O multiplicador respeita a sensibilidade base configurada no Inspector.
- **Livros:** enunciado e quatro alternativas, com navegação por mouse ou teclado. O resultado mostra se houve acerto ou erro e a resposta correta, até selecionar Continuar. No fácil, pergunta e resultado pausam o mundo. No normal e difícil, o mundo continua ativo. No fácil e normal, Esc ou Fechar livro guardam a pergunta sem pontuar; E perto do livro permite reabri-la. No difícil não é possível fechar nem pausar enquanto a pergunta está aberta.
- **Derrota:** abre ao ser capturado, esgotar o tempo conforme a sequência existente ou atingir o limite de erros. Permite tentar novamente no mesmo andar e dificuldade ou voltar ao menu.
- **Vitória:** usa a porta final e os requisitos existentes, com jogar novamente desde `MainScene` ou voltar ao menu. Uma nova partida limpa o registro de conclusão dos andares.
- **Carregamento:** aparece nas trocas de cena e ao buscar perguntas. Mostra progresso quando disponível e um indicador animado quando o tamanho da resposta ainda é desconhecido. Falhas da API exibem a mensagem e permitem tentar novamente ou voltar ao menu. Não há perguntas locais de outra dificuldade como substitutas.

Pausa, configurações, confirmação, carregamento e fim de partida congelam o mundo. Um único controlador mantém o cursor liberado nessas telas, evitando que um clique seja capturado pelo movimento do jogador.

### Código

- `GameInterface.cs`: telas, navegação, estados, progresso e transições.
- `GamePreferences.cs`: preferências de áudio e sensibilidade.
- `QuizManager.cs`: respostas, pontuação e integração com os livros.
- `GameOver.cs` e `DoorGameEnd.cs`: abertura das telas de derrota e vitória.
- `BookManager.cs` e `RemoteQuestionLoader.cs`: carregamento e falhas das perguntas.

Validação realizada: compilação dos scripts do projeto; execução isolada na Unity com pontuação, bloqueio de resposta duplicada, pausa e retorno ao resultado, aplicação das configurações, falha e nova tentativa, telas de fim de partida, troca assíncrona de cena e textos no limite de 500/255 caracteres. A IA foi substituída por um componente mínimo nessa execução isolada; a perseguição deve ser conferida nas cenas completas.


## HUD e progressão

`GameHud.cs` cria cartões em azul URI para livros, erros e fôlego, além do tempo. A barra de fôlego substitui os textos de debug de stamina/sprint/exaustão. A apresentação antiga é ocultada automaticamente.

| Dificuldade | Acertos por andar | Erro que causa derrota |
| --- | --- | --- |
| Fácil | 5 | 5º |
| Normal | 7 | 3º |
| Difícil | 9 | 1º |

Os limites vêm de `GameDifficulty.cs` e substituem os valores antigos do Inspector. Acertos e erros reiniciam a cada carregamento do andar. A distribuição de dificuldade das perguntas continua como antes.

O HUD é compacto e não exibe objetivo fixo na parte inferior. Após cada acerto ou erro que não encerre a partida, um aviso de progresso aparece por 4 segundos ao voltar ao jogo. Ao atingir a meta, esse aviso indica a porta para o próximo andar. Após concluir o segundo, a passagem retorna ao primeiro e a porta principal permite vencer. Apenas visitar o segundo andar não libera a vitória. O terceiro andar não é necessário neste fluxo.

Para testar, abra `UriMenu`, escolha uma dificuldade e inicie uma nova partida. Confira a porta antes e depois da meta, a derrota exatamente no limite e o retorno à saída após concluir o segundo andar. O HUD é criado automaticamente, sem configuração manual de Canvas.


### Comportamento por dificuldade

| Modo | Velocidade base | Aumento por acerto/erro | Inimigo | Leitura |
| --- | --- | --- | --- | --- |
| Fácil | 0,65× | 0,25× | Patrulha; persegue quando detecta e volta a procurar ao perder visão | Pausa; pode fechar sem consumir a pergunta |
| Normal | 1× | 1× | Segue pelo NavMesh devagar sem detecção, rápido com detecção | Continua; pode fechar sem consumir a pergunta |
| Difícil | 1,5× | 2,5× | Segue sempre em velocidade de perseguição | Continua; precisa responder para sair |

Multiplicadores em `GameDifficulty.cs`. A base usa as velocidades do `EnemyAI` e a escala configurada na cena. Acertos, erros e bônus são cumulativos; os erros não mudam o tipo de perseguição. A detecção exige alcance e visão sem obstáculos. Cenas sem pontos de patrulha usam destinos aleatórios alcançáveis no NavMesh, sem consultar a posição do jogador.


### Tontura ao esgotar o fôlego

Ao chegar a zero de stamina, a câmera começa a oscilar suavemente (inclinação, movimento lateral e pulsação de campo de visão). A separação de cores aumenta no filtro VHS existente. O efeito perde intensidade conforme o fôlego se recupera, não dispara apenas por estar com pouca stamina e não altera a orientação usada no movimento. Livros, menus, perda de foco e desativação da câmera removem os deslocamentos visuais.

`PlayerCam.exhaustionEffectStrength` controla a intensidade de 0 a 1 no Inspector; 0 desativa a tontura. O efeito cromático respeita a ativação do filtro VHS da câmera.

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
- **Livros:** enunciado e quatro alternativas, com navegação por mouse ou teclado. O resultado mostra se houve acerto ou erro e a resposta correta, até selecionar Continuar. O tempo e o inimigo continuam ativos durante pergunta e resultado, como no fluxo anterior. Esc permite pausar e depois retornar à mesma tela.
- **Derrota:** abre ao ser capturado, esgotar o tempo conforme a sequência existente ou exceder o limite de erros. Permite tentar novamente no mesmo andar e dificuldade ou voltar ao menu.
- **Vitória:** usa a porta final e os requisitos existentes, com jogar novamente desde `MainScene` ou voltar ao menu. Uma nova partida limpa o registro de visita ao segundo andar.
- **Carregamento:** aparece nas trocas de cena e ao buscar perguntas. Mostra progresso quando disponível e um indicador animado quando o tamanho da resposta ainda é desconhecido. Falhas da API exibem a mensagem e permitem tentar novamente ou voltar ao menu. Não há perguntas locais de outra dificuldade como substitutas.

Pausa, configurações, confirmação, carregamento e fim de partida congelam o mundo. Um único controlador mantém o cursor liberado nessas telas, evitando que um clique seja capturado pelo movimento do jogador.

### Código

- `GameInterface.cs`: telas, navegação, estados, progresso e transições.
- `GamePreferences.cs`: preferências de áudio e sensibilidade.
- `QuizManager.cs`: respostas, pontuação e integração com os livros.
- `GameOver.cs` e `DoorGameEnd.cs`: abertura das telas de derrota e vitória.
- `BookManager.cs` e `RemoteQuestionLoader.cs`: carregamento e falhas das perguntas.

Validação realizada: compilação dos scripts do projeto; execução isolada na Unity com pontuação, bloqueio de resposta duplicada, pausa e retorno ao resultado, aplicação das configurações, falha e nova tentativa, telas de fim de partida, troca assíncrona de cena e textos no limite de 500/255 caracteres. A IA foi substituída por um componente mínimo nessa execução isolada; a perseguição deve ser conferida nas cenas completas.

# Banco inicial de Ciência da Computação

O arquivo [computer_science_questions.json](computer_science_questions.json) contém **90 perguntas**, com **30 por dificuldade**. Para revisar os enunciados, as alternativas e os gabaritos, abra [PERGUNTAS.md](PERGUNTAS.md).

Cada dificuldade tem três perguntas de cada área:

- Programação;
- Algoritmos;
- Estruturas de Dados;
- Banco de Dados;
- Redes de Computadores;
- Sistemas Operacionais;
- Arquitetura de Computadores;
- Segurança da Informação;
- Engenharia de Software;
- Fundamentos da Computação.

As fáceis priorizam conceitos básicos. As médias incluem aplicação, interpretação e cálculos simples. As difíceis incluem análise de cenários, concorrência, complexidade, normalização, redes, memória e teoria da computação. A classificação é inicial e pode ser ajustada pelo professor.

## Importar no Docker

Na máquina que executa o Docker, atualize os arquivos do projeto e execute na raiz:

```powershell
docker compose up -d --build app
docker compose exec app php scripts/seed_questions.php
```

O comando usa a conta existente indicada por `INITIAL_TEACHER_EMAIL`, definida no ambiente do container. Para escolher outra conta:

```powershell
docker compose exec app php scripts/seed_questions.php --email=seu-email@exemplo.com
```

Sem essa variável ou o argumento, a importação usa a única conta ativa, se houver exatamente uma. Se não for possível identificar uma conta, o comando encerra sem importar.

As disciplinas ausentes são criadas, as perguntas novas ficam **publicadas** e o campo de andar fica vazio: o jogo escolhe conforme a dificuldade da partida. Não há exclusão de dados ou alteração de credenciais.

A importação ignora perguntas com o mesmo enunciado e professor. Assim, executá-la novamente não duplica o banco nem sobrescreve alterações nas alternativas, dificuldade ou publicação. Se um enunciado for alterado no painel, uma nova importação poderá recriar sua versão original.

Uma disciplina existente e inativa interrompe a importação; ative-a no painel e tente novamente. Toda a carga é feita em uma transação, para evitar importações parciais.

## Validar sem gravar

```powershell
docker compose exec app php scripts/seed_questions.php --validate-only
```

Fora do Docker, da raiz do projeto:

```powershell
php web/scripts/seed_questions.php --validate-only
php web/tests/question_seed_test.php
```

O teste usa SQLite em memória e precisa de `pdo_sqlite`. Ele confere a distribuição, a importação, o mapeamento dos gabaritos, a repetição do comando, a preservação de edições e a reversão de uma carga interrompida. Não acessa o banco do servidor.

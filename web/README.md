# Painel de perguntas — URI Escapist

Aplicação em PHP e MySQL para o professor organizar questões de múltipla escolha. A API entrega as perguntas publicadas para os livros existentes nas cenas da Unity.

## Instalação recomendada com Docker

O ambiente completo com PHP, Apache, MySQL e phpMyAdmin está definido na raiz do repositório. Consulte o [guia Docker](../DOCKER.md) ou execute:

```powershell
Copy-Item .env.example .env
docker compose up -d --build
```

O painel ficará em `http://127.0.0.1:8000` e o phpMyAdmin em `http://127.0.0.1:8081`. A conta inicial do professor é criada com os valores definidos no `.env`.

Para acessar de outro computador na mesma rede, execute `scripts/habilitar-acesso-rede.ps1` na raiz do projeto usando um PowerShell como Administrador e utilize o IP exibido pelo script. As instruções completas estão no [guia Docker](../DOCKER.md#acessar-por-outro-computador-na-mesma-rede).

## Requisitos

- PHP 8.1 ou mais recente, com `pdo_mysql` e `mbstring`;
- MySQL 8 ou MariaDB 10.4+;
- Apache, Nginx ou o servidor embutido do PHP.

## Instalação local sem Docker

Execute os comandos a partir da pasta `web`.

1. Crie as tabelas:

   ```powershell
   Get-Content -Raw database/schema.sql | mysql -u root -p
   ```

   No XAMPP, o mesmo arquivo pode ser importado pelo phpMyAdmin.

2. Copie `config/local.example.php` para `config/local.php` e ajuste usuário, senha e nome do banco. O arquivo local contém credenciais e não é versionado.

3. Crie a primeira conta de professor:

   ```powershell
   php scripts/create_teacher.php --name="Professor" --email="professor@uri.edu.br" --password="troque-esta-senha"
   ```

4. Inicie o painel:

   ```powershell
   php -S 127.0.0.1:8000 -t public
   ```

5. Acesse `http://127.0.0.1:8000/login.php`.

Para produção, configure o *document root* do servidor em `web/public`, use HTTPS e uma senha exclusiva para o banco.

## Fluxo professor → jogo

Para começar com conteúdo pronto, o projeto inclui [90 perguntas de Ciência da Computação](database/seeds/README.md): 30 fáceis, 30 médias e 30 difíceis, com importação pelo comando `docker compose exec app php scripts/seed_questions.php` após atualizar a imagem da aplicação. A carga cria as disciplinas e publica as perguntas na conta do professor.

1. O professor entra no painel.
2. Cria ou seleciona uma disciplina, por exemplo `Computação Gráfica`.
3. Cadastra a questão escolhendo **Fácil**, **Média** ou **Difícil** e seleciona **Publicada**.
4. No menu do jogo, o jogador escolhe **Fácil**, **Normal** ou **Difícil**.
5. A Unity envia o modo escolhido e a cena atual. A API sorteia questões de disciplinas ativas, e o jogo as distribui aleatoriamente entre todos os livros.

| Modo da partida | Andar 1 | Andar 2 | Andar 3 (futuro) |
| --- | --- | --- | --- |
| Fácil | Fáceis | Fáceis + médias | Médias |
| Normal | Fáceis | Médias | Difíceis |
| Difícil | Difíceis | Difíceis | Difíceis |

O terceiro andar ainda não existe no jogo e não foi habilitado. Sua regra está preparada para quando a cena e o cadastro ativo com slug `andar-3` forem adicionados.

No modo fácil, o segundo andar procura equilibrar as duas dificuldades e usa mais questões da categoria com conteúdo disponível se necessário, sempre incluindo ambas quando há pelo menos dois livros. As perguntas não se repetem dentro do mesmo andar; podem reaparecer em outro andar ou em uma nova tentativa.

Não há mais escolha de andar no cadastro. Associações antigas são ignoradas na seleção, sem apagar as perguntas existentes. O modo permanece selecionado entre andares e tentativas, e pode ser alterado ao voltar ao menu. O modo Pesadelo, se usado, continua independente da dificuldade das perguntas.

A URL padrão do `BookManager` é:

```text
http://127.0.0.1:8000/api/v1/questions.php
```

As cenas `andar1` e `andar2` estão configuradas para `http://192.168.0.10:8000/api/v1/questions.php`. Se o IP do servidor mudar, altere **Questions Api Url** no Inspector em ambos os andares para uma URL acessível pela máquina que executa o jogo. `localhost` sempre representa a própria máquina do jogador.

O projeto está configurado para aceitar HTTP durante o desenvolvimento local. Em uma publicação real, hospede o painel com HTTPS e troque a URL no Inspector antes de gerar o jogo.

## API

### Buscar perguntas pela cena da Unity

```http
GET /api/v1/questions.php?scene=andar1&mode=normal&limit=10&random=1
```

Mapeamento inicial:

- `andar1` → Andar 1;
- `andar2` → Andar 2.

O parâmetro `limit` é preenchido automaticamente com a quantidade de livros ativos da cena.

### Buscar por disciplina — compatibilidade

```http
GET /api/v1/questions.php?discipline=geral&limit=10&random=1
```

Parâmetros:

- `discipline`: chave da disciplina; padrão `geral`;
- `scene`: nome exato da cena Unity; quando informado, possui prioridade sobre `discipline`;
- `mode`: `facil`, `normal` (padrão) ou `dificil`; aplica a matriz quando `scene` é informado. A consulta legada por disciplina não aplica a progressão por andar;
- `limit`: quantidade entre 1 e 50; padrão 10;
- `random`: use `1` para ordem aleatória ou `0` para ordenar por ID.

Exemplo de resposta:

```json
{
  "success": true,
  "data": [
    {
      "id": 12,
      "discipline": "geral",
      "disciplineName": "Geral",
      "prompt": "Qual estrutura segue a regra LIFO?",
      "options": ["Fila", "Pilha", "Árvore", "Grafo"],
      "correctIndex": 1,
      "difficulty": "facil"
    }
  ],
  "meta": {
    "mode": "scene",
    "gameMode": "normal",
    "scene": "andar1",
    "difficulties": ["facil"],
    "count": 1,
    "generatedAt": "2026-08-18T20:00:00-03:00"
  }
}
```

### Verificar serviço

```http
GET /api/v1/health.php
```

O código HTTP é `200` quando o banco responde e `503` quando está indisponível.

## Carregamento e quantidade de perguntas

- O jogo pausa o cronômetro, o movimento e a interação com os livros enquanto carrega.
- Todos os livros precisam receber uma questão válida e distinta. Perguntas locais sem classificação não são usadas como substitutas.
- Se a API falhar ou faltar conteúdo publicado, o jogo apresenta a mensagem e as opções **Tentar novamente** e **Voltar ao menu**.
- A API retorna HTTP `409` quando não há perguntas suficientes para preencher os livros ou falta uma das categorias do andar misto; retorna `422` para modo ou cena inválidos/inativos.
- Para andares com 10 livros, publique pelo menos 10 perguntas de cada dificuldade para permitir todos os modos. No andar misto, são necessárias 10 no total, incluindo fáceis e médias.
- Rascunhos e questões de disciplinas inativas nunca são enviados ao jogo.
- Atualize o servidor e o jogo juntos: o cliente verifica os metadados do modo para detectar uma API antiga.

## Atualizar o Docker na outra máquina

Copie as alterações do projeto para a máquina do servidor (ou atualize o checkout) e execute na raiz:

```powershell
docker compose up -d --build app
```

Isso recria a aplicação preservando o volume do banco. Esta mudança não exige migração de dados. Não é necessário recadastrar as perguntas existentes: confira suas dificuldades e publique conteúdo suficiente.

Depois, execute o jogo atualizado pela cena `UriMenu`, escolha uma dificuldade e entre no primeiro andar. Abrir um andar diretamente no Editor usa Normal inicialmente.

## Validação

```powershell
php web/tests/api_test.php
```

Execute da raiz do repositório, com PHP e `pdo_sqlite`. Os testes verificam a matriz dos três andares, sorteio sem repetição, mistura, falta de conteúdo e o endpoint real com um banco SQLite isolado. Não acessam nem alteram o MySQL do servidor.

## Atualizar um banco existente

No Docker, as migrações são executadas automaticamente quando o container `app` inicia. Sem Docker, execute:

```powershell
php scripts/migrate.php
```

## Estrutura

```text
web/
├── config/       configuração da aplicação e exemplo local
├── database/     esquema MySQL
├── public/       raiz pública, painel, API, CSS e JavaScript
├── scripts/      criação da conta inicial
├── src/          autenticação, banco e funções compartilhadas
└── views/        telas do painel
```

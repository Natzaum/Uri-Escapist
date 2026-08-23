# Painel de perguntas — URI Escapist

Aplicação em PHP e MySQL para o professor organizar questões de múltipla escolha. A API entrega as perguntas publicadas para os livros existentes nas cenas da Unity.

## Instalação recomendada com Docker

O ambiente completo com PHP, Apache, MySQL e phpMyAdmin está definido na raiz do repositório. Consulte o [guia Docker](../DOCKER.md) ou execute:

```powershell
Copy-Item .env.example .env
docker compose up -d --build
```

O painel ficará em `http://127.0.0.1:8000` e o phpMyAdmin em `http://127.0.0.1:8081`. A conta inicial do professor é criada com os valores definidos no `.env`.

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

1. O professor entra no painel.
2. Cria ou seleciona uma disciplina, por exemplo `Computação Gráfica`.
3. Cadastra a questão escolhendo também **Andar 1** ou **Andar 2** e seleciona **Publicada**.
4. Quando a fase inicia, a Unity envia automaticamente o nome da cena atual.
5. A API identifica o andar e distribui somente as questões destinadas a ele entre os componentes `BookQuiz` da cena.

O `BookManager` já vem configurado para o servidor local:

```text
http://127.0.0.1:8000/api/v1/questions.php
```

Em outro computador ou servidor, altere **Questions Api Url** no Inspector para uma URL acessível pela máquina que executa o jogo. `localhost` sempre representa a própria máquina do jogador.

O projeto está configurado para aceitar HTTP durante o desenvolvimento local. Em uma publicação real, hospede o painel com HTTPS e troque a URL no Inspector antes de gerar o jogo.

## API

### Buscar perguntas pela cena da Unity

```http
GET /api/v1/questions.php?scene=andar1&limit=10&random=1
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
    "discipline": "geral",
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

## Comportamento de contingência

- Se a API não responder, os livros mantêm as perguntas preenchidas localmente no projeto Unity.
- Se houver menos perguntas publicadas do que livros, somente os primeiros livros recebem conteúdo remoto; os demais mantêm suas perguntas locais.
- Rascunhos e questões de disciplinas inativas nunca são enviados ao jogo.
- Perguntas antigas sem andar associado continuam disponíveis nos dois andares até serem editadas.

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

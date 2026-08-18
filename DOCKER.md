# Executando o URI Escapist com Docker

O ambiente Docker contém tudo que o painel web precisa:

- **app**: PHP 8.3 com Apache, `pdo_mysql` e `mbstring`;
- **db**: MySQL 8.4 com armazenamento persistente;
- **phpmyadmin**: administração visual do banco;
- criação automática do banco, das tabelas e da disciplina `geral` no primeiro início.

## 1. Preparar o ambiente

Instale e abra o Docker Desktop. No PowerShell, dentro da raiz do projeto, crie seu arquivo de configuração:

```powershell
Copy-Item .env.example .env
notepad .env
```

Troque `MYSQL_PASSWORD`, `MYSQL_ROOT_PASSWORD` e `INITIAL_TEACHER_PASSWORD`. O arquivo `.env` não é versionado.

## 2. Iniciar os containers

```powershell
docker compose up -d --build
```

Confira se os três serviços estão ativos:

```powershell
docker compose ps
```

Na primeira execução, o download das imagens e a compilação da extensão PHP podem levar alguns minutos.

## 3. Entrar com o primeiro professor

O professor informado nas variáveis `INITIAL_TEACHER_NAME`, `INITIAL_TEACHER_EMAIL` e `INITIAL_TEACHER_PASSWORD` é criado automaticamente no primeiro início. O container não altera essa conta quando ela já existe.

Para adicionar outro professor ou redefinir uma senha manualmente, execute:

```powershell
docker compose exec app php scripts/create_teacher.php --name="Professor" --email="professor@uri.edu.br" --password="uma-senha-com-8-ou-mais-caracteres"
```

O comando também permite redefinir a senha caso seja executado novamente com o mesmo e-mail.

## 4. Acessar

| Serviço | Endereço | Credenciais |
|---|---|---|
| Painel do professor | `http://127.0.0.1:8000/login.php` | `INITIAL_TEACHER_EMAIL` e `INITIAL_TEACHER_PASSWORD` do `.env` |
| phpMyAdmin | `http://127.0.0.1:8081` | Usuário e senha do `.env` |
| API para Unity | `http://127.0.0.1:8000/api/v1/questions.php?discipline=geral&limit=10` | Leitura pública |
| Saúde da API | `http://127.0.0.1:8000/api/v1/health.php` | — |

No phpMyAdmin, use o valor de `MYSQL_USER` como usuário e `MYSQL_PASSWORD` como senha. O servidor já está definido internamente como `db`.

A URL padrão configurada no `BookManager` da Unity já aponta para a porta `8000`. Se alterar `APP_PORT` no `.env`, atualize também **Questions Api Url** no Inspector.

## Operação diária

Parar sem excluir os containers:

```powershell
docker compose stop
```

Iniciar novamente:

```powershell
docker compose start
```

Ver os logs:

```powershell
docker compose logs -f app db phpmyadmin
```

Remover os containers mantendo os dados do MySQL:

```powershell
docker compose down
```

## Recriar o banco do zero

> Atenção: o comando abaixo apaga permanentemente professores, disciplinas e perguntas cadastradas.

```powershell
docker compose down -v
docker compose up -d --build
```

O arquivo [`web/database/schema.sql`](web/database/schema.sql) é executado automaticamente somente quando o volume do MySQL está vazio.

## Problemas comuns

### Porta ocupada

Altere as portas no `.env`. Exemplo:

```dotenv
APP_PORT=8001
PHPMYADMIN_PORT=8082
MYSQL_PORT=3308
```

### Conferir o estado do banco

```powershell
docker compose exec db mysqladmin ping -h 127.0.0.1 -u root -p
```

### Recriar apenas a imagem PHP após mudar o código

```powershell
docker compose up -d --build app
```

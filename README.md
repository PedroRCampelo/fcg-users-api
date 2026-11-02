# fcg-users-api

Serviço de usuários e autenticação da plataforma FIAP Cloud Games (FCG). Ele cuida do cadastro, do login e dos papéis (`Usuario` e `Administrador`) e é o **único** serviço que emite o JWT: o Catalog e o Payments só validam o token com a mesma chave.

Faz parte da Fase 2 do Tech Challenge, em que o monólito da Fase 1 foi dividido em quatro microsserviços. A visão geral da arquitetura, os fluxos de evento e as instruções para subir tudo juntos estão no [fcg-orchestration](https://github.com/PedroRCampelo/fcg-orchestration). Este README cobre só o que é específico deste serviço.

## O que o serviço faz

- Cadastra usuários. A senha precisa ter no mínimo 8 caracteres, com letra, número e caractere especial, e é guardada como hash BCrypt.
- Autentica por e-mail e senha e devolve um JWT (claims `sub`, `email`, `name` e `role`, emissor `fcg-users-api`, audiência `fcg`, validade de 60 minutos por padrão).
- Permite que um administrador liste, promova e desative usuários.
- Na primeira inicialização, se ainda não existir nenhum administrador, cria um a partir de `AdminPadrao__Email` e `AdminPadrao__Senha`.
- Publica o `UserCreatedEvent` a cada cadastro, para o Notifications mandar o e-mail de boas-vindas.

Banco: PostgreSQL, database `fcg_users`. As migrações do EF Core são aplicadas sozinhas na subida.

## Eventos

| Direção | Evento | Por onde trafega | Quando |
|---|---|---|---|
| Publica | `UserCreatedEvent` (`UserId`, `Nome`, `Email`) | exchange `FCG.Contracts:UserCreatedEvent`, fila `notifications.user-created` | Depois de um cadastro bem-sucedido |
| Consome | nenhum | | |

O evento é gravado na mesma transação do usuário (outbox transacional do MassTransit) e só depois entregue ao RabbitMQ. Se o broker estiver fora do ar no cadastro, o evento fica no banco e sai quando ele voltar.

Na subida, o Users também declara a fila do Notifications (`notifications.user-created`) e a liga à exchange. Assim um cadastro feito antes de o Notifications existir não se perde: a mensagem espera na fila.

## Endpoints

Rodando com `dotnet run`, o serviço escuta na porta 5101; dentro do container, na 8080. O Swagger está em `/swagger`.

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| POST | `/auth/registrar` | Anônimo | Cadastra um usuário (`nome`, `email`, `senha`). Responde 201, 400 ou 409 (e-mail já cadastrado). |
| POST | `/auth/login` | Anônimo | Autentica (`email`, `senha`) e devolve `{ "token": "..." }`. Responde 200, 400 ou 401. |
| GET | `/usuarios` | Administrador | Lista todos os usuários. |
| PUT | `/usuarios/{id}/promover` | Administrador | Promove o usuário a administrador. 204 ou 404. |
| PUT | `/usuarios/{id}/desativar` | Administrador | Desativa o usuário. 204 ou 404. |
| GET | `/health` | Anônimo | Saúde completa, inclui a conexão com o RabbitMQ. Serve de readiness. |
| GET | `/health/live` | Anônimo | Só confirma que o processo responde, sem dependências. Serve de liveness. |

Erros saem no formato ProblemDetails (RFC 7807).

## Variáveis de ambiente

No .NET, `__` (dois sublinhados) separa os níveis da configuração: `RabbitMq__Host` é a chave `RabbitMq:Host` do `appsettings.json`.

| Variável | Descrição | Obrigatória | Padrão |
|---|---|---|---|
| `ConnectionStrings__Default` | Connection string do PostgreSQL (database `fcg_users`) | Sim¹ | Só em Development: `Host=localhost;Port=5435;Database=fcg_users;Username=fcg;Password=fcg_password` |
| `JWT__SIGNINGKEY` | Chave de assinatura do JWT, mínimo de 32 bytes. Deve ser a mesma do Catalog e do Payments | Sim | Nenhum, de propósito. Sem ela o serviço não sobe |
| `Jwt__Issuer` | Emissor do token | Não | `fcg-users-api` |
| `Jwt__Audience` | Audiência do token | Não | `fcg` |
| `Jwt__ExpirationMinutes` | Validade do token em minutos | Não | `60` |
| `RabbitMq__Host` | Host do RabbitMQ | Não | `localhost` |
| `RabbitMq__Port` | Porta AMQP | Não | `5672` |
| `RabbitMq__VirtualHost` | Virtual host | Não | `/` |
| `RabbitMq__Username` | Usuário do RabbitMQ | Sim¹ | Só em Development: `fcg` |
| `RabbitMq__Password` | Senha do RabbitMQ | Sim¹ | Só em Development: `fcg_password` |
| `RabbitMq__Queues__NotificationsUserCreated` | Fila do Notifications para o `UserCreatedEvent`. Precisa ser igual à do Notifications | Não | `notifications.user-created` |
| `AdminPadrao__Email` | E-mail do administrador criado quando não existe nenhum | Não | `admin@fcg.com` |
| `AdminPadrao__Senha` | Senha desse administrador | Não | `Admin123!` (troque fora do desenvolvimento) |
| `ASPNETCORE_ENVIRONMENT` | Em `Development` o serviço lê o `appsettings.Development.json` | Não | `Development` no `dotnet run`, `Production` na imagem |

¹ Obrigatória em qualquer ambiente que não seja o Development. Rodando com `dotnet run`, o `appsettings.Development.json` já preenche com os valores do PostgreSQL e do RabbitMQ do compose.

Se faltar algo obrigatório, o serviço escreve o motivo no log e encerra com código de saída 1.

## Como rodar isolado

Pré-requisitos: .NET 8 SDK e Docker.

O Users precisa de um PostgreSQL e de um RabbitMQ. O jeito mais simples é subir só os dois com o compose do orquestrador (o [fcg-orchestration](https://github.com/PedroRCampelo/fcg-orchestration) deve estar clonado ao lado deste repositório):

```bash
cd ../fcg-orchestration
cp .env.example .env              # só na primeira vez; preencha JWT__SIGNINGKEY (openssl rand -hex 32)
docker compose up -d rabbitmq postgres
cd ../fcg-users-api
```

Depois, com o .NET:

```bash
export JWT__SIGNINGKEY=$(openssl rand -hex 32)
dotnet run --project src/FCG.Users.Api
```

O serviço sobe em http://localhost:5101/swagger. No primeiro start ele aplica as migrações e cria o administrador padrão (`admin@fcg.com` / `Admin123!`). Para testar o login:

```bash
curl -s -X POST http://localhost:5101/auth/login \
  -H 'content-type: application/json' \
  -d '{"email":"admin@fcg.com","senha":"Admin123!"}'
```

Ou com a imagem Docker, sem precisar do SDK:

```bash
docker build -t fcg-users-api:local .
docker run --rm -p 5101:8080 \
  -e JWT__SIGNINGKEY="$(openssl rand -hex 32)" \
  -e ConnectionStrings__Default="Host=host.docker.internal;Port=5435;Database=fcg_users;Username=fcg;Password=fcg_password" \
  -e RabbitMq__Host=host.docker.internal \
  -e RabbitMq__Username=fcg -e RabbitMq__Password=fcg_password \
  fcg-users-api:local
```

`host.docker.internal` é o nome que o Docker Desktop dá à máquina hospedeira, onde o compose publicou o PostgreSQL (porta 5435) e o RabbitMQ (5672).

Sem o Notifications no ar, o Users funciona normalmente: os `UserCreatedEvent` ficam retidos na fila `notifications.user-created` até ele subir.

Para rodar os testes (não precisam de banco nem de broker):

```bash
dotnet test
```

## Estrutura do código

```
src/
  FCG.Users.Domain           entidades, value objects (Email, Senha) e regras de negócio
  FCG.Users.Application      serviços, DTOs, interfaces e os contratos de evento (Contracts/)
  FCG.Users.Infrastructure   EF Core, migrações, MassTransit, JWT e BCrypt
  FCG.Users.Api              endpoints, middleware de exceções, Swagger e Program.cs
tests/
  FCG.Users.Tests            testes de unidade e de consumo/publicação com o harness do MassTransit
k8s/                         Deployment, Service, ConfigMap e Secret deste serviço
Dockerfile                   build multi-stage, compila para arm64 e amd64
```

## Decisões que afetam este serviço

As justificativas completas estão na seção [Decisões de projeto](https://github.com/PedroRCampelo/fcg-orchestration#decisões-de-projeto) do fcg-orchestration. As que mais pesam aqui:

- **Outbox transacional:** o usuário e o `UserCreatedEvent` são gravados juntos, então nunca existe cadastro sem evento nem evento sem cadastro.
- **Contrato de evento duplicado:** a pasta `Contracts/` é uma cópia idêntica à dos outros repositórios que usam o mesmo evento, sem pacote compartilhado.
- **Quem publica declara a fila de quem consome**, para o evento esperar o consumidor em vez de ser descartado.
- **EF Core 9 sobre .NET 8**, por exigência do MassTransit.EntityFrameworkCore 8.5.
- **Credenciais padrão nos manifestos do Kubernetes** (`k8s/secret.yaml`) servem só para o ambiente local.

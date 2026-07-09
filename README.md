# FIAP Cloud Games - CatalogAPI

Microsservico de catalogo da Fase 2 do Tech Challenge FIAP. Ele e responsavel
pelo CRUD de jogos, pela solicitacao de compra e pela biblioteca do usuario.

## Responsabilidades

- Expor consulta publica de jogos.
- Restringir criacao, atualizacao e exclusao logica de jogos ao perfil `Admin`.
- Validar localmente o JWT emitido pela UsersAPI, sem chamada HTTP entre
  microsservicos.
- Publicar `OrderPlacedEvent` no RabbitMQ ao solicitar compra.
- Consumir `PaymentProcessedEvent` e adicionar o jogo a biblioteca somente
  quando o status for `Approved`.
- Expor a biblioteca do usuario autenticado.

## Endpoints

- `GET /api/games`: lista jogos ativos.
- `GET /api/games/{id}`: consulta um jogo ativo.
- `POST /api/games`: cria jogo, exige JWT com role `Admin`.
- `PUT /api/games/{id}`: atualiza jogo ativo, exige JWT com role `Admin`.
- `DELETE /api/games/{id}`: desativa jogo, exige JWT com role `Admin`.
- `GET /api/me/library/games`: lista biblioteca do usuario autenticado.
- `POST /api/me/library/games/{gameId}`: solicita compra do jogo autenticado.
- `GET /health`: readiness com SQL Server e RabbitMQ.
- `GET /health/live`: liveness do processo.

## Variaveis de ambiente

| Nome | Descricao |
|---|---|
| `ConnectionStrings__CatalogDatabase` | Connection string exclusiva da CatalogAPI. |
| `Jwt__Issuer` | Issuer esperado no JWT. Deve bater com a UsersAPI. |
| `Jwt__Audience` | Audience esperada no JWT. Deve bater com a UsersAPI. |
| `Jwt__Key` | Chave simetrica usada para validar o JWT. Deve ser a mesma da UsersAPI. |
| `RabbitMq__Host` | Host ou Service do RabbitMQ. |
| `RabbitMq__Port` | Porta AMQP do RabbitMQ. |
| `RabbitMq__VirtualHost` | Virtual host do RabbitMQ. |
| `RabbitMq__Username` | Usuario do RabbitMQ. |
| `RabbitMq__Password` | Senha do RabbitMQ. |
| `RabbitMq__PaymentProcessedQueue` | Fila do consumidor de resultado de pagamento. |

## Desenvolvimento local

Requisitos:

- SDK .NET 8;
- SQL Server;
- RabbitMQ.

Comandos:

```bash
dotnet restore TechChallenge.Catalog.sln
dotnet test TechChallenge.Catalog.sln --configuration Release --no-restore
dotnet run --project src/FCG.Catalog.Api/FCG.Catalog.Api.csproj
```

`appsettings.Development.json` nao contem segredos. Em desenvolvimento, use
variaveis de ambiente ou `dotnet user-secrets`.

Opcao 1 - variaveis de ambiente:

```bash
export ConnectionStrings__CatalogDatabase='Server=localhost,1433;Database=FcgCatalogDb;User Id=sa;Password=<local-sql-password>;TrustServerCertificate=True'
export Jwt__Key='<same-32-byte-or-longer-key-used-by-users-api>'
export RabbitMq__Username='<rabbitmq-username>'
export RabbitMq__Password='<rabbitmq-password>'
dotnet run --project src/FCG.Catalog.Api/FCG.Catalog.Api.csproj
```

Opcao 2 - user-secrets:

```bash
dotnet user-secrets set 'ConnectionStrings:CatalogDatabase' 'Server=localhost,1433;Database=FcgCatalogDb;User Id=sa;Password=<local-sql-password>;TrustServerCertificate=True' --project src/FCG.Catalog.Api/FCG.Catalog.Api.csproj
dotnet user-secrets set 'Jwt:Key' '<same-32-byte-or-longer-key-used-by-users-api>' --project src/FCG.Catalog.Api/FCG.Catalog.Api.csproj
dotnet user-secrets set 'RabbitMq:Username' '<rabbitmq-username>' --project src/FCG.Catalog.Api/FCG.Catalog.Api.csproj
dotnet user-secrets set 'RabbitMq:Password' '<rabbitmq-password>' --project src/FCG.Catalog.Api/FCG.Catalog.Api.csproj
dotnet run --project src/FCG.Catalog.Api/FCG.Catalog.Api.csproj
```

Para comandos de migration com `dotnet ef`, forneca
`ConnectionStrings__CatalogDatabase` como variavel de ambiente.

## Docker

```bash
docker build -t tech-challenge-2-catalog-api:latest .
docker run --rm -p 8083:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__CatalogDatabase="Server=host.docker.internal,1433;Database=FcgCatalogDb;User Id=sa;Password=<sql-password>;TrustServerCertificate=True" \
  -e Jwt__Issuer=FCG \
  -e Jwt__Audience=FCG \
  -e Jwt__Key="<same-32-byte-or-longer-key-used-by-users-api>" \
  -e RabbitMq__Host=host.docker.internal \
  -e RabbitMq__Port=5672 \
  -e RabbitMq__VirtualHost=/ \
  -e RabbitMq__Username="<rabbitmq-username>" \
  -e RabbitMq__Password="<rabbitmq-password>" \
  -e RabbitMq__PaymentProcessedQueue=catalog-payment-processed \
  tech-challenge-2-catalog-api:latest
```

## Docker Compose integrado

Este repositorio e o local de orquestracao escolhido para a entrega, sem criar
um quinto repositorio. O `docker-compose.yml` referencia os quatro
microsservicos como contextos independentes em diretorios irmaos:

```text
tech-challenge-2-catalog-api/
tech-challenge-2-users-api/
tech-challenge-2-payments-api/
tech-challenge-2-notifications-api/
```

Crie o arquivo local de variaveis:

```bash
cp .env.example .env
```

Preencha os placeholders em `.env`. A chave `TC2_JWT_KEY` deve ter pelo menos
32 bytes e deve ser a mesma usada por UsersAPI e CatalogAPI. O arquivo `.env`
nao deve ser versionado.

Suba toda a aplicacao:

```bash
docker compose up --build -d
```

Servicos expostos por padrao:

| Servico | URL |
|---|---|
| UsersAPI | `http://localhost:18080` |
| CatalogAPI | `http://localhost:18081` |
| PaymentsAPI | `http://localhost:18082` |
| NotificationsAPI | `http://localhost:18083` |
| RabbitMQ AMQP | `localhost:5672` |
| RabbitMQ Management | `http://localhost:15672` |
| SQL Server | `localhost,14333` |

Execute o smoke test do fluxo completo:

```bash
set -a
. ./.env
set +a
./scripts/smoke-test.sh
```

O script valida:

- readiness de UsersAPI e CatalogAPI;
- login do admin inicial;
- criacao de jogo aprovado e jogo rejeitado;
- cadastro de usuario;
- compra aprovada adicionando jogo a biblioteca;
- compra rejeitada sem adicionar jogo a biblioteca.

Para acompanhar os eventos no video:

```bash
docker compose logs -f users-api catalog-api payments-api notifications-api
```

Para limpar containers e dados persistidos:

```bash
docker compose down -v
```

## Kubernetes

Os manifests ficam em `/k8s`, conforme exigido pelo enunciado.

Crie uma copia local do Secret:

```bash
cp k8s/secret.example.yaml k8s/secret.local.yaml
```

Preencha os placeholders e aplique:

```bash
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secret.local.yaml
kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/service.yaml
```

Em Kind, carregue a imagem antes do deploy:

```bash
kind load docker-image tech-challenge-2-catalog-api:latest
```

## Contratos de eventos

Namespace: `FCG.IntegrationEvents.V1`.

`OrderPlacedEvent`:

- `OrderId`
- `OccurredAtUtc`
- `UserId`
- `UserEmail`
- `GameId`
- `Price`

`PaymentProcessedEvent`:

- `OrderId`
- `ProcessedAtUtc`
- `UserId`
- `UserEmail`
- `GameId`
- `Price`
- `Status`: `Approved` ou `Rejected`

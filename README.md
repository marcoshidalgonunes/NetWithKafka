# NetWithKafka

Example of a .NET 8 BFF communicating with Kafka-backed transaction processors and PostgreSQL.

## Services

- `bff`: ASP.NET Core API exposing `POST /api/process`
- `worker`: .NET worker service that consumes transactions, applies batch balance updates, and replies over Kafka
- `backend`: alternate .NET worker service that calls the `process_transaction` PostgreSQL function directly

`worker` and `backend` consume the same Kafka topic/group and should not be used as primary processors at the same time.

## Solution layout

- `bff/Transactions.Bff.csproj`
- `worker/Transactions.Worker.csproj`
- `backend/Transactions.Backend.csproj`
- `NetWithKafka.sln`

## Local development

Build the full solution:

```bash
dotnet build NetWithKafka.sln
```

Run tests:

```bash
dotnet test NetWithKafka.sln
```

Run services individually:

```bash
dotnet run --project bff/Transactions.Bff.csproj
dotnet run --project worker/Transactions.Worker.csproj
dotnet run --project backend/Transactions.Backend.csproj
```

## Docker Compose

Default runtime path uses `bff` + `worker`:

```bash
docker compose up --build
```

To run the alternate `backend` processor instead, stop `worker` first and enable the `backend` profile:

```bash
docker compose stop worker
docker compose --profile backend up --build backend
```

The BFF health endpoint is available at:

```text
http://localhost:8080/actuator/health
```

## PostgreSQL initialization

Schema and procedure scripts are mounted into `/docker-entrypoint-initdb.d` for fresh PostgreSQL volumes.

If you already have an existing `postgresdata` volume and want the initialization scripts to run again, remove it first:

```bash
docker volume rm postgresdata
```

## Notes

- Kafka bootstrap is `localhost:9092` locally and `kafka:29092` in Docker.
- `worker` is the default processing path in Compose.
- `backend` remains available as a separate .NET 8 processor implementation for comparison or alternate execution.

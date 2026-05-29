# NetWithKafka

Example of a .NET 8 solution comprising an API (BFF), a backend processor, and a worker that orchestrates transaction processing through Kafka and PostgreSQL.

## Services

- `bff`: ASP.NET Core API exposing `POST /api/process`
- `worker`: .NET worker service that orchestrates transaction processing — consumes transactions from Kafka and delegates balance processing to `backend`
- `backend`: .NET service responsible for balance processing; reads balances via `get_balance` and applies updates via `update_balance` directly against PostgreSQL; required by `worker`

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

All three services (`bff`, `backend`, and `worker`) are required and run together:

```bash
docker compose up --build
```

`worker` depends on both `bff` and `backend` being healthy before it starts.

The BFF health endpoint is available at:

```text
http://localhost:8081/actuator/health
```

## PostgreSQL initialization

The `postgresdata` volume is declared as external and must be created before starting the stack:

```bash
docker volume create postgresdata
```

The SQL scripts are **not** mounted automatically. After the `postgres` container is running, apply them in order:

```bash
docker exec -i postgres psql -U postgres < postgres/create_balance_table.sql
docker exec -i postgres psql -U postgres < postgres/create_transaction_table.sql
docker exec -i postgres psql -U postgres < postgres/process_transaction_stored_procedure.sql
```

To reset the database, remove and recreate the volume, then re-run the scripts above:

```bash
docker compose down
docker volume rm postgresdata
docker volume create postgresdata
```

## Notes

- Kafka bootstrap is `localhost:9092` locally and `kafka:29092` in Docker.
- `worker` is the default processing path in Compose.
- `backend` remains available as a separate .NET 8 processor implementation for comparison or alternate execution.

# Transactions BFF

## Description

<!-- Describe the BFF (Backend for Frontend) functionality here -->
This service acts as a **Backend for Frontend (BFF)** for transaction processing. It exposes a REST API that receives transaction requests and communicates asynchronously with downstream services via **Apache Kafka**, using a request-reply pattern with correlation IDs to match responses.

**Key components:**
- `TransactionController` — REST endpoint to submit transactions
- `TransactionService` — Publishes messages to Kafka topics
- `KafkaReplyConsumerService` — Background service that consumes Kafka reply messages
- `KafkaCorrelationStore` — Tracks in-flight requests by correlation ID

## Tech Stack

- **.NET 8** (ASP.NET Core Web API)
- **Confluent.Kafka 2.6.1**

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A running **Kafka** broker (see `appsettings.json` for connection config)
- [Docker](https://www.docker.com/) (optional, for containerized run)

---

## Build

```bash
cd bff
dotnet build
```

---

## Run

### Locally

```bash
dotnet run --project Transactions.Bff.csproj
```

### With Development settings

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project Transactions.Bff.csproj
```

### With Docker

```bash
docker build -t transactions-bff .
docker run -p 8080:8080 --env ASPNETCORE_ENVIRONMENT=Docker transactions-bff
```

---

## Configuration

Settings are in `appsettings.json` / `appsettings.{Environment}.json`.  
Key Kafka settings are mapped via `KafkaConfig`:

| Key | Description |
|-----|-------------|
| `Kafka:BootstrapServers` | Kafka broker address |
| `Kafka:RequestTopic` | Topic to publish transaction requests |
| `Kafka:ReplyTopic` | Topic to consume replies from |

---

## Tests

```bash
dotnet test tests/Transactions.Bff.Tests/Transactions.Bff.Tests.csproj
```

To run with detailed output:

```bash
dotnet test tests/Transactions.Bff.Tests/Transactions.Bff.Tests.csproj --logger "console;verbosity=detailed"
```

**Test coverage includes:**
- `KafkaCorrelationStoreTests` — Unit tests for correlation tracking
- `TransactionControllerTests` — Unit tests for the REST controller
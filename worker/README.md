# Transactions Worker

## Description

> _Fill in specific business rules and workflows for your domain here._

This is a **.NET 8 Worker Service** responsible for bridging **Kafka messaging** and the **Transactions Backend API**. It runs as a background service that consumes transaction messages from a Kafka topic, forwards them to the backend REST API via HTTP, and publishes the responses back to a Kafka reply topic.

**Key responsibilities:**
- Consume transaction request messages from a Kafka topic
- Forward requests to the Transactions Backend API via HTTP
- Publish API responses back to the Kafka reply topic

---

## Tech Stack

| Technology | Version | Purpose |
|---|---|---|
| .NET 8 | 8.0 | Worker Service runtime |
| Confluent.Kafka | 2.6.1 | Kafka consumer/producer |
| Microsoft.Extensions.Http | 8.0.1 | HTTP client for backend API calls |
| Microsoft.Extensions.Hosting | 8.0.1 | Background service host |

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A running **Kafka** broker
- A running instance of **Transactions Backend API**
- _(Optional)_ [Docker](https://www.docker.com/)

---

## Configuration

Edit `appsettings.json`:

| Key | Description |
|---|---|
| `Kafka:BootstrapServers` | Kafka broker address (e.g. `localhost:9092`) |
| `Kafka:RequestTopic` | Topic to consume transaction requests from |
| `Kafka:ReplyTopic` | Topic to publish replies to |
| `Kafka:GroupId` | Kafka consumer group ID |
| `Backend:BaseUrl` | Transactions Backend API base URL |

---

## Build

```bash
dotnet build Transactions.Worker.csproj
```

---

## Run

### Locally

```bash
dotnet run --project Transactions.Worker.csproj
```

### With a specific environment

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project Transactions.Worker.csproj
```

### With Docker

```bash
docker build -t transactions-worker .
docker run -e ASPNETCORE_ENVIRONMENT=Docker transactions-worker
```

---

## Tests

```bash
dotnet test tests/Transactions.Worker.Tests/
```

### With detailed output

```bash
dotnet test tests/Transactions.Worker.Tests/ \
  --logger "console;verbosity=detailed"
```
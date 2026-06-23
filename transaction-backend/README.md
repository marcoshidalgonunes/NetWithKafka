# Transactions Backend

## Description

> _Fill in specific business rules and workflows for your domain here._

This is a **.NET 8** Web API responsible for **transaction operations**. It exposes REST endpoints to manage transactions, persisting data in **PostgreSQL** database. Besides that, it consumes transaction messages from a Kafka topic, saving transactions processed to update balances in database.

---

## Tech Stack

| Technology | Version | Purpose |
|---|---|---|
| .NET 8 | 8.0 | Runtime / ASP.NET Core Web API |
| Npgsql | 8.0.4 | PostgreSQL data access |
| Confluent.Kafka | 2.6.1 | Kafka consumer/producer |
| Microsoft.Extensions.Http | 8.0.1 | HTTP client for backend API calls |
| Microsoft.Extensions.Hosting | 8.0.1 | Host/background services |

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A running **PostgreSQL** instance
- A running **Kafka** broker
- _(Optional)_ [Docker](https://www.docker.com/)

---

## Configuration

Edit `appsettings.json`:

| Key | Description |
|---|---|
| `ConnectionStrings:Default` | PostgreSQL connection string |
| `Kafka:BootstrapServers` | Kafka broker address (e.g. `localhost:9092`) |
| `Kafka:RequestTopic` | Topic to consume transaction requests from |
| `Kafka:ReplyTopic` | Topic to publish replies to |
| `Kafka:GroupId` | Kafka consumer group ID |
| `Backend:BaseUrl` | Transactions Backend API base URL |

---

## Build

```bash
dotnet build Transactions.Backend.csproj
```

---

## Run

### Locally

```bash
dotnet run --project Transactions.Backend.csproj
```

### With a specific environment

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project Transactions.Backend.csproj
```

### With Docker

```bash
docker build -t transactions-backend .
docker run -p 8080:8080 transactions-backend
```

---

## Tests

```bash
dotnet test tests/Transactions.Backend.Tests/
```

### With detailed output

```bash
dotnet test tests/Transactions.Backend.Tests/ \
  --logger "console;verbosity=detailed"
```

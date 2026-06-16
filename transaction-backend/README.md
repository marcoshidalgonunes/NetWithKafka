# Transactions Backend

## Description

> _Fill in specific business rules and workflows for your domain here._

This is a **.NET 8** Web API responsible for **account balance operations**. It exposes REST endpoints to manage financial transactions, processing credits, debits, and other balance operations, persisting data in a **PostgreSQL** database via `Npgsql`.

---

## Tech Stack

| Technology | Version | Purpose |
|---|---|---|
| .NET 8 | 8.0 | Runtime / ASP.NET Core Web API |
| Npgsql | 8.0.4 | PostgreSQL data access |
| Microsoft.Extensions.Hosting | 8.0.1 | Host/background services |

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A running **PostgreSQL** instance
- _(Optional)_ [Docker](https://www.docker.com/)

---

## Configuration

Edit `appsettings.json`:

| Key | Description |
|---|---|
| `ConnectionStrings:Default` | PostgreSQL connection string |

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
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Transactions.Worker.csproj ./
RUN dotnet restore Transactions.Worker.csproj

COPY . ./
RUN dotnet publish Transactions.Worker.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

ENV DOTNET_ENVIRONMENT=Docker

ENTRYPOINT ["dotnet", "Transactions.Worker.dll"]
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Transactions.Backend.csproj ./
RUN dotnet restore Transactions.Backend.csproj

COPY . ./
RUN dotnet publish Transactions.Backend.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

ENV DOTNET_ENVIRONMENT=Docker

ENTRYPOINT ["dotnet", "Transactions.Backend.dll"]
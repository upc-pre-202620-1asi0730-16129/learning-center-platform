FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder
WORKDIR /app
COPY Acme.Center.Platform/*.csproj Acme.Center.Platform/
RUN dotnet restore ./Acme.Center.Platform
COPY . .
RUN dotnet publish ./Acme.Center.Platform -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=builder /app/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "Acme.Center.Platform.dll"]
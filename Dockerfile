# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and projects
COPY *.slnx .
COPY src/Balakhare.Core/*.csproj src/Balakhare.Core/
COPY src/Balakhare.Infrastructure/*.csproj src/Balakhare.Infrastructure/
COPY src/Balakhare.Web/*.csproj src/Balakhare.Web/

# Restore dependencies
RUN dotnet restore src/Balakhare.Web/Balakhare.Web.csproj

# Copy everything else and build
COPY . .
RUN dotnet publish src/Balakhare.Web/Balakhare.Web.csproj -c Release -o out

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Create uploads directory
RUN mkdir -p wwwroot/uploads && chmod 777 wwwroot/uploads

EXPOSE 80
ENTRYPOINT ["dotnet", "Balakhare.Web.dll"]

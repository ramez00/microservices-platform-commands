# Microservices Platform & Commands Solution

A .NET 9.0 microservices solution demonstrating inter-service communication patterns including synchronous (HTTP REST, gRPC) and asynchronous (RabbitMQ) messaging.

## 📋 Table of Contents

- [Overview](#-overview)
- [Architecture](#-architecture)
- [Services](#-services)
- [Technology Stack](#-technology-stack)
- [Communication Patterns](#-communication-patterns)
- [Prerequisites](#-prerequisites)
- [Getting Started](#-getting-started)
- [Configuration](#-configuration)
- [API Endpoints](#-api-endpoints)
- [Deployment](#-deployment)
- [Project Structure](#-project-structure)

## 🎯 Overview

This solution consists of two microservices that work together to manage platforms and their associated commands:

- **PlatformService**: Manages platform entities (Name, Cost, Publisher)
- **CommandsService**: Manages commands associated with platforms (HowTo, CommandLine)

The services communicate using multiple patterns:
- **Synchronous**: HTTP REST API and gRPC
- **Asynchronous**: RabbitMQ Message Bus

## 🏗️ Architecture

```
┌─────────────────┐         ┌─────────────────┐
│ PlatformService │────────▶│ CommandsService │
│                 │  HTTP   │                 │
│                 │  gRPC   │                 │
│                 │         │                 │
└────────┬────────┘         └────────┬────────┘
         │                           │
         │                           │
         └───────────┬───────────────┘
                     │
              ┌──────▼──────┐
              │  RabbitMQ   │
              │  MessageBus │
              └─────────────┘
```

### Data Flow

1. **Platform Creation**: When a platform is created in PlatformService:
   - Synchronous HTTP call to CommandsService (fallback)
   - Asynchronous message published to RabbitMQ
   - CommandsService receives message and creates platform locally

2. **Platform Synchronization**: CommandsService can fetch platforms via:
   - gRPC call to PlatformService (preferred)
   - HTTP REST API call (fallback)

## 🔧 Services

### PlatformService

**Purpose**: Manages platform entities

**Features**:
- CRUD operations for platforms
- SQL Server database (Production) / InMemory database (Development)
- Publishes platform events to RabbitMQ
- Exposes gRPC endpoint for platform data
- HTTP REST API endpoints

**Ports**:
- HTTP API: `5279` (Development)
- gRPC: `7196` (Development)

**Database**:
- Development: InMemory Database
- Production: SQL Server

### CommandsService

**Purpose**: Manages commands associated with platforms

**Features**:
- CRUD operations for commands
- InMemory database
- Subscribes to RabbitMQ events for platform updates
- Consumes gRPC from PlatformService
- HTTP REST API endpoints

**Ports**:
- HTTP API: `6000` (Development)

**Database**:
- InMemory Database

## 🛠️ Technology Stack

- **.NET 9.0** - Framework
- **ASP.NET Core** - Web framework
- **Entity Framework Core 9.0.9** - ORM
- **SQL Server** - Production database
- **RabbitMQ** - Message broker
- **gRPC** - High-performance RPC framework
- **AutoMapper** - Object mapping
- **Docker** - Containerization
- **Kubernetes** - Orchestration
- **OpenAPI/Swagger** - API documentation

## 📡 Communication Patterns

### 1. Synchronous Communication

#### HTTP REST API
- PlatformService → CommandsService: HTTP POST when creating platforms
- Standard REST endpoints for CRUD operations

#### gRPC
- CommandsService → PlatformService: gRPC call to fetch all platforms
- Protocol Buffers definition: `Protos/platforms.proto`
- More efficient than HTTP for inter-service communication

### 2. Asynchronous Communication

#### RabbitMQ Message Bus
- **Exchange**: `trigger` (Fanout type)
- **Event Type**: `Platform_Published`
- PlatformService publishes platform creation events
- CommandsService subscribes and processes events asynchronously

## 📦 Prerequisites

To run the two services locally you need:

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or newer
- A RabbitMQ broker on `localhost:5672`, either through [Docker Desktop](https://www.docker.com/products/docker-desktop) or a local [RabbitMQ](https://www.rabbitmq.com/) install

SQL Server is **not** needed locally: in Development both services use an in-memory database.

To deploy to Kubernetes you additionally need:

- [Docker Desktop](https://www.docker.com/products/docker-desktop) with Kubernetes enabled (or another cluster) and `kubectl`
- The [NGINX Ingress Controller](https://kubernetes.github.io/ingress-nginx/deploy/) if you want to use the ingress

## 🚀 Getting Started

### Run locally

The start order matters: **RabbitMQ, then PlatformService, then CommandsService.** CommandsService calls PlatformService over gRPC and connects to RabbitMQ while it starts, and it exits if either is missing.

#### 1. Clone the repository

```bash
git clone https://github.com/ramez00/microservices-platform-commands.git
cd microservices-platform-commands
```

#### 2. Start RabbitMQ

```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

Skip this step if RabbitMQ is already installed and running on your machine. The management UI is at http://localhost:15672 (user `guest`, password `guest`).

#### 3. Start PlatformService

Open a terminal in the repository root:

```bash
dotnet run --project PlatformService
```

Wait for these lines:

```
--> Using InMem Db
--> Seeding Data.....
Now listening on: http://localhost:7196
Now listening on: http://localhost:5279
```

| Endpoint | Address |
|----------|---------|
| REST API | `http://localhost:5279` |
| gRPC     | `http://localhost:7196` |

#### 4. Start CommandsService

Open a **second** terminal in the repository root:

```bash
dotnet run --project CommandsService
```

Wait for these lines:

```
--> Calling GRPC Server http://localhost:7196
--> Seeding new platforms...
==> Connected to RabbitMQ and listening for messages.
Now listening on: http://localhost:6000
```

| Endpoint | Address |
|----------|---------|
| REST API | `http://localhost:6000` |

#### 5. Check that it works

Follow the steps in [Testing](#-testing) to create a platform in PlatformService and watch it arrive in CommandsService.

#### Stopping

Press `Ctrl+C` in each terminal, then stop the broker with `docker stop rabbitmq` (start it again later with `docker start rabbitmq`). All data is in memory, so every restart begins with the four seeded platforms.

### Troubleshooting

| Symptom | Cause and fix |
|---------|---------------|
| CommandsService exits at startup with a `NullReferenceException` | PlatformService or RabbitMQ is not running. Start them first, then start CommandsService again. |
| PlatformService console shows `Could not connect to the Message Bus` and requests return 500 | RabbitMQ is not reachable on `localhost:5672`. Start it and restart PlatformService. |
| `http://localhost:6000` does not open in the browser (`ERR_UNSAFE_PORT`) | Chrome, Edge and Firefox block port 6000. Use PowerShell, curl or Postman instead. |
| `address already in use` | Another process holds port 5279, 7196 or 6000. Stop it, or change the port in `appsettings.json` (PlatformService) or `Properties/launchSettings.json` (CommandsService). If you change the gRPC port, update `GrpcPlatform` in CommandsService too. |
| A created platform never shows up in CommandsService | CommandsService was not running when the event was published. Restart it and it reloads all platforms over gRPC. |

### Build the Docker images

```bash
docker build -t <your-dockerhub-user>/platformservice PlatformService
docker build -t <your-dockerhub-user>/commandservice CommandsService
docker push <your-dockerhub-user>/platformservice
docker push <your-dockerhub-user>/commandservice
```

Containers run in the Production environment, where the services look for SQL Server, RabbitMQ and each other under their Kubernetes service names (see `appsettings.Production.json`). The images are therefore meant for the [Kubernetes deployment](#-deployment) below and do not work with a plain `docker run`.

## ⚙️ Configuration

### PlatformService Configuration

**appsettings.json**:
```json
{
  "CommandService": "http://localhost:6000/api/c/Platforms/testPost",
  "ConnectionStrings": {
    "PlatformsConn": "Server=localhost,14333;Database=PlatformDb;User Id=sa;Password=P@ssw0rd!;TrustServerCertificate=True"
  },
  "RabbitMQHost": "localhost",
  "RabbitMQPort": "5672",
  "Kestrel": {
    "Endpoints": {
      "Grpc": {
        "Protocols": "Http2",
        "Url": "http://localhost:7196"
      },
      "webApi": {
        "Protocols": "Http1",
        "Url": "http://localhost:5279"
      }
    }
  }
}
```

### CommandsService Configuration

**appsettings.json**:
```json
{
  "RabbitMQHost": "localhost",
  "RabbitMQPort": "5672",
  "GrpcPlatform": "http://localhost:7196"
}
```

## 📚 API Endpoints

### PlatformService

#### GET `/api/Platform/testApi`
Test endpoint to verify service is running.

#### GET `/api/Platform`
Get all platforms.

#### GET `/api/Platform/{id}`
Get platform by ID.

#### POST `/api/Platform/AddNewPlatform`
Create a new platform (without event publishing).

**Request Body**:
```json
{
  "name": "Platform Name",
  "cost": "Free",
  "publisher": "Publisher Name"
}
```

#### POST `/api/Platform/CreatePlatForm`
Create a new platform with event publishing (sends sync HTTP + async RabbitMQ).

**Request Body**:
```json
{
  "name": "Platform Name",
  "cost": "Free",
  "publisher": "Publisher Name"
}
```

### CommandsService

#### GET `/api/c/Platforms`
Test endpoint.

#### GET `/api/c/Platforms/platforms`
Get all platforms from CommandsService database.

#### GET `/api/c/platforms/{platformId}/Commands`
Get all commands for a specific platform.

#### GET `/api/c/platforms/{platformId}/Commands/{commandId}`
Get a specific command for a platform.

#### POST `/api/c/platforms/{platformId}/Commands`
Create a new command for a platform.

**Request Body**:
```json
{
  "howTo": "How to do something",
  "commandLine": "command --option value"
}
```

## 🚢 Deployment

### Kubernetes Deployment

The manifests are in the `K8S` directory. Apply them in this order from the repository root.

1. **Create the SQL Server password secret**

   The SQL Server deployment reads its `sa` password from a secret named `mssql`. It must match the password in the `PlatformsConn` connection string in `PlatformService/appsettings.Production.json`.

   ```bash
   kubectl create secret generic mssql --from-literal=SA_PASSWORD="P@ssw0rd!"
   ```

2. **Deploy SQL Server**
   ```bash
   kubectl apply -f K8S/local-pvc.yaml
   kubectl apply -f K8S/mssql-plat-depl.yaml
   ```

3. **Deploy RabbitMQ**
   ```bash
   kubectl apply -f K8S/rabbitmq-depl.yaml
   ```

4. **Deploy PlatformService**
   ```bash
   kubectl apply -f K8S/platforms-deply.yaml
   ```

5. **Deploy CommandsService**
   ```bash
   kubectl apply -f K8S/commands-deply.yaml
   ```

6. **Deploy the Ingress** (optional, needs the NGINX Ingress Controller)
   ```bash
   kubectl apply -f K8S/ingress-srv.yaml
   ```

   The ingress routes the host `acme.com`. Add this line to your hosts file (`C:\Windows\System32\drivers\etc\hosts` on Windows, `/etc/hosts` on Linux and macOS):

   ```
   127.0.0.1 acme.com
   ```

   The APIs are then available at `http://acme.com/api/platform` and `http://acme.com/api/c/platforms`.

7. **Check the result**
   ```bash
   kubectl get pods
   kubectl get services
   kubectl logs deployment/platforms-depl
   kubectl logs deployment/commands-depl
   ```

   All pods should reach `Running`. If CommandsService started before PlatformService or RabbitMQ were ready, restart it:

   ```bash
   kubectl rollout restart deployment commands-depl
   ```

### Docker Images

The deployments pull pre-built images from Docker Hub:
- `ramez007/platformservice:latest`
- `ramez007/commandservice:latest`

To deploy your own code changes, build and push the images under your own Docker Hub user (see [Build the Docker images](#build-the-docker-images)), change the `image:` lines in `K8S/platforms-deply.yaml` and `K8S/commands-deply.yaml`, and apply the manifests again.

## 📁 Project Structure

```
Soluation/
├── PlatformService/
│   ├── Controllers/
│   │   └── PlatformController.cs
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   ├── IPlatformRepo.cs
│   │   ├── PlatformRepo.cs
│   │   └── PrepDb.cs
│   ├── Models/
│   │   └── Platform.cs
│   ├── Dtos/
│   │   ├── PlatformDto.cs
│   │   └── PlatformPublishedDto.cs
│   ├── AsyncDataServices/
│   │   ├── IMessageBusClient.cs
│   │   └── MessageBusClient.cs
│   ├── SyncDataServices/
│   │   ├── Http/
│   │   │   ├── HttpCommandDataClient.cs
│   │   │   └── ICommandDataClient.cs
│   │   └── Grpc/
│   │       └── GrpcPlatformService.cs
│   ├── Protos/
│   │   └── platforms.proto
│   ├── Mapping/
│   │   └── ProfileMapping.cs
│   ├── Migrations/
│   ├── Program.cs
│   ├── Dockerfile
│   └── PlatformService.csproj
│
├── CommandsService/
│   ├── Controllers/
│   │   ├── CommandsController.cs
│   │   └── PlatformsController.cs
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   ├── ICommandRepo.cs
│   │   ├── CommandRepo.cs
│   │   └── PrepDb.cs
│   ├── Models/
│   │   ├── Command.cs
│   │   └── Platform.cs
│   ├── Dtos/
│   │   ├── CommandDtos.cs
│   │   ├── PlatformDto.cs
│   │   ├── PlatformPublishedDto.cs
│   │   └── GenericEventDto.cs
│   ├── AsyncDataServices/
│   │   └── MessageBusSubscriber.cs
│   ├── SyncDataServices/
│   │   └── Grpc/
│   │       ├── IPlatformDataClient.cs
│   │       └── PlatformDataClient.cs
│   ├── EventProcessing/
│   │   ├── IEventProcessor.cs
│   │   └── EventProcessor.cs
│   ├── MapperProfiles/
│   │   └── CommandsProfile.cs
│   ├── Protos/
│   │   └── platforms.proto
│   ├── Program.cs
│   ├── Dockerfile
│   └── CommandsService.csproj
│
└── K8S/
    ├── platforms-deply.yaml
    ├── commands-deply.yaml
    ├── rabbitmq-depl.yaml
    ├── mssql-plat-depl.yaml
    ├── ingress-srv.yaml
    └── local-pvc.yaml
```

## 🔍 Key Components

### Event Processing (CommandsService)

The `EventProcessor` class handles incoming RabbitMQ messages:
- Detects event type from message
- Processes `Platform_Published` events
- Creates platform entries in CommandsService database
- Prevents duplicate entries using ExternalID

### Message Bus Client (PlatformService)

The `MessageBusClient` class:
- Connects to RabbitMQ
- Publishes platform creation events
- Uses Fanout exchange for pub/sub pattern

### gRPC Service (PlatformService)

The `GrpcPlatformService` class:
- Implements gRPC service interface
- Exposes `GetAllPlatforms` method
- Returns platform data efficiently

### gRPC Client (CommandsService)

The `PlatformDataClient` class:
- Connects to PlatformService gRPC endpoint
- Fetches all platforms
- Maps gRPC models to domain models

## 🧪 Testing

With both services running locally, these steps exercise all three communication paths. The commands are for PowerShell; the `curl` equivalents follow.

1. **List the platforms in PlatformService** (four are seeded)
   ```powershell
   Invoke-RestMethod http://localhost:5279/api/Platform
   ```

2. **List the platforms in CommandsService** — the same four, copied over gRPC at startup
   ```powershell
   Invoke-RestMethod http://localhost:6000/api/c/Platforms/platforms
   ```

3. **Create a platform** — this publishes a `Platform_Published` event to RabbitMQ
   ```powershell
   Invoke-RestMethod -Method Post http://localhost:5279/api/Platform/CreatePlatForm `
     -ContentType 'application/json' `
     -Body '{"name":"Kubernetes","cost":"Free","publisher":"CNCF"}'
   ```

   The CommandsService console shows:

   ```
   --> Message received from RabbitMQ.
   --> Platform Published Event Detected
   --> Platform added!
   ```

4. **List the platforms in CommandsService again** — the new platform is now there
   ```powershell
   Invoke-RestMethod http://localhost:6000/api/c/Platforms/platforms
   ```

5. **Add a command to a platform and read it back**
   ```powershell
   Invoke-RestMethod -Method Post http://localhost:6000/api/c/platforms/1/Commands `
     -ContentType 'application/json' `
     -Body '{"howTo":"Build a project","commandLine":"dotnet build"}'

   Invoke-RestMethod http://localhost:6000/api/c/platforms/1/Commands
   ```

### curl equivalents

In Windows PowerShell use `curl.exe`, because `curl` is an alias for `Invoke-WebRequest` there.

```bash
curl http://localhost:5279/api/Platform
curl http://localhost:6000/api/c/Platforms/platforms

curl -X POST http://localhost:5279/api/Platform/CreatePlatForm \
  -H "Content-Type: application/json" \
  -d '{"name":"Kubernetes","cost":"Free","publisher":"CNCF"}'

curl -X POST http://localhost:6000/api/c/platforms/1/Commands \
  -H "Content-Type: application/json" \
  -d '{"howTo":"Build a project","commandLine":"dotnet build"}'

curl http://localhost:6000/api/c/platforms/1/Commands
```

## 📝 Notes

- **Development Mode**: Both services use InMemory databases for quick development
- **Production Mode**: PlatformService switches to SQL Server
- **Event-Driven**: CommandsService automatically receives platform updates via RabbitMQ
- **Fallback Mechanism**: If RabbitMQ fails, HTTP sync call is attempted
- **gRPC**: More efficient than HTTP for inter-service communication

## 🔐 Security Considerations

- Update default passwords in production
- Use Kubernetes secrets for sensitive configuration
- Implement authentication/authorization for production
- Use HTTPS/TLS for production deployments
- Secure RabbitMQ with authentication


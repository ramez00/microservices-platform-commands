# Microservices Platform & Commands Solution

A .NET 9.0 microservices solution demonstrating inter-service communication patterns including synchronous (HTTP REST, gRPC) and asynchronous (RabbitMQ) messaging.

## 📋 Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Services](#services)
- [Technology Stack](#technology-stack)
- [Communication Patterns](#communication-patterns)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [API Endpoints](#api-endpoints)
- [Deployment](#deployment)
- [Project Structure](#project-structure)

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

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for containerization)
- [Kubernetes](https://kubernetes.io/) (for deployment)
- [RabbitMQ](https://www.rabbitmq.com/) (or use Docker)
- [SQL Server](https://www.microsoft.com/sql-server) (for production)

## 🚀 Getting Started

### Local Development Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Soluation
   ```

2. **Start RabbitMQ** (using Docker)
   ```bash
   docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
   ```

3. **Start SQL Server** (for production mode)
   ```bash
   docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=P@ssw0rd!" -p 14333:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2017-latest
   ```

4. **Run PlatformService**
   ```bash
   cd PlatformService
   dotnet restore
   dotnet run
   ```
   Service will be available at:
   - HTTP API: `http://localhost:5279`
   - gRPC: `http://localhost:7196`

5. **Run CommandsService**
   ```bash
   cd CommandsService
   dotnet restore
   dotnet run
   ```
   Service will be available at:
   - HTTP API: `http://localhost:6000`

### Using Docker

1. **Build Docker images**
   ```bash
   cd PlatformService
   docker build -t platformservice:latest .
   
   cd ../CommandsService
   docker build -t commandservice:latest .
   ```

2. **Run containers**
   ```bash
   docker run -p 5279:80 -p 7196:666 platformservice:latest
   docker run -p 6000:8080 commandservice:latest
   ```

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

The solution includes Kubernetes deployment manifests in the `K8S` directory:

1. **Deploy SQL Server**
   ```bash
   kubectl apply -f K8S/mssql-plat-depl.yaml
   kubectl apply -f K8S/local-pvc.yaml
   ```

2. **Deploy RabbitMQ**
   ```bash
   kubectl apply -f K8S/rabbitmq-depl.yaml
   ```

3. **Deploy PlatformService**
   ```bash
   kubectl apply -f K8S/platforms-deply.yaml
   ```

4. **Deploy CommandsService**
   ```bash
   kubectl apply -f K8S/commands-deply.yaml
   ```

5. **Deploy Ingress** (optional)
   ```bash
   kubectl apply -f K8S/ingress-srv.yaml
   ```

### Docker Images

The deployment uses pre-built Docker images:
- `ramez007/platformservice:latest`
- `ramez007/commandservice:latest`

To use your own images, update the deployment YAML files.

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

### Test API Endpoints

1. **Test PlatformService**
   ```bash
   curl http://localhost:5279/api/Platform/testApi
   ```

2. **Test CommandsService**
   ```bash
   curl http://localhost:6000/api/c/Platforms/testAPI
   ```

3. **Create Platform**
   ```bash
   curl -X POST http://localhost:5279/api/Platform/CreatePlatForm \
     -H "Content-Type: application/json" \
     -d '{"name":"Docker","cost":"Free","publisher":"Docker Inc"}'
   ```

4. **Get Commands for Platform**
   ```bash
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

## 📄 License

[Specify your license here]

## 👥 Contributors

[Add contributors here]

## 🤝 Support

For issues and questions, please open an issue in the repository.


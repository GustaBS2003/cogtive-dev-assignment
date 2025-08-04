# COGTIVE - Factory Floor Dashboard

Este documento apresenta a solução para um dashboard de chão de fábrica com entrada de dados de produção, incluindo integrações com PostgreSQL, migração para .NET MAUI e propostas de melhorias arquiteturais.

---

## 1. PostgreSQL Integration Guide

### 1.1. Setup Process

| Etapa | Descrição | Detalhes |
|-------|-----------|----------|
| 1 | **Install PostgreSQL** | Download do postgresql.org, configuração padrão, usuário 'postgres' |
| 2 | **Create Database** | Database 'cogtive' criada via pgAdmin |
| 3 | **Configure Connection** | Connection string no appsettings.json, SslMode=Disable |
| 4 | **DbContext Factory** | AppDbContextFactory para migrations EF Core |
| 5 | **Migrations** | Migrations específicas para PostgreSQL com type mappings |
| 6 | **Environment Switch** | Variável DATABASE_PROVIDER para alternar entre SQLite/PostgreSQL |

**Comando PowerShell para PostgreSQL:**
```powershell
$env:DATABASE_PROVIDER="Postgres"
```

### 1.2. Benefits & Challenges

**Benefícios:**
- Melhor performance para queries complexas
- Melhor controle de concorrência
- Suporte a tipos de dados avançados
- Escalabilidade para datasets maiores
- Suporte completo ACID

**Desafios Resolvidos:**
- **Compatibilidade de Versões**: Versões consistentes dos pacotes EF Core
- **Conexão**: SSL desabilitado para desenvolvimento local
- **Type Mapping**: Mapeamentos específicos para colunas decimal do PostgreSQL
- **Migrations**: Caminhos separados para SQLite e PostgreSQL

### 1.3. Docker Setup

**Pré-requisitos:** Docker e Docker Compose instalados

#### Quick Start Scripts

Para facilitar a inicialização do ambiente, foram criados scripts para diferentes sistemas operacionais:

**Windows (start.bat):**
```batch
@echo off
docker-compose up -d
echo All services are starting...
echo Web UI will be available at: http://localhost:3000
echo API will be available at: http://localhost:5211/api
echo Use 'docker-compose logs -f' to view logs
```

**Linux/macOS (start.sh):**
```bash
#!/bin/bash
docker-compose up -d
echo "All services are starting..."
echo "Web UI will be available at: http://localhost:3000"
echo "API will be available at: http://localhost:5211/api"
echo "Use 'docker-compose logs -f' to view logs"
```

#### Usage

1. **Windows**: Execute `start.bat`
2. **Linux/macOS**: Execute `chmod +x start.sh && ./start.sh`
3. **Manual**: Execute `docker-compose up -d`

#### Services Available

| Serviço | URL | Descrição |
|---------|-----|-----------|
| **Web UI** | http://localhost:3000 | Interface React para dashboard |
| **API** | http://localhost:5211/api | Backend ASP.NET Core |
| **PostgreSQL** | localhost:5432 | Database (interno) |

#### Useful Commands

```bash
# Ver logs em tempo real
docker-compose logs -f

# Parar todos os serviços
docker-compose down

# Rebuild e restart
docker-compose up -d --build

# Ver status dos containers
docker-compose ps
```

---

## 2. .NET MAUI Migration Documentation

### 2.1. Project Overview

Dashboard de chão de fábrica para entrada de dados de produção, migrado do Xamarin.Forms para .NET MAUI (.NET 8) para aproveitar capacidades modernas e suporte de longo prazo.

### 2.2. Migration Process

| Fase | Ação | Resultado |
|------|------|-----------|
| **Setup** | Novo projeto .NET MAUI (.NET 8) | Suporte para Android, iOS, MacCatalyst, Windows |
| **Code Migration** | Migração de Models, Services, Views | Namespaces atualizados, estrutura MAUI |
| **UI Update** | Controles Xamarin → MAUI | Compatibilidade com Picker, CollectionView, ActivityIndicator |
| **Services** | Refatoração ApiService | HttpClient, async/await, URLs específicas por plataforma |
| **Offline Support** | Implementação modo offline | ConnectivityCheck, armazenamento local |

### 2.3. Challenges & Solutions

| Problema | Solução |
|----------|---------|
| **UI Thread & Modal Dialogs** | Substituição de DisplayAlert por labels ErrorMessage |
| **Connectivity Checks** | Uso de `Connectivity.Current.NetworkAccess == NetworkAccess.Internet` |
| **Secure Storage** | Padronização com `SecureStorage` e `FileSystem.AppDataDirectory` |
| **Platform URLs** | Configuração específica de base URLs por plataforma |
| **XAML Bindings** | Verificação de nomes de controles e bindings |

### 2.4. Recommendations

- Testar em todas as plataformas após migração
- Evitar modal dialogs durante inicialização
- Manter dependências atualizadas

---

## 3. Architecture Improvement Proposal

### 3.1. Current vs Proposed Architecture

**Estado Atual:**
- **Backend**: Minimal API, lógica de negócio misturada com endpoints
- **Frontend Web**: React com lógica de negócio nos componentes
- **Mobile**: Xamarin.Forms, lógica no code-behind

**Arquitetura Proposta:**

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│ Frontend (Web)  │◄──►│ Aplicativo.Core │◄──►│ Backend (API)   │
│ React, Charts   │    │ Models,Services │    │ Controllers,    │
│                 │    │                 │    │ CQRS, Mediator  │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         ▲                                              ▲
         │                                              │
┌─────────────────┐                          ┌─────────────────┐
│ Mobile MAUI     │◄────────────────────────►│ Database        │
│ MVVM            │                          │ EF Core,        │
│                 │                          │ PostgreSQL      │
└─────────────────┘                          └─────────────────┘
```

### 3.2. Backend Improvements

| Área | Melhoria | Benefício |
|------|----------|-----------|
| **Controllers** | Substituir Minimal APIs por Controllers | Melhor organização, versionamento, testabilidade |
| **CQRS & Mediator** | Separar operações read/write | Escalabilidade, desacoplamento |
| **DDD** | Domain models em Aplicativo.Core | Regras de negócio centralizadas |
| **Validation** | FluentValidation + middleware de erro | Validação consistente |
| **SignalR** | Hubs leves + message broker | Escalabilidade para múltiplas instâncias |
| **API Versioning** | Versionamento + OpenAPI | Compatibilidade e documentação |

### 3.3. Mobile App (.NET MAUI)

- **MVVM Pattern**: ViewModels para todas as páginas
- **Shared Logic**: Consumo de Aplicativo.Core
- **Data Visualization**: Microcharts, Syncfusion

### 3.4. Frontend Web (React)

- **Centralized Data Layer**: API service dedicado
- **Reusable Charts**: Chart.js, Recharts, Victory
- **Error/Loading States**: UI padronizada

### 3.5. Scalability & Maintainability

| Aspecto | Solução | Quando Aplicar |
|---------|---------|----------------|
| **Microservices** | API Gateway + serviços separados | Grande escala |
| **Database** | PostgreSQL + read replicas | Produção, analytics pesadas |
| **Messaging** | RabbitMQ, Azure Service Bus | Desacoplamento, real-time |
| **Testing** | xUnit, Moq, Jest, React Testing Library | Sempre |
| **CI/CD** | Pipelines automatizados | Desenvolvimento contínuo |

### 3.6. Example Project Structure

**Backend:**
```
/backend
  /Controllers
    MachinesController.cs
    ProductionDataController.cs
  /Hubs
    ProductionHub.cs
  /CQRS
    /Commands
    /Queries
    /Handlers
```

**Mobile:**
```
/mobile
  /ViewModels
    MachineListViewModel.cs
    ProductionDataViewModel.cs
  /Views
    MachineListPage.xaml
    ProductionDataPage.xaml
  /Services
    ApiService.cs
```

**Web:**
```
/web/src
  /components
    MachineList.tsx
    ProductionChart.tsx
  /services
    api.ts
    signalr.ts
  /contexts
    MachineContext.tsx
```

---

## 4. DevOps & Security

### 4.1. DevOps & Observability
- Docker Compose para desenvolvimento local
- Health checks e endpoints de métricas
- Logging: Serilog, Application Insights, ELK stack

### 4.2. Security
- Autenticação/autorização (JWT, OAuth2)
- Validação e sanitização de inputs
- HTTPS em todos os ambientes

---

## 5. Additional Recommendations

- **CQRS**: Para separação clara de read/write e escalabilidade
- **Event Sourcing**: Para auditoria completa (futuro)
- **API Gateway**: Ocelot/YARP para microservices
- **Code Generation**: NSwag/OpenAPI Generator para sincronização TypeScript/.NET

---

## 6. References

- [.NET MAUI Documentation](https://learn.microsoft.com/dotnet/maui/)
- [MAUI Migration Guide](https://learn.microsoft.com/dotnet/maui/migration/)
- [MAUI Community Toolkit](https://learn.microsoft.com/dotnet/communitytoolkit/maui/)

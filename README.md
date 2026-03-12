# SecureGuard DLP — Data Loss Prevention & Endpoint Monitoring System

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com)
[![Next.js](https://img.shields.io/badge/Next.js-14-000000)](https://nextjs.org)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791)](https://www.postgresql.org)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED)](https://www.docker.com)

SecureGuard is a comprehensive **Data Loss Prevention (DLP)** and **Endpoint Monitoring** system designed to prevent sensitive data leakage from corporate endpoints. It combines a Windows agent with a central server and an admin dashboard for real-time monitoring.

---

## Architecture

```
Managed Endpoint
┌───────────────────────────────┐
│ SecureGuard Agent (.NET 8)    │
│                               │
│ Local Proxy (Titanium 3.2)    │    HTTPS Inspection on 127.0.0.1:8877
│ File Watcher (FSW)            │    Monitors .env, secrets.json, etc.
│ Process Monitor (WMI/ETW)     │    Tracks process & network activity
│                               │
│ Detection Engine              │    4 built-in rules
│  - ImageUploadRule            │    Block image/* uploads → 403
│  - CredentialFileRule         │    Block .env, credentials.json, etc.
│  - UnknownIpRule              │    Flag unknown IPs
│  - CredentialPatternRule      │    Regex: AWS keys, JWT, passwords
│                               │
│ WinForms Alert Popup          │    Real-time desktop notification
│ Remote Log Service            │    HTTP POST to central server
└───────────────┬───────────────┘
                │ HTTPS
                ▼
        Central Server
   ┌──────────────────────┐
   │ ASP.NET Core 8 API   │   REST API + JWT Auth
   │ Entity Framework     │   ORM with migrations
   │ PostgreSQL 16        │   Primary database
   │ SQL Server 2022      │   Secondary database
   │ SignalR Hub          │   Real-time alerts
   └──────────┬───────────┘
              │
              ▼
   ┌──────────────────────┐
   │ Next.js 14 Dashboard │   Admin panel
   │ TailwindCSS          │   Styling
   │ shadcn/ui            │   Components
   │ Recharts             │   Charts
   │ SignalR Client       │   Real-time updates
   └──────────────────────┘
```

---

## Project Structure

```
SecureGuard_DLP/
├── README.md
├── secureguard_prd.md
├── docker-compose.yml
├── .gitignore
├── SecureGuard.sln
├── src/
│   ├── Agent/SecureGuard.Agent/        # .NET 8 Windows Service
│   │   ├── Proxy/                      # Titanium.Web.Proxy integration
│   │   ├── Detection/                  # Rule engine + 4 detection rules
│   │   ├── FileMonitor/                # FileSystemWatcher
│   │   ├── ProcessMonitor/             # WMI + process tracking
│   │   ├── Alert/                      # WinForms popup + AlertService
│   │   └── Logging/                    # HTTP log shipping
│   ├── Server/SecureGuard.Server/      # ASP.NET Core 8 Web API
│   │   ├── Controllers/                # 7 API controllers
│   │   ├── Data/                       # AppDbContext (EF Core)
│   │   ├── Models/                     # Agent, Log, Alert, Policy, etc.
│   │   ├── Hubs/                       # SignalR AlertHub
│   │   └── Middleware/                 # JWT middleware
│   ├── Shared/SecureGuard.Shared/      # Shared DTOs & constants
│   └── Dashboard/secureguard-dashboard/ # Next.js 14 admin panel
│       ├── app/                        # App Router pages
│       └── components/                 # React components
├── installer/
│   └── secureguard-installer.nsi       # NSIS installer script
└── docs/
    └── secureguard_prd.md              # Product Requirements Document
```

---

## Quick Start

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [Node.js 20+](https://nodejs.org)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- Windows 10/11 (for agent)

### 1. Start Server + Database (Docker)

```bash
docker-compose up -d
```

Services started:
- **API**: http://localhost:5000
- **Dashboard**: http://localhost:3000
- **PostgreSQL**: localhost:5432
- **SQL Server**: localhost:1433

### 2. Access Dashboard

Open: **http://localhost:3000**

Default credentials:
```
Username: admin
Password: Admin@123!
```

### 3. Build & Install Agent (Windows)

```powershell
# Build
cd src/Agent/SecureGuard.Agent
dotnet publish -c Release -r win-x64 --self-contained

# Install as Windows Service (run as Administrator)
sc create "SecureGuard Agent" binPath="C:\SecureGuard\SecureGuard.Agent.exe"
sc start "SecureGuard Agent"
```

Or use the NSIS installer:
```
installer/SecureGuard-Setup-1.0.0.exe
```

### 4. Configure Agent

Edit `appsettings.json`:
```json
{
  "Agent": {
    "AgentId": "unique-agent-id",
    "ServerUrl": "https://your-server:5001",
    "ProxyHost": "127.0.0.1",
    "ProxyPort": 8877,
    "WatchPaths": ["C:\\Users"],
    "WhitelistedIps": ["127.0.0.1", "::1", "192.168.1.1"]
  }
}
```

---

## API Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/login` | — | Login |
| POST | `/api/logs` | — | Receive agent logs |
| GET | `/api/logs` | JWT | Query logs |
| POST | `/api/alerts` | — | Receive alerts |
| GET | `/api/alerts` | JWT | Query alerts |
| PUT | `/api/alerts/{id}/acknowledge` | JWT | Acknowledge alert |
| GET | `/api/agents` | JWT | List agents |
| GET | `/api/agents/{id}` | JWT | Agent details |
| PUT | `/api/agents/{id}` | JWT | Update agent |
| GET | `/api/policies` | JWT | List policies |
| POST | `/api/policies` | JWT | Create policy |
| PUT | `/api/policies/{id}` | JWT | Update policy |
| DELETE | `/api/policies/{id}` | JWT | Delete policy |
| GET | `/api/whitelist` | JWT | IP whitelist |
| POST | `/api/whitelist` | JWT | Add IP |
| DELETE | `/api/whitelist/{id}` | JWT | Remove IP |
| GET | `/api/dashboard/stats` | JWT | Dashboard stats |

API Documentation (Swagger): http://localhost:5000/swagger

---

## Detection Rules

| Rule | Trigger | Action |
|------|---------|--------|
| **Image Upload** | Content-Type: image/* on POST/PUT | Block → HTTP 403 |
| **Credential File** | .env, secrets.json, credentials.json access | Block process |
| **Unknown IP** | Connection to non-whitelisted IP | Flag & alert |
| **Credential Pattern** | AWS_SECRET_ACCESS_KEY, API_KEY, JWT_SECRET, etc. | Block |

---

## Dashboard Pages

| Page | Path | Description |
|------|------|-------------|
| Dashboard | `/` | Stats, charts, recent alerts |
| Incidents | `/incidents` | Alert table with filters & acknowledge |
| Agents | `/agents` | Agent list with online/offline status |
| Agent Detail | `/agents/[id]` | Individual agent info + logs |
| Logs | `/logs` | Full log viewer with filters |
| Policies | `/policies` | CRUD policy management |
| Whitelist | `/whitelist` | IP whitelist management |
| Settings | `/settings` | System configuration |
| Login | `/login` | Authentication |

---

## Technology Stack

| Component | Technology |
|-----------|-----------|
| Agent | .NET 8 Windows Service |
| HTTP Proxy | Titanium.Web.Proxy 3.2 |
| File Monitoring | FileSystemWatcher |
| Process Monitoring | WMI + System.Management |
| Alert UI | WinForms |
| Backend API | ASP.NET Core 8 |
| ORM | Entity Framework Core 8 |
| Primary DB | PostgreSQL 16 |
| Secondary DB | SQL Server 2022 |
| Realtime | SignalR |
| Dashboard | Next.js 14 (App Router) |
| UI Components | TailwindCSS + shadcn/ui |
| Charts | Recharts |
| Auth | JWT Bearer Tokens |
| Deployment | Docker Compose |
| Installer | NSIS |

---

## Development

### Run API locally

```bash
cd src/Server/SecureGuard.Server
dotnet run
```

### Run Dashboard locally

```bash
cd src/Dashboard/secureguard-dashboard
npm install
cp .env.local.example .env.local
npm run dev
```

### Database Migrations

```bash
cd src/Server/SecureGuard.Server

# Add migration
dotnet ef migrations add InitialCreate --output-dir Data/Migrations

# Apply migration
dotnet ef database update
```

---

## Security Notes

1. **Change the JWT secret** in production — edit `Jwt__Key` in docker-compose.yml
2. **Change the database password** — edit `POSTGRES_PASSWORD` and connection strings
3. **Change the default admin password** after first login
4. **Root CA** — The proxy generates and installs a self-signed Root CA. Enterprise deployments should use a properly issued CA
5. **HTTPS** — Enable HTTPS in production by configuring SSL certificates

---

## License

MIT License — See [LICENSE](LICENSE) for details.

Data Loss Prevention & Endpoint Monitoring System

## Overview

SecureGuard is a comprehensive DLP and Endpoint Monitoring system designed to prevent sensitive data leakage from corporate endpoints. The system deploys agents on Windows endpoints to monitor file uploads, data transfers, unknown IP communications, and process/filesystem activities.

## Status

🚧 Under Development

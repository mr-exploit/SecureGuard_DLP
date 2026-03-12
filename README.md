# SecureGuard DLP

**Data Loss Prevention & Endpoint Monitoring System**

SecureGuard is a comprehensive DLP and Endpoint Monitoring system designed to prevent sensitive data leakage from corporate Windows endpoints.

---

## System Architecture

```
Managed Endpoint
┌───────────────────────────────┐
│ SecureGuard Agent (.NET 8)    │
│                               │
│ Local Proxy (Titanium 3.2)    │
│ File Watcher                  │
│ Process Monitor               │
│                               │
│ Detection Engine              │
│                               │
│ Block / Alert / Logging       │
└───────────────┬───────────────┘
                │ HTTPS
                ▼
        Central Server
   ┌──────────────────────┐
   │ ASP.NET Core 8 API   │
   │ PostgreSQL 16        │
   │ Next.js 14 Dashboard │
   └──────────────────────┘
```

---

## Technology Stack

| Layer | Technology |
|-------|-----------|
| Agent | .NET 8 Windows Service |
| HTTP Proxy | Titanium.Web.Proxy v3.2 |
| File Monitoring | FileSystemWatcher |
| Process Monitoring | WMI (System.Management) |
| Detection Engine | Custom C# rule engine |
| Backend API | ASP.NET Core 8 |
| ORM | Entity Framework Core 8 |
| Primary Database | PostgreSQL 16 |
| Secondary Database | SQL Server 2022 |
| Realtime | SignalR |
| Dashboard | Next.js 14 (App Router) |
| UI | TailwindCSS + shadcn/ui |

---

## Quick Start

```bash
# Start all services with Docker
docker-compose up -d

# Services:
# API:       http://localhost:5000
# Swagger:   http://localhost:5000/swagger
# Dashboard: http://localhost:3000
```

**Default Admin**: `admin@secureguard.local` / `Admin@SecureGuard2024!`

---

## Detection Rules

| Rule | Trigger | Action |
|------|---------|--------|
| Image Upload | `Content-Type: image/*` | Block (HTTP 403) |
| Credential File | Upload of `.env`, `secrets.json`, etc. | Block |
| Credential Pattern | AWS keys, API keys, JWT in body | Block |
| Unknown IP | Connection to unlisted IP | Flag/Alert |

---

## Project Structure

```
SecureGuard_DLP/
├── src/
│   ├── Agent/SecureGuard.Agent/          # .NET 8 Windows Service
│   ├── Server/SecureGuard.Server/        # ASP.NET Core 8 API
│   ├── Shared/SecureGuard.Shared/        # Shared DTOs + constants
│   └── Dashboard/secureguard-dashboard/  # Next.js 14 Admin Dashboard
├── installer/secureguard-installer.nsi   # NSIS installer
├── docs/secureguard_prd.md               # Product requirements
├── docker-compose.yml
└── SecureGuard.sln
```

---

## Development

### Server
```bash
cd src/Server/SecureGuard.Server
dotnet restore && dotnet run
```

### Dashboard
```bash
cd src/Dashboard/secureguard-dashboard
npm install && npm run dev
```

### Agent (Windows only)
```powershell
# Build
dotnet publish src/Agent/SecureGuard.Agent -c Release -o publish/

# Install as service (run as Administrator)
sc.exe create "SecureGuardAgent" binPath= "C:\SecureGuard\SecureGuard.Agent.exe --windows-service"
sc.exe start "SecureGuardAgent"
```

---

## API Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/login` | No | Get JWT token |
| POST | `/api/logs` | No | Submit log |
| GET | `/api/logs` | Yes | Query logs |
| POST | `/api/alerts` | No | Submit alert |
| GET | `/api/alerts` | Yes | Query alerts |
| GET | `/api/agents` | Yes | List agents |
| GET | `/api/policies` | Yes | List policies |
| GET | `/api/whitelist` | Yes | IP whitelist |
| GET | `/api/dashboard/stats` | Yes | Stats |

---

## Security Notes

- Change `Jwt:Key` and admin password in production
- Use HTTPS for the API server in production
- The Root CA certificate is self-signed — distribute securely

---

© 2024 SecureGuard Security

# SecureGuard -- Product Requirements Document (PRD)

## 1. Product Overview

SecureGuard adalah sistem Data Loss Prevention (DLP) dan Endpoint
Monitoring untuk mencegah kebocoran data sensitif dari endpoint
perusahaan.

Sistem memasang agent di setiap endpoint Windows untuk memonitor: -
Upload file ke internet - Transfer data sensitif - Komunikasi ke IP
tidak dikenal - Aktivitas proses dan filesystem

SecureGuard dirancang untuk lingkungan tanpa Active Directory dan
menggunakan Self‑Signed Root CA untuk SSL inspection.

------------------------------------------------------------------------

## 2. Goals

1.  Mencegah kebocoran data sensitif
2.  Memantau upload file ke internet
3.  Mengontrol komunikasi ke IP tidak dikenal
4.  Memberikan alert realtime
5.  Menyediakan dashboard monitoring endpoint

------------------------------------------------------------------------

## 3. Non‑Goals

Tidak termasuk pada fase awal:

-   Antivirus engine
-   Malware signature detection
-   Kernel level driver monitoring
-   Full EDR seperti CrowdStrike

------------------------------------------------------------------------

## 4. System Architecture

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
       │ ASP.NET Core API     │
       │ PostgreSQL / SQLSrv  │
       │ Next.js Dashboard    │
       └──────────────────────┘

------------------------------------------------------------------------

## 5. Technology Stack

  Layer                Technology
  -------------------- -------------------------
  Agent                .NET 8 Windows Service
  HTTP Proxy           Titanium.Web.Proxy v3.2
  File Monitoring      FileSystemWatcher
  Process Monitoring   WMI + ETW
  Detection Engine     Custom C# rule engine
  Alert UI             WinForms
  Backend API          ASP.NET Core 8
  ORM                  Entity Framework Core
  Database             PostgreSQL 16
  Secondary DB         SQL Server 2022
  Realtime             SignalR
  Dashboard            Next.js 14
  UI                   TailwindCSS + shadcn/ui

------------------------------------------------------------------------

## 6. Proxy Layer

Proxy menggunakan:

Titanium.Web.Proxy **version 3.2 (stable)**

Proxy berjalan di:

    127.0.0.1:8877

Browser diarahkan ke proxy lokal untuk intercept request.

------------------------------------------------------------------------

## 7. HTTPS Inspection

### Step 1 -- Generate Root CA

Server membuat certificate:

    SecureGuardCA.crt

### Step 2 -- Install di Endpoint

Agent installer memasang certificate ke:

    Trusted Root Certification Authorities

Contoh C# code:

``` csharp
X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
store.Open(OpenFlags.ReadWrite);
store.Add(certificate);
store.Close();
```

### Step 3 -- HTTPS Interception

Titanium Proxy:

1.  intercept request
2.  generate certificate domain
3.  sign menggunakan Root CA

------------------------------------------------------------------------

## 8. Detection Engine

Rule contoh:

  Rule                     Action
  ------------------------ ------------
  Upload image             Block
  Upload credential file   Block
  Unknown IP               Quarantine
  Regex credential         Block

------------------------------------------------------------------------

## 9. File Monitoring

Menggunakan:

    FileSystemWatcher

File sensitif:

    .env
    app.config
    secrets.json
    credentials.json

------------------------------------------------------------------------

## 10. Credential Detection

Regex scanning pattern:

    AWS_SECRET_ACCESS_KEY
    API_KEY
    DATABASE_PASSWORD
    JWT_SECRET

Flow:

    File detected
    ↓
    Read file
    ↓
    Regex scan
    ↓
    Match
    ↓
    Block process
    ↓
    Send alert

------------------------------------------------------------------------

## 11. Process Monitoring

Teknologi:

    WMI
    ETW

Digunakan untuk mengetahui process yang membuka file atau melakukan
network activity.

------------------------------------------------------------------------

## 12. Alert System

Jika violation terjadi:

Agent akan:

1.  Tampilkan popup
2.  Kirim log ke server
3.  Kirim realtime alert

Popup example:

    Security Alert

    Upload image tidak diperbolehkan.
    Action telah diblok.

------------------------------------------------------------------------

## 13. Logging

Log dikirim menggunakan:

    HTTP POST

Field log:

    timestamp
    agent_id
    hostname
    username
    process_name
    destination_ip
    violation_type
    severity

------------------------------------------------------------------------

## 14. Central Server

Server bertugas:

-   menerima log
-   menyimpan ke database
-   menyediakan API
-   realtime alert

------------------------------------------------------------------------

## 15. Database

Primary database:

    PostgreSQL

Secondary database:

    SQL Server

------------------------------------------------------------------------

## 16. Admin Dashboard

Dashboard menggunakan:

    Next.js 14

Fitur:

-   Incident monitoring
-   Agent monitoring
-   Logs
-   Policy management
-   IP whitelist

------------------------------------------------------------------------

## 17. Detection Flow

### Image Upload

    User upload image
    ↓
    Proxy intercept
    ↓
    content-type image/*
    ↓
    Block
    ↓
    HTTP 403

### Credential File

    User upload .env
    ↓
    FSWatcher detect
    ↓
    Regex scan
    ↓
    Block process

### Unknown IP

    Process connect IP
    ↓
    Check whitelist
    ↓
    Not allowed
    ↓
    Flag

------------------------------------------------------------------------

## 18. Security Considerations

Beberapa aplikasi menggunakan certificate pinning.

Contoh:

    WhatsApp Desktop

Solusi:

Phase 1

    File monitoring

Phase 2

    Socket monitoring (LSP)

------------------------------------------------------------------------

## 19. Deployment

Server deployment:

    Docker Compose

Services:

    API
    PostgreSQL
    SQL Server
    Dashboard

Agent installer:

    NSIS
    atau
    WiX

Installer akan:

-   install service
-   install Root CA
-   set proxy
-   start agent

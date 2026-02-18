# Architecture Document - Job Command Center (ProfileGenie)

**Version:** 1.0  
**Date:** 2026-02-17  
**Status:** Approved for Implementation

---

## 1. Executive Summary

The Job Command Center (ProfileGenie) is a **local-first automation suite** designed to aggregate, score, and manage LinkedIn job listings using a "God Mode" architecture. Instead of launching suspicious bot browsers, it attaches to the user's existing authenticated Chrome session via the Chrome DevTools Protocol (CDP), making automation indistinguishable from manual browsing.

### Key Architectural Pillars

1. **Maximum Stealth** - Operates within user's real browser context
2. **Data Sovereignty** - User owns all data in local PostgreSQL
3. **Unified Orchestration** - Single "F5 to run" experience via .NET Aspire
4. **Type Safety** - 100% C# stack for maximum code sharing

---

## 2. System Architecture

### 2.1 High-Level Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                        USER'S LOCAL MACHINE                          │
│                                                                      │
│  ┌──────────────────┐                                               │
│  │  Chrome Browser  │◄─────── User manually authenticates          │
│  │  (Port 9222)     │         LinkedIn session                      │
│  └────────┬─────────┘                                               │
│           │ CDP Connection                                          │
│           ▼                                                          │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │              .NET Aspire Host (Orchestrator)                  │  │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │  │
│  │  │  Harvester  │  │     Web     │  │     PostgreSQL      │  │  │
│  │  │  (Worker)   │  │  (Blazor)   │  │     (Container)     │  │  │
│  │  │  PROCESS    │  │  PROCESS    │  │     CONTAINER       │  │  │
│  │  └──────┬──────┘  └──────┬──────┘  └──────────┬──────────┘  │  │
│  │         │                │                     │              │  │
│  │         └────────────────┼─────────────────────┘              │  │
│  │                          │ EF Core                            │  │
│  │                          ▼                                     │  │
│  │              ┌─────────────────────┐                          │  │
│  │              │   Shared Models     │                          │  │
│  │              │   & Scoring Logic   │                          │  │
│  │              └─────────────────────┘                          │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

### 2.2 Component Responsibilities

| Component | Type | Responsibility |
|-----------|------|----------------|
| **AppHost** | Orchestrator | Manages lifecycle of all services, injects configuration |
| **Harvester** | Worker Service | CDP connection, LinkedIn scraping, data persistence |
| **Web** | Blazor Server | Dashboard UI, scoring engine, user interactions |
| **Data** | Class Library | EF Core DbContext, migrations, repositories |
| **Shared** | Class Library | DTOs, domain models, scoring algorithms |
| **ServiceDefaults** | Class Library | OpenTelemetry, health checks, logging config |

---

## 3. Core Components

### 3.1 The Orchestrator (ProfileGenie.AppHost)

```
ProfileGenie.AppHost/
├── Program.cs           # Service orchestration
├── appsettings.json     # Configuration
└── Properties/
    └── launchSettings.json
```

**Key Responsibilities:**
- Spin up PostgreSQL container via `.AddPostgres()`
- Launch Harvester as native process (NOT container)
- Start Blazor web server
- Inject connection strings and environment variables
- Manage service lifecycle and health

**Critical Configuration:**
```csharp
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .AddDatabase("jobdb");

// CRITICAL: Harvester must be a Project (process), NOT a container
builder.AddProject<Projects.ProfileGenie_Harvester>("harvester")
    .WithReference(postgres)
    .WithEnvironment("CHROME_DEBUG_PORT", "9222");

builder.AddProject<Projects.ProfileGenie_Web>("web")
    .WithReference(postgres);
```

### 3.2 The Harvester (ProfileGenie.Harvester)

```
ProfileGenie.Harvester/
├── Program.cs                    # Worker entry point
├── Services/
│   ├── ChromeConnector.cs        # CDP connection logic
│   ├── LinkedInScraper.cs        # Page scraping logic
│   └── Humanizer.cs              # Anti-detection delays
├── Models/
│   └── ScrapedJob.cs             # Raw scraped data
└── appsettings.json
```

**Key Responsibilities:**
- Connect to Chrome on `localhost:9222` via CDP
- Navigate LinkedIn search pages
- Extract job data (title, pay, badges, description)
- Implement human-mimicry (random delays, jitter)
- Persist jobs to PostgreSQL with deduplication

**CDP Connection Strategy:**
```csharp
// PRIMARY: Connect to existing Chrome
var browser = await playwright.Chromium.ConnectOverCDPAsync(
    $"http://localhost:{chromePort}"
);

// FALLBACK: Launch persistent context (never ephemeral)
var browser = await playwright.Chromium.LaunchPersistentContextAsync(
    userDataDir,
    new() { Headless = false }
);
```

**Execution Model:**
- **MUST run as native process** (shares host network)
- **MUST NOT be containerized** (cannot access localhost:9222)

### 3.3 The Command Center (ProfileGenie.Web)

```
ProfileGenie.Web/
├── Program.cs
├── Components/
│   ├── Layout/
│   │   └── MainLayout.razor
│   └── Pages/
│       ├── Dashboard.razor       # Main Kanban view
│       ├── JobDetail.razor       # Job details modal
│       └── Settings.razor        # Scoring matrix config
├── Services/
│   ├── JobService.cs             # Job CRUD operations
│   └── ScoringEngine.cs          # Scoring calculations
└── wwwroot/
    └── css/
```

**Key Responsibilities:**
- Display Kanban-style job pipeline
- Real-time updates via SignalR
- User-configurable scoring matrix
- Status workflow management
- Resume/cover letter editors

### 3.4 Data Store (ProfileGenie.Data)

```
ProfileGenie.Data/
├── AppDbContext.cs
├── Entities/
│   ├── Job.cs
│   ├── HistoryLog.cs
│   └── ScoringConfig.cs
├── Migrations/
└── Repositories/
    └── JobRepository.cs
```

**Database Schema:**

```sql
CREATE TABLE Jobs (
    Id              UUID PRIMARY KEY,
    LinkedInJobId   VARCHAR(50) UNIQUE NOT NULL,
    Title           VARCHAR(500) NOT NULL,
    Company         VARCHAR(500),
    Location        VARCHAR(500),
    PayRate         VARCHAR(100),
    IsRemote        BOOLEAN DEFAULT FALSE,
    IsEasyApply     BOOLEAN DEFAULT FALSE,
    IsTopApplicant  BOOLEAN DEFAULT FALSE,
    MatchLevel      VARCHAR(50),
    Description     TEXT,
    Score           INTEGER DEFAULT 0,
    Status          VARCHAR(50) DEFAULT 'Found',
    ScrapedAt       TIMESTAMP NOT NULL,
    UpdatedAt       TIMESTAMP
);

CREATE TABLE HistoryLogs (
    Id          UUID PRIMARY KEY,
    JobId       UUID REFERENCES Jobs(Id),
    Action      VARCHAR(100),
    Actor       VARCHAR(50),  -- 'System' or 'User'
    Timestamp   TIMESTAMP NOT NULL
);

CREATE TABLE ScoringConfigs (
    Id          UUID PRIMARY KEY,
    Criterion   VARCHAR(100) NOT NULL,
    Weight      INTEGER NOT NULL
);
```

---

## 4. Data Flow

### 4.1 Harvesting Flow

```
1. User launches Chrome with --remote-debugging-port=9222
2. User authenticates to LinkedIn in Chrome
3. User starts .NET Aspire solution
4. Harvester connects to Chrome via CDP
5. Harvester navigates to LinkedIn search
6. Harvester extracts job data with human delays
7. Harvester persists unique jobs to PostgreSQL
8. Web UI detects new jobs via polling/observable
9. User reviews and manages jobs in dashboard
```

### 4.2 Scoring Flow

```
1. User adjusts weights in Settings page
2. ScoringEngine recalculates scores for all jobs
3. Dashboard automatically re-sorts by new scores
4. Changes persist to ScoringConfig table
```

---

## 5. Security Architecture

### 5.1 Authentication Model

| Component | Authentication |
|-----------|---------------|
| **LinkedIn** | User authenticates externally in Chrome |
| **Application** | None (local-first, single user) |
| **Database** | Auto-generated password (dev) / Env vars (prod) |

### 5.2 Credential Handling

- **LinkedIn Credentials**: Application NEVER handles passwords
- **Session Cookies**: Inherited from user's Chrome session
- **Database Credentials**: Managed by Aspire (dev) / secure env vars (prod)

### 5.3 Network Security

- All services run on localhost
- No external API calls (except LinkedIn via user's browser)
- Database not exposed to network

---

## 6. Anti-Detection Architecture

### 6.1 Human-Mimicry Engine

| Feature | Implementation |
|---------|----------------|
| **Non-Linear Delays** | Gaussian distribution for wait times |
| **Micro-Interactions** | Randomized mouse "jitters" |
| **Organic Scrolling** | Variable speeds mimicking reading |
| **Distraction Clicks** | Occasional clicks on non-essential elements |

### 6.2 Rate Limiting

```csharp
public class Humanizer
{
    private readonly Random _random = new();
    
    public async Task HumanDelay(int minMs = 1000, int maxMs = 5000)
    {
        var delay = GaussianRandom(minMs, maxMs);
        await Task.Delay(delay);
    }
    
    public async Task CoolDown()
    {
        // Enforced pause after X pages scraped
        await Task.Delay(TimeSpan.FromMinutes(2));
    }
}
```

---

## 7. Resilience & Error Handling

### 7.1 Connection Recovery

```
Chrome CDP Connection Lost:
├── Log error to Aspire dashboard
├── Enter "Retry-Backoff" state
├── Attempt reconnection every 30s
└── Resume harvesting when available
```

### 7.2 Concurrency Handling

| Scenario | Mitigation |
|----------|------------|
| Simultaneous updates | EF Core concurrency tokens |
| Duplicate jobs | UNIQUE constraint on LinkedInJobId |
| Long-running scrapes | Cancellation token support |

---

## 8. Directory Structure

```
/ProfileGenie
├── ProfileGenie.sln
├── global.json
├── README.md
│
├── ProfileGenie.AppHost/           # Orchestrator
│   ├── Program.cs
│   └── appsettings.json
│
├── ProfileGenie.ServiceDefaults/   # Telemetry, health checks
│   └── Extensions.cs
│
├── ProfileGenie.Data/              # EF Core, Migrations
│   ├── AppDbContext.cs
│   ├── Entities/
│   └── Migrations/
│
├── ProfileGenie.Shared/            # DTOs, Models, Scoring
│   ├── Models/
│   │   ├── Job.cs
│   │   ├── JobStatus.cs
│   │   └── ScoringConfig.cs
│   └── Services/
│       └── ScoringEngine.cs
│
├── ProfileGenie.Harvester/         # Playwright Worker
│   ├── Program.cs
│   ├── Services/
│   │   ├── ChromeConnector.cs
│   │   ├── LinkedInScraper.cs
│   │   └── Humanizer.cs
│   └── appsettings.json
│
└── ProfileGenie.Web/               # Blazor Server UI
    ├── Program.cs
    ├── Components/
    │   ├── Layout/
    │   └── Pages/
    ├── Services/
    └── wwwroot/
```

---

## 9. Technology Decisions

### 9.1 Why CDP over Headless Browser?

| Factor | CDP | Headless |
|--------|-----|----------|
| Account Safety | ✅ Maximum | ❌ High Risk |
| Fingerprint Detection | ✅ Inherited | ❌ Detectable |
| Authentication | ✅ Uses existing session | ❌ Requires re-auth |
| Ban Risk | ✅ Minimal | ❌ High |

### 9.2 Why Harvester as Process (not Container)?

```
Container Network Isolation:
┌─────────────────┐     ┌─────────────────┐
│ Docker Container│  ✗  │ Host Localhost  │
│   Harvester     │     │  Chrome:9222    │
└─────────────────┘     └─────────────────┘

Host Process Network:
┌─────────────────┐     ┌─────────────────┐
│  Host Process   │  ✓  │ Host Localhost  │
│   Harvester     │     │  Chrome:9222    │
└─────────────────┘     └─────────────────┘
```

---

## 10. Future Architecture Considerations

| Consideration | Timeline | Impact |
|---------------|----------|--------|
| Cloud Database (Supabase) | Phase 4+ | Multi-device sync |
| API Layer | Future | Mobile app support |
| Plugin System | Future | Extensible scrapers |

---

## 11. References

- [Requirement and Option Analysis](./Requirement%20and%20Option%20Analysis_%20Job%20Command%20Center.md)
- [Development Plan](./Development%20Plan%20-%20Job%20Command%20Center.md)
- [App Implementation Spec](./App%20Implementation%20Spec.md)
- [Tech Stack](./tech-stack.md)

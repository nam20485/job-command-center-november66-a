# ProfileGenie - LinkedIn Profile Scoring System

A .NET 9 application for automated LinkedIn profile data collection, analysis, and scoring using Playwright browser automation.

## Features

- **Profile Harvester**: Automated LinkedIn profile scraping using Playwright
- **Profile Scoring**: Customizable scoring engine for profile evaluation
- **Web Dashboard**: Blazor Server UI for job management and monitoring
- **Job Queue**: Process multiple profiles in a managed pipeline

## Architecture

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   Blazor Web    │────▶│    PostgreSQL   │◀────│    Harvester    │
│   Dashboard     │     │    Database     │     │    Worker       │
└─────────────────┘     └─────────────────┘     └─────────────────┘
```

## Tech Stack

- **.NET 9** with C# 12
- **Blazor Server** for the web UI
- **Entity Framework Core** with PostgreSQL
- **Microsoft.Playwright** for browser automation
- **OpenTelemetry** for observability

## Project Structure

| Project | Description |
|---------|-------------|
| `ProfileGenie.Web` | Blazor Server web application |
| `ProfileGenie.Harvester` | Playwright-based profile scraping worker |
| `ProfileGenie.Data` | EF Core data access layer |
| `ProfileGenie.Shared` | Shared models and services |
| `ProfileGenie.ServiceDefaults` | Common service configuration |
| `ProfileGenie.AppHost` | Application orchestrator |

## Getting Started

### Prerequisites

- .NET 9 SDK
- PostgreSQL 15+
- Chrome/Chromium browser

### Installation

1. Clone the repository
```bash
git clone https://github.com/nam20485/job-command-center-november66-a.git
cd job-command-center-november66-a/src
```

2. Restore dependencies
```bash
dotnet restore
```

3. Configure database connection in `appsettings.json`

4. Create and apply migrations
```bash
cd ProfileGenie.Data
dotnet ef migrations add InitialCreate
dotnet ef database update
```

5. Run the application
```bash
cd ../ProfileGenie.Web
dotnet run
```

## Usage

1. Navigate to `https://localhost:5001` in your browser
2. Add profile URLs to the job queue via the Dashboard
3. Monitor job progress and view scoring results
4. Configure scoring parameters in Settings

## Development

### Build
```bash
dotnet build
```

### Test
```bash
dotnet test
```

### Run
```bash
# Web application
dotnet run --project ProfileGenie.Web

# Harvester worker
dotnet run --project ProfileGenie.Harvester
```

## License

See [LICENSE.md](LICENSE.md) for details.

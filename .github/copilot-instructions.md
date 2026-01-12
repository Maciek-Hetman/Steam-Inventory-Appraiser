# Steam Inventory Appraiser - AI Agent Instructions

## Architecture Overview
This is a full-stack Steam inventory valuation app with three services orchestrated via Docker Compose:
- **Backend**: .NET 10 Web API (C#) at `localhost:5001` with Swagger at `/swagger`
- **Frontend**: React 19 + Vite at `localhost:3000`
- **Database**: PostgreSQL 18 at `localhost:5433` (note non-standard port)

Data flow: Frontend → Backend API → Steam Community API (inventory + market prices) → PostgreSQL storage

## Development Workflow

### Starting the Application
```bash
# First run: Copy .env.example to .env and configure values
cp .env.example .env
# Edit .env with your database credentials
docker-compose up --build
```

All required environment variables are defined in `.env.example` at project root.

### Database Migrations (EF Core)
Migrations run automatically on startup via `Program.cs` (`dbContext.Database.Migrate()`). To create new migrations:
```bash
docker-compose run --rm migrate dotnet ef migrations add MigrationName --project MyApi
```

### Key Endpoints
- `GET /api/value/steam/profile?steamId64={id}` - Primary endpoint: fetches inventory, prices items via Steam Market API, calculates total value, and persists to database. **Currently only returns 2 items** despite processing full inventory
- `GET /api/steam/inventory/{steamId64}` - Raw inventory data (used by test frontend)

**Note**: Frontend is currently a minimal test harness for API validation, not a production interface.

## Critical Patterns & Conventions

### Steam API Integration
- **No Authentication**: Steam Community API endpoints used are public - no API keys requiredd/MyApi/Program.cs#L18-L22)) - sets base URL and required User-Agent header
- **Steam ID Extraction**: Helper in [SteamProfileHelper.cs](backend/MyApi/Helpers/SteamProfileHelper.cs) handles both direct profiles (`/profiles/{id}`) and vanity URLs (resolved via XML endpoint)
- **Rate Limiting**: No current implementation - Steam Market API calls are sequential and may need throttling for large inventories

### Service Layer Pattern
Backend uses constructor-injected interfaces for testability:
- `ISteamInventoryService` - fetches inventory JSON from `steamcommunity.com/inventory/{id}/730/2` (CS:GO items)
- `ISteamMarketService` - fetches individual item prices from `/market/priceoverview/` endpoint
- Services are scoped in DI container ([Program.cs](backend/MyApi/Program.cs#L25-L26))

### Data Model
Two entities with one-to-many relationship ([AppDbContext.cs](backend/MyApi/Data/AppDbContext.cs)):
- `InventoryValuation` - snapshot per Steam profile (SteamId64, TotalValueUsd, CreatedAt)
- `InventoryValuationItem` - individual item valuations (MarketHashName, Amount, ValueUsd)

EF Core automatically manages foreign key relationship via collection property.

### CORS Configuration
Frontend origin `http://localhost:3000` explicitly allowed in [Program.cs](backend/MyApi/Program.cs#L13-L18). Update when deploying to production.

## Project-Specific Decisions

### Why PostgreSQL on Port 5433?
Avoids conflicts with default PostgreSQL installations on developer machines. Internal Docker port remains 5432.

### Why Separate Docker Services for API/Frontend?
Allows independent rebuilds and hot-reload for frontend (volume mount at [docker-compose.yml](docker-compose.yml#L44-L45)).

### Environment Variables Pattern
Connection strings use environment variable substitution from `.env` file. Required vars:
- `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB` (database)
- `DB_HOST=db` (Docker service name for internal networking)

### Null Handling
Nullable reference types enabled project-wide (`<Nullable>enable</Nullable>` in [MyApi.csproj](backend/MyApi/MyApi.csproj#L5)). Use `!` suppression only for validated Steam API responses.

## Known Gaps (Per README To-Do)
- `/api/value/steam/profile` only returns 2 items in response (despite processing all items for database storage)
- Items worth ≤$0.01 not yet filtered from responses
- No image URL or rarity data in API responses (available in Steam inventory JSON as `icon_url` and `tags`)
- No XML/JSON import/export for database
- Frontend needs full implementation - currently minimal test UI

## Tech Stack Versions
- .NET 10.0 (latest), EF Core 10.0.1, Npgsql 10.0.0
- React 19.2, Vite 7.2.4
- PostgreSQL 18-alpine

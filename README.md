# Eindproject: Digitale rondleiding ROC van Twente

Dit project is de backend voor een digitale rondleiding door ROC van Twente. De applicatie ondersteunt bezoekers die zelfstandig routes en locaties kunnen bekijken, en beheerders die gebouwen, locaties, routes en statistieken kunnen beheren.

De backend is gebouwd als ASP.NET Core Web API met Entity Framework Core, SQL Server, ASP.NET Core Identity en JWT-authenticatie. De frontend staat in de aparte repository `eindproject-frontend-wipwapwebdev` en gebruikt deze API voor de publieke rondleiding en het beheerportaal.

## Functionaliteiten

- Publieke API voor gebouwen, locaties en rondleidingsroutes.
- Admin-login met JWT access tokens en refresh tokens.
- Beheer van locaties, routes en routepunten via beveiligde endpoints.
- Beheer van admin-accounts en rollen.
- Analytics voor paginaweergaven en gestarte routes.
- CORS-configuratie voor koppeling met de frontend.
- Automatische database-migraties en optionele admin-seeding bij het starten.

## Techniek

- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- JWT Bearer Authentication
- Refresh tokens
- OpenAPI in development

## Projectstructuur

```text
TemplateJwtProject/
├── Controllers/       API controllers voor auth, admin, routes, locaties en analytics
├── Data/              Entity Framework DbContext
├── Models/            Domeinmodellen en DTOs
├── Services/          JWT, refresh-token en role-initialisatie
├── Migrations/        Database migraties
├── Docs/              Extra documentatie
└── Program.cs         Applicatieconfiguratie en middleware
```

## Belangrijkste API endpoints

### Publiek

```http
GET /api/Building
GET /api/Building/{id}
GET /api/Location
GET /api/Location/{id}
GET /api/routes
GET /api/routes/{id}
POST /api/analytics/events
```

### Beheer

Voor deze endpoints is een geldige JWT nodig.

```http
POST /api/auth/login
POST /api/auth/refresh-token
POST /api/auth/revoke-token
POST /api/auth/logout-all

POST /api/Location
PUT /api/Location/{id}
DELETE /api/Location/{id}

POST /api/routes
PUT /api/routes/{id}
DELETE /api/routes/{id}

GET /api/analytics/summary
```

## Installatie

### Vereisten

- .NET SDK 10
- SQL Server of SQL Server Express
- Een database connection string

### Configuratie

Zet de connection string en JWT-instellingen in `TemplateJwtProject/appsettings.Development.json` of via environment variables.

Voorbeeld:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=Rondleiding;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "SecretKey": "gebruik-een-lange-veilige-secret-key-van-minimaal-32-tekens",
    "Issuer": "TemplateJwtProject",
    "Audience": "TemplateJwtProjectAdmins",
    "ExpiryInMinutes": "60",
    "RefreshTokenExpiryInDays": "7"
  },
  "CorsSettings": {
    "AllowedOrigins": [
      "http://localhost:5173"
    ]
  }
}
```

Voor productie horen secrets niet in Git. Gebruik daar environment variables.

## Lokaal starten

Voer de backend uit vanaf de root van deze repository:

```powershell
dotnet run --project .\TemplateJwtProject\TemplateJwtProject.csproj
```

In development wordt standaard een admin-account aangemaakt als er nog geen admin bestaat:

```text
Email: admin@example.com
Wachtwoord: Admin123!
```

Pas dit aan voordat de applicatie buiten de lokale omgeving gebruikt wordt.

## Database

De applicatie gebruikt Entity Framework Core migraties. Bij het opstarten worden rollen geinitialiseerd en kan een admin-account worden aangemaakt via configuratie.

Nieuwe migratie aanmaken:

```powershell
dotnet ef migrations add NaamVanMigratie --project .\TemplateJwtProject
```

Database handmatig bijwerken:

```powershell
dotnet ef database update --project .\TemplateJwtProject
```

## Frontend koppelen

De frontend gebruikt de API voor:

- het tonen van gebouwen, locaties en routes;
- het starten van rondleidingen;
- het bijhouden van analytics;
- het admin-dashboard.

Controleer bij lokale ontwikkeling dat de frontend-origin in `CorsSettings:AllowedOrigins` staat, bijvoorbeeld `http://localhost:5173`.

## Deployment

Voor deployment naar MonsterASP staat een aparte handleiding in:

```text
MONSTERASP_DEPLOY.md
```

Belangrijke productie-instellingen:

- `ConnectionStrings__DefaultConnection`
- `JwtSettings__SecretKey`
- `JwtSettings__Issuer`
- `JwtSettings__Audience`
- `CorsSettings__AllowedOrigins__0`
- `ADMIN_EMAIL`
- `ADMIN_PASSWORD`

Commit nooit productie-secrets of database-wachtwoorden naar Git.

## Auteur

Lars Gerbert  
Eindproject Webdevelopment - ROC van Twente

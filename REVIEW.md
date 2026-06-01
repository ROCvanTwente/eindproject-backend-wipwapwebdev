# PR Review

## Verdict: Request changes

## Must fix
- Hardcoded admin-credentials in startup (`TemplateJwtProject/Program.cs`), inclusief `admin@example.com` en `Admin123!`, zijn een blokkerend security-risico. Statische credentials in code maken misbruik eenvoudiger, kunnen per ongeluk buiten development actief blijven en horen niet in source control.
- `RoleInitializer.InitializeAsync` wordt dubbel aangeroepen in `TemplateJwtProject/Program.cs`. Startup-initialisatie moet éénmalig en voorspelbaar gebeuren om onnodige side effects en onduidelijk gedrag te voorkomen.
- `TemplateJwtProject/Controllers/PointController.cs` is niet production-ready: in-memory lijstopslag, mismatch tussen bestandsnaam en controllernaam (`PointController.cs` vs `MapController`), geen namespace en een prototype-achtige structuur.

## Should fix
- CORS-configuratie in `TemplateJwtProject/Program.cs` gebruikt `AllowCredentials()` en moet strikter per environment worden begrensd.
- De repository lijkt verantwoordelijkheden te mixen, inclusief `TemplateJwtProject/package.json`; controleer of dit bestand functioneel in de backend-repo thuishoort.
- Documentatie in `TemplateJwtProject/Docs/ADMIN_SETUP.md` promoot onveilige defaults zoals standaard admin-credentials en moet security-first worden herschreven.

## Nice to have
- Consistentere naamgeving en projectstructuur.
- Extra tests voor login, refresh token, role assignment en forced password change.
- Aanvullende security hardening, zoals beter secret management en strengere auth-bescherming.

## Positieve punten
- Duidelijke backend-structuur met controllers/services/models.
- EF Core-configuratie is overzichtelijk en goed georganiseerd.
- JWT-service is rechttoe rechtaan en begrijpelijk.
- Data annotations worden passend gebruikt.
- Logging sanitation utility is een pluspunt.

## Suggested review summary
- Request changes: de repository heeft een solide basis, maar bevat blocking issues rond security, dubbele startup-logica en prototype-code in de hoofdcodebase.

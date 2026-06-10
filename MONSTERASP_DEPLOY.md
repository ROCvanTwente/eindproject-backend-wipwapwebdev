# MonsterASP deployment

This backend is an ASP.NET Core Web API with Entity Framework Core and SQL Server.

## 1. Create hosting resources

1. Create a website in the MonsterASP control panel.
2. Create an MSSQL database in the MonsterASP control panel.
3. Copy the MSSQL connection string from the control panel.

MonsterASP documents the same flow in their deploy guide:
https://help.monsterasp.net/books/deploy/page/how-to-deploy-net-core-web-application-with-mssql-using-visual-studio

## 2. Configure environment variables

In MonsterASP, go to:

`Websites -> Manage website -> Scripting -> Environment Variables`

Add these values:

```text
ConnectionStrings__DefaultConnection=<MonsterASP MSSQL connection string>
JwtSettings__SecretKey=<long random secret, 32+ characters>
JwtSettings__Issuer=TemplateJwtProject
JwtSettings__Audience=TemplateJwtProjectAdmins
CorsSettings__AllowedOrigins__0=<frontend URL, for example https://example.com>
AdminSeed__Email=<admin email>
AdminSeed__Password=<strong admin password>
```

ASP.NET Core maps double underscores to nested configuration keys. For example,
`ConnectionStrings__DefaultConnection` overrides `ConnectionStrings:DefaultConnection`.

Do not commit the MonsterASP connection string or production JWT secret to Git.

## 3. Publish locally

From the repository root:

```powershell
dotnet publish .\TemplateJwtProject\TemplateJwtProject.csproj -c Release -o .\publish\monsterasp
```

Upload the contents of `publish/monsterasp` to the website root directory on MonsterASP.
MonsterASP's FTP/SFTP guide says the website root is `\wwwroot`:
https://help.monsterasp.net/books/deploy/page/how-to-deploy-website-content-via-ftpsftp

## 4. First start

On first start, the app runs EF Core migrations automatically and seeds the admin user
only when `AdminSeed__Email` and `AdminSeed__Password` are set.

If files are locked during upload, restart/stop the website in the control panel or upload
an `app_offline.htm` file before replacing files.

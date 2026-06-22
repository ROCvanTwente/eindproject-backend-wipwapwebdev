using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TemplateJwtProject.Constants;
using TemplateJwtProject.Data;
using TemplateJwtProject.Models;
using TemplateJwtProject.Services;
using TemplateJwtProject.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Database configuration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure())
        .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));

// Identity configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

if (string.IsNullOrWhiteSpace(secretKey))
{
    throw new InvalidOperationException("JWT SecretKey is not configured");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    };
});

// CORS configuration
var corsSettings = builder.Configuration.GetSection("CorsSettings");
var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:1234", "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var dbContext = services.GetRequiredService<AppDbContext>();
    
    logger.LogInformation("Initializing application roles and admin user");

    await dbContext.Database.ExecuteSqlRawAsync("""
        IF OBJECT_ID(N'[Locations]') IS NOT NULL
           AND COL_LENGTH(N'[Locations]', N'ImageUrl') IS NULL
        BEGIN
            ALTER TABLE [Locations] ADD [ImageUrl] nvarchar(max) NULL;
        END

        IF OBJECT_ID(N'[Locations]') IS NOT NULL
           AND COL_LENGTH(N'[Locations]', N'ImageUrl') IS NOT NULL
           AND (
               SELECT DATA_TYPE
               FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = N'Locations'
                 AND COLUMN_NAME = N'ImageUrl'
           ) <> N'nvarchar'
        BEGIN
            ALTER TABLE [Locations] ALTER COLUMN [ImageUrl] nvarchar(max) NULL;
        END

        IF OBJECT_ID(N'[Locations]') IS NOT NULL
           AND COL_LENGTH(N'[Locations]', N'ImageUrl') IS NOT NULL
           AND (
               SELECT CHARACTER_MAXIMUM_LENGTH
               FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = N'Locations'
                 AND COLUMN_NAME = N'ImageUrl'
           ) <> -1
        BEGIN
            ALTER TABLE [Locations] ALTER COLUMN [ImageUrl] nvarchar(max) NULL;
        END

        IF OBJECT_ID(N'[Buildings]') IS NOT NULL
        BEGIN
            ALTER TABLE [Buildings] ALTER COLUMN [Description] nvarchar(max) NOT NULL;
        END

        IF OBJECT_ID(N'[Locations]') IS NOT NULL
        BEGIN
            ALTER TABLE [Locations] ALTER COLUMN [Description] nvarchar(max) NOT NULL;
        END

        IF OBJECT_ID(N'[Routes]') IS NOT NULL
        BEGIN
            ALTER TABLE [Routes] ALTER COLUMN [Description] nvarchar(max) NOT NULL;
        END

        IF OBJECT_ID(N'[AspNetUsers]') IS NOT NULL
           AND COL_LENGTH(N'[AspNetUsers]', N'PasswordChanged') IS NULL
        BEGIN
            ALTER TABLE [AspNetUsers] ADD [PasswordChanged] bit NOT NULL CONSTRAINT [DF_AspNetUsers_PasswordChanged] DEFAULT CAST(0 AS bit);
        END

        IF OBJECT_ID(N'[AspNetUsers]') IS NOT NULL
           AND COL_LENGTH(N'[AspNetUsers]', N'RequiresAccountSetup') IS NULL
        BEGIN
            ALTER TABLE [AspNetUsers] ADD [RequiresAccountSetup] bit NOT NULL CONSTRAINT [DF_AspNetUsers_RequiresAccountSetup] DEFAULT CAST(0 AS bit);
        END

        IF OBJECT_ID(N'[AspNetUsers]') IS NOT NULL
           AND COL_LENGTH(N'[AspNetUsers]', N'FirstLoginCodeHash') IS NULL
        BEGIN
            ALTER TABLE [AspNetUsers] ADD [FirstLoginCodeHash] nvarchar(max) NULL;
        END

        IF OBJECT_ID(N'[AspNetUsers]') IS NOT NULL
           AND COL_LENGTH(N'[AspNetUsers]', N'FirstLoginCodeExpiresAt') IS NULL
        BEGIN
            ALTER TABLE [AspNetUsers] ADD [FirstLoginCodeExpiresAt] datetime2 NULL;
        END
        """);
    
    await RoleInitializer.InitializeAsync(services);

    // Seed admin user
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? builder.Configuration["AppSettings:AdminEmail"];
    var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? builder.Configuration["AppSettings:AdminPassword"];

    if (app.Environment.IsDevelopment())
    {
        adminEmail ??= "admin@example.com";
        adminPassword ??= "Admin123!";
    }

    if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
    {
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, Roles.Admin);
                logger.LogInformation("Admin account created: {AdminEmail}", LoggingUtilities.SanitizeForLog(adminEmail));
            }
            else
            {
                logger.LogError("Failed to create admin account for {AdminEmail}. Errors: {Errors}",
                    LoggingUtilities.SanitizeForLog(adminEmail),
                    string.Join("; ", result.Errors.Select(e => LoggingUtilities.SanitizeForLog(e.Description))));
            }
        }
        else
        {
            if (!await userManager.IsInRoleAsync(adminUser, Roles.Admin))
            {
                await userManager.AddToRoleAsync(adminUser, Roles.Admin);
            }

            logger.LogInformation("Admin account already exists: {AdminEmail}", LoggingUtilities.SanitizeForLog(adminEmail));
        }
    }
    else
    {
        logger.LogInformation("Admin seed skipped. Set ADMIN_EMAIL and ADMIN_PASSWORD to create one.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}

// CORS middleware (for Authentication and Authorization!)
app.UseCors("DefaultCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

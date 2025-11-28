using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using ConsolidatedApi.Data;
using ConsolidatedApi.Models;
using ConsolidatedApi.Services;
using ConsolidatedApi.Hubs;
using QuestPDF.Infrastructure;
using DotNetEnv;
using System.Security.Claims;

// Load environment variables
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Set QuestPDF license
QuestPDF.Settings.License = LicenseType.Community;

// Database configuration - Use Neon PostgreSQL
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
Console.WriteLine($"Raw DATABASE_URL: '{databaseUrl}'");

if (string.IsNullOrEmpty(databaseUrl))
{
    databaseUrl = "postgresql://neondb_owner:npg_CAiFLbX85sIq@ep-summer-fire-adwac3xi-pooler.c-2.us-east-1.aws.neon.tech/neondb?sslmode=require";
    Console.WriteLine("Using fallback DATABASE_URL");
}

// Convert DATABASE_URL (URI) to Npgsql connection string to avoid parsing issues
string BuildNpgsqlConnectionString(string url)
{
    try
    {
        var uri = new Uri(url);
        var userInfo = uri.UserInfo.Split(':', 2);
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";

        var host = uri.Host;
        var port = uri.IsDefaultPort ? 5432 : uri.Port;
        var database = uri.AbsolutePath.TrimStart('/');

        // Build Npgsql-style connection string
        return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to parse DATABASE_URL as URI. Using raw with enforced SSL. Error: {ex.Message}");
        var raw = url ?? "";
        if (!raw.Contains("SSL Mode=", StringComparison.OrdinalIgnoreCase))
        {
            raw += (raw.EndsWith(";") || raw.Length == 0 ? "" : ";") + "SSL Mode=Require;Trust Server Certificate=true";
        }
        return raw;
    }
}

var connectionString = BuildNpgsqlConnectionString(databaseUrl);
Console.WriteLine($"Final Npgsql connection string: '{connectionString}'");

// Stripe configuration
var stripeSecretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
if (!string.IsNullOrEmpty(stripeSecretKey))
{
    Stripe.StripeConfiguration.ApiKey = stripeSecretKey;
}

// Add services
builder.Services.AddHttpClient();

// Database context with PostgreSQL
builder.Services.AddDbContext<ConsolidatedDbContext>(options =>
    options.UseNpgsql(connectionString));

// Identity configuration
builder.Services.AddIdentity<ApplicationUser, Role>()
    .AddEntityFrameworkStores<ConsolidatedDbContext>()
    .AddDefaultTokenProviders();

// Configure Identity options
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.AllowedForNewUsers = true;
});

// JWT Authentication
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "your-super-secret-jwt-key-here";
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "ConsolidatedApi";
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "ConsolidatedApi";

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
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero,
        NameClaimType = ClaimTypes.NameIdentifier
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["accessToken"];
            return Task.CompletedTask;
        },
        OnChallenge = async context =>
        {
            if (!context.Handled && !context.Response.HasStarted)
            {
                var refreshToken = context.Request.Cookies["refreshToken"];
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(
                    string.IsNullOrEmpty(refreshToken)
                        ? "{\"message\":\"Unauthorized\"}"
                        : "{\"message\":\"Token expired\"}"
                );

                context.HandleResponse();
            }
        }
    };
});

// Register all services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<AddressService>();
builder.Services.AddScoped<ClientOrganizationService>();
builder.Services.AddScoped<UiPageService>();
builder.Services.AddScoped<GeminiPromptService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<DeviceService>();
builder.Services.AddScoped<CustomerDevicesService>();
builder.Services.AddScoped<FeedbackService>();
builder.Services.AddScoped<InstallationRequestService>();
builder.Services.AddScoped<InvoicesService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ServiceRequestService>();
builder.Services.AddScoped<SparePartService>();
builder.Services.AddScoped<ServiceRequestAiService>();
builder.Services.AddScoped<StatsService>();
builder.Services.AddScoped<MinioService>();

// Add controllers and SignalR
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Add Swagger with JWT Authentication
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Consolidated API", 
        Version = "v1" 
    });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
    
    // Ignore problematic types that might cause issues
    c.IgnoreObsoleteActions();
    c.IgnoreObsoleteProperties();
    
    // Custom schema handling for complex types
    c.CustomSchemaIds(type => type.FullName);
});

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "https://lmobileportal.vercel.app",
                "https://preview--genesis-hello-joy.lovable.app",
                "https://cd5be52a-f84e-471e-915b-176724baadb5.lovableproject.com"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .SetIsOriginAllowedToAllowWildcardSubdomains(); // More permissive for development
    });
    
    // Add a more permissive policy for development
    options.AddPolicy("Development", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Configure URLs for Render deployment
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// Database migration
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var dbContext = scope.ServiceProvider.GetRequiredService<ConsolidatedDbContext>();

    logger.LogInformation("Migrating database...");
    try
    {
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migrated successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

// Configure middleware pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Consolidated API v1");
    c.RoutePrefix = string.Empty; // Set Swagger UI as the default page
});

app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// Configure API routes to match microservice structure
app.MapControllerRoute(
    name: "user-api",
    pattern: "user/api/{controller}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "service-api", 
    pattern: "service/api/{controller}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "chat-api",
    pattern: "chat/api/{controller}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "notification-api",
    pattern: "notification/api/{controller}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "gateway",
    pattern: "gateway/{action=Index}/{id?}",
    defaults: new { controller = "Gateway" });

app.MapControllerRoute(
    name: "default",
    pattern: "api/{controller}/{action=Index}/{id?}");

// Map SignalR hubs
app.MapHub<ChatHub>("/chathub");
app.MapHub<UserToUserChatHub>("/usertouserhub");
app.MapHub<NotificationHub>("/notificationhub");

app.Run();
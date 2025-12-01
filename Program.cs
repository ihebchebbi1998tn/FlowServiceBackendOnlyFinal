using MyApi.Data;
using MyApi.Configuration;
using MyApi.Modules.Auth.Services;
using MyApi.Modules.Users.Services;
using MyApi.Modules.Roles.Services;
using MyApi.Modules.Skills.Services;
using MyApi.Modules.Contacts.Services;
using MyApi.Modules.Articles.Services;
using MyApi.Modules.Calendar.Services;
using MyApi.Modules.Projects.Services;
using MyApi.Modules.Lookups.Services;
using MyApi.Modules.Offers.Services;
using MyApi.Modules.Sales.Services;
using MyApi.Modules.Installations.Services;
using MyApi.Modules.ServiceOrders.Services;
using MyApi.Modules.Planning.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Read DATABASE_URL from environment or fallback
var rawConnection = Environment.GetEnvironmentVariable("DATABASE_URL") ??
    builder.Configuration.GetConnectionString("DefaultConnection");

string connectionString;

// Logging setup
var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("Startup");
logger.LogInformation($"Raw connection: {rawConnection?.Substring(0, Math.Min(80, rawConnection?.Length ?? 0))}...");

if (!string.IsNullOrEmpty(rawConnection))
{
    if (rawConnection.StartsWith("postgres://") || rawConnection.StartsWith("postgresql://"))
    {
        try
        {
            var uri = new Uri(rawConnection);

            var userInfo = uri.UserInfo?.Split(':', 2) ?? new string[0];
            var username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "";
            var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
            var database = uri.AbsolutePath?.TrimStart('/') ?? "";

            var npgBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port > 0 ? uri.Port : 5432,
                Username = username,
                Password = password,
                Database = database,
                // Neon requires SSL
                SslMode = SslMode.Require,
            };

            // Append query params if they exist (?sslmode=...&...)
            var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
            foreach (var kv in queryParams)
            {
                try { npgBuilder[kv.Key] = kv.Value.ToString(); } catch { }
            }

            connectionString = npgBuilder.ToString();
            logger.LogInformation("✅ Successfully built connection string from DATABASE_URL");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Failed to parse DATABASE_URL, falling back to raw connection");
            connectionString = rawConnection;
        }
    }
    else
    {
        // Already in key=value form
        connectionString = rawConnection;
    }
}
else
{
    // Fallback for local dev only
    connectionString = "Host=localhost;Port=5432;Database=myapi_dev;Username=postgres;Password=dev_password;SSL Mode=Disable";
    logger.LogWarning("⚠️ Using fallback development connection string");
}

// Register DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyHere12345";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "MyApi",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "MyApiClients",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Register custom services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPreferencesService, PreferencesService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ISkillService, SkillService>();

// Contacts Module Services
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IContactNoteService, ContactNoteService>();
builder.Services.AddScoped<IContactTagService, ContactTagService>();

// Articles Module Services
builder.Services.AddScoped<IArticleService, ArticleService>();

// Calendar Module Services
builder.Services.AddScoped<ICalendarService, CalendarService>();

// Lookups Module Services
builder.Services.AddScoped<ILookupService, LookupService>();

// Tasks Module Services
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IProjectColumnService, ProjectColumnService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITaskCommentService, TaskCommentService>();
builder.Services.AddScoped<ITaskAttachmentService, TaskAttachmentService>();

// Offers Module Services
builder.Services.AddScoped<IOfferService, OfferService>();

// Sales Module Services
builder.Services.AddScoped<ISaleService, SaleService>();

// Installations Module Services
builder.Services.AddScoped<IInstallationService, InstallationService>();

builder.Services.AddScoped<IServiceOrderService, ServiceOrderService>();

// Dispatches Module Services
builder.Services.AddScoped<MyApi.Modules.Dispatches.Services.IDispatchService, MyApi.Modules.Dispatches.Services.DispatchService>();

// Planning Module Services
builder.Services.AddScoped<IPlanningService, PlanningService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Swagger Documentation
builder.Services.AddSwaggerDocumentation(builder.Configuration);

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Render port
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
app.Urls.Add($"http://0.0.0.0:{port}");

// Auto-migrate DB
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var migrationLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        migrationLogger.LogInformation("📦 Checking for pending Entity Framework migrations...");

        IEnumerable<string> pendingMigrations = Enumerable.Empty<string>();
        try
        {
            pendingMigrations = context.Database.GetPendingMigrations().ToList();
            if (pendingMigrations.Any())
            {
                migrationLogger.LogWarning("⚠️ Pending migrations detected: {Count}. List: {List}", pendingMigrations.Count(), string.Join(", ", pendingMigrations));
            }
            else
            {
                migrationLogger.LogInformation("✅ No pending migrations detected.");
            }
        }
        catch (Exception enumEx)
        {
            // If enumeration fails, log and continue to attempt a Migrate() which will surface issues
            migrationLogger.LogWarning(enumEx, "⚠️ Unable to enumerate pending migrations. Will attempt to run Migrate() anyway.");
        }

        try
        {
            if (pendingMigrations.Any())
                migrationLogger.LogInformation("📦 Applying pending migrations...");
            else
                migrationLogger.LogInformation("📦 Ensuring database is up to date...");

            context.Database.Migrate();
            migrationLogger.LogInformation("✅ Database migrations completed successfully.");

            // Verify there are no remaining pending migrations after applying
            try
            {
                var remaining = context.Database.GetPendingMigrations().ToList();
                if (remaining.Any())
                {
                    migrationLogger.LogError("❌ Some migrations are still pending after Migrate(): {List}", string.Join(", ", remaining));
                    // Throw to make the failure obvious in platform logs/exit code
                    throw new InvalidOperationException($"Some EF migrations were not applied: {string.Join(", ", remaining)}");
                }
            }
            catch (Exception verifyEx)
            {
                migrationLogger.LogWarning(verifyEx, "⚠️ Unable to verify pending migrations after Migrate().");
            }
        }
        catch (Exception migrateEx)
        {
            migrationLogger.LogError(migrateEx, "❌ Error while migrating the database.");
            throw; // rethrow so the host fails early and Render will show the error
        }

        try
        {
            migrationLogger.LogInformation("📋 Validating database tables...");

            // Define expected tables based on actual EF Core table names
            var expectedTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "__EFMigrationsHistory",
                "MainAdminUsers",
                "users",
                "UserPreferences",
                "roles",
                "UserRoles",
                "skills",
                "UserSkills",
                "RoleSkills",
                "contacts",
                "ContactTags",
                "ContactTagAssignments",
                "ContactNotes",
                "articles",
                "article_categories",
                "locations",
                "inventory_transactions",
                "calendar_events",
                "event_attendees",
                "event_reminders",
                "event_types",
                "LookupItems",
                "currencies",
                "offers",
                "offer_items",
                "offer_activities",
                "sales",
                "sale_items",
                "sale_activities",
                "installations",
                "maintenance_histories",
                "service_orders",
                "service_order_jobs",
                "projects",
                "projectcolumns",
                "projecttasks",
                "taskcomments",
                "taskattachments",
                "dailytasks",
                "dispatches",
                "dispatch_technicians",
                "dispatch_time_entries",
                "dispatch_expenses",
                "dispatch_materials",
                "dispatch_attachments",
                "dispatch_notes",
                "technician_working_hours",
                "technician_leaves",
                "technician_status_history",
                "dispatch_history"
            };

            var existingTables = context.Database.SqlQueryRaw<string>(
                @"SELECT table_name 
                  FROM information_schema.tables 
                  WHERE table_schema = 'public' 
                  AND table_type = 'BASE TABLE'
                  ORDER BY table_name"
            ).ToList();

            var existingSet = new HashSet<string>(existingTables, StringComparer.OrdinalIgnoreCase);
            
            migrationLogger.LogInformation($"📊 Database Table Validation Report:");
            migrationLogger.LogInformation($"   Expected: {expectedTables.Count} tables | Found: {existingTables.Count} tables");
            migrationLogger.LogInformation("");

            // Show created tables
            var createdTables = expectedTables.Where(t => existingSet.Contains(t)).OrderBy(t => t).ToList();
            if (createdTables.Any())
            {
                migrationLogger.LogInformation("✅ Created Tables:");
                foreach (var table in createdTables)
                {
                    migrationLogger.LogInformation($"   ✅ {table}");
                }
            }

            // Show missing tables in red/error
            var missingTables = expectedTables.Where(t => !existingSet.Contains(t)).OrderBy(t => t).ToList();
            if (missingTables.Any())
            {
                migrationLogger.LogError("");
                migrationLogger.LogError($"❌ Missing Tables ({missingTables.Count}):");
                foreach (var table in missingTables)
                {
                    migrationLogger.LogError($"   ❌ {table} - NOT CREATED");
                }
                migrationLogger.LogError("");
            }

            // Show unexpected tables (exist but not in expected list)
            var unexpectedTables = existingSet.Where(t => !expectedTables.Contains(t)).OrderBy(t => t).ToList();
            if (unexpectedTables.Any())
            {
                migrationLogger.LogWarning("");
                migrationLogger.LogWarning($"⚠️ Unexpected Tables ({unexpectedTables.Count}):");
                foreach (var table in unexpectedTables)
                {
                    migrationLogger.LogWarning($"   ⚠️ {table}");
                }
            }

            // Summary
            migrationLogger.LogInformation("");
            if (missingTables.Any())
            {
                migrationLogger.LogError($"❌ Database validation FAILED - {missingTables.Count} table(s) missing!");
            }
            else
            {
                migrationLogger.LogInformation("✅ Database validation PASSED - All expected tables exist!");
            }
        }
        catch (Exception tableEx)
        {
            migrationLogger.LogError(tableEx, "❌ Error while validating database tables.");
        }
    }
    catch (Exception ex)
    {
        migrationLogger.LogError(ex, "❌ Error while migrating the database.");
        throw;
    }
}

// Middleware pipeline
app.UseSwaggerDocumentation(builder.Configuration);

// Serve static files for Swagger UI customizations
app.UseStaticFiles();

app.UseCors("AllowFrontend");

// Only use HTTPS redirection in development or when HTTPS port is properly configured
if (builder.Environment.IsDevelopment() || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORT")))
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Root redirect → Swagger UI
app.MapGet("/", () => Results.Redirect("/swagger"));

// Health endpoint
app.MapGet("/health", () => new { status = "healthy", timestamp = DateTime.UtcNow });

app.Run();

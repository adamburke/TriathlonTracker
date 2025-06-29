using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TriathlonTracker.Data;
using TriathlonTracker.Models;
using TriathlonTracker.Services;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add database configuration provider
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Configuration.AddDatabase(connectionString);
}

// Add localization services
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddViewLocalization();

// Add Entity Framework with PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure Identity options
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
});

// Configure session timeout to 1 hour
builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.SlidingExpiration = true;
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Add encryption and configuration services
builder.Services.AddSingleton<IEncryptionService, AesEncryptionService>();
builder.Services.AddScoped<IConfigurationService, DatabaseConfigurationService>();

// Add GDPR services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IGdprService, GdprService>();
builder.Services.AddScoped<IConsentService, ConsentService>();
builder.Services.AddScoped<IEnhancedGdprService, EnhancedGdprService>();

// Add Phase 3 GDPR services
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IGdprMonitoringService, GdprMonitoringService>();
builder.Services.AddScoped<IDataRetentionService, DataRetentionService>();
builder.Services.AddScoped<ISecurityService, SecurityService>();

// TODO: Add rate limiting for API endpoints
// Rate limiting will be implemented after resolving API compatibility

// Add Google Authentication
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
    })
    .AddJwtBearer("Bearer", options =>
    {
        var jwtKey = builder.Configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey))
            throw new InvalidOperationException("JWT signing key is not configured. Please set Jwt:Key in your configuration.");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TriathlonTracker API",
        Version = "v1",
        Description = "API for TriathlonTracker including GDPR compliance endpoints. Secured with JWT Bearer (OAuth2)."
    });
    
    // Add GDPR API documentation
    options.SwaggerDoc("gdpr", new OpenApiInfo
    {
        Title = "TriathlonTracker GDPR API",
        Version = "v1",
        Description = "GDPR compliance API endpoints for data subject rights management."
    });
    // Enable XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
    // Add JWT Bearer security definition
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
});

var app = builder.Build();

// Supported cultures
var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("es"),
    new CultureInfo("es-ES"),
    new CultureInfo("es-MX"),
    new CultureInfo("fr"),
    new CultureInfo("fr-CA"),
    new CultureInfo("zh-Hans"),
    new CultureInfo("de"),
    new CultureInfo("tr"),
    new CultureInfo("ru"),
    new CultureInfo("pt")
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Enable Swagger UI in all environments (or restrict to dev if you prefer)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TriathlonTracker API v1");
    c.SwaggerEndpoint("/swagger/gdpr/swagger.json", "TriathlonTracker GDPR API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
// TODO: app.UseRateLimiter(); - Add back when rate limiting is properly configured

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Ensure database is created and seed configuration
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var configService = scope.ServiceProvider.GetRequiredService<IConfigurationService>();
    context.Database.EnsureCreated();
    
    // Migrate existing unencrypted sensitive values
    await MigrateExistingSensitiveValues(context, configService);
    
    // Seed Google OAuth configuration if not exists
    if (!await configService.ExistsAsync("Authentication:Google:ClientId"))
    {
        await configService.SetValueAsync(
            "Authentication:Google:ClientId",
            "YOUR_GOOGLE_CLIENT_ID_HERE",
            "Google OAuth Client ID for authentication"
        );
    }
    
    if (!await configService.ExistsAsync("Authentication:Google:ClientSecret"))
    {
        await configService.SetValueAsync(
            "Authentication:Google:ClientSecret",
            "YOUR_GOOGLE_CLIENT_SECRET_HERE",
            "Google OAuth Client Secret for authentication"
        );
    }
    
    // Seed roles and users
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    // Create roles if they don't exist
    await SeedRoles(roleManager);
    
    // Check if test user exists using FirstOrDefaultAsync to avoid duplicate issues
    var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "test@test.com");
    if (existingUser == null)
    {
        var testUser = new User
        {
            UserName = "test@test.com",
            Email = "test@test.com",
            FirstName = "Test",
            LastName = "User",
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(testUser, "Test@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(testUser, "User");
        }
    }
    else
    {
        // Ensure existing test user has User role
        if (!await userManager.IsInRoleAsync(existingUser, "User"))
        {
            await userManager.AddToRoleAsync(existingUser, "User");
        }
    }
    
    // Check if admin user exists
    var existingAdmin = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@test.com");
    if (existingAdmin == null)
    {
        var adminUser = new User
        {
            UserName = "admin@test.com",
            Email = "admin@test.com",
            FirstName = "Admin",
            LastName = "User",
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(adminUser, "Test@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
    else
    {
        // Ensure existing admin user has Admin role
        if (!await userManager.IsInRoleAsync(existingAdmin, "Admin"))
        {
            await userManager.AddToRoleAsync(existingAdmin, "Admin");
        }
    }
    
    // Fix existing Google OAuth users who don't have roles assigned
    var googleUsers = await context.Users
        .Where(u => u.Email != null && !u.Email.EndsWith("@test.com"))
        .ToListAsync();
    
    foreach (var user in googleUsers)
    {
        var userRoles = await userManager.GetRolesAsync(user);
        if (!userRoles.Any())
        {
            await userManager.AddToRoleAsync(user, "User");
        }
    }
    
    // Seed default data retention policies
    await SeedDataRetentionPolicies(context);
}

// Helper method to seed roles
static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
{
    var roles = new[] { "Admin", "User", "DataProtectionOfficer" };
    
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// Helper method to seed data retention policies
static async Task SeedDataRetentionPolicies(ApplicationDbContext context)
{
    if (!await context.DataRetentionPolicies.AnyAsync())
    {
        var policies = new[]
        {
            new DataRetentionPolicy
            {
                DataType = "PersonalData",
                Description = "User personal information and account data",
                RetentionPeriodDays = 2555, // 7 years
                LegalBasis = "Contract",
                RetentionReason = "Required for account management and legal compliance",
                IsActive = true,
                AutoDelete = false,
                DeletionMethod = "SoftDelete",
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new DataRetentionPolicy
            {
                DataType = "TriathlonData",
                Description = "Triathlon performance records and training data",
                RetentionPeriodDays = 1825, // 5 years
                LegalBasis = "Contract",
                RetentionReason = "Service provision and user experience",
                IsActive = true,
                AutoDelete = false,
                DeletionMethod = "SoftDelete",
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new DataRetentionPolicy
            {
                DataType = "ConsentRecords",
                Description = "User consent and withdrawal records",
                RetentionPeriodDays = 2555, // 7 years
                LegalBasis = "LegalObligation",
                RetentionReason = "GDPR compliance and audit requirements",
                IsActive = true,
                AutoDelete = false,
                DeletionMethod = "HardDelete",
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new DataRetentionPolicy
            {
                DataType = "ProcessingLogs",
                Description = "Data processing activity logs",
                RetentionPeriodDays = 1095, // 3 years
                LegalBasis = "LegalObligation",
                RetentionReason = "Audit trail and compliance monitoring",
                IsActive = true,
                AutoDelete = true,
                DeletionMethod = "HardDelete",
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new DataRetentionPolicy
            {
                DataType = "MarketingData",
                Description = "Marketing preferences and communication history",
                RetentionPeriodDays = 730, // 2 years
                LegalBasis = "Consent",
                RetentionReason = "Marketing communications and preference management",
                IsActive = true,
                AutoDelete = true,
                DeletionMethod = "Anonymize",
                CreatedBy = "System",
                UpdatedBy = "System"
            }
        };

        context.DataRetentionPolicies.AddRange(policies);
        await context.SaveChangesAsync();
    }
}

// Helper method to migrate existing unencrypted sensitive values
static async Task MigrateExistingSensitiveValues(ApplicationDbContext context, IConfigurationService configService)
{
    var sensitiveKeys = new[] { "Authentication:Google:ClientId", "Authentication:Google:ClientSecret", "Jwt:Key" };
    
    foreach (var key in sensitiveKeys)
    {
        var config = await context.Configurations.FirstOrDefaultAsync(c => c.Key == key);
        if (config != null && !config.IsEncrypted)
        {
            // Re-save the value through the service to encrypt it
            await configService.SetValueAsync(config.Key, config.Value, config.Description);
        }
    }
}

app.Run();

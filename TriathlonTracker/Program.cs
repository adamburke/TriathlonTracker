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
        Title = "TriathlonTracker Reporting API",
        Version = "v1",
        Description = "API for reporting system integration. Secured with JWT Bearer (OAuth2)."
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
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TriathlonTracker Reporting API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

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
    
    // Seed test user
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    
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
        await userManager.CreateAsync(testUser, "Test@123");
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

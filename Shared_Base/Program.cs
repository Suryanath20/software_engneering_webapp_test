using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using ANDtr.Web.Common;
using ANDtr.Web.Data;
using ANDtr.Web.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Connection string resolution
var connectionString = Environment.GetEnvironmentVariable("ANDTR_DB")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Port=3306;Database=andtr_db;User=andtr_user;Password=andtr_password;TreatTinyAsBoolean=true;";

// Test MySQL reachability for resilient developer experience
bool isMySqlAvailable = false;
try
{
    using var testConn = new MySqlConnector.MySqlConnection(connectionString);
    testConn.Open();
    isMySqlAvailable = true;
    testConn.Close();
}
catch
{
    isMySqlAvailable = false;
}

// 2. DbContext registration
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (isMySqlAvailable)
    {
        options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36)), mySqlOptions =>
        {
            mySqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
        });
    }
    else
    {
        options.UseInMemoryDatabase("ANDtr_Database");
    }
});

// 3. Identity configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, AppClaimsPrincipalFactory>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Accounts/Login";
    options.LogoutPath = "/Accounts/Logout";
    options.AccessDeniedPath = "/Accounts/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// 4. Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireEmployeeRole", policy => policy.RequireRole("Employee"));
    options.AddPolicy("RequireCustomerRole", policy => policy.RequireRole("Customer"));
});

// 5. Razor View Engine Locations
builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationFormats.Clear();
    options.ViewLocationFormats.Add("/Shared_Base/Views/{1}/{0}.cshtml");
    options.ViewLocationFormats.Add("/Shared_Base/Views/Shared/{0}.cshtml");
    options.ViewLocationFormats.Add("/Shared_Base/{0}.cshtml");
    options.ViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
    options.ViewLocationFormats.Add("/{0}.cshtml");
});

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<MustChangePasswordFilter>();
});

var app = builder.Build();

// 6. Database Migration & Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var db = services.GetRequiredService<AppDbContext>();

    try
    {
        if (db.Database.IsRelational())
        {
            logger.LogInformation("[ANDtr] Applying EF Core migrations to MySQL...");
            await db.Database.MigrateAsync();
        }
        else
        {
            logger.LogInformation("[ANDtr] MySQL at localhost:3306 is not connected. Initialized in-memory database fallback for seamless development preview. (Run 'docker compose up -d' for persistent MySQL).");
            await db.Database.EnsureCreatedAsync();
        }

        await DbSeeder.SeedAsync(services);
        logger.LogInformation("[ANDtr] Database initialization and seeding complete.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "[ANDtr] Error during database migration or seeding.");
    }
}

// 7. Pipeline configuration
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

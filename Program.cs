using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ANDtr.Web.Common;
using ANDtr.Web.Data;
using ANDtr.Web.Models;
using ANDtr.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Database connection (MySQL with In-Memory fallback for local development)
var connectionString = Environment.GetEnvironmentVariable("ANDTR_DB")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Port=3306;Database=andtr_db;User=andtr_user;Password=andtr_password;TreatTinyAsBoolean=true;";

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

// Authentication & Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;

    // Lock account for 5 minutes after 5 failed attempts
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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireEmployeeRole", policy => policy.RequireRole("Employee"));
    options.AddPolicy("RequireCustomerRole", policy => policy.RequireRole("Customer"));
});

// Application services for team user stories
builder.Services.AddScoped<IImportService, ImportService>();
builder.Services.AddScoped<INoticeSearchService, NoticeSearchService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IShareService, ShareService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Seed initial dataset and user accounts
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var db = services.GetRequiredService<AppDbContext>();

    try
    {
        if (db.Database.IsRelational())
        {
            await db.Database.MigrateAsync();
        }
        else
        {
            await db.Database.EnsureCreatedAsync();
        }

        await DbSeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error initializing database.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Accounts/AccessDenied");
}

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORT")))
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();

using IS7012Final.Data;
using Microsoft.AspNetCore.Identity;
using IS7012Final.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var sqlConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(sqlConnection));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// For development convenience, do not require confirmed account. Change to 'true' in production.
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();
// Simple no-op email sender for development (used by Register page when sending confirmation emails)
builder.Services.AddSingleton<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, IS7012Final.Services.EmailSender>();

// Ensure console logging so diagnostic logs appear in the VS Output / console
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication middleware must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

// Development-only diagnostics middleware: log request/response cookie headers and auth state
app.Use(async (context, next) =>
{
    try
    {
        var logger = app.Logger;
        // Log request-level information
        var path = context.Request.Path + context.Request.QueryString;
        var hasAuthCookie = context.Request.Cookies.ContainsKey(".AspNetCore.Identity.Application");
        logger.LogInformation("Request {Path}: HasAuthCookie={HasAuthCookie}; User.IsAuthenticated={IsAuth}; User.Identity.Name={Name}", path, hasAuthCookie, context.User?.Identity?.IsAuthenticated, context.User?.Identity?.Name);

        // Log incoming Cookie header briefly
        if (context.Request.Headers.ContainsKey("Cookie"))
        {
            var cookieHeader = context.Request.Headers["Cookie"].ToString();
            logger.LogDebug("Request Cookie header (truncated): {Cookie}", cookieHeader.Length > 200 ? cookieHeader.Substring(0, 200) + "..." : cookieHeader);
        }
    }
    catch (Exception ex)
    {
        // swallow diagnostics exceptions
        app.Logger.LogDebug(ex, "Diagnostics middleware error");
    }

    // Capture the response to log Set-Cookie headers after the rest of the pipeline runs
    await next();

    try
    {
        if (context.Response.Headers.ContainsKey("Set-Cookie"))
        {
            foreach (var setCookie in context.Response.Headers["Set-Cookie"])
            {
                app.Logger.LogInformation("Response Set-Cookie: {SetCookie}", setCookie);
            }
        }

        // Add debug headers as well for quick browser inspection
        var hasCookie = context.Request.Cookies.ContainsKey(".AspNetCore.Identity.Application");
        var isAuth = context.User?.Identity?.IsAuthenticated == true;
        context.Response.Headers["X-Debug-Auth-HasCookie"] = hasCookie.ToString();
        context.Response.Headers["X-Debug-Auth-IsAuthenticated"] = isAuth.ToString();
    }
    catch { }
});

app.MapRazorPages();
// Automatically apply any pending EF Core migrations at startup so the
// database schema is created/updated and rows added via the UI persist.
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
    }
    catch (Exception)
    {
        // If migration fails at startup we don't want to crash the app here during development.
        // The error will appear in logs; developers can run EF CLI manually to diagnose.
    }
}

// Ensure AspNetUsers has FirstName and LastName columns (defensive for environments where migrations were not applied)
using (var scope = app.Services.CreateScope())
{
    var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var conn = db.Database.GetDbConnection();
        conn.Open();
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "PRAGMA table_info('AspNetUsers')";
            var reader = cmd.ExecuteReader();
            var hasFirst = false;
            var hasLast = false;
            while (reader.Read())
            {
                var colName = reader.GetString(1);
                if (string.Equals(colName, "FirstName", StringComparison.OrdinalIgnoreCase)) hasFirst = true;
                if (string.Equals(colName, "LastName", StringComparison.OrdinalIgnoreCase)) hasLast = true;
            }
            reader.Close();

            if (!hasFirst)
            {
                logger.LogInformation("Adding FirstName column to AspNetUsers");
                using var addCmd = conn.CreateCommand();
                addCmd.CommandText = "ALTER TABLE AspNetUsers ADD COLUMN FirstName TEXT";
                addCmd.ExecuteNonQuery();
            }
            if (!hasLast)
            {
                logger.LogInformation("Adding LastName column to AspNetUsers");
                using var addCmd = conn.CreateCommand();
                addCmd.CommandText = "ALTER TABLE AspNetUsers ADD COLUMN LastName TEXT";
                addCmd.ExecuteNonQuery();
            }
        }
    }
    catch (Exception ex)
    {
        // Log but do not rethrow - avoid crashing in dev environment
        var logger2 = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
        logger2.LogWarning(ex, "Failed to ensure user columns exist");
    }
}

app.Run();

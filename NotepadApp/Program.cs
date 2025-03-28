using NotepadApp.Hubs;
using Microsoft.EntityFrameworkCore;
using NotepadApp.Data;
using StackExchange.Redis;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file if it exists
Env.Load();

// Configure Redis for SignalR
var redisConnection = Environment.GetEnvironmentVariable("REDIS_URL");
if (!string.IsNullOrEmpty(redisConnection))
{
    // Parse Heroku Redis URL format: redis://h:password@host:port
    var uri = new Uri(redisConnection);
    var host = uri.Host;
    var redisPort = uri.Port;
    var userInfoParts = uri.UserInfo.Split(':');
    if (userInfoParts.Length != 2)
        throw new InvalidOperationException("REDIS_URL is not in the expected format 'redis://user:password@host:port'");
    var password = Uri.UnescapeDataString(userInfoParts[1]);
    var redisConfig = new ConfigurationOptions
    {
        EndPoints = { { host, redisPort } },
        Password = password,
        AbortOnConnectFail = false,
        ConnectRetry = 5,
        SyncTimeout = 5000,
        KeepAlive = 60,
        ConnectTimeout = 5000,
        Ssl = true,
        SslProtocols = System.Security.Authentication.SslProtocols.Tls12,
        AllowAdmin = true,
        ConfigCheckSeconds = 0
    };
    redisConfig.CertificateValidation += (sender, cert, chain, errors) => true;
    builder.Services.AddSignalR().AddStackExchangeRedis(options =>
    {
        options.Configuration = redisConfig;
        options.Configuration.ChannelPrefix = RedisChannel.Literal("NotepadApp_");
    });
}
else
{
    Console.WriteLine("Warning: REDIS_URL environment variable is not set, falling back to in-memory SignalR.");
    builder.Services.AddSignalR();
}

// Configure PostgreSQL
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl)) {

    // Parse Heroku PostgreSQL URL format: postgres://username:password@host:port/database
    var uri = new Uri(databaseUrl);
    var host = uri.Host;
    var dbPort = uri.Port;
    var database = uri.LocalPath.TrimStart('/');
    var credentials = uri.UserInfo.Split(':');
    var username = credentials[0];
    var password = credentials[1];
    var connectionString = $"Host={host};Port={dbPort};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true;";
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
} else {
    Console.WriteLine("Warning: DATABASE_URL not found, falling back to SQLite.");
    var fallbackDbPath = Path.Combine(AppContext.BaseDirectory, "local.db");
    var sqliteConnection = $"Data Source={fallbackDbPath}";
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(sqliteConnection));
}

builder.Services.AddRazorPages();

builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 7044;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Handle Heroku PORT environment variable
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    app.Urls.Add($"http://+:{port}");
}
else
{
    // In development, use both HTTP and HTTPS
    app.Urls.Add("http://localhost:5181");
    app.Urls.Add("https://localhost:7044");
    app.UseHttpsRedirection();
}

// Only force HTTPS in production
if (!app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        context.Request.Scheme = "https";
        await next();
    });
}

// Additional middleware
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.MapHub<NoteHub>("/noteHub");

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

app.Run();

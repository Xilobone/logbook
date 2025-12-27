using Logbook;
using Logbook.Data;
using Logbook.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("IdentityProvider"));

System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

builder.Services.AddAuthorization();
builder.Services.AddControllers();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("FrontEnd-DEV",
            policy =>
            {
                policy
                    .WithOrigins("http://localhost:5051")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
    });

    builder.Services.AddDbContext<LogbookDBContext>(options =>
    {
        options.UseSqlite(builder.Configuration["DBConnectionString"])
        .UseLazyLoadingProxies();
        options.LogTo(message => Logger.Log(message, Logger.LogLevel.Info, Logger.DBChannel));
    });
} else
{
    builder.Services.AddDbContext<LogbookDBContext>(options =>
    {
        options.UseMySql(builder.Configuration["DBConnectionString"], new MariaDbServerVersion(new Version(10, 11, 14)))
        .UseLazyLoadingProxies();
        options.LogTo(message => Logger.Log(message, Logger.LogLevel.Info, Logger.DBChannel));
    });
}

//add logger
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
builder.Logging.AddFilter("Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager", LogLevel.None);
Logger.Initialize(builder.Configuration.GetSection("Logger"));

builder.Services.Configure<RefreshConfig>("RefreshEvents",
    builder.Configuration.GetSection("RefreshEventsService"));
builder.Services.Configure<RefreshConfig>("RefreshCalendars",
    builder.Configuration.GetSection("RefreshCalendarsService"));

builder.Services.AddTransient<Logbook.Graph.GraphClientProvider>();
builder.Services.AddHostedService<RefreshEventsService>();
builder.Services.AddHostedService<RefreshCalendarsService>();

//This shoul be protected by a certificate, but then the certificate password will need to be stored somewhere still
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(builder.Configuration["KeyPath"]!))
    .SetApplicationName("Logbook");

builder.Services.Configure<ApplicationSettings>(builder.Configuration.GetSection("ApplicationSettings"));
var app = builder.Build();

EncryptionHelper.Init(app.Services.GetRequiredService<IDataProtectionProvider>());
app.UseCors("FrontEnd-DEV");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Logger.Log("Starting application");
app.Run();

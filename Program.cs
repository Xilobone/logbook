using Logbook;
using Logbook.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("IdentityProvider"));

System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

builder.Services.Configure<CalendarConfig>(builder.Configuration.GetSection("CalendarConfig"));

builder.Services.AddAuthorization();
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontEnd-DEV",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:5051")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<LogbookDBContext>(options =>
    options.UseSqlite(builder.Configuration["DBConnectionString"]));
}
else
{
    builder.Services.AddDbContext<LogbookDBContext>(options =>
    options.UseMySql(builder.Configuration["DBConnectionString"], new MariaDbServerVersion(new Version(10, 11, 14))));
}


//add logger
Logger.Initialize(builder.Configuration.GetSection("Logger"));

GraphClient.Initialize(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("FrontEnd-DEV");

app.MapControllers();

Logger.Log("Starting application");
app.Run();

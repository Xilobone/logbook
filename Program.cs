using logbook;
using Logbook;
using Logbook.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("IdentityProvider"));

System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

builder.Services.AddAuthorization();
builder.Services.AddControllers();

builder.Services.AddDbContext<LogbookDBContext>(options =>
    options.UseSqlite(builder.Configuration["DBConnectionString"]));

//add logger
Logger.Initialize(builder.Configuration.GetSection("Logger"));

GraphClient.Initialize(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Logger.Log("Starting application");
app.Run();

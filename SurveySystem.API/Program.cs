using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using SurveySystem.Core.Interfaces;
using SurveySystem.Infrastructure.Data;
using SurveySystem.Infrastructure.Repositories;
using SurveySystem.Infrastructure.Services;
using SurveySystem.Infrastructure.Extensions;
using System.Text;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure()
                )
            );
builder.Services.AddInfrastructureServices();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.WithOrigins("https://localhost:7018", "http://localhost:5203")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

//// Add Windows Authentication
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultScheme = "Windows";
//    options.DefaultChallengeScheme = "Windows";
//})
//.AddNegotiate() // Adds Windows Authentication (Kerberos/NTLM)
//// Add JWT bearer for API clients
//.AddJwtBearer("JwtBearer", options =>
//{
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = builder.Configuration["Jwt:Issuer"],
//        ValidAudience = builder.Configuration["Jwt:Audience"],
//        IssuerSigningKey = new SymmetricSecurityKey(
//            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
//    };
//});


//// Add Authorization
//builder.Services.AddAuthorization(options =>
//{
//    // Policies
//    options.AddPolicy("RequireAdministratorRole", policy =>
//        policy.RequireRole("Domain Admins", "SurveyAdmins"));

//    options.AddPolicy("RequireSurveyCreatorRole", policy =>
//        policy.RequireRole("Domain Admins", "SurveyAdmins", "SurveyCreators"));

//    options.AddPolicy("RequireAuthenticated", policy =>
//        policy.RequireAuthenticatedUser());
//});


// Core services
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowWebApp");

//app.UseAuthentication();
//app.UseAuthorization();

app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

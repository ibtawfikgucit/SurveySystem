using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using SurveySystem.Core.Interfaces;
//using SurveySystem.Core.Services;
using SurveySystem.Infrastructure.Data;
using SurveySystem.Infrastructure.Repositories;
using SurveySystem.Infrastructure.Services;
using SurveySystem.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure()
                )
            );
builder.Services.AddInfrastructureServices();
//builder.Services.AddInfrastructureServices(builder.Configuration);

//// Add Windows Authentication
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = "Windows";
//})
//.AddCookie(options =>
//{
//    options.LoginPath = "/Account/Login";
//    options.AccessDeniedPath = "/Account/AccessDenied";
//    options.Cookie.Name = "SurveySystem.Auth";
//    options.Cookie.HttpOnly = true;
//    options.ExpireTimeSpan = TimeSpan.FromHours(12);
//    options.SlidingExpiration = true;
//})
//.AddNegotiate() // Adds Windows Authentication (Kerberos/NTLM)
//.AddCookie("Windows"); // Cookie for Windows Authentication



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
//builder.Services.AddScoped<IActiveDirectoryService, ActiveDirectoryService>();


// Add Razor Pages and controllers
builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
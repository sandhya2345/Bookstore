using Microsoft.AspNetCore.Authentication.JwtBearer; // For JWT
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens; // For token validation
using OnlineBookStore.Models;
using OnlineBookStore.Services;
using System.Text;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<EmailService>();


// Add services to the container.
builder.Services.AddControllersWithViews();



// Database config
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// JWT secret key that is stored in appsetting.json
var jwtKey = builder.Configuration["JwtSettings:SecretKey"]; // Must match AccountController

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
var signingKey = new SymmetricSecurityKey(keyBytes);

// JWT authentication configured to extract token from cookies
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        // Read JWT from cookie (not from headers)
        OnMessageReceived = context =>
        {
            var token = context.Request.Cookies["jwt"];
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey
    };
});

// Authorization services
builder.Services.AddAuthorization();

var app = builder.Build();

// Production-friendly error handler
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

// Apply authentication & authorization
app.UseAuthentication();
app.UseAuthorization();

// MVC routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
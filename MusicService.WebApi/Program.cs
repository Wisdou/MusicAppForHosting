using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MusicService.DAL.PostgreSQL;
using MusicService.WebApi;
using MusicService.WebApi.Contracts.Validators;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Enums;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

var authOptions = new AuthOptions
{
    Audience = "music-service",
    Issuer = "music-service",
    SecretKey = "very-long-secret-key-with-letter-and-number3",
    ClockSkew = TimeSpan.FromMinutes(1),
    UsernameClaimName = "username",
    RoleClaimName = "role",
    UserIdClaimName = "user_id",
    EmailClaimName = "email",
    AccessTokenLifetime = TimeSpan.FromDays(5),
};

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Database");
ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

builder.Services.AddSingleton(authOptions);
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);;
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(swagger =>
{
    swagger.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "Jwt",
        In = ParameterLocation.Header,
    });

    swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddDbContext<MusicServiceDbContext>(e => e.UseNpgsql(connectionString));
builder.Services.AddValidatorsFromAssembly(typeof(SignInRequestValidator).Assembly);
builder.Services.AddFluentValidationAutoValidation(e =>
{
    e.EnableFormBindingSourceAutomaticValidation = true;
    e.ValidationStrategy = ValidationStrategy.All;
    e.EnableBodyBindingSourceAutomaticValidation = true;
});
builder.Services.AddSingleton(TimeProvider.System);

// NOTE: Keys stored in unencrypted form.
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/data-protection-keys"))
    .SetApplicationName("MusicService.WebApi");

builder.Services.AddAuthentication(e =>
    {
        e.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        e.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        e.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        e.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            RoleClaimType = authOptions.RoleClaimName,
            AuthenticationType = JwtBearerDefaults.AuthenticationScheme,
            NameClaimType = authOptions.UsernameClaimName,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = authOptions.Issuer,
            ValidAudience = authOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.SecretKey)),
            ClockSkew = authOptions.ClockSkew,
        };

        options.MapInboundClaims = false;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>
    {
        o.Events = new CookieAuthenticationEvents
        {
            OnRedirectToAccessDenied = e =>
            {
                e.Response.StatusCode = 403;
                return Task.CompletedTask;
            },
            OnRedirectToLogin = e =>
            {
                e.Response.StatusCode = 401;
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<MusicServiceDbContext>();
dbContext.Database.Migrate();
scope.Dispose();

app.UseCors(e => e
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using messaging.Application;
using messaging.Application.Interfaces;
using messaging.Application.Middleware;
using messaging.Application.Services;
using messaging.Domain.DTOs.Chat;
using messaging.Hubs;
using messaging.Infrastructure;
using messaging.Infrastructure.Extension;
using messaging.Infrastructure.Repositories;
using messaging.Infrastructure.Settings;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "Messaging API",
            Version = "v1",
            Description = "API for real-time chat with SignalR"
        }
    );
});
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

builder.Services.AddCors(options =>
{
    var frontendUrl = builder.Configuration.GetValue<string>("FrontendUrl");
    options.AddPolicy(
        "AllowFrontendAccess",
        builder =>
            builder
                .WithOrigins(
                    !string.IsNullOrEmpty(frontendUrl) ? frontendUrl : "http://localhost:5173"
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .WithExposedHeaders("WWW-Authenticate")
    );
});

builder
    .Services.AddAuthentication("Bearer")
    .AddJwtBearer(
        "Bearer",
        options =>
        {
            var jwtSettings =
                builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
                ?? throw new Exception("No JWT Settings");

            var key = jwtSettings.SecretKey;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true
            };
        }
    );

// Services & Repositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IChatRoomService, ChatRoomService>();

// Validation
builder.Services.AddValidatorsFromAssemblyContaining<MessageToSendDTOValidator>();
builder.Services.AddFluentValidationAutoValidation();

// EF DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

var app = builder.Build();
app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Messaging API v1");
    options.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseCors("AllowFrontendAccess");
app.UseAuthentication();
app.UseAuthorization();
app.MapOpenApi();
app.MapControllers();

app.MapGet(
    "/",
    context =>
    {
        context.Response.Redirect("/swagger");
        return Task.CompletedTask;
    }
);

app.MapHub<ChatHub>(
    "/chatHub",
    options =>
    {
        options.Transports = HttpTransportType.LongPolling;
        options.CloseOnAuthenticationExpiration = true;
        options.ApplicationMaxBufferSize = 64 * 1024;
        options.TransportMaxBufferSize = 64 * 1024;
    }
);

app.MigrateDb();

await app.RunAsync();

using FluentValidation;
using FluentValidation.AspNetCore;
using messaging.Application.Interfaces;
using messaging.Application.Services;
using messaging.Domain.DTOs.Chat;
using messaging.Hubs;
using messaging.Infrastructure;
using messaging.Infrastructure.Extension;
using messaging.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.EntityFrameworkCore;
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

builder.Services.AddControllers();


builder.Services.AddCors(options =>
{
    var frontendUrl = builder.Configuration.GetValue<string>("FrontendUrl");
    options.AddPolicy("AllowFrontendAccess", builder => builder
        .WithOrigins(!string.IsNullOrEmpty(frontendUrl) ? frontendUrl : "http://localhost:5173")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
    );
});

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddCors(options =>
{
    // var frontendUrl = builder.Configuration.GetValue<string>("FrontendUrl");
    options.AddPolicy(
        "AllowFrontendAccess",
        builder =>
        {
            builder
                .WithOrigins(
                    // !string.IsNullOrEmpty(frontendUrl) ? frontendUrl :
                    "http://localhost:5173"
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        }
    );
});

// Services & Repositories
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IChatService, ChatService>();

// Validation
builder.Services.AddValidatorsFromAssemblyContaining<MessageToSendDTOValidator>();
builder.Services.AddFluentValidationAutoValidation();

// EF DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

var app = builder.Build();

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Messaging API v1");
        options.RoutePrefix = string.Empty;
    });
}
app.UseCors("AllowFrontendAccess");

app.MapOpenApi();

app.UseHttpsRedirection();
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
        options.Transports = HttpTransportType.WebSockets;
        options.CloseOnAuthenticationExpiration = true;
        options.ApplicationMaxBufferSize = 64 * 1024;
        options.TransportMaxBufferSize = 64 * 1024;
    }
);

app.MigrateDb();

await app.RunAsync();

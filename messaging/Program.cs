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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
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

builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IChatService, ChatService>();

builder.Services.AddValidatorsFromAssemblyContaining<MessageToSendDTOValidator>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

var app = builder.Build();

app.UseCors("AllowFrontendAccess");

app.MapOpenApi();
app.MapControllers();
app.UseHttpsRedirection();

app.MapHub<ChatHub>(
    "/chatHub",
    options =>
    {
        options.Transports = HttpTransportType.WebSockets;
        options.CloseOnAuthenticationExpiration = true;
        options.ApplicationMaxBufferSize = 64 * 1024; // 64KB
        options.TransportMaxBufferSize = 64 * 1024;
    }
);

app.MigrateDb();

await app.RunAsync();

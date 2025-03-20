using System.Text;
using COEN390_Server.Services;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<MyDbContext>();
builder.Services.AddSingleton<NotificationService>();
builder.Services.AddSingleton<SettingsUpdateService>();

builder.Services.AddControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidIssuer = "yourdomain.com",
            ValidAudience = "yourdomain.com",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSecret"]!))
        };

        options.Events = new JwtBearerEvents {
            OnMessageReceived = context => {
                if (context.Request.Headers.ContainsKey("sec-websocket-protocol") && context.HttpContext.WebSockets.IsWebSocketRequest) {
                    var token = context.Request.Headers["sec-websocket-protocol"].ToString();
                    context.Token = token.Substring(token.IndexOf(',') + 1).Trim();
                    context.Request.Headers["sec-websocket-protocol"] = "client";
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme() {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement { {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope()) {
    var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseWebSockets();
app.Map("/websockets", async context => { 
    if (context.WebSockets.IsWebSocketRequest) {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var wsGuid = Guid.NewGuid();
        var settingsService = context.RequestServices.GetRequiredService<SettingsUpdateService>();
        settingsService.AddWebsocket(wsGuid, webSocket);
        await settingsService.Listen(wsGuid, webSocket);
    } else {
        context.Response.StatusCode = 400;
    }
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

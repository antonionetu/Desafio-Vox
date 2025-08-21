using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Vox.API.Hubs;
using Vox.API.Middlewares;
using Vox.API.signalR;
using Vox.Application.Handlers;
using Vox.Application.Services;
using Vox.Infrastructure.Data;
using Vox.Infrastructure.Repositories;
using Vox.Domain.Factories;
using Vox.Infrastructure.Utils;

var builder = WebApplication.CreateBuilder(args);
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET") ?? jwtSettings["SecretKey"];

if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
    throw new Exception("JWT_SECRET inválido ou muito curto. Deve ter no mínimo 32 bytes.");

var key = Encoding.ASCII.GetBytes(secretKey);

// --- Redis e Cache ---
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    options.InstanceName = "VoxCache:";
});
builder.Services.AddSingleton<CacheManager>();

// --- Fila ---
builder.Services.AddSingleton<IQueueManager, QueueManager>();

// --- DbContext ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// --- Controllers ---
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        );
    });

// --- Repositórios ---
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IPacienteRepository, PacienteRepository>();
builder.Services.AddScoped<IMedicoRepository, MedicoRepository>();
builder.Services.AddScoped<IHorarioRepository, HorarioRepository>();
builder.Services.AddScoped<IConsultaRepository, ConsultaRepository>();
builder.Services.AddScoped<INotificacaoRepository, NotificacaoRepository>();

// --- Serviços ---
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IPacienteService, PacienteService>();
builder.Services.AddScoped<IMedicoService, MedicoService>();
builder.Services.AddScoped<IHorarioService, HorarioService>(); 
builder.Services.AddScoped<IConsultaService, ConsultaService>();
builder.Services.AddScoped<INotificacaoService, NotificacaoService>();

// --- Factories ---
builder.Services.AddScoped<PacienteFactory>();
builder.Services.AddScoped<MedicoFactory>();
builder.Services.AddScoped<UsuarioFactory>();
builder.Services.AddScoped<HorarioFactory>();
builder.Services.AddScoped<ConsultaFactory>();

// --- Handlers ---
builder.Services.AddScoped<AutenticacaoHandler>();
builder.Services.AddScoped<MedicoHandler>();
builder.Services.AddScoped<PacienteHandler>();
builder.Services.AddScoped<HorarioHandler>();
builder.Services.AddScoped<ConsultaHandler>();

// --- SignalR ---
builder.Services.AddSignalR();

// --- Autenticação JWT ---
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            RoleClaimType = "tipo"
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hub/horarios"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            },

            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    var result = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        error = "invalid_token",
                        error_description = "O token expirou."
                    });
                    return context.Response.WriteAsync(result);
                }
                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddAuthorization();
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

// --- Cors ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// --- Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("desafio", new OpenApiInfo { Title = "Vox API", Version = "desafio" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o seu token JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference 
                { 
                    Type = ReferenceType.SecurityScheme, 
                    Id = "Bearer" 
                }
            },
            new string[] {}
        }
    });

    c.DocumentFilter<Vox.API.Swagger.CustomSchemaFilter>();

    c.TagActionsBy(api =>
    {
        var groupName = api.ActionDescriptor.EndpointMetadata
            .OfType<ApiExplorerSettingsAttribute>()
            .FirstOrDefault()?.GroupName;

        return new[] { groupName ?? api.ActionDescriptor.RouteValues["controller"] };
    });

    c.DocInclusionPredicate((docName, apiDesc) => true);
});

var app = builder.Build();

// --- Swagger em /api/docs ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "api/docs/{documentName}/swagger.json";
    });

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/api/docs/desafio/swagger.json", "Vox API");
        c.RoutePrefix = "api/docs";
        c.DefaultModelsExpandDepth(-1);
    });
}

// --- Cors ---
app.UseCors("AllowFrontend");

// --- Middlewares ---
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TokenMiddleware>();

// --- Controllers ---
app.MapControllers();

// --- SignalR ---
app.MapHub<NotificacaoHub>("/hub/notificacoes");
app.MapHub<HorarioHub>("/hub/horarios");

app.Run();

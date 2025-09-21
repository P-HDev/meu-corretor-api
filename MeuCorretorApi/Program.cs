using Dominio.Interfaces;
using Service.Interfaces;
using InfraEstrutura.ContextoBancoPsql;
using InfraEstrutura.Repositories;
using Microsoft.EntityFrameworkCore;
using Service;
using InfraEstrutura.Storage;
using System;
using System.IO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.HttpOverrides;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Config Forwarded Headers (Azure App Service já envia X-Forwarded-Proto)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Limpa listas para aceitar proxies conhecidos do Azure
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddDbContext<ContextoDb>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IImovelRepository, ImovelRepository>();
builder.Services.AddScoped<IImovelService, ImovelService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<IImageStorage, LocalImageStorage>();
builder.Services.AddHttpContextAccessor();

var allowedOrigins = new[] {
    "http://localhost:4200",
    "https://salmon-wave-0a229740f.1.azurestaticapps.net"
};
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(allowedOrigins)
              .WithExposedHeaders("Location")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

builder.Services.AddControllers();

// JWT Auth
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key não configurado");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MeuCorretorApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? jwtIssuer;
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.RequireHttpsMetadata = false; // ajustar para true em prod https
    o.SaveToken = true;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ClockSkew = TimeSpan.FromSeconds(30)
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly);
    foreach (var xml in xmlFiles)
    {
        c.IncludeXmlComments(xml, includeControllerXmlComments: true);
    }
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT no formato: Bearer {seu_token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            }, new string[] {}
        }
    });
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Aplica migrations apenas se configurado (evita lógica complexa de baseline)
using (var scope = app.Services.CreateScope())
{
    var autoMigrate = builder.Configuration.GetValue<bool>("AutoMigrate");
    if (autoMigrate)
    {
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<ContextoDb>();
            db.Database.Migrate();
            Console.WriteLine("[Migrations] Banco atualizado.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Migrations][ERRO] Falha ao aplicar migrations: {ex.Message}");
        }
    }
    else
    {
        Console.WriteLine("[Migrations] AutoMigrate desabilitado (defina AutoMigrate=true em config para habilitar).");
    }
}

// Usa forwarded headers antes de redirecionamentos / auth
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    // HSTS apenas em produção
    app.UseHsts();
}

app.UseStaticFiles();
app.UseCors("Frontend");

// Swagger em dev ou se habilitado explicitamente em produção
var enableSwaggerInProd = builder.Configuration.GetValue<bool>("Swagger:EnableInProduction");
if (app.Environment.IsDevelopment() || enableSwaggerInProd)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MeuCorretorApi v1");
        c.RoutePrefix = "swagger"; // /swagger
    });
}

app.UseHttpsRedirection();
app.Run();

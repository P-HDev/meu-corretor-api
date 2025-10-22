using InfraEstrutura.ContextoBancoPsql;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service;
using Dominio.Interfaces;
using InfraEstrutura.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.HttpOverrides;

namespace MeuCorretorApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddForwardedHeadersConfig(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(o =>
        {
            o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            o.KnownNetworks.Clear();
            o.KnownProxies.Clear();
        });
        return services;
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<ContextoDb>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IImovelService, ImovelService>();
        services.AddScoped<IAutenticacaoService, AutenticacaoService>();
        services.AddHttpContextAccessor();
        return services;
    }

    public static IServiceCollection AddBlobStorage(this IServiceCollection services, IConfiguration config)
    {
        var sasUrl = config["AzureStorage:SasBlobUrl"];
        if (!string.IsNullOrWhiteSpace(sasUrl))
        {
            var uri = new Uri(sasUrl);
            var path = uri.AbsolutePath.Trim('/');
            if (!string.IsNullOrEmpty(path))
            {
                var containerClient = new Azure.Storage.Blobs.BlobContainerClient(uri);
                services.AddSingleton(containerClient);
                services.AddSingleton<IImageStorage>(sp => new ContainerSasImageStorage(sp.GetRequiredService<Azure.Storage.Blobs.BlobContainerClient>()));
                return services;
            }
            
            services.AddSingleton(new Azure.Storage.Blobs.BlobServiceClient(uri));
            services.AddSingleton<IImageStorage>(sp =>
            {
                var client = sp.GetRequiredService<Azure.Storage.Blobs.BlobServiceClient>();
                var container = config["AzureStorage:ContainerName"] ?? "imagens";
                return new AzureBlobImageStorage(client, container);
            });
            return services;
        }

        var connStr = config.GetConnectionString("AzureBlobStorage");
        if (!string.IsNullOrWhiteSpace(connStr))
        {
            services.AddSingleton(new Azure.Storage.Blobs.BlobServiceClient(connStr));
            services.AddSingleton<IImageStorage>(sp =>
            {
                var client = sp.GetRequiredService<Azure.Storage.Blobs.BlobServiceClient>();
                var container = config["AzureStorage:ContainerName"] ?? "imagens";
                return new AzureBlobImageStorage(client, container);
            });
            return services;
        }
        throw new InvalidOperationException("Configure AzureStorage:SasBlobUrl ou ConnectionStrings:AzureBlobStorage");
    }

    public static IServiceCollection AddCorsPolicies(this IServiceCollection services, IConfiguration config)
    {
        var origins = config.GetSection("Cors:Origins").Get<string[]>() ?? Array.Empty<string>();
        services.AddCors(o =>
        {
            o.AddPolicy("Frontend", p =>
                p.WithOrigins(origins)
                 .WithExposedHeaders("Location")
                 .AllowAnyHeader()
                 .AllowAnyMethod()
                 .AllowCredentials());
        });
        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var jwtKey = config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key não configurado");
        var jwtIssuer = config["Jwt:Issuer"] ?? "MeuCorretorApi";
        var jwtAudience = config["Jwt:Audience"] ?? jwtIssuer;
        var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

        services.AddAuthentication(o =>
        {
            o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(o =>
        {
            o.RequireHttpsMetadata = false;
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
        return services;
    }

    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly);
            foreach (var xml in xmlFiles)
                c.IncludeXmlComments(xml, true);
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "POST /api/Autenticacao/login -> copie token -> Authorize -> Bearer {token}"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    }, Array.Empty<string>()
                }
            });
        });
        return services;
    }
}
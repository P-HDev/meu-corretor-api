using MeuCorretorApi.Extensions;
using InfraEstrutura.ContextoBancoPsql;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddForwardedHeadersConfig()
    .AddPersistence(builder.Configuration)
    .AddApplicationServices()
    .AddBlobStorage(builder.Configuration)
    .AddCorsPolicies(builder.Configuration)
    .AddJwtAuthentication(builder.Configuration)
    .AddSwaggerWithJwt();

builder.Services.AddControllers();

var app = builder.Build();

app.UseForwardedHeaders();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

if (builder.Configuration.GetValue<bool>("AutoMigrate"))
{
    using var scope = app.Services.CreateScope();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ContextoDb>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Migrations][ERRO] {ex.Message}");
    }
}

if (!app.Environment.IsDevelopment())
    app.UseHsts();

var enableSwagger = app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:EnableInProduction");
if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MeuCorretorApi v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
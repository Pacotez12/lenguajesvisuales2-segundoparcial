using LenguajesVisualesII.Api.Data;
using LenguajesVisualesII.Api.Middleware;
using LenguajesVisualesII.Api.Services;
using LenguajesVisualesII.Api.Swagger;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger + soporte para archivos
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<FormFileOperationFilter>();
});

// Base de datos SQL Server
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Logs en BD
builder.Services.AddScoped<ILogService, LogService>();

var app = builder.Build();

// ✅ Migraciones al arrancar SIN tumbar la aplicación
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error applying migrations on startup.");
        // ❌ Nunca usar throw en hosting compartido
    }
}

// Archivos estáticos
app.UseStaticFiles();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// MonsterASP NO soporta HTTPS interno
// app.UseHttpsRedirection();

// Autorización
app.UseAuthorization();

// Middleware de logs personalizados
app.UseMiddleware<RequestResponseLoggingMiddleware>();

// Manejo de errores
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.MapGet("/error", () =>
    {
        return Results.Problem("An error occurred.");
    });
}

// Controllers
app.MapControllers();

app.Run();

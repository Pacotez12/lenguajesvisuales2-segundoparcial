using LenguajesVisualesII.Api.Data;
using LenguajesVisualesII.Api.Middleware;
using LenguajesVisualesII.Api.Services;
using LenguajesVisualesII.Api.Swagger;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<FormFileOperationFilter>();
});

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddScoped<ILogService, LogService>();

var app = builder.Build();

// Aplicar migraciones / crear BD al arrancar (usa scope)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<AppDbContext>();
        // Aplica migraciones pendientes y crea la BD si no existe
        await db.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetService<ILogger<Program>>();
        logger?.LogError(ex, "Error aplicando migraciones/creando la base de datos al arrancar.");
        throw;
    }
}

app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();

app.UseMiddleware<RequestResponseLoggingMiddleware>();

// Configuración del manejo de errores
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.MapGet("/error", (HttpContext http) =>
    {
        return Results.Problem("An error occurred.");
    });
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseStaticFiles(); 

app.UseSwagger();

app.MapControllers();

app.Run();

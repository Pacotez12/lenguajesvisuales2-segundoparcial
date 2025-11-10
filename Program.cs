using System.IO;
using LenguajesVisualesII.Api.Data;
using LenguajesVisualesII.Api.Middleware;
using LenguajesVisualesII.Api.Services;
using LenguajesVisualesII.Api.Swagger;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<FormFileOperationFilter>();
});

// DbContext
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Servicios
builder.Services.AddScoped<ILogService, LogService>();

var app = builder.Build();

// Migraciones automáticas (NO romper el arranque si falla)
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error applying migrations on startup (continuing).");
        // No throw: si no hay SQLClient o la DB no responde, que igual levante la app
    }
}

// Static files (asegurar wwwroot/uploads)
var uploadSubFolder = builder.Configuration["StaticFiles:UploadRoot"] ?? "uploads";
var uploadsPhysical = Path.Combine(app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot"), uploadSubFolder);
Directory.CreateDirectory(uploadsPhysical);

app.UseStaticFiles(); // sirve wwwroot/*
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPhysical),
    RequestPath = "/" + uploadSubFolder.Trim('/', '\\')
});

// Swagger (opcional mostrar siempre)
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Redirect("/swagger"));

// NADA de HTTPS redirection aquí (hosting compartido + http)
// app.UseHttpsRedirection();

app.UseAuthorization();

// middleware de logging custom
app.UseMiddleware<RequestResponseLoggingMiddleware>();

// Manejo de errores en producción
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.MapGet("/error", () => Results.Problem("An error occurred."));
}

app.MapControllers();

app.Run();

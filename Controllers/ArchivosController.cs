using ICSharpCode.SharpZipLib.Zip;
using LenguajesVisualesII.Api.Data;
using LenguajesVisualesII.Api.Models;
using LenguajesVisualesII.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LenguajesVisualesII.Api.Controllers;
 
[ApiController]
[Route("api/[controller]")]
public class ArchivosController : ControllerBase
{
    private readonly AppDbContext db;
    private readonly IWebHostEnvironment env;
    private readonly IConfiguration cfg;

    public ArchivosController(AppDbContext db, IWebHostEnvironment env, IConfiguration cfg)
    {
        this.db = db; this.env = env; this.cfg = cfg;
    }

    /// <summary>Sube un .zip, descomprime en wwwroot/uploads/{CI}/ y registra archivos</summary>
    [HttpPost("upload-zip")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadZip(ZipUploadDto dto)
    {
        var zip = dto.Zip;
        var ci = dto.Ci;

        if (zip is null || zip.Length == 0) return BadRequest("Archivo .zip requerido");
        if (string.IsNullOrWhiteSpace(ci)) return BadRequest("Campo 'Ci' requerido");

        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.CI == ci);
        if (cliente is null) return NotFound($"Cliente CI {ci} no existe");

        var uploadRoot = cfg.GetValue<string>("StaticFiles:UploadRoot") ?? "wwwroot/uploads";
        var destDir = Path.Combine(env.ContentRootPath, uploadRoot, ci);
        Directory.CreateDirectory(destDir);

        var tempZip = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");

        try
        {
            using (var fs = System.IO.File.Create(tempZip))
                await zip.CopyToAsync(fs);

            using var zipStream = new ZipInputStream(System.IO.File.OpenRead(tempZip));
            ZipEntry? entry;
            while ((entry = zipStream.GetNextEntry()) != null)
            {
                if (entry.IsDirectory) continue;

                var entryName = entry.Name.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
                var outPath = Path.GetFullPath(Path.Combine(destDir, entryName));

                if (!outPath.StartsWith(Path.GetFullPath(destDir), StringComparison.OrdinalIgnoreCase))
                    continue;

                Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
                using var outFile = System.IO.File.Create(outPath);
                zipStream.CopyTo(outFile);

                var relativeUrl = $"/uploads/{ci}/{entry.Name}".Replace('\\', '/');
                db.ArchivosCliente.Add(new ArchivoCliente
                {
                    CICliente = ci,
                    NombreArchivo = entry.Name,
                    UrlArchivo = relativeUrl
                });
            }

            await db.SaveChangesAsync();
            return Ok(new { message = "Archivos cargados y registrados" });
        }
        finally
        {
            try { if (System.IO.File.Exists(tempZip)) System.IO.File.Delete(tempZip); } catch { }
        }
    }

    [HttpGet("by-cliente/{ci}")]
    public async Task<ActionResult<IEnumerable<ArchivoCliente>>> GetByCliente(string ci)
    {
        var list = await db.ArchivosCliente.Where(a => a.CICliente == ci).ToListAsync();
        return list;
    }
}

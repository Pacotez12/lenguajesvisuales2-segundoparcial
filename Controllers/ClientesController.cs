using LenguajesVisualesII.Api.Data;
using LenguajesVisualesII.Api.DTOs;
using LenguajesVisualesII.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LenguajesVisualesII.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController(AppDbContext db) : ControllerBase
{
    /// <summary>Registra un cliente con fotos opcionales</summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Cliente), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(ClienteCreateDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var exists = await db.Clientes.AnyAsync(c => c.CI == dto.CI);
        if (exists) return Conflict($"Ya existe un cliente con CI {dto.CI}.");

        async Task<byte[]?> ToBytes(IFormFile? f)
        {
            if (f == null) return null;
            using var ms = new MemoryStream();
            await f.CopyToAsync(ms);
            return ms.ToArray();
        }

        var cliente = new Cliente
        {
            CI = dto.CI,
            Nombres = dto.Nombres,
            Direccion = dto.Direccion,
            Telefono = dto.Telefono,
            FotoCasa1 = await ToBytes(dto.FotoCasa1),
            FotoCasa2 = await ToBytes(dto.FotoCasa2),
            FotoCasa3 = await ToBytes(dto.FotoCasa3)
        };

        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByCI), new { ci = cliente.CI }, cliente);
    }

    [HttpGet("{ci}")]
    public async Task<ActionResult<Cliente>> GetByCI(string ci)
    {
        var cliente = await db.Clientes.Include(c => c.Archivos).FirstOrDefaultAsync(c => c.CI == ci);
        if (cliente == null) return NotFound();
        return cliente;
    }
}

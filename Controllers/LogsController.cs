using LenguajesVisualesII.Api.Data;
using LenguajesVisualesII.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LenguajesVisualesII.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LogsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LogApi>>> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 200) pageSize = 50;

        var query = db.LogApis.AsNoTracking().OrderByDescending(l => l.IdLog);
        var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        var total = await db.LogApis.CountAsync();

        return Ok(new { total, page, pageSize, data });
    }
}

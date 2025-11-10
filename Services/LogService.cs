using LenguajesVisualesII.Api.Data;
using LenguajesVisualesII.Api.Models;

namespace LenguajesVisualesII.Api.Services;

public class LogService : ILogService
{
    private readonly AppDbContext _db;

    public LogService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<int> SaveAsync(LogApi log, CancellationToken ct = default)
    {
        await _db.LogApis.AddAsync(log, ct);
        await _db.SaveChangesAsync(ct);
        return log.IdLog;
    }
}

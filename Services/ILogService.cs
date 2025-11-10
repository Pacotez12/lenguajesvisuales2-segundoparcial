using LenguajesVisualesII.Api.Models;

namespace LenguajesVisualesII.Api.Services;

public interface ILogService
{
    Task<int> SaveAsync(LogApi log, CancellationToken ct = default);
}

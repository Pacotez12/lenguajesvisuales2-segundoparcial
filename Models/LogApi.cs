using System;
using System.ComponentModel.DataAnnotations;

namespace LenguajesVisualesII.Api.Models;

public class LogApi
{
    [Key]
    public int IdLog { get; set; }

    // Fecha y hora en UTC por defecto
    public DateTime DateTime { get; set; } = DateTime.UtcNow;

    [MaxLength(30)]
    public string? TipoLog { get; set; }

    public string? RequestBody { get; set; }
    public string? ResponseBody { get; set; }

    [MaxLength(500)]
    public string? UrlEndpoint { get; set; }

    [MaxLength(10)]
    public string? MetodoHttp { get; set; }

    [MaxLength(100)]
    public string? DireccionIp { get; set; }

    public string? Detalle { get; set; }
}

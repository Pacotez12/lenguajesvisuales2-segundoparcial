using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LenguajesVisualesII.Api.Models;

public class ArchivoCliente
{
    [Key]
    public int IdArchivo { get; set; }

    [Required, MaxLength(20)]
    public string CICliente { get; set; } = default!;

    [ForeignKey(nameof(CICliente))]
    public Cliente? Cliente { get; set; }

    [Required, MaxLength(255)]
    public string NombreArchivo { get; set; } = default!;

    [Required, MaxLength(500)]
    public string UrlArchivo { get; set; } = default!;

    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}

using System.ComponentModel.DataAnnotations;

namespace LenguajesVisualesII.Api.Models;

public class Cliente
{
    [Key]
    [MaxLength(20)]
    public string CI { get; set; } = default!;

    [Required, MaxLength(120)]
    public string Nombres { get; set; } = default!;

    [Required, MaxLength(200)]
    public string Direccion { get; set; } = default!;

    [Required, MaxLength(30)]
    public string Telefono { get; set; } = default!;

    public byte[]? FotoCasa1 { get; set; }
    public byte[]? FotoCasa2 { get; set; }
    public byte[]? FotoCasa3 { get; set; }

    public ICollection<ArchivoCliente> Archivos { get; set; } = new List<ArchivoCliente>();
}

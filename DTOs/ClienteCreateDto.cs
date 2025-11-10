using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LenguajesVisualesII.Api.DTOs;

public class ClienteCreateDto
{
    [Required, MaxLength(20)]
    public string CI { get; set; } = default!;
    [Required, MaxLength(120)]
    public string Nombres { get; set; } = default!;
    [Required, MaxLength(200)]
    public string Direccion { get; set; } = default!;
    [Required, MaxLength(30)]
    public string Telefono { get; set; } = default!;

    public IFormFile? FotoCasa1 { get; set; }
    public IFormFile? FotoCasa2 { get; set; }
    public IFormFile? FotoCasa3 { get; set; }
}

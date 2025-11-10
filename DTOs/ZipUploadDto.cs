using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LenguajesVisualesII.Api.DTOs;

public class ZipUploadDto
{
    [Required]
    public IFormFile Zip { get; set; } = default!;

    [Required]
    public string Ci { get; set; } = default!;
}

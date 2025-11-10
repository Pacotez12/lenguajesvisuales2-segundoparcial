# LenguajesVisuales2Parcial (API .NET 8 + SQL Server)

Proyecto final listo para correr con Swagger y endpoints que **funcionan** (incluida la carga de `.zip` sin romper Swagger).

## Pasos
1. Ajusta la cadena en `appsettings.json` (SQL Server).
2. Restaura/compila:
   ```bash
   dotnet restore
   dotnet build
   ```
3. Crea DB (Code First):
   ```bash
   dotnet tool install --global dotnet-ef
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```
4. Ejecuta:
   ```bash
   dotnet run
   ```
   Swagger: `http://localhost:5123/swagger`

## Endpoints
- `POST /api/clientes` (**multipart/form-data**): CI, Nombres, Direccion, Telefono, FotoCasa1?, FotoCasa2?, FotoCasa3?
- `GET /api/clientes/{ci}`
- `POST /api/archivos/upload-zip` (**multipart/form-data**): campos `Zip` (archivo) y `Ci` (texto)
- `GET /api/archivos/by-cliente/{ci}`
- `GET /api/logs`

## Fix aplicado a Swagger
- Para subir archivos se usa `[Consumes("multipart/form-data")]` y un **DTO** (`ZipUploadDto`) con `IFormFile Zip` + `string Ci`.
- **No** se declara `IFormFile` directo con `[FromForm]` en la firma del método; en su lugar se recibe el DTO con `[FromForm]`, lo que evita el error de Swashbuckle.

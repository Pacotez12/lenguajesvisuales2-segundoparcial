# 📌 Lenguajes Visuales II – Segundo Parcial  
### API REST en ASP.NET Core (.NET 8) + SQL Server + Entity Framework Core

Este proyecto corresponde al segundo parcial de la materia **Lenguajes Visuales II (UNINORTE)**.  
Consiste en el desarrollo de una **Web API REST** que permite la gestión de clientes, la carga de archivos asociados y el registro de logs del sistema.  
El enfoque utilizado es **Code First** con Entity Framework Core y base de datos **SQL Server**.

---

## ✅ Objetivo General
Evaluar la capacidad del estudiante para construir una aplicación Web API completa, aplicando buenas prácticas de desarrollo, configuración y despliegue.

---

## ✅ Tecnologías Utilizadas
- **ASP.NET Core Web API (.NET 8.0)**
- **Entity Framework Core 8 – Code First**
- **SQL Server 2019 / 2022**
- **Swagger (Swashbuckle)** para documentación de endpoints
- **Middleware personalizado** para registro de logs
- **SharpZipLib** para descompresión de archivos ZIP
- **C# 12**
- **Render (hosting de la API)** *(para despliegue)*
- **Railway (hosting SQL Server)** *(para despliegue)*

---

## ✅ Estructura del Proyecto

```
/Controllers
    ArchivosController.cs
    ClientesController.cs
    LogsController.cs

/Models
    Cliente.cs
    ArchivoCliente.cs
    LogApi.cs

/DTOs
    ClienteCreateDto.cs
    ZipUploadDto.cs

/Data
    AppDbContext.cs

/Middleware
    RequestResponseLoggingMiddleware.cs
```

---

## ✅ Requerimientos Cubiertos

### ✔ Requerimiento 1 – Registro de Clientes
- POST para registrar clientes con:
  - CI  
  - Nombres  
  - Dirección  
  - Teléfono  
  - FotoCasa1 / FotoCasa2 / FotoCasa3  
- Fotos almacenadas en disco y rutas registradas en la base de datos.
- Validación de campos obligatorios.

### ✔ Requerimiento 2 – Carga de Múltiples Archivos
- Servicio que recibe un archivo **ZIP**.
- Descomprime su contenido.
- Guarda archivos en `wwwroot/uploads/{ci}/`.
- Registra cada archivo en la tabla **ArchivoCliente**.

### ✔ Requerimiento 3 – Logs y Seguimiento
- Middleware registra:
  - Request  
  - Response  
  - Método HTTP  
  - URL  
  - IP  
  - Fecha  
  - Body de request y response  
- Registros almacenados en la tabla `LogApi`.
- Endpoint GET para consultar logs.

### ✔ Requerimiento 4 – Hosting
Preparado para despliegue en **Render + Railway**.

### ✔ Requerimiento 5 – GitHub
Este repositorio incluye:
- Código fuente completo  
- README documentado  
- Proyecto funcional  

### ✔ Requerimiento 6 – Evidencias
Se deben agregar capturas de pantalla de pruebas en Swagger o Postman.

---

## ✅ Configuración Local

### 📌 1. Cadena de conexión
Editar `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=LV2SegParcial;User Id=sa;Password=tu_password;TrustServerCertificate=true;"
  }
}
```

### 📌 2. Aplicar migraciones

```
update-database
```

### 📌 3. Ejecutar la API

```
dotnet run
```

### 📌 4. Acceder a Swagger

```
http://localhost:5123/swagger
```

---

## ✅ Despliegue en Hosting

### Hosting recomendado:
- **Render** → API  
- **Railway** → SQL Server remoto

Variables de entorno necesarias:

```
ASPNETCORE_ENVIRONMENT = Production
ConnectionStrings__DefaultConnection = <cadena de Railway>
StaticFiles__UploadRoot = wwwroot/uploads
```

---

## ✅ Autor
**Marco Andrés Brizuela Godoy**  
Ingeniería Informática – UNINORTE  
Año 2025

---

## ✅ Licencia
Proyecto académico. Uso estrictamente educativo.


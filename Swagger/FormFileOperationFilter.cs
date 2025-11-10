using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LenguajesVisualesII.Api.Swagger;

public class FormFileOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Encuentra parámetros que sean DTOs con propiedades IFormFile / IEnumerable<IFormFile>
        var fileProps = new Dictionary<string, OpenApiSchema>();

        foreach (var param in context.MethodInfo.GetParameters())
        {
            var type = param.ParameterType;
            if (type == typeof(IFormFile) || type == typeof(IEnumerable<IFormFile>)) // caso directo (raro)
            {
                operation.RequestBody ??= new OpenApiRequestBody();
                operation.RequestBody.Content["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            [param.Name] = CreateFileSchema(type)
                        }
                    }
                };
                return;
            }

            if (!type.IsClass || type == typeof(string)) continue;

            // inspecciona propiedades del DTO
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.PropertyType == typeof(IFormFile))
                {
                    fileProps[prop.Name] = CreateFileSchema(prop.PropertyType);
                }
                else if (prop.PropertyType == typeof(IEnumerable<IFormFile>) || prop.PropertyType == typeof(IFormFile[]))
                {
                    fileProps[prop.Name] = new OpenApiSchema
                    {
                        Type = "array",
                        Items = CreateFileSchema(typeof(IFormFile))
                    };
                }
            }
        }

        if (!fileProps.Any()) return;

        // Construye requestBody multipart/form-data con las propiedades detectadas
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>()
        };

        // añade las propiedades de archivo
        foreach (var kvp in fileProps)
            schema.Properties[kvp.Key] = kvp.Value;

        // intenta añadir también propiedades simples (string, int) del DTO para completar el formulario
        // Recolecta de primer parámetro DTO
        var dtoParam = context.MethodInfo.GetParameters().FirstOrDefault(p => p.ParameterType.IsClass && p.ParameterType != typeof(string));
        if (dtoParam != null)
        {
            foreach (var prop in dtoParam.ParameterType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (schema.Properties.ContainsKey(prop.Name)) continue;
                var t = prop.PropertyType;
                if (t == typeof(string))
                    schema.Properties[prop.Name] = new OpenApiSchema { Type = "string" };
                else if (t == typeof(int) || t == typeof(long))
                    schema.Properties[prop.Name] = new OpenApiSchema { Type = "integer", Format = "int32" };
                else if (t == typeof(bool))
                    schema.Properties[prop.Name] = new OpenApiSchema { Type = "boolean" };
            }
        }

        operation.Parameters?.Clear();
        operation.RequestBody = new OpenApiRequestBody
        {
            Content =
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = schema
                }
            }
        };
    }

    private OpenApiSchema CreateFileSchema(System.Type t)
    {
        return new OpenApiSchema
        {
            Type = "string",
            Format = "binary"
        };
    }
}
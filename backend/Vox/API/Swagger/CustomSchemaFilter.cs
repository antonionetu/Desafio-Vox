using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Vox.API.Swagger
{
    public class CustomSchemaFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (swaggerDoc.Components.Schemas.ContainsKey("DTOs.PacienteOutputDTO"))
            {
                var schema = swaggerDoc.Components.Schemas["DTOs.PacienteOutputDTO"];
                if (schema.Properties.ContainsKey("Nome"))
                {
                    schema.Properties["Nome"].Description = "Nome completo do paciente";
                    schema.Properties["Nome"].Nullable = false;
                }
                if (schema.Properties.ContainsKey("Id"))
                {
                    schema.Properties["Id"].ReadOnly = true;
                }
            }
        }
    }
}

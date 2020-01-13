using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Rocket.Surgery.Operational.Swashbuckle {
    class ProblemDetailsSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (typeof(ProblemDetails).IsAssignableFrom(context.Type))
            {
                schema.AdditionalPropertiesAllowed = true;
                schema.Properties.Remove(nameof(ProblemDetails.Extensions));
                schema.Properties.Remove("extensions");
            }
        }
    }
}
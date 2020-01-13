using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Extensions.DependencyInjection;
using Rocket.Surgery.Operational.Swashbuckle.NewtonsoftJson;
using Swashbuckle.AspNetCore.SwaggerGen;

[assembly: Convention(typeof(SwashbuckleNewtonsoftJsonConvention))]

namespace Rocket.Surgery.Operational.Swashbuckle.NewtonsoftJson
{
    /// <summary>
    /// ValidationConvention.
    /// Implements the <see cref="Rocket.Surgery.Extensions.DependencyInjection.IServiceConvention" />
    /// </summary>
    /// <seealso cref="Rocket.Surgery.Extensions.DependencyInjection.IServiceConvention" />
    /// <seealso cref="IServiceConvention" />
    [PublicAPI]
    public class SwashbuckleNewtonsoftJsonConvention : IServiceConvention
    {
        /// <summary>
        /// Registers the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Register(IServiceConventionContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Services.AddSwaggerGenNewtonsoftSupport();
            context.Services.Configure<SwaggerGenOptions>(
                options =>
                {
                    options.MapType<JToken>(
                        () => new OpenApiSchema()
                        {
                            Type = "object",
                            AdditionalPropertiesAllowed = true,
                            Nullable = true,
                        }
                    );

                    options.MapType<JObject>(
                        () => new OpenApiSchema()
                        {
                            Type = "object",
                            AdditionalPropertiesAllowed = true,
                            Nullable = true,
                        }
                    );

                    options.MapType<JArray>(
                        () => new OpenApiSchema()
                        {
                            Type = "array",
                            Items = new OpenApiSchema()
                            {
                                Type = "object",
                                AdditionalPropertiesAllowed = true,
                                Nullable = true,
                            }
                        }
                    );
                }
            );
        }
    }
}
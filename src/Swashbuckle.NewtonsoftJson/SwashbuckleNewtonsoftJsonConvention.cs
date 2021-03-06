﻿using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NodaTime;
using Rocket.Surgery.AspNetCore.Swashbuckle.NewtonsoftJson;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

[assembly: Convention(typeof(SwashbuckleNewtonsoftJsonConvention))]

namespace Rocket.Surgery.AspNetCore.Swashbuckle.NewtonsoftJson
{
    /// <summary>
    /// ValidationConvention.
    /// Implements the <see cref="IServiceConvention" />
    /// </summary>
    /// <seealso cref="IServiceConvention" />
    /// <seealso cref="IServiceConvention" />
    [PublicAPI]
    public class SwashbuckleNewtonsoftJsonConvention : IServiceConvention
    {
        /// <summary>
        /// Registers the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="configuration"></param>
        /// <param name="services"></param>
        public void Register(IConventionContext context, IConfiguration configuration, IServiceCollection services)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            services.AddSwaggerGenNewtonsoftSupport();
            services.Configure<SwaggerGenOptions>(
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
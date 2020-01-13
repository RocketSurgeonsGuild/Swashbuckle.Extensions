﻿using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.XPath;
using FluentValidation.Validators;
using JetBrains.Annotations;
using MicroElements.Swashbuckle.FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Extensions.DependencyInjection;
using Rocket.Surgery.Operational.Swashbuckle;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Rocket.Surgery.Extensions.FluentValidation;

[assembly: Convention(typeof(SwashbuckleConvention))]

namespace Rocket.Surgery.Operational.Swashbuckle
{
    /// <summary>
    /// ValidationConvention.
    /// Implements the <see cref="Rocket.Surgery.Extensions.DependencyInjection.IServiceConvention" />
    /// </summary>
    /// <seealso cref="Rocket.Surgery.Extensions.DependencyInjection.IServiceConvention" />
    /// <seealso cref="IServiceConvention" />
    [PublicAPI]
    public class SwashbuckleConvention : IServiceConvention
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

            context.Services.AddSwaggerGen(
                options =>
                {
                    options.ConfigureForNodaTime();
                    options.SchemaFilter<ProblemDetailsSchemaFilter>();
                    options.OperationFilter<OperationIdFilter>();
                    options.OperationFilter<StatusCode201Filter>();
                    options.OperationFilter<OperationMediaTypesFilter>();
                    options.OperationFilter<AuthorizeFilter>();
                    options.AddFluentValidationRules();

                    options.MapType<JsonElement>(
                        () => new OpenApiSchema()
                        {
                            Type = "object",
                            AdditionalPropertiesAllowed = true,
                        }
                    );
                    options.MapType<JsonElement?>(
                        () => new OpenApiSchema()
                        {
                            Type = "object",
                            AdditionalPropertiesAllowed = true,
                            Nullable = true,
                        }
                    );

                    options.DocInclusionPredicate(
                        (docName, apiDesc) =>
                        {
                            if (!apiDesc.TryGetMethodInfo(out var methodInfo))
                                return false;
                            return methodInfo.DeclaringType?.GetCustomAttributes(true).OfType<ApiControllerAttribute>()
                                   .Any() ==
                                true;
                        }
                    );

                    options.CustomSchemaIds(
                        type =>
                        {
                            if (type == typeof(FluentValidation.Severity))
                                return $"Validation{nameof(FluentValidation.Severity)}";
                            return type.IsNested ? type.DeclaringType?.Name + type.Name : type.Name;
                        }
                    );

                    foreach (var item in Directory.EnumerateFiles(AppContext.BaseDirectory, "*.xml")
                       .Where(x => File.Exists(Path.ChangeExtension(x, "dll"))))
                    {
                        options.IncludeXmlComments(
                            () =>
                            {
                                using var stream = File.OpenRead(item)!;
                                return new XPathDocument(stream);
                            }
                        );
                    }
                }
            );
            
            AddFluentValdiationRules(context.Services);
        }

        private static void AddFluentValdiationRules(IServiceCollection services)
        {
            

            services.AddSingleton(
                new FluentValidationRule("NotEmpty")
                {
                    Matches = propertyValidator => propertyValidator is INotEmptyValidator,
                    Apply = context =>
                    {
                        var propertyType = context.SchemaFilterContext.Type
                           .GetProperties()
                           .Where(x => x.Name.Equals(context.PropertyKey, StringComparison.OrdinalIgnoreCase))
                           .Select(x => x.PropertyType)
                           .Concat(
                                context.SchemaFilterContext.Type
                                   .GetFields()
                                   .Where(x => x.Name.Equals(context.PropertyKey, StringComparison.OrdinalIgnoreCase))
                                   .Select(x => x.FieldType)
                            )
                           .FirstOrDefault();
                        if (propertyType == typeof(string))
                        {
                            context.Schema.Properties[context.PropertyKey].MinLength = 1;
                        }
                    }
                }
            );

            services.AddSingleton(
                new FluentValidationRule("ValueTypeOrEnum")
                {
                    Matches = propertyValidator => true,
                    Apply = context =>
                    {
                        var propertyType = context.SchemaFilterContext.Type
                           .GetProperties()
                           .Where(x => x.Name.Equals(context.PropertyKey, StringComparison.OrdinalIgnoreCase))
                           .Select(x => x.PropertyType)
                           .Concat(
                                context.SchemaFilterContext.Type
                                   .GetFields()
                                   .Where(x => x.Name.Equals(context.PropertyKey, StringComparison.OrdinalIgnoreCase))
                                   .Select(x => x.FieldType)
                            )
                           .FirstOrDefault();
                        if (propertyType != null &&
                            ( propertyType.IsValueType && Nullable.GetUnderlyingType(propertyType) == null ||
                                propertyType.IsEnum ))
                        {
                            context.Schema.Required.Add(context.PropertyKey);
                            context.Schema.Properties[context.PropertyKey].Nullable = false;
                        }
                    }
                }
            );

            services.AddSingleton(
                new FluentValidationRule("Nullable")
                {
                    Matches = propertyValidator => propertyValidator is INotNullValidator ||
                        propertyValidator is INotEmptyValidator ||
                        propertyValidator is INullValidator,
                    Apply = context =>
                    {
                        context.Schema.Properties[context.PropertyKey].Nullable =
                            !( context.PropertyValidator is INotNullValidator ||
                                context.PropertyValidator is INotEmptyValidator );
                    }
                }
            );

            services.AddSingleton(
                new FluentValidationRule("IsOneOf")
                {
                    Matches = propertyValidator => propertyValidator is StringInValidator,
                    Apply = context =>
                    {
                        var validator = context.PropertyValidator as StringInValidator;
                        context.Schema.Properties[context.PropertyKey].Enum =
                            validator!.Values.Select(x => new OpenApiString(x)).Cast<IOpenApiAny>().ToList();
                    }
                }
            );
        }
    }
}
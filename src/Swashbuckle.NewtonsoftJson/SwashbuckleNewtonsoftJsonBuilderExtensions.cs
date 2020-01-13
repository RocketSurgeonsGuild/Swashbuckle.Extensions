using System;
using JetBrains.Annotations;
using Rocket.Surgery.Operational.Swashbuckle.NewtonsoftJson;

// ReSharper disable once CheckNamespace
namespace Rocket.Surgery.Conventions
{
    /// <summary>
    /// FluentValidationHostBuilderExtensions.
    /// </summary>
    [PublicAPI]
    public static class SwashbuckleNewtonsoftJsonBuilderExtensions
    {
        /// <summary>
        /// Adds fluent validation.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>IConventionHostBuilder.</returns>
        public static IConventionHostBuilder UseSwashbuckleNewtonsoftJson(this IConventionHostBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Scanner.PrependConvention<SwashbuckleNewtonsoftJsonConvention>();
            return builder;
        }
    }
}
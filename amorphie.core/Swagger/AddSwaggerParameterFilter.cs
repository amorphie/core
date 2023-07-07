using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace amorphie.core.Swagger
{
    public class AddSwaggerParameterFilter : IOperationFilter
    {
        /// <summary>
        /// Swagger: Creates new required http header.
        /// </summary>
        /// <param name="operation"> Open Api Operation.</param>
        /// <param name="context">Operation Filter Context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var globalAttributes = context.ApiDescription.ActionDescriptor.FilterDescriptors.Select(p => p.Filter);
            var controllerAttributes = context.MethodInfo?.DeclaringType?.GetCustomAttributes(true);
            var methodAttributes = context.MethodInfo?.GetCustomAttributes(true);
            var produceAttributes = globalAttributes
                .Union(controllerAttributes ?? throw new InvalidOperationException())
                .Union(methodAttributes)
                .OfType<AddSwaggerParameterAttribute>()
                .ToList();

            if (!produceAttributes.Any())
            {
                return;
            }

            if (operation.Parameters == null)
            {
                operation.Parameters = new List<OpenApiParameter>();
            }
            foreach (AddSwaggerParameterAttribute addHeadersAttribute in produceAttributes)
            {

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = addHeadersAttribute.Name,
                    In = addHeadersAttribute.ParameterLocation,
                    Required = addHeadersAttribute.Required
                });
            }
        }
    }
}

using Microsoft.OpenApi.Models;

namespace amorphie.core.Swagger
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class AddSwaggerParameterAttribute : Attribute
    {
        public readonly string Name;
        public readonly bool Required;
        public readonly ParameterLocation ParameterLocation;
        public AddSwaggerParameterAttribute(string name, ParameterLocation parameterLocation, bool required = false)
        {
            Name = name;
            Required = required;
            ParameterLocation = parameterLocation;
        }
    }
}

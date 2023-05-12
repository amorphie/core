using System.Reflection;
using FluentValidation;
using AutoMapper;


namespace amorphie.core.Extension
{
    public static class DefaultBuilderExtensions
    {
        public static void AddBBTDefault(this WebApplicationBuilder builder) => AddBBTDefault(builder, AppDomain.CurrentDomain.GetAssemblies());
        public static void AddBBTDefault(this WebApplicationBuilder builder, IEnumerable<Assembly> assemblies)
        {
            // Add services to the container.
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            builder.Services.AddValidatorsFromAssemblies(assemblies);
            builder.Services.AddAutoMapper(assemblies);
        }
    }
}


using amorphie.core.Base;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace amorphie.core.Module.minimal_api
{
    public abstract class BaseBBTRoute<
        TDTOModel,
        TDBModel,
        TValidator,
        TDbContext> : BaseRoute
        where TDTOModel : class, new()
        where TDBModel : EntityBase
        where TValidator : AbstractValidator<TDBModel>
        where TDbContext : DbContext
    {
        protected BaseBBTRoute(WebApplication app) : base(app)
        {
        }

        public abstract string[]? PropertyCheckList { get; }

        public override void AddRoutes(RouteGroupBuilder routeGroupBuilder)
        {
            routeGroupBuilder.MapGet("/{id}", Get)
             .Produces<TDTOModel>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status404NotFound);

            routeGroupBuilder.MapGet("/", GetAll)
            .Produces<TDTOModel[]>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

            routeGroupBuilder.MapPost("/", Upsert);

            routeGroupBuilder.MapDelete("/{id}", Delete)
            .Produces<TDTOModel>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }

        protected virtual async ValueTask<IResult> Get(
            [FromServices] TDbContext context,
            [FromRoute(Name = "id")] int id)
        {
            DbSet<TDBModel> dbSet = context.Set<TDBModel>();
            return await dbSet.FindAsync(id)
                is TDBModel model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        }

        protected virtual async ValueTask<IResult> GetAll(
            [FromServices] TDbContext context,
            [FromQuery][Range(0, 100)] int page,
            [FromQuery][Range(5, 100)] int pageSize)
        {
            IList<TDBModel> resultList = await context.Set<TDBModel>()
             .Skip(page)
             .Take(pageSize)
             .ToListAsync();

            return (resultList != null && resultList.Count() > 0)
                 ? Results.Ok(resultList)
                 : Results.NoContent();
        }

        protected virtual async ValueTask<IResult> Upsert(
            [FromServices] IMapper mapper,
            [FromServices] TValidator validator,
            [FromServices] TDbContext context,
            [FromBody] TDTOModel data)
        {
            var dbModelData = mapper.Map<TDBModel>(data);


            FluentValidation.Results.ValidationResult validationResult = await validator.ValidateAsync(dbModelData);

            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            DbSet<TDBModel> dbSet = context.Set<TDBModel>();

            bool IsChange = false;
            TDBModel? dataFromDB = await dbSet.FirstOrDefaultAsync(x => x.Id == dbModelData.Id);

            if (dataFromDB != null)
            {
                if (PropertyCheckList != null)
                {
                    object? dbValue;
                    object? dtoValue;

                    foreach (string property in PropertyCheckList)
                    {
                        dbValue = typeof(TDBModel).GetProperties().First(p => p.Name.Equals(property)).GetValue(dataFromDB);
                        dtoValue = typeof(TDTOModel).GetProperties().First(p => p.Name.Equals(property)).GetValue(data);

                        if (dbValue != null && !dbValue.Equals(dtoValue))
                        {
                            typeof(TDBModel).GetProperties().First(p => p.Name.Equals(property)).SetValue(dataFromDB, dtoValue);
                            IsChange = true;
                        }
                    }
                }
            }
            else
            {
                await dbSet.AddAsync(dbModelData);
                IsChange = true;
            }

            if (IsChange)
            {
                await context.SaveChangesAsync();
                return Results.Ok();
            }
            else
            {
                return Results.Ok("No change!");
            }
        }

        protected virtual async ValueTask<IResult> Delete(
            [FromServices] TDbContext context,
            [FromRoute(Name = "id")] int id)
        {
            DbSet<TDBModel> dbSet = context.Set<TDBModel>();


            if (await dbSet.FindAsync(id) is TDBModel model)
            {
                dbSet.Remove(model);
                await context.SaveChangesAsync();
                return Results.Ok(model);
            }

            return Results.NotFound();
        }
    }
}


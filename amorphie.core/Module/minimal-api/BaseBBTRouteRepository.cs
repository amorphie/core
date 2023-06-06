using amorphie.core.Base;
using amorphie.core.Repository;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using static Google.Rpc.Context.AttributeContext.Types;

namespace amorphie.core.Module.minimal_api
{
    public abstract class BaseBBTRouteRepository<
        TDTOModel,
        TDBModel,
        TValidator,
        TDbContext,
        TRepository> : BaseRoute
        where TDTOModel : class, new()
        where TDBModel : EntityBase
        where TValidator : AbstractValidator<TDBModel>
        where TDbContext : DbContext
        where TRepository : IBBTRepository<TDBModel, TDbContext>
    {
        protected BaseBBTRouteRepository(WebApplication app) : base(app)
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

            routeGroupBuilder.MapGet("/fulltext", FullText)
            .Produces<TDTOModel[]>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);


            routeGroupBuilder.MapPost("/", Upsert)
            .Produces<TDTOModel>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status204NoContent);

            routeGroupBuilder.MapDelete("/{id}", Delete)
            .Produces<TDTOModel>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }

        protected virtual async ValueTask<IResult> Get(
            [FromServices] TRepository repository,
            [FromRoute(Name = "id")] Guid id)
        {
            return await repository.GetById(id)
                is TDBModel model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        }

        protected virtual async ValueTask<IResult> GetAll(
            [FromServices] TRepository repository,
            [FromQuery][Range(0, 100)] int page,
            [FromQuery][Range(5, 100)] int pageSize)
        {
            IList<TDBModel> resultList = await repository.GetAll(page, pageSize).ToListAsync();

            return (resultList != null && resultList.Count() > 0)
                 ? Results.Ok(resultList)
                 : Results.NoContent();
        }

        protected virtual async ValueTask<IResult> FullText(
    [FromServices] TRepository repository, DtoSearchBase searchCriteria)
        {
            IList<TDBModel> resultList = await repository.GetAll(searchCriteria.Page, searchCriteria.PageSize).ToListAsync();

            return (resultList != null && resultList.Count() > 0)
                 ? Results.Ok(resultList)
                 : Results.NoContent();
        }

        protected virtual async ValueTask<IResult> Upsert(
            [FromServices] IMapper mapper,
            [FromServices] TValidator validator,
            [FromServices] TRepository repository,
            [FromBody] TDTOModel data)
        {
            var dbModelData = mapper.Map<TDBModel>(data);


            FluentValidation.Results.ValidationResult validationResult = await validator.ValidateAsync(dbModelData);

            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            bool IsChange = false;
            TDBModel? dataFromDB = await repository.GetById(dbModelData.Id);

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

                if (IsChange)
                    await repository.SaveChangesAsync();

                return Results.NoContent();
            }
            else
            {
                await repository.Insert(dbModelData);

                return Results.Created($"/{dbModelData.Id}", dbModelData);
            }
        }

        protected virtual async ValueTask<IResult> Delete(
            [FromServices] TRepository repository,
            [FromRoute(Name = "id")] Guid id)
        {
            if (await repository.GetById(id) is TDBModel model)
            {
                await repository.Delete(model);
                return Results.Ok(model);
            }

            return Results.NotFound();
        }
    }
}


using amorphie.core.Base;
using amorphie.core.Extension;
using amorphie.core.Identity;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using static amorphie.core.Extension.DatabaseExtensions;

namespace amorphie.core.Module.minimal_api
{
    public abstract class BaseBBTRoute<TDTOModel, TDBModel, TDbContext> : BaseRoute
        where TDTOModel : class, new()
        where TDBModel : EntityBase
        where TDbContext : DbContext
    {
        protected BaseBBTRoute(WebApplication app)
            : base(app) { }

        public abstract string[]? PropertyCheckList { get; }

        public override void AddRoutes(RouteGroupBuilder routeGroupBuilder)
        {
            Get(routeGroupBuilder);
            GetAll(routeGroupBuilder);
            Upsert(routeGroupBuilder);
            Delete(routeGroupBuilder);
        }

        protected virtual void Get(RouteGroupBuilder routeGroupBuilder)
        {
            routeGroupBuilder
                .MapGet("/{id}", GetMethod)
                .Produces<TDTOModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);
        }

        protected virtual async ValueTask<IResult> GetMethod(
            [FromServices] TDbContext context,
            [FromServices] IMapper mapper,
            [FromRoute(Name = "id")] Guid id,
            HttpContext httpContext,
            CancellationToken token
        )
        {
            DbSet<TDBModel> dbSet = context.Set<TDBModel>();
            return
                await dbSet.AsNoTracking().FirstOrDefaultAsync<TDBModel>(x => x.Id == id, token)
                    is TDBModel model
                ? TypedResults.Ok(mapper.Map<TDTOModel>(model))
                : TypedResults.NotFound();
        }

        protected virtual void GetAll(RouteGroupBuilder routeGroupBuilder)
        {
            routeGroupBuilder
                .MapGet("/", GetAllMethod)
                .Produces<TDTOModel[]>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status204NoContent);
        }

        protected virtual async ValueTask<IResult> GetAllMethod(
            [FromServices] TDbContext context,
            [FromServices] IMapper mapper,
            [FromQuery][Range(0, 100)] int page,
            [FromQuery][Range(5, 100)] int pageSize,
            HttpContext httpContext,
            CancellationToken token,
            [FromQuery] string? sortColumn,
            [FromQuery] SortDirectionEnum? sortDirection = SortDirectionEnum.Asc
        )
        {
            IQueryable<TDBModel> query = context
                .Set<TDBModel>()
                .AsNoTracking();

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = await query.Sort(sortColumn, sortDirection);
            }
            IList<TDBModel> resultList = await query
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync(token);

            return (resultList != null && resultList.Count > 0)
                ? Results.Ok(mapper.Map<IList<TDTOModel>>(resultList))
                : Results.NoContent();
        }

        protected virtual void Upsert(RouteGroupBuilder routeGroupBuilder)
        {
            routeGroupBuilder
                .MapPost("/", UpsertMethod)
                .Produces<TDTOModel>(StatusCodes.Status200OK)
                .Produces<TDTOModel>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status204NoContent);
        }

        protected virtual async ValueTask<IResult> UpsertMethod(
            [FromServices] IMapper mapper,
            [FromServices] IValidator<TDBModel> validator,
            [FromServices] TDbContext context,
            [FromServices] IBBTIdentity bbtIdentity,
            [FromBody] TDTOModel data,
            HttpContext httpContext,
            CancellationToken token
        )
        {
            var dbModelData = mapper.Map<TDBModel>(data);

            FluentValidation.Results.ValidationResult validationResult =
                await validator.ValidateAsync(dbModelData);

            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            DbSet<TDBModel> dbSet = context.Set<TDBModel>();

            bool IsChange = false;
            TDBModel? dataFromDB = await dbSet.FirstOrDefaultAsync(x => x.Id == dbModelData.Id, token);

            if (dataFromDB != null)
            {
                if (PropertyCheckList != null)
                {
                    object? dbValue;
                    object? dtoValue;

                    foreach (string property in PropertyCheckList)
                    {
                        dbValue = typeof(TDBModel)
                            .GetProperties()
                            .First(p => p.Name.Equals(property))
                            .GetValue(dataFromDB);
                        dtoValue = typeof(TDTOModel)
                            .GetProperties()
                            .First(p => p.Name.Equals(property))
                            .GetValue(data);

                        if (dbValue != null && !dbValue.Equals(dtoValue))
                        {
                            typeof(TDBModel)
                                .GetProperties()
                                .First(p => p.Name.Equals(property))
                                .SetValue(dataFromDB, dtoValue);
                            IsChange = true;
                        }
                    }
                }

                if (IsChange)
                {
                    dataFromDB.ModifiedAt = DateTime.UtcNow;
                    dataFromDB.ModifiedBy = bbtIdentity.UserId.Value;
                    dataFromDB.ModifiedByBehalfOf = bbtIdentity.BehalfOfId.Value;

                    await context.SaveChangesAsync(token);
                    return Results.Ok(mapper.Map<TDTOModel>(dataFromDB));
                }
                else
                {
                    return Results.NoContent();
                }
            }
            else
            {
                dbModelData.CreatedAt = DateTime.UtcNow;
                dbModelData.CreatedBy = bbtIdentity.UserId.Value;
                dbModelData.CreatedByBehalfOf = bbtIdentity.BehalfOfId.Value;

                dbModelData.ModifiedAt = dbModelData.CreatedAt;
                dbModelData.ModifiedBy = dbModelData.CreatedBy;
                dbModelData.ModifiedByBehalfOf = dbModelData.CreatedByBehalfOf;

                await dbSet.AddAsync(dbModelData);
                await context.SaveChangesAsync(token);

                return Results.Created($"/{dbModelData.Id}", mapper.Map<TDTOModel>(dbModelData));
            }
        }

        protected virtual void Delete(RouteGroupBuilder routeGroupBuilder)
        {
            routeGroupBuilder
                .MapDelete("/{id}", DeleteMethod)
                .Produces<TDTOModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);
        }

        protected virtual async ValueTask<IResult> DeleteMethod(
            [FromServices] IMapper mapper,
            [FromServices] TDbContext context,
            [FromRoute(Name = "id")] Guid id,
             HttpContext httpContext,
            CancellationToken token
        )
        {
            DbSet<TDBModel> dbSet = context.Set<TDBModel>();

            if (await dbSet.FindAsync(id, token) is TDBModel model)
            {
                dbSet.Remove(model);
                await context.SaveChangesAsync(token);
                return Results.Ok(mapper.Map<TDTOModel>(model));
            }

            return Results.NotFound();
        }
    }
}

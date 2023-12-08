using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace amorphie.core.Extension;

public enum SortDirectionEnum
{
    Asc,
    Desc
}

public static class DatabaseExtensions
{

    public async static Task<IQueryable<TModel>> Sort<TModel>(this IQueryable<TModel> query, string SortColumn, SortDirectionEnum? sortDirectionEnum)
    {
        if (!string.IsNullOrEmpty(SortColumn))
        {
            string sortDirection = string.Empty;
if(sortDirectionEnum.GetValueOrDefault(SortDirectionEnum.Asc) == SortDirectionEnum.Asc)
{
                sortDirection = "OrderBy";
}
else
{
                sortDirection = "OrderByDescending";
            }
            var queryExpr = query.Expression;
            var parameter = Expression.Parameter(typeof(TModel), "p");
            var property = typeof(TModel).GetProperties().FirstOrDefault(p => string.Equals(p.Name, SortColumn, StringComparison.OrdinalIgnoreCase));
            if (property == null)
            {
                throw new Exception($"Property: {SortColumn} not found");
            }
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var expression = Expression.Lambda(propertyAccess, parameter);

            queryExpr = Expression.Call(typeof(Queryable),
                                             sortDirection,
                                              new[] { GetElementType(query), expression.Body.Type },
                                              query.Expression,
                                              Expression.Quote(expression));

            query = query.Provider.CreateQuery<TModel>(queryExpr);

        }
        return query;


    }

    private static System.Type GetElementType(IQueryable source)
    {
        Expression expr = source.Expression;
        System.Type elementType = source.ElementType;
        while (expr.NodeType == ExpressionType.Call &&
               elementType == typeof(object))
        {
            var call = (MethodCallExpression)expr;
            expr = call.Arguments.First();
            elementType = expr.Type.GetGenericArguments().First();
        }

        return elementType;
    }
}
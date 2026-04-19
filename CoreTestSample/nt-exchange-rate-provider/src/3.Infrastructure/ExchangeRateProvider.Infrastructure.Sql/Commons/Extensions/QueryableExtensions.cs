using NT.DDD.Base.Paginations.Enums;
using System.Linq.Expressions;

namespace ExchangeRateProvider.Infrastructure.Sql.Commons.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ToPagedQuery<T>(this IQueryable<T> query, int pageSize, int pageNumber)
    {
        if (pageNumber < 1) pageNumber = 1;
        int skip = (pageNumber - 1) * pageSize;

        return query.Skip(skip).Take(pageSize);
    }
    public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> source, Dictionary<string, RequestOrderType> orders)
    {
        var expression = source.Expression;
        int count = 0;
        if (orders is not null)
            foreach (var item in orders)
            {
                ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
                MemberExpression selector = Expression.PropertyOrField(parameter, item.Key);
                string method = item.Value == RequestOrderType.Descending ?
                    count == 0 ? "OrderByDescending" : "ThenByDescending" :
                    count == 0 ? "OrderBy" : "ThenBy";
                expression = Expression.Call(typeof(Queryable), method,
                    new Type[] { source.ElementType, selector.Type },
                    expression, Expression.Quote(Expression.Lambda(selector, parameter)));
                count++;
            }
        return count > 0 ? source.Provider.CreateQuery<T>(expression) : source;
    }
}

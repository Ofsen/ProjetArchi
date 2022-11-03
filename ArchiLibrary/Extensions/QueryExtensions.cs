using ArchiLibrary.Extensions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ArchiLibrary.Extensions
{
    public static class QueryExtensions
    {
        public static IOrderedQueryable<TModel> Sort<TModel>(this IQueryable<TModel> query, ParamsModel myParams)
        {
            if (!string.IsNullOrWhiteSpace(myParams.Asc))
            {
                string champ = myParams.Asc;

                // create a lambda expression
                var parameter = Expression.Parameter(typeof(TModel), "x");
                var property = Expression.Property(parameter, champ);

                var o = Expression.Convert(property, typeof(object));
                var lambda = Expression.Lambda<Func<TModel, object>>(o, parameter);

                // use the lambda expression
                query = query.OrderBy(lambda);
            }
            
            if (!string.IsNullOrWhiteSpace(myParams.Desc))
            {
                string champ = myParams.Desc;

                // create a lambda expression
                var parameter = Expression.Parameter(typeof(TModel), "x");
                var property = Expression.Property(parameter, champ);

                var o = Expression.Convert(property, typeof(object));
                var lambda = Expression.Lambda<Func<TModel, object>>(o, parameter);

                // use the lambda expression
                query = query.OrderByDescending(lambda);
            } 

            return (IOrderedQueryable<TModel>)query;
        }
    }
}

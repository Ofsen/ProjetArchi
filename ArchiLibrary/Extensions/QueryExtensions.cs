using ArchiLibrary.Extensions.Models;
using Microsoft.AspNetCore.Mvc;
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
            if (!string.IsNullOrWhiteSpace(myParams.Sort))
            {
                var ascSortingParams = myParams.Sort.Split(",");

                // ?sort=name&desc=name

                //Where(x => x.Name && x.Slogan)

                if(ascSortingParams.Length > 1)
                {
                    foreach (var p in ascSortingParams)
                    {
                        string champ = myParams.Sort;

                        // create a lambda expression
                        var parameter = Expression.Parameter(typeof(TModel), "x");
                        var property = Expression.Property(parameter, champ);

                        var o = Expression.Convert(property, typeof(object));
                        var lambda = Expression.Lambda<Func<TModel, object>>(o, parameter);

                        // use the lambda expression
                        query = query.OrderBy(lambda);
                    }
                } else
                {

                }
            }
            
            if (!string.IsNullOrWhiteSpace(myParams.Desc))
            {
                var descSortingParams = myParams.Desc.Split(",");

                foreach (var p in descSortingParams)
                {
                    string champ = myParams.Desc;

                    // create a lambda expression
                    var parameter = Expression.Parameter(typeof(TModel), "x");
                    var property = Expression.Property(parameter, champ);

                    var o = Expression.Convert(property, typeof(object));
                    var lambda = Expression.Lambda<Func<TModel, object>>(o, parameter);

                    // use the lambda expression
                    if (!string.IsNullOrWhiteSpace(myParams.Sort))
                    {
                        //query = query.ThenByDescending(lambda);
                    }
                    else
                    {
                        query = query.OrderByDescending(lambda);
                    }
                }
            }

            return (IOrderedQueryable<TModel>)query;
        }
    }
}

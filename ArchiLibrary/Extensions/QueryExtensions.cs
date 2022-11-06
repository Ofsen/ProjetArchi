using ArchiLibrary.Extensions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ArchiLibrary.Extensions
{
    public static class QueryExtensions
    {
        public static IOrderedQueryable<TModel> Sort<TModel>(this IQueryable<TModel> query, ParamsModel myParams, Boolean hasDesc)
        {
            IOrderedQueryable<TModel> localQuery = (IOrderedQueryable<TModel>)query;

            if (!string.IsNullOrWhiteSpace(myParams.Sort))
            {
                var ascSortingParams = myParams.Sort.Split(",");

                string champ = ascSortingParams[0];

                // create a lambda expression
                var parameter = Expression.Parameter(typeof(TModel), "x");

                var property = Expression.Property(parameter, champ);
                var o = Expression.Convert(property, typeof(object));

                var lambda = Expression.Lambda<Func<TModel, object>>(o, parameter);

                // use the lambda expression
                if (hasDesc && string.IsNullOrWhiteSpace(myParams.Desc))
                {
                    localQuery = localQuery.OrderByDescending(lambda);
                }
                else
                {
                    localQuery = localQuery.OrderBy(lambda);
                }

                if (ascSortingParams.Length > 1)
                {
                    for (int i = 1; i < ascSortingParams.Length; i++)
                    {
                        string champIte = ascSortingParams[i];

                        // create a lambda expression
                        var parameterIte = Expression.Parameter(typeof(TModel), "x");
                        var propertyIte = Expression.Property(parameterIte, champIte);

                        var oIte = Expression.Convert(propertyIte, typeof(object));
                        var lambdaIte = Expression.Lambda<Func<TModel, object>>(oIte, parameterIte);

                        // use the lambda expression
                        if (hasDesc && string.IsNullOrWhiteSpace(myParams.Desc))
                        {
                            localQuery = localQuery.ThenByDescending(lambdaIte);
                        }
                        else
                        {
                            localQuery = localQuery.ThenBy(lambdaIte);
                        }
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(myParams.Desc))
            {
                var descSortingParams = myParams.Desc.Split(",");

                string champ = descSortingParams[0];

                // create a lambda expression
                var parameter = Expression.Parameter(typeof(TModel), "x");
                var property = Expression.Property(parameter, champ);

                var o = Expression.Convert(property, typeof(object));
                var lambda = Expression.Lambda<Func<TModel, object>>(o, parameter);

                // use the lambda expression
                if (!string.IsNullOrWhiteSpace(myParams.Sort))
                {
                    localQuery = localQuery.ThenByDescending(lambda);
                }
                else
                {
                    localQuery = localQuery.OrderByDescending(lambda);
                }

                if (descSortingParams.Length > 1)
                {
                    for (int i = 1; i < descSortingParams.Length; i++)
                    {
                        string champIte = descSortingParams[i];

                        // create a lambda expression
                        var parameterIte = Expression.Parameter(typeof(TModel), "x");

                        var propertyIte = Expression.Property(parameterIte, champIte);
                        var oIte = Expression.Convert(propertyIte, typeof(object));
                        
                        var lambdaIte = Expression.Lambda<Func<TModel, object>>(oIte, parameterIte);

                        // use the lambda expression
                        localQuery = localQuery.ThenByDescending(lambdaIte);
                    }
                }
            }

            return localQuery;
        }

        public static IQueryable<TModel> PartialResponse<TModel>(this IQueryable<TModel> query, String fields)
        {
            var fieldsArray = fields.Split(",", StringSplitOptions.RemoveEmptyEntries);
            
            // creates: parameter x with the type dynamic
            var parameter = Expression.Parameter(typeof(TModel), "x");

            // creates: [ x.Name, x.ID ... ]
            var properties = fieldsArray.Select(x => Expression.Property(parameter, x));

            // creates: foreach elem do: Name = elem.Name
            var bindings = properties.Select(x => Expression.Bind(x.Member, x));

            // creates: dynamic object
            //var dynamicObject = ;

            // creates: new { Name = x.Name, ID = x.ID ... }
            var dict = DynamicClassFactory.CreateType(fieldsArray.Select(f => typeof(TModel).GetProperty(f, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase))
                        .Select(p => new DynamicProperty(p.Name, p.PropertyType))
                        .ToList(), false);

            var newStuff = Expression.New(dict);
            var members = Expression.MemberInit(newStuff, bindings);
            //var members = Expression.MemberInit(Expression.New(typeof(dynamic)), bindings);

            // creates: x => new { Name = x.Name, ID = x.ID ... }
            var lambda = Expression.Lambda<Func<TModel, dynamic>>(members, parameter);

            return (dynamic)query.Select(lambda);

            //var properties = fieldsArray
            //            .Select(f => typeof(TModel).GetProperty(f, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase))
            //            .Select(p => new DynamicProperty(p.Name, p.PropertyType))
            //            .ToList();

            //var resultType = DynamicClassFactory.CreateType(properties, false);
            //var bindings = properties.Select(p => Expression.Bind(resultType.GetProperty(p.Name), Expression.Property(parameter, p.Name)));
            //var result = Expression.MemberInit(Expression.New(resultType), bindings);

            //var lambda = Expression.Lambda<Func<TModel, dynamic>>(result, parameter);
            //query = (dynamic)query.Select(lambda);

            //return query;
        }
    }
}

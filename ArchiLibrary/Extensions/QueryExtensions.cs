using ArchiLibrary.Extensions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections;
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

        public static IQueryable<dynamic> PartialResponse<TModel>(this IQueryable<TModel> query, String fields)
        {
            var fieldsArray = fields.Split(",", StringSplitOptions.RemoveEmptyEntries);

            // creates: parameter x with the type dynamic
            var parameter = Expression.Parameter(typeof(TModel), "x");

            // only get fields of the model
            List<string> fieldsModel = typeof(TModel).GetProperties().Select(x => x.Name.ToLower()).ToList();
            List<string> untrimmedFields = new List<string>();
            foreach (var field in fieldsModel)
            {
                if (fieldsArray.Contains(field.ToLower()))
                    untrimmedFields.Add(field);
            }

            var properties = untrimmedFields
                        .Select(f => typeof(TModel).GetProperty(f, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase))
                        .Select(p => new DynamicProperty(p.Name, p.PropertyType))
                        .ToList();

            // create a dynamic type
            var resultType = DynamicClassFactory.CreateType(properties, false);

            // create the : x = x.Name
            var bindings = properties.Select(p => Expression.Bind(resultType.GetProperty(p.Name), Expression.Property(parameter, p.Name)));

            // initialize the dynamic type 
            var result = Expression.MemberInit(Expression.New(resultType), bindings);

            var lambda = Expression.Lambda<Func<TModel, dynamic>>(result, parameter);
            var query2 = (dynamic)query.Select(lambda);
            return (IQueryable<dynamic>)query2;
        }

        public static IQueryable<TModel> FilterResponses<TModel>(this IQueryable<TModel> query, IDictionary<PropertyInfo, string> keys)
        {
            // initialize the body of predicate
            BinaryExpression expression = null;
            // create the parameter in our lambda expression
            var parameter = Expression.Parameter(typeof(TModel), "x");

            // making a list of binary expressions
            IList<BinaryExpression> exps = new List<BinaryExpression>();

            foreach (var key in keys)
            {
                BinaryExpression localExpression = null;
                // setting up the property
                var prop = Expression.Property(parameter, key.Key.Name);

                // String type
                if(key.Key.PropertyType == typeof(string))
                {
                    // from { "name": "hello,yes,wow" } to [ x.name == "hello", x.name == "yes", x.name == "wow" ]
                    List<BinaryExpression> equalExpressions = key.Value.Split(",").Select(x => Expression.Equal(prop, Expression.Constant(x))).ToList();
                    // creating the full binary expression with all the Or
                    localExpression = equalExpressions.First();
                    if(equalExpressions.Count() > 1)
                        foreach(var value in equalExpressions.Skip(1))
                        {
                            localExpression = Expression.Or(localExpression, value);
                        }
                }

                // Integer & Datetime type
                if(key.Key.PropertyType == typeof(int) || key.Key.PropertyType == typeof(DateTime))
                {
                    // Range
                    if(key.Value.First() == '[' && key.Value.Last() == ']')
                    {
                        // from { "rating": "[4,5]" } to [ x.rating == "hello", x.rating == "yes", x.rating == "wow" ]
                        string[] bothParties = key.Value.Trim('[', ']').Split(",");
                        // if (bothParties.Length > 2) return BadRequest(); this is not possible here

                        if (bothParties[0] != string.Empty && bothParties[1] != string.Empty)
                        {
                            ConstantExpression leftConst, rightConst;
                            if(key.Key.PropertyType == typeof(int))
                            {
                                leftConst = Expression.Constant(int.Parse(bothParties[0]));
                                rightConst = Expression.Constant(int.Parse(bothParties[1]));
                            } else
                            {
                                leftConst = Expression.Constant(DateTime.Parse(bothParties[0]));
                                rightConst = Expression.Constant(DateTime.Parse(bothParties[1]));
                            }

                            // creating the ( x.rating >= 4 ) and the ( x.rating <= 5 )
                            BinaryExpression leftSideExpression = Expression.GreaterThanOrEqual(prop, leftConst);
                            BinaryExpression rightSideExpression = Expression.LessThanOrEqual(prop, rightConst);
                            
                            localExpression = Expression.And(leftSideExpression, rightSideExpression);
                        }

                        if (bothParties[0] == string.Empty && bothParties[1] != string.Empty)
                        {
                            ConstantExpression rightConst;
                            if (key.Key.PropertyType == typeof(int))
                            {
                                rightConst = Expression.Constant(int.Parse(bothParties[1]));
                            }
                            else
                            {
                                rightConst = Expression.Constant(DateTime.Parse(bothParties[1]));
                            }
                            BinaryExpression rightSideExpression = Expression.LessThanOrEqual(prop, rightConst);

                            localExpression = rightSideExpression;
                        }

                        if (bothParties[0] != string.Empty && bothParties[1] == string.Empty)
                        {
                            ConstantExpression leftConst;
                            if (key.Key.PropertyType == typeof(int))
                            {
                                leftConst = Expression.Constant(int.Parse(bothParties[0]));
                            }
                            else
                            {
                                leftConst = Expression.Constant(DateTime.Parse(bothParties[0]));
                            }
                            BinaryExpression leftSideExpression = Expression.GreaterThanOrEqual(prop, leftConst);

                            localExpression = leftSideExpression;
                        }
                    }
                    else
                    {
                        // from { "rating": "4,5" or "4" or "4,5,6,8,9..." } to [ x.rating == 4, x.rating == 5 ]
                        var equalExpressions = key.Value.Split(",").Select(x => Expression.Equal(prop, Expression.Constant(x))).ToList();
                        // if (bothParties.Length > 2) return BadRequest(); this is not possible here
                        localExpression = equalExpressions.First();
                        if (equalExpressions.Count() > 1)
                            foreach (var value in equalExpressions.Skip(1))
                            {
                                localExpression = Expression.Or(localExpression, value);
                            }
                    }
                }

                exps.Add(localExpression);
            }

            expression = exps.First();
            foreach(var bExp in exps.Skip(1))
            {
                expression = Expression.And(expression, bExp);
            }

            var lambda = Expression.Lambda<Func<TModel, bool>>(expression, parameter);

            return query.Where(lambda);
        }
    }
}

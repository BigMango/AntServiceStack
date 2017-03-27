//-----------------------------------------------------------------------
// <copyright file="AutoMapperUtil.cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <summary></summary>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace AntServiceStack.Manager.Common
{
    public static class AutoMapperUtil
    {
        public static MapperConfiguration Configuration { get; private set; }

        public static T MapperTo<T1, T>(T1 source, Type target)
        {
            var _configuration = new MapperConfiguration(cfg =>cfg.CreateMap(typeof(T1), target));
            return _configuration.CreateMapper().Map<T>(source);
        }

        public static List<T> MapperToList<T1, T>(List<T1> source)
        {
            var _configuration = new MapperConfiguration(cfg => cfg.CreateMap<T1, T>());
            return _configuration.CreateMapper().Map<List<T1>, List<T>>(source);
        }
        public static IQueryable<TDestination> MappperTo<TDestination>(this IQueryable source, params Expression<Func<TDestination, object>>[] membersToExpand)
        {
            return source.ProjectTo(Configuration, membersToExpand);
        }
        public static void Execute()
        {
            Configuration = new MapperConfiguration(
                cfg =>
                {
                    var types = Assembly.GetExecutingAssembly().GetExportedTypes();
                    LoadReverseMappings(types, cfg);
                    LoadStandardMappings(types, cfg);
                });
        }
        private static void LoadStandardMappings(IEnumerable<Type> types, IMapperConfigurationExpression mapperConfiguration)
        {
            var maps = (from t in types
                        from i in t.GetInterfaces()
                        where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapFrom<>) &&
                              !t.IsAbstract &&
                              !t.IsInterface
                        select new
                        {
                            Source = i.GetGenericArguments()[0],
                            Destination = t
                        }).ToArray();

            foreach (var map in maps)
            {
                mapperConfiguration.CreateMap(map.Source, map.Destination);
            }
        }
        private static void LoadReverseMappings(IEnumerable<Type> types, IMapperConfigurationExpression mapperConfiguration)
        {
            var maps = (from t in types
                        from i in t.GetInterfaces()
                        where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapTo<>) &&
                              !t.IsAbstract &&
                              !t.IsInterface
                        select new
                        {
                            Destination = i.GetGenericArguments()[0],
                            Source = t
                        }).ToArray();

            foreach (var map in maps)
            {
                mapperConfiguration.CreateMap(map.Source, map.Destination);
            }
        }
    }

    public interface IMapTo<T>
    where T : class
    {
    }

    public interface IMapFrom<T>
       where T : class
    {
    }
}
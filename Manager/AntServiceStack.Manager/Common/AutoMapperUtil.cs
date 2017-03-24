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
using System.Reflection;
using AutoMapper;

namespace AntServiceStack.Manager.Common
{
    public class AutoMapperUtil
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
        
    }
}
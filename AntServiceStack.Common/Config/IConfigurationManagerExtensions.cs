using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntServiceStack.Common.Extensions;

namespace AntServiceStack.Common.Config
{
    public static class IConfigurationManagerExtensions
    {
        public static IProperty GetProperty(this IConfigurationManager manager, string key)
        {
            manager = manager.NotNull("manager");
            return manager.GetProperty(key, new PropertyConfig());
        }

        public static IProperty GetProperty(this IConfigurationManager manager, string key, string defaultValue)
        {
            manager = manager.NotNull("manager");
            return manager.GetProperty(key, new PropertyConfig(defaultValue));
        }

        public static IProperty GetProperty(this IConfigurationManager manager, string key, string defaultValue, bool useCache)
        {
            manager = manager.NotNull("manager");
            return manager.GetProperty(key, new PropertyConfig(defaultValue, useCache));
        }

        public static IProperty<T> GetProperty<T>(this IConfigurationManager manager, string key)
        {
            manager = manager.NotNull("manager");
            return manager.GetProperty<T>(key, new PropertyConfig<T>());
        }

        public static IProperty<T> GetProperty<T>(this IConfigurationManager manager, string key, T defaultValue)
        {
            manager = manager.NotNull("manager");
            return manager.GetProperty<T>(key, new PropertyConfig<T>(defaultValue));
        }

        public static IProperty<T> GetProperty<T>(this IConfigurationManager manager, string key, T defaultValue, bool useCache)
        {
            manager = manager.NotNull("manager");
            return manager.GetProperty<T>(key, new PropertyConfig<T>(defaultValue, useCache));
        }

        public static IProperty<T> GetProperty<T>(this IConfigurationManager manager, string key, T defaultValue, IValueParser<T> valueParser)
        {
            manager = manager.NotNull("manager");
            return manager.GetProperty<T>(key, new PropertyConfig<T>(defaultValue, valueParser));
        }

        public static IProperty<T> GetProperty<T>(this IConfigurationManager manager, string key, T defaultValue, bool useCache, IValueParser<T> valueParser)
        {
            manager = manager.NotNull("manager");
            return manager.GetProperty<T>(key, new PropertyConfig<T>(defaultValue, useCache, valueParser));
        }

        public static IProperty<T> GetProperty<T>(this IConfigurationManager manager, string key, T defaultValue, IValueCorrector<T> valueCorrector)
        {
            manager = manager.NotNull("manager");
            return manager.GetProperty<T>(key, new PropertyConfig<T>(defaultValue, valueCorrector));
        }

        public static IProperty<T> GetProperty<T>(this IConfigurationManager manager, string key, T defaultValue, bool useCache, IValueCorrector<T> valueCorrector)
        {
            manager = manager.NotNull("manager");
            return manager.GetProperty<T>(key, new PropertyConfig<T>(defaultValue, useCache, valueCorrector));
        }

        public static IProperty<T> GetProperty<T>(this IConfigurationManager manager, string key, T defaultValue, IValueParser<T> valueParser, IValueCorrector<T> valueCorrector)
        {
            manager = manager.NotNull("manager");
            return manager.GetProperty<T>(key, new PropertyConfig<T>(defaultValue, valueParser, valueCorrector));
        }

        public static IProperty<T> GetProperty<T>(this IConfigurationManager manager, string key, T defaultValue, bool useCache, IValueParser<T> valueParser, IValueCorrector<T> valueCorrector)
        {
            manager = manager.NotNull("manager");
            return manager.GetProperty<T>(key, new PropertyConfig<T>(defaultValue, useCache, valueParser, valueCorrector));
        }

        public static IProperty<T> GetProperty<T>(this IConfigurationManager manager, string key, T defaultValue, T min, T max) where T : IComparable<T>
        {
            manager = manager.NotNull("manager");
            return manager.GetProperty<T>(key, new PropertyConfig<T>(defaultValue, (IValueCorrector<T>)new RangeCorrector<T>(min, max)));
        }

        public static IProperty<T> GetProperty<T>(this IConfigurationManager manager, string key, T defaultValue, IValueParser<T> valueParser, T min, T max) where T : IComparable<T>
        {
            manager = manager.NotNull("manager");
            return manager.GetProperty<T>(key, new PropertyConfig<T>(defaultValue, valueParser, (IValueCorrector<T>)new RangeCorrector<T>(min, max)));
        }
    }
}

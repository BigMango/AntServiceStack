using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Config
{
    public interface IConfigurationManager
    {
        IEnumerable<string> Keys { get; }

        IEnumerable<IConfigurationSource> Sources { get; }

        event EventHandler<PropertyChangedEventArgs> OnPropertyChange;

        string GetPropertyValue(string key, string defaultValue);

        IProperty GetProperty(string key, PropertyConfig config);

        IProperty<T> GetProperty<T>(string key, PropertyConfig<T> config);

        void Register<T>(IValueParser<T> valueParser);
    }
}

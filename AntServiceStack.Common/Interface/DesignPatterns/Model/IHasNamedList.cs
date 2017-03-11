using System;
using System.Collections.Generic;

namespace AntServiceStack.DesignPatterns.Model
{
    public interface IHasNamedList<T> : IHasNamed<IList<T>>
    {
    }
}
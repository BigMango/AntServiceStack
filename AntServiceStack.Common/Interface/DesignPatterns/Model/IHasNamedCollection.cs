using System.Collections.Generic;

namespace AntServiceStack.DesignPatterns.Model
{
    public interface IHasNamedCollection<T> : IHasNamed<ICollection<T>>
    {
    }
}
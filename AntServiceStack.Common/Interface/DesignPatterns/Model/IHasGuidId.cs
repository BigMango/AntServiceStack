using System;

namespace AntServiceStack.DesignPatterns.Model
{
    public interface IHasGuidId : IHasId<Guid>
    {
    }
}
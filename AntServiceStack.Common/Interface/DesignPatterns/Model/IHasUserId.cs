using System;

namespace AntServiceStack.DesignPatterns.Model
{
    public interface IHasUserId
    {
        Guid UserId { get; }
    }
}
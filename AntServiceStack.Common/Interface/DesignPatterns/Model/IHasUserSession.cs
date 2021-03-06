﻿using System;

namespace AntServiceStack.DesignPatterns.Model
{
    public interface IHasUserSession
    {
        Guid UserId { get; }

        Guid SessionId { get; }
    }
}
namespace CHystrix
{
    using System;

    internal interface ICircuitBreaker
    {
        bool AllowRequest();
        bool IsOpen();
        void MarkSuccess();
    }
}


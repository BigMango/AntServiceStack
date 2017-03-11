namespace CHystrix
{
    using System;

    internal interface IConfigChangeEvent
    {
        event HandleConfigChangeDelegate OnConfigChanged;

        void RaiseConfigChangeEvent();
    }
}


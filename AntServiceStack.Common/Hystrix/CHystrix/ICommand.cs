namespace CHystrix
{
    using System;

    public interface ICommand
    {
        string CommandKey { get; }

        string GroupKey { get; }

        string InstanceKey { get; }

        string Key { get; }

        CommandStatusEnum Status { get; }
    }
}


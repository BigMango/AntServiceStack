namespace CHystrix.Threading
{
    using System;
    using System.Runtime.CompilerServices;

    internal delegate void CThreadWorkCompleteCallback(CThread thread, ICWorkItem workItem);
}


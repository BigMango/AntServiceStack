namespace CHystrix.Threading
{
    using System;

    internal class StatusChangeEventArgs : EventArgs
    {
        private CTaskStatus _status;

        internal StatusChangeEventArgs(CTaskStatus status)
        {
            this._status = status;
        }

        public CTaskStatus Status
        {
            get
            {
                return this._status;
            }
        }
    }
}


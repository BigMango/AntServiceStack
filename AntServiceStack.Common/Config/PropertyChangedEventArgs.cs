using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Config
{
    public class PropertyChangedEventArgs : PropertyChangedEventArgs<string>
    {
        internal PropertyChangedEventArgs(string key, string oldValue, string newValue)
          : base(key, oldValue, newValue, DateTime.Now)
        {
        }

        internal PropertyChangedEventArgs(string key, string oldValue, string newValue, DateTime changedTime)
          : base(key, oldValue, newValue, changedTime)
        {
        }
    }

    public class PropertyChangedEventArgs<T> : EventArgs
    {
        private string _description;

        public string Key { get; private set; }

        public T OldValue { get; private set; }

        public T NewValue { get; private set; }

        public DateTime ChangedTime { get; private set; }

        internal PropertyChangedEventArgs(string key, T oldValue, T newValue)
          : this(key, oldValue, newValue, DateTime.Now)
        {
        }

        internal PropertyChangedEventArgs(string key, T oldValue, T newValue, DateTime changedTime)
        {
            this.Key = key;
            this.OldValue = oldValue;
            this.NewValue = newValue;
            this.ChangedTime = changedTime;
        }

        public override string ToString()
        {
            if (this._description == null)
                this._description = string.Format("{0} changed from {1} to {2} at {3}", (object)this.Key, (object)this.OldValue, (object)this.NewValue, (object)this.ChangedTime);
            return this._description;
        }
    }
}

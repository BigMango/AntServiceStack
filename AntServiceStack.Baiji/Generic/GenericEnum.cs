using AntServiceStack.Baiji.Exceptions;
using AntServiceStack.Baiji.Schema;

namespace AntServiceStack.Baiji.Generic
{
    /// <summary>
    /// The default class to hold values for enum schema in GenericReader and GenericWriter.
    /// </summary>
    public class GenericEnum
    {
        public EnumSchema Schema
        {
            get;
            private set;
        }

        private string value;

        public string Value
        {
            get
            {
                return value;
            }
            set
            {
                if (! Schema.Contains(value))
                {
                    throw new BaijiException("Unknown value for enum: " + value + "(" + Schema + ")");
                }
                this.value = value;
            }
        }

        public GenericEnum(EnumSchema schema, string value)
        {
            Schema = schema;
            Value = value;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            return obj is GenericEnum && Value.Equals((obj as GenericEnum).Value);
        }

        public override int GetHashCode()
        {
            return 17 * Value.GetHashCode();
        }

        public override string ToString()
        {
            return "Schema: " + Schema + ", value: " + Value;
        }
    }
}
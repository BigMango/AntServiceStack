using System.Collections.Generic;
using System.Text;
using AntServiceStack.Baiji.Exceptions;
using AntServiceStack.Baiji.Schema;
using AntServiceStack.Baiji.Utils;

namespace AntServiceStack.Baiji.Generic
{
    /// <summary>
    /// The default type used by GenericReader and GenericWriter for RecordSchema.
    /// </summary>
    public class GenericRecord
    {
        public RecordSchema Schema
        {
            get;
            private set;
        }

        private readonly IDictionary<string, object> _contents = new Dictionary<string, object>();

        public GenericRecord(RecordSchema schema)
        {
            Schema = schema;
        }

        public object this[string fieldName]
        {
            get
            {
                return _contents[fieldName];
            }
        }

        public void Add(string fieldName, object fieldValue)
        {
            if (Schema.Contains(fieldName))
            {
                // TODO: Use a matcher to verify that object has the right type for the field.
                //contents.Add(fieldName, fieldValue);
                _contents[fieldName] = fieldValue;
                return;
            }
            throw new BaijiException("No such field: " + fieldName);
        }

        public bool TryGetValue(string fieldName, out object result)
        {
            return _contents.TryGetValue(fieldName, out result);
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            var other = obj as GenericRecord;
            if (other == null)
            {
                return false;
            }
            return Schema.Equals(other.Schema) && ObjectUtils.AreEqual(_contents, other._contents);
        }

        public override int GetHashCode()
        {
            return 31 * _contents.GetHashCode() /* + 29 * Schema.GetHashCode()*/;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("Schema: ");
            sb.Append(Schema);
            sb.Append(", contents: ");
            sb.Append("{ ");
            foreach (var kv in _contents)
            {
                sb.Append(kv.Key);
                sb.Append(": ");
                sb.Append(kv.Value);
                sb.Append(", ");
            }
            sb.Append("}");
            return sb.ToString();
        }
    }
}
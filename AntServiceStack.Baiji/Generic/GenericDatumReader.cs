using System;
using System.Collections.Generic;
using AntServiceStack.Baiji.IO;
using AntServiceStack.Baiji.Schema;

namespace AntServiceStack.Baiji.Generic
{
    /// PreresolvingDatumReader for reading data to GenericRecord classes or primitives.
    /// <see cref="PreresolvingDatumReader">For more information about performance considerations for choosing this implementation</see>
    public class GenericDatumReader : PreresolvingDatumReader
    {
        public GenericDatumReader(Schema.Schema schema)
            : base(schema)
        {
        }

        protected override bool IsReusable(SchemaType type)
        {
            switch (type)
            {
                case SchemaType.Double:
                case SchemaType.Boolean:
                case SchemaType.Int:
                case SchemaType.Long:
                case SchemaType.Float:
                case SchemaType.Bytes:
                case SchemaType.String:
                case SchemaType.Null:
                case SchemaType.DateTime:
                    return false;
            }
            return true;
        }

        protected override ArrayAccess GetArrayAccess(ArraySchema schema)
        {
            return new GenericArrayAccess();
        }

        protected override EnumAccess GetEnumAccess(EnumSchema schema)
        {
            return new GenericEnumAccess(schema);
        }

        protected override MapAccess GetMapAccess(MapSchema schema)
        {
            return new GenericMapAccess();
        }

        protected override RecordAccess GetRecordAccess(RecordSchema schema)
        {
            return new GenericRecordAccess(schema);
        }

        private class GenericEnumAccess : EnumAccess
        {
            private readonly EnumSchema schema;

            public GenericEnumAccess(EnumSchema schema)
            {
                this.schema = schema;
            }

            public object CreateEnum(object reuse, int ordinal)
            {
                if (reuse is GenericEnum)
                {
                    var ge = (GenericEnum)reuse;
                    if (ge.Schema.Equals(schema))
                    {
                        ge.Value = schema.GetSymbol(ordinal);
                        return ge;
                    }
                }
                return new GenericEnum(schema, schema.GetSymbol(ordinal));
            }
        }

        internal class GenericRecordAccess : RecordAccess
        {
            private readonly RecordSchema _schema;

            public GenericRecordAccess(RecordSchema schema)
            {
                _schema = schema;
            }

            public object CreateRecord(object reuse)
            {
                var ru = (!(reuse is GenericRecord) || !(reuse as GenericRecord).Schema.Equals(_schema))
                    ? new GenericRecord(_schema)
                    : reuse as GenericRecord;
                return ru;
            }

            public object GetField(object record, string fieldName, int fieldPos)
            {
                object result;
                if (!((GenericRecord)record).TryGetValue(fieldName, out result))
                {
                    return null;
                }
                return result;
            }

            public void AddField(object record, string fieldName, int fieldPos, object fieldValue)
            {
                ((GenericRecord)record).Add(fieldName, fieldValue);
            }
        }

        private class GenericArrayAccess : ArrayAccess
        {
            public object Create(object reuse)
            {
                return (reuse is object[]) ? reuse : new object[0];
            }

            public void EnsureSize(ref object array, int targetSize)
            {
                if (((object[])array).Length < targetSize)
                {
                    SizeTo(ref array, targetSize);
                }
            }

            public void Resize(ref object array, int targetSize)
            {
                SizeTo(ref array, targetSize);
            }

            public void AddElements(object arrayObj, int elements, int index, ReadItem itemReader, IDecoder decoder,
                bool reuse)
            {
                var array = (object[])arrayObj;
                for (int i = index; i < index + elements; i++)
                {
                    array[i] = reuse ? itemReader(array[i], decoder) : itemReader(null, decoder);
                }
            }

            private static void SizeTo(ref object array, int targetSize)
            {
                var o = (object[])array;
                Array.Resize(ref o, targetSize);
                array = o;
            }
        }

        private class GenericMapAccess : MapAccess
        {
            public object Create(object reuse)
            {
                if (reuse is IDictionary<string, object>)
                {
                    var result = (IDictionary<string, object>)reuse;
                    result.Clear();
                    return result;
                }
                return new Dictionary<string, object>();
            }

            public void AddElements(object mapObj, int elements, ReadItem itemReader, IDecoder decoder, bool reuse)
            {
                var map = ((IDictionary<string, object>)mapObj);
                for (int i = 0; i < elements; i++)
                {
                    var key = decoder.ReadString();
                    map[key] = itemReader(null, decoder);
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using AntServiceStack.Baiji.Exceptions;
using AntServiceStack.Baiji.IO;
using AntServiceStack.Baiji.Schema;

namespace AntServiceStack.Baiji.Generic
{
    /// <summary>
    /// PreresolvingDatumWriter for writing data from GenericRecords or primitive types.
    /// <see cref="PreresolvingDatumWriter">For more information about performance considerations for choosing this implementation</see>
    /// </summary>
    public class GenericDatumWriter : PreresolvingDatumWriter
    {
        public GenericDatumWriter(Schema.Schema schema)
            : base(schema, new GenericArrayAccess(), new DictionaryMapAccess())
        {
        }

        protected override void WriteRecordFields(object recordObj, RecordFieldWriter[] writers, IEncoder encoder)
        {
            var record = (GenericRecord)recordObj;
            foreach (var writer in writers)
            {
                writer.WriteField(record[writer.Field.Name], encoder);
            }
        }

        protected override WriteItem ResolveEnum(EnumSchema es)
        {
            return (v, e) =>
            {
                if (v == null || !(v is GenericEnum) || !((v as GenericEnum).Schema.Equals(es)))
                {
                    throw TypeMismatch(v, "enum", "GenericEnum");
                }
                e.WriteEnum(es.Ordinal((v as GenericEnum).Value));
            };
        }

        /*
       * TODO: This method of determining the Union branch has problems. If the data is IDictionary<string, object>
       * if there are two branches one with record schema and the other with map, it choose the first one. Similarly if
       * the data is byte[] and there are fixed and bytes schemas as branches, it choose the first one that matches.
       * Also it does not recognize the arrays of primitive types.
       */
        protected override bool UnionBranchMatches(Schema.Schema sc, object obj)
        {
            if (obj == null && sc.Type != Baiji.Schema.SchemaType.Null) return false;
            switch (sc.Type)
            {
                case SchemaType.Null:
                    return obj == null;
                case SchemaType.Boolean:
                    return obj is bool;
                case SchemaType.Int:
                    return obj is int;
                case SchemaType.Long:
                    return obj is long;
                case SchemaType.Float:
                    return obj is float;
                case SchemaType.Double:
                    return obj is double;
                case SchemaType.Decimal:
                    return obj is decimal;
                case SchemaType.Bytes:
                    return obj is byte[];
                case SchemaType.DateTime:
                    return obj is DateTime;
                case SchemaType.String:
                    return obj is string;
                case SchemaType.Record:
                    //return obj is GenericRecord && (obj as GenericRecord).Schema.Equals(s);
                    return obj is GenericRecord && (obj as GenericRecord).Schema.SchemaName.Equals((sc as RecordSchema).SchemaName);
                case SchemaType.Enumeration:
                    //return obj is GenericEnum && (obj as GenericEnum).Schema.Equals(s);
                    return obj is GenericEnum && (obj as GenericEnum).Schema.SchemaName.Equals((sc as EnumSchema).SchemaName);
                case SchemaType.Array:
                    return obj is Array && !(obj is byte[]);
                case SchemaType.Map:
                    return obj is IDictionary<string, object>;
                case SchemaType.Union:
                    return false;   // Union directly within another union not allowed!
                default:
                    throw new BaijiException("Unknown schema type: " + sc.Type);
            }
        }

        private class GenericArrayAccess : ArrayAccess
        {
            public void EnsureArrayObject(object value)
            {
                if (value == null || !(value is Array))
                {
                    throw TypeMismatch(value, "array", "Array");
                }
            }

            public long GetArrayLength(object value)
            {
                int count = 0;
                var arrayInstance = (Array)value;
                for (int i = 0; i < arrayInstance.Length; i++)
                {
                    if (arrayInstance.GetValue(i) != null)
                        count++;
                }
                return count;
            }

            public void WriteArrayValues(object array, WriteItem valueWriter, IEncoder encoder)
            {
                var arrayInstance = (Array)array;
                for (int i = 0; i < arrayInstance.Length; i++)
                {
                    encoder.StartItem();
                    if (arrayInstance.GetValue(i) != null)
                    {
                        valueWriter(arrayInstance.GetValue(i), encoder);
                    }
                }
            }
        }
    }
}
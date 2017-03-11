using System;
using System.Collections;
using AntServiceStack.Baiji.Exceptions;
using AntServiceStack.Baiji.Generic;
using AntServiceStack.Baiji.IO;
using AntServiceStack.Baiji.Schema;
using System.Collections.Generic;

namespace AntServiceStack.Baiji.Specific
{
    /// <summary>
    /// PreresolvingDatumWriter for writing data from ISpecificRecord classes.
    /// <see cref="PreresolvingDatumWriter">For more information about performance considerations for choosing this implementation</see>
    /// </summary>
    public class SpecificDatumWriter : PreresolvingDatumWriter
    {
        public SpecificDatumWriter(Schema.Schema schema)
            : base(schema, new SpecificArrayAccess(), new DictionaryMapAccess())
        {
        }

        protected override void WriteRecordFields(object recordObj, RecordFieldWriter[] writers, IEncoder encoder)
        {
            var record = (ISpecificRecord)recordObj;
            for (int i = 0; i < writers.Length; i++)
            {
                var writer = writers[i];
                writer.WriteField(record.Get(writer.Field.Pos), encoder);
            }
        }

        protected override WriteItem ResolveEnum(EnumSchema es)
        {
            List<String> enumNames = new List<string>();
            foreach (var field in typeof(EnumSchema).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
            {
                if (field.IsLiteral)
                {
                    enumNames.Add(field.Name);
                }
            }
            int length = enumNames.ToArray().Length;
            var translator = new int[length];
            for (int i = 0; i < length; i++)
            {
                if (es.Contains(enumNames[i]))
                {
                    translator[i] = es.Ordinal(enumNames[i]);
                }
                else
                {
                    translator[i] = -1;
                }
            }

            return (v, e) =>
            {
                if (v == null)
                {
                    throw new BaijiTypeException("value is null in SpecificDatumWriter.WriteEnum");
                }
                e.WriteEnum(es.Ordinal(v.ToString()));
            };
        }

        protected override bool UnionBranchMatches(Schema.Schema sc, object obj)
        {
            if (obj == null && sc.Type != SchemaType.Null)
            {
                return false;
            }
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
                case SchemaType.Bytes:
                    return obj is byte[];
                case SchemaType.DateTime:
                    return obj is DateTime;
                case SchemaType.String:
                    return obj is string;
                case SchemaType.Record:
                    return obj is ISpecificRecord &&
                           (((obj as ISpecificRecord).GetSchema()) as RecordSchema).SchemaName.Equals(
                               (sc as RecordSchema).SchemaName);
                case SchemaType.Enumeration:
                    return obj.GetType().IsEnum && (sc as EnumSchema).Symbols.Contains(obj.ToString());
                case SchemaType.Array:
                    return obj is IList;
                case SchemaType.Map:
                    return obj is IDictionary;
                case SchemaType.Union:
                    return false; // Union directly within another union not allowed!
                default:
                    throw new BaijiException("Unknown schema type: " + sc.Type);
            }
        }

        private class SpecificArrayAccess : ArrayAccess
        {
            public void EnsureArrayObject(object value)
            {
                if (!(value is IList))
                {
                    throw new BaijiTypeException("Array does not implement non-generic IList");
                }
            }

            public long GetArrayLength(object value)
            {
                int count = 0;
                var list = (IList)value;
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] != null)
                        count++;
                }
                return count;
            }

            public void WriteArrayValues(object array, WriteItem valueWriter, IEncoder encoder)
            {
                var list = (IList)array;
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] != null)
                    {
                        valueWriter(list[i], encoder);
                    }
                }
            }
        }
    }
}
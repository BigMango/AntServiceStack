using System;
using System.Collections;
using AntServiceStack.Baiji.Exceptions;
using AntServiceStack.Baiji.Generic;
using AntServiceStack.Baiji.IO;
using AntServiceStack.Baiji.Schema;

namespace AntServiceStack.Baiji.Specific
{
    /// PreresolvingDatumReader for reading data to ISpecificRecord classes.
    /// <see cref="PreresolvingDatumReader">For more information about performance considerations for choosing this implementation</see>
    public class SpecificDatumReader : PreresolvingDatumReader
    {
        public SpecificDatumReader(Schema.Schema schema)
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
                case SchemaType.Enumeration:
                case SchemaType.String:
                case SchemaType.Null:
                case SchemaType.DateTime:
                    return false;
            }
            return true;
        }

        protected override ArrayAccess GetArrayAccess(ArraySchema schema)
        {
            return new SpecificArrayAccess(schema);
        }

        protected override EnumAccess GetEnumAccess(EnumSchema schema)
        {
            return new SpecificEnumAccess(schema);
        }

        protected override MapAccess GetMapAccess(MapSchema schema)
        {
            return new SpecificMapAccess(schema);
        }

        protected override RecordAccess GetRecordAccess(RecordSchema schema)
        {
            return new SpecificRecordAccess(schema);
        }

        private static ObjectCreator.CtorDelegate GetConstructor(string name, SchemaType schemaType)
        {
            var creator = ObjectCreator.Instance;
            return creator.GetConstructor(name, schemaType, creator.GetType(name, schemaType));
        }

        private class SpecificEnumAccess : EnumAccess
        {
            private readonly Type type;

            public SpecificEnumAccess(EnumSchema schema)
            {
                type = ObjectCreator.Instance.GetType(schema);
            }

            public object CreateEnum(object reuse, int ordinal)
            {
                return Enum.ToObject(type, ordinal);
            }
        }

        private class SpecificRecordAccess : RecordAccess
        {
            private readonly ObjectCreator.CtorDelegate objCreator;

            public SpecificRecordAccess(RecordSchema schema)
            {
                objCreator = GetConstructor(schema.Fullname, SchemaType.Record);
            }

            public object CreateRecord(object reuse)
            {
                return reuse ?? objCreator();
            }

            public object GetField(object record, string fieldName, int fieldPos)
            {
                return ((ISpecificRecord)record).Get(fieldPos);
            }

            public void AddField(object record, string fieldName, int fieldPos, object fieldValue)
            {
                ((ISpecificRecord)record).Put(fieldPos, fieldValue);
            }
        }

        private class SpecificArrayAccess : ArrayAccess
        {
            private readonly ObjectCreator.CtorDelegate objCreator;

            public SpecificArrayAccess(ArraySchema schema)
            {
                bool nEnum = false;
                string type = TypeHelper.GetType(schema, false, ref nEnum);
                type = type.Remove(0, 5); // remove List<
                type = type.Remove(type.Length - 1); // remove >

                objCreator = GetConstructor(type, SchemaType.Array);
            }

            public object Create(object reuse)
            {
                IList array;

                if (reuse != null)
                {
                    array = reuse as IList;
                    if (array == null)
                    {
                        throw new BaijiException("array object does not implement non-generic IList");
                    }
                    // retaining existing behavior where array contents aren't reused
                    // TODO: try to reuse contents?
                    array.Clear();
                }
                else
                {
                    array = objCreator() as IList;
                }
                return array;
            }

            public void EnsureSize(ref object array, int targetSize)
            {
                // no action needed
            }

            public void Resize(ref object array, int targetSize)
            {
                // no action needed
            }

            public void AddElements(object array, int elements, int index, ReadItem itemReader, IDecoder decoder,
                bool reuse)
            {
                var list = (IList)array;
                for (int i = 0; i < elements; i++)
                {
                    list.Add(itemReader(null, decoder));
                }
            }
        }

        private class SpecificMapAccess : MapAccess
        {
            private readonly ObjectCreator.CtorDelegate objCreator;

            public SpecificMapAccess(MapSchema schema)
            {
                bool nEnum = false;
                string type = TypeHelper.GetType(schema, false, ref nEnum);
                type = type.Remove(0, 18); // remove Dictionary<string,
                type = type.Remove(type.Length - 1); // remove >

                objCreator = GetConstructor(type.Trim(), SchemaType.Map);
            }

            public object Create(object reuse)
            {
                IDictionary map;
                if (reuse != null)
                {
                    map = reuse as IDictionary;
                    if (map == null)
                    {
                        throw new BaijiException("map object does not implement non-generic IList");
                    }

                    map.Clear();
                }
                else
                {
                    map = objCreator() as IDictionary;
                }
                return map;
            }

            public void AddElements(object mapObj, int elements, ReadItem itemReader, IDecoder decoder, bool reuse)
            {
                var map = ((IDictionary)mapObj);
                for (int i = 0; i < elements; i++)
                {
                    var key = decoder.ReadString();
                    map[key] = itemReader(null, decoder);
                }
            }
        }
    }
}
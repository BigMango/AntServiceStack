using AntServiceStack.Baiji.Exceptions;
using AntServiceStack.Baiji.Schema;
using AntServiceStack.Baiji.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AntServiceStack.Baiji.Specific
{
    public class SpecificJsonParser
    {
        private delegate object ReadItem(object record);

        private static readonly IDictionary<Schema.Schema, ObjectCreator.CtorDelegate> _ctorCache =
            new Dictionary<Schema.Schema, ObjectCreator.CtorDelegate>();

        private static readonly IDictionary<Schema.Schema, Type> _typeCache =
            new Dictionary<Schema.Schema, Type>();

        private Schema.Schema _schema;
        private Encoding _encoding;

        public SpecificJsonParser(Schema.Schema schema, Encoding encoding)
        {
            _schema = schema;
            _encoding = encoding;
        }

        public object Parse(object reuse, Stream stream)
        {
            return Parse<object>(reuse, stream);
        }

        public T Parse<T>(T reuse, Stream stream)
        {
            RecordSchema recordSchema = (RecordSchema)_schema;
            if (recordSchema == null)
            {
                throw new BaijiException("schema mismatched.");
            }
            using (JsonReader jsonReader = new JsonTextReader(new StreamReader(stream, _encoding)))
            {
                jsonReader.DateParseHandling = DateParseHandling.None;
                jsonReader.CloseInput = false;
                var jsonObject = JObject.Load(jsonReader);
                return (T)ReadRecord(reuse, jsonObject, recordSchema);
            }
        }

        private object ReadRecord(object record, object reuse, RecordSchema recordSchema)
        {
            var ctor = GetConstructor(recordSchema, recordSchema.Fullname);
            record = record ?? ctor();
            var jsonObj = (JObject)reuse;
            foreach (KeyValuePair<string, JToken> p in jsonObj)
            {
                Field field;
                recordSchema.TryGetField(p.Key, out field);
                if (field == null)
                    continue;
                ReadItem readField = ResolveItemReader(field.Schema);
                var fieldValue = readField(p.Value);
                Put(record, field.Pos, fieldValue);
            }
            return record;
        }

        private object ReadEnum(object reuse, EnumSchema enumSchema)
        {
            var enumType = LoadType(enumSchema);
            return Enum.Parse(enumType, (string)((JValue)reuse).Value, true);
        }

        private IList ReadArray(object reuse, ArraySchema arraySchema)
        {
            var itemType = LoadType(arraySchema.ItemSchema);
            var ctor = GetConstructor(arraySchema, itemType.ToString());
            var array = (IList)ctor();
            var arrayItems = (JArray)reuse;
            ReadItem readArrayItem = ResolveItemReader(arraySchema.ItemSchema);
            for (int i = 0; i < arrayItems.Count; i++)
            {
                var arrayItem = arrayItems[i];
                array.Add(readArrayItem(arrayItem.Value<object>()));
            }
            return array;
        }

        private IDictionary ReadMap(object reuse, MapSchema mapSchema)
        {
            var valueType = LoadType(mapSchema.ValueSchema);
            var ctor = GetConstructor(mapSchema, valueType.ToString());
            var dictionary = (IDictionary)ctor();
            var entries = ((JObject)reuse).ToObject<Dictionary<string, object>>();
            ReadItem readValue = ResolveItemReader(mapSchema.ValueSchema);
            entries.ToList()
                .ForEach(m =>
                {
                    var key = m.Key;
                    var value = readValue(m.Value);
                    dictionary.Add(key, value);
                });
            return dictionary;
        }

        private object ReadUnion(object reuse, UnionSchema unionSchema)
        {
            ReadItem readItem = null;
            for (int i = 0; i < unionSchema.Count; i++)
            {
                var writerBranch = unionSchema[i];
                if (writerBranch.Type == SchemaType.Null)
                    continue;
                readItem = ResolveItemReader(writerBranch);
                break;
            }
            if (readItem == null)
                throw new BaijiException("Corresponding schema not found.");
            return readItem(reuse);
        }

        private object ReadValue(object source)
        {
            var value = source is JValue ? ((JValue)source).Value : source;
            return value;
        }

        private byte[] ReadBytes(object source)
        {
            var value = source is JValue ? ((JValue)source).Value : source;
            return Convert.FromBase64String((string)value);
        }

        private object ReadNull()
        {
            return null;
        }

        private void Put(object obj, int fieldPos, object fieldValue)
        {
            ((ISpecificRecord)obj).Put(fieldPos, fieldValue);
        }

        private void Put(object obj, string fieldName, object fieldValue)
        {
            ((ISpecificRecord)obj).Put(fieldName, fieldValue);
        }

        private ReadItem ResolveItemReader(Schema.Schema schema)
        {
            ReadItem readItem;
            switch (schema.Type)
            {
                case SchemaType.Null:
                    return readItem = (rec) => ReadNull();

                case SchemaType.Int:
                    {
                        readItem = (rec) => Convert.ToInt32(ReadValue(rec));
                        return readItem;
                    }
                case SchemaType.Long:
                    {
                        readItem = (rec) => Convert.ToInt64(ReadValue(rec));
                        return readItem;
                    }
                case SchemaType.Float:
                    {
                        readItem = (rec) => Convert.ToSingle(ReadValue(rec));
                        return readItem;
                    }
                case SchemaType.Short:
                    {
                        readItem = (rec) => Convert.ToInt16(ReadValue(rec));
                        return readItem;
                    }
                case SchemaType.Byte:
                    {
                        readItem = (rec) => Convert.ToByte(ReadValue(rec));
                        return readItem;
                    }
                case SchemaType.Double:
                    {
                        readItem = (rec) => Convert.ToDouble(ReadValue(rec));
                        return readItem;
                    }
                case SchemaType.Decimal:
                    {
                        readItem = (rec) => Convert.ToDecimal(ReadValue(rec));
                        return readItem;
                    }
                case SchemaType.Boolean:
                case SchemaType.String:
                    {
                        readItem = (rec) => ReadValue(rec);
                        return readItem;
                    }
                case SchemaType.Bytes:
                    return readItem = (rec) => ReadBytes(rec);
                case SchemaType.DateTime:
                    return readItem = (rec) => DateTimeUtils.GetDateFromTimeIntervalString((string)ReadValue(rec));
                case SchemaType.Record:
                    {
                        return readItem = (rec) => ReadRecord(null, rec, (RecordSchema)schema);
                    }
                case SchemaType.Enumeration:
                    {
                        readItem = (rec) => ReadEnum(rec, (EnumSchema)schema);
                        return readItem;
                    }
                case SchemaType.Array:
                    {
                        readItem = (rec) => ReadArray(rec, (ArraySchema)schema);
                        return readItem;
                    }
                case SchemaType.Map:
                    return readItem = (rec) => ReadMap(rec, (MapSchema)schema);

                case SchemaType.Union:
                    return readItem = (rec) => ReadUnion(rec, (UnionSchema)schema);

                default:
                    throw new BaijiException("Unknown schema type: " + schema);
            }
        }

        private static ObjectCreator.CtorDelegate GetConstructor(Schema.Schema schema, string name)
        {
            ObjectCreator.CtorDelegate ctorDelegate;
            if (!_ctorCache.TryGetValue(schema, out ctorDelegate))
            {
                lock (_ctorCache)
                {
                    if (!_ctorCache.TryGetValue(schema, out ctorDelegate))
                    {
                        ctorDelegate = CreateConstructor(schema, name);
                        _ctorCache[schema] = ctorDelegate;
                    }
                }
            }
            return ctorDelegate;
        }

        private static ObjectCreator.CtorDelegate CreateConstructor(Schema.Schema schema, string name)
        {
            var creator = ObjectCreator.Instance;
            return creator.GetConstructor(name, schema.Type, LoadType(schema));
        }

        private static Type LoadType(Schema.Schema schema)
        {
            Type type;
            if (!_typeCache.TryGetValue(schema, out type))
            {
                lock (_typeCache)
                {
                    if (!_typeCache.TryGetValue(schema, out type))
                    {
                        type = ObjectCreator.Instance.GetType(schema);
                        _typeCache[schema] = type;
                    }
                }
            }
            return type;
        }
    }
}
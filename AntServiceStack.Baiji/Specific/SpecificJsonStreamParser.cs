using AntServiceStack.Baiji.Exceptions;
using AntServiceStack.Baiji.Schema;
using AntServiceStack.Baiji.Utils;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AntServiceStack.Baiji.Specific
{
    public class SpecificJsonStreamParser
    {
        private delegate object ReadItem(JsonReader reader);
        private delegate object ReadNumber(object number);
        private delegate bool TokenTypePredicate(JsonToken tokenType);

        private static readonly IDictionary<Schema.Schema, ObjectCreator.CtorDelegate> _ctorCache =
            new Dictionary<Schema.Schema, ObjectCreator.CtorDelegate>();

        private static readonly IDictionary<Schema.Schema, Type> _typeCache =
            new Dictionary<Schema.Schema, Type>();

        private Schema.Schema _schema;
        private Encoding _encoding;

        public SpecificJsonStreamParser(Schema.Schema schema, Encoding encoding)
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
            using (JsonReader jr = new JsonTextReader(new StreamReader(stream, _encoding)))
            {
                jr.DateParseHandling = DateParseHandling.None;
                jr.CloseInput = false;
                jr.Read();
                return (T)ReadRecord(jr, recordSchema);
            }
        }

        private object ReadRecord(JsonReader jr, RecordSchema recordSchema)
        {
            if (jr.TokenType == JsonToken.Null)
                return null;

            if (jr.TokenType != JsonToken.StartObject) 
                throw new BaijiRuntimeException("An object should start with '{'.");

            var ctor = GetConstructor(recordSchema, recordSchema.Fullname);
            var recordObject = ctor();
            while (jr.Read() && jr.TokenType != JsonToken.EndObject)
            {
                string fieldName = jr.Value.ToString();
                Field field;
                recordSchema.TryGetField(fieldName.ToLower(), out field);
                if (field == null)
                {
                    ReadDummyData(jr);
                    continue;
                }
                ReadItem readField = ResolveItemReader(field.Schema);

                jr.Read();
                var fieldValue = readField(jr);
                Put(recordObject, field.Pos, fieldValue);
            }
            return recordObject;
        }

        private object ReadValue(JsonReader jr, Type type, TokenTypePredicate predicate, ReadNumber readNumber = null)
        {
            if (type.IsValueType && jr.TokenType == JsonToken.Null)
                throw new BaijiRuntimeException("An value type can not be null.");

            if (predicate != null && !predicate(jr.TokenType))
            {
                string valueType = jr.ValueType == null ? jr.TokenType.ToString() : jr.ValueType.ToString();
                string message = string.Format("Can not convert {0} to {1}", valueType, type);
                throw new BaijiRuntimeException(message);
            }

            if (readNumber != null && jr.ValueType != type)
                return readNumber(jr.Value);

            return jr.Value;
        }

        private object ReadValue(JsonReader jr, Type type, JsonToken expectedTokenType, ReadNumber readNumber = null)
        {
            return ReadValue(jr, type, tokenType => tokenType == expectedTokenType, readNumber);
        }

        private object ReadValue(JsonReader jr)
        {
            return jr.Value;
        }

        private bool NumberTokenPredicate(JsonToken tokenType)
        {
            return tokenType == JsonToken.Integer || tokenType == JsonToken.Float;
        }

        private object ReadNull()
        {
            return null;
        }

        private object ReadBytes(JsonReader jr)
        {
            if (jr.TokenType == JsonToken.Null)
                return null;

            return Convert.FromBase64String((string)jr.Value);
        }

        private object ReadDate(JsonReader jr)
        {
            return DateTimeUtils.GetDateFromTimeIntervalString((string)jr.Value);
        }

        private object ReadEnum(JsonReader jr, EnumSchema enumSchema)
        {
            var enumType = LoadType(enumSchema);
            return Enum.Parse(enumType, (string)jr.Value, true);
        }

        private object ReadArray(JsonReader jr, ArraySchema arraySchema)
        {
            if (jr.TokenType == JsonToken.Null)
                return null;

            if (jr.TokenType != JsonToken.StartArray) 
                throw new BaijiRuntimeException("An array should start with '['.");
            
            var itemType = LoadType(arraySchema);
            var ctor = GetConstructor(arraySchema, itemType.ToString());
            var array = (IList)ctor();
            ReadItem readArrayItem = ResolveItemReader(arraySchema.ItemSchema);
            while (jr.Read() && jr.TokenType != JsonToken.EndArray)
            {
                if (jr.TokenType == JsonToken.Null)
                    continue;
                var arrayItem = readArrayItem(jr);
                array.Add(arrayItem);
            }
            return array;
        }

        private object ReadMap(JsonReader jr, MapSchema mapSchema)
        {
            if (jr.TokenType == JsonToken.Null)
                return null;

            if (jr.TokenType != JsonToken.StartObject)
                throw new BaijiRuntimeException("A map should start with '{'.");

            var valueType = LoadType(mapSchema.ValueSchema);
            var ctor = GetConstructor(mapSchema, valueType.ToString());
            var dictionary = (IDictionary)ctor();
            ReadItem readValueItem = ResolveItemReader(mapSchema.ValueSchema);
            while (jr.Read() && jr.TokenType != JsonToken.EndObject)
            {
                string key = jr.Value.ToString();

                jr.Read();
                var valueItem = readValueItem(jr);
                dictionary.Add(key, valueItem);
            }
            return dictionary;
        }

        private object ReadUnion(JsonReader jr, UnionSchema unionSchema)
        {
            ReadItem readMember = null;
            bool canBeNull = false;
            foreach (Schema.Schema memberSchema in unionSchema.Schemas)
            {
                if (memberSchema.Type == SchemaType.Null)
                {
                    canBeNull = true;
                    continue;
                }
                if (readMember == null)
                    readMember = ResolveItemReader(memberSchema);
            }

            if (canBeNull && jr.TokenType == JsonToken.Null)
                return null;
            if (readMember == null)
                throw new BaijiException("Corresponding schema not found.");
            return readMember(jr);
        }

        private void ReadDummyData(JsonReader jr)
        {
            jr.Read();
            if (jr.TokenType == JsonToken.StartArray || jr.TokenType == JsonToken.StartConstructor || jr.TokenType == JsonToken.StartObject)
                ReadDummyContainerObject(jr);
        }

        private void ReadDummyContainerObject(JsonReader jr)
        {
            Stack<JsonToken> stack = new Stack<JsonToken>();
            stack.Push(jr.TokenType);
            while (stack.Count > 0 && jr.Read())
            {
                switch (jr.TokenType)
                {
                    case JsonToken.StartArray:
                    case JsonToken.StartConstructor:
                    case JsonToken.StartObject:
                        stack.Push(jr.TokenType);
                        break;
                    case JsonToken.EndArray:
                        if (stack.Peek() == JsonToken.StartArray)
                            stack.Pop();
                        else
                            throw new BaijiRuntimeException("invalid json object");
                        break;
                    case JsonToken.EndConstructor:
                        if (stack.Peek() == JsonToken.StartConstructor)
                            stack.Pop();
                        else
                            throw new BaijiRuntimeException("invalid json object");
                        break;
                    case JsonToken.EndObject:
                        if (stack.Peek() == JsonToken.StartObject)
                            stack.Pop();
                        else
                            throw new BaijiRuntimeException("invalid json object");
                        break;
                }
            }
        }

        private ReadItem ResolveItemReader(Schema.Schema schema)
        {
            ReadItem readItem;
            switch (schema.Type)
            {
                case SchemaType.Null:
                    return readItem = (jr) => ReadNull();
                case SchemaType.Int:
                    return readItem = (jr) => ReadValue(jr, typeof(int), NumberTokenPredicate, value => Convert.ToInt32(value));
                case SchemaType.Float:
                    return readItem = (jr) => ReadValue(jr, typeof(float), NumberTokenPredicate, value => Convert.ToSingle(value));
                case SchemaType.String:
                    return readItem = (jr) => ReadValue(jr, typeof(string), JsonToken.String);
                case SchemaType.Boolean:
                    return readItem = (jr) => ReadValue(jr, typeof(bool), JsonToken.Boolean);
                case SchemaType.Long:
                    return readItem = (jr) => ReadValue(jr, typeof(long), NumberTokenPredicate, value => Convert.ToInt64(value));
                case SchemaType.Double:
                    return readItem = (jr) => ReadValue(jr, typeof(double), NumberTokenPredicate, value => Convert.ToDouble(value));
                case SchemaType.Bytes:
                    return readItem = (jr) => ReadBytes(jr);
                case SchemaType.DateTime:
                    return readItem = (jr) => ReadDate(jr);
                case SchemaType.Record:
                    return readItem = (jr) => ReadRecord(jr, (RecordSchema)schema);
                case SchemaType.Enumeration:
                    return readItem = (jr) => ReadEnum(jr, (EnumSchema)schema);
                case SchemaType.Array:
                    return readItem = (jr) => ReadArray(jr, (ArraySchema)schema);
                case SchemaType.Map:
                    return readItem = (jr) => ReadMap(jr, (MapSchema)schema);
                case SchemaType.Union:
                    return readItem = (jr) => ReadUnion(jr, (UnionSchema)schema);

                default:
                    throw new BaijiException("Unknown schema type: " + schema);
            }
        }

        private void Put(object obj, int fieldPos, object fieldValue)
        {
            ((ISpecificRecord)obj).Put(fieldPos, fieldValue);
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

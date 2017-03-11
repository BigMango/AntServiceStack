using AntServiceStack.Baiji.Exceptions;
using AntServiceStack.Baiji.Schema;
using AntServiceStack.Baiji.Utils;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using System.Text;

namespace AntServiceStack.Baiji.Specific
{
    public class SpecificJsonWriter
    {
        private Schema.Schema _schema;
        private Encoding _encoding;

        private delegate void WriteItem(object source, JsonWriter writer);

        public SpecificJsonWriter(Schema.Schema schema, Encoding encoding)
        {
            _schema = schema;
            _encoding = encoding;
        }

        public void Write(ISpecificRecord obj, Stream stream)
        {
            StreamWriter sw = new StreamWriter(stream, _encoding);
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.CloseOutput = false;
                WriteRecord(obj, writer, (RecordSchema)_schema);
                writer.Flush();
            }
        }

        private WriteItem ResolveItemWriter(Schema.Schema schema)
        {
            WriteItem writeItem;
            switch (schema.Type)
            {
                case SchemaType.Null:
                    return writeItem = (src, wr) => WriteNull();
                case SchemaType.Boolean:
                case SchemaType.Int:
                case SchemaType.Long:
                case SchemaType.Float:
                case SchemaType.Double:
                case SchemaType.String:
                case SchemaType.Bytes:
                    return writeItem = (src, wr) => WriteValue(src, wr);
                case SchemaType.DateTime:
                    return writeItem = (src, wr) => WriteValue(DateTimeUtils.GetTimeIntervalString((DateTime)src), wr);
                case SchemaType.Record:
                    return writeItem = (src, wr) => WriteRecord(src, wr, (RecordSchema)schema);
                case SchemaType.Enumeration:
                    return writeItem = (src, wr) => WriteEnum(src, wr, (EnumSchema)schema);
                case SchemaType.Array:
                    return writeItem = (src, wr) => WriteArray(src, wr, (ArraySchema)schema);
                case SchemaType.Map:
                    return writeItem = (src, wr) => WriteMap(src, wr, (MapSchema)schema);
                case SchemaType.Union:
                    return writeItem = (src, wr) => WriteUnion(src, wr, (UnionSchema)schema);
                default:
                    throw new BaijiException("Unknown schema type: " + schema);
            }
        }

        private void WriteRecord(object source, JsonWriter jsonWriter, RecordSchema recordSchema)
        {
            jsonWriter.WriteStartObject();
            var record = (ISpecificRecord)source;
            foreach (Field field in recordSchema.Fields)
            {
                var fieldVal = Get(record, field.Pos);
                if (fieldVal == null)
                {
                    if (!JsonConfig.IncludeNullValues)
                        continue;
                    jsonWriter.WritePropertyName(field.Name);
                    jsonWriter.WriteNull();
                }
                else
                {
                    WriteItem writeField = ResolveItemWriter(field.Schema);
                    jsonWriter.WritePropertyName(field.Name);
                    writeField(fieldVal, jsonWriter);
                }
            }
            jsonWriter.WriteEndObject();
        }

        private object Get(ISpecificRecord record, int fieldPos)
        {
            return record.Get(fieldPos);
        }

        private void WriteArray(object source, JsonWriter jsonWriter, ArraySchema arraySchema)
        {
            if (source == null)
            {
                return;
            }
            WriteItem writeArrayItem = ResolveItemWriter(arraySchema.ItemSchema);
            jsonWriter.WriteStartArray();
            var list = (IList)source;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == null)
                    continue;
                writeArrayItem(list[i], jsonWriter);
            }
            jsonWriter.WriteEndArray();
        }

        private void WriteMap(object source, JsonWriter jsonWriter, MapSchema mapSchema)
        {
            //key type is assumed as string by default
            var dictionary = (IDictionary)source;
            jsonWriter.WriteStartObject();
            WriteItem writeValue = ResolveItemWriter(mapSchema.ValueSchema);
            var enumerator = dictionary.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Key == null || enumerator.Value == null)
                    continue;
                jsonWriter.WritePropertyName((string)enumerator.Key);
                writeValue(enumerator.Value, jsonWriter);
            }
            jsonWriter.WriteEndObject();
        }

        private void WriteUnion(object source, JsonWriter jsonWriter, UnionSchema unionSchema)
        {
            WriteItem writeItem = null;
            for (int i = 0; i < unionSchema.Count; i++)
            {
                if (unionSchema[i].Type == SchemaType.Null)
                {
                    continue;
                }
                else
                {
                    writeItem = ResolveItemWriter(unionSchema[i]);
                    writeItem(source, jsonWriter);
                    return;
                }
            }
            throw new BaijiException("nullable schema cannot find.");

        }

        //enum value should represented as integer
        private void WriteEnum(object source, JsonWriter jsonWriter, EnumSchema enumSchema)
        {
            jsonWriter.WriteValue(source.ToString());
        }

        private void WriteValue(object source, JsonWriter jsonWriter)
        {
            //jsonWriter should recognize primitive values
            jsonWriter.WriteValue(source);
        }

        private void WriteNull()
        {
        }
    }
}
using System.Collections.Generic;
using AntServiceStack.Baiji.Exceptions;
using AntServiceStack.Baiji.IO;
using AntServiceStack.Baiji.Schema;

namespace AntServiceStack.Baiji.Generic
{
    /// <summary>
    /// A general purpose reader of data from Baiji streams. This reader analyzes and resolves the reader and writer schemas
    /// when constructed so that reads can be more efficient. Once constructed, a reader can be reused or shared among threads
    /// to avoid incurring more resolution costs.
    /// </summary>
    public abstract class PreresolvingDatumReader : DatumReader
    {
        public Schema.Schema Schema
        {
            get;
            private set;
        }

        protected delegate object ReadItem(object reuse, IDecoder dec);

        // read a specific field from a decoder
        private delegate object DecoderRead(IDecoder dec);

        // read & set fields on a record
        private delegate void FieldReader(object record, IDecoder decoder);

        private readonly ReadItem _reader;

        private readonly IDictionary<Schema.Schema, ReadItem> _recordReaders =
            new Dictionary<Schema.Schema, ReadItem>();

        protected PreresolvingDatumReader(Schema.Schema schema)
        {
            Schema = schema;
            _reader = ResolveReader(schema);
        }

        public object Read(object reuse, IDecoder decoder)
        {
            return _reader(reuse, decoder);
        }

        public T Read<T>(T reuse, IDecoder decoder)
        {
            return (T)_reader(reuse, decoder);
        }

        protected abstract ArrayAccess GetArrayAccess(ArraySchema schema);
        protected abstract EnumAccess GetEnumAccess(EnumSchema schema);
        protected abstract MapAccess GetMapAccess(MapSchema schema);
        protected abstract RecordAccess GetRecordAccess(RecordSchema schema);

        /// <summary>
        /// Build a reader that accounts for schema differences between the reader and writer schemas.
        /// </summary>
        private ReadItem ResolveReader(Schema.Schema schema)
        {
            switch (schema.Type)
            {
                case SchemaType.Null:
                    return ReadNull;
                case SchemaType.Boolean:
                    return Read(d => d.ReadBoolean());
                case SchemaType.Int:
                    return Read(d => d.ReadInt());
                case SchemaType.Long:
                    return Read(d => d.ReadLong());
                case SchemaType.Float:
                    return Read(d => d.ReadFloat());
                case SchemaType.Double:
                    return Read(d => d.ReadDouble());
                case SchemaType.String:
                    return Read(d => d.ReadString());
                case SchemaType.Bytes:
                    return Read(d => d.ReadBytes());
                case SchemaType.DateTime:
                    return Read(d => d.ReadDateTime());
                case SchemaType.Record:
                    return ResolveRecord((RecordSchema)schema);
                case SchemaType.Enumeration:
                    return ResolveEnum((EnumSchema)schema);
                case SchemaType.Array:
                    return ResolveArray((ArraySchema)schema);
                case SchemaType.Map:
                    return ResolveMap((MapSchema)schema);
                case SchemaType.Union:
                    return ResolveUnion((UnionSchema)schema);
                default:
                    throw new BaijiException("Unknown schema type: " + schema);
            }
        }

        private ReadItem ResolveEnum(EnumSchema schema)
        {
            var enumAccess = GetEnumAccess(schema);
            return (r, d) => enumAccess.CreateEnum(r, d.ReadEnum());
        }

        private ReadItem ResolveRecord(RecordSchema schema)
        {
            ReadItem recordReader;
            if (!_recordReaders.TryGetValue(schema, out recordReader))
            {
                lock (_recordReaders)
                {
                    if (!_recordReaders.TryGetValue(schema, out recordReader))
                    {
                        RecordAccess recordAccess = GetRecordAccess(schema);
                        var fieldReaders = new List<FieldReader>();
                        recordReader = (r, d) => ReadRecord(r, d, recordAccess, fieldReaders);
                        _recordReaders.Add(schema, recordReader);
                        foreach (Field f in schema)
                        {
                            Field field = f;
                            ReadItem readItem = ResolveReader(field.Schema);
                            if (IsReusable(field.Schema.Type))
                            {
                                fieldReaders.Add((rec, d) => recordAccess.AddField(rec, field.Name, field.Pos,
                                    readItem(recordAccess.GetField(rec, field.Name, field.Pos), d)));
                            }
                            else
                            {
                                fieldReaders.Add((rec, d) => recordAccess.AddField(rec, field.Name, field.Pos,
                                    readItem(null, d)));
                            }
                        }
                    }
                }
            }
            return recordReader;
        }

        private object ReadRecord(object reuse, IDecoder decoder, RecordAccess recordAccess,
            IEnumerable<FieldReader> readSteps)
        {
            var rec = recordAccess.CreateRecord(reuse);
            foreach (FieldReader fr in readSteps)
            {
                fr(rec, decoder);
                // TODO: on exception, report offending field
            }
            return rec;
        }

        private ReadItem ResolveUnion(UnionSchema schema)
        {
            var lookup = new ReadItem[schema.Count];

            for (int i = 0; i < schema.Count; i++)
            {
                lookup[i] = ResolveReader(schema[i]);
            }

            return (r, d) => ReadUnion(r, d, lookup);
        }

        private object ReadUnion(object reuse, IDecoder d, ReadItem[] branchLookup)
        {
            return branchLookup[d.ReadUnionIndex()](reuse, d);
        }

        private ReadItem ResolveMap(MapSchema schema)
        {
            var valueSchema = schema.ValueSchema;
            var reader = ResolveReader(valueSchema);
            var mapAccess = GetMapAccess(schema);
            return (r, d) => ReadMap(r, d, mapAccess, reader);
        }

        private object ReadMap(object reuse, IDecoder decoder, MapAccess mapAccess, ReadItem valueReader)
        {
            object map = mapAccess.Create(reuse);
            for (int n = (int)decoder.ReadMapStart(); n != 0; n = (int)decoder.ReadMapNext())
            {
                mapAccess.AddElements(map, n, valueReader, decoder, false);
            }
            return map;
        }

        private ReadItem ResolveArray(ArraySchema schema)
        {
            var itemReader = ResolveReader(schema.ItemSchema);
            var arrayAccess = GetArrayAccess(schema);
            return (r, d) => ReadArray(r, d, arrayAccess, itemReader, IsReusable(schema.ItemSchema.Type));
        }

        private object ReadArray(object reuse, IDecoder decoder, ArrayAccess arrayAccess, ReadItem itemReader,
            bool itemReusable)
        {
            object array = arrayAccess.Create(reuse);
            int i = 0;
            for (var n = (int)decoder.ReadArrayStart(); n != 0; n = (int)decoder.ReadArrayNext())
            {
                arrayAccess.EnsureSize(ref array, i + n);
                arrayAccess.AddElements(array, n, i, itemReader, decoder, itemReusable);
                i += n;
            }
            arrayAccess.Resize(ref array, i);
            return array;
        }

        private object ReadNull(object reuse, IDecoder decoder)
        {
            decoder.ReadNull();
            return null;
        }

        private ReadItem Read(DecoderRead decoderRead)
        {
            return (r, d) => decoderRead(d);
        }

        /// <summary>
        /// Indicates if it's possible to reuse an object of the specified type. Generally
        /// false for immutable objects like int, long, string, etc but may differ between
        /// the Specific and Generic implementations. Used to avoid retrieving the existing
        /// value if it's not reusable.
        /// </summary>
        protected virtual bool IsReusable(SchemaType tag)
        {
            return true;
        }

        // interfaces to handle details of working with Specific vs Generic objects

        protected interface RecordAccess
        {
            /// <summary>
            /// Creates a new record object. Derived classes can override this to return an object of their choice.
            /// </summary>
            /// <param name="reuse">If appropriate, will reuse this object instead of constructing a new one</param>
            /// <returns></returns>
            object CreateRecord(object reuse);

            /// <summary>
            /// Used by the default implementation of ReadRecord() to get the existing field of a record object. The derived
            /// classes can override this to make their own interpretation of the record object.
            /// </summary>
            /// <param name="record">The record object to be probed into. This is guaranteed to be one that was returned
            /// by a previous call to CreateRecord.</param>
            /// <param name="fieldName">The name of the field to probe.</param>
            /// <param name="fieldPos">field number</param>
            /// <returns>The value of the field, if found. Null otherwise.</returns>
            object GetField(object record, string fieldName, int fieldPos);

            /// <summary>
            /// Used by the default implementation of ReadRecord() to add a field to a record object. The derived
            /// classes can override this to suit their own implementation of the record object.
            /// </summary>
            /// <param name="record">The record object to be probed into. This is guaranteed to be one that was returned
            /// by a previous call to CreateRecord.</param>
            /// <param name="fieldName">The name of the field to probe.</param>
            /// <param name="fieldPos">field number</param>
            /// <param name="fieldValue">The value to be added for the field</param>
            void AddField(object record, string fieldName, int fieldPos, object fieldValue);
        }

        protected interface EnumAccess
        {
            object CreateEnum(object reuse, int ordinal);
        }

        protected interface ArrayAccess
        {
            /// <summary>
            /// Creates a new array object. The initial size of the object could be anything.
            /// </summary>
            /// <param name="reuse">If appropriate use this instead of creating a new one.</param>
            /// <returns>An object suitable to deserialize a Baiji array</returns>
            object Create(object reuse);

            /// <summary>
            /// Hint that the array should be able to handle at least targetSize elements. The array
            /// is not required to be resized
            /// </summary>
            /// <param name="array">Array object who needs to support targetSize elements. This is guaranteed to be somthing returned by
            /// a previous call to CreateArray().</param>
            /// <param name="targetSize">The new size.</param>
            void EnsureSize(ref object array, int targetSize);

            /// <summary>
            /// Resizes the array to the new value.
            /// </summary>
            /// <param name="array">Array object whose size is required. This is guaranteed to be something returned by
            /// a previous call to CreateArray().</param>
            /// <param name="targetSize">The new size.</param>
            void Resize(ref object array, int targetSize);

            void AddElements(object array, int elements, int index, ReadItem itemReader, IDecoder decoder, bool reuse);
        }

        protected interface MapAccess
        {
            /// <summary>
            /// Creates a new map object.
            /// </summary>
            /// <param name="reuse">If appropriate, use this map object instead of creating a new one.</param>
            /// <returns>An empty map object.</returns>
            object Create(object reuse);

            void AddElements(object map, int elements, ReadItem itemReader, IDecoder decoder, bool reuse);
        }
    }
}
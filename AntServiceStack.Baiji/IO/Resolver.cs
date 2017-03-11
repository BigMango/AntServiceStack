using System;
using System.Collections.Generic;
using System.Text;
using AntServiceStack.Baiji.Exceptions;
using AntServiceStack.Baiji.Schema;
using Newtonsoft.Json.Linq;

namespace AntServiceStack.Baiji.IO
{
    internal static class Resolver
    {
        /// <summary>
        /// Reads the passed JToken default value field and writes it in the specified encoder 
        /// </summary>
        /// <param name="enc">encoder to use for writing</param>
        /// <param name="schema">schema object for the current field</param>
        /// <param name="jtok">default value as JToken</param>
        public static void EncodeDefaultValue(IEncoder enc, Schema.Schema schema, JToken jtok)
        {
            if (null == jtok)
            {
                return;
            }

            switch (schema.Type)
            {
                case SchemaType.Boolean:
                    if (jtok.Type != JTokenType.Boolean)
                    {
                        throw new BaijiException("Default boolean value " + jtok +
                                                 " is invalid, expected is json boolean.");
                    }
                    enc.WriteBoolean((bool)jtok);
                    break;

                case SchemaType.Int:
                    if (jtok.Type != JTokenType.Integer)
                    {
                        throw new BaijiException("Default int value " + jtok + " is invalid, expected is json integer.");
                    }
                    enc.WriteInt(Convert.ToInt32((int)jtok));
                    break;

                case SchemaType.Long:
                    if (jtok.Type != JTokenType.Integer)
                    {
                        throw new BaijiException("Default long value " + jtok + " is invalid, expected is json integer.");
                    }
                    enc.WriteLong(Convert.ToInt64((long)jtok));
                    break;

                case SchemaType.Float:
                    if (jtok.Type != JTokenType.Float)
                    {
                        throw new BaijiException("Default float value " + jtok + " is invalid, expected is json number.");
                    }
                    enc.WriteFloat((float)jtok);
                    break;

                case SchemaType.Double:
                    if (jtok.Type == JTokenType.Integer)
                    {
                        enc.WriteDouble(Convert.ToDouble((int)jtok));
                    }
                    else if (jtok.Type == JTokenType.Float)
                    {
                        enc.WriteDouble(Convert.ToDouble((float)jtok));
                    }
                    else
                    {
                        throw new BaijiException("Default double value " + jtok +
                                                 " is invalid, expected is json number.");
                    }

                    break;

                case SchemaType.Bytes:
                    if (jtok.Type != JTokenType.String)
                    {
                        throw new BaijiException("Default bytes value " + jtok + " is invalid, expected is json string.");
                    }
                    var en = Encoding.GetEncoding("iso-8859-1");
                    enc.WriteBytes(en.GetBytes((string)jtok));
                    break;

                case SchemaType.String:
                    if (jtok.Type != JTokenType.String)
                    {
                        throw new BaijiException("Default string value " + jtok +
                                                 " is invalid, expected is json string.");
                    }
                    enc.WriteString((string)jtok);
                    break;

                case SchemaType.Enumeration:
                    if (jtok.Type != JTokenType.String)
                    {
                        throw new BaijiException("Default enum value " + jtok + " is invalid, expected is json string.");
                    }
                    enc.WriteEnum((schema as EnumSchema).Ordinal((string)jtok));
                    break;

                case SchemaType.Null:
                    if (jtok.Type != JTokenType.Null)
                    {
                        throw new BaijiException("Default null value " + jtok + " is invalid, expected is json null.");
                    }
                    enc.WriteNull();
                    break;

                case SchemaType.Array:
                    if (jtok.Type != JTokenType.Array)
                    {
                        throw new BaijiException("Default array value " + jtok + " is invalid, expected is json array.");
                    }
                    JArray jarr = jtok as JArray;
                    enc.WriteArrayStart();
                    enc.SetItemCount(jarr.Count);
                    foreach (JToken jitem in jarr)
                    {
                        enc.StartItem();
                        EncodeDefaultValue(enc, (schema as ArraySchema).ItemSchema, jitem);
                    }
                    enc.WriteArrayEnd();
                    break;

                case SchemaType.Record:
                    if (jtok.Type != JTokenType.Object)
                    {
                        throw new BaijiException("Default record value " + jtok +
                                                 " is invalid, expected is json object.");
                    }
                    RecordSchema rcs = schema as RecordSchema;
                    JObject jo = jtok as JObject;
                    foreach (Field field in rcs)
                    {
                        JToken val = jo[field.Name];
                        if (null == val)
                        {
                            val = field.DefaultValue;
                        }
                        if (null == val)
                        {
                            throw new BaijiException("No default value for field " + field.Name);
                        }

                        EncodeDefaultValue(enc, field.Schema, val);
                    }
                    break;

                case SchemaType.Map:
                    if (jtok.Type != JTokenType.Object)
                    {
                        throw new BaijiException("Default map value " + jtok + " is invalid, expected is json object.");
                    }
                    jo = jtok as JObject;
                    enc.WriteMapStart();
                    enc.SetItemCount(jo.Count);
                    foreach (KeyValuePair<string, JToken> jp in jo)
                    {
                        enc.StartItem();
                        enc.WriteString(jp.Key);
                        EncodeDefaultValue(enc, (schema as MapSchema).ValueSchema, jp.Value);
                    }
                    enc.WriteMapEnd();
                    break;

                case SchemaType.Union:
                    enc.WriteUnionIndex(0);
                    EncodeDefaultValue(enc, (schema as UnionSchema).Schemas[0], jtok);
                    break;

                default:
                    throw new BaijiException("Unsupported schema type " + schema.Type);
            }
        }
    }
}
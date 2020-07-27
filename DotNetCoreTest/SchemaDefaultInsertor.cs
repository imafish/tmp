using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetCoreTest
{
    class SchemaDefaultInsertor
    {
        public JToken InsertDefaultValuesToJsonObject(JSchema schema, JToken obj)
        {
            JToken result = DoInsertDefaults(schema, obj);

            return result;
        }

        public JToken InsertDefaultValuesToJsonObject(JsonReader schemaReader, JsonReader jsonReader) {
            var schema = JSchema.Load(schemaReader);
            var obj = JToken.Load(jsonReader);

            var result = InsertDefaultValuesToJsonObject(schema, obj);
            return result;
        }

        private JToken DoInsertDefaults(JSchema schema, JToken obj)
        {
            JToken result = obj;

            switch (schema.Type) {
                case JSchemaType.Array:
                    result = InsertDefaultsFromArray(schema, obj);
                    break;
                case JSchemaType.Object:
                    result = InsertDefaultsFromObject(schema, obj);
                    break;
                case JSchemaType.String:
                case JSchemaType.Integer:
                case JSchemaType.Boolean:
                case JSchemaType.Number:
                    result = InsertDefaultValue(schema, obj);
                    break;
                case null:
                case JSchemaType.None:
                case JSchemaType.Null:
                    break;
                default:
                    throw new NotImplementedException(String.Format("JSON schema type: {0} is not supportted.", schema.Type.ToString()));
            }

            return result;
        }

        private JToken InsertDefaultValue(JSchema schema, JToken obj)
        {
            if (schema.Default == null || obj != null)
            {
                return obj;
            }

            // TODO (XG): maybe add type assertions here
            return schema.Default;
        }

        private JToken InsertDefaultsFromObject(JSchema schema, JToken obj)
        {
            var schemaProperties = schema.Properties;
            if (schemaProperties?.Count == 0)
            {
                return obj;
            }
            // obj being null means the data from json string does not have this field.
            // no need to parse the schema's child tokens to get default values.
            if (obj == null)
            {
                return obj;
            }

            if (!(obj is JObject result))
            {
                throw new NullReferenceException(
                    string.Format("expect {0} to be a JSON object (JObject), got: {1}", nameof(obj), obj.Type.ToString()));
            }

            foreach (var property in schemaProperties.Keys)
            {
                result[property] = DoInsertDefaults(schemaProperties[property], result[property]);
            }
            return result;
        }

        private JToken InsertDefaultsFromArray(JSchema schema, JToken obj)
        {
            if (schema.Default == null || obj != null)
            {
                return obj;
            }
            if (!(schema.Default is JArray defaults))
            {
                throw new NullReferenceException(
                    string.Format("expect {0} to be a JSON array (JArray), got: {1}", nameof(schema.Default), schema.Default.Type.ToString()));
            }

            return defaults;
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetCoreTest
{
    /// <summary>
    /// <b>JsonDeserizliaserWithDefaults class deserialize a JSON text stream to an object.</b><br/>
    /// If schema contains default values, it fills the corresponding fields with defaults, if it's not already set.
    /// </summary>
    class JsonDeseriazliaserWithDefaults<T>
    {
        public T Deserialize(JsonReader jsonReader, JsonReader schemaReader)
        {
            var defaultInsertor = new SchemaDefaultInsertor();
            var token = defaultInsertor.InsertDefaultValuesToJsonObject(schemaReader, jsonReader);

            return token.ToObject<T>();
        }
    }
}

using System;
using Newtonsoft.Json;
using Terradue.OpenSearch.GeoJson.Result;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Terradue.GeoJson.Converter;
using Terradue.GeoJson.Geometry;
using Terradue.OpenSearch.GeoJson.Import;
using Terradue.ServiceModel.Syndication;
using System.Linq;
using System.Collections.Specialized;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Terradue.OpenSearch.GeoJson.Converter {
    public class FeatureResultJsonConverter : JsonConverter {
        public FeatureResultJsonConverter() {
        }

        #region implemented abstract members of JsonConverter

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            
            FeatureResult f = (FeatureResult)value;

            writer.WriteStartObject();

            if (f.Id != null){
                writer.WritePropertyName("id");
                writer.WriteValue(f.Id);
            }

            if (f.Properties != null) {
                writer.WritePropertyName("properties");
                serializer.Serialize(writer, f.Properties);
            }

            writer.WritePropertyName("type");
            serializer.Serialize(writer, (f.Type));

            if (f.Geometry != null) {
                writer.WritePropertyName("geometry");
                serializer.Serialize(writer, f.Geometry);
            }

            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {

            FeatureResult f = new FeatureResult();
            JToken jsont = serializer.Deserialize<JToken>(reader);

            if (jsont.SelectToken("id") != null)
                f.Id = jsont.SelectToken("id").ToString();
            
            if (jsont.SelectToken("properties") != null)
                GenericFunctions.ImportProperties(jsont.SelectToken("properties"), f);

            if (jsont.SelectToken("geometry") != null) {
                GeometryConverter geomConverter = new GeometryConverter();
                f.Geometry = (GeometryObject)geomConverter.ReadJson(jsont.SelectToken("geometry").CreateReader(), typeof(GeometryObject), jsont, serializer);
            }

            return f;

        }

        public override bool CanConvert(Type objectType) {
            return objectType == typeof(FeatureResult);
        }

        #endregion






    }
}


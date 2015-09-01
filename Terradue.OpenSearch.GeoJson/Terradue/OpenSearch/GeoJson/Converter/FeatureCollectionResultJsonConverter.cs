using System;
using Newtonsoft.Json;
using Terradue.OpenSearch.GeoJson.Result;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Converters;
using Terradue.GeoJson.Converter;

namespace Terradue.OpenSearch.GeoJson.Converter {
    public class FeatureCollectionResultJsonConverter : JsonConverter {
        public FeatureCollectionResultJsonConverter() {
        }

        #region implemented abstract members of JsonConverter

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {

            FeatureCollectionResult fc = (FeatureCollectionResult)value;

            serializer.Converters.Add(new FeatureResultJsonConverter());
            serializer.Converters.Add(new StringEnumConverter());
            //serializer.Converters.Add(new GeometryConverter());

            writer.WriteStartObject();

            if (fc.Id != null){
                writer.WritePropertyName("id");
                writer.WriteValue(fc.Id);
            }

            if (fc.Properties != null) {
                writer.WritePropertyName("properties");
                serializer.Serialize(writer, (fc.Properties));
            }
                
            if (fc.FeatureResults != null) {
                writer.WritePropertyName("features");
                serializer.Serialize(writer, fc.FeatureResults.ToArray());
            }

            writer.WritePropertyName("type");
            serializer.Serialize(writer, (fc.Type));

            writer.WriteEndObject();

        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {

            FeatureCollectionResult fc = new FeatureCollectionResult();

            JToken jsont = serializer.Deserialize<JToken>(reader);

            if (jsont.SelectToken("id") != null)
                fc.Id = jsont.SelectToken("id").ToString();
            if (jsont.SelectToken("properties") != null)
                GenericFunctions.ImportProperties(jsont.SelectToken("properties"), fc);
            
            var featureConverter = new FeatureResultJsonConverter();
            if (jsont.SelectToken("features") != null) {
                var featuresObject = jsont.SelectTokens("features").Children();
                var features = featuresObject.Select(featureO => featureConverter.ReadJson(featureO.CreateReader(), typeof(FeatureResult), featureO, serializer)).Cast<FeatureResult>().ToList();
                fc.FeatureResults = features;
            }

            return fc;

        }

        public override bool CanConvert(Type objectType) {
            return objectType == typeof(FeatureCollectionResult);
        }

        #endregion
    }
}


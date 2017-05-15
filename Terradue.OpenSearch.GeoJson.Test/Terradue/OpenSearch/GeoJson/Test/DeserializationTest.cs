using System;
using NUnit.Framework;
using Terradue.ServiceModel.Syndication;
using System.Xml;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.GeoJson.Result;
using Terradue.OpenSearch.GeoJson.Extensions;
using Terradue.OpenSearch.Request;
using System.IO;
using Terradue.OpenSearch.Engine;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Terradue.OpenSearch.GeoJson.Converter;
using Terradue.GeoJson.Geometry;

namespace Terradue.OpenSearch.GeoJson.Test {

    [TestFixture()]
    public class DeserializationTest{

        [Test()]
        public void FromJson() {

            var file = new FileStream("../Samples/test1.json", FileMode.Open, FileAccess.Read);

            FeatureCollectionResult fc = (FeatureCollectionResult)FeatureCollectionResult.DeserializeFromStream(file);

            Assert.That(fc.FeatureResults.Count == 2);

            file.Close();
            file.Dispose();
        }

        [Test()]
        public void FromComplexJson() {

            var fs = new FileStream("../Samples/ASA_IM__0.json", FileMode.Open, FileAccess.Read);

            FeatureCollectionResult fc = (FeatureCollectionResult)FeatureCollectionResult.DeserializeFromStream(fs);

            fs.Close();

            Assert.That(fc.FeatureResults.Count == 1);

            var file = new FileStream("../Samples/ASA_IM__0.json", FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(file);

            Terradue.OpenSearch.Response.MemoryOpenSearchResponse response = new Terradue.OpenSearch.Response.MemoryOpenSearchResponse(reader.ReadBytes(unchecked((int)file.Length)), "application/json");

            fc = FeatureCollectionOpenSearchEngineExtension.TransformJsonResponseToFeatureCollection(response);

            file.Close();
            file.Dispose();

            Assert.That(fc.FeatureResults.Count == 1);

            var serializer = JsonSerializer.Create(new JsonSerializerSettings(){NullValueHandling= NullValueHandling.Ignore});
            serializer.Converters.Add(new FeatureCollectionResultJsonConverter());

            string jsonouts;

            using (StringWriter sw = new StringWriter())
            using (JsonTextWriter jw = new JsonTextWriter(sw)) {
                serializer.Serialize(jw, fc);
                jsonouts = sw.ToString();
            }

            JToken jsonout;

            using (StringReader str = new StringReader(jsonouts))
            using (JsonTextReader jtr = new JsonTextReader(str)) {
                jsonout = serializer.Deserialize<JToken>(jtr);
            }

            JToken jsonin;
            file = new FileStream("../Samples/ASA_IM__0.json", FileMode.Open);
            using (var sr2 = new StreamReader(file))
            using (var jsonTextReader = new JsonTextReader(sr2))
            {
                jsonin = serializer.Deserialize<JToken>(jsonTextReader);
            }
            file.Close();

            Assert.IsTrue(JToken.DeepEquals(jsonin, jsonout));

           
        }

        [Test()]
        public void FromJsonS1doublePoly()
        {

            var file = new FileStream("../Samples/S1doublepoly.json", FileMode.Open, FileAccess.Read);

            FeatureCollectionResult fc = (FeatureCollectionResult)FeatureCollectionResult.DeserializeFromStream(file);

            file.Close();
            file.Dispose();

            Assert.That(fc.FeatureResults.Count == 1);

            Assert.That(fc.Features.First().Geometry is Polygon);


        }
    }
}


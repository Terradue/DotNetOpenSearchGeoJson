using System;
using NUnit.Framework;
using Terradue.ServiceModel.Syndication;
using System.Xml;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.GeoJson.Result;
using Terradue.OpenSearch.GeoJson.Extensions;
using Terradue.OpenSearch.Request;
using System.IO;

namespace Terradue.OpenSearch.GeoJson.Test {

    [TestFixture()]
    public class DeserializationTest{

        [Test()]
        public void FromComplexJson() {

            FeatureCollectionResult fc = (FeatureCollectionResult)FeatureCollectionResult.DeserializeFromStream(new FileStream("../Samples/GDALDataResult.json", FileMode.Open, FileAccess.Read));

            Assert.That(fc.FeatureResults.Count == 50);

            var file = new FileStream("../Samples/GDALDataResult.json", FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(file);

            Terradue.OpenSearch.Response.MemoryOpenSearchResponse response = new Terradue.OpenSearch.Response.MemoryOpenSearchResponse(reader.ReadBytes(unchecked((int)file.Length)), "application/json");

            fc = FeatureCollectionOpenSearchEngineExtension.TransformJsonResponseToFeatureCollection(response);

            Assert.That(fc.FeatureResults.Count == 50);

        }
    }
}


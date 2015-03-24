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
using Mono.Addins;

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

            AddinManager.Initialize ();
            AddinManager.Registry.Update ();

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            GenericOpenSearchable entity = new GenericOpenSearchable(new OpenSearchUrl("http://sb-10-16-10-20.dev.terradue.int/sbws/wps/dcs-doris-ifg/0000012-150204105530785-oozie-oozi-W/results/search?format=json&count=3&startPage=&startIndex=1&q=&lang=&id="), ose);

            var results = ose.Query(entity, new System.Collections.Specialized.NameValueCollection());

            Assert.That(results.Result.Items.Count() == 3);

            entity = new GenericOpenSearchable(new OpenSearchUrl("https://data.terradue.com/gs/catalogue/tepqw/gtfeature/search?format=json&uid=S1A_EW_RAW__0SDH_20150323T073730_20150323T073759_005156_0067FF_2E86"), ose);

            results = ose.Query(entity, new System.Collections.Specialized.NameValueCollection());

            Assert.That(results.Result.Items.Count() == 1);

        }
    }
}


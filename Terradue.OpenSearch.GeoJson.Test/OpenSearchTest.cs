using System;
using NUnit.Framework;
using Terradue.ServiceModel.Syndication;
using System.Xml;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.GeoJson.Result;
using System.IO;
using Terradue.OpenSearch.Engine;
using Mono.Addins;
using System.Linq;
using Terradue.GeoJson.Geometry;

namespace Terradue.OpenSearch.GeoJson.Test {

    [TestFixture()]
    public class OpenSearchTest {

        [Test()]
        public void DiscoverDescription() {

            AddinManager.Initialize();
            AddinManager.Registry.Update(null);

            var ose = new OpenSearchEngine();
            ose.LoadPlugins();

            GenericOpenSearchable entity = new GenericOpenSearchable(new OpenSearchUrl("https://data.terradue.com/gs/catalogue/tepqw/gtfeature/search?format=json&uid=ASA_IMS_1PNUPA20090201_092428_000000162076_00079_36205_2699.N1"), ose);

            var results = ose.Query(entity, new System.Collections.Specialized.NameValueCollection(), "json");
            FeatureCollectionResult col = (FeatureCollectionResult)results;

            var properties = col.FeatureResults.First().Properties;

            Assert.That(col.Features[0].Geometry != null);
            Assert.That(!string.IsNullOrEmpty(col.Features[0].ToWkt()));

            results = ose.Query(entity, new System.Collections.Specialized.NameValueCollection(), "atom");

        }

    }
}


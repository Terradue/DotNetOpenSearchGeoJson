using System;
using NUnit.Framework;
using Terradue.ServiceModel.Syndication;
using System.Xml;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.GeoJson.Result;
using System.IO;
using Terradue.OpenSearch.Engine;
using System.Linq;
using Terradue.GeoJson.Geometry;

namespace Terradue.OpenSearch.GeoJson.Test {

    [TestFixture()]
    public class OpenSearchTest {

        [Test()]
        public void DiscoverDescription() {

            var ose = new OpenSearchEngine();
            ose.LoadPlugins();

            GenericOpenSearchable entity = new GenericOpenSearchable(new OpenSearchUrl("https://catalog.terradue.com/noa-cosmoskymed/search?uid=CSKS4_SCS_B_HI_09_HH_RA_SF_20150911040406_20150911040413&format=json"), ose);

            var results = ose.Query(entity, new System.Collections.Specialized.NameValueCollection(), "json");
            FeatureCollectionResult col = (FeatureCollectionResult)results;

            var properties = col.FeatureResults.First().Properties;

            Assert.That(col.Features[0].Geometry != null);
            Assert.That(!string.IsNullOrEmpty(col.Features[0].ToWkt()));

            results = ose.Query(entity, new System.Collections.Specialized.NameValueCollection(), "atom");

        }

    }
}


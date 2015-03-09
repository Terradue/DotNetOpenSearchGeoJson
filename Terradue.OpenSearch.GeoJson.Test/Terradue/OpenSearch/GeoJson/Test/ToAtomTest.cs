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
    public class ToAtomTest {

        [Test()]
        public void FromLandsat8AtomTest() {

            var fc = FeatureCollectionResult.DeserializeFromStream(new FileStream("../Samples/ASA_IM__0.json", FileMode.Open, FileAccess.Read));

            var feed = AtomFeed.CreateFromOpenSearchResultCollection(fc);

            Assert.That(feed != null);

        }
    }
}


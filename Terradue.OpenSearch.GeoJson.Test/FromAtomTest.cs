using System;
using NUnit.Framework;
using Terradue.ServiceModel.Syndication;
using System.Xml;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.GeoJson.Result;

namespace Terradue.OpenSearch.GeoJson.Test {

    [TestFixture()]
    public class FromAtomTest {

        [Test()]
        public void FromLandsat8AtomTest() {

            XmlReader reader = XmlReader.Create("../Samples/landsat8.atom");
            SyndicationFeed feed = SyndicationFeed.Load(reader);
            AtomFeed atom = new AtomFeed(feed);

            FeatureCollectionResult col = FeatureCollectionResult.FromOpenSearchResultCollection(atom);

            Assert.That(col.Features[0].Geometry != null);

        }
    }
}


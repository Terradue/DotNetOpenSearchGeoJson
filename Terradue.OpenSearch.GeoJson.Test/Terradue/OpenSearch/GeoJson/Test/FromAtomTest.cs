using System;
using NUnit.Framework;
using Terradue.ServiceModel.Syndication;
using System.Xml;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.GeoJson.Result;
using ServiceStack.Text;
using System.Linq;

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
           
            JsonObject json = JsonSerializer.DeserializeFromString<JsonObject>(col.SerializeToString());

            Assert.AreEqual("ceos", json.Object("properties").ArrayObjects("authors").First().Child("identifier"));
            Assert.AreEqual("eros", json.ArrayObjects("features").First().Object("properties").ArrayObjects("authors").First().Child("identifier"));

            Assert.AreEqual("private", json.ArrayObjects("features").First().Object("properties").ArrayObjects("categories").First().Child("@label"));

        }

        [Test()]
        public void FromWPSAtomTest() {

            XmlReader reader = XmlReader.Create("../Samples/wps.atom");
            SyndicationFeed feed = SyndicationFeed.Load(reader);
            AtomFeed atom = new AtomFeed(feed);

            FeatureCollectionResult col = FeatureCollectionResult.FromOpenSearchResultCollection(atom);

            Console.Out.Write(col.SerializeToString());
            Assert.That(col.Features[0].Geometry != null);

        }
    }
}


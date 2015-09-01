using System;
using NUnit.Framework;
using Terradue.ServiceModel.Syndication;
using System.Xml;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.GeoJson.Result;
using System.Linq;
using Newtonsoft.Json.Linq;

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
           
            var jsont = col.SerializeToString();

            JToken json = JToken.Parse(jsont);

            Assert.AreEqual("ceos", json.SelectToken("properties").SelectToken("authors").First().SelectToken("identifier").ToString());
            Assert.AreEqual("eros", json.SelectToken("features").First().SelectToken("properties").SelectToken("authors").First().SelectToken("identifier").ToString());

            Assert.AreEqual("private", json.SelectToken("features").First().SelectToken("properties").SelectToken("categories").First().SelectToken("@label").ToString());

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


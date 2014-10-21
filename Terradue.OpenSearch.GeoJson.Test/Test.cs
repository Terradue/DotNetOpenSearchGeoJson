using NUnit.Framework;
using System;
using Terradue.ServiceModel.Syndication;
using System.Xml;
using System.IO;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.GeoJson.Result;
using ServiceStack.Text;
using System.Collections.Generic;
using System.Linq;

namespace Terradue.OpenSearch.GeoJson.Test {

    [TestFixture()]
    public class Test {

        [Test()]
        public void WPSAtomToJsonTest() {

            FileStream wps = new FileStream("../Samples/wps.xml", FileMode.Open);

            AtomFeed feed = new AtomFeed(DeserializeFromStream(wps));

            FeatureCollectionResult fc = FeatureCollectionResult.FromOpenSearchResultCollection(feed);

            Assert.That(((Dictionary<string, object>)fc.FeatureResults.First().Properties["offering"])["operation"] is List<object>);

        }

        public SyndicationFeed DeserializeFromStream(Stream stream) {
            var sr = XmlReader.Create(stream);
            Atom10FeedFormatter atomFormatter = new Atom10FeedFormatter();
            atomFormatter.ReadFrom(sr);
            sr.Close();
            return atomFormatter.Feed;
        }
    }
}


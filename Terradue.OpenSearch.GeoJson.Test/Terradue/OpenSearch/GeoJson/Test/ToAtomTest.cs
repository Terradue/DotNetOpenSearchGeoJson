using System;
using NUnit.Framework;
using Terradue.ServiceModel.Syndication;
using System.Xml;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.GeoJson.Result;
using Terradue.OpenSearch.GeoJson.Extensions;
using Terradue.OpenSearch.Request;
using System.IO;
using System.Linq;

namespace Terradue.OpenSearch.GeoJson.Test
{

    [TestFixture()]
    public class ToAtomTest
    {

        [Test()]
        public void FromLandsat8AtomTest()
        {

            var fs = new FileStream("../Samples/ASA_IM__0.json", FileMode.Open, FileAccess.Read);

            var fc = FeatureCollectionResult.DeserializeFromStream(fs);

            fs.Close();

            var feed = AtomFeed.CreateFromOpenSearchResultCollection(fc);

            Assert.That(feed != null);

        }

        [Test()]
        public void FromS1GRDJsonTest()
        {

            var fs = new FileStream("../Samples/S1_GRD.json", FileMode.Open, FileAccess.Read);

            var fc = FeatureCollectionResult.DeserializeFromStream(fs);

            fs.Close();

            var feed = AtomFeed.CreateFromOpenSearchResultCollection(fc);

            Assert.That(feed != null);

            string identifier = feed.Items.First().ElementExtensions.ReadElementExtensions<string>("identifier", "http://purl.org/dc/elements/1.1/").First();

            Assert.AreEqual("S1A_IW_GRDH_1SDV_20160731T181152_20160731T181219_012396_013552_7CC6", identifier);

        }
    }
}


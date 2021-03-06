using System;
using NUnit.Framework;
using Terradue.ServiceModel.Syndication;
using System.Xml;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.GeoJson.Result;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.IO;
using Terradue.GeoJson.Geometry;
using System.Collections.Generic;

namespace Terradue.OpenSearch.GeoJson.Test {

    [TestFixture()]
    public class FromAtomTest {

        [Test()]
        public void FromLandsat8AtomTest() {

            XmlReader reader = XmlReader.Create(Util.TestBaseDir + "/Samples/landsat8.atom");
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
        public void FromLandsat8_2AtomTest()
        {

            XmlReader reader = XmlReader.Create(Util.TestBaseDir + "/Samples/landsat8-2.atom");
            SyndicationFeed feed = SyndicationFeed.Load(reader);
            AtomFeed atom = new AtomFeed(feed);

            FeatureCollectionResult col = FeatureCollectionResult.FromOpenSearchResultCollection(atom);

            Assert.That(col.Features[0].Geometry != null);

            var jsont = col.SerializeToString();

            JToken json = JToken.Parse(jsont);

            Assert.That(json.SelectToken("features").First().SelectToken("properties").SelectToken("authors").First().SelectToken("name").ToString().Contains("EROS"));

            Assert.AreEqual(json.SelectToken("features").First().SelectToken("properties").SelectToken("polygon").ToString().Split(' ').First(),
                            (col.Features[0].Geometry as Polygon).Coordinates.First().First()[1].ToString());

        }

        [Test()]
        public void FromWPSAtomTest() {

            XmlReader reader = XmlReader.Create(Util.TestBaseDir + "/Samples/wps.atom");
            SyndicationFeed feed = SyndicationFeed.Load(reader);
            AtomFeed atom = new AtomFeed(feed);

            FeatureCollectionResult col = FeatureCollectionResult.FromOpenSearchResultCollection(atom);

            Console.Out.Write(col.SerializeToString());
            Assert.That(col.Features[0].Geometry != null);

        }

        [Test()]
        public void FromWPSAtomTest2() {

            XmlReader reader = XmlReader.Create(Util.TestBaseDir + "/Samples/wps2.xml");
            SyndicationFeed feed = SyndicationFeed.Load(reader);
            AtomFeed atom = new AtomFeed(feed);

            FeatureCollectionResult col = FeatureCollectionResult.FromOpenSearchResultCollection(atom);

            Console.Out.Write(col.SerializeToString());

        }

        [Test()]
        public void FromWPSAtomTest3() {

            XmlReader reader = XmlReader.Create(Util.TestBaseDir + "/Samples/test2.xml");
            SyndicationFeed feed = SyndicationFeed.Load(reader);
            AtomFeed atom = new AtomFeed(feed);

            FeatureCollectionResult col = FeatureCollectionResult.FromOpenSearchResultCollection(atom);

            Console.Out.Write(col.SerializeToString());

        }

        [Test()]
        public void FromWPSAtomNgeo() {

            XmlReader reader = XmlReader.Create(Util.TestBaseDir + "/Samples/ngeo.atom");
            SyndicationFeed feed = SyndicationFeed.Load(reader);
            AtomFeed atom = new AtomFeed(feed);

            FeatureCollectionResult col = FeatureCollectionResult.FromOpenSearchResultCollection(atom);

            Console.Out.Write(col.SerializeToString());

        }

        [Test()]
        public void FromOwsAtom() {

            FileStream file = new FileStream(Util.TestBaseDir + "/Samples/thematicapp.xml", FileMode.Open, FileAccess.Read);

            var xr = XmlReader.Create(file,new XmlReaderSettings(){
                IgnoreWhitespace = true
            });

            AtomFeed feed = AtomFeed.Load(xr);

            file.Close();

            FeatureCollectionResult fc = FeatureCollectionResult.FromOpenSearchResultCollection(feed);

            string json = fc.SerializeToString();
        }

		[Test()]
		public void FromDPAtom()
		{

			FileStream file = new FileStream(Util.TestBaseDir + "/Samples/dp.xml", FileMode.Open, FileAccess.Read);

			var xr = XmlReader.Create(file, new XmlReaderSettings() {
				IgnoreWhitespace = true
			});

			AtomFeed feed = AtomFeed.Load(xr);

			file.Close();

			FeatureCollectionResult fc = FeatureCollectionResult.FromOpenSearchResultCollection(feed);

			string json = fc.SerializeToString();
		}

		[Test()]
		public void FromASARAtom()
		{

			FileStream file = new FileStream(Util.TestBaseDir + "/Samples/ASAR.atom", FileMode.Open, FileAccess.Read);

			var xr = XmlReader.Create(file, new XmlReaderSettings() {
				IgnoreWhitespace = true
			});

			AtomFeed feed = AtomFeed.Load(xr);

			file.Close();

			FeatureCollectionResult fc = FeatureCollectionResult.FromOpenSearchResultCollection(feed);

			string json = fc.SerializeToString();
		}

        [Test()]
        public void FromS1MultiPolyAtom()
        {

            FileStream file = new FileStream(Util.TestBaseDir + "/Samples/S1multipoly.atom", FileMode.Open, FileAccess.Read);

            var xr = XmlReader.Create(file, new XmlReaderSettings()
            {
                IgnoreWhitespace = true
            });

            AtomFeed feed = AtomFeed.Load(xr);

            file.Close();

            FeatureCollectionResult fc = FeatureCollectionResult.FromOpenSearchResultCollection(feed);

            string json = fc.SerializeToString();

            Assert.That(fc.Features.First().Geometry is MultiPolygon);
        }

        [Test()]
        public void FromThemApp()
        {

            FileStream file = new FileStream(Util.TestBaseDir + "/Samples/themapp.xml", FileMode.Open, FileAccess.Read);

            var xr = XmlReader.Create(file, new XmlReaderSettings()
            {
                IgnoreWhitespace = true
            });

            AtomFeed feed = AtomFeed.Load(xr);

            file.Close();

            Uri relativeUri = new Uri("/test/test.html", UriKind.RelativeOrAbsolute);

            FeatureCollectionResult fc = FeatureCollectionResult.FromOpenSearchResultCollection(feed);

            Dictionary<string, object> prop = (Dictionary<string, object>)fc.FeatureResults.First().Properties;

            Dictionary<string, string> content = (Dictionary<string, string>)prop["content"];

            Assert.AreEqual("/geobrowser/nigerAppContent.html", content["src"]);

            string json = fc.SerializeToString();
        }

        [Test()]
        public void FromASA_INS_AXAtom()
        {

            FileStream file = new FileStream(Util.TestBaseDir + "/Samples/ASA_INS_AX.xml", FileMode.Open, FileAccess.Read);

            var xr = XmlReader.Create(file, new XmlReaderSettings()
            {
                IgnoreWhitespace = true
            });

            AtomFeed feed = AtomFeed.Load(xr);

            file.Close();

            FeatureCollectionResult fc = FeatureCollectionResult.FromOpenSearchResultCollection(feed);

            string json = fc.SerializeToString();
        }

        [Test()]
        public void FromAtomS1doublepoly()
        {

            FileStream file = new FileStream(Util.TestBaseDir + "/Samples/S1doublepoly.atom", FileMode.Open, FileAccess.Read);

            var xr = XmlReader.Create(file, new XmlReaderSettings()
            {
                IgnoreWhitespace = true
            });

            AtomFeed feed = AtomFeed.Load(xr);

            file.Close();

            FeatureCollectionResult fc = FeatureCollectionResult.FromOpenSearchResultCollection(feed);

            Assert.That(fc.Features.First().Geometry is Polygon);

            FileStream outs = new FileStream(Util.TestBaseDir + "/out/S1doublepoly.json", FileMode.Create, FileAccess.Write);

            fc.SerializeToStream(outs);

            outs.Close();

        }
        
        [Test()]
        public void FromIt4IAtom()
        {
            
            FileStream file = new FileStream(Util.TestBaseDir + "/Samples/it4i.xml", FileMode.Open, FileAccess.Read);
            
            var xr = XmlReader.Create(file, new XmlReaderSettings()
            {
                IgnoreWhitespace = true
            });
            
            AtomFeed feed = AtomFeed.Load(xr);
            
            file.Close();
            
            FeatureCollectionResult fc = FeatureCollectionResult.FromOpenSearchResultCollection(feed);
            
            Assert.That(fc.Features[0].Geometry != null);
            
            string json = fc.SerializeToString();
            
        }

        [Test()]
        public void FromGufAtomTest() {

            XmlReader reader = XmlReader.Create(Util.TestBaseDir + "/Samples/guf.atom");
            SyndicationFeed feed = SyndicationFeed.Load(reader);
            AtomFeed atom = new AtomFeed(feed);

            FeatureCollectionResult col = FeatureCollectionResult.FromOpenSearchResultCollection(atom);

            Assert.That(col.Features[0].Properties != null);

            var jsont = col.SerializeToString();

            JToken json = JToken.Parse(jsont);

            Assert.AreEqual("DLR", json.SelectToken("features").First().SelectToken("properties").SelectToken("authors").First().SelectToken("name").ToString());
            Assert.AreEqual("Terradue", json.SelectToken("features").First().SelectToken("properties").SelectToken("contributors").First().SelectToken("name").ToString());

        }

    }
}


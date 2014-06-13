//
//  ResultCollectionImporter.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using Terradue.ServiceModel.Syndication;
using System.Collections.Specialized;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.GeoJson.Result;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Terradue.OpenSearch.GeoJson.Import {
    public class ResultCollectionImporter {
        ImportOptions options;
        ImportUtils util;

        public ResultCollectionImporter(ImportOptions options) {
            this.options = options;
            util = new ImportUtils(options);
        }

        public FeatureCollectionResult ImportResults(IOpenSearchResultCollection results) {

            if (results == null)
                throw new ArgumentNullException("results");

            FeatureCollectionResult fc = new FeatureCollectionResult();
            fc.Properties = new Dictionary<string, object>();

            NameValueCollection namespaces = null;
            XmlNamespaceManager xnsm = null;
            string prefix = "";

            List<XmlElement> elements = new List<XmlElement>();
            foreach (var element in results.ElementExtensions) {
                elements.Add(element.GetObject<XmlElement>());
            }

            if (options.KeepNamespaces) {

                // Initialize namespaces
                namespaces = new NameValueCollection();
                namespaces.Set("", "http://geojson.org/ns#");
                namespaces.Set("atom", "http://www.w3.org/2005/Atom");
                foreach (var elem in results.ElementExtensions) {
                    XmlReader reader = elem.GetReader();
                    XmlDocument doc = new XmlDocument();
                    doc.Load(reader);
                    xnsm = new XmlNamespaceManager(doc.NameTable);
                    foreach (var names in xnsm.GetNamespacesInScope(XmlNamespaceScope.All)) {
                        namespaces.Set(names.Key, names.Value);
                    }
                }
                fc.Properties.Add("@namespaces", namespaces);
                prefix = "atom:";
            }

            fc.Properties = fc.Properties.Concat(util.ImportXmlDocument(elements.ToArray(), ref namespaces)).ToDictionary(x => x.Key, x => x.Value);

            if (results.Date != null && fc.Properties.ContainsKey(prefix + "updated") == null)
                fc.Properties.Add(prefix + "updated", results.Date.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"));

            if (results.Links != null && results.Links.Count > 0) {
                fc.Links = results.Links;
            }

            if (options.KeepNamespaces == true && fc.Properties.ContainsKey("dc:identifier") == null) {
                namespaces.Set("dc", "http://purl.org/dc/elements/1.1/");
                fc.Properties.Add("dc:identifier", results.Identifier);
            } 
            if (options.KeepNamespaces == false && fc.Properties.ContainsKey("identifier") == null) {
                fc.Properties.Add("identifier", results.Identifier);
            }

            if (results.Items != null) {
                foreach (var item in results.Items) {
                    fc.FeatureResults.Add(ImportItem(item));
                }
            }

            return fc;

        }

        public FeatureResult ImportItem(IOpenSearchResultItem result) {
            if (result == null)
                throw new ArgumentNullException("result");

            FeatureResult feature;
            XmlDocument doc = new XmlDocument();

            List<XmlElement> elements = new List<XmlElement>();
            foreach (var ext in result.ElementExtensions) {
                XmlElement element = ext.GetObject<XmlElement>();
                elements.Add(element);
            }

            XmlElement geometry = ImportUtils.FindGmlGeometry(elements.ToArray());
            if (geometry != null)
                feature = new FeatureResult(Terradue.GeoJson.Geometry.GeometryFactory.GmlToFeature(geometry));
            else {
                geometry = util.FindDctSpatialGeometry(elements.ToArray());
                if (geometry != null)
                    feature = new FeatureResult(Terradue.GeoJson.Geometry.GeometryFactory.WktToFeature(geometry.InnerXml));
                else
                    feature = new FeatureResult(Terradue.GeoJson.Geometry.GeometryFactory.WktToFeature(null));
            }

            NameValueCollection namespaces = null;
            XmlNamespaceManager xnsm = null;
            string prefix = "";
           
            feature.Id = result.Id;
            feature.Identifier = result.identifier;
            if (result.Date != null)
                feature.Properties.Add(prefix + "published", result.Date.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"));
            if (result.Links != null && result.Links.Count > 0) {
                feature.Links = result.Links;
            }
            if (result.Title != null)
                feature.Title = result.Title;

            return feature;
        }
    }
}


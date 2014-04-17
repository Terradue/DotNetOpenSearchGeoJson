//
//  ResultCollectionImporter.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.ServiceModel.Syndication;
using System.Collections.Specialized;
using System.Xml;
using System.Collections.Generic;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.GeoJson.Result;
using System.Linq;

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

            if (options.KeepNamespaces) {

                // Initialize namespaces
                namespaces = new NameValueCollection();
                namespaces.Set("", "http://geojson.org/ns#");
                namespaces.Set("atom", "http://www.w3.org/2005/Atom");
                foreach (XmlNode node in results.ElementExtensions) {
                    xnsm = new XmlNamespaceManager(node.OwnerDocument.NameTable);
                    foreach (var names in xnsm.GetNamespacesInScope(XmlNamespaceScope.All)) {
                        namespaces.Set(names.Key, names.Value);
                    }
                }
                fc.Properties.Add("@namespaces", namespaces);
                prefix = "atom:";
            }

            // Import Feed
            if (options.KeepNamespaces) {
                namespaces.Set("dc", "http://purl.org/dc/elements/1.1/");
                fc.Properties.Add("dc:identifier", results.Identifier);
            } else {
                fc.Properties.Add("identifier", results.Identifier);
            }


            if (results.Date != null)
                fc.Properties.Add(prefix + "updated", results.Date.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"));
            if (results.Links != null && results.Links.Count > 0) {

                fc.Links = new List<SyndicationLink>(results.Links);
            }
            if (results.Title != null)
                fc.Properties.Add(prefix + "title", results.Title);

            if (results.Items != null) {
                foreach (var item in results.Items) {
                    fc.FeatureResults.Add(ImportItem(item));
                }
            }

            fc.Properties = fc.Properties.Concat(util.ImportXmlDocument(results.ElementExtensions, ref namespaces)).ToDictionary(x => x.Key, x => x.Value);

            return fc;

        }

        public FeatureResult ImportItem(IOpenSearchResultItem result) {
            if (result == null)
                throw new ArgumentNullException("result");

            FeatureResult feature;

            XmlElement geometry = ImportUtils.FindGmlGeometry(result.ElementExtensions);
            if (geometry != null)
                feature = new FeatureResult(Terradue.GeoJson.Geometry.GeometryFactory.GmlToFeature(geometry));
            else {
                geometry = util.FindDctSpatialGeometry(result.ElementExtensions);
                if (geometry != null)
                    feature = new FeatureResult(Terradue.GeoJson.Geometry.GeometryFactory.WktToFeature(geometry.InnerXml));
                else
                    feature = new FeatureResult(Terradue.GeoJson.Geometry.GeometryFactory.WktToFeature(null));
            }

            NameValueCollection namespaces = null;
            XmlNamespaceManager xnsm = null;
            string prefix = "";

            if (options.KeepNamespaces) {

                // Initialize namespaces
                namespaces = new NameValueCollection();
                namespaces.Set("", "http://geojson.org/ns#");
                namespaces.Set("atom", "http://www.w3.org/2005/Atom");
                namespaces.Set("dc", "http://purl.org/dc/elements/1.1/");
                foreach (XmlNode node in result.ElementExtensions) {
                    xnsm = new XmlNamespaceManager(node.OwnerDocument.NameTable);
                    foreach (var names in xnsm.GetNamespacesInScope(XmlNamespaceScope.All)) {
                        namespaces.Set(names.Key, names.Value);
                    }
                }
                feature.Properties.Add("@namespaces", namespaces);
                prefix = "atom:";
            }

           
            feature.Id = result.Id;
            if (result.Date != null)
                feature.Properties.Add(prefix + "published", result.Date.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"));
            if (result.Links != null && result.Links.Count > 0) {

                feature.Links = new List<SyndicationLink>(result.Links);
            }
            if (result.Title != null)
                feature.Properties.Add(prefix + "title", result.Title);


            if (options.KeepNamespaces) {
                feature.Properties.Add("dc:identifier", result.Identifier);
            } else {
                feature.Properties.Add("identifier", result.Identifier);
            }


            feature.Properties = feature.Properties.Concat(util.ImportXmlDocument(result.ElementExtensions, ref namespaces)).ToDictionary(x => x.Key, x => x.Value);

            return feature;
        }
    }
}


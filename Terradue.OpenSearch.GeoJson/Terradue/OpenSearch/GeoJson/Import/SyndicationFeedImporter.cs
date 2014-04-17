//
//  SyndicationFeedImporter.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Collections.Specialized;
using System.Xml;
using System.Collections.Generic;
using Terradue.OpenSearch.GeoJson.Result;

namespace Terradue.OpenSearch.GeoJson.Import {
    public class SyndicationFeedImporter {

        ImportOptions options;
        ImportUtils util;

        public SyndicationFeedImporter(ImportOptions options) {
            this.options = options;
            util = new ImportUtils(options);
        }

        public FeatureCollectionResult ImportFeed(SyndicationFeed feed) {

            FeatureCollectionResult fc = new FeatureCollectionResult();
            fc.Properties = new System.Collections.Generic.Dictionary<string, object>();

            NameValueCollection namespaces = null;
            XmlNamespaceManager xnsm = null;
            string prefix = "";

            if (options.KeepNamespaces) {

                // Initialize namespaces
                namespaces = new NameValueCollection();
                namespaces.Set("", "http://geojson.org/ns#");
                namespaces.Set("atom", "http://www.w3.org/2005/Atom");
                XmlReader reader = feed.ElementExtensions.GetReaderAtElementExtensions();
                xnsm = new XmlNamespaceManager(reader.NameTable);
                foreach (var names in xnsm.GetNamespacesInScope(XmlNamespaceScope.All)) {
                    namespaces.Set(names.Key, names.Value);
                }
                fc.Properties.Add("@namespaces", namespaces);
                prefix = "atom:";
            }

            // Import Feed
            if (feed != null) {
                if ((feed.Authors != null) && (feed.Authors.Count > 0)) {
                    object[] authors = new object[feed.Authors.Count];
                    fc.Properties.Add(prefix + "authors", authors);
                    for (int i = 0; i < feed.Authors.Count; i++) {
                        Dictionary<string,object> author = new Dictionary<string, object>();
                        author.Add(prefix + "name", feed.Authors[i].Name);
                        author.Add(prefix + "email", feed.Authors[i].Email);
                        author.Add(prefix + "uri", feed.Authors[i].Uri);
                        author = author.Concat(util.ImportAttributeExtensions(feed.Authors[i].AttributeExtensions, namespaces, xnsm)).ToDictionary(x => x.Key, x => x.Value);
                        author = author.Concat(util.ImportElementExtensions(feed.Authors[i].ElementExtensions, namespaces)).ToDictionary(x => x.Key, x => x.Value);
                        authors[i] = author;
                    }
                }
                if (feed.BaseUri != null) {
                    fc.Properties.Add(prefix + "uri", feed.BaseUri.ToString());
                }
                if ((feed.Categories != null) && (feed.Categories.Count > 0)) {
                    object[] categories = new object[feed.Categories.Count];
                    fc.Properties.Add(prefix + "categories", categories);
                    for (int i = 0; i < feed.Categories.Count; i++) {
                        Dictionary<string,object> category = new Dictionary<string, object>();
                        category.Add(prefix + "name", feed.Categories[i].Name);
                        category.Add(prefix + "label", feed.Categories[i].Label);
                        category.Add(prefix + "scheme", feed.Categories[i].Scheme);
                        category = category.Union(util.ImportAttributeExtensions(feed.Categories[i].AttributeExtensions, namespaces, xnsm)).ToDictionary(x => x.Key, x => x.Value);
                        category = category.Union(util.ImportElementExtensions(feed.Categories[i].ElementExtensions, namespaces)).ToDictionary(x => x.Key, x => x.Value);
                        categories[i] = category;
                    }
                }
                if ((feed.Contributors != null) && (feed.Contributors.Count > 0)) {
                    object[] contributors = new object[feed.Contributors.Count];
                    fc.Properties.Add(prefix + "categories", contributors);
                    for (int i = 0; i < feed.Contributors.Count; i++) {
                        Dictionary<string,object> contributor = new Dictionary<string, object>();
                        contributor.Add(prefix + "name", feed.Contributors[i].Name);
                        contributor.Add(prefix + "email", feed.Contributors[i].Email);
                        contributor.Add(prefix + "uri", feed.Contributors[i].Uri);
                        contributor = contributor.Union(util.ImportAttributeExtensions(feed.Contributors[i].AttributeExtensions, namespaces, xnsm)).ToDictionary(x => x.Key, x => x.Value);
                        contributor = contributor.Union(util.ImportElementExtensions(feed.Contributors[i].ElementExtensions, namespaces)).ToDictionary(x => x.Key, x => x.Value);
                        contributors[i] = contributor;
                    }
                }
                if (feed.Copyright != null) {
                    fc.Properties.Add(prefix + "rights", feed.Copyright.Text);
                }
                if (feed.Description != null) {
                    fc.Properties.Add(prefix + "content", feed.Description.Text);
                }
                if (feed.Generator != null) {
                    fc.Properties.Add(prefix + "generator", feed.Generator);
                }
                fc.Properties.Add(prefix + "id", feed.Id);
                if (feed.ImageUrl != null)
                    fc.Properties.Add(prefix + "logo", feed.ImageUrl.ToString());
                if (feed.Language != null)
                    fc.Properties.Add(prefix + "language", feed.Language);
                if (feed.LastUpdatedTime != null)
                    fc.Properties.Add(prefix + "updated", feed.LastUpdatedTime);
                if ((feed.Links != null) && (feed.Links.Count > 0)) {
                    fc.Links = new List<SyndicationLink>(feed.Links);
                }
                if (feed.Title != null)
                    fc.Properties.Add(prefix + "title", feed.Title);

                if (feed.Items != null) {
                    foreach (var item in feed.Items) {
                        fc.Features.Add(ImportItem(item));
                    }
                }

                return fc;

            }
            return null;
        }

        public FeatureResult ImportItem(SyndicationItem item) {
            if (item != null) {

                XmlElement geometry = ImportUtils.FindGmlGeometry(item.ElementExtensions);

                FeatureResult feature = new FeatureResult(Terradue.GeoJson.Geometry.GeometryFactory.GmlToFeature(geometry));

                NameValueCollection namespaces = null;
                XmlNamespaceManager xnsm = null;
                string prefix = "";

                if (options.KeepNamespaces) {

                    // Initialize namespaces
                    namespaces = new NameValueCollection();
                    namespaces.Set("", "http://geojson.org/ns#");
                    namespaces.Set("atom", "http://www.w3.org/2005/Atom");
                    XmlReader reader = item.ElementExtensions.GetReaderAtElementExtensions();
                    xnsm = new XmlNamespaceManager(reader.NameTable);
                    foreach (var names in xnsm.GetNamespacesInScope(XmlNamespaceScope.All)) {
                        namespaces.Set(names.Key, names.Value);
                    }
                    feature.Properties.Add("@namespaces", namespaces);
                    prefix = "atom:";
                }

                // Import Item
                if ((item.Authors != null) && (item.Authors.Count > 0)) {
                    object[] authors = new object[item.Authors.Count];
                    feature.Properties.Add(prefix + "authors", authors);
                    for (int i = 0; i < item.Authors.Count; i++) {
                        Dictionary<string,object> author = new Dictionary<string, object>();
                        author.Add(prefix + "name", item.Authors[i].Name);
                        author.Add(prefix + "email", item.Authors[i].Email);
                        author.Add(prefix + "uri", item.Authors[i].Uri);
                        author = author.Union(util.ImportAttributeExtensions(item.Authors[i].AttributeExtensions, namespaces, xnsm)).ToDictionary(x => x.Key, x => x.Value);
                        author = author.Union(util.ImportElementExtensions(item.Authors[i].ElementExtensions, namespaces)).ToDictionary(x => x.Key, x => x.Value);
                        authors[i] = author;
                    }
                }
                if (item.BaseUri != null) {
                    feature.Properties.Add(prefix + "uri", item.BaseUri.ToString());
                }
                if ((item.Categories != null) && (item.Categories.Count > 0)) {
                    object[] categories = new object[item.Categories.Count];
                    feature.Properties.Add(prefix + "categories", categories);
                    for (int i = 0; i < item.Categories.Count; i++) {
                        Dictionary<string,object> category = new Dictionary<string, object>();
                        category.Add(prefix + "name", item.Categories[i].Name);
                        category.Add(prefix + "label", item.Categories[i].Label);
                        category.Add(prefix + "scheme", item.Categories[i].Scheme);
                        category = category.Union(util.ImportAttributeExtensions(item.Categories[i].AttributeExtensions, namespaces, xnsm)).ToDictionary(x => x.Key, x => x.Value);
                        category = category.Union(util.ImportElementExtensions(item.Categories[i].ElementExtensions, namespaces)).ToDictionary(x => x.Key, x => x.Value);
                        categories[i] = category;
                    }
                }
                if ((item.Contributors != null) && (item.Contributors.Count > 0)) {
                    object[] contributors = new object[item.Contributors.Count];
                    feature.Properties.Add(prefix + "categories", contributors);
                    for (int i = 0; i < item.Contributors.Count; i++) {
                        Dictionary<string,object> contributor = new Dictionary<string, object>();
                        contributor.Add(prefix + "name", item.Contributors[i].Name);
                        contributor.Add(prefix + "email", item.Contributors[i].Email);
                        contributor.Add(prefix + "uri", item.Contributors[i].Uri);
                        contributor = contributor.Union(util.ImportAttributeExtensions(item.Contributors[i].AttributeExtensions, namespaces, xnsm)).ToDictionary(x => x.Key, x => x.Value);
                        contributor = contributor.Union(util.ImportElementExtensions(item.Contributors[i].ElementExtensions, namespaces)).ToDictionary(x => x.Key, x => x.Value);
                        contributors[i] = contributor;
                    }
                }
                if (item.Copyright != null) {
                    feature.Properties.Add(prefix + "rights", item.Copyright.Text);
                }
                if (item.Summary != null) {
                    feature.Properties.Add(prefix + "summary", item.Summary.Text);
                }
                if (item.Content != null) {
                    feature.Properties.Add(prefix + "content", item.Summary.Text);
                }
                feature.Id = item.Id;
                if (item.LastUpdatedTime != null)
                    feature.Properties.Add(prefix + "published", item.LastUpdatedTime);
                if ((item.Links != null) && (item.Links.Count > 0)) {
                    feature.Links = new List<SyndicationLink>(item.Links);
                }
                if (item.Title != null)
                    feature.Properties.Add(prefix + "title", item.Title);

                feature.Properties = feature.Properties.Concat(util.ImportAttributeExtensions(item.AttributeExtensions, namespaces, xnsm)).ToDictionary(x => x.Key, x => x.Value);
                feature.Properties = feature.Properties.Concat(util.ImportElementExtensions(item.ElementExtensions, namespaces)).ToDictionary(x => x.Key, x => x.Value);

                

                return feature;
            }

            return null;
        }


    }
}


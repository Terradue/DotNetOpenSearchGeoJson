//
//  FeatureResult.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Linq;
using System.Collections.Generic;
using Terradue.OpenSearch.Result;
using ServiceStack.Text;
using System.Runtime.Serialization;
using Terradue.ServiceModel.Syndication;
using Terradue.OpenSearch.GeoJson.Import;
using System.Xml;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Xml.Linq;

namespace Terradue.OpenSearch.GeoJson.Result {

    [DataContract]
    public class FeatureResult : Terradue.GeoJson.Feature.Feature, IOpenSearchResultItem {

        public FeatureResult() : base() {
            links = new Collection<SyndicationLink>();
            elementExtensions = new SyndicationElementExtensionCollection();
            Namespaces = InitNameSpaces;
        }

        public FeatureResult(Terradue.GeoJson.Feature.Feature feature) : base(feature.Geometry, feature.Properties) {
            base.Id = feature.Id;
            base.BoundingBoxes = feature.BoundingBoxes;
            base.CRS = feature.CRS;
            links = new Collection<SyndicationLink>();
            elementExtensions = new SyndicationElementExtensionCollection();
            Properties = feature.Properties;
            Namespaces = InitNameSpaces;
        }

        public FeatureResult(FeatureResult result) : this((Terradue.GeoJson.Feature.Feature)result) {
            links = new Collection<SyndicationLink>(result.Links);
            elementExtensions = new SyndicationElementExtensionCollection(result.elementExtensions);
            this.Identifier = result.Identifier;
            this.Title = result.Title;
            Properties = result.Properties;
            Namespaces = InitNameSpaces;
        }

        protected NameValueCollection Namespaces;

        protected virtual NameValueCollection InitNameSpaces {
            get {
                NameValueCollection namespaces = new NameValueCollection();
                namespaces.Set("", "http://geojson.org/ns#");
                namespaces.Set("atom", "http://www.w3.org/2005/Atom");
                return namespaces;
            }
        }

        public static FeatureResult ParseJson(string json) {
            return new FeatureResult(Terradue.GeoJson.Feature.Feature.ParseJson(json));
        }

        public static FeatureResult FromOpenSearchResultItem(IOpenSearchResultItem result) {
            if (result == null)
                throw new ArgumentNullException("result");

            FeatureResult feature;

            if (result.ElementExtensions != null && result.ElementExtensions.Count > 0) {

                List<XmlElement> elements = new List<XmlElement>();
                foreach (var ext in result.ElementExtensions) {
                    XmlElement element = ext.GetObject<XmlElement>();
                    elements.Add(element);
                }

                XmlElement geometry = ImportUtils.FindGmlGeometry(elements.ToArray());
                if (geometry != null) feature = new FeatureResult(Terradue.GeoJson.Geometry.GeometryFactory.GmlToFeature(geometry));
                else {
                    geometry = ImportUtils.FindDctSpatialGeometry(elements.ToArray());
                    if (geometry != null) feature = new FeatureResult(Terradue.GeoJson.Geometry.GeometryFactory.WktToFeature(geometry.InnerXml));
                    else feature = new FeatureResult(Terradue.GeoJson.Geometry.GeometryFactory.WktToFeature(null));
                }

                feature.ElementExtensions = new SyndicationElementExtensionCollection(result.ElementExtensions);
            } else {
                feature = new FeatureResult(Terradue.GeoJson.Geometry.GeometryFactory.WktToFeature(null));
                feature.ElementExtensions = new SyndicationElementExtensionCollection();
            }

            string prefix = "";

            feature.Id = result.Id;
            feature.Identifier = result.Identifier;
            feature.Date = result.Date;

            if (result.Links != null && result.Links.Count > 0) {
                feature.Links = new Collection<SyndicationLink>();
                foreach (var link in result.Links) {
                    if (link.RelationshipType == "self")
                        continue;
                    feature.Links.Add(new SyndicationLink(link));
                }

            }

            if (result.Title != null)
                feature.Title = result.Title;

            return feature;
        }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <value>The properties are read-only in this class since it is intented for serialization purpose only</value>
        [DataMember(Name = "properties")]
        public new Dictionary <string,object> Properties {
            get {
                // Start with an empty NameValueCollection
                Dictionary <string,object> properties = new Dictionary<string, object>();

                string prefix = "";
                if (ShowNamespaces)
                    prefix = "atom:";
                if (Links != null && Links.Count > 0) {
                    properties[prefix + "links"] = FeatureCollectionResult.LinksToProperties(Links, ShowNamespaces);
                }
                properties[prefix + "published"] = this.Date.ToString("yyyy-MM-ddTHH:mm:ssZ");
                properties[prefix + "title"] = this.Title;

                ImportUtils util = new ImportUtils(new Terradue.OpenSearch.GeoJson.Import.ImportOptions() {
                    KeepNamespaces = ShowNamespaces,
                    AsMixed = AlwaysAsMixed
                });
                properties = properties.Concat(util.SyndicationElementExtensions(ElementExtensions, ref Namespaces)).ToDictionary(x => x.Key, x => x.Value);

                if (ShowNamespaces)
                    prefix = "dc:";
                if (!properties.ContainsKey(prefix + "identifier") && !string.IsNullOrEmpty(this.Identifier)) {
                    properties[prefix + "identifier"] = this.Identifier;
                }

                if (ShowNamespaces)
                    properties.Add("@namespaces", Namespaces.AllKeys
                               .ToDictionary(p => p, p => Namespaces[p]));

                return properties;
            }

            set {
                // Start with an empty NameValueCollection
                Dictionary <string,object> properties = value;

                ImportUtils util = new ImportUtils(new Terradue.OpenSearch.GeoJson.Import.ImportOptions() {
                    KeepNamespaces = ShowNamespaces,
                    AsMixed = AlwaysAsMixed
                });
                foreach (SyndicationElementExtension e in util.PropertiesToSyndicationElementExtensions(properties)) {
                    ElementExtensions.Add(e);
                }
                ImportSyndicationElements(value);
            }
        }

        void ImportSyndicationElements(Dictionary <string,object> properties) {
            if (properties.ContainsKey("links") && properties["links"] is List<object>) {
                foreach (object link in (List<object>)properties["links"]) {
                    if (link is Dictionary<string, object>) {
                        Links.Add(ImportUtils.FromDictionnary((Dictionary<string, object>)link));
                    }
                }
            }
            if (properties.ContainsKey("atom:links") && properties["atom:links"] is List<object>) {
                foreach (object link in (List<object>)properties["atom:links"]) {
                    if (link is Dictionary<string, object>) {
                        Links.Add(ImportUtils.FromDictionnary((Dictionary<string, object>)link,"atom:"));
                    }
                }
            }
            if (properties.ContainsKey("published") && properties["published"] is string) {
                DateTime.TryParse((string)properties["published"], out date);
            }
            if (properties.ContainsKey("atom:published") && properties["atom:published"] is string) {
                DateTime.TryParse((string)properties["atom:published"], out date);
            }
            if (properties.ContainsKey("dc:identifier") && properties["dc:identifier"] is string) {
                identifier = (string)properties["dc:identifier"];
            }
            if (properties.ContainsKey("identifier") && properties["identifier"] is string) {
                identifier = (string)properties["identifier"];
            }
        }

        #region IOpenSearchResultItem implementation

        [DataMember(Name = "id")]
        public new string Id {
            get {
                var link = Links.SingleOrDefault(l => l.RelationshipType == "self");
                return link == null ? base.Id : link.Uri.ToString();
            }
            set {
                base.Id = value;
            }
        }

        string title;

        [IgnoreDataMember]
        public string Title {
            get {
                return title;
            }
            set {
                title = value;
            }
        }

        DateTime date;

        [IgnoreDataMember]
        public DateTime Date {
            get {
                return date;
            }
            set {
                date = value;
            }
        }

        string identifier;

        [IgnoreDataMember]
        public string Identifier {
            get {
                return identifier;
            }
            set {
                identifier = value;
            }
        }

        [IgnoreDataMember]

        Collection<Terradue.ServiceModel.Syndication.SyndicationLink> links;

        [IgnoreDataMember]
        public Collection<Terradue.ServiceModel.Syndication.SyndicationLink> Links {
            get {
                return links;
            }
            set {
                links = value;
            }
        }

        SyndicationElementExtensionCollection elementExtensions;

        [IgnoreDataMember]
        public SyndicationElementExtensionCollection ElementExtensions {
            get {
                return elementExtensions;
            }
            set {
                elementExtensions = value;
            }
        }

        bool showNamespaces = false;

        [IgnoreDataMember]
        public bool ShowNamespaces {
            get {
                return showNamespaces;
            }
            set {
                showNamespaces = value;
            }
        }

        bool alwaysAsMixed = false;

        [IgnoreDataMember]
        public bool AlwaysAsMixed {
            get {
                return alwaysAsMixed;
            }
            set {
                alwaysAsMixed = value;
            }
        }

        #endregion
    }
}

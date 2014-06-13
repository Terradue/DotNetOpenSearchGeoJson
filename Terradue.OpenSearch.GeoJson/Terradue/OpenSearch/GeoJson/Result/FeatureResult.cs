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

namespace Terradue.OpenSearch.GeoJson.Result {

    [DataContract]
    public class FeatureResult : Terradue.GeoJson.Feature.Feature, IOpenSearchResultItem {

        public FeatureResult() : base() {
            links = new Collection<SyndicationLink>();
            elementExtensions = new SyndicationElementExtensionCollection();
        }

        public FeatureResult(Terradue.GeoJson.Feature.Feature feature) : base(feature.Geometry, feature.Properties) {
            base.Id = feature.Id;
            base.BoundingBoxes = feature.BoundingBoxes;
            base.CRS = feature.CRS;
            links = new Collection<SyndicationLink>();
            elementExtensions = new SyndicationElementExtensionCollection();
            ImportSyndicationElements(feature);
        }

        public FeatureResult(FeatureResult result) : this((Terradue.GeoJson.Feature.Feature)result) {
            links = new Collection<SyndicationLink>(result.Links);
            elementExtensions = new SyndicationElementExtensionCollection(result.elementExtensions);
        }

        public static FeatureResult FromOpenSearchResultItem(IOpenSearchResultItem result) {
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
                geometry = ImportUtils.FindDctSpatialGeometry(elements.ToArray());
                if (geometry != null)
                    feature = new FeatureResult(Terradue.GeoJson.Geometry.GeometryFactory.WktToFeature(geometry.InnerXml));
                else
                    feature = new FeatureResult(Terradue.GeoJson.Geometry.GeometryFactory.WktToFeature(null));
            }

            NameValueCollection namespaces = null;
            XmlNamespaceManager xnsm = null;
            string prefix = "";

            feature.Id = result.Id;
            feature.Identifier = result.Identifier;
            if (result.Date != null)
                feature.Properties.Add(prefix + "published", result.Date.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"));
            if (result.Links != null && result.Links.Count > 0) {
                feature.Links = result.Links;
            }
            if (result.Title != null)
                feature.Title = result.Title;

            feature.ElementExtensions = new SyndicationElementExtensionCollection(result.ElementExtensions);

            return feature;
        }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <value>The properties are read-only in this class since it is intented for serialization purpose only</value>
        [DataMember(Name="properties")]
        public new Dictionary <string,object> Properties {
            get {
                // Start with an empty NameValueCollection
                Dictionary <string,object> properties = new Dictionary<string, object>();
                NameValueCollection namespaces = new NameValueCollection();
                namespaces.Set("", "http://geojson.org/ns#");
                namespaces.Set("atom", "http://www.w3.org/2005/Atom");

                ImportUtils util = new ImportUtils(new Terradue.OpenSearch.GeoJson.Import.ImportOptions(){ KeepNamespaces = ShowNamespaces });

                string prefix = "";
                if (ShowNamespaces) prefix = "atom:";
                if (Links != null && Links.Count > 0) {
                    List<Dictionary<string,object>> links = new List<Dictionary<string,object>>();
                    foreach (SyndicationLink link in Links) {
                        Dictionary<string,object> newlink = new Dictionary<string,object>();
                        newlink.Add("@" + prefix+ "href", link.Uri.ToString());
                        if (link.RelationshipType != null) newlink.Add("@" + prefix+ "rel", link.RelationshipType);
                        if (link.Title != null) newlink.Add("@" + prefix+ "title", link.Title);
                        if (link.MediaType != null) newlink.Add("@" + prefix+ "type", link.MediaType);
                        if (link.Length != 0) newlink.Add("@" + prefix+ "length", link.Length);

                        links.Add(newlink);
                    }
                    properties["links"] = links.ToArray();
                }
                List<XmlElement> elements = new List<XmlElement>();
                foreach (var element in ElementExtensions) {
                    elements.Add(element.GetObject<XmlElement>());
                }
                properties = properties.Concat(util.ImportXmlDocument(elements.ToArray(), ref namespaces)).ToDictionary(x => x.Key, x => x.Value);

                properties.Add("@namespaces", namespaces.AllKeys
                               .ToDictionary(p => p, p => namespaces[p]));

                return properties;
            }
        }

        void InitNameSpaces() {

        }

        void ImportSyndicationElements(Terradue.GeoJson.Feature.Feature feature) {
            if ( feature.Properties.ContainsKey("links") && feature.Properties["links"] is Dictionary<string,object>[]) {
                foreach (object link in (Dictionary<string,object>[])feature.Properties["links"]) {
                    if (link is Dictionary<string, object>) {
                        Links.Add(ImportUtils.FromDictionnary((Dictionary<string, object>)link));
                    }
                }
            }
        }

        #region IOpenSearchResultItem implementation

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

        bool showNamespaces;
        public bool ShowNamespaces {
            get {
                return showNamespaces;
            }
            set {
                showNamespaces = value;
            }
        }
        #endregion
    }
}

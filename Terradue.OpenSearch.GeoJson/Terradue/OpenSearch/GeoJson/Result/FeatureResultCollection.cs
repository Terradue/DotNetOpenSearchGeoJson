//
//  FeatureCollectionResult.cs
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
using System.IO;

namespace Terradue.OpenSearch.GeoJson.Result {
    [DataContract]
    public class FeatureCollectionResult : Terradue.GeoJson.Feature.FeatureCollection, IOpenSearchResultCollection {

        protected NameValueCollection namespaces;

        public FeatureCollectionResult(Terradue.GeoJson.Feature.FeatureCollection fc) : base() {
            base.BoundingBoxes = fc.BoundingBoxes;
            base.CRS = fc.CRS;
            base.Properties = fc.Properties;
            Links = new Collection<SyndicationLink>();
            fc.Features.FirstOrDefault(f => {
                FeatureResults.Add(new FeatureResult(f));
                return false;
            });
            ImportSyndicationElements(fc);
            InitNameSpaces();
        }

        public FeatureCollectionResult() : base() {
            Links = new Collection<SyndicationLink>();
            FeatureResults = new List<FeatureResult>();
            InitNameSpaces();
        }

        public static FeatureCollectionResult FromOpenSearchResultCollection(IOpenSearchResultCollection results) {
            if (results == null)
                throw new ArgumentNullException("results");

            FeatureCollectionResult fc = new FeatureCollectionResult();

            NameValueCollection namespaces = null;
            XmlNamespaceManager xnsm = null;
            string prefix = "";

            fc.ElementExtensions = new SyndicationElementExtensionCollection(results.ElementExtensions);
           
            if (results.Date != null && fc.Properties.ContainsKey(prefix + "updated") == null)
                fc.Properties.Add(prefix + "updated", results.Date.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"));

            if (results.Links != null && results.Links.Count > 0) {
                fc.Links = results.Links;
            }

            if (results.Items != null) {
                foreach (var item in results.Items) {
                    fc.FeatureResults.Add(FeatureResult.FromOpenSearchResultItem(item));
                }
            }
            return fc;
        }

        void InitNameSpaces() {
            namespaces = new NameValueCollection();
            namespaces.Set("", "http://geojson.org/ns#");
            namespaces.Set("atom", "http://www.w3.org/2005/Atom");
            namespaces.Set("os", "http://a9.com/-/spec/opensearch/1.1/");
        }

        [IgnoreDataMember]
        public new List<Terradue.GeoJson.Feature.Feature> Features { get; private set; }

        [DataMember(Name="features")]
        public List<FeatureResult> FeatureResults { get; set; }

        [DataMember(Name="properties")]
        public new Dictionary <string,object> Properties {
            get {
                Dictionary <string,object> properties = base.Properties != null ? base.Properties : new Dictionary <string,object>();
                if (properties.ContainsKey("links")) {
                    properties.Remove("links");
                }
                string prefix = "";
                if (properties.ContainsKey("@namespaces")) prefix = "atom:";
                if (Links != null && Links.Count > 0) {
                    List<Dictionary<string,object>> links = new List<Dictionary<string,object>>();
                    foreach (SyndicationLink link in Links) {
                        Dictionary<string,object> newlink = new Dictionary<string,object>();
                        newlink.Add("@" + prefix + "href", link.Uri.ToString());
                        if (link.RelationshipType != null) newlink.Add("@" + prefix + "rel", link.RelationshipType);
                        if (link.Title != null) newlink.Add("@" + prefix + "title", link.Title);
                        if (link.MediaType != null) newlink.Add("@" + prefix + "type", link.MediaType);
                        if (link.Length != 0) newlink.Add("@" + prefix + "length", link.Length);

                        links.Add(newlink);
                    }
                    properties["links"] = links.ToArray();
                }

                return properties;
            }
        }

        void ImportSyndicationElements(Terradue.GeoJson.Feature.FeatureCollection fc) {
            if ( fc.Properties.ContainsKey("links") && fc.Properties["links"] is Dictionary<string,object>[]) {
                Links = new Collection<SyndicationLink>();
                foreach (object link in (Dictionary<string,object>[])fc.Properties["links"]) {
                    if (link is Dictionary<string, object>) {
                        Links.Add(ImportUtils.FromDictionnary((Dictionary<string, object>)link));
                    }
                }
            }
        }

        #region IResultCollection implementation

        public List<string> GetSelfShortList() {
            return base.Features.Select(f => f.Id).ToList();
        }

        [IgnoreDataMember]
        public IEnumerable<IOpenSearchResultItem> Items {
            get {
                return FeatureResults.Cast<IOpenSearchResultItem>().ToList();
            }
        }

        List<Terradue.ServiceModel.Syndication.SyndicationLink> links;

        [IgnoreDataMember]
        public Collection<Terradue.ServiceModel.Syndication.SyndicationLink> Links {
            get {
                return new Collection<SyndicationLink>(links);
            }
            set {
                links = value.ToList();
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

        long count;
        [IgnoreDataMember]
        public long Count {
            get {
                return count;
            }
            set {
                count = value;
            }
        }

        public void SerializeToStream(System.IO.Stream stream) {
            JsConfig.ExcludeTypeInfo = true;
            JsConfig.IncludeTypeInfo = false;
            JsonSerializer.SerializeToStream(this, stream);
        }

        public string SerializeToString() {
            MemoryStream ms = new MemoryStream();
            SerializeToStream(ms);
            ms.Seek(0, SeekOrigin.Begin);
            StreamReader sr = new StreamReader(ms);
            return sr.ReadToEnd();
        }

        public static IOpenSearchResultCollection DeserializeFromStream(Stream stream) {
            StreamReader sr = new StreamReader(stream);
            Console.Write(sr.ReadToEnd());
            stream.Seek(0, SeekOrigin.Begin);
            return JsonSerializer.DeserializeFromStream<FeatureCollectionResult>(stream);
        }

        bool showNamespaces;
        public bool ShowNamespaces {
            get {
                return showNamespaces;
            }
            set {
                showNamespaces = value;
                foreach (FeatureResult item in Items) {
                    item.ShowNamespaces = value;
                }
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
                foreach (FeatureResult item in Items) {
                    item.AlwaysAsMixed = value;
                }
            }
        }

        public string ContentType {
            get {
                return "application/json";
            }
        }

        #endregion
    }

}


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

        protected NameValueCollection Namespaces;

        public FeatureCollectionResult(Terradue.GeoJson.Feature.FeatureCollection fc) : base() {
            base.BoundingBoxes = fc.BoundingBoxes;
            base.CRS = fc.CRS;
            base.Properties = fc.Properties;
            Links = new Collection<SyndicationLink>();
            fc.Features.FirstOrDefault(f => {
                FeatureResults.Add(new FeatureResult(f));
                return false;
            });
            ImportSyndicationElements(fc.Properties);
            InitNameSpaces();
            elementExtensions = new SyndicationElementExtensionCollection();
        }

        public FeatureCollectionResult() : base() {
            Links = new Collection<SyndicationLink>();
            FeatureResults = new List<FeatureResult>();
            InitNameSpaces();
            elementExtensions = new SyndicationElementExtensionCollection();
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

            fc.authors = results.Authors;
            fc.categories = results.Categories;

            if (results.Items != null) {
                foreach (var item in results.Items) {
                    fc.FeatureResults.Add(FeatureResult.FromOpenSearchResultItem(item));
                }
            }


            return fc;
        }

        void InitNameSpaces() {
            Namespaces = new NameValueCollection();
            Namespaces.Set("", "hresultsttp://geojson.org/ns#");
            Namespaces.Set("atom", "http://www.w3.org/2005/Atom");
            Namespaces.Set("os", "http://a9.com/-/spec/opensearch/1.1/");
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
                if (ShowNamespaces) prefix = "atom:";
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

                if (Links != null && Links.Count > 0) {
                    properties[prefix + "links"] = LinksToProperties(Links, ShowNamespaces);
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
        }

        #region IResultCollection implementation

        public new string Id {
            get {
                var links = Links.Where(l => l.RelationshipType == "self").ToArray();
                if (links.Count() > 0) return links[0].Uri.ToString();
                return null;
            }
            set{ }
        }

        public List<string> GetSelfShortList() {
            return base.Features.Select(f => f.Id).ToList();
        }

        [IgnoreDataMember]
        public IEnumerable<IOpenSearchResultItem> Items {
            get {
                return FeatureResults.Cast<IOpenSearchResultItem>().ToList();
            }
            set {
                FeatureResults = value.Cast<FeatureResult>().ToList();
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

        Collection<SyndicationCategory> categories;
        public Collection<SyndicationCategory> Categories {
            get {
                return categories;
            }
        }

        Collection<SyndicationPerson> authors;
        public Collection<SyndicationPerson> Authors {
            get {
                return authors;
            }
        }

        #endregion

        public static List<Dictionary<string,object>> LinksToProperties(Collection<Terradue.ServiceModel.Syndication.SyndicationLink> ilinks, bool showNamespaces) {
            string prefix = "";
            if (showNamespaces)
                prefix = "atom:";

            List<Dictionary<string,object>> links = new List<Dictionary<string,object>>();
            foreach (SyndicationLink link in ilinks) {
                Dictionary<string,object> newlink = new Dictionary<string,object>();
                newlink.Add("@" + prefix + "href", link.Uri.ToString());
                if (link.RelationshipType != null)
                    newlink.Add("@" + prefix + "rel", link.RelationshipType);
                if (link.Title != null)
                    newlink.Add("@" + prefix + "title", link.Title);
                if (link.MediaType != null)
                    newlink.Add("@" + prefix + "type", link.MediaType);
                if (link.Length != 0)
                    newlink.Add("@" + prefix + "length", link.Length);

                links.Add(newlink);
            }

            return links;
        }
    }

}


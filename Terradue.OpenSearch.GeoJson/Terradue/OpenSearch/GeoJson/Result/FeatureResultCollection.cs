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
using System.Xml.Linq;

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
            authors = new Collection<SyndicationPerson>();
            categories = new Collection<SyndicationCategory>();
            contributors = new Collection<SyndicationPerson>();
        }

        public FeatureCollectionResult() : base() {
            Links = new Collection<SyndicationLink>();
            FeatureResults = new List<FeatureResult>();
            InitNameSpaces();
            elementExtensions = new SyndicationElementExtensionCollection();
            authors = new Collection<SyndicationPerson>();
            categories = new Collection<SyndicationCategory>();
            contributors = new Collection<SyndicationPerson>();
        }

        public static FeatureCollectionResult FromOpenSearchResultCollection(IOpenSearchResultCollection results) {
            if (results == null)
                throw new ArgumentNullException("results");

            FeatureCollectionResult fc = new FeatureCollectionResult();

            NameValueCollection namespaces = null;
            XmlNamespaceManager xnsm = null;
            string prefix = "";

            fc.ElementExtensions = new SyndicationElementExtensionCollection(results.ElementExtensions);
           
            if (results.Links != null && results.Links.Count > 0) {
                fc.Links = results.Links;
            }

            fc.authors = results.Authors;
            fc.categories = results.Categories;
            fc.Date = results.Date;
            fc.Title = results.Title;
            fc.TotalResults = results.TotalResults;
            fc.OpenSearchable = results.OpenSearchable;

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
        public new List<Terradue.GeoJson.Feature.Feature> Features { 
            get{
                return FeatureResults.Cast<Terradue.GeoJson.Feature.Feature>().ToList();
            } 
        }

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

                if (Authors != null && Authors.Count > 0) {
                    foreach (var author in Authors) {
                        var authord = util.SyndicationElementExtensions(author.ElementExtensions, ref Namespaces);
                        authord.Add("email", author.Email);
                        authord.Add("name", author.Name);
                        if ( author.Uri != null )
                            authord.Add("uri", author.Uri.ToString());
                        properties[prefix + "authors"] = authord;

                    }
                }

                if (Categories != null && Categories.Count > 0) {
                    foreach (var cat in Categories) {
                        var catd = util.SyndicationElementExtensions(cat.ElementExtensions, ref Namespaces);
                        catd.Add("name", cat.Name);
                        catd.Add("label", cat.Label);
                        catd.Add("scheme", cat.Scheme);
                        properties[prefix + "categories"] = catd;
                    }
                }

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
                if (value != null)
                    ImportSyndicationElements(value);
                base.Properties = null;
            }
        }

        void ImportSyndicationElements(Dictionary <string,object> properties) {

            ImportUtils util = new ImportUtils(new Terradue.OpenSearch.GeoJson.Import.ImportOptions() {
                KeepNamespaces = ShowNamespaces,
                AsMixed = AlwaysAsMixed
            });

            Dictionary <string,object> forExtensions = new Dictionary<string, object>();

            foreach (var key in properties.Keys) {

                if (key == "atom:title") {
                    if (properties["atom:title"] is string)
                        title = new TextSyndicationContent((string)properties["atom:title"]);
                }

                if (key == "title") {
                    if (properties["title"] is string)
                        title = new TextSyndicationContent((string)properties["title"]);
                    continue;
                }

                if (key == "links" && properties["links"] is List<object>) {
                    foreach (object link in (List<object>)properties["links"]) {
                        if (link is Dictionary<string, object>) {
                            Links.Add(ImportUtils.FromDictionnary((Dictionary<string, object>)link));
                        }
                    }
                    continue;
                }
                if (key == "atom:links" && properties["atom:links"] is List<object>) {
                    foreach (object link in (List<object>)properties["atom:links"]) {
                        if (link is Dictionary<string, object>) {
                            Links.Add(ImportUtils.FromDictionnary((Dictionary<string, object>)link, "atom:"));
                        }
                    }
                    continue;
                }
                if (key == "published" && properties["published"] is string) {
                    DateTime.TryParse((string)properties["published"], out date);
                    continue;
                }
                if (key == "atom:published" && properties["atom:published"] is string) {
                    DateTime.TryParse((string)properties["atom:published"], out date);
                    continue;
                }

                forExtensions.Add(key, properties[key]);
            }

            foreach (SyndicationElementExtension e in util.PropertiesToSyndicationElementExtensions(forExtensions)) {
                ElementExtensions.Add(e);
            }
        }

        #region IResultCollection implementation

        string id;
        public new string Id {
            get {
                return id;
            }
            set{
                id = value;
            }
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

        TextSyndicationContent title;
        [IgnoreDataMember]
        public TextSyndicationContent Title {
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

        [IgnoreDataMember]
        public string Identifier {
            get {
                var identifier = ElementExtensions.ReadElementExtensions<string>("identifier", "http://purl.org/dc/elements/1.1/");
                return identifier.Count == 0 ? this.Id : identifier[0];
            }
            set {
                foreach (var ext in this.ElementExtensions.ToArray()) {
                    if (ext.OuterName == "identifier" && ext.OuterNamespace == "http://purl.org/dc/elements/1.1/") {
                        this.ElementExtensions.Remove(ext);
                        continue;
                    }
                }
                this.ElementExtensions.Add(new XElement(XName.Get("identifier", "http://purl.org/dc/elements/1.1/"), value).CreateReader());
            }
        }

        [IgnoreDataMember]
        public long Count {
            get {
                return Items.Count();
            }
        }

        readonly Collection<SyndicationPerson> contributors;
        public Collection<SyndicationPerson> Contributors {
            get {
                return contributors;
            }
        }

        TextSyndicationContent copyright;
        public TextSyndicationContent Copyright {
            get {
                return copyright;
            }
            set {
                copyright = value;
            }
        }

        TextSyndicationContent description;
        public TextSyndicationContent Description {
            get {
                return description;
            }
            set {
                description = value;
            }
        }

        string generator;
        public string Generator {
            get {
                return generator;
            }
            set {
                generator = value;
            }
        }

        public long TotalResults {
            get {
                var totalResults = ElementExtensions.ReadElementExtensions<string>("totalResults", "http://a9.com/-/spec/opensearch/1.1/");
                return totalResults.Count == 0 ? 0 : long.Parse(totalResults[0]);
            }
            set {
                foreach (var ext in this.ElementExtensions.ToArray()) {
                    if (ext.OuterName == "totalResults" && ext.OuterNamespace == "http://a9.com/-/spec/opensearch/1.1//") {
                        this.ElementExtensions.Remove(ext);
                        continue;
                    }
                }
                this.ElementExtensions.Add(new XElement(XName.Get("totalResults", "http://a9.com/-/spec/opensearch/1.1/"), value).CreateReader());
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

        IOpenSearchable openSearchable;
        public IOpenSearchable OpenSearchable {
            get {
                return openSearchable;
            }
            set {
                openSearchable = value;
            }
        }

        NameValueCollection parameters;
        public NameValueCollection Parameters {
            get {
                return parameters;
            }
            set {
                parameters = value;
            }
        }

        public TimeSpan QueryTimeSpan {
            get {
                var duration = ElementExtensions.ReadElementExtensions<double>("queryTime", "http://purl.org/dc/elements/1.1/");
                return duration.Count == 0 ? new TimeSpan() : TimeSpan.FromMilliseconds(duration[0]);
            }
            set {
                foreach (var ext in this.ElementExtensions.ToArray()) {
                    if (ext.OuterName == "queryTime" && ext.OuterNamespace == "http://purl.org/dc/elements/1.1/") {
                        this.ElementExtensions.Remove(ext);
                        continue;
                    }
                }
                this.ElementExtensions.Add(new XElement(XName.Get("queryTime", "http://purl.org/dc/elements/1.1/"), value.TotalMilliseconds).CreateReader());
            }
        }

        public object Clone() {
            return new FeatureCollectionResult(this);
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


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
using System.Text;
using System.IO;

namespace Terradue.OpenSearch.GeoJson.Result {

    [DataContract]
    public class FeatureResult : Terradue.GeoJson.Feature.Feature, IOpenSearchResultItem {

        public FeatureResult() : base() {
            links = new Collection<SyndicationLink>();
            elementExtensions = new SyndicationElementExtensionCollection();
            Namespaces = InitNameSpaces;
            authors = new Collection<SyndicationPerson>();
            categories = new Collection<SyndicationCategory>();
            contributors = new Collection<SyndicationPerson>();
        }

        public FeatureResult(Terradue.GeoJson.Feature.Feature feature) : base(feature.Geometry, feature.Properties) {
            base.Id = feature.Id;
            base.BoundingBoxes = feature.BoundingBoxes;
            base.CRS = feature.CRS;
            links = new Collection<SyndicationLink>();
            elementExtensions = new SyndicationElementExtensionCollection();
            Properties = feature.Properties;
            Namespaces = InitNameSpaces;
            authors = new Collection<SyndicationPerson>();
            categories = new Collection<SyndicationCategory>();
            contributors = new Collection<SyndicationPerson>();
        }

        public FeatureResult(FeatureResult result) : this((Terradue.GeoJson.Feature.Feature)result) {
            links = new Collection<SyndicationLink>(result.Links);
            elementExtensions = new SyndicationElementExtensionCollection(result.elementExtensions);
            this.Title = result.Title;
            this.Date = result.Date;
            base.Id = result.Id;
            this.authors = result.Authors;
            this.categories = result.Categories;

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

        public new static FeatureResult ParseJson(string json) {
            var feature = Terradue.GeoJson.Feature.Feature.ParseJson(json);
            return new FeatureResult(feature);
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
                if (geometry != null)
                    feature = new FeatureResult(Terradue.GeoJson.Geometry.GeometryFactory.GmlToFeature(geometry));
                else {
                    geometry = ImportUtils.FindDctSpatialGeometry(elements.ToArray());
                    if (geometry != null)
                        feature = new FeatureResult(Terradue.GeoJson.Geometry.GeometryFactory.WktToFeature(geometry.InnerXml));
                    else {
                        geometry = ImportUtils.FindGeoRssGeometry(elements.ToArray());
                        if (geometry != null)
                            feature = new FeatureResult(Terradue.GeoJson.Geometry.GeometryFactory.GeoRSSToFeature(geometry));
                        else
                            feature = new FeatureResult(Terradue.GeoJson.Geometry.GeometryFactory.WktToFeature(null));
                    }
                }

                feature.ElementExtensions = new SyndicationElementExtensionCollection(result.ElementExtensions);
            } else {
                feature = new FeatureResult(Terradue.GeoJson.Geometry.GeometryFactory.WktToFeature(null));
                feature.ElementExtensions = new SyndicationElementExtensionCollection();
            }

            string prefix = "";

            feature.Id = result.Id;
            feature.Date = result.Date;
            feature.Summary = result.Summary;
            feature.Content = result.Content;
            feature.contributors = result.Contributors;
            feature.authors = result.Authors;
            feature.Title = result.Title;
            feature.categories = result.Categories;
            feature.Copyright = result.Copyright;
            feature.Identifier = result.Identifier;

            feature.Links = new Collection<SyndicationLink>(result.Links);


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
                if (Title != null)
                    properties[prefix + "title"] = this.Title.Text;
                if (Summary != null)
                    properties[prefix + "summary"] = this.Summary.Text;
                if (Content != null) {
                    var content = new Dictionary<string, string>();
                    MemoryStream ms = new MemoryStream();
                    var xw = XmlWriter.Create(ms);
                    this.Content.WriteTo(xw, "content", "");
                    xw.Flush();
                    ms.Seek(0, SeekOrigin.Begin);
                    content.Add("type", this.Content.Type);
                    content.Add("text", XElement.Load(XmlReader.Create(ms)).Value);
                    properties[prefix + "content"] = content;
                }

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
                        catd.Add("@term", cat.Name);
                        catd.Add("@label", cat.Label);
                        catd.Add("@scheme", cat.Scheme);
                        properties[prefix + "categories"] = catd;
                    }
                }

                if (Copyright != null) {
                    properties[prefix + "copyright"] = this.Copyright.Text;
                }

                var exts = util.SyndicationElementExtensions(ElementExtensions, ref Namespaces);

                properties = properties.Concat(exts).ToDictionary(x => x.Key, x => x.Value);

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

                if (key == "where" && properties["where"] is Dictionary<string, object>) {
                    DateTime.TryParse((string)properties["atom:published"], out date);
                    continue;
                }

                forExtensions.Add(key, properties[key]);
            }

            foreach (SyndicationElementExtension e in util.PropertiesToSyndicationElementExtensions(forExtensions)) {
                ElementExtensions.Add(e);
            }
        }

        #region IOpenSearchResultItem implementation

        [DataMember(Name = "id")]
        public new string Id {
            get {
                var links = Links.Where(l => l.RelationshipType == "self").ToArray();
                if (links.Count() > 0)
                    return links[0].Uri.ToString();
                return base.Id;
            }
            set {
                base.Id = value;
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
                return identifier.Count == 0 ? base.Id : identifier[0];
            }
            set {
                if (value == null)
                    return;
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

        TextSyndicationContent summary;

        public TextSyndicationContent Summary {
            get {
                return summary;
            }
            set {
                summary = value;
            }
        }

        Collection<SyndicationPerson> contributors;

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

        SyndicationContent content;

        public SyndicationContent Content {
            get {
                return content;
            }
            set {
                content = value;
            }
        }

        #endregion
    }
}

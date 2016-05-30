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
using System.Runtime.Serialization;
using Terradue.ServiceModel.Syndication;
using Terradue.OpenSearch.GeoJson.Import;
using System.Xml;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Xml.Linq;
using Newtonsoft.Json;
using Terradue.OpenSearch.GeoJson.Converter;
using Newtonsoft.Json.Linq;

namespace Terradue.OpenSearch.GeoJson.Result {

    public class FeatureCollectionResult : Terradue.GeoJson.Feature.FeatureCollection, IOpenSearchResultCollection {

        public FeatureCollectionResult(Terradue.GeoJson.Feature.FeatureCollection fc) : base() {
            base.BoundingBoxes = fc.BoundingBoxes;
            base.CRS = fc.CRS;
            base.Properties = fc.Properties;
            Links = new Collection<SyndicationLink>();
            fc.Features.FirstOrDefault(f => {
                FeatureResults.Add(new FeatureResult(f));
                return false;
            });
            elementExtensions = new SyndicationElementExtensionCollection();
            authors = new Collection<SyndicationPerson>();
            categories = new Collection<SyndicationCategory>();
            contributors = new Collection<SyndicationPerson>();
        }

        public FeatureCollectionResult() : base() {
            Links = new Collection<SyndicationLink>();
            FeatureResults = new List<FeatureResult>();
            elementExtensions = new SyndicationElementExtensionCollection();
            authors = new Collection<SyndicationPerson>();
            categories = new Collection<SyndicationCategory>();
            contributors = new Collection<SyndicationPerson>();
        }


        public static FeatureCollectionResult FromOpenSearchResultCollection(IOpenSearchResultCollection results) {
            if (results == null)
                throw new ArgumentNullException("results");

            FeatureCollectionResult fc = new FeatureCollectionResult();

            fc.ElementExtensions = new SyndicationElementExtensionCollection(results.ElementExtensions);
           
            if (results.Links != null && results.Links.Count > 0) {
                fc.Links = results.Links;
            }

            fc.authors = results.Authors;
            fc.categories = results.Categories;
            fc.LastUpdatedTime = results.LastUpdatedTime;
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

        [JsonIgnore]
        public new List<Terradue.GeoJson.Feature.Feature> Features { 
            get{
                return FeatureResults.Cast<Terradue.GeoJson.Feature.Feature>().ToList();
            } 
        }

        [JsonProperty(PropertyName="features")]
        [JsonConverter(typeof(FeatureResultJsonConverter))]
        public List<FeatureResult> FeatureResults { get; set; }

        [JsonProperty(PropertyName = "properties")]
        public new Dictionary <string,object> Properties {
            get {
                return GenericFunctions.ExportProperties(this);
            }

            set {
                var json = JsonConvert.SerializeObject(value);
                GenericFunctions.ImportProperties(JToken.Parse(json), this);
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

        [JsonIgnore]
        public IEnumerable<IOpenSearchResultItem> Items {
            get {
                return FeatureResults.Cast<IOpenSearchResultItem>().ToList();
            }
            set {
                FeatureResults = value.Cast<FeatureResult>().ToList();
            }
        }

        List<Terradue.ServiceModel.Syndication.SyndicationLink> links;

        [JsonIgnore]
        public Collection<Terradue.ServiceModel.Syndication.SyndicationLink> Links {
            get {
                return new Collection<SyndicationLink>(links);
            }
            set {
                links = value.ToList();
            }
        }

        SyndicationElementExtensionCollection elementExtensions;
        [JsonIgnore]
        public SyndicationElementExtensionCollection ElementExtensions {
            get {
                return elementExtensions;
            }
            set {
                elementExtensions = value;
            }
        }

        TextSyndicationContent title;
        [JsonIgnore]
        public TextSyndicationContent Title {
            get {
                return title;
            }
            set {
                title = value;
            }
        }

        DateTimeOffset date;
        [JsonIgnore]
        public DateTimeOffset LastUpdatedTime {
            get {
                return date;
            }
            set {
                date = value;
            }
        }

        [JsonIgnore]
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

        [JsonIgnore]
        public long Count {
            get {
                return Items.Count();
            }
        }

        readonly Collection<SyndicationPerson> contributors;
        [JsonIgnore]
        public Collection<SyndicationPerson> Contributors {
            get {
                return contributors;
            }
        }

        TextSyndicationContent copyright;
        [JsonIgnore]
        public TextSyndicationContent Copyright {
            get {
                return copyright;
            }
            set {
                copyright = value;
            }
        }

        TextSyndicationContent description;
        [JsonIgnore]
        public TextSyndicationContent Description {
            get {
                return description;
            }
            set {
                description = value;
            }
        }


        string generator;
        [JsonIgnore]
        public string Generator {
            get {
                return generator;
            }
            set {
                generator = value;
            }
        }

        [JsonIgnore]
        public long TotalResults {
            get {
                var totalResults = ElementExtensions.ReadElementExtensions<string>("totalResults", "http://a9.com/-/spec/opensearch/1.1/");
                return totalResults.Count == 0 ? 0 : long.Parse(totalResults[0]);
            }
            set {
                foreach (var ext in this.ElementExtensions.ToArray()) {
                    if (ext.OuterName == "totalResults" && ext.OuterNamespace == "http://a9.com/-/spec/opensearch/1.1/") {
                        this.ElementExtensions.Remove(ext);
                        continue;
                    }
                }
                this.ElementExtensions.Add(new XElement(XName.Get("totalResults", "http://a9.com/-/spec/opensearch/1.1/"), value).CreateReader());
            }
        }

        public void SerializeToStream(System.IO.Stream stream) {
            JsonSerializer serializer = new JsonSerializer(){NullValueHandling = NullValueHandling.Ignore};
            serializer.Converters.Add(new FeatureCollectionResultJsonConverter());
            StreamWriter sw = new StreamWriter(stream);
            serializer.Serialize(new JsonTextWriter(sw), this);
            sw.Flush();
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
            JsonSerializer serializer = new JsonSerializer(){NullValueHandling = NullValueHandling.Ignore};
            serializer.Converters.Add(new FeatureCollectionResultJsonConverter());
            FeatureCollectionResult fc = serializer.Deserialize<FeatureCollectionResult>(new JsonTextReader(sr));
            return fc;
        }

        [JsonIgnore]
        public string ContentType {
            get {
                return "application/json";
            }
        }

        Collection<SyndicationCategory> categories;
        [JsonIgnore]
        public Collection<SyndicationCategory> Categories {
            get {
                return categories;
            }
        }

        Collection<SyndicationPerson> authors;
        [JsonIgnore]
        public Collection<SyndicationPerson> Authors {
            get {
                return authors;
            }
        }

        IOpenSearchable openSearchable;
        [JsonIgnore]
        public IOpenSearchable OpenSearchable {
            get {
                return openSearchable;
            }
            set {
                openSearchable = value;
            }
        }

        NameValueCollection parameters;
        [JsonIgnore]
        public NameValueCollection Parameters {
            get {
                return parameters;
            }
            set {
                parameters = value;
            }
        }

        [JsonIgnore]
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


    }

}


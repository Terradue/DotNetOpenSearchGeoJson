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
using System.Runtime.Serialization;
using Terradue.ServiceModel.Syndication;
using Terradue.OpenSearch.GeoJson.Import;
using System.Xml;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Xml.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Terradue.OpenSearch.GeoJson.Converter;
using Newtonsoft.Json.Linq;
using Terradue.GeoJson.Geometry;
using Terradue.GeoJson.Feature;
using Terradue.ServiceModel.Ogc.Owc.AtomEncoding;

namespace Terradue.OpenSearch.GeoJson.Result
{

    [DataContract]
    public class FeatureResult : Terradue.GeoJson.Feature.Feature, IOpenSearchResultItem
    {

        public FeatureResult() : base()
        {
            links = new Collection<SyndicationLink>();
            elementExtensions = new SyndicationElementExtensionCollection();
            authors = new Collection<SyndicationPerson>();
            categories = new Collection<SyndicationCategory>();
            contributors = new Collection<SyndicationPerson>();
        }

        public FeatureResult(Terradue.GeoJson.Feature.Feature feature) : base(feature.Geometry, feature.Properties)
        {
            base.Id = feature.Id;
            base.BoundingBoxes = feature.BoundingBoxes;
            base.CRS = feature.CRS;
            links = new Collection<SyndicationLink>();
            elementExtensions = new SyndicationElementExtensionCollection();
            Properties = feature.Properties;
            authors = new Collection<SyndicationPerson>();
            categories = new Collection<SyndicationCategory>();
            contributors = new Collection<SyndicationPerson>();
        }

        public FeatureResult(FeatureResult result) : this((Terradue.GeoJson.Feature.Feature)result)
        {
            links = new Collection<SyndicationLink>(result.Links);
            elementExtensions = new SyndicationElementExtensionCollection(result.elementExtensions);
            this.Title = result.Title;
            this.LastUpdatedTime = result.LastUpdatedTime;
            base.Id = result.Id;
            this.authors = result.Authors;
            this.categories = result.Categories;

        }

        public static FeatureResult FromOpenSearchResultItem(IOpenSearchResultItem result)
        {
            if (result == null)
                throw new ArgumentNullException("result");

            FeatureResult feature;

            var geom = ImportUtils.FindGeometry(result);

            if (geom != null)
                feature = new FeatureResult(new Feature(geom, null));
            else
                feature = new FeatureResult();

            feature.ElementExtensions = new SyndicationElementExtensionCollection(result.ElementExtensions);
            feature.Id = result.Id;
            feature.LastUpdatedTime = result.LastUpdatedTime;
            feature.PublishDate = result.PublishDate;
            feature.Summary = result.Summary;
            feature.Content = result.Content;
            feature.contributors = result.Contributors;
            feature.authors = result.Authors;
            feature.Title = result.Title;
            feature.categories = result.Categories;
            feature.Copyright = result.Copyright;
            feature.Identifier = result.Identifier;

            feature.Links = new Collection<SyndicationLink>(result.Links);

            feature.sortKey = result.SortKey;

            return feature;
        }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <value>The properties are read-only in this class since it is intented for serialization purpose only</value>
        [JsonProperty(PropertyName = "properties")]
        public new Dictionary<string, object> Properties
        {
            get
            {
                return GenericFunctions.ExportProperties(this);
            }

            set
            {
                var json = JsonConvert.SerializeObject(value);
                GenericFunctions.ImportProperties(JToken.Parse(json), this);
            }
        }


        #region IOpenSearchResultItem implementation

        public bool ShowNamespaces
        {
            get
            {
                return false;
            }
            set
            {

            }
        }

        string sortKey;
        public string SortKey
        {
            get
            {
                if (sortKey == null)
                    return LastUpdatedTime.ToUniversalTime().ToString("O");
                return sortKey.ToString();
            }
            set
            {
                sortKey = value;
            }
        }

        TextSyndicationContent title;

        [JsonIgnore]
        public TextSyndicationContent Title
        {
            get
            {
                if (title != null)
                    return title;
                var titles = ElementExtensions.ReadElementExtensions<string>("title", "");
                if (titles.Count > 0)
                    return new TextSyndicationContent(titles[0]);
                if (Identifier != null)
                    return new TextSyndicationContent(Identifier);
                return null;
            }
            set
            {
                title = value;
            }
        }

        DateTimeOffset lastUpdatedTime;

        [JsonIgnore]
        public DateTimeOffset LastUpdatedTime
        {
            get
            {
                return lastUpdatedTime;
            }
            set
            {
                lastUpdatedTime = value;
            }
        }

        DateTimeOffset publishDate;

        [JsonIgnore]
        public DateTimeOffset PublishDate
        {
            get
            {
                return publishDate;
            }
            set
            {
                publishDate = value;
            }
        }

        [JsonIgnore]
        public string Identifier
        {
            get
            {
                var identifier = ElementExtensions.ReadElementExtensions<string>("identifier", "http://purl.org/dc/elements/1.1/");
                if (identifier.Count > 0)
                    return identifier[0];
                if (!string.IsNullOrEmpty(base.Id))
                    return base.Id;
                identifier = ElementExtensions.ReadElementExtensions<string>("identifier", "");
                if (identifier.Count > 0)
                    return identifier[0];
                identifier = ElementExtensions.ReadElementExtensions<string>("id", "");
                if (identifier.Count > 0)
                    return identifier[0];
                return Guid.NewGuid().ToString();
            }
            set
            {
                if (value == null)
                    return;
                foreach (var ext in this.ElementExtensions.ToArray())
                {
                    if (ext.OuterName == "identifier" && ext.OuterNamespace == "http://purl.org/dc/elements/1.1/")
                    {
                        this.ElementExtensions.Remove(ext);
                        continue;
                    }
                }
                this.ElementExtensions.Add(new XElement(XName.Get("identifier", "http://purl.org/dc/elements/1.1/"), value).CreateReader());
            }
        }

        Collection<Terradue.ServiceModel.Syndication.SyndicationLink> links;
        [JsonIgnore]
        public Collection<Terradue.ServiceModel.Syndication.SyndicationLink> Links
        {
            get
            {
                return links;
            }
            set
            {
                links = value;
            }
        }

        SyndicationElementExtensionCollection elementExtensions;

        [JsonIgnore]
        public SyndicationElementExtensionCollection ElementExtensions
        {
            get
            {
                return elementExtensions;
            }
            set
            {
                elementExtensions = value;
            }
        }


        Collection<SyndicationCategory> categories;
        [JsonIgnore]
        public Collection<SyndicationCategory> Categories
        {
            get
            {
                return categories;
            }
        }

        Collection<SyndicationPerson> authors;
        [JsonIgnore]
        public Collection<SyndicationPerson> Authors
        {
            get
            {
                return authors;
            }
        }

        TextSyndicationContent summary;
        [JsonIgnore]
        public TextSyndicationContent Summary
        {
            get
            {
                return summary;
            }
            set
            {
                summary = value;
            }
        }

        Collection<SyndicationPerson> contributors;
        [JsonIgnore]
        public Collection<SyndicationPerson> Contributors
        {
            get
            {
                return contributors;
            }
        }

        TextSyndicationContent copyright;
        [JsonIgnore]
        public TextSyndicationContent Copyright
        {
            get
            {
                return copyright;
            }
            set
            {
                copyright = value;
            }
        }

        SyndicationContent content;
        [JsonIgnore]
        public SyndicationContent Content
        {
            get
            {
                return content;
            }
            set
            {
                content = value;
            }
        }

        #endregion

        [JsonIgnore]
        public DateTimeInterval DateTimeInterval
        {
            get
            {
                var date = ElementExtensions.ReadElementExtensions<string>("date", "http://purl.org/dc/elements/1.1/");
                if (date.Count > 0)
                    return DateTimeInterval.Parse(date[0]);
                date = ElementExtensions.ReadElementExtensions<string>("date", "");
                if (date.Count > 0)
                    return DateTimeInterval.Parse(date[0]);
                return null;
            }
            set
            {
                if (value == null)
                    return;
                foreach (var ext in this.ElementExtensions.ToArray())
                {
                    if (ext.OuterName == "date" && ext.OuterNamespace == "http://purl.org/dc/elements/1.1/")
                    {
                        this.ElementExtensions.Remove(ext);
                        continue;
                    }
                }
                this.ElementExtensions.Add(new XElement(XName.Get("date", "http://purl.org/dc/elements/1.1/"), value.ToString()).CreateReader());
            }
        }
    }
}

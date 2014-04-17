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
using System.ServiceModel.Syndication;
using Terradue.OpenSearch.GeoJson.Import;

namespace Terradue.OpenSearch.GeoJson.Result {
    [DataContract]
    public class FeatureCollectionResult : Terradue.GeoJson.Feature.FeatureCollection, IOpenSearchResultCollection {
        public FeatureCollectionResult(Terradue.GeoJson.Feature.FeatureCollection fc) : base() {
            base.BoundingBoxes = fc.BoundingBoxes;
            base.CRS = fc.CRS;
            base.Properties = fc.Properties;
            Links = new List<SyndicationLink>();
            fc.Features.FirstOrDefault(f => {
                FeatureResults.Add(new FeatureResult(f));
                return false;
            });
        }

        public FeatureCollectionResult() : base() {
            Links = new List<SyndicationLink>();
            FeatureResults = new List<FeatureResult>();
        }

        [IgnoreDataMember]
        public new List<Terradue.GeoJson.Feature.Feature> Features { get; private set; }

        [DataMember(Name="features")]
        public List<FeatureResult> FeatureResults { get; private set; }

        [DataMember(Name="properties")]
        public new Dictionary <string,object> Properties {
            get {
                Dictionary <string,object> properties = base.Properties;
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
            set {
                base.Properties = value;
                if ( base.Properties.ContainsKey("links") && base.Properties["links"] is Dictionary<string,object>[]) {
                    Links = new List<SyndicationLink>();
                    foreach (object link in (Dictionary<string,object>[])base.Properties["links"]) {
                        if (link is Dictionary<string, object>) {
                            Links.Add(ImportUtils.FromDictionnary((Dictionary<string, object>)link));
                        }
                    }
                }
            }
        }

        #region IResultCollection implementation

        public List<string> GetSelfShortList() {
            return base.Features.Select(f => f.Id).ToList();
        }

        [IgnoreDataMember]
        public List<IOpenSearchResultItem> Items {
            get {
                return FeatureResults.Cast<IOpenSearchResultItem>().ToList();
            }
        }

        List<System.ServiceModel.Syndication.SyndicationLink> links;

        [IgnoreDataMember]
        public List<System.ServiceModel.Syndication.SyndicationLink> Links {
            get {
                return links;
            }
            set {
                links = value;
            }
        }

        [IgnoreDataMember]
        public System.Xml.XmlNodeList ElementExtensions {
            get {
                throw new NotImplementedException();
            }
        }

        [IgnoreDataMember]
        public string Title {
            get {
                throw new NotImplementedException();
            }
        }

        [IgnoreDataMember]
        public DateTime Date {
            get {
                throw new NotImplementedException();
            }
        }

        [IgnoreDataMember]
        public string Identifier {
            get {
                if (this.Properties.ContainsKey("identifier")) return this.Properties["identifier"].ToString();
                if (this.Properties.ContainsKey("dc:identifier")) return this.Properties["dc:identifier"].ToString();
                return null;
            }
        }

        [IgnoreDataMember]
        public long Count {
            get {
                throw new NotImplementedException();
            }
        }

        public void Serialize(System.IO.Stream stream) {
            JsConfig.ExcludeTypeInfo = true;
            JsConfig.IncludeTypeInfo = false;
            JsonSerializer.SerializeToStream(this, stream);
        }

        #endregion
    }

    [DataContract]
    public class FeatureResult : Terradue.GeoJson.Feature.Feature, IOpenSearchResultItem {
        public FeatureResult(Terradue.GeoJson.Feature.Feature feature) : base(feature.Geometry, feature.Properties) {
            base.Id = feature.Id;
            base.BoundingBoxes = feature.BoundingBoxes;
            base.CRS = feature.CRS;
            links = new List<SyndicationLink>();
        }

        [DataMember(Name="properties")]
        public new Dictionary <string,object> Properties {
            get {
                Dictionary <string,object> properties = base.Properties;
                if (properties.ContainsKey("links")) {
                    properties.Remove("links");
                }
                string prefix = "";
                if (properties.ContainsKey("@namespaces")) prefix = "atom:";
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
                return properties;
            }
            set {
                base.Properties = value;
                if ( base.Properties.ContainsKey("links") && base.Properties["links"] is Dictionary<string,object>[]) {
                    Links = new List<SyndicationLink>();
                    foreach (object link in (Dictionary<string,object>[])base.Properties["links"]) {
                        if (link is Dictionary<string, object>) {
                            Links.Add(ImportUtils.FromDictionnary((Dictionary<string, object>)link));
                        }
                    }
                }
            }
        }

        #region IOpenSearchResultItem implementation

        [IgnoreDataMember]
        public string Title {
            get {
                throw new NotImplementedException();
            }
        }

        [IgnoreDataMember]
        public DateTime Date {
            get {
                throw new NotImplementedException();
            }
        }

        [IgnoreDataMember]
        public string Identifier {
            get {
                if (this.Properties.ContainsKey("identifier")) return this.Properties["identifier"].ToString();
                if (this.Properties.ContainsKey("dc:identifier")) return this.Properties["dc:identifier"].ToString();
                return null;
            }
        }

        List<System.ServiceModel.Syndication.SyndicationLink> links;
        [IgnoreDataMember]
        public List<System.ServiceModel.Syndication.SyndicationLink> Links {
            get {
                return links;
            }
            set {
                links = value;
            }
        }

        [IgnoreDataMember]
        public System.Xml.XmlNodeList ElementExtensions {
            get {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}


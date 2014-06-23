//
//  FeatureCollectionOpenSearchEngineExtension.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using Mono.Addins;
using Terradue.ServiceModel.Syndication;
using System.Collections.Generic;
using System.Xml;
using System.Net;
using System.Collections.Specialized;
using System.Linq;
using System.Collections.ObjectModel;
using Terradue.OpenSearch.Engine.Extensions;
using Terradue.OpenSearch.GeoJson.Result;
using Terradue.OpenSearch.GeoJson.Import;
using Terradue.OpenSearch.Response;
using Terradue.OpenSearch.Schema;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Result;

[assembly:Addin]
[assembly:AddinDependency ("OpenSearchEngine", "1.0")]

namespace Terradue.OpenSearch.GeoJson.Extensions {

    /// <summary>
    /// Atom open search engine extension.
    /// </summary>
    [Extension(typeof(IOpenSearchEngineExtension))]
    [ExtensionNode("GeoJson", "GeoJson native query")]
    public class FeatureCollectionOpenSearchEngineExtension : OpenSearchEngineExtension<FeatureCollectionResult> {

        public FeatureCollectionOpenSearchEngineExtension() {
        }

        static bool qualified;

        public static bool Qualified {
            get {
                return qualified;
            }
            set {
                qualified = value;
            }
        }

        #region OpenSearchEngineExtension implementation

        public override string Identifier {
            get {
                return "GeoJson";
            }
        }

        public override string Name {
            get {
                return "GeoJson Feature Collection";
            }
        }

        public override string[] GetInputFormatTransformPath() {
            return new string[] { "application/rdf+xml", "application/atom+xml" };
        }

        public override IOpenSearchResultCollection TransformResponse(OpenSearchResponse response) {
            if (response.ContentType == "application/atom+xml")
                return TransformAtomResponseToFeatureCollection(response);
            if (response.ContentType == "application/xml")
                return TransformAtomResponseToFeatureCollection(response);
            if (response.ContentType == "application/rdf+xml")
                return TransformRdfXmlDocumentToFeatureCollection(response);
            if (response.ContentType == "application/json")
                return TransformJsonResponseToFeatureCollection(response);

            throw new NotSupportedException("GeoJson extension does not transform OpenSearch response from " + response.ContentType);
        }

        public override string DiscoveryContentType {
            get {
                return "application/json";
            }
        }

        public override OpenSearchUrl FindOpenSearchDescriptionUrlFromResponse(OpenSearchResponse response) {

            if (response.ContentType == "application/json") {
                // TODO
                throw new NotImplementedException();
            }

            return null;
        }

        public override SyndicationLink[] GetEnclosures(IOpenSearchResult result) {

            if (!(result.Result is FeatureCollectionResult)) throw new InvalidOperationException("The extension is able to get enclosure only from FeatureCollectionResult");

            List<SyndicationLink> links = new List<SyndicationLink>();

            foreach (FeatureResult item in ((FeatureCollectionResult)result.Result).Features) {
                foreach (SyndicationLink link in item.Links) {
                    if (link.RelationshipType == "enclosure") {
                        links.Add(link);
                    }
                }
            }

            return links.ToArray();

        }

        #region implemented abstract members of OpenSearchEngineExtension

        public override IOpenSearchResultCollection CreateOpenSearchResultFromOpenSearchResult(IOpenSearchResultCollection results) {
            if (results is FeatureCollectionResult)
                return results;

            return FeatureCollectionResult.FromOpenSearchResultCollection(results);
        }

        #endregion

        #endregion

        public FeatureCollectionResult TransformAtomResponseToFeatureCollection(OpenSearchResponse response) {
            // First query natively the ATOM
            return AtomFeedToFeatureCollection(AtomOpenSearchEngineExtension.TransformAtomResponseToAtomFeed(response));
        }

        public static FeatureCollectionResult AtomFeedToFeatureCollection(AtomFeed feed) {

            return FeatureCollectionResult.FromOpenSearchResultCollection(feed);

        }

        public FeatureCollectionResult TransformRdfXmlDocumentToFeatureCollection(OpenSearchResponse response) {

            return ResultCollectionToFeatureCollection(RdfOpenSearchEngineExtension.TransformRdfResponseToRdfXmlDocument(response));

        }

        protected FeatureCollectionResult ResultCollectionToFeatureCollection(IOpenSearchResultCollection results) {

            return FeatureCollectionResult.FromOpenSearchResultCollection(results);

        }

        public static FeatureCollectionResult TransformJsonResponseToFeatureCollection(OpenSearchResponse response) {

            return (FeatureCollectionResult)FeatureCollectionResult.DeserializeFromStream(response.GetResponseStream());

        }

        //---------------------------------------------------------------------------------------------------------------------
        public static void ReplaceSelfLinks(IOpenSearchResult osr, Func<FeatureResult,OpenSearchDescription,string,string> entryTemplate) {
            FeatureCollectionResult feed = (FeatureCollectionResult)osr.Result;

            var matchLinks = feed.Links.Where(l => l.RelationshipType == "self").ToArray();
            foreach (var link in matchLinks) {
                feed.Links.Remove(link);
            }

            IProxiedOpenSearchable entity = (IProxiedOpenSearchable)osr.OpenSearchableEntity;
            OpenSearchDescription osd = entity.GetProxyOpenSearchDescription();
            UriBuilder myUrl = new UriBuilder(OpenSearchFactory.GetOpenSearchUrlByType(osd, "application/json").Template);
            string[] queryString = Array.ConvertAll(osr.SearchParameters.AllKeys, key => string.Format("{0}={1}", key, osr.SearchParameters[key]));
            myUrl.Query = string.Join("&", queryString);

            feed.Links.Add(new SyndicationLink(myUrl.Uri, "self", "Reference link", "application/json", 0));

            foreach (FeatureResult item in feed.Items) {
                string template = entryTemplate(item,osd,"application/json");
                if (template != null) {
                    item.Links.Add(new SyndicationLink(new Uri(template), "self", "Reference link", "application/json", 0));
                }
            }
        }

        public static void ReplaceOpenSearchDescriptionLinks(IOpenSearchResult osr) {
            FeatureCollectionResult fc = (FeatureCollectionResult)osr.Result;

            var matchLinks = fc.Links.Where(l => l.RelationshipType == "search").ToArray();
            foreach (var link in matchLinks) {
                fc.Links.Remove(link);
            }
            OpenSearchDescription osd;
            if ( osr.OpenSearchableEntity is IProxiedOpenSearchable )
                osd = ((IProxiedOpenSearchable)osr.OpenSearchableEntity).GetProxyOpenSearchDescription();
            else
                osd = osr.OpenSearchableEntity.GetOpenSearchDescription();
            OpenSearchDescriptionUrl url = OpenSearchFactory.GetOpenSearchUrlByRel(osd, "self");
            if ( url != null ){
                fc.Links.Add(new SyndicationLink(new Uri(url.Template), "search", "OpenSearch Description link", "application/opensearchdescription+xml", 0));
                foreach (FeatureResult item in fc.Items) {
                    matchLinks = item.Links.Where(l => l.RelationshipType == "search").ToArray();
                    foreach (var link in matchLinks) {
                        item.Links.Remove(link);
                    }
                    item.Links.Add(new SyndicationLink(new Uri(url.Template), "search", "OpenSearch Description link", "application/opensearchdescription+xml", 0));
                }
            }

        }

        public static void ReplaceId(IOpenSearchResult osr, Func<FeatureResult,OpenSearchDescription,string,string> entryTemplate) {

            FeatureCollectionResult fc = (FeatureCollectionResult)osr.Result;

            IProxiedOpenSearchable entity = (IProxiedOpenSearchable)osr.OpenSearchableEntity;
            OpenSearchDescription osd = entity.GetProxyOpenSearchDescription();
            UriBuilder myUrl = new UriBuilder(OpenSearchFactory.GetOpenSearchUrlByType(osd, "application/json").Template);
            string[] queryString = Array.ConvertAll(osr.SearchParameters.AllKeys, key => string.Format("{0}={1}", key, osr.SearchParameters[key]));
            myUrl.Query = string.Join("&", queryString);

            if (fc.Properties.ContainsKey("@namespaces")) {
                fc.Properties["atom:id"] = myUrl.ToString();
            } else {
                fc.Properties["id"] = myUrl.ToString();
            }

            foreach (FeatureResult item in fc.Items) {
                string template = entryTemplate(item,osd,"application/json");
                if (template != null) {
                    item.Id = template;
                }

            }

        }
    }
}


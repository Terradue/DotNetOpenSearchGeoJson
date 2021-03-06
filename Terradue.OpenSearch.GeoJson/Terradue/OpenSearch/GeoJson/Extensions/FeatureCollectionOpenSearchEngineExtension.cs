//
//  FeatureCollectionOpenSearchEngineExtension.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
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
using System.IO;
using System.Text;

namespace Terradue.OpenSearch.GeoJson.Extensions {

    /// <summary>
    /// Atom open search engine extension.
    /// </summary>
    [OpenSearchEngineExtension("GeoJson", "GeoJson native query")]
    public class FeatureCollectionOpenSearchEngineExtension : OpenSearchEngineExtension<FeatureCollectionResult> {

        public FeatureCollectionOpenSearchEngineExtension() {
        }

        #region OpenSearchEngineExtension implementation

        public override string Identifier {
            get {
                return "json";
            }
        }

        public override string Name {
            get {
                return "GeoJson Feature Collection";
            }
        }

        public override IOpenSearchResultCollection ReadNative(IOpenSearchResponse response) {
            if (response.ObjectType == typeof(byte[])) {
                if (response.ContentType == "application/json")
                    return TransformJsonResponseToFeatureCollection(response);

                throw new NotSupportedException("GeoJson extension does not transform OpenSearch response of contentType " + response.ContentType);
            }
            throw new NotSupportedException("GeoJson extension does not transform response of type " + response.GetType());

        }

        public override string DiscoveryContentType {
            get {
                return "application/json";
            }
        }

        public override OpenSearchUrl FindOpenSearchDescriptionUrlFromResponse(IOpenSearchResponse response) {

            if (response.ObjectType == typeof(byte[])) {
                if (response.ContentType == "application/json") {

                    FeatureCollectionResult col = (FeatureCollectionResult)FeatureCollectionResult.DeserializeFromStream(new MemoryStream((byte[])response.GetResponseObject()));
                    col.OpenSearchable = response.Entity;

                    var link = col.Links.FirstOrDefault(l => l.RelationshipType == "search");
                    if (link != null)
                        return new OpenSearchUrl(link.Uri);
                    else
                        return null;

                }
            }

            return null;
        }

        public override IOpenSearchResultCollection CreateOpenSearchResultFromOpenSearchResult(IOpenSearchResultCollection results) {
            if (results is FeatureCollectionResult)
                return results;

            return FeatureCollectionResult.FromOpenSearchResultCollection(results);
        }

        #endregion

        public FeatureCollectionResult ResultCollectionToFeatureCollection(IOpenSearchResultCollection results) {

            return FeatureCollectionResult.FromOpenSearchResultCollection(results);

        }

        public static FeatureCollectionResult TransformJsonResponseToFeatureCollection(IOpenSearchResponse response) {

            return (FeatureCollectionResult)FeatureCollectionResult.DeserializeFromStream(new MemoryStream((byte[])response.GetResponseObject()));

        }

        //---------------------------------------------------------------------------------------------------------------------

    }
}


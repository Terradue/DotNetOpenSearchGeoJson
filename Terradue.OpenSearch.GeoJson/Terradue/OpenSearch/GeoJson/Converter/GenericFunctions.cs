﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terradue.ServiceModel.Syndication;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using System.Linq;
using Newtonsoft.Json;
using Terradue.OpenSearch.GeoJson.Result;
using System.IO;
using System.Xml;
using Terradue.OpenSearch.GeoJson.Import;

namespace Terradue.OpenSearch.GeoJson.Converter {
    public class GenericFunctions {
        public GenericFunctions() {
        }

        public static Dictionary<string, object> ExportProperties(FeatureResult feature) {

            // Start with an empty NameValueCollection
            Dictionary <string,object> properties = new Dictionary<string, object>();

            if (feature.Links != null && feature.Links.Count > 0) {
                properties["links"] = GenericFunctions.LinksToProperties(feature.Links);
            }
            if (feature.LastUpdatedTime.Ticks != 0)
                properties["updated"] = feature.LastUpdatedTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
            if (feature.PublishDate.Ticks != 0)
                properties["published"] = feature.PublishDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
            if (feature.Title != null)
                properties["title"] = feature.Title.Text;
            if (feature.Summary != null)
                properties["summary"] = feature.Summary.Text;
            if (feature.Content != null) {
                MemoryStream ms = new MemoryStream();
                var xw = XmlWriter.Create(ms);
                feature.Content.WriteTo(xw, "content", "");
                xw.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                properties["content"] = XElement.Load(XmlReader.Create(ms)).Value;
            }

            if (feature.Authors != null && feature.Authors.Count > 0) {
                List<object> authors = new List<object>();
                foreach (var author in feature.Authors) {
                    Dictionary<string,object> authord = GenericFunctions.ExportSyndicationElementExtensions(author.ElementExtensions);
                    if (authord == null)
                        authord = new Dictionary<string, object>();
                    authord.Add("email", author.Email);
                    authord.Add("name", author.Name);
                    if (author.Uri != null)
                        authord.Add("uri", author.Uri.ToString());

                    authors.Add(authord);
                }
                properties["authors"] = authors;
            }

            if (feature.Categories != null && feature.Categories.Count > 0) {
                List<object> categories = new List<object>();
                foreach (var category in feature.Categories) {
                    Dictionary<string,object> cat = GenericFunctions.ExportSyndicationElementExtensions(category.ElementExtensions);
                    if (cat == null)
                        cat = new Dictionary<string, object>();
                    cat.Add("@term", category.Name);
                    cat.Add("@label", category.Label);
                    cat.Add("@scheme", category.Scheme);

                    categories.Add(cat);
                }
                properties["categories"] = categories;
            }

            if (feature.Copyright != null) {
                properties["copyright"] = feature.Copyright.Text;
            }

            var exts = GenericFunctions.ExportSyndicationElementExtensions(feature.ElementExtensions);

            properties = properties.Concat(exts).ToDictionary(x => x.Key, x => x.Value);

            return properties;
        }

        public static Dictionary<string, object> ExportProperties(FeatureCollectionResult fc) {

            // Start with an empty NameValueCollection
            Dictionary <string,object> properties = new Dictionary<string, object>();

            if (fc.Links != null && fc.Links.Count > 0) {
                properties["links"] = GenericFunctions.LinksToProperties(fc.Links);
            }
            if (fc.LastUpdatedTime.Ticks != 0)
                properties["updated"] = fc.LastUpdatedTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
            if (fc.Title != null)
                properties["title"] = fc.Title.Text;

            if (fc.Authors != null && fc.Authors.Count > 0) {
                List<object> authors = new List<object>();
                foreach (var author in fc.Authors) {
                    Dictionary<string,object> authord = GenericFunctions.ExportSyndicationElementExtensions(author.ElementExtensions);
                    authord.Add("email", author.Email);
                    authord.Add("name", author.Name);
                    if (author.Uri != null)
                        authord.Add("uri", author.Uri.ToString());

                    authors.Add(authord);
                }
                properties["authors"] = authors;
            }

            if (fc.Categories != null && fc.Categories.Count > 0) {
                List<object> categories = new List<object>();
                foreach (var category in fc.Categories) {
                    Dictionary<string,object> cat = GenericFunctions.ExportSyndicationElementExtensions(category.ElementExtensions);
                    cat.Add("@term", category.Name);
                    cat.Add("@label", category.Label);
                    cat.Add("@scheme", category.Scheme);

                    categories.Add(cat);
                }
                properties["categories"] = categories;
            }

            if (fc.Copyright != null) {
                properties["copyright"] = fc.Copyright.Text;
            }

            var exts = GenericFunctions.ExportSyndicationElementExtensions(fc.ElementExtensions);

            properties = properties.Concat(exts).ToDictionary(x => x.Key, x => x.Value);

            return properties;
        }

        public static List<Dictionary<string,object>> LinksToProperties(Collection<Terradue.ServiceModel.Syndication.SyndicationLink> ilinks) {

            List<Dictionary<string,object>> links = new List<Dictionary<string,object>>();
            foreach (SyndicationLink link in ilinks) {
                Dictionary<string,object> newlink = new Dictionary<string,object>();
                newlink.Add("@href", link.Uri.ToString());
                if (link.RelationshipType != null)
                    newlink.Add("@rel", link.RelationshipType);
                if (link.Title != null)
                    newlink.Add("@title", link.Title);
                if (link.MediaType != null)
                    newlink.Add("@type", link.MediaType);
                if (link.Length != 0)
                    newlink.Add("@length", link.Length);

                links.Add(newlink);
            }

            return links;
        }

        public static SyndicationContent TrySyndicationContent(JProperty prop) {

            SyndicationContent content;

            try {
                content = SyndicationContent.CreateXmlContent(XElement.Parse(prop.Value.ToString()));
            } catch {
            }
            try {
                content = SyndicationContent.CreateUrlContent(new Uri(prop.Value.ToString()), "");
            } catch {
            }
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(prop.Value.ToString());
            if (doc.ParseErrors.Count() == 0)
                content = SyndicationContent.CreateHtmlContent(prop.Value.ToString());
            else
                content = SyndicationContent.CreatePlaintextContent(prop.Value.ToString());


            return content;
        }

        public static TextSyndicationContent TryTextSyndicationContent(JProperty prop) {

            TextSyndicationContent content;

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(prop.Value.ToString());
            if (doc.ParseErrors.Count() == 0)
                content = TextSyndicationContent.CreateHtmlContent(prop.Value.ToString());
            else
                content = TextSyndicationContent.CreatePlaintextContent(prop.Value.ToString());


            return content;
        }

        public static Dictionary<string, object> ExportSyndicationElementExtensions(SyndicationElementExtensionCollection exts) {
            
            var xml = XElement.Load(exts.GetReaderAtExtensionWrapper());

            xml = RemoveAllNamespaces(xml);

            string json = JsonConvert.SerializeXNode(xml, Newtonsoft.Json.Formatting.None, true);

            var dic = (Dictionary<string, object>)Deserialize(json);

            return dic;
        }

        public static object Deserialize(string json) {
            return ToObject(JToken.Parse(json));
        }

        private static object ToObject(JToken token) {
            switch (token.Type) {
                case JTokenType.Object:
                    return token.Children<JProperty>()
                        .ToDictionary(prop => prop.Name,
                                      prop => ToObject(prop.Value));

                case JTokenType.Array:
                    return token.Select(ToObject).ToList();

                default:
                    return ((JValue)token).Value;
            }
        }

        public static void ImportProperties(JToken propObject, FeatureResult feature) {

            foreach (var child in propObject.Children<JProperty>()) {

                if (child.Path == "properties.authors") {
                    foreach (var linkObject in child.Values())
                        feature.Authors.Add(SyndicationPersonFromJTokenList(linkObject));
                    continue;
                }

                if (child.Path == "properties.contirbutors") {
                    foreach (var linkObject in child.Values())
                        feature.Contributors.Add(SyndicationPersonFromJTokenList(linkObject));
                    continue;
                }

                if (child.Path == "properties.title") {
                    feature.Title = GenericFunctions.TryTextSyndicationContent(child);
                    continue;
                }

                if (child.Path == "properties.links") {
                    foreach (var linkObject in child.Values())
                        feature.Links.Add(SyndicationLinkFromJTokenList(linkObject));
                    continue;
                }

                if (child.Path == "properties.published") {
                    feature.PublishDate = child.Values().First().Value<DateTime>();
                    continue;
                }

                if (child.Path == "properties.updated") {
                    feature.LastUpdatedTime = child.Values().First().Value<DateTime>();
                    continue;
                }

                if (child.Path == "properties.summary") {
                    feature.Summary = GenericFunctions.TryTextSyndicationContent(child);
                    continue;
                }

                if (child.Path == "properties.content") {
                    feature.Content = GenericFunctions.TrySyndicationContent(child);
                    continue;                                             
                }

                if (child.Path == "properties.copyright") {
                    feature.Copyright = GenericFunctions.TryTextSyndicationContent(child);
                    continue;
                }

                var xmlReader = JsonConvert.DeserializeXNode("{" + child.ToString() + "}").CreateReader();

                feature.ElementExtensions.Add(new SyndicationElementExtension(xmlReader));
            }
        }

        public static void ImportProperties(JToken propObject, FeatureCollectionResult fc) {

            foreach (var child in propObject.Children<JProperty>()) {

                if (child.Path == "properties.authors") {
                    foreach (var linkObject in child.Values())
                        fc.Authors.Add(SyndicationPersonFromJTokenList(linkObject));
                    continue;
                }

                if (child.Path == "properties.contirbutors") {
                    foreach (var linkObject in child.Values())
                        fc.Contributors.Add(SyndicationPersonFromJTokenList(linkObject));
                    continue;
                }

                if (child.Path == "properties.title") {
                    fc.Title = GenericFunctions.TryTextSyndicationContent(child);
                    continue;
                }

                if (child.Path == "properties.links") {
                    foreach (var linkObject in child.Values())
                        fc.Links.Add(SyndicationLinkFromJTokenList(linkObject));
                    continue;
                }

                if (child.Path == "properties.updated") {
                    fc.LastUpdatedTime = child.Values().First().Value<DateTime>();
                    continue;
                }

                if (child.Path == "properties.title") {
                    fc.Title = GenericFunctions.TryTextSyndicationContent(child);
                    continue;
                }

                if (child.Path == "properties.copyright") {
                    fc.Copyright = GenericFunctions.TryTextSyndicationContent(child);
                    continue;
                }

                var xmlReader = JsonConvert.DeserializeXNode("{" + child.ToString() + "}").CreateReader();

                fc.ElementExtensions.Add(new SyndicationElementExtension(xmlReader));
            }
        }

        public static SyndicationLink SyndicationLinkFromJTokenList(JToken link, string prefix = "") {
            if (link.SelectToken("@href") != null) {
                long length = 0;
                string rel = null;
                string title = null;
                string type = null;
                Uri href = new Uri((string)link.SelectToken("@href").ToString());
                object r = null;
                if (link.SelectToken("@rel") != null)
                    rel = link.SelectToken("@rel").ToString();
                if (link.SelectToken("@title") != null)
                    title = link.SelectToken("@title").ToString();
                if (link.SelectToken("@type") != null)
                    type = link.SelectToken("@type").ToString();
                if (link.SelectToken("@size") != null)
                    long.TryParse(link.SelectToken("@size").ToString(), out length);
                else
                    length = 0;
                SyndicationLink slink = new SyndicationLink(href, rel, title, type, length);
                return slink;
            }
            throw new ArgumentException("Not a link");
        }

        public static SyndicationPerson SyndicationPersonFromJTokenList(JToken person, string prefix = "") {
            if (person.SelectToken("name") != null) {

                SyndicationPerson sp = new SyndicationPerson();

                foreach (var child in person.Children<JProperty>()) {
                    if (child.Name == "uri") {
                        sp.Uri = child.Value.ToString();
                        continue;
                    }
                    if (child.Name == "email") {
                        sp.Email = (string)child.Value.ToString();
                        continue;
                    }
                    if (child.Name == "name") {
                        sp.Name = (string)child.Value.ToString();
                        continue;
                    }

                    var xmlReader = JsonConvert.DeserializeXNode("{" + child.ToString() + "}").CreateReader();
                    sp.ElementExtensions.Add(new SyndicationElementExtension(xmlReader));
                }

                return sp;
            }
            throw new ArgumentException("Not a link");
        }

        public static XElement RemoveAllNamespaces(XElement e)
        {
            return new XElement(e.Name.LocalName,
                        (from n in e.Nodes()
            select ((n is XElement) ? RemoveAllNamespaces(n as XElement) : n)),
                        (e.HasAttributes) ? 
                        (from a in e.Attributes() 
             where (!a.IsNamespaceDeclaration)  
             select new XAttribute(a.Name.LocalName, a.Value)) : null);
        }   
    }
}

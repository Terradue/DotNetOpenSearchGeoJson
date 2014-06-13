//
//  ImportUtils.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Collections.Generic;
using System.Xml;
using Terradue.ServiceModel.Syndication;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Linq;

namespace Terradue.OpenSearch.GeoJson.Import {
    public class ImportUtils {
        ImportOptions options;

        public ImportUtils(ImportOptions options) {
            this.options = options;
        }

        public Dictionary<string, object> ImportXmlDocument(XmlElement[] elements, ref NameValueCollection namespaces) {
            string prefix = "";
            Dictionary<string,object> prop = new Dictionary<string, object>();
            foreach (XmlElement node in elements) {
                if (node.Prefix == "xmlns")
                    continue;
                if (options.KeepNamespaces) {
                    prefix = node.Prefix + ":";
                    XmlNamespaceManager xnsm2 = new XmlNamespaceManager(node.OwnerDocument.NameTable);
                    foreach (var names in xnsm2.GetNamespacesInScope(XmlNamespaceScope.All)) {
                        namespaces.Set(names.Key, names.Value);
                    }
                }
                prop.Add(prefix + node.LocalName, ImportNode(node));
            }
            return prop;
        }

        public Dictionary<string, object> ImportAttributeExtensions(Dictionary<XmlQualifiedName, string> attributeExtensions, NameValueCollection namespaces, XmlNamespaceManager xnsm) {
       
            Dictionary<string, object> properties = new Dictionary<string, object>();
            string prefix = "";

            foreach (var key in attributeExtensions.Keys) {
                if (options.KeepNamespaces) {
                    prefix = xnsm.LookupPrefix(key.Namespace) + ":";
                }
                properties.Add("@" + prefix + key.Name, attributeExtensions[key]);
            }
            return properties;
        }

        public Dictionary<string, object> ImportElementExtensions(SyndicationElementExtensionCollection elementExtensions, NameValueCollection namespaces) {
            Dictionary<string, object> prop = new Dictionary<string, object>();
            string prefix = "";
            foreach (SyndicationElementExtension extension in elementExtensions) {
                XmlReader reader = extension.GetReader();
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(reader);
                if (options.KeepNamespaces) {
                    prefix = xmlDoc.Prefix + ":";
                    XmlNamespaceManager xnsm2 = new XmlNamespaceManager(reader.NameTable);
                    foreach (var names in xnsm2.GetNamespacesInScope(XmlNamespaceScope.All)) {
                        namespaces.Set(names.Key, names.Value);
                    }
                }
                foreach (XmlNode node in xmlDoc.ChildNodes) {
                    if (node.Prefix == "xmlns")
                        continue;
                    prop.Add(prefix + node.LocalName, ImportNode(node));
                }
            }
            return prop; 
        }

        object ImportNode(XmlNode node) {
            string prefix = "";
            if (options.KeepNamespaces) {
                prefix = node.Prefix + ":";
            }

            if (node.NodeType == XmlNodeType.Attribute)
                return node.Value;
            if (node.NodeType == XmlNodeType.Text)
                return node.InnerText;
            if (node.NodeType == XmlNodeType.Element | node.NodeType == XmlNodeType.Document) {

                if (node.ChildNodes.Count == 1 && node.FirstChild.NodeType == XmlNodeType.Text) {
                    return node.FirstChild.InnerText;
                }

                Dictionary<string, object> properties = new Dictionary<string, object>();
                foreach (XmlNode childnode in node.ChildNodes) {
                    if (node.Prefix == "xmlns")
                        continue;
                    try {
                        properties.Add(prefix + childnode.LocalName, ImportNode(childnode));
                    } catch (ArgumentException) {
                        if (properties[prefix + childnode.LocalName] is object[]) {
                            object[] array = (object[])properties[prefix + childnode.LocalName];
                            List<object> list = array.ToList();
                            list.Add(ImportNode(childnode));
                            properties[prefix + childnode.LocalName] = list.ToArray();
                        } else {
                            List<object> list = new List<object>();
                            list.Add(ImportNode(childnode));
                            properties[prefix + childnode.LocalName] = list.ToArray();
                        }
                    }

                }
                if (node.Attributes != null) {
                    foreach (XmlAttribute attr in node.Attributes) {
                        if (attr.Prefix == "xmlns")
                            continue;
                        properties.Add("@" + prefix + attr.LocalName, ImportNode(attr));
                    }
                }
                return properties;
            }
            return null;
            ;


        }

        public static XmlElement FindGmlGeometry(XmlElement[] elements) {
            foreach (XmlElement element in elements) {
                XmlElement geom = FindGmlGeometry(element);
                if (geom != null)
                    return geom;
            }
            return null;
        }

        public static XmlElement FindGmlGeometry(XmlElement element) {
            // OGC 13-026
            if (element.NamespaceURI == "http://www.georss.org/georss") {
                // TODO implement a seperate class for GeoRSS Schema (schemas/georss/1.1/georss.rnc)
                if (element.LocalName == "where") {
                    if (element.ChildNodes[0] != null && element.ChildNodes[0].NodeType == XmlNodeType.Element)
                        return (XmlElement)element.ChildNodes[0];
                }
                throw new NotImplementedException();
            }
            return null;
        }

        public static XmlElement FindDctSpatialGeometry(XmlElement[] elements) {
            foreach (XmlNode node in elements) {
                XmlNamespaceManager xnsm = new XmlNamespaceManager(node.OwnerDocument.NameTable);
                xnsm.AddNamespace("dct", "http://purl.org/dc/terms/");
                if (node.NamespaceURI == "http://purl.org/dc/terms/" && node.LocalName == "spatial")
                    return (XmlElement)node;
                var spatial = (XmlElement)node.SelectSingleNode("dct:spatial", xnsm);
                if (spatial != null)
                    return spatial;
            }
            return null;
        }

        public static SyndicationLink FromDictionnary(Dictionary<string,object> link) {
            if (link.ContainsKey("@atom:href")) {
                long length = 0;
                string rel = null;
                string title = null;
                string type = null;
                Uri href = new Uri((string)link["@atom:href"]);
                object r = null;
                if (link.TryGetValue("@atom:rel", out r))
                    rel = (string)r;
                if (link.TryGetValue("@atom:title", out r))
                    title = (string)r;
                if (link.TryGetValue("@atom:type", out r))
                    type = (string)r;
                if (link.TryGetValue("@atom:length", out r))
                if (r is string)
                    long.TryParse((string)r, out length);
                else
                    length = (long)r;
                SyndicationLink slink = new SyndicationLink(href, rel, title, type, length);
                return slink;
            }
            if (link.ContainsKey("@href")) {
                long length = 0;
                string rel = null;
                string title = null;
                string type = null;
                Uri href = new Uri((string)link["@href"]);
                object r = null;
                if (link.TryGetValue("@rel", out r))
                    rel = (string)r;
                if (link.TryGetValue("@title", out r))
                    title = (string)r;
                if (link.TryGetValue("@type", out r))
                    type = (string)r;
                if (link.TryGetValue("@length", out r))
                    long.TryParse((string)r, out length);
                SyndicationLink slink = new SyndicationLink(href, rel, title, type, length);
                return slink;
            }
            throw new ArgumentException("Not a link");
        }
    }
}


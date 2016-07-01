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
using System.Xml.XPath;
using Newtonsoft.Json.Linq;
using Terradue.OpenSearch.Result;
using Terradue.GeoJson.Geometry;
using Terradue.GeoJson.GeoRss;
using Terradue.GeoJson.GeoRss10;

namespace Terradue.OpenSearch.GeoJson.Import
{
	public class ImportUtils
	{
		ImportOptions options;

		public ImportUtils(ImportOptions options)
		{
			this.options = options;
		}

		public Dictionary<string, object> SyndicationElementExtensions(SyndicationElementExtensionCollection elements, ref NameValueCollection namespaces)
		{
			string prefix = "";
			Dictionary<string, object> properties = new Dictionary<string, object>();

			XPathDocument x = new XPathDocument(elements.GetReaderAtElementExtensions());
			XPathNavigator nav = x.CreateNavigator();

			if (options.KeepNamespaces) {
				var allNodes = nav.SelectDescendants(XPathNodeType.All, true);
				while (allNodes.MoveNext()) {
					var names = allNodes.Current.GetNamespacesInScope(XmlNamespaceScope.Local);
					foreach (var n in names.Keys) {
						namespaces.Set(n, names[n]);
					}
				}
			}

			nav.MoveToRoot();
			var childnodes = nav.SelectChildren(XPathNodeType.Element);
			XPathNavigator prev = null;
			while (childnodes.MoveNext()) {
				var childnode = childnodes.Current;
				XmlNamespaceManager xnsm = new XmlNamespaceManager(childnode.NameTable);
				if (options.KeepNamespaces) {
					prefix = childnode.Prefix + ":";
				}
				try {
					properties.Add(prefix + childnode.LocalName, ImportNode(childnode.Clone()));
				} catch (ArgumentException) {
					if (properties.ContainsKey(prefix + childnode.LocalName) && properties[prefix + childnode.LocalName] is object[]) {
						object[] array = (object[])properties[prefix + childnode.LocalName];
						List<object> list = array.ToList();
						list.Add(ImportNode(childnode.Clone()));
						properties[prefix + childnode.LocalName] = list.ToArray();
					} else {
						List<object> list = new List<object>();
						list.Add(ImportNode(childnode.Clone()));
						properties[prefix + childnode.LocalName] = list.ToArray();
					}
				}
				prev = childnode.Clone();
			}

			return properties;

		}

		public SyndicationElementExtension[] PropertiesToSyndicationElementExtensions(Dictionary<string, object> properties)
		{

			XmlNamespaceManager xmlns = new XmlNamespaceManager(new NameTable());
			List<SyndicationElementExtension> elements = new List<SyndicationElementExtension>();
			NameValueCollection namespaces = new NameValueCollection();
			if (properties.ContainsKey("@namespaces")) {
				var temp = properties["@namespaces"];
				var namedic = (Dictionary<string, object>)temp;
				namedic.Keys.SingleOrDefault(k => {
					namespaces.Set(k, (string)namedic[k]);
					return false;
				});
			}

			foreach (var key in properties.Keys) {

				if (key.StartsWith("@atom") || key.StartsWith("atom"))
					continue;

				if (key == "@namespaces")
					continue;

				XDocument xdoc = new XDocument();
				var prop = ImportProperty(key, properties[key], namespaces);

				xdoc.Add(prop);
				XmlReader xreader = xdoc.CreateReader();
				xreader.Settings.IgnoreWhitespace = true;
				xmlns = new XmlNamespaceManager(xreader.NameTable);

				foreach (var prefix in namespaces.AllKeys) {
					if (string.IsNullOrEmpty(prefix))
						continue;
					xmlns.AddNamespace(prefix, namespaces[prefix]);
				}
				elements.Add(new SyndicationElementExtension(xreader));

			}

			return elements.ToArray();

		}

		XObject ImportProperty(string key, object obj, NameValueCollection namespaces)
		{
			string ns, localname;
			XObject xobject;

			ns = "";
			localname = key;
			if (key.StartsWith("@")) {
				localname = localname.Remove(0, 1);
			}

			if (localname.Contains(":")) {
				string prefix = localname.Split(':')[0];
				ns = namespaces[prefix] == null ? "" : namespaces[prefix];
				localname = key.Split(':')[1];
			}
			if (key.StartsWith("@")) {
				xobject = new XAttribute(XName.Get(localname, ns), obj);
				return xobject;
			}
			if (obj is Dictionary<string, object>) {
				Dictionary<string, object> prop = (Dictionary<string, object>)obj;
				List<object> objects = new List<object>();
				foreach (var key2 in prop.Keys) {
					XObject child = ImportProperty(key2, prop[key2], namespaces);
					if (child is XElement && ((XElement)child).Name == XName.Get(localname, ns) && ((XElement)child).FirstNode.NodeType == XmlNodeType.Text)
						objects.Add(((XElement)child).Value);
					else
						objects.Add(child);
				}
				xobject = new XElement(XName.Get(localname, ns), objects.ToArray());
			} else
				xobject = new XElement(XName.Get(localname, ns), obj);


			return xobject;
		}

		/*public Dictionary<string, object> SyndicationElementExtensions(XmlElement[] elements, ref NameValueCollection namespaces) {
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
        }*/

		public Dictionary<string, object> ImportAttributeExtensions(Dictionary<XmlQualifiedName, string> attributeExtensions, NameValueCollection namespaces, XmlNamespaceManager xnsm)
		{

			Dictionary<string, object> properties = new Dictionary<string, object>();
			string prefix = "";

			foreach (var key in attributeExtensions.Keys) {
				if (options.KeepNamespaces && !string.IsNullOrEmpty(xnsm.LookupPrefix(key.Namespace))) {
					prefix = xnsm.LookupPrefix(key.Namespace) + ":";
				}
				properties.Add("@" + prefix + key.Name, attributeExtensions[key]);
			}
			return properties;
		}

		/*public object ImportNode(XmlNode node) {
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


        }*/

		public object ImportNode(XPathNavigator nav)
		{
			string prefix = "";
			string text = null;

			var childnodes = nav.SelectChildren(XPathNodeType.All);
			string textNodeLocalName = nav.LocalName;

			Dictionary<string, object> properties = new Dictionary<string, object>();
			while (childnodes.MoveNext()) {

				textNodeLocalName = nav.LocalName;

				var childnode = childnodes.Current;

				if (childnode.Prefix == "xmlns")
					continue;

				if (options.KeepNamespaces && !string.IsNullOrEmpty(childnode.Prefix)) {
					prefix = childnode.Prefix + ":";
				}

				if (childnode.NodeType == XPathNodeType.Attribute) {
					properties.Add("@" + prefix + childnode.LocalName, childnode.Value);
					continue;
				}
				if (childnode.NodeType == XPathNodeType.Text) {
					text = childnode.Value;

					continue;
				}
				if (childnode.NodeType == XPathNodeType.Element) {
					try {
						var subprop = ImportNode(childnode.Clone());
						if (subprop is Dictionary<string, object> && ((Dictionary<string, object>)subprop).Count == 0) {
							continue;
						}
						if (properties.ContainsKey(prefix + childnode.LocalName)) {
							List<object> array = null;
							if (properties[prefix + childnode.LocalName] is List<object>) {
								array = (List<object>)properties[prefix + childnode.LocalName];
							} else {
								array = new List<object>();
								array.Add(properties[prefix + childnode.LocalName]);
								properties.Remove(prefix + childnode.LocalName);
								properties.Add(prefix + childnode.LocalName, array);
							}
							array.Add(subprop);

						} else {
							properties.Add(prefix + childnode.LocalName, subprop);
						}
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


			}
			if (nav.MoveToFirstAttribute()) {
				if ((options.KeepNamespaces || textNodeLocalName == "Query") && !string.IsNullOrEmpty(nav.Prefix)) {
					prefix = nav.Prefix + ":";
				}
				properties.Add("@" + prefix + nav.LocalName, nav.Value);
				while (nav.MoveToNextAttribute()) {
					if ((options.KeepNamespaces || textNodeLocalName == "Query") && !string.IsNullOrEmpty(nav.Prefix)) {
						prefix = nav.Prefix + ":";
					}
					try {
						properties.Add("@" + prefix + nav.LocalName, nav.Value);
					} catch (ArgumentException) {
					}
				}
			}

			if (string.IsNullOrEmpty(text))
				return properties;

			if (options.AsMixed == false && text != null && properties.Count == 0) {
				return text;
			}

			if (text != null) {
				if (options.KeepNamespaces && !string.IsNullOrEmpty(nav.Prefix)) {
					prefix = nav.Prefix + ":";
				}
				properties.Add(prefix + textNodeLocalName, text);
			}

			return properties;

		}

		public static GeometryObject FindGeometry(IOpenSearchResultItem item)
		{

			GeometryObject savegeom = null;

			if (item.ElementExtensions != null && item.ElementExtensions.Count > 0) {

				foreach (var ext in item.ElementExtensions) {

					XmlReader xr = ext.GetReader();

					switch (xr.NamespaceURI) {
					// 1) search for georss
					case "http://www.georss.org/georss":
						savegeom = Terradue.GeoJson.GeoRss.GeoRssHelper.Deserialize(xr).ToGeometry();
						if (xr.LocalName != "box" || xr.LocalName != "point") {
							return savegeom;
						}
						break;
					// 2) search for georss10
					case "http://www.georss.org/georss/10":
						savegeom = GeoRss10Extensions.ToGeometry(GeoRss10Helper.Deserialize(xr));
						if (xr.LocalName != "box" || xr.LocalName != "point") {
							return savegeom;
						}
						break;
					// 3) search for dct:spatial
					case "http://purl.org/dc/terms/":
						if (xr.LocalName == "spatial") {
							xr.Read();
							savegeom = WktExtensions.WktToGeometry(xr.Value);
							if (!(savegeom is Point)) {
								return savegeom;
							}
						}
						break;
					default:
						continue;
					}

				}

			}

			return savegeom;

		}


		public static SyndicationLink FromDictionnary(Dictionary<string, object> link, string prefix = "")
		{
			if (link.ContainsKey("@" + prefix + "href")) {
				long length = 0;
				string rel = null;
				string title = null;
				string type = null;
				Uri href = new Uri((string)link["@" + prefix + "href"]);
				object r = null;
				if (link.TryGetValue("@" + prefix + "rel", out r))
					rel = (string)r;
				if (link.TryGetValue("@" + prefix + "title", out r))
					title = (string)r;
				if (link.TryGetValue("@" + prefix + "type", out r))
					type = (string)r;
				if (link.TryGetValue("@" + prefix + "", out r))
					if (r is string)
						long.TryParse((string)r, out length);
					else
						length = (long)r;
				SyndicationLink slink = new SyndicationLink(href, rel, title, type, length);
				return slink;
			}
			throw new ArgumentException("Not a link");
		}

		public static SyndicationLink FromJTokenList(JToken link, string prefix = "")
		{
			if (link.SelectToken("@" + prefix + "href") != null) {
				long length = 0;
				string rel = null;
				string title = null;
				string type = null;
				Uri href = new Uri((string)link.SelectToken("@" + prefix + "href").ToString());
				object r = null;
				if (link.SelectToken("@" + prefix + "rel") != null)
					rel = link.SelectToken("@" + prefix + "rel").ToString();
				if (link.SelectToken("@" + prefix + "title") != null)
					title = link.SelectToken("@" + prefix + "title").ToString();
				if (link.SelectToken("@" + prefix + "type") != null)
					type = link.SelectToken("@" + prefix + "type").ToString();
				if (link.SelectToken("@" + prefix + "size") != null)
					long.TryParse(link.SelectToken("@" + prefix + "size").ToString(), out length);
				else
					length = 0;
				SyndicationLink slink = new SyndicationLink(href, rel, title, type, length);
				return slink;
			}
			throw new ArgumentException("Not a link");
		}
	}
}


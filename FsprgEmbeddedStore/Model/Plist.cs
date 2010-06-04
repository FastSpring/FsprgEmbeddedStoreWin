using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Collections;
using System.Globalization;

namespace FsprgEmbeddedStore.Model
{
    public class Plist : PlistDict
    {
        private const string PLIST = "plist";
        private const string DICT = "dict";
        private const string KEY = "key";
        private const string ARRAY = "array";
        private const string REAL = "real";
        private const string INTEGER = "integer";
        private const string TRUE = "true";
        private const string FALSE = "false";
        private const string DATE = "date";
        private const string STRING = "string";
        private const string DATA = "data";

        public XmlDocument OriginalDoc { internal set; get; }

        private Plist(XmlDocument originalDoc, Dictionary<string, object> dict) : base(dict) {
            OriginalDoc = originalDoc;
        }

        public static Plist Parse(string plistXml) {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(plistXml);

            return Parse(doc);
        }

        public static Plist Parse(XmlDocument plistDoc)
        {
            XmlElement plistE = plistDoc.DocumentElement;

            if (!plistE.LocalName.Equals(PLIST))
            {
                throw new Exception("Expected plist top element, was: " + plistE.LocalName);
            }

            // Assure that the top element is a dict and the single child element.
            if (plistE.ChildNodes.Count != 1 || !plistE.FirstChild.Name.Equals(DICT))
            {
                throw new Exception("Expected single 'dict' child element.");
            }
            XmlElement dictE = (XmlElement)plistE.FirstChild;

            return new Plist(plistDoc, ParseDict(dictE));
        }

        private static Dictionary<string, object> ParseDict(XmlElement dictE) {
            Dictionary<string, object> dict = new Dictionary<string, object>(dictE.ChildNodes.Count);

            IEnumerator childNodes = dictE.ChildNodes.GetEnumerator();
            while(childNodes.MoveNext()) {
                XmlElement keyE = (XmlElement)childNodes.Current;
                if (!keyE.Name.Equals(KEY)) {
                    throw new Exception("Expected key element but was " + keyE.Name);
                }
                if(childNodes.MoveNext()) {
                    XmlElement valueE = (XmlElement)childNodes.Current;
                    dict.Add(keyE.InnerText, ParseElement(valueE));
                }
            }

            return dict;
        }

        private static object[] ParseArray(XmlElement listE) {
            object[] list = new object[listE.ChildNodes.Count];

            int i = 0;
            foreach (XmlNode childNode in listE.ChildNodes)
            {
                list[i] = ParseElement((XmlElement)childNode);
                i++;
            }

            return list;
        }

        private static object ParseElement(XmlElement element)
        {
            string type = element.Name;
            switch (type)
            {
                case INTEGER:
                    return long.Parse(element.InnerText);
                case REAL:
                    return decimal.Parse(element.InnerText);
                case STRING:
                    return element.InnerText;
                case DATE:
                    return DateTime.ParseExact(element.InnerText, "yyyy-MM-dd'T'HH:mm:ss'Z'", new CultureInfo("en-US"));
                case DATA:
                    return Convert.FromBase64String(element.InnerText);
                case ARRAY:
                    return ParseArray(element);
                case TRUE:
                    return true;
                case FALSE:
                    return false;
                case DICT:
                    return new PlistDict(ParseDict(element));
                default:
                    throw new Exception("Unexpected type: " + type);
            }
        }
    }

}

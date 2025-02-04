﻿using System;
using System.Security;
using System.ComponentModel;
using System.Xml;

namespace CompareTranslatorXml
{
    class Processor
    {
        XmlDocument doc;
        XmlElement node;
        string path;

        public Processor(string path)
        {
            this.path = path;
            doc = new XmlDocument();
            doc.Load(path);
            node = doc.DocumentElement;
        }

        public void GetListStringData(string prop, ref SortedBindingList<StringObject> strings)
        {
            XmlNodeList li = node.SelectNodes("strings");
            foreach (XmlNode item in li)
            {
                foreach (XmlNode stringNode in item.SelectNodes("string"))
                {
                    int index = strings.Count;
                    StringObject stringObject = new StringObject(
                        index,
                        stringNode.Attributes["id"].InnerText
                    );

                    string originProp = "Origin" + prop;
                    string text = stringNode.Attributes["text"].InnerText.ToString();
                    stringObject.SetData(prop, new MixedValue(text));
                    stringObject.SetData(originProp, new MixedValue(text));
                    AddOrUpdate(prop, ref strings, stringObject);
                }
            }
        }

        public void AddOrUpdate(string prop, ref SortedBindingList<StringObject> strings, StringObject stringObject)
        {
            foreach (StringObject stringItem in strings)
            {
                if (stringItem.Id == stringObject.Id)
                {
                    // SecurityElement.Escape(stringNode.Attributes["text"].InnerText)
                    // No escape value
                    stringItem.SetData(prop, new MixedValue(stringObject.GetType().GetProperty(prop).GetValue(stringObject, null)));
                    return;
                }
            }

            strings.Add(stringObject);
        }

        public void AddOrUpdate(StringObject stringObj)
        {
            XmlNode stringsNode = node.SelectSingleNode("strings");
            XmlNode existStringNode = stringsNode.SelectSingleNode(string.Format("string[@id='{0}']", stringObj.Id));
            if (stringObj.En == null || stringObj.En.Equals(""))
            {
                if (existStringNode != null)
                {
                    stringsNode.RemoveChild(existStringNode);
                    doc.Save(this.path);
                }
                return;
            }
            // XmlNode existStringNode = stringsNode.SelectSingleNode(string.Format("string[@id='{0}']", stringObj.Id));
            string textVal = stringObj.Vn;

            if (textVal == null || textVal.Equals(""))
            {
                textVal = stringObj.En;
            }
            if (existStringNode != null)
            {
                existStringNode.Attributes["text"].InnerText = textVal;
            } else
            {
                XmlElement newStringObjectNode = doc.CreateElement("string");
                XmlAttribute newIdAttribute = doc.CreateAttribute("id");
                XmlAttribute newTextAttribute = doc.CreateAttribute("text");
                newIdAttribute.InnerText = stringObj.Id;
                newTextAttribute.InnerText = textVal;
                newStringObjectNode.SetAttributeNode(newIdAttribute);
                newStringObjectNode.SetAttributeNode(newTextAttribute);
                stringsNode.AppendChild(newStringObjectNode);
                doc.DocumentElement.AppendChild(stringsNode);
            }

            doc.Save(this.path);
        }
    }
}

﻿using System;
using System.Linq;
using System.Xml;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Schema;

/// <summary>
/// This class should be used by a GUI, which will pass in certain information retrieved by a XML dataset,
/// which the user will specify and give some small information about it.
/// </summary>
public class XmlLoader {

    /// <summary>
    /// Should be a robust system in reading from an XML File.
    /// </summary>
    /// <param name="fileName">The name of the dataset to pass in. DBLP is used as default.</param>
    /// <param name="parentXmlnode">The XmlNode which is used as the main target to look at. "article" is used as default.</param>
    public void ReadFile(string fileName, string parentXmlAttribute, string[] childrenXmlAttributes)
    {
        //TODO: Need to account for errors in reading in urls. An error page should say this:
        /*<root response="False">
            <error>Movie not found!</error>
          </root>
         */

        /*There is only one URL, title or year, thus we only need to load in data using an 
		IEnumerable<string>. However, there may exist one or more authors. Thus, we need to
		use an IEnumerable<IEnumerable<string>>*/
        IEnumerable<IEnumerable<string>> authors = (from el in SimpleStreamAxis(fileName, parentXmlAttribute)
                                                    select (from author in el.Elements(childrenXmlAttributes[0])
                                                            select author.Value));
        IEnumerable<string> conferences = from el in SimpleStreamAxis(fileName, parentXmlAttribute)
                                   select (string)el.Element(childrenXmlAttributes[1]);
        IEnumerable<string> titles = from el in SimpleStreamAxis(fileName, parentXmlAttribute)
                                     select (string)el.Element(childrenXmlAttributes[2]);
        IEnumerable<string> years = from el in SimpleStreamAxis(fileName, parentXmlAttribute)
                                    select (string)el.Element(childrenXmlAttributes[3]);

		XmlFileTrace.SetListsFromEnumerables(authors, conferences, titles, years);
    }

    private IEnumerable<XElement> SimpleStreamAxis(string fileName, string elementName)
    {
        XmlReaderSettings settings = new XmlReaderSettings();
        settings.ValidationType = ValidationType.DTD;
        settings.ProhibitDtd = false;
        settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBackHandler);
        settings.IgnoreWhitespace = true;
        using (XmlReader reader = XmlReader.Create(fileName, settings))
        {
            reader.MoveToContent();

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        while (reader.Name == elementName)
                        {
                            XElement el = XElement.ReadFrom(reader) as XElement;
                            if (el != null)
                                yield return el;
                        }
                        break;
                }
            }
            reader.Close();
        }
    }

    private static void ValidationCallBackHandler(object sender, ValidationEventArgs e)
    {
        if (e.Severity == XmlSeverityType.Warning)
            throw new XmlSchemaException("Could not find schema definition.");
        else throw new XmlSchemaValidationException("Xml Schema Validation error: " + e.Message);
    }

    internal bool DidntReachEmptyPage(string movieURLWithPageIndex)
    {
        XmlDocument urlDoc = new XmlDocument();
        urlDoc.Load(movieURLWithPageIndex);

        XmlElement root = urlDoc.DocumentElement;
        XmlNodeList nodes = root.SelectNodes("error");

        foreach (XmlNode node in nodes)
        {
            string errorMessage = node.InnerText;
            if (errorMessage == "Movie not found!")
            {
                return false;
            }
        }

        return true;
    }
}

﻿
using System;
using System.Xml;
using System.Collections.Generic;

public class Dialog
{
    private Page m_currentPage;
    private Dictionary<string, Page> m_pageDict;

    // Constructor with XML Node as parameter
    public Dialog(XmlNode dialogNode)
    {
        m_pageDict = new Dictionary<string, Page>();
        
        // Note: If Page has no options it's a quit page and returns its ID as exit code
        foreach (XmlNode page in dialogNode)
        {
            string id = page.Attributes["id"].Value;
            Page p = new Page(page.SelectSingleNode("premise").InnerText);
            foreach(XmlNode opt in page.SelectNodes("option"))
            {
                p.AddOption(opt.InnerText, opt.Attributes["pointer"].Value);
            }
            
            m_pageDict.Add(id, p);

        }
        m_currentPage = m_pageDict["root"];
    }

    // Returns the pointer for the choice made
    public string MakeChoice(int choice)
    {
        string exitCode = "";
        exitCode = m_currentPage.GetPointer(choice);
        return exitCode;
    }

    // Sets the next page as the one with the specified code
    public void NextPage(string code)
    {
        m_currentPage = m_pageDict[code];
    }

    // Getter for current page
    public Page GetCurrentPage()
    {
        return m_currentPage;
    }
    
    // Reset the dialog
    public void Reset()
    {
        m_currentPage = m_pageDict["root"];
    }

}

using System;
using System.Collections;
using System.Collections.Generic;

// Handles page's components: premise, options and their pointers/exitcodes
public class Page
{
    private string m_premise;
    private List<string> m_options;
    private List<string> m_choicePointers;
    
    public Page(string premise) : this()
    {
        m_premise = premise;
    }

    public Page()
    {
        m_options = new List<string>();
        m_choicePointers = new List<string>();
    }

    public void AddOption(string text, string idPointer)
    {
        m_options.Add(text);
        m_choicePointers.Add(idPointer);
    }


    public string GetPremise() { return m_premise; }
    public void SetPremise(string premise) { m_premise = premise; }

    public int GetOptionCount()
    {
        return m_options.Count;
    }

    public string GetOption(int i)
    {
        return m_options[i];
    }

    public string GetPointer(int i)
    {
        return m_choicePointers[i];
    }
}

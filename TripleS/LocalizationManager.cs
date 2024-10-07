using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace TripleS {

    /// <summary>
    /// Manages localization files for dialouge and UI.
    /// </summary>
    public static class LocalizationManager 
    {
        public static Languages CurrentLanguage { get; private set; }
        public static Dictionary<string, string> Dialogues { get; private set; }
        public static Dictionary<string, string> UINames { get; private set; }
        private static bool first = false;

        public static void Initialize(Languages newLang)
        {
            if (newLang != CurrentLanguage || !first)
            {
                first = true;
                CurrentLanguage = newLang;
                Dialogues = new Dictionary<string, string>();
                UINames = new Dictionary<string, string>();
                var name = EnumToString(CurrentLanguage);
                string path = $"{SSS.Game.Content.RootDirectory}/loc/{name}.xml";

                string contents = "";
                using (FileStream fs = File.OpenRead(path))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            contents += line;
                        }
                    }
                }

                if (contents != "")
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(contents);

                    var sections = doc.SelectNodes("loc/dialogue/line");
                    if (sections.Count > 0)
                    {
                        foreach (XmlNode section in sections)
                            Dialogues.Add(section.Attributes["id"].Value, section.InnerXml);
                    }
                    sections = doc.SelectNodes("loc/ui/line");
                    if (sections.Count > 0)
                    {
                        foreach (XmlNode section in sections)
                        {
                            string trueString = section.InnerXml.Replace(@"\n", Environment.NewLine);
                            UINames.Add(section.Attributes["id"].Value, trueString);
                        }
                    }
                }
            }
        }

        private static string EnumToString(Languages lang)
        {
            switch (lang)
            {
                case Languages.English:
                    return "eng";
                case Languages.French:
                    return "frn";
                case Languages.Spanish:
                    return "spn";
                case Languages.Japanese:
                    return "jpn";
                case Languages.Portugese:
                    return "prt";
                case Languages.German:
                    return "grm";
                case Languages.Russian:
                    return "rus";
                case Languages.Dutch:
                    return "dut";
                case Languages.Italian:
                    return "ita";
                case Languages.Swedish:
                    return "swe";
                case Languages.Norwegian:
                    return "nor";
                case Languages.Dansk:
                    return "dan";
                case Languages.Finnish:
                    return "fin";
                case Languages.Icelandic:
                    return "ice";
            }
            return "eng";
        }

        public static string GetVariableString(string line, bool ui, object[] variables)
        {
            line = ui ? UINames[line] : Dialogues[line];
            int stringVars = line.Count(x => x == '{');
            if(stringVars > 0 && variables.Length >= stringVars)
            {
                for(int i = 0; i < stringVars; i++)
                {
                    line = line.Replace(@$"{{{i}}}", $"{variables[i]}");
                }
            }
            return line;
        }

        public static string GetVariableString(string line, bool ui, object variable)
        {
            line = ui ? UINames[line] : Dialogues[line];
            if (line.Contains('{'))
                line = line.Replace("{0}", $"{variable}");
            return line;
        }
    }

    public enum Languages
    {
        English,
        French,
        Spanish,
        Japanese,
        Portugese,
        German,
        Russian,
        Dutch,
        Italian,
        Swedish,
        Norwegian,
        Dansk,
        Finnish,
        Icelandic
    }
}
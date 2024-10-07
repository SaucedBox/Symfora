using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using Microsoft.Xna.Framework;

namespace TripleS {

    /// <summary>
    /// Manages param system.
    /// </summary>
    public static class ParamLoader {

        public static Game Game;
        private static bool loaded;
        public static List<ParamMarker> Markers { get; private set; }
        
        public static void Initialize(Game game)
        {
            Game = game;
            Parse($"{game.Content.RootDirectory}/param.xml");
        }

        private static void Parse(string path)
        {
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
                Markers = new List<ParamMarker>();
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(contents);

                var sections = doc.SelectNodes("params/parSection");
                if (sections.Count > 0)
                {
                    foreach (XmlNode section in sections)
                    {
                        string sectionName = section.Attributes["name"].Value;
                        Markers.Add(new ParamMarker(sectionName, 0, "", "", PropType.None, MarkerType.Section));

                        var entries = section.SelectNodes("entry");
                        if (entries.Count > 0)
                        {
                            foreach (XmlNode entry in entries)
                            {
                                int entryId = int.Parse(entry.Attributes["id"].Value);
                                Markers.Add(new ParamMarker(sectionName, entryId, "", "", PropType.None, MarkerType.Entry));

                                var props = entry.SelectNodes("prop");
                                if (props.Count > 0)
                                {
                                    foreach (XmlNode prop in props)
                                    {
                                        string propName = prop.Attributes["name"].Value;
                                        string typeName = prop.Attributes["type"].Value + "t";
                                        PropType type = (PropType)Enum.Parse(typeof(PropType), typeName);

                                        string contString = prop.InnerText;
                                        Markers.Add(new ParamMarker(sectionName, entryId, propName, contString, type, MarkerType.Property));
                                    }
                                }
                            }
                        }
                    }
                }
                loaded = true;
            }
        }

        public static ParamMarker GetPropMarker(string section, int entry, string property, bool noEx = false)
        {
            if (loaded)
            {
                foreach (ParamMarker marker in Markers)
                {
                    if (marker.ParamType == MarkerType.Property && marker.Section == section && marker.EntryID == entry && marker.Property == property)
                        return marker;
                }
                if (!noEx)
                    throw new ParamExecption($"Param does not exist at ({section}, {entry}, {property}).");
                else
                    return default;
            }
            else
                throw new ParamExecption("Params not yet loaded.");
        }

        public static bool ParamExists(string section, int entry, string property)
        {
            if (loaded)
            {
                foreach (ParamMarker marker in Markers)
                {
                    if (marker.ParamType == MarkerType.Property && marker.Section == section && marker.EntryID == entry && marker.Property == property)
                        return true;
                }
                return false;
            }
            else
                throw new ParamExecption("Params not yet loaded.");
        }

        public static T GetParam<T>(string section, int entry, string property, bool noEx = false)
        {
            if (loaded)
            { 
                var marker = GetPropMarker(section, entry, property, noEx);
                if (marker.Equals(default(ParamMarker)))
                {
                    return default(T);
                }
                return (T)ConvertParamType(marker);
            }
            else
                throw new ParamExecption("Params not yet loaded.");
        }

        public static ParamMarker[] GetMarkers(MarkerType type, string section = "", int entry = 0)
        {
            if (loaded)
            {
                List<ParamMarker> ms = new List<ParamMarker>();

                switch (type)
                {
                    case MarkerType.Section:
                        foreach (ParamMarker m in Markers)
                        {
                            if (m.ParamType == MarkerType.Section)
                                ms.Add(m);
                        }
                        break;
                    case MarkerType.Entry:
                        foreach (ParamMarker m in Markers)
                        {
                            if (m.ParamType == MarkerType.Entry && m.Section == section)
                                ms.Add(m);
                        }
                        break;
                    case MarkerType.Property:
                        foreach (ParamMarker m in Markers)
                        {
                            if (m.ParamType == MarkerType.Property && m.Section == section && m.EntryID == entry)
                                ms.Add(m);
                        }
                        break;
                }

                return ms.ToArray();
            }
            else
                throw new ParamExecption("Params not yet loaded.");
        }

        public static object ConvertParamType(ParamMarker marker)
        {
            object content = marker.Contents;
            switch (marker.Type)
            {
                case PropType.stringt:
                    content = marker.Contents;
                    break;
                case PropType.intt:
                    content = int.Parse(marker.Contents);
                    break;
                case PropType.floatt:
                    content = float.Parse(marker.Contents);
                    break;
                case PropType.boolt:
                    content = bool.Parse(marker.Contents);
                    break;
            }

            return content;
        }

        public static Dictionary<string, object> BatchProps(string section, int entry)
        {
            if (loaded)
            {
                var objs = new Dictionary<string, object>();
                foreach(ParamMarker marker in GetMarkers(MarkerType.Property, section, entry))
                {
                    objs.Add(marker.Property, marker.Contents);
                }
                return objs;
            }
            else
                throw new ParamExecption("Params not yet loaded.");
        }
    }
}

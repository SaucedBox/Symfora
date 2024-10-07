using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks.Sources;

namespace TripleS {

    /// <summary>
    /// Manager for saves states.
    /// </summary>
    public static class GameStateManager {

        public static int maxSaves;
        /// <summary>
        /// Which save slot is currently in use.
        /// </summary>
        public static int CurrentSave { get; set; }
        public static Dictionary<int, float> States { get; set; }

        /// <summary>
        /// Loads save.
        /// </summary>
        public static void ReadSave()
        {
            if (CurrentSave < maxSaves)
            {
                string path = GetSavePath(CurrentSave, out string directPath);

                if (File.Exists(path))
                {
                    string save = ReadBytes(path);
                    if (save != "")
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(save);

                        var nodes = doc.SelectNodes("save/state");
                        foreach (XmlNode node in nodes)
                        {
                            var id = int.Parse(node.Attributes["id"].Value);
                            var contents = float.Parse(node.InnerText);
                            States[id] = contents;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Saves current states.
        /// </summary>
        public static void WriteSave()
        {
            if (CurrentSave < maxSaves)
            {
                //ReadSave();
                string path = GetSavePath(CurrentSave, out string directPath);

                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<save>\n</save>");
                XmlElement root = doc.DocumentElement;

                int i = -1;
                foreach (KeyValuePair<int, float> state in States)
                {
                    i++;
                    var el = doc.CreateElement("state");
                    el.SetAttribute("id", state.Key.ToString());
                    el.InnerText = state.Value.ToString();
                    root.AppendChild(el);
                }

                WriteBytes(doc.OuterXml, path, directPath);
            }
        }

        /// <summary>
        /// Deletes save.
        /// </summary>
        public static void WipeSave()
        {
            string path = GetSavePath(CurrentSave, out string directPath);
            File.Delete(path);
            States = new Dictionary<int, float>();
        }

        public static bool SaveBeenWiped()
        {
            string path = GetSavePath(CurrentSave, out string directPath);
            return File.Exists(path);
        }

        private static void WriteBytes(string contents, string path, string directory)
        {
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(directory);
            }
            using (FileStream fs = File.Create(path))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(contents);
                fs.Write(bytes, 0, bytes.Length);
            }
        }

        private static string ReadBytes(string path)
        {
            string contents = "";
            using (FileStream fs = File.OpenRead(path))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    contents = sr.ReadToEnd();
                }
            }
            return contents;
        }

        private static string GetSavePath(int slot, out string directory)
        {
            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            directory = $"{docPath}/Delirium Games/{SSS.GameName}";
            return $"{docPath}/Delirium Games/{SSS.GameName}/save{CurrentSave}.tss";
        }

        public static void SetState(int id, int value)
        {
            if(States != null && CurrentSave < maxSaves)
            {
                if (States.ContainsKey(id))
                    States[id] = value;
                else
                    States.Add(id, value);
            }
        }
    }
}

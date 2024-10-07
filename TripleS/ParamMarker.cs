using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripleS {

    /// <summary>
    /// Data for parameters.
    /// </summary>
    public struct ParamMarker {

        public string Contents { get; private set; }
        public string Section { get; private set; }
        public int EntryID { get; private set; }
        public string Property { get; private set; }
        public PropType Type { get; private set; }
        public MarkerType ParamType { get; private set; }

        public ParamMarker(string section, int entry, string prop, string contents, PropType type, MarkerType pType)
        {
            Contents = contents;
            Section = section;
            EntryID = entry;
            Property = prop;
            Type = type;
            ParamType = pType;
        }
    }

    public enum PropType
    {
        intt,
        floatt,
        boolt,
        stringt,
        None
    }

    public enum MarkerType
    {
        Section,
        Entry,
        Property
    }
}

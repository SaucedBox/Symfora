using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripleS {
    public interface ITripleSGame {

        public static Renderer _Renderer { get; }
        public static LevelHandler _LevelHandler { get; set; }
        void InitOrganize() { }
    }
}

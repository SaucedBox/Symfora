using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TripleS {
    public struct Polygon {

        public Vector2[] Points { get; set; }
        public bool Closed { get; set;  }
    }
}

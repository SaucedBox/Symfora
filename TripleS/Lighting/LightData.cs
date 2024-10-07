using Microsoft.Xna.Framework;

namespace TripleS.Lighting {

    /// <summary>
    /// Data for a point light.
    /// </summary>
    public struct LightData {

        public Vector3 Color { get; set; }
        public Vector2 Position { get; set; }
        public float Brightness { get; set; }
        public float Radius { get; set; }
        public bool Visible { get; set; }
        public bool Shadows { get; set; }
        public float Angle { get; set; }
        public float Falloff { get; set; }
        public float DirRadius { get; set; }
        public bool Directional { get; set; }
        public bool Area { get; set; }
        public Vector2[,] Tris { get; set; }
        public Vector2 AreaBounds { get; set; }

        public LightData(Vector2 pos, float brightness, float radius, Vector3 color, bool visible, float falloff, bool area, bool dir, bool shad, float angle = 0, float dirrad = 0, Vector2 bounds = new Vector2())
        {
            Visible = visible;
            Color = color;
            AreaBounds = bounds;
            Position = pos;
            Brightness = brightness;
            Radius = radius;
            Directional = dir;
            Angle = angle;
            DirRadius = dirrad;
            Tris = null;
            Shadows = shad;
            Falloff = falloff;
            Area = area;
        }
    }

    public struct ShadowRay {
        public Vector2 Position { get; set; }
        public Vector2 Direction { get; set; }
        public float Distance { get; set; }
        public float TwiceFirstDir { get; set; }
        public Vector2 End
        {
            get { return Position + (Direction * Distance); }
            private set { }
        }
        public Vector2 TFPos
        {
            get { return TwiceFirstDir == -1 ? End : Position + (Direction * TwiceFirstDir); }
            private set { }
        }
        public bool CalTop { get; set; }
    }
}

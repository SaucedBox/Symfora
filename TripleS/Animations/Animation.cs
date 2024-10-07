using Microsoft.Xna.Framework.Graphics;

namespace TripleS.Animations {

    /// <summary>
    /// Data for animations.
    /// </summary>
    public struct Animation {

        public AnimationType Type { get; }
        public Texture2D[] Frames { get; }
        public int FrameWidth { get; }
        public int FrameHeight { get; }
        public float Speed { get; }
        public bool Loop { get; }
        public string ID { get; }
        public int Length { get; }

        public Animation(string n, AnimationType t, Texture2D[] f, int le, int w, int h, float s, bool l)
        {
            ID = n;
            Type = t;
            Frames = f;
            FrameWidth = w;
            FrameHeight = h;
            Speed = s;
            Loop = l;
            if (t == AnimationType.Images)
                Length = f.Length;
            else
                Length = le;
        }

        public Animation(string n, AnimationType t, Texture2D f, int le, int w, int h, float s, bool l)
        {
            ID = n;
            Type = t;
            Frames = new Texture2D[1] { f };
            FrameWidth = w;
            FrameHeight = h;
            Speed = s;
            Loop = l;
            if (t == AnimationType.Images)
                Length = 1;
            else
                Length = le;
        }
    }

    public enum AnimationType
    {
        Strip,
        Images
    }
}

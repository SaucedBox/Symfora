using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TripleS.Animations {
    public class SingletonAnimator {

        public int Frame { get; set; }
        public int MaxFrames { get; private set; }
        public float Speed { get; private set; }
        public int FrameHeight { get; private set; }

        public Texture2D image;
        private float interTime;

        public SingletonAnimator(int maxF, float speed, int frameDim, Texture2D img)
        {
            MaxFrames = maxF;
            Frame = MaxFrames;
            Speed = speed;
            FrameHeight = frameDim;
            image = img;
        }

        public Rectangle Update(out Texture2D img)
        {
            if (Frame > 0)
            {
                interTime += Speed * SSS.Delta;
                int next = (int)MathF.Ceiling(interTime);
                if (interTime >= next - (Speed * SSS.Delta))
                    Frame--;
            }

            img = image;
            return new Rectangle(0, FrameHeight * (Frame - 1), image.Width, FrameHeight);
        }

        //a:image in assets, max frames, speed, frame height
        public static SingletonAnimator QuickParse(string text)
        {
            if (text[0] == 'a' && text[1] == ':')
            {
                text = text.Substring(2);
                string[] details = text.Split(", ");

                int maxF = int.Parse(details[1]), frameH = int.Parse(details[3]);
                float speed = float.Parse(details[2]);
                Texture2D gangGang = SSS.Game.Content.Load<Texture2D>("assets/" + details[0]);

                return new SingletonAnimator(maxF, speed, frameH, gangGang);
            }
            return null;
        }
    }
}

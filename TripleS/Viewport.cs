using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Threading.Tasks.Sources;

namespace TripleS {

    /// <summary>
    /// Game camera.
    /// </summary>
    public class GameView {

        public Matrix MasterMatrix { get; private set; }
        public Rectangle ViewRect { get; private set; }
        public Vector2 Position { get; private set; }
        public float Zoom { get; set; }
        public Vector2 TargetPosition { get; set; }
        public float Speed { get; set; }
        public bool Lerp { get; set; }
        public Vector2 Centre
        {
            get
            {
                return new Vector2(Position.X + (ViewRect.Width / 2), Position.Y + (ViewRect.Height / 2));
            }
            private set { }
        }

        private Vector2 cameraPos;
        public int viewPortWidth;
        public int viewPortHeight;
        public int windowWidth;
        public int windowHeight;

        public GameView()
        {
            Zoom = 1;
            Speed = 0.25f;
            cameraPos = new Vector2();
            windowWidth = SSS.Game.GraphicsDevice.Viewport.Width;
            windowHeight = SSS.Game.GraphicsDevice.Viewport.Height;
            Lerp = true;
        }

        public void Update(GraphicsDevice device)
        {
            windowWidth = device.Viewport.Width;
            windowHeight = device.Viewport.Height;
            if (Lerp)
            {
                cameraPos.X = MathHelper.Lerp(cameraPos.X, -TargetPosition.X + (viewPortWidth / 2), Speed);
                cameraPos.Y = MathHelper.Lerp(cameraPos.Y, -TargetPosition.Y + (viewPortHeight / 2), Speed);
            }
            else
            {
                cameraPos.X = -TargetPosition.X + (viewPortWidth / 2);
                cameraPos.Y = -TargetPosition.Y + (viewPortHeight / 2);
            }

            MasterMatrix = Matrix.CreateTranslation((int)cameraPos.X, (int)cameraPos.Y, 0);

            viewPortHeight = (int)(windowHeight / Zoom);
            viewPortWidth = (int)(windowWidth / Zoom);
            Position = new Vector2((int)-cameraPos.X, (int)-cameraPos.Y);
            ViewRect = new Rectangle((int)Position.X, (int)Position.Y, (int)(viewPortWidth), (int)(viewPortHeight));
        }
    }
}

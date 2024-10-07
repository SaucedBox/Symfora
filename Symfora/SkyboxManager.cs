using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TripleS;

//0 - top left, 1 - middle, 2 - top middle
namespace Symfora {
    public class SkyboxManager {

        public int SkyType { get; private set; }
        public int AnchorPoint { get; private set; }

        private Texture2D background;
        private List<SkyboxElement> elements;
        private Color darkness;

        public void LoadSkybox(int sky)
        {
            SkyType = sky;
            AnchorPoint = ParamLoader.GetParam<int>("skybox", SkyType, "anchor");

            var bgn = ParamLoader.GetParam<string>("skybox", SkyType, "base");
            background = bgn != "" ? SSS.Game.Content.Load<Texture2D>("assets/skybox/" + bgn) : SSS.Square;

            var col = MathUtil.HexToRGB(ParamLoader.GetParam<string>("skybox", SkyType, "ambient"));
            Core._Renderer.Lighting.AmbientColor = new Color(col.X, col.Y, col.Z);

            var preEles = ParamLoader.GetMarkers(MarkerType.Property, "skybox", SkyType);
            preEles = preEles.Where(x => x.Property.Contains("element")).ToArray();
            if(preEles.Length > 0)
            {
                elements = new List<SkyboxElement>(preEles.Length);
                foreach (ParamMarker marker in preEles)
                {
                    elements.Add(ParseElement(marker.Contents));
                }
                elements = elements.OrderBy(x => x.ParalaxLayer).ToList();
            }

            var rpWnd = ParamLoader.GetParam<string>("skybox", SkyType, "ropeWind").Split(',');
            RopeEngine.EnableWind = bool.Parse(rpWnd[0]);
            RopeEngine.WindSpeed = int.Parse(rpWnd[1]);
            RopeEngine.WindAmp = int.Parse(rpWnd[2]);

            float dam = Math.Clamp(ParamLoader.GetParam<float>("skybox", SkyType, "darkness"), 0.0f, 1.0f);
            darkness = new Color(dam, dam, dam);
        }

        public void Draw(Renderer renderer)
        {
            if (LevelHandler.CurrentLevel != 1)
            {
                Vector2 pos = Vector2.Zero;
                if (AnchorPoint == 1)
                    pos = new Vector2(background.Width / 4, background.Height / 4);
                else if (AnchorPoint == 2)
                    pos = new Vector2(background.Width / 4, 0);

                renderer.BasicDraw(SSS.Square, renderer.View.ViewRect, 1, 1, col: renderer.Lighting.AmbientColor);

                if ((background.Width + background.Height) / 2 > 1)
                    renderer.BasicDraw(background, Vector2.Zero, 1, 1, col: darkness, source: new Rectangle(
                        -(int)renderer.View.Position.X + (int)pos.X, 
                        -(int)renderer.View.Position.Y + (int)pos.Y, 
                        Math.Clamp(background.Width + (int)renderer.View.Position.X, 0, LevelHandler.MapBounds.Width * 16),
                        Math.Clamp(background.Height + (int)renderer.View.Position.Y, 0, LevelHandler.MapBounds.Height * 16)));

                if (elements != null)
                {
                    Vector2 levelCentre = LevelHandler.MapBounds.Location.ToVector2() + new Vector2(LevelHandler.MapBounds.Width / 2, LevelHandler.MapBounds.Height / 2);
                    foreach (SkyboxElement element in elements)
                    {
                        Vector2 target = renderer.View.TargetPosition;
                        pos = new Vector2(target.X - ((target.X - levelCentre.X) / element.ParalaxLayer) + element.Position.X, target.Y - ((target.Y - levelCentre.Y) / element.ParalaxLayer) + element.Position.Y);

                        renderer.BasicDraw(element.Image, pos, 1, 1);
                    }
                }
            }
        }

        //fileName, paralax, x, y
        private SkyboxElement ParseElement(string value)
        {
            var seperands = value.Split(", ", 4);

            int x = int.Parse(seperands[2]);
            int y = int.Parse(seperands[3]);
            Vector2 pos = new Vector2(x, y);
            Texture2D tex = SSS.Game.Content.Load<Texture2D>("assets/skybox/" + seperands[0]);
            int paralax = int.Parse(seperands[1]);

            return new SkyboxElement { Image = tex, ParalaxLayer = paralax, Position = pos };
        }
    }

    internal struct SkyboxElement
    {
        public Texture2D Image { get; set; }
        public Vector2 Position { get; set; }
        public int ParalaxLayer { get; set; }
    }
}

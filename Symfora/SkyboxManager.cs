using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.ComponentModel;
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
        private Texture2D abyss;
        private SkyboxElement[] elements;
        private Color darkness;
        private bool noStretch;
        private bool abyssParalax;
        private List<Rectangle> bounds;

        public void LoadSkybox(int sky)
        {
            SkyType = sky;
            AnchorPoint = ParamLoader.GetParam<int>("skybox", SkyType, "anchor");
            noStretch = ParamLoader.GetParam<bool>("skybox", SkyType, "noStretch");

            var bgn = ParamLoader.GetParam<string>("skybox", SkyType, "base");
            background = bgn != "" ? SSS.Game.Content.Load<Texture2D>("assets/skybox/" + bgn) : SSS.Square;
            var aby = ParamLoader.GetParam<string>("skybox", SkyType, "abyss");
            abyss = aby != "" ? SSS.Game.Content.Load<Texture2D>("assets/skybox/" + aby) : null;
            abyssParalax = abyss != null && aby == "skyfade7";

            var col = MathUtil.HexToRGB(ParamLoader.GetParam<string>("skybox", SkyType, "ambient"));
            Core._Renderer.Lighting.AmbientColor = new Color(col.X, col.Y, col.Z);

            var preEles = ParamLoader.GetMarkers(MarkerType.Property, "skybox", SkyType);
            preEles = preEles.Where(x => x.Property.Contains("bounds")).ToArray();
            if (preEles.Length > 0)
            {
                bounds = new List<Rectangle>(preEles.Length);
                foreach (ParamMarker marker in preEles)
                    bounds.Add(ParseBounds(marker.Contents));
            }

            preEles = ParamLoader.GetMarkers(MarkerType.Property, "skybox", SkyType);
            preEles = preEles.Where(x => x.Property.Contains("element")).ToArray();
            if(preEles.Length > 0)
            {
                var tElements = new List<SkyboxElement>(preEles.Length);
                foreach (ParamMarker marker in preEles)
                    tElements.Add(ParseElement(marker.Contents, marker.Property));
                elements = tElements.OrderBy(x => x.ParalaxLayer).ToArray();
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
                renderer.BasicDraw(SSS.Square, renderer.View.ViewRect, 1, 1, col: renderer.Lighting.AmbientColor);

                var pos = Vector2.Clamp(renderer.View.Position, Vector2.Zero, new Vector2((LevelHandler.MapBounds.Width * 16) - renderer.View.viewPortWidth, (LevelHandler.MapBounds.Height * 16) - renderer.View.viewPortHeight));
                if ((background.Width + background.Height) / 2 > 1)
                {
                    var rect = noStretch ? background.Bounds : new Rectangle(0, 0, renderer.View.viewPortWidth, renderer.View.viewPortHeight);
                    renderer.BasicDraw(background, pos, 1, 1, col: darkness, source: rect);
                }
                if (abyss != null)
                {
                    float paralax = abyssParalax ? (renderer.View.Position.Y / 32) - 50 : 0;
                    renderer.BasicDraw(abyss, pos + new Vector2(0, renderer.View.viewPortHeight - abyss.Height - paralax), 1, 1, col: darkness, source: new Rectangle(0, 0, renderer.View.viewPortWidth, abyss.Height));
                }

                if (elements != null)
                {
                    foreach (SkyboxElement element in elements)
                    {
                        Vector2 target = renderer.View.Position;
                        var pos1 = new Vector2(((target.X - element.Position.X) / element.ParalaxLayer) + element.Position.X, ((target.Y - element.Position.Y) / element.ParalaxLayer) + element.Position.Y);
                        if(element.BoundsIndex != -1)
                        {
                            var tb = bounds[element.BoundsIndex];
                            int nx = tb.X - (int)pos1.X;
                            int ny = tb.Y - (int)pos1.Y;
                            renderer.BasicDraw(element.Image, bounds[element.BoundsIndex].Location.ToVector2(), 1, 1, source: new Rectangle(nx, ny, tb.Width, tb.Height));
                        }
                        else
                            renderer.BasicDraw(element.Image, pos1, 1, 1);
                    }
                }
            }
        }

        //fileName, paralax, x, y
        private SkyboxElement ParseElement(string value, string name)
        {
            var seperands = value.Split(",", 4);

            int x = int.Parse(seperands[2]);
            int y = int.Parse(seperands[3]);
            if (!int.TryParse(name[^1..], out int bi))
                bi = -1;
            Vector2 pos = new Vector2(x, y);
            Texture2D tex = SSS.Game.Content.Load<Texture2D>("assets/skybox/" + seperands[0]);
            float paralax = float.Parse(seperands[1]);

            return new SkyboxElement { Image = tex, ParalaxLayer = paralax, Position = pos, BoundsIndex = bi };
        }

        private Rectangle ParseBounds(string value)
        {
            var seperands = value.Split(",", 4);
            int[] v = new int[4];
            for(int i = 0; i < 4; i++)
                v[i] = int.Parse(seperands[i]);
            return new Rectangle(v[0], v[1], v[2], v[3]);
        }

        public void DebugElement(string imageName, int newX, int newY, float paralax)
        {
            imageName = "assets/skybox/" + imageName;
            int ret = -1;
            for(int i = 0; i < elements.Length; i++)
            {
                if (elements[i].Image.Name == imageName)
                    ret = i;
            }
            if (ret != -1)
            {
                elements[ret].Position = new Vector2(newX, newY);
                elements[ret].ParalaxLayer = paralax == -1f ? elements[ret].ParalaxLayer : paralax;
            }
        }
    }

    internal struct SkyboxElement
    {
        public Texture2D Image { get; set; }
        public Vector2 Position { get; set; }
        public float ParalaxLayer { get; set; }
        public int BoundsIndex { get; set; }
    }
}

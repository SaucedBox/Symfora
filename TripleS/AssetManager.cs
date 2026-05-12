using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TripleS.Animations;

namespace TripleS {
    public static class AssetManager {

        public static Dictionary<string, Texture2D> Assets { get; private set; }
        public static Dictionary<string, Rectangle> CanvasCuts { get; private set; }
        public static Dictionary<string, Effect> Effects { get; private set; }

        private static Renderer rendererInst;

        public static void Load(Renderer r)
        {
            rendererInst = r;
            Assets = new Dictionary<string, Texture2D>();
            CanvasCuts = new Dictionary<string, Rectangle>();

            int length = ParamLoader.GetMarkers(MarkerType.Entry, "cuts").Length;

            for (int i = 0; i < length; i++)
            {
                var rect = ParseCanvasCut(ParamLoader.GetParam<string>("cuts", i, "rect"));
                CanvasCuts.Add(ParamLoader.GetParam<string>("cuts", i, "name"), rect);
            }
        }

        public static void UnloadAll()
        {
            SSS.Game.Content.Unload();
            Assets.Clear();
            Effects.Clear();
        }

        public static Texture2D GetAsset(string path)
        {
            if (path.Contains('_'))
            {
                if (!Assets.ContainsKey(path))
                {
                    string canvasName = path.Remove(path.IndexOf('_'));
                    Texture2D canvas = SSS.Game.Content.Load<Texture2D>("assets/" + canvasName);
                    if (canvas != null)
                    {
                        Assets.Add(path, CropImage(canvas, CanvasCuts[path]));
                    }
                }
                return Assets[path];
            }
            else
            {
                throw new Exception("Asset path is not valid or does not contain the name of the canvas.");
            }
        }

        public static Effect LoadEffect(string name)
        {
            Effects ??= new Dictionary<string, Effect>();
            var asset = SSS.Game.Content.Load<Effect>("effects/" + name);
            if (asset != null)
            {
                Effects.Add(name, asset);
                return asset;
            }
            return null;
        }

        public static Effect LoadPostEffect(string name, bool addByDefault)
        {
            var asset = SSS.Game.Content.Load<Effect>("effects/" + name);
            if (asset != null && rendererInst.PostEffects != null)
            {
                name = "pp_" + name;
                Effects.Add(name, asset);
                if(addByDefault)
                    rendererInst.PostEffects.Add(name, asset);
                return asset;
            }
            return null;
        }

        public static void TogglePostEffect(bool state, string name)
        {
            name = "pp_" + name;
            if (Effects.ContainsKey(name))
            {
                bool contains = rendererInst.PostEffects.ContainsKey(name);
                if (!state && contains)
                {
                    rendererInst.PostEffects.Remove(name);
                }
                else if(state && !contains)
                {
                    rendererInst.PostEffects.Add(name, Effects[name]);
                }
            }
        }

        public static Animation GetParamAnim(int id)
        {
            Texture2D frames = SSS.Game.Content.Load<Texture2D>(ParamLoader.GetParam<string>("anim", id, "texture"));
            AnimationType type = (AnimationType)Enum.Parse(typeof(AnimationType), ParamLoader.GetParam<string>("anim", id, "type"));

            return new Animation(
                ParamLoader.GetParam<string>("anim", id, "animName"),
                type,
                frames,
                ParamLoader.GetParam<int>("anim", id, "length"),
                ParamLoader.GetParam<int>("anim", id, "width"),
                ParamLoader.GetParam<int>("anim", id, "height"),
                ParamLoader.GetParam<float>("anim", id, "speed"),
                ParamLoader.GetParam<bool>("anim", id, "loops")
            );
        }

        private static Rectangle ParseCanvasCut(string info)
        {
            var indi = info.Split(',');
            return new Rectangle(int.Parse(indi[0]), int.Parse(indi[1]), int.Parse(indi[2]), int.Parse(indi[3]));
        }

        public static Texture2D CropImage(Texture2D texture, Rectangle crop)
        {
            Rectangle nbounds = texture.Bounds;
            nbounds.X += crop.X;
            nbounds.Y += crop.Y;
            nbounds.Width = crop.Width;
            nbounds.Height = crop.Height;

            Texture2D croped = new Texture2D(SSS.Game.GraphicsDevice, nbounds.Width, nbounds.Height);
            Color[] data = new Color[nbounds.Width * nbounds.Height];
            texture.GetData(0, nbounds, data, 0, nbounds.Width * nbounds.Height);
            croped.SetData(data);

            return croped;
        }

        public static Texture2D GetDynamicAsset(string path)
        {
            if (Assets.TryGetValue(path, out Texture2D value)) {            
                return value;
            }
            else {
                var img = SSS.Game.Content.Load<Texture2D>(path);
                Assets.Add(path, img);
                return img;
            }
        }
    }
}
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TripleS {

    /// <summary>
    /// Basic methods and setup for game.
    /// </summary>
    public static class SSS {

        public static Game Game { get; private set; }
        public static string GameName { get; private set; }
        public static Texture2D Square { get; private set; }
        public static bool PreviewMode { get; private set; }
        public static string PreviewMapFile { get; private set; }
        public static float Delta { get; set; }
        public static bool FirstLoad { get; private set; }

        public static void Init(Game game, string name, int maxSaveSlots, string[] args)
        {
            Game = game;
            GameStateManager.maxSaves = maxSaveSlots;
            GameName = name;
            GameStateManager.States = new Dictionary<int, float>();
            GameStateManager.ReadSave();
            if (!GameStateManager.States.ContainsKey(0))
            {
                FirstLoad = true;
                GameStateManager.States[0] = 1;
            }

            ParamLoader.Initialize(game);

            Texture2D tex = new Texture2D(Game.GraphicsDevice, 1, 1);
            tex.SetData(new Color[] { new Color(1, 1, 1, 1f) });
            Square = tex;

            if (args.Length > 0)
            {
                PreviewMode = args[0].Trim() != "" && args[0] != "" && System.IO.File.Exists(args[0]);
                if (PreviewMode)
                {
                    var last = args[0].Substring(args[0].LastIndexOf('/') + 1);
                    PreviewMapFile = last.Remove(last.IndexOf('.'));
                    Debug.Log("Loading as dev preview: " + PreviewMapFile, LogType.Info);
                }
            }         
        }

        public static void ClearSave()
        {
            GameStateManager.WipeSave();
            GameStateManager.States = new Dictionary<int, float>();
            GameStateManager.ReadSave();
            if (!GameStateManager.States.ContainsKey(0))
            {
                FirstLoad = true;
                GameStateManager.States[0] = 1;
            }
        }

        /// <summary>
        /// Basic, default draw info.
        /// </summary>
        public static DrawInfo DefaultInfo() 
        {
            return new DrawInfo(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
        }
    }
}

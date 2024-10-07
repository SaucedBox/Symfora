using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace TripleS {

    /// <summary>
    /// Game component with added methods.
    /// </summary>
    public class MinimalComponent : IGameComponent {

        public Game Game { get; }

        public MinimalComponent(Game game) {
            Game = game;
        }
        public virtual void Initialize() { }
        public virtual void Update(GameTime gameTime) { }
        public virtual void Draw(Renderer renderer) { }
        public virtual void Load(ContentManager content) { }
    }
}

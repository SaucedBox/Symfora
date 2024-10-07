using Microsoft.Xna.Framework.Graphics;

namespace TripleS {

    /// <summary>
    /// Basic Monogame info for drawing.
    /// </summary>
    public struct DrawInfo {

        public SpriteSortMode SortMode { get; }
        public BlendState Blend { get; }
        public SamplerState Sampler { get; }
        public DepthStencilState StencilState { get; }
        public RasterizerState Rasterizer { get; }

        public DrawInfo(SpriteSortMode ssm, BlendState bs, SamplerState samp, DepthStencilState dst, RasterizerState rs)
        {
            SortMode = ssm;
            Blend = bs;
            Sampler = samp;
            StencilState = dst;
            Rasterizer = rs;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TripleS.Lighting;
using TripleS.Physics;

namespace TripleS {

    /// <summary>
    /// A drawing helper class that supports the lighting system.
    /// </summary>
    public class Renderer {

        public Effect CurrentEffect { get; protected set; }
        public SpriteBatch Batch { get; private set; }
        public DrawInfo Info { get; private set; }
        public GameView View { get; private set; }
        public LightingEngine Lighting { get; protected set; }
        public bool EnableLigthing { get; set; }
        public bool EnableBGEffects { get; set; }
        public Rectangle? ScreenSource { get; set; }
        private bool ShadowMapsDone { get; set; }
        private Dictionary<int, RenderTarget2D> ShadowMaps { get; set; }
        public Dictionary<string, Effect> PostEffects { get; private set; }

        public Vector2 monitorResolution;
        public Vector2 gameResolution;

        public RenderTarget2D normalsTarget;
        public RenderTarget2D screenTarget1;
        public RenderTarget2D screenTarget2;
        public RenderTarget2D[] postTargets;

        private VertexBuffer primVertexBuffer;
        private BasicEffect primBasicEffect;
        private Matrix primWorld;
        private Matrix primView = Matrix.CreateLookAt(new Vector3(0, 0, 3), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
        private Matrix primProjection;
        bool preShad = false;

        public Renderer(SpriteBatch spriteBatch, DrawInfo info, GameView port, int ml)
        {
            Batch = spriteBatch;
            Info = info;
            View = port;
            Lighting = new LightingEngine(ml);
        }

        public void BasicDraw(Texture2D texture, Vector2 position, int layer, float subLayer, float size = 1, float rot = 0, SpriteEffects mirror = SpriteEffects.None, Color? col = null, Rectangle? source = null, Vector2? rotSource = null, Effect effect = null)
        {
            if (!col.HasValue)
                col = Color.White;
            if (!rotSource.HasValue)
                rotSource = Vector2.Zero;

            if (CurrentEffect != effect)
            {
                CurrentEffect = effect;

                Batch.End();
                Batch.Begin(Info.SortMode, Info.Blend, Info.Sampler, Info.StencilState, Info.Rasterizer, CurrentEffect, View.MasterMatrix);
            }

            var dims = new Vector2(texture.Width, texture.Height);
            if (source != null)
                dims = new Vector2(source.Value.X, source.Value.Y);

            SetEffectParams(dims, col.Value.ToVector4());

            Batch.Draw(texture, position, source, col.Value, rot, rotSource.Value, size, mirror, GetProperLayer(layer, subLayer));
        }

        public void BasicDraw(Texture2D texture, Rectangle position, int layer, float subLayer, float rot = 0, SpriteEffects mirror = SpriteEffects.None, Color? col = null, Rectangle? source = null, Vector2? rotSource = null, Effect effect = null)
        {
            if (!col.HasValue)
                col = Color.White;
            if (!rotSource.HasValue)
                rotSource = Vector2.Zero;

            float width = (float)position.Width / (float)texture.Width;
            float height = (float)position.Height / (float)texture.Height;

            if (CurrentEffect != effect)
            {
                CurrentEffect = effect;
                Batch.End();
                Batch.Begin(Info.SortMode, Info.Blend, Info.Sampler, Info.StencilState, Info.Rasterizer, CurrentEffect, View.MasterMatrix);
            }

            var dims = new Vector2(position.Width, position.Height);
            if (source != null)
                dims = new Vector2(source.Value.X, source.Value.Y);

            SetEffectParams(dims, col.Value.ToVector4());
            Batch.Draw(texture, position.Location.ToVector2(), source, col.Value, 0, rotSource.Value, new Vector2(width, height), mirror, GetProperLayer(layer, subLayer));
        }

        public void DrawText(SpriteFont font, string text, Vector2 position, int layer, float subLayer, float size = 1, SpriteEffects mirror = SpriteEffects.None, Color? col = null, Effect effect = null)
        {
            if (!col.HasValue)
                col = Color.White;

            if (CurrentEffect != effect)
            {
                CurrentEffect = effect;
                Batch.End();
                Batch.Begin(Info.SortMode, Info.Blend, Info.Sampler, Info.StencilState, Info.Rasterizer, CurrentEffect, View.MasterMatrix);
            }

            SetEffectParams(font.MeasureString(text), col.Value.ToVector4());
            Batch.DrawString(font, text, position, col.Value, 0, Vector2.Zero, size, mirror, GetProperLayer(layer, subLayer));
        }

        /// <summary>
        /// Initates the very first phase of drawing. Should be called before Phase 1. Do not draw between this phase and phase 1.
        /// </summary>
        public void NormalsPhase(LevelHandler levelHandler)
        {
            primWorld = Matrix.CreateTranslation((-SSS.Game.GraphicsDevice.Viewport.Width / 2), (SSS.Game.GraphicsDevice.Viewport.Height / 2), -SSS.Game.GraphicsDevice.Viewport.Width);
            primBasicEffect.World = primWorld;
            primBasicEffect.View = primView;
            primBasicEffect.Projection = primProjection;
            primBasicEffect.VertexColorEnabled = true;

            Batch.GraphicsDevice.Clear(Color.White);
            if (!ShadowMapsDone)
            {
                if (preShad)
                {
                    ShadowMaps = new Dictionary<int, RenderTarget2D>(LightingEngine.Lights.Count);
                    foreach (KeyValuePair<UUID, LightData> ldp in LightingEngine.Lights.Where(x => x.Value.Shadows))
                    {
                        var l = ldp.Value;
                        ShadowMaps[ldp.Key.MainUUID] = CreateStandardRT2D();
                        Batch.GraphicsDevice.SetRenderTarget(ShadowMaps[ldp.Key.MainUUID]);
                        Batch.GraphicsDevice.Clear(Color.Transparent);
                        CurrentEffect = null;
                        Batch.Begin(Info.SortMode, Info.Blend, Info.Sampler, Info.StencilState, Info.Rasterizer, null, View.MasterMatrix);
                        for (int i = 0; i < l.Tris.GetLongLength(0); i++)
                        {
                            DrawTriangle(l.Tris[i, 0], l.Tris[i, 1], l.Tris[i, 2], Color.White);
                        }
                        End();
                        Batch.GraphicsDevice.SetRenderTarget(null);
                    }
                    ShadowMapsDone = true;
                }
                preShad = true;
            }

            Batch.GraphicsDevice.SetRenderTarget(normalsTarget);
            Batch.GraphicsDevice.Clear(Color.Transparent);
            CurrentEffect = null;
            Batch.Begin(Info.SortMode, Info.Blend, Info.Sampler, Info.StencilState, Info.Rasterizer, null, View.MasterMatrix);

            levelHandler.DrawLightmaps(this);

            End();
        }

        /// <summary>
        /// Initiates first phase of drawing.
        /// </summary>
        public void PhaseOne()
        {
            Batch.GraphicsDevice.SetRenderTarget(screenTarget1);
            Batch.GraphicsDevice.Clear(Color.Transparent);

            CurrentEffect = null;
            Batch.Begin(Info.SortMode, Info.Blend, SamplerState.PointClamp, Info.StencilState, Info.Rasterizer, null, View.MasterMatrix);
        }

        /// <summary>
        /// Initiates second phase of drawing.
        /// </summary>
        public void PhaseTwo()
        {
            End();

            Batch.GraphicsDevice.SetRenderTarget(screenTarget2);
            Batch.GraphicsDevice.Clear(Color.Transparent);

            CurrentEffect = null;
            Batch.Begin(Info.SortMode, Info.Blend, Info.Sampler, Info.StencilState, Info.Rasterizer, null, View.MasterMatrix);
        }

        /// <summary>
        /// Initiates third phase of drawing.
        /// </summary>

        float testTimer;
        public void PhaseThree(float time)
        {
            /*if (Lighting.TestRays != null)
            {
                foreach (ShadowRay ray in Lighting.TestRays)
                    Debug.DrawLine(this, ray.Position, ray.End, Color.Cyan);
                testTimer += SSS.Delta;
                if (testTimer >= Lighting.TestTris.GetLongLength(0))
                    testTimer = 0;
                int sel = (int)MathF.Floor(testTimer);
                Debug.DrawLine(this, Lighting.TestTris[sel, 0], Lighting.TestTris[sel, 1], Color.Red);
                Debug.DrawLine(this, Lighting.TestTris[sel, 1], Lighting.TestTris[sel, 2], Color.Red);
                Debug.DrawLine(this, Lighting.TestTris[sel, 2], Lighting.TestTris[sel, 0], Color.Red);
            }*/
            End();
            StartPost();

            SSS.Game.GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp; //DUDE THIS ONE LINE OF CODE TOOK ME A WEEK TO CREATE WHAT THE FUCK
            SSS.Game.GraphicsDevice.SamplerStates[2] = SamplerState.PointClamp;

            Effect gf = EnableBGEffects ? Lighting.BackgroundEffect : null;
            Batch.Begin(Info.SortMode, Info.Blend, Info.Sampler, Info.StencilState, Info.Rasterizer, gf);
            Batch.Draw(screenTarget1, new Rectangle(0, 0, (int)(gameResolution.X * View.Zoom), (int)(gameResolution.Y * View.Zoom)), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
            End();

            if (EnableLigthing)
            {
                SetLightingParams(time);
                Effect lf = Lighting.LightingEffect;
                Effect alf = Lighting.AreaLightingEffect;

                lf.Parameters["geoMap"]?.SetValue(normalsTarget);
                alf.Parameters["geoMap"]?.SetValue(normalsTarget);
                 
                int i = -1;
                foreach (KeyValuePair<UUID, LightData> ldp in LightingEngine.Lights)
                {
                    i++;
                    var light = ldp.Value;
                    //float md = light.Area ? (light.AreaBounds.X + light.AreaBounds.Y) / 2 : 100 * (1 + light.Falloff);
                    //if (Vector2.Distance(light.Area ? light.Position + (light.AreaBounds / 2) : light.Position, View.TargetPosition) > md)
                        //continue;

                    var cleffect = light.Area ? alf : lf;
                    if (ShadowMapsDone && light.Shadows)
                        Lighting.LightingEffect.Parameters["shadowMap"]?.SetValue(ShadowMaps[ldp.Key.MainUUID]);
                    else
                        Lighting.LightingEffect.Parameters["shadowMap"]?.SetValue(SSS.Square);
                    cleffect.Parameters["lightPos"]?.SetValue(light.Position);
                    cleffect.Parameters["radius"]?.SetValue(light.Radius);
                    cleffect.Parameters["lightCol"]?.SetValue(light.Color);
                    cleffect.Parameters["brightness"]?.SetValue(light.Brightness);
                    cleffect.Parameters["falloffDist"]?.SetValue(light.Falloff);
                    cleffect.Parameters["visible"]?.SetValue(light.Visible ? 1 : 0);
                    cleffect.Parameters["shadow"]?.SetValue(light.Shadows ? 1 : 0);
                    cleffect.Parameters["angle"]?.SetValue(light.Angle);
                    cleffect.Parameters["dirRadius"]?.SetValue(light.DirRadius);
                    cleffect.Parameters["directional"]?.SetValue(light.Directional ? 1 : 0);
                    cleffect.Parameters["areaDim"]?.SetValue(light.AreaBounds);
                    cleffect.Parameters["ambientLight"]?.SetValue(i == 0 ? Lighting.AmbientColor.ToVector3() : Vector3.Zero);

                    Batch.Begin(Info.SortMode, i != 0 ? BlendState.Additive : BlendState.AlphaBlend, Info.Sampler, Info.StencilState, Info.Rasterizer, cleffect);
                    Batch.Draw(screenTarget2, new Rectangle(0, 0, (int)(gameResolution.X * View.Zoom), (int)(gameResolution.Y * View.Zoom)), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                    End();
                }
            }
            else
            {
                Batch.Begin(Info.SortMode, Info.Blend, Info.Sampler, Info.StencilState, Info.Rasterizer);
                Batch.Draw(screenTarget2, new Rectangle(0, 0, (int)(gameResolution.X * View.Zoom), (int)(gameResolution.Y * View.Zoom)), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                End();
            }

            EndPost(time);

            CurrentEffect = null;
            Begin(CurrentEffect);
        }

        /// <summary>
        /// Initiates last phase of drawing (ends everything).
        /// </summary>
        public void PhaseFour()
        {
            End();
            Batch.GraphicsDevice.SetRenderTarget(null);
        }

        public void Begin(Effect effect)
        {
            Batch.Begin(Info.SortMode, Info.Blend, Info.Sampler, Info.StencilState, Info.Rasterizer, effect, View.MasterMatrix);
        }

        public void End()
        {
            Batch.End();
        }

        /// <summary>
        /// Updates graphics device changes.
        /// </summary>
        public void UpdateGraphics()
        {
            monitorResolution = new Vector2();
            monitorResolution.X = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            monitorResolution.Y = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            gameResolution = new Vector2();
            if (View.windowHeight != 0 && View.windowWidth != 0)
            {
                gameResolution.X = View.windowWidth;
                gameResolution.Y = View.windowHeight;
            }
            else
            {
                gameResolution.X = Batch.GraphicsDevice.Viewport.Width;
                gameResolution.Y = Batch.GraphicsDevice.Viewport.Height;
            }

            normalsTarget = CreateStandardRT2D();
            screenTarget1 = CreateStandardRT2D();
            screenTarget2 = CreateStandardRT2D();
            //postTargets = Enumerable.Repeat(CreateStandardRT2D(), 2).ToArray();
            postTargets = new RenderTarget2D[2] { CreateStandardRT2D(), CreateStandardRT2D() };
        }

        public void Load()
        {
            primProjection = Matrix.CreateOrthographic(SSS.Game.GraphicsDevice.Viewport.Width, SSS.Game.GraphicsDevice.Viewport.Height, 0.01f, 5000);
            primVertexBuffer = new VertexBuffer(SSS.Game.GraphicsDevice, typeof(VertexPositionColor), 3, BufferUsage.WriteOnly);
            primBasicEffect = new BasicEffect(SSS.Game.GraphicsDevice);
            PostEffects = new Dictionary<string, Effect>();

            UpdateGraphics();
        }

        public void SetEffectParams(Vector2 dims, Vector4 color)
        {
            if (CurrentEffect != null)
            {
                CurrentEffect.Parameters["sourceSize"]?.SetValue(dims);
                CurrentEffect.Parameters["sourceColor"]?.SetValue(color);
            }
        }

        public void SetLightingParams(float time)
        {
            //Lighting.GetParams();
            Lighting.LightingEffect.Parameters["resolution"]?.SetValue(gameResolution);
            Lighting.LightingEffect.Parameters["cameraLocation"]?.SetValue(View.Position);
            Lighting.LightingEffect.Parameters["gameTime"]?.SetValue(time);
            Lighting.LightingEffect.Parameters["zoom"]?.SetValue(View.Zoom);
            Lighting.AreaLightingEffect.Parameters["resolution"]?.SetValue(gameResolution);
            Lighting.AreaLightingEffect.Parameters["cameraLocation"]?.SetValue(View.Position);
            Lighting.AreaLightingEffect.Parameters["gameTime"]?.SetValue(time);
            Lighting.AreaLightingEffect.Parameters["zoom"]?.SetValue(View.Zoom);
            //Lighting.SetParams();
        }

        public void ResetLightingBlocks()
        {
            ShadowMapsDone = false;
            preShad = false;
        }

        /// <summary>
        /// Gets draw layer.
        /// </summary>
        /// <returns>Draw layer</returns>
        public float GetProperLayer(int main, float sub)
        {
            var rSub = Math.Clamp(sub, 0, 1);
            var rMain = Math.Clamp(main, 0, 10);
            var real = (rMain / 10) + (rSub / 100);
            return Math.Clamp(real, 0, 1);
        }

        public void DrawTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Color color)
        {
            VertexPositionColor[] vertices = new VertexPositionColor[3];
            vertices[0] = new VertexPositionColor(new Vector3(p1.X, -p1.Y, 0), color);
            vertices[1] = new VertexPositionColor(new Vector3(p2.X, -p2.Y, 0), color);
            vertices[2] = new VertexPositionColor(new Vector3(p3.X, -p3.Y, 0), color);
            primVertexBuffer.SetData(vertices);
            SSS.Game.GraphicsDevice.SetVertexBuffer(primVertexBuffer);

            foreach (EffectPass pass in primBasicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                SSS.Game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
            }
        }

        public RenderTarget2D CreateStandardRT2D()
        {
            return new RenderTarget2D(Batch.GraphicsDevice, Batch.GraphicsDevice.Viewport.Width, Batch.GraphicsDevice.Viewport.Height, false, Batch.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
        }

        private void StartPost()
        {
            if (PostEffects != null && PostEffects.Count > 0)
            {
                Batch.GraphicsDevice.SetRenderTarget(postTargets[0]);
            }
        }

        private void EndPost(float time)
        {
            if (PostEffects != null && PostEffects.Count > 0)
            {
                int i = -1;
                foreach(string key in PostEffects.Keys)
                {
                    i++;
                    int tid = i % 2 == 0 ? 0 : 1;

                    if (i != PostEffects.Count - 1)
                        Batch.GraphicsDevice.SetRenderTarget(postTargets[tid == 1 ? 0 : 1]);
                    else
                        Batch.GraphicsDevice.SetRenderTarget(null);
                    Batch.GraphicsDevice.Clear(Color.Transparent);

                    PostEffects[key].Parameters["resolution"]?.SetValue(gameResolution);
                    PostEffects[key].Parameters["cameraLocation"]?.SetValue(View.Position);
                    PostEffects[key].Parameters["gameTime"]?.SetValue(time);
                    PostEffects[key].Parameters["zoom"]?.SetValue(View.Zoom);

                    Batch.Begin(Info.SortMode, Info.Blend, Info.Sampler, Info.StencilState, Info.Rasterizer, PostEffects[key]);
                    Batch.Draw(postTargets[tid], new Rectangle(0, 0, (int)gameResolution.X, (int)gameResolution.Y), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                    End();
                }
                Batch.GraphicsDevice.SetRenderTarget(null);
            }
        }
    }
}

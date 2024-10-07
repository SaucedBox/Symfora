using System;
using System.Collections.Generic;
using TripleS;
using TripleS.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Symfora {
    internal static class RopeEngine {

        public static List<RopeSegment[]> Ropes { get; set; }
        public static List<Texture2D[]> RopeTextures { get; set; }
        public static bool EnableWind { get; set; }
        public static float WindSpeed { get; set; } //2
        public static float WindAmp { get; set; } //24

        static float windTimer = 0f;

        public static int AddRope(int segCount, float length, float width, bool imageStretch, Vector2 pos, Texture2D[] textures)
        {
            Ropes ??= new List<RopeSegment[]>();
            RopeTextures ??= new List<Texture2D[]>();

            var rope = new RopeSegment[10];
            for (int i = 0; i < segCount; i++)
            {
                rope[i] = new RopeSegment(length, width, new Vector2(pos.X, pos.Y + (length * i)), i - 1, imageStretch);
            }

            Ropes.Add(rope);
            RopeTextures.Add(textures);
            return Ropes.Count;
        }

        public static void Update()
        {
            if (Ropes != null)
            {
                if (EnableWind)
                {
                    windTimer += SSS.Delta * WindSpeed;
                    if (windTimer > MathF.PI * 2)
                        windTimer = 0;
                }
                int windFlag = EnableWind ? 1 : 0;
                var playerBounds = Core._Player.Transform.GetRectangle();

                for (int ri = 0; ri < Ropes.Count; ri++)
                {
                    float wind = 0f;
                    if (EnableWind)
                        wind = (MathF.Sin(windTimer + ri) * 0.5f + 0.5f) / WindAmp + 0.05f;

                    var rope = Ropes[ri];
                    for (int i = 0; i < rope.Length; i++)
                    {
                        if (CollisionEngine.PointInRect(playerBounds, rope[i].Position) && MathF.Abs(Core._Player.Transform.Velocity.X) > 0.05f)
                            VelFlexRope(ri, -Core._Player.Transform.Velocity.X / 2);

                        if (rope[i].Velocity != 0f)
                            rope[i].Velocity = MathHelper.Lerp(rope[i].Velocity, MathUtil.IntSign(-rope[i].Rotation) * 6, 0.035f);
                        if (MathF.Abs(rope[i].Velocity) <= 0.15f && rope[i].Velocity != 0)
                            rope[i].UngulateTimer += SSS.Delta;
                        if(rope[i].UngulateTimer > 1f)
                        {
                            rope[i].UngulateTimer = 0;
                            rope[i].Velocity = 0;
                            rope[i].Rotation = 0;
                        }

                        rope[i].Rotation += MathHelper.ToRadians(rope[i].Velocity);

                        float trueWindAmp = WindAmp / 120 * windFlag;
                        float realRot = rope[i].Rotation + (wind * (i + 1)) + trueWindAmp;
                        if (rope[i].Parent != -1)
                        {
                            rope[i].Position = Transform.RotateAroundAPoint(rope[rope[i].Parent].Position, MathHelper.ToDegrees(rope[rope[i].Parent].GlobalRotation + MathF.PI), rope[rope[i].Parent].Length);
                            realRot += rope[rope[i].Parent].Rotation;
                            rope[i].GlobalRotation = realRot;
                        }
                        else
                            rope[i].GlobalRotation = rope[i].Rotation + trueWindAmp + (0.05f * windFlag);
                        realRot += MathF.PI / 2;
                        rope[i].OldVelocity = rope[i].Velocity;

                        rope[i].AppliedRotation = realRot;
                    }
                }
            }
        }

        public static void Draw(Renderer renderer)
        {
            if (Ropes != null)
            {
                for (int ri = 0; ri < Ropes.Count; ri++)
                {
                    var rope = Ropes[ri];
                    for (int i = 0; i < rope.Length; i++)
                    {
                        Texture2D texture = RopeTextures[ri][0]; //start end texture
                        if (RopeTextures[ri].Length > 1)
                        {
                            if (i == rope.Length - 1)
                                texture = RopeTextures[ri][RopeTextures[i].Length - 1]; //end end texture
                            else if (RopeTextures[ri].Length > 2 && i != 0)
                            {
                                int mid = RopeTextures[ri].Length - 2;
                                mid = (rope.Length - 2) / mid;
                                mid = (int)MathF.Floor((i - 1) / mid) * mid;
                                texture = RopeTextures[ri][mid]; //middle textures
                            }
                        }

                        if (rope[i].Stretch)
                            renderer.Batch.Draw(texture, rope[i].Position, null, Color.White, rope[i].AppliedRotation, Vector2.Zero, new Vector2(rope[i].Length, rope[i].Width), SpriteEffects.None, 0);
                        else
                            renderer.Batch.Draw(texture, rope[i].Position, null, Color.White, rope[i].AppliedRotation, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
                    }
                }
            }
        }

        public static void VelFlexRope(int ropeID, float vel)
        {
            for (int i = 0; i < Ropes[ropeID].Length; i++)
            {
                float adder = MathUtil.IntSign(vel) * i / 2f;
                Ropes[ropeID][i].Velocity = vel + adder;
                Ropes[ropeID][i].UngulateTimer = 0;
            }
        }
    }

    internal struct RopeSegment {
        public float Velocity { get; set; }
        public float OldVelocity { get; set; }
        public float Rotation { get; set; }
        public float AppliedRotation { get; set; }
        public float GlobalRotation { get; set; }
        public float Length { get; set; }
        public Vector2 Position { get; set; }
        public int Parent { get; set; }
        public bool Stretch { get; set; }
        public float Width { get; set; }
        public float UngulateTimer { get; set; }

        public RopeSegment(float len, float width, Vector2 pos, int parent, bool imageStretch)
        {
            Velocity = 0;
            Rotation = 0;
            Length = len;
            Position = pos;
            Parent = parent;
            GlobalRotation = 0;
            OldVelocity = 0;
            AppliedRotation = 0;
            Stretch = imageStretch;
            Width = width;
            UngulateTimer = 0;
        }
    }
}

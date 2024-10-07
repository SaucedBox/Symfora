using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TripleS;
using TripleS.Physics;
using TripleS.Animations;
using System.Reflection.Metadata;

namespace Symfora {
    public static class ParticleManager {

        public static List<Particle> Particles { get; set; }

        public static void Load()
        {
            Particles = new List<Particle>();
        }

        public static void Update()
        {
            for(int i = 0; i < Particles.Count; i++)
            {
                Particle part = Particles[i];
                part.Life = part.Life - SSS.Delta * part.Animator.Speed;
                part.Rotation = part.Rotation + part.RotationVel;
                if (!part.Static)
                {
                    part.Transform.Velocity = CollisionEngine.UpdateVelocity(part.Transform, SSS.Delta);
                    if (part.Collide)
                    {
                        foreach (Collider col in Core._Colliders)
                        {
                            if (CollisionEngine.CollideAll(col, part.Transform, out Vector2 oPos, out Vector2 oVel))
                            {
                                part.Transform.Velocity = oVel;
                                part.Transform.Position = oPos;
                            }
                        }
                    }
                    part.Transform.ApplyVelocity();
                }
                Particles[i] = part;
                if (part.Life <= 0)
                    Particles.RemoveAt(i);
            }
        }

        //Life, Collides, Static, Animation, Gravity, Width, Height, Hex Color
        public static void AddParticles(int id, Vector2 pos, Vector2 vel, float rot, int count, int rand, int spread = 0)
        {
            var props = GetProps(id);

            SingletonAnimator anim = SingletonAnimator.QuickParse((string)props["anim"]);
            Vector3 preCol = MathUtil.HexToRGB((string)props["color"]);

            for (int i = 0; i < count; i++)
            {
                Particle part = new Particle {
                    Transform = new Transform(pos, anim.image.Width, anim.FrameHeight, 1f, true, (bool)props["gravity"]),
                    Collide = (bool)props["collides"],
                    Life = anim.MaxFrames,
                    Static = (bool)props["static"],
                    Animator = anim,
                    Color = new Color(preCol.X, preCol.Y, preCol.Z),
                    RotationVel = MathHelper.ToRadians(rot),
                    Rotation = Random.Shared.Next(-rand, rand),
                    Group = -1
                };

                Vector2 sp = Vector2.Zero;
                if (spread != 0f)
                {
                    float ts = spread * i;
                    float s = MathF.Sin(MathHelper.ToRadians(ts - 135f));
                    float c = MathF.Cos(MathHelper.ToRadians(ts - 135f));
                    sp = new Vector2(s, c);
                }
                Vector2 rv = Vector2.Zero;
                if (rand != 0)
                    rv = new Vector2(Random.Shared.Next(-rand, rand) / 10f, Random.Shared.Next(-rand, rand) / 10f);
                part.Transform.Velocity = vel + sp + rv;

                Particles.Add(part);
            }
        }

        public static void AddParticle(int id, Vector2 pos, Vector2 vel, float rot)
        {
            var props = GetProps(id);

            Vector3 preCol = MathUtil.HexToRGB((string)props["color"]);
            SingletonAnimator anim = SingletonAnimator.QuickParse((string)props["anim"]);

            Particle part = new Particle
            {
                Transform = new Transform(pos, anim.image.Width, anim.FrameHeight, 2f, true, (bool)props["gravity"]),
                Collide = (bool)props["collides"],
                Life = anim.MaxFrames,
                Static = (bool)props["static"],
                Color = new Color(preCol.X, preCol.Y, preCol.Z),
                RotationVel = MathHelper.ToRadians(rot),
                Animator = anim,
                Group = -1
            };

            part.Transform.Velocity = vel;
            Particles.Add(part);
        }

        public static void Draw(Renderer renderer)
        {
            foreach (Particle part in Particles)
            {
                var sauce = part.Animator.Update(out Texture2D img);
                renderer.BasicDraw(img, part.Transform.Position, 1, 1, col: part.Color, source: sauce, rot: part.Rotation, rotSource: new Vector2(sauce.Width / 2, sauce.Height / 2));
            }
        }

        private static Dictionary<string, object> GetProps(int id)
        {
            var props = ParamLoader.BatchProps("particle", id);
            if (!props.ContainsKey("color"))
                props["color"] = "#FFFFFFFF";

            if (!props.ContainsKey("gravity"))
                props["gravity"] = true;
            else
                props["gravity"] = bool.Parse((string)props["gravity"]);

            if (!props.ContainsKey("collides"))
                props["collides"] = false;
            else
                props["collides"] = bool.Parse((string)props["collides"]);

            if (!props.ContainsKey("static"))
                props["static"] = false;
            else
                props["static"] = bool.Parse((string)props["static"]);

            return props;
        }
    }

    public struct Particle
    {
        public Transform Transform { get; set; }
        public float Life { get; set; }
        public bool Collide { get; set; }
        public bool Static { get; set; }
        public int Group { get; set; }
        public Color Color { get; set; }
        public float Rotation { get; set; }
        public float RotationVel { get; set; }
        public SingletonAnimator Animator { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TripleS.Physics;

namespace TripleS.Lighting {

    /// <summary>
    /// Stores data for lighting.
    /// </summary>
    public class LightingEngine {

        public static Dictionary<UUID, LightData> Lights { get; set; }
        public Effect LightingEffect { get; protected set; }
        public Effect AreaLightingEffect { get; protected set; }
        public Effect BackgroundEffect { get; protected set; }
        public int MaxLights { get; }
        public ShadowRay[] TestRays { get; private set; }
        public Vector2[,] TestTris { get; private set; }
        public Color AmbientColor { get; set; }

        public LightingEngine(int ml)
        {
            MaxLights = ml;
            Lights = new Dictionary<UUID, LightData>(MaxLights);
        }

        /// <summary>
        /// Loads effects and calculates shadow map.
        /// </summary>
        /// <param name="path">Path to main lighting effect</param>
        /// <param name="bg">Background effect (optional)</param>
        public void Load(ContentManager content, Renderer renderer, string path, string area, Collider[] levelGeo, string bg = "")
        {
            renderer.ResetLightingBlocks();
            LightingEffect = content.Load<Effect>(path);
            AreaLightingEffect = content.Load<Effect>(area);
            if (bg != "")
                BackgroundEffect = content.Load<Effect>(bg);

            foreach (KeyValuePair<UUID, LightData> kvp in Lights.Where(x => x.Value.Shadows))
            {
                var light = kvp.Value;
                var rays = new List<ShadowRay>();
                foreach (Vert vert in LevelHandler.Verts)
                {
                    Vector2 dir = Vector2.Normalize(vert.Position - light.Position);
                    Ray2D ray = new Ray2D(light.Position, dir);
                    List<float?> di = new List<float?>();
                    float bestDist = float.PositiveInfinity;
                    int bestDIndex = 0;
                    foreach (Collider col in levelGeo.Where(x => !x.PhantomShadow)) //.Where(x => x.Type != ColliderType.Slope)
                    {
                        float? tdi = ray.Intersects(col.GetRectangle());
                        if (tdi != null)
                        {
                            di.Add(tdi);
                            if (tdi < bestDist)
                            {
                                bestDIndex = di.Count - 1;
                                bestDist = tdi.Value;
                            }
                        }
                    }

                    int xNorm = MathUtil.IntSign(Vector2.Dot(dir, new Vector2(vert.XNormal, 0)));
                    int yNorm = MathUtil.IntSign(Vector2.Dot(dir, new Vector2(0, vert.YNormal)));

                    bool canSee = xNorm + yNorm != 2;
                    if (di.Count > 0 && canSee)
                    {
                        float vertDist = Vector2.Distance(ray.Position, vert.Position);
                        bool reached = MathF.Floor(di[bestDIndex].Value) >= MathF.Floor(vertDist);
                        if (reached && ((xNorm == -1 && yNorm == -1) || (di[bestDIndex] != vertDist)))
                        {
                            rays.Add(new ShadowRay { Position = ray.Position, Direction = ray.Direction, Distance = di[bestDIndex].Value, TwiceFirstDir = -1 });
                        }
                        else if (reached)
                        {
                            if (di.Count == 1)
                            {
                                rays.Add(new ShadowRay { Position = ray.Position, Direction = ray.Direction, Distance = LevelHandler.MapBounds.Width * 16, TwiceFirstDir = -1 });
                            }
                            else
                            {
                                float secondBest = float.PositiveInfinity;
                                for (int i = 0; i < di.Count; i++)
                                {
                                    if (di[i].Value < secondBest && di[i].Value != di[bestDIndex])
                                        secondBest = di[i].Value;
                                }

                                rays.Add(new ShadowRay { Position = ray.Position, Direction = ray.Direction, Distance = secondBest, TwiceFirstDir = di[bestDIndex].Value });
                            }
                        }
                    }
                }

                TestRays = rays.ToArray();

                var sortRays = rays.OrderBy(x => MathF.Atan2(x.Position.X - x.End.X, x.Position.Y - x.End.Y)).ToArray();
                var tris = new Vector2[sortRays.Length, 3];
                for (int i = 0; i < sortRays.Length; i++)
                {
                    var ray = sortRays[i];
                    int newIndex = i == sortRays.Length - 1 ? 0 : i + 1;
                    var next = sortRays[newIndex];

                    if (ray.TwiceFirstDir != -1 && i == 0)
                    {
                        sortRays[newIndex].CalTop = Vector2.Distance(ray.End, sortRays[sortRays.Length - 1].End) < Vector2.Distance(ray.TFPos, sortRays[sortRays.Length - 1].End);
                    }

                    Vector2 calEnd = ray.TwiceFirstDir == -1 ? ray.End : ray.CalTop ? ray.End : ray.TFPos;
                    tris[i, 0] = ray.Position;
                    tris[i, 1] = calEnd;

                    bool parallel = ray.End.Y == next.End.Y || ray.End.X == next.End.X;
                    if (next.TwiceFirstDir != -1 && !parallel)
                    {
                        bool bot = Vector2.Distance(next.End, calEnd) > Vector2.Distance(next.TFPos, calEnd);
                        tris[i, 2] = bot ? next.TFPos : next.End;
                        sortRays[newIndex].CalTop = bot;
                    }
                    else
                    {
                        tris[i, 2] = next.End;
                    }
                }

                var l = Lights[kvp.Key];
                l.Tris = tris;
                TestTris = tris;
                Lights[kvp.Key] = l;
            }
        }

        public void RemoveLight(UUID uuid)
        {
            Lights.Remove(uuid);
        }

        public static UUID AddLight(LightData light)
        {
            UUID u = new UUID(1);
            Lights.Add(u, light);
            return u;
        }

        public LightData SetLightPos(UUID uuid, Vector2 pos)
        {
            var ol = Lights[uuid];
            Lights[uuid] = new LightData(pos, ol.Brightness, ol.Radius, ol.Color, ol.Visible, ol.Falloff, ol.Area, ol.Directional, ol.Shadows, ol.Angle, ol.DirRadius);
            return Lights[uuid];
        }
        public LightData SetLightColor(UUID uuid, Vector3 col)
        {
            var ol = Lights[uuid];
            Lights[uuid] = new LightData(ol.Position, ol.Brightness, ol.Radius, col, ol.Visible, ol.Falloff, ol.Area, ol.Directional, ol.Shadows, ol.Angle, ol.DirRadius);
            return Lights[uuid];
        }
        public LightData SetLightRadius(UUID uuid, float rad)
        {
            var ol = Lights[uuid];
            Lights[uuid] = new LightData(ol.Position, ol.Brightness, rad, ol.Color, ol.Visible, ol.Falloff, ol.Area, ol.Directional, ol.Shadows, ol.Angle, ol.DirRadius);
            return Lights[uuid];
        }
        public LightData SetLightBright(UUID uuid, float bri)
        {
            var ol = Lights[uuid];
            Lights[uuid] = new LightData(ol.Position, bri, ol.Radius, ol.Color, ol.Visible, ol.Falloff, ol.Area, ol.Directional, ol.Shadows, ol.Angle, ol.DirRadius);
            return Lights[uuid];
        }
        public LightData SetLightVisible(UUID uuid, bool vis)
        {
            var ol = Lights[uuid];
            Lights[uuid] = new LightData(ol.Position, ol.Brightness, ol.Radius, ol.Color, vis, ol.Falloff, ol.Area, ol.Directional, ol.Shadows, ol.Angle, ol.DirRadius);
            return Lights[uuid];
        }

        /// <summary>
        /// Removes all lights.
        /// </summary>
        public void ResetLights()
        {
            Lights.Clear();
        }
    }
}

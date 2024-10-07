using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace TripleS.Physics {

    /// <summary>
    /// A static helper class that can calculate 
    /// collision and physics with rectangles.
    /// Named "The Damscus Engine"
    /// </summary>
    public static class CollisionEngine {

        public static float gravityF = 4;

        public static bool CollideSlope(Collider col, Transform obj, out Vector2 oPos, out Vector2 oVel)
        {
            //determine relative distance between slope pos and object pos based on parameters (how far along we are on the slope) 
            float x = (!col.MiddleSlope ? (col.Slope < 0 ? obj.Position.X + obj.Width : obj.Position.X) : obj.Position.X + obj.Width / 2) - col.X;
            if(col.UpsidedownSlope)
                x = (col.Slope < 0 ? obj.Position.X : obj.Position.X + obj.Width) - col.X;

            //get the y pos. y = mx+b. get the slope and multiply it by our x (how far along we are) so it applies the slope rate. then add the collider y pos as before it was not a global position. then add a height offset
            float y = col.Slope * x + (col.Y + col.Height * col.SlopeOffset);

            if (col.UpsidedownSlope)
            {
                oPos = new Vector2(obj.Position.X, y);
                oVel = new Vector2(obj.Velocity.X, 0);

                if (Math.Floor(x) > col.Width || Math.Ceiling(x) < 0 || obj.Position.Y < col.Y - 1)
                    return false;
                else if (obj.Position.Y + obj.Velocity.Y < y)
                {
                    if (y < col.Y)
                        oPos = new Vector2(obj.Position.X, col.Y);
                    return true;
                }
                else
                    return false;
            }
            else
            {
                oPos = new Vector2(obj.Position.X, y - obj.Height);
                oVel = new Vector2(obj.Friction ? col.Slope : obj.Velocity.X, 0);

                //if position is inside and aligned with collider bounds. else if determines if object is below slope with velocity applied.
                if (Math.Floor(x) > col.Width + obj.Velocity.X || Math.Ceiling(x) < 0 + obj.Velocity.X || obj.Position.Y + obj.Height > col.Y + col.Height + 1)
                    return false;
                else if (obj.Position.Y + obj.Height + obj.Velocity.Y > y)
                {
                    if (y < col.Y)
                        oPos = new Vector2(obj.Position.X, col.Y - obj.Height);
                    return true;
                }
                else
                    return false;
            }
        }

        public static bool CollideTop(Collider col, Transform obj, out Vector2 oPos, out Vector2 oVel)
        {
            oPos = new Vector2(obj.Position.X, col.Y - obj.Height);
            oVel = new Vector2(obj.Velocity.X, 0);

            if (obj.Position.Y + obj.Velocity.Y + obj.Height > col.Y && obj.OldPosition.Y + obj.Height <= col.Y
                && obj.Position.X - col.X + obj.Width > 0 && obj.Position.X - col.X < col.Width)
                return true;
            return false;
        }

        public static bool CollideBottom(Collider col, Transform obj, out Vector2 oPos, out Vector2 oVel)
        {
            oPos = new Vector2(obj.Position.X, col.Y + col.Height);
            oVel = new Vector2(obj.Velocity.X, 0);

            if (obj.Position.Y + obj.Velocity.Y < col.Y + col.Height && obj.OldPosition.Y >= col.Y + col.Height
                && obj.Position.X - col.X + obj.Width > 0 && obj.Position.X - col.X < col.Width)
                return true;
            return false;
        }

        public static bool CollideLeft(Collider col, Transform obj, out Vector2 oPos, out Vector2 oVel)
        {
            oPos = new Vector2(col.X - obj.Width, obj.Position.Y);
            oVel = new Vector2(0, obj.Velocity.Y);

            if (obj.Position.X + obj.Velocity.X + obj.Width > col.X && obj.OldPosition.X + obj.Width <= col.X
                && obj.Position.Y - col.Y + obj.Height > 0 && obj.Position.Y - col.Y < col.Height)
                return true;
            return false;
        }

        public static bool CollideRight(Collider col, Transform obj, out Vector2 oPos, out Vector2 oVel)
        {
            oPos = new Vector2(col.X + col.Width, obj.Position.Y);
            oVel = new Vector2(0, obj.Velocity.Y);

            if (obj.Position.X + obj.Velocity.X < col.X + col.Width && obj.OldPosition.X >= col.X + col.Width
                && obj.Position.Y - col.Y + obj.Height > 0 && obj.Position.Y - col.Y < col.Height)
                return true;
            return false;
        }

        public static bool CollidePointTop(Collider col, Vector2 pos, Vector2 vel, Vector2 old, out Vector2 oPos, out Vector2 oVel)
        {
            oPos = new Vector2(pos.X, col.Y - 1);
            oVel = new Vector2(vel.X, 0);

            if (pos.Y + vel.Y >= col.Y - 1 && old.Y <= col.Y
                && pos.X - col.X > 0 && pos.X - col.X < col.Width)
                return true;
            return false;
        }

        public static bool CollidePointBottom(Collider col, Vector2 pos, Vector2 vel, Vector2 old, out Vector2 oPos, out Vector2 oVel)
        {
            oPos = new Vector2(pos.X, col.Y + col.Height + 1);
            oVel = new Vector2(vel.X, 0);

            if (pos.Y + vel.Y <= col.Y + col.Height + 1 && old.Y >= col.Y + col.Height
                && pos.X - col.X > 0 && pos.X - col.X < col.Width)
                return true;
            return false;
        }

        public static bool CollidePointLeft(Collider col, Vector2 pos, Vector2 vel, Vector2 old, out Vector2 oPos, out Vector2 oVel)
        {
            oPos = new Vector2(col.X - 1, pos.Y);
            oVel = new Vector2(0, vel.Y);

            if (pos.X + vel.X >= col.X - 1 && old.X <= col.X
                && pos.Y - col.Y > 0 && pos.Y - col.Y < col.Height)
                return true;
            return false;
        }

        public static bool CollidePointRight(Collider col, Vector2 pos, Vector2 vel, Vector2 old, out Vector2 oPos, out Vector2 oVel)
        {
            oPos = new Vector2(col.X + col.Width + 1, pos.Y);
            oVel = new Vector2(0, vel.Y);

            if (pos.X + vel.X <= col.X + col.Width + 1 && old.X >= col.X + col.Width
                && pos.Y - col.Y > 0 && pos.Y - col.Y < col.Height)
                return true;
            return false;
        }

        public static bool CollideFace(Collider col, Transform obj, out Vector2 oPos, out Vector2 oVel)
        {
            oPos = obj.Position;
            oVel = obj.Velocity;

            switch (col.Side)
            {
                case 0:
                    return CollideTop(col, obj, out oPos, out oVel);
                case 1:
                    return CollideBottom(col, obj, out oPos, out oVel);
                case 2:
                    return CollideLeft(col, obj, out oPos, out oVel);
                case 3:
                    return CollideRight(col, obj, out oPos, out oVel);
            }

            return false;
        }

        public static bool CollideRect(Collider col, Transform obj, out Vector2 oPos, out Vector2 oVel)
        {
            if (CollideTop(col, obj, out oPos, out oVel))
                return true;
            else if (CollideBottom(col, obj, out oPos, out oVel))
                return true;
            else if (CollideLeft(col, obj, out oPos, out oVel))
                return true;
            else if (CollideRight(col, obj, out oPos, out oVel))
                return true;
            else
                return false;
        }

        public static bool CollideAll(Collider col, Transform obj, out Vector2 oPos, out Vector2 oVel)
        {
            oPos = obj.Position;
            oVel = obj.Velocity;

            switch (col.Type)
            {
                case ColliderType.Rectangle:
                    return CollideRect(col, obj, out oPos, out oVel);
                case ColliderType.Face:
                    return CollideFace(col, obj, out oPos, out oVel);
                case ColliderType.Slope:
                    return CollideSlope(col, obj, out oPos, out oVel);
            }

            return false;
        }

        public static bool CollideAllPoint(Collider col, Vector2 point, Vector2 vel, Vector2 oldPos, out Vector2 oPos, out Vector2 oVel)
        {
            if (CollidePointTop(col, point, vel, oldPos, out oPos, out oVel))
                return true;
            else if (CollidePointBottom(col, point, vel, oldPos, out oPos, out oVel))
                return true;
            else if (CollidePointLeft(col, point, vel, oldPos, out oPos, out oVel))
                return true;
            else if (CollidePointRight(col, point, vel, oldPos, out oPos, out oVel))
                return true;
            else
                return false;
        }


        /// <summary>
        /// Calculates velocity based on physics.
        /// Should be called before collision.
        /// </summary>
        /// <param name="deltaTime">Delta time of game</param>
        /// <returns>Physical velocity</returns>
        public static Vector2 UpdateVelocity(Transform obj, float deltaTime)
        {
            obj.OldVelocity = obj.Velocity;
            Vector2 outVel = obj.Velocity;
            if (obj.Friction)
                outVel.X = (float)Math.Round(outVel.X /= (obj.Weight / Math.Abs(outVel.X)) / 100 + 1, 3, MidpointRounding.ToZero);
            if (obj.Gravity)
                outVel.Y += obj.Weight / gravityF * deltaTime;
            return outVel;
        }

        public static bool PointInRect(Rectangle bounds, Vector2 point)
        {
            return point.X > bounds.Left &&
                point.X < bounds.Right &&
                point.Y > bounds.Top &&
                point.Y < bounds.Bottom;
        }

        public static bool RectInRect(Rectangle A, Rectangle B)
        {
            return A.Intersects(B);
            /*
                return A.Bottom > B.Top &&
                A.Top < B.Bottom &&
                A.Right > B.Left &&
                A.Left < B.Right;
            */
        }

        //OH MY GO-
        //Get y=mx+b nubmers for each side. if there are two below the inside point AND the point is within the bounds of the triangle, return true.
        public static bool PointInTriangle(Vector2[] points, Vector2 calPoint)
        {
            bool[] yp = null;
            if (points.Length == 3)
            {
                float m1 = MathUtil.Diff(points[0].Y, points[1].Y) / MathUtil.Diff(points[0].X, points[1].X);
                float b1 = (points[0].X * m1) - points[0].Y;
                float y1 = m1 * calPoint.X - b1;
                bool tou1 = float.IsInfinity(y1) || float.IsNaN(y1) ? true : y1 > calPoint.Y;

                float m2 = -MathUtil.Diff(points[1].Y, points[2].Y) / MathUtil.Diff(points[1].X, points[2].X);
                float b2 = (points[1].X * m2) - points[1].Y;
                float y2 = m2 * calPoint.X - b2;
                bool tou2 = float.IsInfinity(y2) || float.IsNaN(y2) ? true : y2 > calPoint.Y;

                float m3 = MathUtil.Diff(points[2].Y, points[0].Y) / MathUtil.Diff(points[2].X, points[0].X);
                float b3 = (points[2].X * m3) - points[2].Y;
                float y3 = m3 * calPoint.X - b3;
                bool tou3 = float.IsInfinity(y3) || float.IsNaN(y3) ? true : y3 > calPoint.Y;

                yp = new bool[3] { tou1, tou2, tou3 };

                var xs = new float[3] { points[0].X, points[1].X, points[2].X };
                var ys = new float[3] { points[0].Y, points[1].Y, points[2].Y };
                Vector2 smallest = new Vector2(MathUtil.SmallestNumber(xs), MathUtil.SmallestNumber(ys));
                Vector2 biggest = new Vector2(MathUtil.BiggestNumber(xs), MathUtil.BiggestNumber(ys));

                bool betweenTri = PointInRect(new Rectangle((int)smallest.X, (int)smallest.Y, (int)biggest.X - (int)smallest.X, (int)biggest.Y - (int)smallest.Y), calPoint);

                if (betweenTri && (yp.Where(x => x == true).Count() == 2))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool PointBetweenTwoX(Vector2 cal, Vector2 one, Vector2 two)
        {
            return cal.X < MathF.Max(one.X, two.X) && cal.X > MathF.Min(one.X, two.X);
        }

        public static bool PointBetweenTwoY(Vector2 cal, Vector2 one, Vector2 two)
        {
            return cal.Y < MathF.Max(one.Y, two.Y) && cal.Y > MathF.Min(one.Y, two.Y);
        }

        public static Rectangle InflateRect(Rectangle rect, float amount)
        {
            rect.Inflate(amount, amount);
            return rect;
        }

        public static float Magnitude(Vector2 inp)
        {
            return MathF.Sqrt((inp.X * inp.X) + (inp.Y * inp.Y));
        }

        public static void DirectSlope(Collider col, Transform obj, out Vector2 oPos, out Vector2 oVel)
        {
            float x = (!col.MiddleSlope ? (col.Slope < 0 ? obj.Position.X + obj.Width : obj.Position.X) : obj.Position.X + obj.Width / 2) - col.X;
            float y = col.Slope * x + (col.Y + col.Height * col.SlopeOffset);

            oPos = new Vector2(obj.Position.X, y - obj.Height);
            oVel = new Vector2(obj.Friction ? col.Slope : obj.Velocity.X, 0);
        }
    }
}

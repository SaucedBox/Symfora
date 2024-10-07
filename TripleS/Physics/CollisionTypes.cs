using System;
using Microsoft.Xna.Framework;

namespace TripleS.Physics {

    public struct Collider {

        public int Width { get; }
        public int Height { get; }
        public float X { get; }
        public float Y { get; }
        public bool PhantomShadow { get; }
        public ColliderType Type { get; }

        public float Slope { get; }
        public float SlopeOffset { get; }
        public bool UpsidedownSlope { get; }
        public bool MiddleSlope { get; }

        public int Side { get; }
        public float SideOffset { get; }

        public string[] Tags { get; set; }

        public Collider(Rectangle col)
        {
            X = col.X;
            Y = col.Y;
            Width = col.Width;
            Height = col.Height;
            Type = ColliderType.Rectangle;

            Slope = 0;
            SlopeOffset = 0;
            MiddleSlope = false;
            UpsidedownSlope = false;
            Side = 0;
            SideOffset = 0;
            PhantomShadow = false;
            Tags = new string[0];
        }

        public Collider(Rectangle col, float m, float slopeOff, bool middleSlope, bool upsidedown)
        {
            X = col.X;
            Y = col.Y;
            Width = col.Width;
            Height = col.Height;
            Type = ColliderType.Slope;

            Slope = m;
            SlopeOffset = slopeOff;
            MiddleSlope = middleSlope;
            UpsidedownSlope = upsidedown;

            Side = 0;
            SideOffset = 0;
            PhantomShadow = false;
            Tags = new string[0];
        }

        public Collider(Rectangle col, int side, float sOff, bool noShad)
        {
            X = col.X;
            Y = col.Y;
            Width = col.Width;
            Height = col.Height;
            Type = ColliderType.Face;

            Slope = 0;
            SlopeOffset = 0;
            MiddleSlope = false;
            UpsidedownSlope = false;

            Side = side;
            SideOffset = sOff;
            PhantomShadow = noShad;
            Tags = new string[0];
        }

        public Rectangle GetRectangle()
        {
            return new Rectangle((int)X, (int)Y, Width, Height);
        }
    }

    public enum ColliderType {
        Rectangle,
        Slope,
        Face
    }

    public class Transform {
        public int Width { get; set; }
        public int Height { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 OldPosition { get; private set; }
        public Vector2 OldVelocity { get; set; }
        public float Weight { get; set; }
        public bool Friction { get; set; }
        public bool Gravity { get; set; }
        public Vector2 Centre
        {
            get
            {
                return new Vector2(Position.X + Width / 2, Position.Y + Height / 2);
            }
            private set { }
        }

        public Transform(Vector2 position, int width, int height, float weight, bool friction, bool gravity = true)
        {
            Position = position;
            Width = width;
            Height = height;
            Weight = weight;
            Velocity = Vector2.Zero;
            Friction = friction;
            Gravity = gravity;
            OldPosition = position;
            OldVelocity = Velocity;
        }

        public Rectangle GetRectangle()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
        }

        public void ApplyVelocity()
        {
            OldPosition = Position;
            Position += Velocity;
        }

        public void ChangeVelocityX(float X)
        {
            Velocity = new Vector2(X, Velocity.Y);
        }

        public void ChangeVelocityY(float Y)
        {
            Velocity = new Vector2(Velocity.X, Y);
        }

        public static Vector2 RotateAroundAPoint(Vector2 point, float angle, float space)
        {
            var oldPoint = point;

            space = MathF.Sqrt(2) * space / 2;
            float s = MathF.Sin(MathHelper.ToRadians(angle - 135f));
            float c = MathF.Cos(MathHelper.ToRadians(angle - 135f));

            float xnew = space * c - space * s;
            float ynew = space * s + space * c;

            point.X = xnew;
            point.Y = ynew;
            return point + oldPoint;
        }

        public void ResetOldPosition()
        {
            OldPosition = Position;
        }

        public float WHMedian()
        {
            return MathF.Min(Width, Height) + (MathUtil.Diff(Width, Height) / 2);
        }
    }

    public struct Vert
    {
        public Vector2 Position { get; set; }
        public int XNormal { get; }
        public int YNormal { get; }

        public Vert(Vector2 pos, int xnorm, int ynorm)
        {
            Position = pos;
            XNormal = Math.Clamp(xnorm, -1, 1);
            YNormal = Math.Clamp(ynorm, -1, 1);
        }
    }
}

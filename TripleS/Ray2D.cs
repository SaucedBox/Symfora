using System;
using Microsoft.Xna.Framework;

namespace TripleS {
    public struct Ray2D {

        public Vector2 Direction;
        public Vector2 Position;

        public Ray2D(Vector2 pos, Vector2 dir)
        {
            Direction = dir;
            Position = pos;
        }

        public float? Intersects(Rectangle rect)
        {
            //Get points on rectangle
            Vector2 min = rect.Location.ToVector2();
            Vector2 max = min + new Vector2(rect.Width, rect.Height);

            //Two ? numbers
            float? num = null;
            float? num2 = null;
            //Return null if the pint isn't inside the rect on the X and the X direction is too small to calculate 
            if (Math.Abs(Direction.X) < 1E-06f)
            {
                if (Position.X < min.X || Position.X > max.X)
                {
                    return null;
                }
            }
            else
            {
                //Get the collision on the x. Divide the difference between the min and the max,
                //and if the min collision is greater than the max, we have contact.
                num = (min.X - Position.X) / Direction.X;
                num2 = (max.X - Position.X) / Direction.X;
                if (num > num2)
                {
                    //invert values
                    float? num3 = num;
                    num = num2;
                    num2 = num3;
                }
            }

            if (Math.Abs(Direction.Y) < 1E-06f)
            {
                if (Position.Y < min.Y || Position.Y > max.Y)
                {
                    return null;
                }
            }
            else
            {
                //Same as above but for Y
                float num4 = (min.Y - Position.Y) / Direction.Y;
                float num5 = (max.Y - Position.Y) / Direction.Y;
                if (num4 > num5)
                {
                    float num6 = num4;
                    num4 = num5;
                    num5 = num6;
                }

                if ((num.HasValue && num > num5) || (num2.HasValue && num4 > num2))
                {
                    return null;
                }

                if (!num.HasValue || num4 > num)
                {
                    num = num4;
                }

                if (!num2.HasValue || num5 < num2)
                {
                    num2 = num5;
                }
            }

            if (num.HasValue && num < 0f && num2 > 0f)
            {
                return 0f;
            }

            if (num < 0f)
            {
                return null;
            }

            return num;
        }
    }
}

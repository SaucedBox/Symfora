using System;
using TripleS.Physics;
using Microsoft.Xna.Framework;

namespace TripleS {
    public static class MathUtil {

        public static float MinClamp(float value, float min)
        {
            if (value < min)
            {
                return min;
            }

            return value;
        }

        public static float MaxClamp(float value, float max)
        {
            if (value > max)
            {
                return max;
            }

            return value;
        }

        public static Vector2 RoundVector2(Vector2 vector, int digit)
        {
            return new Vector2(MathF.Round(vector.X, digit), MathF.Round(vector.Y, digit));
        }

        public static Vector2 SafeNomralize(Vector2 value)
        {
            value.Normalize();
            if (float.IsNaN(value.X))
                value.X = 0;
            if (float.IsNaN(value.Y))
                value.Y = 0;
            return value;
        }

        public static bool WithinHalfSquare(float point, float centre, int width)
        {
            return point < centre + width && point > centre - width;
        }

        public static int IntSign(int number, bool calZero = false)
        {
            if (calZero && number == 0)
                return number;
            return number < 0 ? -1 : 1;
        }

        public static int IntSign(float number, bool calZero = false)
        {
            if (calZero && number == 0)
                return 0;
            return number < 0f ? -1 : 1;
        }

        /// <summary>
        /// Converts a 6 digit hex color into a 0-1 based RGB value.
        /// </summary>
        public static Vector3 HexToRGB(string hex)
        {
            var color = System.Drawing.ColorTranslator.FromHtml(hex);
            float r = Convert.ToInt16(color.R);
            float g = Convert.ToInt16(color.G);
            float b = Convert.ToInt16(color.B);
            return new Vector3(r / 255, g / 255, b / 255);
        }

        /// <summary>
        /// In order of top left, top right, bottom left, bottom right, Gets the 4 verticies of a given rectangle
        /// </summary>
        public static Vert[] GetRectangleVerts(Rectangle rect)
        {
            return new Vert[4]
            {
                new Vert(new Vector2(rect.Left, rect.Top), -1, -1),
                new Vert(new Vector2(rect.Right, rect.Top), 1, -1),
                new Vert(new Vector2(rect.Left, rect.Bottom), -1, 1),
                new Vert(new Vector2(rect.Right, rect.Bottom), 1, 1)
            };
        }

        public static float Diff(float val1, float val2)
        {
            return val1 > val2 ? val1 - val2 : val2 - val1;
        }

        public static float SmallestNumber(float[] val)
        {
            float best = float.PositiveInfinity;
            foreach(float cval in val)
            {
                if(cval < best)
                {
                    best = cval;
                }
            }
            return best;
        }

        public static float BiggestNumber(float[] val)
        {
            float best = float.NegativeInfinity;
            foreach (float cval in val)
            {
                if (cval > best)
                {
                    best = cval;
                }
            }
            return best;
        }

        public static Vector2 AbsVector(Vector2 val)
        {
            return new Vector2(MathF.Abs(val.X), MathF.Abs(val.Y));
        }
    }
}

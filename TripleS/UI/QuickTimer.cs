using System;
using Microsoft.Xna.Framework;

namespace TripleS.UI {

    /// <summary>
    /// Timer for UI fading and effects, or any other use.
    /// </summary>
    public class QuickTimer {

        public float Value { get; private set; }
        public bool Active { get; set; }

        float minimum;
        float maximum;
        bool up;
        float def;
        float speed;

        public QuickTimer(float min, float max, bool countUp, float speedMult)
        {
            minimum = min;
            maximum = max;
            up = countUp;
            def = countUp ? min : max;
            Value = def;
            Active = true;
            speed = speedMult;
        }

        /// <summary>
        /// Updates timer.
        /// </summary>
        /// <param name="time">Use for delta time</param>
        /// <param name="inverse">Count down or up</param>
        /// <returns>If timer has completed</returns>
        public bool UpdateBool(GameTime time, bool inverse)
        {
            if (Active)
            {
                float deltaTime = up ? (float)time.ElapsedGameTime.TotalSeconds : (float)-time.ElapsedGameTime.TotalSeconds;

                Value += deltaTime * speed;
                Value = Math.Clamp(Value, Math.Min(minimum, maximum), Math.Max(minimum, maximum));

                if (up)
                {
                    if (Value >= maximum)
                    {
                        Value = def;
                        return !inverse;
                    }
                    else
                        return inverse;
                }
                else
                {
                    if (Value <= minimum)
                    {
                        Value = def;
                        return !inverse;
                    }
                    else
                        return inverse;
                }
            }
            else
                return false; 
        }

        /// <summary>
        /// Updates timer.
        /// </summary>
        /// <param name="time">Use for delta time</param>
        /// <returns>Timer value</returns>
        public float UpdateFloat(GameTime time)
        {
            if (Active)
            {
                float deltaTime = up ? (float)time.ElapsedGameTime.TotalSeconds : (float)-time.ElapsedGameTime.TotalSeconds;

                Value += deltaTime * speed;
                Value = Math.Clamp(Value, Math.Min(minimum, maximum), Math.Max(minimum, maximum));
            }
            return Value;
        }

        /// <summary>
        /// Resets timer.
        /// </summary>
        public void Reset()
        {
            Value = def;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TripleS;
using TripleS.Lighting;
using TripleS.Physics;
using TripleS.Scripting;

namespace Symfora {
    public static class PhantomManager {

        public static bool[][] Tallies { get; set; }
        public static int LastSoanoHeadTallyID { get; set; }
        public static int StatPoints { get; set; }
        private static int deathTick = -1;
        private static int maxDeathTick = 0;

        public static void Initialize()
        {
            if (SSS.FirstLoad)
            {
                Tallies = new bool[6][] { new bool[1] { false },
                new bool[1] { false },
                new bool[2] { false, false },
                new bool[2] { false, false },
                new bool[2] { false, false },
                new bool[2] { false, false } };
            }
            else
            {
                Tallies = new bool[6][] { new bool[1] { QPB(38) },  //Ponderosa
                new bool[1] { QPB(39) },                            //Shallows
                new bool[2] { QPB(40), QPB(41) },                   //Relayer
                new bool[2] { QPB(42), QPB(43) },                   //Paralevo
                new bool[2] { QPB(44), QPB(45) },                   //Up. Chlorals
                new bool[2] { QPB(46), QPB(47) } };                 //Do. Chlorals
            }
            if (GameStateManager.States.ContainsKey(66))
                StatPoints = (int)GameStateManager.States[66];
        }

        public static void AddDeath()
        {
            if(LevelHandler.CurrentLevel > 1 && LevelHandler.CurrentLevel < 17 && deathTick < maxDeathTick)
            {
                deathTick++;
                int main = GetCurrentTallyID();
                maxDeathTick = Tallies[main].Length - 1; 
                Tallies[main][deathTick] = true;
            }
        }

        public static void TickStats(int mult)
        {
            StatPoints += 1 + mult;
            GameStateManager.States[66] += 1 + mult;
        }

        private static bool QPB(int gs)
        {
            return GameStateManager.States[gs] == 0 ? false : true;
        }

        public static int QPT(int first, int second)
        {
            return Tallies[first][second] ? 1 : 0;
        }

        public static int GetCurrentTallyID()
        {
            int main = int.Parse(ParamLoader.GetParam<string>("level", LevelHandler.CurrentLevel, "name")[1..]);
            return Math.Clamp(main - 1, 0, 5);
        }
    }
}

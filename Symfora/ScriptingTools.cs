using System;
using System.Linq;
using TripleS;
using TripleS.Scripting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using TripleS.Animations;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using TripleS.UI;
using System.ComponentModel;
using System.Net.Http.Headers;

namespace Symfora {
    public static class ST {

        public static Entity GetEnt(string name)
        {
            return Core._LevelHandler.EntityMan.GetEnt(name);
        }

        public static void KillEnt(string name)
        {
            Core._LevelHandler.EntityMan.KillEnt(name);
        }

        public static float GetGS(int id)
        {
            return GameStateManager.States[id];
        }

        public static bool ExistsGS(int id)
        {
            return GameStateManager.States.ContainsKey(id);
        }

        public static void SetGS(int id, int value)
        {
            GameStateManager.States[id] = value;
        }

        public static void SetEnt(string name, bool state)
        {
            var e = Core._LevelHandler.EntityMan.GetEnt(name);
            if (state)
                e.Activate();
            else
                e.Deactivate();
        }

        public static void FlipEnt(string name)
        {
            var e = Core._LevelHandler.EntityMan.GetEnt(name);
            if (!e.Active)
                e.Activate();
            else
                e.Deactivate();
        }

        public static void SetAllEnts(string name, bool state)
        {
            foreach (Entity ent in Core._LevelHandler.EntityMan.GetAllEnts(name))
            {
                if (state)
                    ent.Activate();
                else
                    ent.Deactivate();
            }
        }

        public static void ShootBallista(string name)
        {
            var target = Core._LevelHandler.EntityMan.GetEnt(name);
            if(target.ID == "ballista")
            {
                var trueTarget = (EntBallista)target;
                trueTarget.Shoot();
            }
        }

        public static void PlaySound(string eventPath, int x, int y, float vol, float pitch)
        {
            AudioSystem.PlayEvent(eventPath, true, new Vector2(x, y), true, vol, true, pitch);
        }

        public static int PlayMusic(string eventPath, float vol)
        {
            return AudioSystem.PlayEvent(eventPath, false, Vector2.Zero, false, vol, false);
        }

        public static void SetMusicParam(int id, string path, float val)
        {
            AudioSystem.SetParameter(id, path, val);
        }

        public static void SelfDialogue(string id)
        {
            DialogueManager.AddSelfDialogue(id);
        }

        public static void Tutorial(int id)
        {
            Keys[] keys = null;
            switch (id)
            {
                case 0: keys = new Keys[2] { GameInputs.Controls["left"], GameInputs.Controls["right"] };
                    break;
                case 1: keys = new Keys[1] { GameInputs.Controls["jump"] };
                    break;
                case 2: keys = new Keys[1] { GameInputs.Controls["boost"] };
                    break;
                case 3: keys = new Keys[2] { GameInputs.Controls["ramLeft"], GameInputs.Controls["ramRight"] };
                    break;
                case 4: keys = new Keys[1] { GameInputs.Controls["boost"] };
                    break;
                case 5: keys = new Keys[1] { GameInputs.Controls["down"] };
                    break;
                case 7: keys = new Keys[2] { GameInputs.Controls["boost"], GameInputs.Controls["ramRight"] };
                    break;
                case 11: keys = new Keys[1] { GameInputs.Controls["boost"] };
                    break;
            }
            DialogueManager.AddTutorial("tutorial" + id, keys);
        }

        public static void RestoreSelfBoosts()
        {
            Core._Player.AddSelfBoost(Core._Player.MaxSelfBoosts);
        }

        public static void KillPlayer()
        {
            Core.Die();
        }

        public static void SaveGame()
        {
            Core.SaveGame(true);
        }

        public static void SoanoFlight(bool state)
        {
            if (!state)
            {
                Core._Player.JetBoosted = false;
                Core._Player.jetTimer = 0;
                Core._Player.SoanoFlying = false;
            }
            else
            {
                Core._Player.JetBoost(true);
            }
        }

        public static void Blackout()
        {
            DialogueManager.GeneralBlackout();
        }

        public static int GetProgress()
        {
            return Core.Progress;
        }

        public static void SetNPCState(string npcEnt, int id, float state)
        {
            var maker = (EntNPC)GetEnt(npcEnt);
            Core.GetNPC(maker.npcUUID.MainUUID).ChangeAIState(id, state);
        }
        public static float GetNPCState(string npcEnt, int id)
        {
            var maker = (EntNPC)GetEnt(npcEnt);
            return Core.GetNPC(maker.npcUUID.MainUUID).GetAIState(id);
        }

        public static void ChangeNPCHealth(string npcEnt, int amount)
        {
            var maker = (EntNPC)GetEnt(npcEnt);
            Core.GetNPC(maker.npcUUID.MainUUID).Health += amount;
        }

        public static void StartScene(int id)
        {
            DialogueManager.StartSceneSequence(id);
        }

        public static void AwakeBoss(string entName)
        {
            var find = Core._LevelHandler.EntityMan.Entities.Where(x => x.Name == entName);
            if (find.Any())
            {
                EntNPC pre = (EntNPC)find.First();
                NPC target = Core.GetNPC(pre.npcUUID.MainUUID);
                if (target.Boss)
                    target.Noticed = true;
            }
        }

        public static void FallKillPlayer()
        {
            GameInputs.BlockAllInputs = true;
            Core._Player.FallKilling = true;
            Core._Player.fallDeathTimer = 1f;
            Core._Player.Invincible = true;
        }

        public static void BlockSelfBoosts(bool active)
        {
            Core._Player.BlockSelfBoost = active;
        }

        public static int IntClamp(int v, int mi, int mx)
        {
            return Math.Clamp(v, mi, mx);
        }

        public static void Print(object val)
        {
            Debug.Log(val);
        }
    }
}

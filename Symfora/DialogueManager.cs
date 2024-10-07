using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Linq;
using TripleS;
using TripleS.UI;
using Microsoft.Xna.Framework.Input;

namespace Symfora {
    public static class DialogueManager {

        private static List<string> selfDialogues;
        private static bool selfTutorial;
        private static float selfTimer; 
        private static int currentSelf;
        private static List<int[]> tutorialInputs;

        public static int CurrentScene { get; private set; }
        public static int CurrentAct { get; private set; }
        public static bool InScene { get; private set; }
        private static float sceneTimer;
        private static ActInfo[] acts;
        private static float blackoutTimer;

        public static void AddSelfDialogue(string id)
        {
            if (!selfTutorial)
            {
                selfDialogues ??= new List<string>();

                string text = LocalizationManager.Dialogues[id];
                if (selfDialogues.Count == 0)
                {
                    selfTimer = CalculateTime(text.Length);
                    currentSelf = 0;
                }
                text = text.Replace(@"\n", Environment.NewLine);
                selfDialogues.Add(text);
            }
        }

        public static void Draw(Renderer renderer)
        {
            if(Core._Player.Health == 0)
            {
                selfDialogues = null;
                selfTutorial = false;
                selfTimer = 0f;
                currentSelf = 0;
                tutorialInputs = null;
                CurrentScene = 0;
                CurrentAct = 0;
                InScene = false;
                selfTimer = 0f;
                acts = null;
            }

            if (selfDialogues != null && selfDialogues.Count > 0 && !InScene)
            {
                bool badTut = selfTutorial && tutorialInputs[currentSelf][0] == -1;

                Color color = Color.Gray * Math.Clamp(selfTimer, 0f, 1f);
                if (selfTutorial && !badTut)
                    color = Color.Cyan;
                else if (badTut)
                    color = Color.Cyan * Math.Clamp(selfTimer, 0f, 1f);
                var font = UIManager.fonts["regular"];

                Vector2 otherDim = font.MeasureString(selfDialogues[currentSelf]);
                otherDim = Core._Player.Transform.Position - new Vector2(otherDim.X / 4 - 4, otherDim.Y / 2);
                Vector2 pos = renderer.View.Position - (renderer.View.Position * renderer.View.Zoom) + (otherDim * renderer.View.Zoom);

                renderer.DrawText(font, selfDialogues[currentSelf], pos, 1, 1, col: color, size: 2);

                if (!selfTutorial || badTut)
                {
                    if (selfTimer <= 0)
                    {
                        currentSelf++;
                        if (currentSelf >= selfDialogues.Count)
                        {
                            currentSelf = 0;
                            selfTimer = 0;
                            selfDialogues = null;
                            if (selfTutorial)
                                tutorialInputs = null;
                            if (badTut)
                                selfTutorial = false;  
                        }
                        else
                            selfTimer = CalculateTime(selfDialogues[currentSelf].Length);
                    }
                    else
                        selfTimer -= SSS.Delta;
                }
                else
                {
                    foreach (int k in tutorialInputs[currentSelf]) 
                    {
                        if (GameInputs.OncePress((Keys)k))
                        {
                            currentSelf++;
                            if (currentSelf >= tutorialInputs.Count)
                            {
                                currentSelf = 0;
                                selfTimer = 0;
                                selfDialogues = null;
                                selfTutorial = false;  
                                tutorialInputs = null;
                            }
                            break;
                        } 
                    }
                }
            }

            if (InScene)
            {
                if (sceneTimer < acts[CurrentAct].MaxTime && !GameInputs.OncePress(Keys.Space))
                {
                    if (sceneTimer == 0f)
                    {
                        Core._Player.Invincible = acts[CurrentAct].FreezePlayer;
                        Core._Player.BlockMovement = acts[CurrentAct].FreezePlayer;
                    }

                    sceneTimer += SSS.Delta;
                    var font = UIManager.fonts["regular"];
                    string text = LocalizationManager.Dialogues[acts[CurrentAct].LocName];
                    text = text.Replace(@"\n", Environment.NewLine);

                    Vector2 position = Core._Player.Transform.Position;
                    Vector2 otherDim = font.MeasureString(text);
                    otherDim = new Vector2(otherDim.X / 4 - 4, otherDim.Y / 2);

                    if (!acts[CurrentAct].Thought && acts[CurrentAct].TextPos != "player")
                    {
                        var tp = acts[CurrentAct].TextPos;
                        if (tp.Contains('.'))
                        {
                            var sp = tp.Split('.');
                            position = new Vector2(int.Parse(sp[0]), int.Parse(sp[1]));
                        }
                        else
                            position = Core._LevelHandler.EntityMan.GetEnt(tp).Position;
                    }

                    otherDim = position - otherDim;
                    Vector2 pos = renderer.View.Position - (renderer.View.Position * renderer.View.Zoom) + (otherDim * renderer.View.Zoom);

                    Color color = acts[CurrentAct].TextPos == "player" ? Color.Yellow : Color.White;
                    color = acts[CurrentAct].Thought ? Color.Gray : color;

                    renderer.DrawText(font, text, pos, 1, 1, col: color, size: 2);
                }
                else
                {
                    CurrentAct++;
                    sceneTimer = 0f;
                    if(CurrentAct >= acts.Length)
                    {
                        InScene = false;
                        CurrentAct = 0;
                        CurrentScene = 0;
                    }
                }

                if (acts[CurrentAct].CamPos.HasValue && InScene)
                    Core.CamTarget = acts[CurrentAct].CamPos.Value;
            }

            if (blackoutTimer > 0)
            {
                blackoutTimer -= SSS.Delta;
                if (blackoutTimer <= 0)
                    UIDesigner.ChangeUI(6);
            }
        }

        public static void AddTutorial(string id, Keys[] key)
        {
            selfDialogues ??= new List<string>();
            if (!selfTutorial)
            {
                selfDialogues.Clear();
                currentSelf = 0;
                selfTimer = 0;
            }
            selfTutorial = true;
            tutorialInputs ??= new List<int[]>();

            var text = LocalizationManager.Dialogues[id];
            if (key == null || key.Length == 0)
            {
                tutorialInputs.Add(new int[1] { -1 });
                selfTimer = CalculateTime(64);
            }
            else
            {
                int[] convert = new int[key.Length];
                string[] parse = new string[key.Length];
                for (int i = 0; i < key.Length; i++)
                {
                    convert[i] = (int)key[i];
                    parse[i] = key[i].ToString();
                }
                tutorialInputs.Add(convert);
                text = LocalizationManager.GetVariableString(id, false, parse);
            }
            text = text.Replace(@"\n", Environment.NewLine);
            selfDialogues.Add(text);
        }

        public static float CalculateTime(int length)
        {
            float time = length / 8f;
            return Math.Clamp(time, 2f, 8f);
        }

        public static void StartSceneSequence(int id)
        {
            InScene = true;
            CurrentScene = id;
            CurrentAct = 0;
            sceneTimer = 0f;

            var nacts = ParamLoader.GetMarkers(MarkerType.Property, "scene", CurrentScene);
            acts = new ActInfo[nacts.Length];
            for (int i = 0; i < nacts.Length; i++)
            {
                acts[i] = new ActInfo(CurrentScene, i);
            }
        }

        public static void GeneralBlackout()
        {
            blackoutTimer = 20f;
            UIDesigner.ChangeUI(5);
        }
    }

    internal readonly struct ActInfo 
    {
        public float MaxTime { get; }
        public string TextPos { get; }
        public Vector2? CamPos { get; }
        public bool FreezePlayer { get; }
        public bool Thought { get; }
        public string LocName { get; }

        public ActInfo(int scene, int act)
        {
            //text position, freeze player, is a thought, camera position (/ = dont change), time (/ = auto text time)
            LocName = "scene" + scene + "Act" + act;
            var info = ParamLoader.GetParam<string>("scene", scene, "a" + act).Split(", ");

            TextPos = info[0];
            FreezePlayer = info[1] != "false";
            Thought = info[2] != "false";
            MaxTime = info[4] == "/" ? DialogueManager.CalculateTime(LocalizationManager.Dialogues[LocName].Length) : int.Parse(info[4]);

            if (!info[3].Contains('/'))
            {
                var pxy = info[3].Split('.');
                CamPos = new Vector2(float.Parse(pxy[0]), float.Parse(pxy[1]));
            }
            else
                CamPos = null;
        }
    }
}
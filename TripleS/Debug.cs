using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.Linq;
using TripleS.Scripting;
using TripleS.Physics;
using TripleS.Lighting;

namespace TripleS {

    /// <summary>
    /// Debug input console.
    /// </summary>
    public static class Debug
    {
        static string command;

        public static SpriteFont DebugFont { get; set; }
        public static bool Active { get; set; }
        public static Keys ToggleInput { get; set; }
        public static string LastCommand { get; private set; }
        public static int FontSize { get; set; }
        public static float FPS { get; private set; }
        public static Dictionary<string, object> OnScreenStats { get; set; }
        public static bool DrawNodegraph { get; set; }
        public static bool DrawNormals { get; set; }

        private static List<string> logs;
        private static List<string> prevCommands;
        public static event EventHandler<CommandEventArgs> OnCommand;
        private static int scrollIndex;
        private static int oldSV = 0;

        public static void OpenConsole()
        {
            Active = true;
            GameInputs.MaxInputs = 32;
            GameInputs.InputFilter = InputType.All;
            GameInputs.ToggleInputs(true);
            GameInputs.IncludeSpaces = true;
            command = "";
            prevCommands = new List<string>(6);
        }

        public static void CloseConsole()
        {
            Active = false;
            GameInputs.ToggleInputs(false);
        }

        public static void Load(ContentManager content, Keys toggle)
        {
            DebugFont = content.Load<SpriteFont>("font/DefaultFont");
            ToggleInput = toggle;
            FontSize = 24 / 12;
            OnScreenStats = new Dictionary<string, object>();
        }

        public static void Update(out bool pause)
        {
            pause = false;
            if (Active)
            {
                if (!GameInputs.TakingInput)
                    GameInputs.ToggleInputs(true);

                command = GameInputs.Input;

                if (GameInputs.OncePress(Keys.Enter) && command.Length > 0)
                {
                    LastCommand = command;
                    command = "";
                    GameInputs.ResetInput();
                    if(prevCommands.Count >= prevCommands.Capacity)
                        prevCommands.RemoveAt(0);
                    prevCommands.Add(":" + LastCommand + "\n");

                    var cmd = LastCommand.Split(' ');
                    CommandEventArgs args = new CommandEventArgs(cmd[0], cmd.Length > 1 ? cmd[1] : "");
                    OnCommand?.Invoke(null, args);
                }

                var scroll = GameInputs.GetMouse().ScrollWheelValue;
                var dif = scroll - oldSV;
                if (dif != 0)
                    dif = MathUtil.IntSign(dif);
                oldSV = scroll;
                scrollIndex = Math.Clamp(scrollIndex + dif, 0, (int)MathUtil.MinClamp(logs.Count - 20, 0));

                if (GameInputs.OncePress(ToggleInput) || GameInputs.OncePress(Keys.Escape))
                {
                    pause = true;
                    CloseConsole();
                }
            }
            else
            {
                if (GameInputs.OncePress(ToggleInput))
                {
                    Active = false;
                    pause = true;
                    OpenConsole();
                }
            }
        }

        public static void PrintToVS(object message)
        {
            System.Diagnostics.Debug.Print($"{message}");
        }

        public static void Log(object message, LogType type = LogType.Message)
        {
            logs ??= new List<string>(512);

            string logType = "m"; //m, i, l, w
            switch (type)
            {
                case LogType.Log:
                    logType = "l";
                    break;
                case LogType.Info:
                    logType = "i";
                    break;
                case LogType.Warning:
                    logType = "w";
                    break;
            }

            if (logs.Count >= logs.Capacity)
                logs.RemoveAt(0);
            logs.Add($"{logType}{message}");
        }

        private static float underlineTimer;

        public static void Draw(Renderer renderer, GameTime time)
        {
            var delta = (float)time.ElapsedGameTime.TotalSeconds;
            FPS = MathF.Ceiling(1f / delta);
            if (DebugFont != null)
            {
                Vector2 infoPos = renderer.View.Position + new Vector2(renderer.View.windowWidth - 240, 5);
                var zoom = (int)renderer.View.Zoom;
                if (Active)
                {
                    infoPos = renderer.View.Position + new Vector2(renderer.View.viewPortWidth * zoom / 2, 5);
                    renderer.BasicDraw(SSS.Square, new Rectangle((int)renderer.View.Position.X, (int)renderer.View.Position.Y, renderer.View.windowWidth, renderer.View.windowHeight), 1, 1, col: Color.Black * 0.6f);

                    var divPos = renderer.View.Position + new Vector2(0, renderer.View.viewPortHeight * zoom / 4 * 3);
                    renderer.BasicDraw(SSS.Square, new Rectangle((int)divPos.X, (int)divPos.Y, renderer.View.viewPortWidth * zoom, 3), 1, 1);

                    if (underlineTimer < 0.8f)
                        underlineTimer += delta;
                    else
                        underlineTimer = 0;
                    var underline = underlineTimer < 0.4f ? "_" : "";

                    renderer.DrawText(DebugFont, command + underline, divPos + (Vector2.One * 24), 1, 1, size: FontSize);

                    var list = string.Join("", prevCommands.ToArray().Reverse());
                    renderer.DrawText(DebugFont, list, divPos + new Vector2(24, 64), 1, 1, col: Color.Gray, size: FontSize);

                    int start = logs.Count > 20 ? logs.Count - 20 : 0;
                    start -= scrollIndex;
                    for(int i = start; i < logs.Count - scrollIndex; i++)
                    {
                        string log = logs[i];
                        var col = Color.White;
                        switch (log[0])
                        {
                            case 'l':
                                col = Color.Lime;
                                break;
                            case 'i':
                                col = Color.Cyan;
                                break;
                            case 'w':
                                col = Color.Yellow;
                                break;
                        }
                        renderer.DrawText(DebugFont, log.Substring(1), renderer.View.Position + new Vector2(24, 24 + ((i - start) * 30)), 1, 1, size: FontSize, col: col);
                    }
                }
                string stats = $"fps: {FPS}";
                foreach(KeyValuePair<string, object> stat in OnScreenStats)
                    stats += $"\n{stat.Key}: {stat.Value}";
                renderer.DrawText(DebugFont, stats, infoPos, 0, 0, col: Color.Cyan, size: FontSize / (Active ? 1 : 2));
            }
        }

        private static System.Diagnostics.Stopwatch watch;

        public static void StartLogTimer()
        {
            watch ??= new System.Diagnostics.Stopwatch();
            watch.Start();
        }

        public static void EndLogTimer(string stage)
        {
            watch.Stop();
            Log($"Took {watch.ElapsedMilliseconds}ms for {stage}", LogType.Info);
            watch.Reset();
        }

        public static void DrawVisuals(Renderer renderer)
        {
            if (DrawNodegraph)
            {
                foreach (NavNode node in NodegraphBuiler.Nodes)
                {
                    renderer.BasicDraw(SSS.Square, new Rectangle((int)node.Location.X - 2, (int)node.Location.Y - 2, 4, 4), 1, 1, col: Color.Lime);
                    foreach (int con in node.Connections)
                    {
                        Vector2 dest = NodegraphBuiler.Nodes[con].Location;
                        DrawLine(renderer, node.Location, dest, Color.Lime);
                    }
                }
            }
            if (DrawNormals)
            {
                foreach(Vert vert in LevelHandler.Verts)
                {
                    renderer.BasicDraw(SSS.Square, vert.Position, 1, 1, col: Color.Cyan);
                    renderer.BasicDraw(SSS.Square, new Rectangle((int)vert.Position.X, (int)vert.Position.Y - (vert.YNormal * 5), 1, vert.YNormal * 5), 1, 1, col: Color.Magenta);
                    renderer.BasicDraw(SSS.Square, new Rectangle((int)vert.Position.X - (vert.XNormal * 5), (int)vert.Position.Y, vert.XNormal * 5, 1), 1, 1, col: Color.Yellow);
                }
            }

            /*foreach (ShadowRay ray in renderer.Lighting.TestRays)
            {
                DrawLine(renderer, ray.Position, ray.End, Color.Lime);
            }*/
        }

        public static void SetOSS(string name, object value)
        {
            if (OnScreenStats.ContainsKey(name))
                OnScreenStats[name] = value;
            else
                OnScreenStats.Add(name, value);
        }

        public static void DrawLine(Renderer renderer, Vector2 start, Vector2 end, Color color)
        {
            Vector2 direction = MathUtil.SafeNomralize(end - start);
            for(int i = 0; i < Vector2.Distance(start, end); i++)
            {
                renderer.BasicDraw(SSS.Square, start + (direction * i), 1, 1, col: color);
            }
        }

        public static void DrawLine(Renderer renderer, Vector2 start, Vector2 dir, int length, Color color)
        {
            for (int i = 0; i < length; i++)
            {
                renderer.BasicDraw(SSS.Square, start + (dir * i), 1, 1, col: color);
            }
        }
    }

    public class CommandEventArgs : EventArgs
    {
        public string Command { get; set; }
        public string Argument { get; set; }

        public CommandEventArgs(string com, string arg)
        {
            Command = com;
            Argument = arg;
        }
    }

    public enum LogType
    {
        Message,
        Info,
        Log,
        Warning
    }
}

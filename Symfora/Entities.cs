using Microsoft.Xna.Framework;
using System;
using System.Linq;
using TripleS.Scripting;
using TripleS.Physics;
using TripleS.Tiled;
using TripleS;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Symfora {
    public class EntCatalyst : Entity {

        public bool Charged { get; set; }
        public bool Chargable { get; private set; }

        private bool blockEnter;
        private bool draw;
        private float cooldown;

        public EntCatalyst()
        {
            ID = "catalyst";
            DefaultProperties = new DefaultProp[4]
            {
                new DefaultProp("boostMult", TiledPropertyType.Float),
                new DefaultProp("charged", TiledPropertyType.Bool),
                new DefaultProp("draw", TiledPropertyType.Bool),
                new DefaultProp("startActive", TiledPropertyType.Bool)
            };
            Static = true;
            Point = false;
            StartActiveProperty = "startActive";
        }

        public override void Load(ContentManager content)
        {
            base.Load(content);
            Chargable = GetEntProp<bool>("charged");
            draw = GetEntProp<bool>("draw");
        }

        public override void Update(GameTime time)
        {
            base.Update(time);

            if (CollisionEngine.RectInRect(Core._Player.Transform.GetRectangle(), Bounds.Value) && (Chargable ? Charged : true))
            {
                if (!blockEnter && cooldown <= 0)
                {
                    cooldown = 1f;
                    Charged = false;
                    Core._Player.Boost(false, GetEntProp<float>("boostMult"));
                    blockEnter = true;
                }
            }
            else if (blockEnter)
                blockEnter = false;

            if (cooldown > 0)
                cooldown -= SSS.Delta;
        }

        public override void Draw(Renderer renderer)
        {
            base.Draw(renderer);
            if(draw)
                renderer.BasicDraw(SSS.Square, Bounds.Value, 1, 1);
        }
    }

    public class EntJetCatalyst : Entity {

        public bool Charged { get; set; }
        public bool Chargable { get; private set; }

        private bool blockEnter;
        private bool draw;

        public EntJetCatalyst()
        {
            ID = "catalyst_jet";
            DefaultProperties = new DefaultProp[3]
            {
                new DefaultProp("charged", TiledPropertyType.Bool),
                new DefaultProp("draw", TiledPropertyType.Bool),
                new DefaultProp("startActive", TiledPropertyType.Bool)
            };
            Static = true;
            Point = false;
            StartActiveProperty = "startActive";
        }

        public override void Load(ContentManager content)
        {
            base.Load(content);
            Chargable = GetEntProp<bool>("charged");
            draw = GetEntProp<bool>("draw");
        }

        public override void Update(GameTime time)
        {
            base.Update(time);

            if (CollisionEngine.RectInRect(Core._Player.Transform.GetRectangle(), Bounds.Value) && (Chargable ? Charged : true) && Active)
            {
                if (!blockEnter)
                {
                    Charged = false;
                    Core._Player.JetBoost(false);
                    blockEnter = true;
                }
            }
            else if (blockEnter)
                blockEnter = false;
        }

        public override void Draw(Renderer renderer)
        {
            base.Draw(renderer);
            if (draw)
                renderer.BasicDraw(SSS.Square, Bounds.Value, 1, 1, col: Color.Beige);
        }
    }

    public class EntDoor : Entity {

        private Collider collider;
        private bool draw;

        public EntDoor()
        {
            ID = "door";
            DefaultProperties = new DefaultProp[3]
            {
                new DefaultProp("draw", TiledPropertyType.Bool),
                new DefaultProp("skin", TiledPropertyType.Int),
                new DefaultProp("startActive", TiledPropertyType.Bool)
            };
            Static = true;
            Point = false;
            StartActiveProperty = "startActive";
        }

        public override void Load(ContentManager content)
        {
            base.Load(content);
            bool tall = Bounds.Value.Height > Bounds.Value.Width;
            collider = new Collider(new Rectangle((int)Position.X, (int)Position.Y, tall ? 16 : Bounds.Value.Width, tall ? Bounds.Value.Height : 16));
            if (Active)
                Core._Colliders.Add(collider);
            draw = GetEntProp<bool>("draw");
        }

        public override void Activate()
        {
            base.Activate();
            Core._Colliders.Add(collider);
        }

        public override void Deactivate()
        {
            base.Deactivate();
            Core._Colliders.Remove(collider);
        }

        public override void Draw(Renderer renderer)
        {
            base.Draw(renderer);
            if(draw && Active)
                renderer.BasicDraw(SSS.Square, Bounds.Value, 1, 1);
        }
    }

    public class EntCataCharger : Entity, IInteractableEnt
    {
        public bool DisableInteraction { get; set; }

        public EntCataCharger()
        {
            ID = "catalyst_charger";
            DefaultProperties = new DefaultProp[2]
            {
                new DefaultProp("target", TiledPropertyType.String),
                new DefaultProp("startActive", TiledPropertyType.Bool)
            };
            Static = true;
            Point = true;
            StartActiveProperty = "startActive";
        }

        public void OnInteraction() 
        {
            var target = Core._LevelHandler.EntityMan.GetEnt(GetEntProp<string>("target"));
            if(target != null)
            {
                if (target.ID == "catalyst")
                {
                    var trueTarget = (EntCatalyst)target;
                    trueTarget.Charged = true;
                }
                else if (target.ID == "catalyst_jet")
                {
                    var trueTarget = (EntJetCatalyst)target;
                    trueTarget.Charged = true;
                }
            }
        }

        public override void Draw(Renderer renderer)
        {
            renderer.BasicDraw(SSS.Square, new Rectangle((int)Centre.X - 4, (int)Centre.Y - 4, 8, 8), 1, 1, col: Color.CornflowerBlue);
        }
    }

    public class EntBlockade : Entity
    {
        public Collider Collider { get; private set; }
        private int type;
        private bool dead;
        private bool ballista;
        private Rectangle newBounds;

        public EntBlockade()
        {
            ID = "blockade";
            DefaultProperties = new DefaultProp[3]
            {
                new DefaultProp("skin", TiledPropertyType.Int),
                new DefaultProp("ballistaVunerable", TiledPropertyType.Bool),
                new DefaultProp("type", TiledPropertyType.Int)
            };
            Static = true;
            Point = false;
        }

        public override void Load(ContentManager content)
        {
            base.Load(content);
            Collider = new Collider(Bounds.Value);
            type = GetEntProp<int>("type");
            ballista = GetEntProp<bool>("ballistaVunerable");
            Core._Colliders.Add(Collider);
            newBounds = new Rectangle(Bounds.Value.X - 1, Bounds.Value.Y - 1, Bounds.Value.Width + 2, Bounds.Value.Height + 2);
        }

        public override void Draw(Renderer renderer)
        {
            if(!dead)
                renderer.BasicDraw(SSS.Square, Bounds.Value, 1, 1, col: Color.RosyBrown);
        }

        public override void Update(GameTime time)
        {
            base.Update(time);
            bool playerTouching = CollisionEngine.RectInRect(Core._Player.Transform.GetRectangle(), newBounds) && Core._Player.Ramming && (int)Core._Player.WeaponType == type;
            bool balTouching = false;
            if (ballista) {
                var closestProj = Core.GetClosestProjectile(Centre);               
                balTouching = closestProj != null && closestProj.ID == 6 && CollisionEngine.RectInRect(CollisionEngine.InflateRect(closestProj.Transform.GetRectangle(), 4), newBounds);
            }

            if (!dead && (playerTouching || balTouching))
                Destroy();
        }

        public void Destroy()
        {
            //CB 3: Add gibs
            dead = true;
            Core._Colliders.Remove(Collider);
            AudioSystem.PlayEvent("dodgeCollide", true, Centre, true);
        }
    }

    public class EntSwitch : Entity, IInteractableEnt
    {
        public bool DisableInteraction { get; private set; }
        public bool Switched { get; private set; }
        private bool draw;
        private bool once;

        public EntSwitch()
        {
            ID = "switch";
            DefaultProperties = new DefaultProp[4]
            {
                new DefaultProp("once", TiledPropertyType.Bool),
                new DefaultProp("draw", TiledPropertyType.Bool),
                new DefaultProp("skin", TiledPropertyType.Int),
                new DefaultProp("startActive", TiledPropertyType.Bool)
            };
            Static = true;
            Point = true;
            StartActiveProperty = "startActive";
        }

        public override void Load(ContentManager content)
        {
            base.Load(content);
            draw = GetEntProp<bool>("draw");
            DisableInteraction = !Active;
        }

        public void OnInteraction()
        {
            if (!once)
            {
                Switched = !Switched;
                if (GetEntProp<bool>("once"))
                {
                    DisableInteraction = true;
                    once = true;
                }

                if(Name != "")
                {
                    object[] args = new object[1] { Switched };
                    ScriptManager.ExecuteFunction("switch_" + Name, args);
                }
            }
        }

        public override void Draw(Renderer renderer)
        {
            if (draw)
            {
                Color col = Active ? (Switched ? Color.Lime : Color.Red) : Color.Black;
                renderer.BasicDraw(SSS.Square, new Rectangle((int)Centre.X - 4, (int)Centre.Y - 4, 8, 8), 1, 1, col: col);
            }
        }

        public override void Activate()
        {
            base.Activate();
            DisableInteraction = false;
        }
        public override void Deactivate()
        {
            base.Activate();
            DisableInteraction = true;
        }
    }

    public class EntTimer : Entity, IInteractableEnt {
        public bool DisableInteraction { get; private set; }
        public float Time { get; private set; }
        private bool draw;
        private float maxTime;

        public EntTimer()
        {
            ID = "timer";
            DefaultProperties = new DefaultProp[4]
            {
                new DefaultProp("time", TiledPropertyType.Int),
                new DefaultProp("draw", TiledPropertyType.Bool),
                new DefaultProp("skin", TiledPropertyType.Int),
                new DefaultProp("startActive", TiledPropertyType.Bool)
            };
            Static = true;
            Point = true;
            StartActiveProperty = "startActive";
            Time = -1000;
        }

        public override void Load(ContentManager content)
        {
            base.Load(content);
            draw = GetEntProp<bool>("draw");
            maxTime = GetEntProp<int>("time");
        }

        public override void Update(GameTime time)
        {
            DisableInteraction = !Active;
            base.Update(time);
            if (Time > 0)
            {
                Time -= SSS.Delta;
                int next = (int)MathF.Floor(Time);
                if(Time >= next && Time <= next + SSS.Delta)
                    AudioSystem.PlayEvent("timer", true, Position, true);
            }
            else if(Time != -1000)
            {
                Time = -1000;
                DisableInteraction = false;
                ScriptMethod(false);
            }
        }

        public void OnInteraction()
        {
            if(Time <= 0)
            {
                DisableInteraction = true;
                AudioSystem.PlayEvent("timer", true, Position, true);
                Time = maxTime;
                ScriptMethod(true);
            }
        }

        private void ScriptMethod(bool state)
        {
            if (Name != "")
            {
                object[] args = new object[1] { state };
                ScriptManager.ExecuteFunction("timer_" + Name, args);
            }
        }

        public override void Draw(Renderer renderer)
        {
            if (draw)
            {
                Color color = Time > 0 ? Color.Red * (Time / maxTime) : Color.Red;
                renderer.BasicDraw(SSS.Square, new Rectangle((int)Centre.X - 4, (int)Centre.Y - 4, 8, 8), 1, 1, col: color);
            }
        }
    }

    public class EntLadder : Entity, IInteractableEnt {

        private bool draw;
        public bool DisableInteraction { get; private set; }

        public EntLadder()
        {
            ID = "ladder";
            DefaultProperties = new DefaultProp[3]
            {
                new DefaultProp("draw", TiledPropertyType.Bool),
                new DefaultProp("skin", TiledPropertyType.Int),
                new DefaultProp("startActive", TiledPropertyType.Bool)
            };
            Static = true;
            Point = false;
            StartActiveProperty = "startActive";
        }

        public override void Load(ContentManager content)
        {
            base.Load(content);
            draw = GetEntProp<bool>("draw");
        }

        public void OnInteraction()
        {
            Core._Player.MountLadder(Bounds.Value);
        }

        public override void Draw(Renderer renderer)
        {
            base.Draw(renderer);
            if (draw && Active)
                renderer.BasicDraw(SSS.Square, Bounds.Value, 1, 1, col: Color.Beige);
        }
    }

    public class EntWire : Entity
    {
        private Texture2D wireTexture;
        private Entity target;
        private bool interactTarget;

        public EntWire()
        {
            ID = "wireOverlay";
            DefaultProperties = new DefaultProp[1]
            {
                new DefaultProp("target", TiledPropertyType.String)
            };
            Static = true;
            Point = false;
            DrawLayer = 1;
        }

        public override void Load(ContentManager content)
        {
            base.Load(content);
            wireTexture = SSS.Square;

            target = Core._LevelHandler.EntityMan.GetEnt(GetEntProp<string>("target"));
            interactTarget = typeof(IInteractableEnt).IsAssignableFrom(target.GetType());
        }

        public override void Draw(Renderer renderer)
        {
            base.Draw(renderer);
            Color color = Color.Cyan;

            bool targetBool = target.Active;
            bool nonInteractEx = false;
            if (target.ID == "magnet")
            {
                var trt = (EntMagnet)target;
                targetBool = trt.Holding;
            }
            else if (target.ID == "timer")
            {
                var trt = (EntTimer)target;
                targetBool = trt.Time > 0;
                nonInteractEx = true;
            }
            else if (target.ID == "switch")
            {
                var trt = (EntSwitch)target;
                targetBool = trt.Switched;
                nonInteractEx = true;
            }
            else if (target.ID == "catalyst")
            {
                var trt = (EntCatalyst)target;
                targetBool = trt.Charged;
                nonInteractEx = true;
            }
            else if (target.ID == "catalyst_jet")
            {
                var trt = (EntJetCatalyst)target;
                targetBool = trt.Charged;
                nonInteractEx = true;
            }

            if (((!interactTarget || nonInteractEx) && targetBool) || (!nonInteractEx && interactTarget && Core._Player.LastInteractEnt == target.UUID))
                color = Color.Orange;

            if (Polyigonal.HasValue)
            {
                for (int i = 0; i < Polyigonal.Value.Points.Length - 1; i++)
                {
                    Debug.DrawLine(renderer, Polyigonal.Value.Points[i], Polyigonal.Value.Points[i + 1], color: color);
                }
            }
        }
    }

    public class EntNPC : Entity {

        public UUID npcUUID { get; private set; }

        public EntNPC()
        {
            ID = "npc";
            DefaultProperties = new DefaultProp[5]
            {
                new DefaultProp("id", TiledPropertyType.Int),
                new DefaultProp("noticeDist", TiledPropertyType.Int),
                new DefaultProp("phantom", TiledPropertyType.Int),
                new DefaultProp("perma", TiledPropertyType.Bool),
                new DefaultProp("wanderDist", TiledPropertyType.Int)
            };
            Static = true;
            Point = true;
        }

        public override void Load(ContentManager content)
        {
            base.Load(content);

            int nextOpenIndex = -1;
            int apUUID = (LevelHandler.CurrentLevel * 10000) + UUID;
            bool permaDeathed = false;
            if (GetEntProp<bool>("perma"))
            {
                for(int i = 200; i < 300; i++)
                {
                    if (!GameStateManager.States.ContainsKey(i))
                    {
                        nextOpenIndex = i;
                        break;
                    }
                    else if (GameStateManager.States[i] == apUUID)
                    {
                        permaDeathed = true;
                        break;
                    }
                }
            }

            if (!permaDeathed)
            {
                var pt = (PhantomType)GetEntProp<int>("phantom");
                NPC npc = new NPC(GetEntProp<int>("id"), GetEntProp<int>("noticeDist"), GetEntProp<int>("wanderDist"), pt);
                Core.SpawnNPC(npc, Position, Name, nextOpenIndex, apUUID);
                npcUUID = npc.UUID;
            }
        }
    }

    public class EntMagnet : Entity
    {
        public bool Holding { get; private set; }
        private bool draw;
        private bool direction;
        private bool flag;
        private int size;

        public EntMagnet()
        {
            ID = "magnet";
            DefaultProperties = new DefaultProp[4]
            {
                new DefaultProp("size", TiledPropertyType.Int),
                new DefaultProp("startActive", TiledPropertyType.Bool),
                new DefaultProp("draw", TiledPropertyType.Bool),
                new DefaultProp("direction", TiledPropertyType.Bool)
            };
            Static = true;
            Point = true;
            StartActiveProperty = "startActive";
        }

        public override void Load(ContentManager content)
        {
            base.Load(content);
            direction = GetEntProp<bool>("direction");
            draw = GetEntProp<bool>("draw");
            size = GetEntProp<int>("size");
            flag = true;
        }

        public override void Update(GameTime time)
        {
            base.Update(time);
            Holding = false;
            foreach(NPC npc in Core._NPCs.Where(x => x.Corpse && x.Size == size).ToArray())
            {
                float dist = Vector2.Distance(npc.Transform.Centre, Position);
                bool cond = npc.UUID == Core._Player.CurrentCorpse && Core._Player.HoldingCorpse;
                if (!cond && dist < 32)
                {
                    if (npc.Transform.Gravity)
                        npc.Transform.Gravity = false;
                    Vector2 dest = new Vector2(Position.X + (direction ? -npc.Transform.Width : 0), Position.Y - npc.Transform.Height / 2);
                    npc.Transform.Position = Vector2.Lerp(npc.Transform.Position, dest, 0.2f);
                    Holding = true;
                    if (flag)
                    {
                        flag = false;
                        ExecuteFunction();
                    }
                }
            }
            if(!flag && !Holding)
            {
                flag = true;
                ExecuteFunction();
            }
        }

        private void ExecuteFunction()
        {
            if (Name != "")
            {
                object[] args = new object[1] { Holding };
                ScriptManager.ExecuteFunction("magnet_" + Name, args);
            }
        }

        public override void Draw(Renderer renderer)
        {
            base.Draw(renderer);
            if (draw)
            {
                Color color = Holding ? Color.Crimson : Color.White;
                renderer.BasicDraw(SSS.Square, new Rectangle((int)Position.X - 4, (int)Position.Y - 4, 8, 8), 3, 1, col: color);
            }
        }
    }

    public class EntBallista : Entity 
    {
        private Vector2 direction;
        private bool draw;

        public EntBallista()
        {
            ID = "ballista";
            DefaultProperties = new DefaultProp[4]
            {
                new DefaultProp("speed", TiledPropertyType.Float),
                new DefaultProp("startActive", TiledPropertyType.Bool),
                new DefaultProp("draw", TiledPropertyType.Bool),
                new DefaultProp("direction", TiledPropertyType.Int)
            };
            Static = true;
            Point = true;
            StartActiveProperty = "startActive";
        }

        public override void Load(ContentManager content)
        {
            base.Load(content);
            float angle = MathHelper.ToRadians(GetEntProp<int>("direction"));
            direction = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            draw = GetEntProp<bool>("draw");
        }

        public void Shoot()
        {
            if (Active)
            {
                Projectile proj = new Projectile(6, null);
                Core.SpawnProjectile(proj, Position, direction * GetEntProp<float>("speed"));
            }
        }

        public override void Draw(Renderer renderer)
        {
            base.Draw(renderer);
            if (draw)
            {
                renderer.BasicDraw(SSS.Square, new Rectangle((int)Position.X - 4, (int)Position.Y - 4, 8, 20), 1, 1, col: Color.IndianRed);
            }
        }
    }

    public class EntSpiralDoor : Entity, IInteractableEnt
    {
        public bool DisableInteraction { get; private set; }
        float timer = -100f;
        bool toSoanoHub;
        bool teleport;

        public EntSpiralDoor()
        {
            ID = "spiralDoor";
            DefaultProperties = new DefaultProp[2]
            {
                new DefaultProp("teleportTarget", TiledPropertyType.String),
                new DefaultProp("startActive", TiledPropertyType.Bool)
            };
            Static = true;
            Point = false;
            StartActiveProperty = "startActive";
        }

        public override void Load(ContentManager content)
        {
            base.Load(content);
            toSoanoHub = LevelHandler.CurrentLevel != 21;
            teleport = GetEntProp<string>("teleportTarget") != "";
        }

        public override void Update(GameTime time)
        {
            if (timer > 0f)
                timer -= SSS.Delta * 1.5f;
            else if (timer > -SSS.Delta)
            {
                if (!teleport)
                {
                    Core._Player.AddSelfBoost(Core._Player.MaxSelfBoosts);
                    Core._Player.Health = Core._Player.MaxHealth;
                    if (toSoanoHub)
                    {
                        GameStateManager.States[33] = Position.X + ((Bounds.Value.Width - Core._Player.Transform.Width) / 2);
                        GameStateManager.States[34] = Position.Y + (Bounds.Value.Height - Core._Player.Transform.Height);
                        Core.SaveGame(true);

                        Core.LoadLevel(21, "startDoor");
                    }
                    else
                    {
                        Core.SaveGame(false);
                        Core.LoadLevel((int)GameStateManager.States[1]);
                    }
                }
                else
                {
                    DisableInteraction = false;
                    var targetP = Core._LevelHandler.EntityMan.GetEnt(GetEntProp<string>("teleportTarget"));
                    Core._Player.Transform.Velocity = Vector2.Zero;
                    Core._Player.Transform.OldVelocity = Vector2.Zero;
                    if (targetP.Bounds.HasValue)
                    {
                        Core._Player.Transform.Position = new Vector2(targetP.Position.X + (targetP.Bounds.Value.Width / 2) - (Core._Player.Transform.Width / 2), targetP.Position.Y + targetP.Bounds.Value.Height - Core._Player.Transform.Height);
                       Core._Player.Transform.ResetOldPosition();
                    }
                    else
                        Core._Player.Transform.Position = new Vector2(targetP.Position.X + (Core._Player.Transform.Width / 2), targetP.Position.Y + (Core._Player.Transform.Height / 2));
                    timer = -100f;
                }
                UIDesigner.ChangeUI(6);
            }
            base.Update(time);
        }

        public void OnInteraction()
        {
            UIDesigner.ChangeUI(5);
            timer = 1f;
            DisableInteraction = true;
        }

        public override void Draw(Renderer renderer)
        {
            renderer.BasicDraw(SSS.Square, new Rectangle((int)Position.X, (int)Position.Y, 16, 32), 1, 1, col: new Color(0.192f, 0.149f, 0.254f));
            base.Draw(renderer);
        }
    }

    public class EntTrigger : Entity {

        bool[] props;
        bool onceHit;
        bool subHit;

        public EntTrigger()
        {
            ID = "trigger";
            DefaultProperties = new DefaultProp[5]
            {
                new DefaultProp("startActive", TiledPropertyType.Bool),
                new DefaultProp("once", TiledPropertyType.Bool),
                new DefaultProp("player", TiledPropertyType.Bool),
                new DefaultProp("npcs", TiledPropertyType.Bool),
                new DefaultProp("corpses", TiledPropertyType.Bool)
            };
            Static = true;
            Point = false;
            StartActiveProperty = "startActive";
        }

        public override void Load(ContentManager content)
        {
            base.Load(content);
            props = new bool[4] { GetEntProp<bool>("once"), GetEntProp<bool>("player"), GetEntProp<bool>("npcs"), GetEntProp<bool>("corpses") };
        }

        public override void Update(GameTime time)
        {
            base.Update(time);
            if (!onceHit && Active && Name != "")
            {
                if (props[1])
                {
                    bool collide = CollisionEngine.RectInRect(Bounds.Value, Core._Player.Transform.GetRectangle());
                    if (collide && !subHit)
                        Trigger();
                    else if (!collide)
                        subHit = false;
                }
                if (props[2] || props[3])
                {
                    foreach(NPC npc in Core._NPCs)
                    {
                        if ((props[3] && npc.Corpse) || (props[2] && !npc.Corpse))
                        {
                            bool collide = CollisionEngine.RectInRect(Bounds.Value, npc.Transform.GetRectangle());
                            if (collide && !subHit)
                                Trigger();
                            else if (!collide)
                                subHit = false;
                        }
                    }
                }
            }
        }

        private void Trigger()
        {
            subHit = true;
            onceHit = props[0];
            ScriptManager.ExecuteFunction("trigger_" + Name, new string[0]);
        }
    }

    public class EntTriggerInput : Entity {

        string input;
        bool solved;
        bool hitFlag;

        public EntTriggerInput()
        {
            ID = "trigger_keyInput";
            DefaultProperties = new DefaultProp[2]
            {
                new DefaultProp("startActive", TiledPropertyType.Bool),
                new DefaultProp("input", TiledPropertyType.String)
            };
            Static = true;
            Point = false;
            StartActiveProperty = "startActive";
        }

        public override void Load(ContentManager content)
        {
            base.Load(content);
            input = GetEntProp<string>("input");
        }

        public override void Update(GameTime time)
        {
            base.Update(time);
            if (Active && Name != "" && !solved)
            {
                bool inside = CollisionEngine.RectInRect(Core._Player.Transform.GetRectangle(), Bounds.Value);
                if (inside)
                {
                    if (!hitFlag)
                    {
                        GameInputs.InputFilter = InputType.Alphabet;
                        GameInputs.ToggleInputs(true);
                        GameInputs.MaxInputs = 100;
                    }
                    hitFlag = true;
                    if (GameInputs.Input.Length >= GameInputs.MaxInputs)
                        GameInputs.ResetInput();
                    if(GameInputs.Input.ToLower().Contains(input))
                    {
                        ScriptManager.ExecuteFunction("keyInput_" + Name, new string[0]);
                        solved = true;
                        GameInputs.ToggleInputs(false);
                    }
                }
                else if (hitFlag) 
                {
                    hitFlag = false;
                    GameInputs.ToggleInputs(false);
                }
            }
        }
    }

    public class EntSinkpit : Entity
    {
        bool draw;
        int skin;
        int[] kin;
        int maxKin;
        List<NPC> lastKin;
        bool kinActive;

        public EntSinkpit()
        {
            ID = "sinkpit";
            DefaultProperties = new DefaultProp[4]
            {
                new DefaultProp("draw", TiledPropertyType.Bool),
                new DefaultProp("count", TiledPropertyType.Int),
                new DefaultProp("maxAttacking", TiledPropertyType.Int),
                new DefaultProp("skin", TiledPropertyType.Int)
            };
            Static = true;
            Point = false;
        }

        public override void Load(ContentManager content)
        {
            base.Load(content);
            draw = GetEntProp<bool>("draw");
            maxKin = GetEntProp<int>("maxAttacking");
            kin = new int[GetEntProp<int>("count")];
            for (int i = 0; i < kin.Length; i++)
            {
                //CB 7
                NPC npc = new NPC(3, 0, 0, PhantomType.None);
                kin[i] = npc.UUID.MainUUID;
                Core.SpawnNPC(npc, Centre);
            }
        }

        public override void Update(GameTime time)
        {
            base.Update(time);
            if (Core._Player.Transform.Position.X < Bounds.Value.Right && Core._Player.Transform.Position.X > Bounds.Value.Left
                && Core._Player.Transform.Position.Y + Core._Player.Transform.Height + 1 > Bounds.Value.Top && Core._Player.Transform.Position.Y < Bounds.Value.Top)
            {
                if (!kinActive)
                {
                    kinActive = true;
                    lastKin = new List<NPC>(maxKin);
                    for (int i = 0; i < kin.Length; i++)
                    {
                        var p = Core._NPCs.Where(x => x.UUID.MainUUID == kin[i]);
                        if (p.Count() > 0)
                        {
                            NPC target = p.First();
                            if (target.Corpse)
                                continue;
                            else
                            {
                                target.ChangeAIState(0, 1);
                                target.Transform.ChangeVelocityY(-target.Speed * 2);
                                int side = Random.Shared.Next(0, 10) <= 5 ? -1 : 1;
                                target.Transform.Position = new Vector2(Core._Player.Transform.Centre.X + (Core._Player.Transform.Width / 2 * side), Bounds.Value.Top + target.Transform.Height + 1);
                                lastKin.Add(target);
                                if (i >= maxKin - 1)
                                    break;
                            }
                        }
                    }
                }
            }
            
            if (kinActive)
            {
                int counter = 0;
                foreach(NPC npc in lastKin)
                {
                    if (npc.GetAIState(0) != 0 && !npc.Corpse)
                        counter++;
                }
                if(counter <= 0)
                    kinActive = false;
            }
        }

        public override void Draw(Renderer renderer)
        {
            base.Draw(renderer);
            if(draw)
                renderer.BasicDraw(SSS.Square, Bounds.Value, 1, 1, col: Color.SandyBrown);
        }
    }

    public class EntTransition : Entity {

        bool start;
        bool tied;
        bool tieFlag;
        float timer = -100f;
        string target;

        public EntTransition()
        {
            ID = "transition";
            DefaultProperties = new DefaultProp[3]
            {
                new DefaultProp("targetLevel", TiledPropertyType.Int),
                new DefaultProp("endTarget", TiledPropertyType.String),
                new DefaultProp("start", TiledPropertyType.Bool)
            };
            Static = true;
            Point = false;
            Active = true;
        }

        public override void Load(ContentManager content)
        {
            base.Load(content);
            start = GetEntProp<bool>("start");
            tied = start && Bounds.Value.Width < Bounds.Value.Height;
            if (tied)
                start = false;
            target = GetEntProp<string>("endTarget");
        }

        public override void Update(GameTime time)
        {
            base.Update(time);
            if (Active && target != null && target != "")
            {
                if (!start && timer <= -100f)
                {
                    if (CollisionEngine.RectInRect(Bounds.Value, Core._Player.Transform.GetRectangle()))
                    {
                        bool cond = tied ? tieFlag : true;
                        if (cond)
                        {
                            UIDesigner.ChangeUI(5);
                            Core._Player.Invincible = true;
                            timer = 1f;
                            if (tied)
                                Core._Player.Frozen = true;
                        }
                    }
                    else
                        tieFlag = true;
                }

                if (timer > 0f)
                    timer -= SSS.Delta * 1.5f;
                else if (timer > -SSS.Delta)
                {
                    Core.LoadLevel(GetEntProp<int>("targetLevel"), GetEntProp<string>("endTarget"));
                    UIDesigner.ChangeUI(6);
                }
            }
        }
    }

    public class EntSoanoHead : Entity, IInteractableEnt {
        public bool DisableInteraction { get; private set; }
        public int TallyID { get; private set; }

        public EntSoanoHead()
        {
            ID = "soanoHead";
            DefaultProperties = new DefaultProp[1]
            {
                new DefaultProp("firstTallyID", TiledPropertyType.Int)
            };
            Static = true;
            Point = false;
            Active = true;
        }

        public override void Load(ContentManager content)
        {
            base.Load(content);
            int index = GetEntProp<int>("firstTallyID");
            bool canGo = !PhantomManager.Tallies[index].Contains(true);
            if ((GameStateManager.States.ContainsKey(58 + index) && GameStateManager.States[58 + index] == 1) || canGo)
            {
                DisableInteraction = true;
            }
        }

        public void OnInteraction()
        {
            int index = GetEntProp<int>("firstTallyID");
            int trueTallies = PhantomManager.Tallies[index].Where(x => x).Count();

            PhantomManager.LastSoanoHeadTallyID = index;
            UIDesigner.upgradePoints = trueTallies;
            UIDesigner.Load(true);
            UIDesigner.ChangeMenu("upgrade");

            DisableInteraction = true;
            if (trueTallies >= PhantomManager.Tallies[index].Length)
                GameStateManager.States[58 + index] = 1;
        }

        float sin;

        public override void Draw(Renderer renderer)
        {
            base.Draw(renderer);
            sin += SSS.Delta;
            if (sin >= MathHelper.ToRadians(360))
                sin = 0;
            renderer.BasicDraw(SSS.Square, new Rectangle((int)Position.X, (int)Position.Y + (int)(MathF.Sin(sin) * 8), 54, 54), 1, 1, col: Color.SlateGray);
        }
    }

    public class EntHubItem : Entity, IInteractableEnt {
        public bool DisableInteraction { get; private set; }

        public EntHubItem()
        {
            ID = "hubItem";
            DefaultProperties = new DefaultProp[1]
            {
                new DefaultProp("type", TiledPropertyType.Int)
            };
            Static = true;
            Point = true;
            Active = true;
        }

        public override void Update(GameTime time)
        {
            base.Update(time);
            if(DisableInteraction && UIDesigner.CurrentMenu == "game")
            {
                DisableInteraction = false;
            }
        }

        public void OnInteraction()
        {
            int type = GetEntProp<int>("type");
            if (type != 2)
            {
                Core._Player.Frozen = true;
                DisableInteraction = true;
            }

            if (type == 0)
            {
                UIDesigner.Load(true);
                UIDesigner.ChangeMenu("library");
            }
            else if (type == 1)
                UIDesigner.ChangeMenu("char");
            else if(type == 2)
            {
                int t = (int)Core._Player.WeaponType + 1;
                if (t >= 3)
                    t = 0;
                Core._Player.ChangeWeaponType((WeaponTypes)Enum.ToObject(typeof(WeaponTypes), t));
            }
        }
    }

    public class EntCollider : Entity {

        Collider referance;

        public EntCollider()
        {
            ID = "collider";
            DefaultProperties = new DefaultProp[1]
            {
                new DefaultProp("tags", TiledPropertyType.String)
            };
            Static = true;
            Point = false;
            Active = true;
        }

        public override void Load(ContentManager content)
        {
            base.Load(content);
            referance = new Collider(Bounds.Value);
            if(GetEntProp<string>("tags") != "")
                referance.Tags = GetEntProp<string>("tags").Split(", ");
            Core._Colliders.Add(referance);
        }

        public override void Deactivate()
        {
            base.Deactivate();
            Core._Colliders.Remove(referance);
        }

        public override void Activate()
        {
            base.Activate();
            Core._Colliders.Add(referance);
        }
    }

    public class EntRope : Entity {

        public EntRope()
        {
            ID = "rope";
            DefaultProperties = new DefaultProp[4]            
            {
                new DefaultProp("drawStretched", TiledPropertyType.Bool),
                new DefaultProp("width", TiledPropertyType.Float),
                new DefaultProp("segments", TiledPropertyType.Int),
                new DefaultProp("texture", TiledPropertyType.String)

            };
            Static = true;
            Point = false;
            Active = true;
        }

        public override void Load(ContentManager content)
        {
            base.Load(content);

            Texture2D[] textures;
            var sProp = GetEntProp<string>("texture").Split(',');
            if (sProp.Length > 0 && sProp[0] != "")
            {
                textures = new Texture2D[sProp.Length];
                for(int i = 0; i < sProp.Length; i++)
                    textures[i] = AssetManager.GetDynamicAsset(sProp[i]);
            }
            else
                textures = new Texture2D[1] { SSS.Square }; 

            int entHeight = (int)Polyigonal.Value.Points[1].Y - (int)Polyigonal.Value.Points[0].Y;
            float length = GetEntProp<bool>("drawStretched") ? entHeight / GetEntProp<int>("segments") : textures[0].Height;
            RopeEngine.AddRope(GetEntProp<int>("segments"), length, GetEntProp<float>("width"), GetEntProp<bool>("drawStretched"), Position, textures);
        }
    }

    public class EntProp : Entity {

        private Texture2D texture;
        private float rotMult;
        private float rotation;
        private float startRot;
        private float velocity;
        private bool noMove;
        private float goofyRandom;
        private bool dead;

        public EntProp()
        {
            ID = "prop";
            DefaultProperties = new DefaultProp[6]
            {
                new DefaultProp("static", TiledPropertyType.Bool),
                new DefaultProp("rotationMult", TiledPropertyType.Float),
                new DefaultProp("startRotation", TiledPropertyType.Int),
                new DefaultProp("texture", TiledPropertyType.String),
                new DefaultProp("squib", TiledPropertyType.Int),
                new DefaultProp("squibSE", TiledPropertyType.String)
            };
            Static = true;
            Point = false;
            Active = true;
        }

        public override void Load(ContentManager content)
        {
            base.Load(content);

            string texPath = GetEntProp<string>("texture");
            if (texPath != "")
            {
                texture = AssetManager.GetDynamicAsset(texPath);
                rotMult = GetEntProp<float>("rotationMult");
                startRot = MathHelper.ToRadians(GetEntProp<int>("startRotation"));
                rotation = startRot;
                noMove = GetEntProp<bool>("static");
                var r = new Random((int)(Position.X + Position.Y) / 2);
                goofyRandom = r.Next(0, 50);
            }
        }

        float timer;
        float windTimer;
        float appliedWind;

        public override void Update(GameTime time)
        {
            base.Update(time);
            if(texture != null && !dead)
            {
                bool blowUp = false;
                if (!noMove)
                {
                    if (RopeEngine.EnableWind)
                    {
                        windTimer += SSS.Delta * RopeEngine.WindSpeed;
                        if (windTimer > MathF.PI * 2)
                            windTimer = 0;
                        appliedWind = (MathF.Sin(windTimer + goofyRandom) * 3f * 0.5f + 0.5f) / RopeEngine.WindAmp + 0.05f + (RopeEngine.WindAmp / 120);
                    }

                    if (velocity != 0f)
                    {
                        if (timer > 0)
                            timer -= SSS.Delta;

                        velocity = MathHelper.Lerp(velocity, MathUtil.IntSign(startRot - rotation) * 6, 0.035f) * rotMult;
                        if (MathF.Abs(velocity - (startRot - rotation)) <= 0.2f && timer <= 0f)
                        {
                            velocity = 0f;
                            timer = 0f;
                        }
                    }
                    rotation += MathHelper.ToRadians(velocity);

                    Vector2 target = Transform.RotateAroundAPoint(Position, MathHelper.ToDegrees(rotation + MathF.PI), texture.Height / 2);
                    if (CollisionEngine.PointInRect(Core._Player.Transform.GetRectangle(), target) && MathF.Abs(Core._Player.Transform.Velocity.X) > 0.05f)
                    {
                        velocity = -Core._Player.Transform.Velocity.X;
                        timer = MathF.Abs(velocity);
                        blowUp = true;
                    }
                }
                else
                    blowUp = CollisionEngine.PointInRect(Core._Player.Transform.GetRectangle(), Position + new Vector2(texture.Width / 2, texture.Height / 2));

                if (blowUp)
                {
                    int squibID = GetEntProp<int>("squib");
                    if (squibID != -1)
                    {
                        int count = (texture.Width + texture.Height) / 8;
                        ParticleManager.AddParticles(squibID, Position, new Vector2(0.3f, -0.05f), 1, count, 4, 4);
                    }
                    string squibSE = GetEntProp<string>("squibSE");
                    if(squibSE != "")
                        AudioSystem.PlayEvent(squibSE, true, Position, true);
                    dead = squibID != -1 || squibSE != "";
                }
            }
        }

        public override void Draw(Renderer renderer)
        {
            base.Draw(renderer);
            if (texture != null && !dead)
            {
                if (noMove)
                    renderer.BasicDraw(texture, Position, 1, 1, rot: startRot);
                else
                    renderer.Batch.Draw(texture, Position, null, Color.White, rotation + appliedWind, new Vector2(texture.Width / 2, 0), new Vector2(1, 1), SpriteEffects.None, 0);
            }
        }
    }

    public class EntHouseDoor : Entity, IInteractableEnt {
        public bool DisableInteraction { get; private set; }
        float timer = -100f;
        bool toInside;

        public EntHouseDoor()
        {
            ID = "houseDoor";
            DefaultProperties = new DefaultProp[1]
            {
                new DefaultProp("goesInside", TiledPropertyType.Bool)
            };
            Static = true;
            Point = false;
            Active = true;
        }

        public override void Load(ContentManager content)
        {
            base.Load(content);
            toInside = GetEntProp<bool>("goesInside");
        }

        public override void Update(GameTime time)
        {
            if (timer > 0f)
                timer -= SSS.Delta * 1.5f;
            else if (timer > -SSS.Delta)
            {
                Core._Player.AddSelfBoost(Core._Player.MaxSelfBoosts);
                Core._Player.Health = Core._Player.MaxHealth;
                if (toInside)
                {
                    GameStateManager.States[33] = Position.X + ((Bounds.Value.Width - Core._Player.Transform.Width) / 2);
                    GameStateManager.States[34] = Position.Y + (Bounds.Value.Height - Core._Player.Transform.Height);
                    Core.SaveGame(true);

                    Core.LoadLevel(4, "startDoor");
                }
                else
                {
                    Core.SaveGame(false);
                    Core.LoadLevel(3);
                }
                UIDesigner.ChangeUI(6);
            }
            base.Update(time);
        }

        public void OnInteraction()
        {
            UIDesigner.ChangeUI(5);
            timer = 1f;
            DisableInteraction = true;
        }

        public override void Draw(Renderer renderer)
        {
            renderer.BasicDraw(SSS.Square, new Rectangle((int)Position.X, (int)Position.Y, 16, 32), 1, 1, col: Color.Gold);
            base.Draw(renderer);
        }
    }

    public class EntAdvancedScene : Entity {

        int[] exeActs;
        int scene;
        int past;

        public EntAdvancedScene()
        {
            ID = "sceneBrain";
            DefaultProperties = new DefaultProp[2]
            {
                new DefaultProp("scene", TiledPropertyType.Int),
                new DefaultProp("acts", TiledPropertyType.String)
            };
            Static = true;
            Point = false;
            Active = true;
        }

        public override void Load(ContentManager content)
        {
            base.Load(content);
            var pre = GetEntProp<string>("acts").Split(',');
            exeActs = new int[pre.Length];
            for (int i = 0; i < pre.Length; i++)
                exeActs[i] = int.Parse(pre[i].Trim());
            scene = GetEntProp<int>("scene");
        }

        public override void Update(GameTime time)
        {
            base.Update(time);
            if (DialogueManager.InScene && DialogueManager.CurrentScene == scene) 
            {
                int ri = -1;
                for (int i = 0; i < exeActs.Length; i++) 
                {
                    if (exeActs[i] == DialogueManager.CurrentAct)
                        ri = i;
                }

                if (ri != -1 && past != ri)
                    ScriptManager.ExecuteFunction($"scene{scene}_act{DialogueManager.CurrentAct}", Array.Empty<object>());
                past = ri;
            }
        }
    }
}
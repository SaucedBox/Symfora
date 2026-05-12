using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TripleS;
using TripleS.Physics;
using TripleS.Scripting;
using TripleS.UI;

namespace Symfora {
    public class Player {

        public Transform Transform { get; set; }
        public bool Frozen { get; set; }
        public int Health { get; set; }
        public float Speed { get; set; }
        public float AccelSpeed { get; set; }
        public float DeaccelSpeed { get; set; }
        public float KnockbackRes { get; set; }
        public float RamDeaccel { get; set; }
        public float RamAmount { get; set; }
        public float JumpVel { get; set; }
        public bool Grounded { get; private set; }
        public bool Invincible { get; set; }
        public bool Boosted { get; private set; }
        public bool Ramming { get; private set; }
        public bool OnLadder { get; private set; }
        public bool Noclip { get; private set; }
        public bool JetBoosted { get; set; }
        public float JetBoostFling { get; set; }
        public float BoostSpeed { get; set; }
        public int MaxBoosts { get; set; }
        public int MaxSelfBoosts { get; set; }
        public int MaxHealth { get; set; }
        public float MaxJetTime { get; set; }
        public WeaponTypes WeaponType { get; private set; }
        public int SelfBoosts { get; private set; }
        public int LastInteractEnt { get; private set; }
        public bool HoldingCorpse { get; private set; }
        public UUID CurrentCorpse { get; private set; }
        public bool Direction { get; set; } //false = right
        public int StatPoints { get; set; }
        public int UpgradePoints { get; set; }
        public Upgrade[] Upgrades { get; set; }
        public Rectangle ProjCataBounds { get { return CollisionEngine.InflateRect(Transform.GetRectangle(), projCataB); } set { } }
        public bool CanProjCata { get; set; }
        public bool SoanoFlying { get; set; }
        public bool FallKilling { get; set; }
        public bool SuperBoost { get; private set; }
        public bool BlockSelfBoost { get; set; }
        public bool BlockMovement { get; set; }
        public int Rallies { get; set; }

        private float ramXVel;
        private float knockbackVel;
        private int boosts;
        private float boostCooldown;
        private const float maxBC = 1f;
        private const float interactDist = 18;
        private const int projCataB = 16;
        private float ramCooldown;
        private bool isSelfBoosted;
        private bool isTouchingWall;
        private float immunityTimer;
        public float jetTimer;
        private bool readyToInteract;
        private bool interactBlock;
        private float initialSpeed; //change this system if too many variables are here
        private float initialWeight; //^^^
        public float sbRegenTimer;
        private Rectangle ladderBounds;
        private NPC corpseNPC;
        private bool onSlope;
        private bool prevSlope;
        private float spewTimer;
        private bool weaverUpgrade;
        private bool homingBoost;
        public float fallDeathTimer;
        public float rallyTimer;
        public Vector2 externalVelocity;

        public Player()
        {
            Debug.OnCommand += OnCommand;
        }

        public void Spawn(Vector2 spawnPos)
        {
            Transform = new Transform(spawnPos, 12, 28, 15, false);
            Transform.ResetOldPosition();
            Frozen = false;

            AccelSpeed = 0.3f;
            DeaccelSpeed = 0.1f;
            BoostSpeed = 5;
            RamDeaccel = 0.1f;
            JetBoostFling = 1f;

            if (Core.FirstLevelLoad)
            {
                ResetStats();
                Upgrades = new Upgrade[2] { Upgrade.None, Upgrade.None };
            }
            else
            {
                MaxHealth = (int)GameStateManager.States[15];
                Speed = GameStateManager.States[16];
                JumpVel = GameStateManager.States[17];
                MaxBoosts = (int)GameStateManager.States[18];
                MaxSelfBoosts = (int)GameStateManager.States[19];
                RamAmount = GameStateManager.States[20];
                MaxJetTime = GameStateManager.States[21];
                KnockbackRes = GameStateManager.States[22];
                ChangeWeaponType((WeaponTypes)Enum.ToObject(typeof(WeaponTypes), (int)GameStateManager.States[300]));
                Upgrades = new Upgrade[2] { (Upgrade)GameStateManager.States[36], (Upgrade)GameStateManager.States[37] };
            }

            Health = MaxHealth;
            SelfBoosts = MaxSelfBoosts;
            knockbackVel = 0;
            initialSpeed = Speed;
            initialWeight = Transform.Weight;
            Noclip = SSS.PreviewMode;
            readyToInteract = false;
            Invincible = false;
            SoanoFlying = false;
            JetBoosted = false;
            BlockMovement = false;
            jetTimer = 0;
            spewTimer = Core.HasUpgradeOn(Upgrade.Spewing_Emission) ? 0 : -100;
            weaverUpgrade = Core.HasUpgradeOn(Upgrade.Weavers_Key);
            Rallies = 0;
            rallyTimer = 0;
            LastInteractEnt = 0;
            BlockSelfBoost = false;
        }

        public void Update()
        {
            if (!Frozen && !LevelHandler.MainMenu)
            {
                if (Health <= 0 && !Invincible)
                {
                    Frozen = true;
                    Core.Die();
                }

                if (SelfBoosts < MaxSelfBoosts) 
                {
                    if (sbRegenTimer < 5f)
                        sbRegenTimer += SSS.Delta;
                    else
                    {
                        sbRegenTimer = 0;
                        AddSelfBoost(1);
                    } 
                }

                if(rallyTimer > 0 && Rallies > 0)
                {
                    rallyTimer -= SSS.Delta;
                    if (rallyTimer <= 0)
                        Rallies = 0;
                }

                knockbackVel = MathF.Abs(knockbackVel) <= 0.1f ? 0.0f : MathHelper.Lerp(knockbackVel, 0.0f, 0.1f);
                interactBlock = false;
                if (!JetBoosted && !Noclip && !OnLadder)
                {
                    bool collide = false;
                    foreach(NPC npc in Core._NPCs)
                    {
                        foreach (KeyValuePair<int, Rectangle> pair in npc.Hitboxes.ToArray())
                        {
                            bool ramb = pair.Key == 0 ? npc.Ramming : true;
                            if (immunityTimer <= 0 && ramb && !npc.Corpse && !npc.Passive)
                            {
                                var hb = pair.Value;
                                if (CollisionEngine.RectInRect(new Rectangle(hb.X + (int)npc.Transform.Position.X, hb.Y + (int)npc.Transform.Position.Y, hb.Width, hb.Height), Transform.GetRectangle()))
                                {
                                    immunityTimer = 0.4f;
                                    Health = Math.Clamp(Health - 1, 0, MaxHealth);
                                    if(Core.HasUpgradeOn(Upgrade.Reflection) && npc.Transform.Position.Y + npc.Transform.Height >= Transform.Position.Y)
                                    {
                                        npc.Transform.Velocity = new Vector2(Transform.Velocity.X, Transform.Velocity.Y + (Transform.Weight * -0.75f));
                                        npc.Health = Math.Clamp(npc.Health - 1, 0, npc.MaxHealth);
                                    }
                                    Knockback(npc.Transform);
                                    AudioSystem.PlayEvent("dodgeCollide", true, Transform.Centre, true);
                                    collide = true;

                                    if (npc.Boss)
                                    {
                                        rallyTimer = 1.2f;
                                        Rallies++;
                                    }
                                    break;
                                }
                            }
                        }
                        if (collide)
                            break;
                    }
                    if (immunityTimer > 0)
                        immunityTimer -= SSS.Delta;

                    if (Boosted)
                    {
                        if (Grounded && boostCooldown > SSS.Delta)
                        {
                            Boosted = false;
                            boosts = MaxBoosts;
                        }
                        if (boostCooldown < maxBC / 2)
                            boostCooldown += SSS.Delta;

                        if(Core.HasUpgradeOn(Upgrade.Flowing_Ram) && ramCooldown - (maxBC * 0.75f) > 0)
                            ramXVel = MathF.Cos(ramCooldown * 20) * 6;
                        else
                            ramXVel = MathHelper.Lerp(ramXVel, 0, RamDeaccel);

                        if (homingBoost)
                        {
                            var closest = Core.GetClosestNPC(Transform.Centre, false, out float _, min: 100);
                            if (closest != null && Vector2.Distance(closest.Transform.Centre, Transform.Centre) > 16)
                            {
                                if (Health == MaxHealth)
                                    Health -= 1;
                                var vel = Vector2.Normalize(closest.Transform.Position - Transform.Position) * RamAmount;
                                ramXVel = vel.X;
                                Transform.Velocity = new Vector2(Transform.Velocity.X, vel.Y);
                            }
                            else
                                homingBoost = false;
                        }

                        if (ramCooldown <= 0)
                        {
                            float amp = RamAmount * (SuperBoost ? 3 : 1);

                            if (GameInputs.OncePress(GameInputs.Controls["ramRight"]))
                            {
                                ramCooldown = maxBC;
                                homingBoost = Core.HasUpgradeOn(Upgrade.Submission) && Health == MaxHealth;
                                ramXVel = amp;
                                AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                            }
                            if (GameInputs.OncePress(GameInputs.Controls["ramLeft"]))
                            {
                                ramCooldown = maxBC;
                                homingBoost = Core.HasUpgradeOn(Upgrade.Submission) && Health == MaxHealth;
                                ramXVel = -amp;
                                AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                            }
                            if (weaverUpgrade && GameInputs.OncePress(GameInputs.Controls["down"]))
                            {
                                ramCooldown = maxBC;
                                Transform.ChangeVelocityY(amp);
                                AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                            }
                        }
                        else
                        {
                            ramCooldown -= SSS.Delta;
                            if (SuperBoost && (ramCooldown <= 0 + SSS.Delta || !Ramming))
                                SuperBoost = false;
                        }
                    }
                    else
                    {
                        ramXVel = 0;
                        SuperBoost = false;
                    }

                    Ramming = Boosted && (MathF.Abs(ramXVel) > RamAmount / 2f || Transform.Velocity.Y > RamAmount / 2f);

                    float horizontalMov = GameInputs.GetHorizontalInputVel(AccelSpeed, DeaccelSpeed) * Speed * (BlockMovement ? 0 : 1);
                    if (horizontalMov > 0)
                        Direction = false;
                    else if (horizontalMov < 0)
                        Direction = true;
                    Transform.Velocity = new Vector2(horizontalMov + ramXVel, Transform.Velocity.Y);

                    if (!BlockMovement)
                    {
                        if (GameInputs.OncePress(GameInputs.Controls["boost"]))
                        {
                            bool extrathing = BlockSelfBoost ? Boosted : SelfBoosts > 0 || Boosted;
                            if (!CanProjCata && extrathing)
                                Boost(true);
                            else if (CanProjCata)
                                Boost(false);
                        }
                        else if (GameInputs.OncePress(GameInputs.Controls["jump"]) && !Boosted && Grounded)
                            Transform.Velocity = new Vector2(Transform.Velocity.X, -JumpVel);
                    }
                }
                else if (JetBoosted)
                {
                    if (!SoanoFlying)
                    {
                        jetTimer += SSS.Delta;
                        Ramming = true;
                        if (jetTimer >= MaxJetTime)
                        {
                            JetBoosted = false;
                            Ramming = false;
                            jetTimer = 0;
                        }
                    }

                    float accelSpeed = SoanoFlying ? 0.1f : JetBoostFling * 0.3f;
                    if (GameInputs.GetKeyboard().IsKeyDown(GameInputs.Controls["right"]))
                        Transform.Velocity += new Vector2(accelSpeed, 0);
                    if (GameInputs.GetKeyboard().IsKeyDown(GameInputs.Controls["left"]))
                        Transform.Velocity -= new Vector2(accelSpeed, 0);
                    if (GameInputs.GetKeyboard().IsKeyDown(GameInputs.Controls["down"]))
                        Transform.Velocity += new Vector2(0, accelSpeed);
                    if (GameInputs.GetKeyboard().IsKeyDown(GameInputs.Controls["up"]))
                        Transform.Velocity -= new Vector2(0, accelSpeed);

                    float maxSpeed = Speed * (JetBoostFling * 3);
                    Transform.Velocity = Vector2.Clamp(Transform.Velocity, new Vector2(-maxSpeed, -maxSpeed), new Vector2(maxSpeed, maxSpeed));

                    if (spewTimer > -100)
                    {
                        if (spewTimer <= 0)
                        {
                            spewTimer = 0.4f + (Random.Shared.Next(0, 5) / 10f);
                            var proj = new Projectile(0, null);
                            proj.Deflect();
                            Vector2 dir = -Vector2.Normalize(Transform.Velocity);
                            Core.SpawnProjectile(proj, Transform.Centre, dir);
                            //ParticleManager.AddParticles(0, Transform.Centre, Vector2.Normalize(-Transform.Velocity) * 2, 1f, 3, 4, 4);
                        }
                        else
                            spewTimer -= SSS.Delta;
                    }
                }
                else if (Noclip)
                {
                    float horizontalMov = GameInputs.GetHorizontalInputVel(AccelSpeed, DeaccelSpeed) * Speed * 4;
                    float verticalMov = GameInputs.GetVerticalInputVel(AccelSpeed, DeaccelSpeed) * Speed * 4;
                    Transform.Velocity = new Vector2(horizontalMov, verticalMov);
                }
                else if (OnLadder)
                {
                    float top = ladderBounds.Y - (Transform.Height / 2);
                    float bot = ladderBounds.Y + ladderBounds.Height - Transform.Height;
                    Transform.Position = new Vector2(ladderBounds.X + (ladderBounds.Width / 2) - (Transform.Width / 2), Math.Clamp(Transform.Position.Y, top, bot));
                    Transform.Velocity = new Vector2(0, GameInputs.GetVerticalInputVel(AccelSpeed, DeaccelSpeed) * Speed);
                    if (Transform.Position.Y <= top || Transform.Position.Y >= bot || GameInputs.OncePress(GameInputs.Controls["interact"]) || GameInputs.OncePress(GameInputs.Controls["jump"]))
                    {
                        Transform.Gravity = true;
                        OnLadder = false;
                        interactBlock = true;
                    }
                }

                UpdateInteractions();

                if (HoldingCorpse)
                {
                    float xHold = Direction ? Transform.Centre.X - (Transform.Width / 2) - 2 - corpseNPC.Transform.Width : Transform.Centre.X + (Transform.Width / 2) + 2;
                    corpseNPC.Transform.Position = Vector2.Lerp(corpseNPC.Transform.Position, new Vector2(xHold, Transform.Position.Y + Transform.Height - corpseNPC.Transform.Height), 0.5f);

                    if (GameInputs.OnceRightClick(ButtonState.Released) || JetBoosted)
                    {
                        Speed = initialSpeed;
                        Transform.Weight = initialWeight;
                        corpseNPC.Transform.Gravity = true;
                        corpseNPC.Transform.Friction = true;
                        corpseNPC.Transform.ChangeVelocityX(Transform.Velocity.X * 2.5f);
                        corpseNPC.exploded = Core.HasUpgradeOn(Upgrade.Corpse_Accension);
                        HoldingCorpse = false;
                    }
                }
                else
                {
                    foreach (NPC npc in Core._NPCs.Where(x => x.Corpse))
                    {
                        float dist = Vector2.Distance(npc.Transform.Centre, Transform.Centre);
                        if (dist < interactDist && GameInputs.OnceRightClick(ButtonState.Pressed))
                        {
                            corpseNPC = npc;
                            Speed = initialSpeed / (MathUtil.MinClamp(corpseNPC.Size, 1f) + 0.2f);
                            Transform.Weight = initialWeight + 2;
                            HoldingCorpse = true;
                            corpseNPC.Transform.Friction = false;
                            corpseNPC.Transform.Gravity = false;
                            CurrentCorpse = npc.UUID;
                        }
                    }
                }

                if (!Noclip)
                {
                    Grounded = false;
                    Transform.Velocity = CollisionEngine.UpdateVelocity(Transform, SSS.Delta) + externalVelocity;
                    isTouchingWall = false;
                    prevSlope = false;
                    bool ceiling = false;
                    foreach (Collider col in Core._Colliders)
                    {
                        if (col.Tags != null && (col.Tags.Contains("ex_player") || col.Tags.Contains("ex_player_seeker")))
                            continue;

                        if (weaverUpgrade && col.X < Transform.Position.X && col.X + col.Width > Transform.Position.X + Transform.Width
                            && Transform.Position.Y < col.Y + col.Height + 2 && Transform.Position.Y > col.Y + col.Height)
                            ceiling = true;

                        if (CollisionEngine.CollideAll(col, Transform, out Vector2 oPos, out Vector2 oVel))
                        {
                            Transform.Velocity = oVel;
                            Transform.Position = oPos;
                            if (col.Type == ColliderType.Slope && col.Y + col.Height > Transform.Position.Y + Transform.Height && col.Y != Transform.Position.Y + Transform.Height)
                            {
                                Grounded = true;
                                prevSlope = true;
                            }

                            if (col.GetRectangle().Top >= Transform.Position.Y + Transform.Height || onSlope)
                                Grounded = true;
                            if (Transform.Position.Y + Transform.Height > col.GetRectangle().Top && Transform.Position.Y < col.GetRectangle().Bottom)
                                isTouchingWall = true;
                        }
                    }
                    if ((isTouchingWall || ceiling) && Boosted && !JetBoosted && GameInputs.GetKeyboard().IsKeyDown(GameInputs.Controls["jump"]))
                    {
                        if (!SuperBoost && Core.HasUpgradeOn(Upgrade.Spiders_Key))
                            SuperBoost = true;
                        Transform.ChangeVelocityY(0);
                    }
                }
                Transform.Velocity -= new Vector2(knockbackVel, 0);
                Transform.ApplyVelocity();
                
                if (prevSlope && !Boosted && !JetBoosted && !OnLadder)
                    Transform.ChangeVelocityY(Transform.Weight);
                CanProjCata = false;

                Debug.SetOSS("on slope", prevSlope);
                Debug.SetOSS("grounded", Grounded);
                Debug.SetOSS("centre", Transform.Centre);
                Debug.SetOSS("velocity", Transform.Velocity);
            }

            if (FallKilling)
            {
                fallDeathTimer -= SSS.Delta;
                if(fallDeathTimer <= 0)
                {
                    FallKilling = false;
                    Invincible = false;
                    Core.Die();
                }
            }
        }

        public void Draw(Renderer renderer)
        {
            if (!LevelHandler.MainMenu)
            {
                var color = Boosted ? Color.CornflowerBlue : Color.White;
                color = Ramming ? Color.Crimson : color;
                color = CanProjCata ? Color.Lime : color;
                color = SuperBoost ? Color.Blue : color;
                renderer.BasicDraw(SSS.Square, Transform.GetRectangle(), 0, 0, col: color);

                if (rallyTimer > 0)
                    renderer.DrawText(Debug.DebugFont, "x" + Rallies, Transform.Position + new Vector2(Transform.Width + 2, 6), 1, 1, col: Color.Orange);
            }
        }

        public void UpdateInteractions()
        {
            bool prev = readyToInteract;
            int targetUUID = -1;
            readyToInteract = false;
            if (!Boosted && !JetBoosted && !OnLadder && !HoldingCorpse && !interactBlock)
            {
                foreach (Entity ent in Core._LevelHandler.EntityMan.Entities.Where(x => typeof(IInteractableEnt).IsAssignableFrom(x.GetType())))
                {
                    bool inDistance = ent.Point ? Vector2.Distance(ent.Centre, Transform.Centre) < interactDist
                        : CollisionEngine.RectInRect(Transform.GetRectangle(), CollisionEngine.InflateRect(ent.Bounds.Value, interactDist / 2));
                    var preEnt = (IInteractableEnt)ent;
                    if (inDistance && ent.Active && !preEnt.DisableInteraction)
                    {
                        targetUUID = ent.UUID;
                        break;
                    }
                }
            }

            if(targetUUID != -1)
            {
                readyToInteract = true;
                if (GameInputs.OncePress(GameInputs.Controls["interact"]))
                {
                    IInteractableEnt intent = (IInteractableEnt)Core._LevelHandler.EntityMan.Entities.Where(x => x.UUID == targetUUID).First();
                    intent.OnInteraction();
                    LastInteractEnt = targetUUID;
                }
            }

            if (prev != readyToInteract)
                UIDesigner.ChangeUI(4);
        }

        public void Boost(bool self, float mult = 1)
        {
            if (!OnLadder)
            {
                bool groundCond = self ? Grounded : true;
                if (!Boosted && groundCond)
                {
                    Boosted = true;
                    isSelfBoosted = self;
                    SelfBoosts = self ? SelfBoosts - 1 : SelfBoosts;
                    boosts = 0;
                    boostCooldown = maxBC;
                }

                if (boosts < MaxBoosts && boostCooldown >= maxBC / 2)
                {
                    boostCooldown = 0;
                    boosts++;
                    Transform.ChangeVelocityY(-BoostSpeed * (isSelfBoosted ? 0.5f : 0.75f) * mult);
                    if (boosts == 1)
                    {
                        if (self)
                            sbRegenTimer = 0f;
                        AudioSystem.PlayEvent("boost", true, Transform.Centre, true);
                    }
                    else
                        AudioSystem.PlayEvent("smallBoost", true, Transform.Centre, true);
                }
            }
        }

        public void JetBoost(bool soano)
        {
            if (!JetBoosted)
            {
                if (Boosted)
                {
                    Boosted = false;
                    boosts = MaxBoosts;
                }

                SoanoFlying = soano;
                JetBoosted = true;
                jetTimer = 0;
                Transform.ChangeVelocityY(-1f);
            }
        }

        public void MountLadder(Rectangle mladderBounds)
        {
            ladderBounds = mladderBounds;
            OnLadder = !OnLadder;
            if (OnLadder)
            {
                Transform.Gravity = false;
                Transform.Velocity = Vector2.Zero;
                Transform.Position = new Vector2(ladderBounds.X, Math.Clamp(Transform.Position.Y, ladderBounds.Y - (Transform.Height / 2) + 10, ladderBounds.Y + ladderBounds.Height - Transform.Height - 10));
                GameInputs.ResetMoveInput();
            }
            else
            {
                Transform.Gravity = true;
            }
        }

        public void AddSelfBoost(int amount)
        {
            SelfBoosts = Math.Clamp(SelfBoosts + amount, 0, MaxSelfBoosts);
        }

        private void OnCommand(object sender, CommandEventArgs args)
        {
            if (args.Command == "jet")
            {
                JetBoost(false);
            }
            else if (args.Command == "noclip")
            {
                Noclip = !Noclip;
            }
            else if (args.Command == "nodegraph")
            {
                Debug.DrawNodegraph = !Debug.DrawNodegraph;
            }
            else if (args.Command == "normals")
            {
                Debug.DrawNormals = !Debug.DrawNormals;
            }
            else if(args.Command == "kill")
            {
                Health = 1;
            }
            else if(args.Command == "particle" && args.Argument != "")
            {
                ParticleManager.AddParticles(int.Parse(args.Argument), Transform.Centre, -Vector2.One / 4, 1f, 4, 4, 4);
            }
            else if(args.Command == "lighting")
            {
                Core._Renderer.EnableLigthing = !Core._Renderer.EnableLigthing;
            }
            else if (args.Command == "load_level" && args.Argument != "")
            {
                Core.LoadLevel(int.Parse(args.Argument));
            }
            else if (args.Command == "zoom" && args.Argument != "")
            {
                Core._Renderer.View.Zoom = int.Parse(args.Argument);
            }
            else if (args.Command == "reset")
            {
                Core.LoadLevel(LevelHandler.CurrentLevel);
            }
            else if (args.Command == "god")
            {
                Health = MaxHealth;
                Invincible = !Invincible;
                Debug.Log("God mode is " + (Invincible ? "ON" : "OFF"), LogType.Info);
            }
            else if (args.Command == "projectile" && args.Argument != "")
            {
                Core.SpawnProjectile(new Projectile(int.Parse(args.Argument), null), Transform.Centre + new Vector2(64, 0), Vector2.Zero);
            }
            else if (args.Command == "skybox" && args.Argument != "")
            {
                var s = args.Argument.Split(',');
                Core._Skybox.DebugElement(s[0], int.Parse(s[1]), int.Parse(s[2]), s.Length > 3 ? int.Parse(s[3]) : -1);
            }
            else if (args.Command == "cnpcst" && args.Argument != "")
            {
                var npc = Core.GetClosestNPC(Transform.Centre, false, out float _);
                npc.ChangeAIState(0, int.Parse(args.Argument));
                npc.ChangeAIState(1, 0);
                npc.ChangeAIState(2, 0);
                npc.ChangeAIState(3, 0);
                npc.ChangeAIState(4, 0);
                npc.ChangeAIState(5, 0);
                npc.ChangeAIState(6, 0);
                npc.ChangeAIState(7, 0);
                npc.ChangeAIState(8, 0);
                npc.ChangeAIState(9, 0);
            }
        }                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 

        public void RestoreBoosts()
        {
            SelfBoosts = MaxBoosts;
        }

        public void ChangeWeaponType(WeaponTypes wp)
        {
            WeaponType = wp;
            UIDesigner.ChangeUI(0);
        }

        public void Knockback(Transform trans, float mult = 1)
        {
            Vector2 dir = Vector2.Normalize(trans.Position - Transform.Position);
            knockbackVel = dir.X * (trans.Weight * 0.7f) * 2 * mult;
            Transform.ChangeVelocityY(dir.Y * mult);
        }

        public void ResetStats()
        {
            MaxHealth = 5;          //10
            Speed = 2f;             //11
            JumpVel = 1.8f;         //12
            MaxBoosts = 5;          //13
            MaxSelfBoosts = 5;       //14
            RamAmount = 8.5f;        //15
            MaxJetTime = 5f;        //16
            KnockbackRes = 1f;      //17
        }
    }

    public enum WeaponTypes
    {
        Peirce,
        Slash, 
        Blunt
    }

    public enum Upgrade {
        None,
        Sharp_Deflection,
        Double_Deflection,
        Flowing_Ram,
        Corpse_Accension,
        Angers_Display,
        Spewing_Emission,
        Spiders_Key,
        Submission,
        Reflection,
        Weavers_Key
    }
}

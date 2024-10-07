using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TripleS;
using TripleS.Physics;
using TripleS.Scripting;

namespace Symfora {
    public class NPC {

        public static List<int[]> Packs { get; set; }
        public Transform Transform { get; set; }
        public UUID UUID { get; private set; }
        public NodeController NavController { get; private set; }
        public bool Corpse { get; private set; }
        public bool Noticed { get; set; }
        public bool Boss { get; private set; }
        public int ID { get; private set; }
        public int AIID { get; private set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int MaxBoosts { get; set; }
        public bool CanDeflect { get; private set; }
        public int Size { get; private set; } //0 - small, 1 medium, 2 - medium large, 3 - large
        public bool Corpseable { get; private set; }
        public float Speed { get; private set; }
        public int NoticeDistance { get; private set; }
        public float RamAmount { get; private set; }
        public bool Phantom { get; private set; }
        public bool Grounded { get; private set; }
        public bool Ramming { get; private set; }
        public bool CantStun { get; private set; }
        public bool CantKnockback { get; private set; }
        public bool Invincible { get; set; }
        public bool TakeNoPlayerDamage { get; set; }
        public bool Freeze { get; set; }
        public int Pack { get; private set; }
        public bool StopDraw { get; set; }
        public bool Passive { get; set; }
        public int MaxPackCount { get; private set; }
        public int PackDist { get; private set; }
        public bool CanSeePlayer { get; private set; }
        public float DistToPlayer { get; private set; }
        private bool PreRam { get; set; }
        public PhantomType PType { get; private set; }
        public Dictionary<int, Rectangle> Hitboxes { get; private set; }

        private bool isTouchingWall;
        private bool isTouchingCeiling;
        private float[] ai;
        private bool[] aiFlags;
        private int packID;
        private bool packLeader;
        private int lastTouchedWallDir;
        private float knockbackVel;
        private float immunityTimer;
        private float stunTimer;
        private int wanderAmount;
        private bool gPreRamCheck;
        private bool alwaysNotice;
        private int wanderDir;
        private Vector2 initPos;
        private const float maxImmunity = 0.4f;
        private bool updateNodes = true;
        private string entName;
        private string titleId;
        private float timeInCurrentState = 0.0f;
        private int lastState = 0;
        public bool exploded;
        private Ray2D kefalSwingRay;
        private bool lastTickStunned;
        private int[] bossPhaseMakers;
        private float stunTimeMult;
        private int permaID = -1;
        private int permaSlotValue = -1;
        bool gravity = true;

        public NPC(int id, int nd, int wd, PhantomType ppt)
        {
            UUID = new UUID(1);
            NavController = new NodeController();
            ID = id;
            Vector2 dimensions = Vector2.One;
            float weight = 1;
            Phantom = false;
            NoticeDistance = nd;
            wanderAmount = wd;
            PType = ppt;
            stunTimeMult = 1f;
            switch (ID)
            {
                case 0: //Mad tramp
                    MaxHealth = 2;
                    MaxBoosts = 3;
                    dimensions = new Vector2(12, 28);
                    weight = 10;
                    Corpseable = true;
                    Speed = 1;
                    AIID = 0;
                    RamAmount = 8f;
                    NavController.NodeDistance = 16;
                    Size = 0;
                    break;
                case 1: //Mad citizen
                    MaxHealth = 3;
                    MaxBoosts = 5;
                    dimensions = new Vector2(16, 32);
                    weight = 10;
                    Corpseable = true;
                    Speed = 1;
                    AIID = 1;
                    RamAmount = 8f;
                    NavController.NodeDistance = 18;
                    Size = 1;
                    break;
                case 2: //Bug smoll
                    MaxHealth = 1;
                    dimensions = new Vector2(10, 7);
                    weight = 5;
                    Corpseable = true;
                    Speed = 1;
                    AIID = 2;
                    RamAmount = 5f;
                    PackDist = 128;
                    MaxPackCount = 10;
                    NavController.NodeDistance = 10;
                    Size = 0;
                    break;
                case 3: //Sinkskipper
                    MaxHealth = 2;
                    dimensions = new Vector2(8, 16);
                    weight = 5;
                    Corpseable = true;
                    Speed = 1;
                    AIID = 3;
                    RamAmount = 4f;
                    NavController.NodeDistance = 10;
                    Size = 0;
                    alwaysNotice = true;
                    gPreRamCheck = true;
                    break;
                case 4: //Guard
                    MaxHealth = 4;
                    dimensions = new Vector2(19, 35);
                    weight = 12;
                    Corpseable = true;
                    Speed = 1;
                    AIID = 4;
                    RamAmount = 8f;
                    NavController.NodeDistance = 19;
                    Size = 2;
                    break;
                case 5: //Seeker
                    MaxHealth = 7;
                    MaxBoosts = 5;
                    dimensions = new Vector2(17, 35);
                    weight = 9;
                    Corpseable = true;
                    Speed = 2;
                    AIID = 5;
                    RamAmount = 10f;
                    NavController.NodeDistance = 19;
                    Size = 2;
                    break;
                case 6: //Chloravoth
                    MaxHealth = 6;
                    MaxBoosts = 0;
                    dimensions = new Vector2(36, 40);
                    weight = 15;
                    Corpseable = false;
                    gPreRamCheck = true;
                    Speed = 0;
                    AIID = 6;
                    RamAmount = 0f;
                    NavController.NodeDistance = 16;
                    CantKnockback = true;
                    CantStun = true;
                    Size = 3;
                    break;
                case 7: //Mad tramp EZ
                    MaxHealth = 2;
                    MaxBoosts = 3;
                    dimensions = new Vector2(12, 28);
                    weight = 10;
                    Corpseable = true;
                    Speed = 1;
                    AIID = 7;
                    RamAmount = 8f;
                    NavController.NodeDistance = 16;
                    Size = 0;
                    break;
                case 8: //Mad citizen EZ
                    MaxHealth = 3;
                    MaxBoosts = 5;
                    dimensions = new Vector2(16, 32);
                    weight = 10;
                    Corpseable = true;
                    Speed = 1;
                    AIID = 8;
                    RamAmount = 8f;
                    NavController.NodeDistance = 18;
                    Size = 1;
                    break;
                case 9: //Soano Head Stalker
                    MaxHealth = 1;
                    Invincible = true;
                    dimensions = new Vector2(54, 32);
                    weight = 10;
                    Phantom = true;
                    Corpseable = false;
                    Speed = 1;
                    AIID = 9;
                    updateNodes = false;
                    alwaysNotice = true;
                    gravity = false;
                    break;
                case 10: //Rock Crawdad
                    MaxHealth = 2;
                    dimensions = new Vector2(15, 11);
                    weight = 5;
                    Corpseable = true;
                    Speed = 1;
                    AIID = 10;
                    RamAmount = 5f;
                    PackDist = 128;
                    MaxPackCount = 5;
                    NavController.NodeDistance = 10;
                    Size = 0;
                    break;
                case 11: //Experiment
                    MaxHealth = 7;
                    dimensions = new Vector2(50, 50);
                    weight = 7;
                    Corpseable = true;
                    Speed = 1.8f;
                    AIID = 11;
                    RamAmount = 5f;
                    NavController.NodeDistance = 40;
                    Size = 2;
                    break;
                case 12: //Oil Ball
                    MaxHealth = 2;
                    dimensions = new Vector2(18, 15);
                    weight = 5;
                    Corpseable = true;
                    Speed = 0.5f;
                    AIID = 12;
                    RamAmount = 6f;
                    PackDist = 200;
                    MaxPackCount = 10;
                    NavController.NodeDistance = 18;
                    Size = 1;
                    break;
                case 13: //Oil Serpent
                    MaxHealth = 5;
                    dimensions = new Vector2(58, 40);
                    weight = 7;
                    Corpseable = true;
                    Speed = 1.85f;
                    AIID = 13;
                    RamAmount = 5f;
                    NavController.NodeDistance = 40;
                    Size = 2;
                    break;
                case 14: //Kefal
                    MaxHealth = 3;
                    MaxBoosts = 3;
                    dimensions = new Vector2(17, 38);
                    weight = 10;
                    Corpseable = true;
                    Speed = 1;
                    AIID = 14;
                    RamAmount = 14f;
                    NavController.NodeDistance = 18;
                    Size = 1;
                    break;
                case 15: //Ranged Kefal
                    MaxHealth = 3;
                    MaxBoosts = 2;
                    dimensions = new Vector2(15, 36);
                    weight = 10;
                    Corpseable = true;
                    Speed = 1;
                    AIID = 15;
                    RamAmount = 6f;
                    NavController.NodeDistance = 17;
                    Size = 1;
                    break;
                case 16: //Deranged Kefal
                    MaxHealth = 4;
                    MaxBoosts = 3;
                    dimensions = new Vector2(14, 32);
                    weight = 7;
                    Corpseable = true;
                    Speed = 1.4f;
                    AIID = 16;
                    RamAmount = 14f;
                    NavController.NodeDistance = 16;
                    Size = 1;
                    break;
                case 17: //Bug mediumlarge
                    MaxHealth = 2;
                    dimensions = new Vector2(14, 14);
                    weight = 5;
                    Corpseable = true;
                    Speed = 1;
                    AIID = 17;
                    RamAmount = 8f;
                    MaxBoosts = 2;
                    PackDist = 200;
                    MaxPackCount = 10;
                    NavController.NodeDistance = 10;
                    gravity = false;
                    Size = 2;
                    break;
                case 18: //Ponderosa Serpent
                    MaxHealth = 7;
                    dimensions = new Vector2(68, 50);
                    weight = 7;
                    Corpseable = false;
                    Speed = 1f;
                    AIID = 18;
                    RamAmount = 5f;
                    NavController.NodeDistance = 50;
                    Size = 3;
                    break;
                case 19: //Stenonat
                    MaxHealth = 20;
                    dimensions = new Vector2(14, 30);
                    weight = 5;
                    Speed = 3.5f;
                    AIID = 19;
                    RamAmount = 12f;
                    NavController.NodeDistance = 25;
                    Boss = true;
                    Size = 1;
                    Corpseable = true;
                    bossPhaseMakers = new int[1] { 12 };
                    titleId = "boss0";
                    stunTimeMult = 0.5f;
                    break;
                case 20: //Gang's Pet Serpent
                    MaxHealth = 20;
                    dimensions = new Vector2(64, 64);
                    weight = 5;
                    Speed = 2.15f;
                    AIID = 20;
                    Size = 3;
                    Corpseable = false;
                    Invincible = true;
                    CantKnockback = true;
                    CantStun = true;
                    alwaysNotice = true;
                    Phantom = true;
                    gravity = false;
                    break;
                case 21: //Faucet Clog
                    MaxHealth = 3;
                    dimensions = new Vector2(80, 832);
                    weight = 1;
                    Speed = 0f;
                    AIID = 21;
                    Size = 3;
                    Corpseable = false;
                    Invincible = true;
                    CantKnockback = true;
                    CantStun = true;
                    alwaysNotice = true;
                    Phantom = true;
                    gravity = false;
                    break;
                case 22: //Mad citizen EZ 1 hp
                    MaxHealth = 1;
                    MaxBoosts = 5;
                    dimensions = new Vector2(16, 32);
                    weight = 10;
                    Corpseable = true;
                    Speed = 1;
                    AIID = 8;
                    RamAmount = 8f;
                    NavController.NodeDistance = 18;
                    Size = 1;
                    break;
                case 23: //Big Chloravoth invincible
                    MaxHealth = 3;
                    MaxBoosts = 0;
                    dimensions = new Vector2(54, 58);
                    weight = 15;
                    Corpseable = false;
                    gPreRamCheck = true;
                    Speed = 0;
                    AIID = 6;
                    RamAmount = 0f;
                    NavController.NodeDistance = 16;
                    CantKnockback = true;
                    CantStun = true;
                    Size = 3;
                    TakeNoPlayerDamage = true;
                    break;
                case 24: //Sky Scythe stationary
                    MaxHealth = 1;
                    MaxBoosts = 0;
                    dimensions = new Vector2(432, 832);
                    weight = 15;
                    Corpseable = false;
                    Speed = 0;
                    AIID = 24;
                    RamAmount = 0f;
                    NavController.NodeDistance = 16;
                    CantKnockback = true;
                    CantStun = true;
                    Size = 3;
                    Invincible = true;
                    Phantom = true;
                    gravity = false;
                    alwaysNotice = true;
                    break;
                case 25: //Sky Scythe trader
                    MaxHealth = 1;
                    MaxBoosts = 0;
                    dimensions = new Vector2(432, 832);
                    weight = 15;
                    Corpseable = false;
                    Speed = 0;
                    AIID = 25;
                    RamAmount = 0f;
                    NavController.NodeDistance = 16;
                    CantKnockback = true;
                    CantStun = true;
                    Size = 3;
                    Invincible = true;
                    Phantom = true;
                    gravity = false;
                    alwaysNotice = true;
                    break;
                case 26: //Placeholder talking NPC
                    MaxHealth = 1;
                    MaxBoosts = 0;
                    dimensions = new Vector2(14, 32);
                    weight = 15;
                    Corpseable = false;
                    Speed = 0;
                    AIID = 26;
                    RamAmount = 0f;
                    NavController.NodeDistance = 16;
                    CantKnockback = true;
                    CantStun = true;
                    Size = 1;
                    Invincible = true;
                    Phantom = true;
                    gravity = false;
                    alwaysNotice = true;
                    break;
                case 27: //Gorjan
                    MaxHealth = 1;
                    MaxBoosts = 0;
                    dimensions = new Vector2(14, 40);
                    weight = 15;
                    Corpseable = false;
                    Speed = 2;
                    AIID = 27;
                    RamAmount = 0f;
                    NavController.NodeDistance = 16;
                    CantKnockback = true;
                    CantStun = true;
                    Size = 1;
                    Invincible = true;
                    alwaysNotice = true;
                    break;
                case 28: //Fynalie
                    MaxHealth = 10;
                    dimensions = new Vector2(14, 32);
                    weight = 5;
                    Speed = 4.5f;
                    AIID = 28;
                    RamAmount = 12f;
                    NavController.NodeDistance = 25;
                    Boss = true;
                    Size = 1;
                    Corpseable = true;
                    bossPhaseMakers = new int[2] { 6,2 };
                    titleId = "boss1";
                    stunTimeMult = 0.5f;
                    break;
                case 29: //LM Base
                    MaxHealth = 20;
                    dimensions = new Vector2(160, 120);
                    weight = 10;
                    Speed = 1.5f;
                    AIID = 29;
                    RamAmount = 15f;
                    NavController.NodeDistance = 25;
                    Boss = true;
                    Size = 1;
                    Corpseable = false;
                    bossPhaseMakers = new int[1] { 2 };
                    titleId = "boss2";
                    stunTimeMult = 0.5f;
                    break;
                case 30: //LM Head
                    MaxHealth = 10;
                    dimensions = new Vector2(90, 50);
                    weight = 9;
                    Speed = 2f;
                    AIID = 30;
                    RamAmount = 15f;
                    NavController.NodeDistance = 25;
                    Boss = false;
                    Size = 1;
                    Corpseable = false;
                    stunTimeMult = 0.5f;
                    break;
                case 31: //Jesersant
                    MaxHealth = 6;
                    dimensions = new Vector2(16, 40);
                    weight = 5;
                    Speed = 0.5f;
                    AIID = 31;
                    RamAmount = 15f;
                    NavController.NodeDistance = 25;
                    Boss = true;
                    Size = 1;
                    Corpseable = false;
                    bossPhaseMakers = new int[2] { 4, 2 };
                    titleId = "boss3";
                    stunTimeMult = 0.5f;
                    break;
                case 32: //Jesersant Hallucination
                    MaxHealth = 1;
                    dimensions = new Vector2(40, 40);
                    weight = 1;
                    Speed = 1f;
                    AIID = 32;
                    RamAmount = 15f;
                    Phantom = true;
                    gravity = false;
                    alwaysNotice = true;
                    Size = 1;
                    Corpseable = false;
                    Invincible = true;
                    break;
                case 33: //Jesersant Clone
                    MaxHealth = 2;
                    dimensions = new Vector2(16, 40);
                    weight = 1;
                    Speed = 1f;
                    AIID = 33;
                    Phantom = true;
                    gravity = false;
                    alwaysNotice = true;
                    Size = 1;
                    Corpseable = false;
                    break;
                case 34: //Bafixi
                    MaxHealth = 50;
                    dimensions = new Vector2(14, 37);
                    weight = 5;
                    Speed = 3.5f;
                    AIID = 34;
                    RamAmount = 13f;
                    NavController.NodeDistance = 25;
                    Boss = true;
                    Size = 1;
                    Corpseable = false;
                    bossPhaseMakers = new int[2] { 35, 20 };
                    titleId = "boss4";
                    stunTimeMult = 0.5f;
                    break;
                case 35: //Azaprota
                    MaxHealth = 7;
                    dimensions = new Vector2(70, 42);
                    weight = 1;
                    Speed = 1.5f;
                    AIID = 35;
                    NavController.NodeDistance = 25;
                    Boss = true;
                    Size = 1;
                    Corpseable = false;
                    bossPhaseMakers = new int[0] { };
                    gravity = false;
                    titleId = "boss5";
                    break;
                case 36: //Azaprota Floating Heads
                    MaxHealth = 1;
                    TakeNoPlayerDamage = true;
                    dimensions = new Vector2(54, 54);
                    weight = 1;
                    Speed = 1.5f;
                    AIID = 36;
                    NavController.NodeDistance = 25;
                    Size = 1;
                    Corpseable = false;
                    gravity = false;
                    Phantom = true;
                    alwaysNotice = true;
                    break;
            }
            Health = MaxHealth;
            NoticeDistance = alwaysNotice ? 0 : NoticeDistance;
            Noticed = !Boss && alwaysNotice;
            Transform = new Transform(Vector2.Zero, (int)dimensions.X, (int)dimensions.Y, weight, false, gravity);
            Hitboxes = new Dictionary<int, Rectangle>() { { 0, new Rectangle(0, 0, (int)dimensions.X, (int)dimensions.Y) } };
        }

        public void Spawn(Vector2 position, string entityName, int pSlot, int pSlotValue)
        {
            Transform.Position = position;
            Transform.ResetOldPosition();
            Core._NPCs.Add(this);
            ai = new float[10];
            aiFlags = new bool[4];
            initPos = position;
            permaID = pSlot;
            permaSlotValue = pSlotValue;
            entName = entityName;

            if (AIID == 9)
                ai[3] = -1;
        }

        public void Update(GameTime time)
        {
            if (!Freeze)
            {
                if (Health <= 0 && !Corpse && ID != 35)
                    Kill();

                Vector2 dirToPlayer = Vector2.Normalize(Core._Player.Transform.Centre - Transform.Centre);
                knockbackVel = MathF.Abs(knockbackVel) <= 0.1f ? 0.0f : MathHelper.Lerp(knockbackVel, 0.0f, 0.1f);
                lastTickStunned = false;
                DistToPlayer = Vector2.Distance(Transform.Centre, Core._Player.Transform.Centre);

                if (Boss && Noticed)
                    UIDesigner.SetBossInfo(MaxHealth, Health, bossPhaseMakers, titleId);

                if (!Corpse)
                {
                    if (updateNodes)
                        NavController.UpdateConnections(Transform, Core._Player.Transform.Centre, SSS.Delta, Transform.Height < 16);
                    if (immunityTimer <= 0 && Core._Player.Ramming && (Core._Player.JetBoosted || Core._Player.Boosted))
                    {
                        if (CollisionEngine.RectInRect(Core._Player.Transform.GetRectangle(), Transform.GetRectangle()))
                        {
                            immunityTimer = maxImmunity;
                            if(!TakeNoPlayerDamage)
                                Health = Math.Clamp(Health - (Core.HasUpgradeOn(Upgrade.Spiders_Key) ? 2 : 1), 0, MaxHealth);
                            Knockback();
                            if(Health != 0)
                                AudioSystem.PlayEvent("dodgeCollide", true, Transform.Centre, true, pitch: 1.4f);
                            if(Boss)
                            {
                                if (bossPhaseMakers.Length > 0)
                                {
                                    for(int i = 0; i < bossPhaseMakers.Length; i++)
                                    {
                                        if(Health == bossPhaseMakers[i])
                                        {
                                            Core.SpawnProjectile(new Projectile(5, null), Transform.Centre, Vector2.Zero);
                                            break;
                                        }
                                    }
                                }

                                if (Core._Player.Rallies > 0)
                                {
                                    Core._Player.Health = Math.Clamp(Core._Player.Health + 1, 0, Core._Player.MaxHealth);
                                    Core._Player.Rallies--;
                                    if (Core._Player.Rallies <= 0)
                                        Core._Player.rallyTimer = 0;
                                }
                            }
                        }
                    }
                    else if (immunityTimer > 0)
                        immunityTimer -= SSS.Delta;

                    if (stunTimer > 0)
                    {
                        stunTimer -= SSS.Delta;
                        lastTickStunned = stunTimer <= 0;
                    }

                    if (PreRam && (Ramming || (Grounded && !gPreRamCheck)))
                        PreRam = false;

                    if(lastState != ai[0])
                    {
                        lastState = (int)ai[0];
                        timeInCurrentState = 0f;
                    }
                    timeInCurrentState += SSS.Delta;
                    int horiDirToPlayer = Core._Player.Transform.Centre.X < Transform.Centre.X ? -1 : 1;

                    bool stuned = !CantStun && stunTimer > 0;
                    if (Noticed && !stuned)
                    {
                        if (NoticeDistance != 0 && !Boss)
                            Noticed = DistToPlayer < NoticeDistance * 1.2f;
                        else
                            Noticed = true;
                        switch (AIID) //0.4 seconds is standard for time between pre warning and ram
                        {
                            #region Mad Tramp
                            //1 - boosts, 2 - ram cooldown, 3 - boost cooldown, 4 - attack cooldown, 5 - ram, 6 - slingshot timer
                            case 0:
                                Ramming = false;
                                if (ai[4] > 0)
                                    ai[4] -= SSS.Delta;

                                if (ai[0] == 0)
                                {
                                    updateNodes = true;

                                    Transform.ChangeVelocityX(NavController.NextNodePos.X < Transform.Centre.X ? -Speed : Speed);
                                    if (MathF.Abs(NavController.NextNodePos.X - Transform.Centre.X) < NavController.NodeDistance && Transform.Position.Y > NavController.NextNodePos.Y && isTouchingWall)
                                    {
                                        ai[0] = 1;
                                        Grounded = false;
                                    }
                                    if (ai[4] <= 0 && DistToPlayer < 500 && CanSeePlayer)
                                    {
                                        ai[0] = 3;
                                        ai[1] = 0;
                                        Transform.ChangeVelocityY(-Speed * 3);
                                    }
                                }
                                else if (ai[0] == 1)
                                {
                                    updateNodes = false;

                                    if (isTouchingWall)
                                    {
                                        if (Transform.Position.Y > NavController.NextNodePos.Y)
                                            Transform.ChangeVelocityY(-Speed);
                                        else
                                        {
                                            ai[0] = 0;
                                            Transform.ChangeVelocityY(-Speed * 2);
                                        }
                                    }
                                    if (ai[4] <= 0 && (DistToPlayer < Transform.WHMedian() || !isTouchingWall))
                                    {
                                        ai[0] = 3;
                                        ai[1] = 0;
                                        Transform.ChangeVelocityY(-Speed * 3);
                                    }
                                }
                                else if (ai[0] == 3)
                                {
                                    updateNodes = false;
                                    Ramming = MathF.Abs(ai[5]) > Speed * 2;
                                    int dir = Core._Player.Transform.Centre.X < Transform.Centre.X ? -1 : 1;
                                    ai[5] = MathHelper.Lerp(ai[5], 0, 0.1f);

                                    if (!Ramming)
                                    {
                                        Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, Speed * dir, 0.1f));
                                        Vector2 difVec = new Vector2(MathF.Abs(MathUtil.Diff(Transform.Centre.X, Core._Player.Transform.Centre.X)), MathF.Abs(MathUtil.Diff(Transform.Centre.Y, Core._Player.Transform.Centre.Y)));
                                        if (difVec.Y < Core._Player.Transform.Height + 2 && difVec.X < 64 && ai[2] <= 0)
                                        {
                                            ai[2] = 2.9f;
                                            AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                            PreRam = true;
                                        }
                                        if (ai[2] >= 2.5f - SSS.Delta)
                                        {
                                            ai[2] -= SSS.Delta;
                                            if (ai[2] <= 2.5f)
                                            {
                                                ai[2] = 2.5f;
                                                ai[5] = RamAmount * dir;
                                                AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                            }
                                        }
                                    }
                                    if (ai[2] > 0 && ai[2] <= 2.5f)
                                        ai[2] -= SSS.Delta;
                                    if (MathF.Abs(ai[5]) > 0.1f)
                                        Transform.ChangeVelocityX(ai[5]);

                                    if (ai[1] < MaxBoosts)
                                    {
                                        if (ai[3] <= 0)
                                        {
                                            if (Transform.Position.Y + 2 > Core._Player.Transform.Position.Y + Core._Player.Transform.Height && MathF.Abs(Transform.Velocity.X) < Speed)
                                            {
                                                Transform.ChangeVelocityY(-Speed * 3);
                                                ai[1] += 1;
                                                ai[3] = 0.5f;
                                            }
                                        }
                                        else
                                            ai[3] -= SSS.Delta;
                                    }

                                    if (Grounded)
                                    {
                                        ai[3] = 0;
                                        ai[0] = 4;
                                        ai[4] = 3;
                                        ai[2] = 0;
                                    }
                                }
                                else if (ai[0] == 4)
                                {
                                    updateNodes = false;
                                    Transform.ChangeVelocityX(0);
                                    if (ai[6] < 3)
                                    {
                                        ai[6] += SSS.Delta;
                                        if (ai[6] >= 0.5f && ai[6] < 0.6f)
                                        {
                                            ai[6] = 0.6f;
                                            Core.SpawnProjectile(new Projectile(0, this), Transform.Centre, dirToPlayer);
                                        }
                                    }
                                    else
                                    {
                                        ai[6] = 0;
                                        ai[0] = 0;
                                    }
                                }
                                break;
                            #endregion
                            #region Mad Citizen
                            //1 - boosts, 2 - ram cooldown, 3 - boost cooldown, 4 - attack cooldown, 5 - ram, 6 - climb timer, 7 - shoot chance
                            case 1:
                                Ramming = false;
                                float rs = 1.9f;
                                if (ai[4] > 0)
                                    ai[4] -= SSS.Delta;

                                if (ai[0] == 0)
                                {
                                    updateNodes = true;
                                    Transform.ChangeVelocityX(NavController.NextNodePos.X < Transform.Centre.X ? -Speed : Speed);
                                    if (Transform.Position.Y > NavController.NextNodePos.Y)
                                    {
                                        if (isTouchingWall)
                                        {
                                            ai[0] = 1;
                                            Grounded = false;
                                        }
                                        else if (MathF.Abs(NavController.NextNodePos.X - Transform.Centre.X) < Transform.Width)
                                        {
                                            if (Grounded)
                                                Transform.ChangeVelocityY(-Speed * 4);
                                            Transform.ChangeVelocityX(Transform.Velocity.X / 6);
                                        }
                                    }
                                    if (ai[4] <= 0 && DistToPlayer < 500 && CanSeePlayer)
                                    {
                                        ai[0] = 2;
                                        ai[2] = rs - 0.4f - SSS.Delta * 2;
                                        ai[1] = 0;
                                        Transform.ChangeVelocityY(-Speed * 3);
                                    }
                                }
                                else if (ai[0] == 1)
                                {
                                    updateNodes = false;
                                    if (Transform.Position.Y > NavController.NextNodePos.Y)
                                    {
                                        if (isTouchingWall)
                                        {
                                            if (ai[6] < 0.5f)
                                                ai[6] += SSS.Delta;
                                            else
                                                Transform.Velocity = new Vector2(Speed * -lastTouchedWallDir * 3, -Speed * 1.5f);
                                        }
                                        else
                                        {
                                            ai[6] = 0;
                                            Transform.ChangeVelocityX(Transform.Velocity.X + (Speed * lastTouchedWallDir * 0.4f));
                                            if (Grounded || MathF.Abs(Transform.Velocity.X) > Speed * 15) //extra precautions
                                                ai[0] = 0;
                                        }
                                    }
                                    else
                                    {
                                        ai[0] = 0;
                                        Transform.ChangeVelocityY(-Speed * 2);
                                    }
                                    if (ai[4] <= 0 && (DistToPlayer < Transform.WHMedian() * 2))
                                    {
                                        ai[0] = 2;
                                        ai[2] = rs - 0.4f - SSS.Delta * 2;
                                        ai[1] = 0;
                                    }
                                }
                                else if (ai[0] == 2)
                                {
                                    updateNodes = false;
                                    Ramming = MathF.Abs(ai[5]) > Speed * 2;
                                    int dir = Core._Player.Transform.Centre.X < Transform.Centre.X ? -1 : 1;
                                    int mdir = Core._Player.Transform.Centre.X < Transform.Centre.X + (40 * dir) ? -1 : 1;
                                    ai[5] = MathHelper.Lerp(ai[5], 0, 0.1f);

                                    if (!Ramming)
                                    {
                                        Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, Speed * mdir, 0.05f));
                                        Vector2 difVec = new Vector2(MathF.Abs(MathUtil.Diff(Transform.Centre.X, Core._Player.Transform.Centre.X)), MathF.Abs(MathUtil.Diff(Transform.Centre.Y, Core._Player.Transform.Centre.Y)));
                                        if (difVec.Y < Core._Player.Transform.Height * 1.4f && difVec.X < 86 && ai[2] <= 0)
                                        {
                                            ai[2] = rs;
                                            aiFlags[0] = true;
                                            ai[7] = Random.Shared.Next(0, 10);
                                            AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                            PreRam = true;
                                        }
                                        if (ai[2] >= rs - 0.4f - SSS.Delta && ai[2] < rs)
                                        {
                                            //ai[2] -= SSS.Delta;
                                            if (ai[2] <= rs - 0.4f)
                                            {
                                                ai[2] = rs - 0.4f;
                                                aiFlags[0] = false;
                                                ai[5] = RamAmount * dir;
                                                AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                            }
                                        }
                                    }
                                    if (ai[2] > 0)
                                    {
                                        ai[2] -= SSS.Delta;
                                        if (ai[7] <= 5 && ai[7] > 0 && ai[2] < 0.5f)
                                        {
                                            ai[7] = 0;
                                            Core.SpawnProjectile(new Projectile(1, this), Transform.Centre, dirToPlayer);
                                        }
                                    }
                                    if (MathF.Abs(ai[5]) > 0.1f)
                                        Transform.ChangeVelocityX(ai[5]);

                                    if (ai[1] < MaxBoosts)
                                    {
                                        if (ai[3] <= 0)
                                        {
                                            if (Transform.Position.Y + 2 > Core._Player.Transform.Position.Y + Core._Player.Transform.Height && MathF.Abs(Transform.Velocity.X) < Speed)
                                            {
                                                Transform.ChangeVelocityY(-Speed * 3);
                                                ai[1] += 1;
                                                ai[3] = 0.8f;
                                            }
                                        }
                                        else
                                            ai[3] -= SSS.Delta;
                                    }

                                    if (Grounded && !Ramming && !PreRam && timeInCurrentState > 1f && !aiFlags[0])
                                    {
                                        ai[3] = 0;
                                        ai[0] = 3;
                                        ai[4] = 1.5f;
                                        ai[2] = 0;
                                        ai[5] = 0;
                                    }
                                }
                                else if (ai[0] == 3)
                                {
                                    updateNodes = false;
                                    if (ai[4] > 0)
                                    {
                                        if (!isTouchingWall && Grounded)
                                        {
                                            int dir = MathUtil.IntSign(Transform.Centre.X - Core._Player.Transform.Centre.X);
                                            Transform.ChangeVelocityX(Speed / 2 * dir);
                                        }
                                        else if(isTouchingWall)
                                        {
                                            if (Grounded)
                                                Transform.ChangeVelocityY(-Speed * 3);
                                            Transform.ChangeVelocityX(0);
                                            if (Transform.Velocity.Y >= 0)
                                                Transform.ChangeVelocityY(0);
                                        }
                                    }
                                    bool dist = DistToPlayer < Transform.WHMedian() * 2;
                                    if ((ai[4] <= 0 || dist) && timeInCurrentState > 0.8f)
                                    {
                                        ai[0] = ai[4] <= 0 ? 0 : 2;
                                        ai[4] = 0;
                                        ai[1] = 0;
                                        if (dist)
                                            Transform.ChangeVelocityY(-Speed * 3);
                                    }
                                }
                                break;
                            #endregion
                            #region Bug S
                            //1 - sting cooldown, 2 - ram, 3 - ram cooldown, 4 - in pack, 5 & 6 - Random pos
                            case 2:
                                updateNodes = false;
                                Ramming = false;

                                if (ai[0] == 0)
                                {
                                    if (DistToPlayer < 80 && CanSeePlayer)
                                        ai[0] = 1;
                                    Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Centre.X < Core._Player.Transform.Centre.X ? Speed : -Speed, Transform.Velocity.X, 0.5f));
                                    if (Transform.Centre.X > Core._Player.Transform.Position.X && Transform.Centre.X < Core._Player.Transform.Position.X + (Core._Player.Transform.Width / 2))
                                        Transform.ChangeVelocityX(0);
                                    if (isTouchingWall && Transform.Position.Y > Core._Player.Transform.Position.Y + Core._Player.Transform.Height)
                                        Transform.ChangeVelocityY(-Speed);
                                }
                                else if (ai[0] == 1)
                                {
                                    Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Centre.X < Core._Player.Transform.Centre.X ? Speed : -Speed, Transform.Velocity.X, 0.5f));
                                    if (Grounded && DistToPlayer < 48)
                                    {
                                        Transform.ChangeVelocityY(-Speed);
                                        ai[0] = 2;
                                        ai[1] = 0;
                                    }
                                }
                                else if (ai[0] == 2)
                                {
                                    if (ai[4] == 0)
                                        ai[4] = JoinPack(this) ? 1 : -1;
                                    if (Grounded)
                                        Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Centre.X < Core._Player.Transform.Centre.X ? Speed / 4 : -Speed / 4, Transform.Velocity.X, 0.2f));
                                    ai[1] += SSS.Delta;
                                    if (ai[1] > 3f || (DistToPlayer < 16 && Grounded))
                                    {
                                        ai[1] = 0;
                                        ai[3] = 0;
                                        ai[0] = 3;
                                        Pack = GetPack(UUID, out packLeader, out packID);
                                        if (ai[4] == 1 && !packLeader)
                                        {
                                            ai[0] = 4;
                                            ai[5] = Random.Shared.Next(-16, 16);
                                            ai[6] = Random.Shared.Next(-16, 16);
                                        }
                                        ai[4] = 0;
                                        Transform.ChangeVelocityY(-Speed);
                                    }
                                }
                                else if (ai[0] == 3)
                                {
                                    Ramming = MathF.Abs(ai[2]) > Speed * 2;
                                    ai[2] = MathHelper.Lerp(ai[2], 0, 0.1f);
                                    if (Grounded || (!Ramming && ai[3] >= 0.8f))
                                    {
                                        ai[0] = 0;
                                        ai[1] = 0;
                                        ai[3] = 0;
                                        ai[2] = 0;
                                    }

                                    if (!Ramming)
                                    {
                                        ai[3] += SSS.Delta;
                                        if (ai[3] > 0.4f && ai[3] < 0.4f + SSS.Delta)
                                        {
                                            PreRam = true;
                                            AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                            ai[3] = 0.4f + SSS.Delta;
                                        }
                                        if (ai[3] >= 0.8f)
                                        {
                                            ai[2] = Transform.Centre.X < Core._Player.Transform.Centre.X ? RamAmount : -RamAmount;
                                            AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                        }
                                    }
                                    if (MathF.Abs(ai[2]) > 0.1f)
                                        Transform.ChangeVelocityX(ai[2]);
                                }
                                else if (ai[0] == 4)
                                {
                                    if (Pack != -1 && !packLeader && Packs.ElementAtOrDefault(Pack) != null)
                                    {
                                        NPC leader = Core.GetNPC(Packs[Pack].First());
                                        Ramming = leader.Ramming;

                                        Vector2 targetVel = Vector2.Normalize(leader.Transform.Centre + new Vector2(ai[5], ai[6]) - Transform.Centre);
                                        //targetVel = Vector2.Normalize(leader.Transform.Centre - (targetVel * packID * 16) - Transform.Centre);
                                        targetVel *= leader.Transform.Velocity.Length();

                                        float timeVal1 = (float)time.TotalGameTime.TotalSeconds * 8 + ai[5];
                                        targetVel += new Vector2(MathF.Sin(timeVal1), MathF.Cos(timeVal1)) / 3;
                                        Transform.Velocity = Vector2.Lerp(Transform.Velocity, targetVel * Speed, 0.2f) * (Ramming ? 1.3f : 1);
                                    }
                                    else
                                        ai[0] = 0;
                                }
                                break;
                            #endregion
                            #region Sinkskipper
                            //1 - Ram timer, 2 - Secondary ram timer, 3 - Greater attack timer, 4 - Ram amount
                            case 3:
                                Phantom = false;
                                StopDraw = false;
                                updateNodes = false;
                                Ramming = false;

                                if (ai[0] == 0)
                                {
                                    Phantom = true;
                                    Transform.Position = initPos;
                                    StopDraw = true;
                                }
                                if (ai[0] == 1)
                                {
                                    Ramming = MathF.Abs(Transform.Velocity.Y) > Speed * 2;
                                    if (Transform.Position.Y + Transform.Height + 32 < Core._Player.Transform.Position.Y && ai[1] <= 0 && !Ramming && !PreRam)
                                    {
                                        ai[1] = 0.4f;
                                        PreRam = true;
                                        AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                    }

                                    int side = Transform.Centre.X < Core._Player.Transform.Centre.X ? 1 : -1;
                                    Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, side + MathF.Cos((float)time.TotalGameTime.TotalSeconds * 8), 0.1f));

                                    if (ai[1] > 0)
                                        ai[1] -= SSS.Delta;
                                    else if (PreRam)
                                    {
                                        Ramming = true;
                                        Transform.ChangeVelocityY(RamAmount);
                                        AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                    }

                                    if (Grounded)
                                    {
                                        ai[0] = 2;
                                        ai[1] = 0;
                                        ai[2] = 0;
                                        ai[3] = 0;
                                    }
                                }
                                else if (ai[0] == 2)
                                {
                                    Ramming = MathF.Abs(ai[4]) > Speed * 2;
                                    ai[4] = MathHelper.Lerp(ai[4], 0, 0.1f);
                                    Transform.ChangeVelocityX(0);
                                    if (ai[2] < 1f)
                                    {
                                        ai[2] += SSS.Delta;
                                        if (ai[2] > 0.6f && ai[3] <= 1)
                                        {
                                            PreRam = true;
                                            if (ai[2] < 0.6f + SSS.Delta)
                                            {
                                                ai[2] = 0.6f + SSS.Delta;
                                                AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                            }
                                        }
                                    }
                                    else if (ai[2] >= 1f && Grounded)
                                    {
                                        ai[2] = 0f;
                                        if (ai[3] <= 1)
                                        {
                                            ai[3]++;
                                            Ramming = true;
                                            AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                            Transform.Velocity = new Vector2(0, -Speed);
                                            ai[4] = Transform.Centre.X < Core._Player.Transform.Centre.X ? RamAmount : -RamAmount;
                                        }
                                        else
                                        {
                                            Core.SpawnProjectile(new Projectile(1, this), Transform.Centre, Vector2.Normalize(Core._Player.Transform.Centre - Transform.Centre));
                                            ai[3] = 0;
                                            ai[2] = 0;
                                            ai[4] = 0;
                                            ai[0] = 3;
                                        }
                                    }
                                    if (MathF.Abs(ai[4]) > 0.1f)
                                        Transform.ChangeVelocityX(ai[4]);
                                }
                                else if (ai[0] == 3)
                                {
                                    Phantom = true;
                                    Vector2 end = initPos + new Vector2(32);
                                    float goofyAmount = (float)time.TotalGameTime.TotalSeconds * 8;
                                    Vector2 goofyness = new Vector2(MathF.Sin(goofyAmount), MathF.Cos(goofyAmount));
                                    Transform.Velocity = Vector2.Lerp(Transform.Velocity, Vector2.Normalize(end - Transform.Centre) + goofyness, 0.1f);

                                    if (Transform.Centre.Y > end.Y)
                                        ai[0] = 0;
                                }
                                break;
                            #endregion
                            #region Guard
                            //1 - Ram timer, 2 - Ilde timer, 3 - Sword timer (flick and stab), 4 - Ram
                            case 4:
                                Ramming = false;
                                gPreRamCheck = true;

                                if (ai[0] == 0)
                                {
                                    updateNodes = true;
                                    Transform.ChangeVelocityX(NavController.NextNodePos.X < Transform.Centre.X ? -Speed : Speed);
                                    if (ai[4] <= 0 && DistToPlayer < 150 && CanSeePlayer)
                                    {
                                        ai[0] = 2;
                                        ai[1] = 0;
                                        Transform.ChangeVelocityY(-Speed * 3);
                                    }
                                }
                                else if (ai[0] == 1)
                                {
                                    updateNodes = false;
                                    Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, 0f, 0.1f));
                                    if (ai[2] > 0)
                                        ai[2] -= SSS.Delta;
                                    else
                                    {
                                        ai[0] = Random.Shared.Next(2, 5);
                                        ai[1] = 0;
                                        ai[3] = 0;
                                        if (ai[0] == 2)
                                            Transform.ChangeVelocityY(-Speed * 3);
                                    }

                                    if (DistToPlayer > 300 || !CanSeePlayer)
                                        ai[0] = 0;
                                }
                                else if (ai[0] == 2)
                                {
                                    updateNodes = false;
                                    gPreRamCheck = false;
                                    Ramming = Transform.Velocity.Y > Speed * 2;
                                    int side = Transform.Centre.X < Core._Player.Transform.Centre.X ? 1 : -1;
                                    if (!Grounded)
                                    {
                                        Transform.ChangeVelocityX(side + MathF.Cos((float)time.TotalGameTime.TotalSeconds * 4) * 0.3f * Speed);

                                        bool aligned = Core._Player.Transform.Centre.X > Transform.Position.X && Core._Player.Transform.Centre.X < Transform.Position.X + Transform.Width;
                                        if (aligned && !Ramming && !PreRam)
                                        {
                                            PreRam = true;
                                            AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                            ai[1] = 0.4f;
                                        }
                                        else if (aligned)
                                            Transform.ChangeVelocityX(0f);

                                        if (PreRam)
                                        {
                                            if (ai[1] > 0)
                                                ai[1] -= SSS.Delta;
                                            else
                                            {
                                                Transform.ChangeVelocityY(RamAmount);
                                                AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                                Ramming = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ai[0] = 1;
                                        ai[2] = 2;
                                        if (Transform.OldVelocity.Y > 0)
                                        {
                                            var proj = new Projectile(2, this);
                                            Core.SpawnProjectile(proj, Transform.Centre - new Vector2(0, Transform.Height), new Vector2(side / proj.Transform.Weight, -0.5f));
                                        }
                                    }
                                }
                                else if (ai[0] == 3)
                                {
                                    ai[4] = MathHelper.Lerp(ai[4], 0, 0.1f);
                                    ai[3] += SSS.Delta;
                                    updateNodes = false;
                                    Transform.ChangeVelocityX(0);

                                    if (ai[3] < 0.4f)
                                    {
                                        if (!PreRam)
                                        {
                                            PreRam = true;
                                            AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                        }
                                    }
                                    else if (ai[3] > 1.5f)
                                    {
                                        ai[0] = 1;
                                        ai[3] = 0;
                                        ai[2] = 1;
                                        PreRam = false;
                                        Hitboxes.Remove(1);
                                    }

                                    if (ai[3] >= 0.8f && ai[3] <= 0.8f + SSS.Delta)
                                    {
                                        int side = Transform.Centre.X < Core._Player.Transform.Centre.X ? 1 : -1;
                                        Hitboxes[1] = MakeHBSide(new Rectangle(0, Transform.Height - 12, 32, 8), side);
                                        AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                        ai[4] = side * 3;
                                        ai[3] = 0.9f + SSS.Delta;
                                    }

                                    if (MathF.Abs(ai[4]) > 0.05f)
                                        Transform.ChangeVelocityX(ai[4]);
                                }
                                else if (ai[0] == 4)
                                {
                                    ai[3] += SSS.Delta;
                                    updateNodes = false;
                                    Transform.ChangeVelocityX(0);

                                    if (ai[3] < 0.4f)
                                    {
                                        if (!PreRam)
                                        {
                                            PreRam = true;
                                            AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                            Transform.ChangeVelocityY(-Speed);
                                        }
                                    }
                                    else if (ai[3] <= 0.4f + SSS.Delta)
                                    {
                                        PreRam = false;
                                        AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                        Transform.ChangeVelocityY(RamAmount);
                                        ai[3] = 0.6f;
                                    }

                                    if (Grounded && ai[3] >= 0.4f)
                                    {
                                        PreRam = false;
                                        var proj = new Projectile(2, this);
                                        float xvel = Transform.Centre.X < Core._Player.Transform.Centre.X ? 1 : -1;
                                        xvel = xvel / (proj.Transform.Weight * 2);

                                        Core.SpawnProjectile(proj, Transform.Centre, new Vector2(xvel, -0.5f));
                                        proj = new Projectile(2, this);
                                        Core.SpawnProjectile(proj, Transform.Centre, new Vector2(xvel, -0.2f));

                                        ai[0] = 1;
                                        ai[3] = 0;
                                        ai[2] = 1;
                                    }
                                }
                                break;
                            #endregion
                            #region Seeker
                            //1 - boosts, 2 - ram & fling cooldown, 3 - boost cooldown, 4 - attack cooldown, 5 - ram, 6 - jet timer, 7 - projectile timer 1
                            case 5:
                                Ramming = false;
                                CantStun = false;
                                if (ai[4] > 0)
                                    ai[4] -= SSS.Delta;

                                if (ai[0] == 0)
                                {
                                    updateNodes = true;
                                    Transform.ChangeVelocityX(NavController.NextNodePos.X < Transform.Centre.X ? -Speed : Speed);
                                    if (Transform.Position.Y > NavController.NextNodePos.Y)
                                    {
                                        if (isTouchingWall)
                                        {
                                            ai[0] = 1;
                                            Grounded = false;
                                        }
                                        else if (MathF.Abs(NavController.NextNodePos.X - Transform.Centre.X) < Transform.Width)
                                        {
                                            if (Grounded)
                                                Transform.ChangeVelocityY(-Speed * 3);
                                            Transform.ChangeVelocityX(Transform.Velocity.X / 6);
                                        }
                                    }
                                    if (ai[4] <= 0 && DistToPlayer < 200 && CanSeePlayer)
                                    {
                                        ai[0] = 2;
                                        ai[1] = 0;
                                        Transform.ChangeVelocityY(-Speed * 2);
                                    }
                                }
                                else if (ai[0] == 1)
                                {
                                    updateNodes = false;
                                    if (isTouchingWall)
                                    {
                                        if (Transform.Position.Y > NavController.NextNodePos.Y)
                                            Transform.ChangeVelocityY(-Speed);
                                        else
                                        {
                                            ai[0] = 0;
                                            Transform.ChangeVelocityY(-Speed * 2);
                                        }
                                    }
                                    if (isTouchingCeiling || (ai[4] <= 0 && (DistToPlayer < Transform.WHMedian() * 2)))
                                    {
                                        ai[0] = 2;
                                        ai[1] = 0;
                                        ai[4] = 0;
                                    }
                                }
                                else if (ai[0] == 2)
                                {
                                    updateNodes = false;
                                    Ramming = MathF.Abs(ai[5]) > Speed * 2;
                                    int dir = Core._Player.Transform.Centre.X < Transform.Centre.X ? -1 : 1;
                                    int mdir = Core._Player.Transform.Centre.X < Transform.Centre.X + (40 * dir) ? -1 : 1;
                                    ai[5] = MathHelper.Lerp(ai[5], 0, 0.1f);

                                    if (lastTickStunned)
                                    {
                                        PreRam = false;
                                        ai[2] = 0;
                                    }

                                    if (!Ramming)
                                    {
                                        Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, Speed * mdir, 0.1f));
                                        Vector2 difVec = new Vector2(MathF.Abs(MathUtil.Diff(Transform.Centre.X, Core._Player.Transform.Centre.X)), MathF.Abs(MathUtil.Diff(Transform.Centre.Y, Core._Player.Transform.Centre.Y)));
                                        if (difVec.Y < Core._Player.Transform.Height * 1.4f && difVec.X < 86)
                                        {
                                            Transform.ChangeVelocityY(MathHelper.Lerp(Transform.Velocity.Y, Transform.Centre.Y < Core._Player.Transform.Centre.Y ? 1 : -1, 0.1f));
                                            if (ai[2] <= 0)
                                            {
                                                ai[2] = 1.9f;
                                                AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                                PreRam = true;
                                            }
                                        }
                                        if (ai[2] >= 1.3f - SSS.Delta)
                                        {
                                            ai[2] -= SSS.Delta;
                                            if (ai[2] <= 1.3f)
                                            {
                                                ai[2] = 1.3f;
                                                ai[5] = RamAmount * dir;
                                                AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                            }
                                        }
                                    }
                                    if (ai[2] > 0 && ai[2] <= 1.5f)
                                        ai[2] -= SSS.Delta;

                                    if (MathF.Abs(ai[5]) > 0.1f)
                                        Transform.ChangeVelocityX(ai[5]);

                                    if (ai[1] < MaxBoosts)
                                    {
                                        if (ai[3] <= 0)
                                        {
                                            if (Transform.Position.Y + 2 > Core._Player.Transform.Position.Y + Core._Player.Transform.Height && MathF.Abs(Transform.Velocity.X) < Speed * 2)
                                            {
                                                Transform.ChangeVelocityY(-Speed * 2);
                                                ai[1] += 1;
                                                ai[3] = 0.8f;
                                                if (ai[1] == 1)
                                                {
                                                    ai[7] = 0.35f;
                                                    Core.SpawnProjectile(new Projectile(1, this), Transform.Centre, dirToPlayer);
                                                }
                                            }
                                        }
                                        else
                                            ai[3] -= SSS.Delta;
                                    }

                                    if (ai[7] > 0)
                                    {
                                        ai[7] -= SSS.Delta;
                                        if (ai[7] <= SSS.Delta)
                                            Core.SpawnProjectile(new Projectile(1, this), Transform.Centre, dirToPlayer);
                                    }

                                    if (Grounded)
                                    {
                                        ai[3] = 0;
                                        ai[0] = 4;
                                        ai[2] = 0;
                                        Transform.ChangeVelocityY(-Speed * 2);
                                    }
                                }
                                else if (ai[0] == 3)
                                {
                                    updateNodes = false;
                                    if (ai[4] > 0)
                                    {
                                        if (!isTouchingWall && Grounded)
                                        {
                                            int dir = MathUtil.IntSign(Transform.Centre.X - Core._Player.Transform.Centre.X);
                                            Transform.ChangeVelocityX(Speed / 2 * dir);
                                        }
                                        else if (isTouchingWall)
                                        {
                                            Transform.ChangeVelocityY(-Speed / 2);
                                            Transform.ChangeVelocityX(0);
                                        }
                                    }
                                    if (ai[4] <= 0 || (ai[4] <= 1.5f && DistToPlayer < Transform.WHMedian() * 2))
                                    {
                                        ai[4] = 0;
                                        ai[1] = 0;
                                        ai[0] = 0;
                                        Transform.ChangeVelocityY(-Speed * 2);
                                    }
                                }
                                else if (ai[0] == 4)
                                {
                                    CantStun = true;
                                    updateNodes = false;
                                    Ramming = Transform.Velocity.Y > Speed * 2;
                                    if (AlignWithPlayer(0, true) && Transform.Position.Y + Transform.Height + 20 < Core._Player.Transform.Position.Y && !PreRam && !Ramming && ai[2] >= 0)
                                    {
                                        PreRam = true;
                                        AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                        ai[2] = 0.6f;
                                    }
                                    if (ai[2] > 0)
                                    {
                                        ai[2] -= SSS.Delta;
                                        if (ai[2] <= SSS.Delta)
                                        {
                                            Transform.ChangeVelocityY(RamAmount);
                                            AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                        }
                                    }

                                    bool below = ai[2] < -1f;
                                    if (below ? below && Grounded : Grounded)
                                        ai[0] = 5;

                                    if (ai[2] < 0)
                                    {
                                        ai[2] -= SSS.Delta;
                                        Transform.ChangeVelocityX(0);
                                    }
                                    else
                                        Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, Transform.Centre.X < Core._Player.Transform.Centre.X ? Speed : -Speed, 0.1f));
                                }
                                else if (ai[0] == 5)
                                {
                                    if (ai[6] < 5f)
                                    {
                                        Transform.Velocity = Vector2.Lerp(Transform.Velocity, -dirToPlayer * 2, 0.07f);
                                        Transform.Velocity += new Vector2(MathF.Sin((float)time.TotalGameTime.TotalSeconds * 4) / 4, MathF.Cos((float)time.TotalGameTime.TotalSeconds * 4) / 4);
                                        float comShootTime = MathF.Floor(ai[6]);
                                        if (ai[6] < comShootTime + SSS.Delta && ai[6] > comShootTime)
                                            Core.SpawnProjectile(new Projectile(1, this), Transform.Centre, dirToPlayer);
                                        ai[6] += SSS.Delta;
                                    }
                                    else if (Grounded)
                                    {
                                        ai[4] = 3;
                                        ai[0] = 3;
                                        ai[6] = 0;
                                        ai[2] = 0;
                                        ai[1] = 0;
                                    }
                                }
                                break;
                            #endregion
                            #region Chloravoth
                            //1 - grand attack timer, 2 - swipe timer, 3 - general cooldown timer, 4 - bug blocker
                            case 6:
                                Ramming = Transform.Velocity.Y > 1;
                                Transform.ChangeVelocityX(0);
                                updateNodes = false;
                                if (ai[0] == 0 && DistToPlayer < 200)
                                {
                                    if (ai[1] == 0)
                                        ai[1] = Random.Shared.Next(0, 10) <= 3 ? 0.5f : 2.5f;
                                    else if (ai[1] > 0)
                                        ai[1] -= SSS.Delta;
                                    else
                                    {
                                        ai[1] = 0;
                                        ai[2] = 0;
                                        ai[3] = 0;
                                        ai[0] = Random.Shared.Next(1, ai[4] != 1 ? 4 : 3);
                                    }
                                }
                                else if (ai[0] == 1)
                                {
                                    if (!PreRam && ai[2] == 0)
                                    {
                                        PreRam = true;
                                        AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                        ai[2] = 0.4f;
                                    }
                                    else if (ai[2] > 0)
                                        ai[2] -= SSS.Delta;
                                    if (PreRam && ai[2] <= 0f)
                                    {
                                        Hitboxes[1] = MakeHBSide(new Rectangle(0, Transform.Height - 12, 32, 8));
                                        AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                        PreRam = false;
                                        ai[2] = -1f;
                                    }
                                    else if (ai[2] <= -1f)
                                    {
                                        ai[2] -= SSS.Delta;
                                        if (ai[2] <= -2f)
                                        {
                                            Hitboxes.Remove(1);
                                            ai[2] = 0;
                                            ai[0] = 0;
                                        }
                                    }
                                }
                                else if (ai[0] == 2)
                                {
                                    if (ai[3] < 6f)
                                        ai[3] += SSS.Delta * 2;
                                    else
                                    {
                                        ai[3] = 0;
                                        ai[0] = 0;
                                    }
                                    float lockin = (float)MathF.Ceiling(ai[3]);
                                    if (ai[3] >= lockin - (SSS.Delta * 2))
                                    {
                                        int side = Transform.Centre.X < Core._Player.Transform.Centre.X ? 1 : -1;
                                        Core.SpawnProjectile(new Projectile(2, this), Transform.Centre, new Vector2(side / 4, -0.5f));
                                    }
                                }
                                else if (ai[0] == 3)
                                {
                                    if (ai[3] < 4f)
                                        ai[3] += SSS.Delta;
                                    else
                                    {
                                        ai[3] = 0;
                                        ai[0] = 0;
                                    }
                                    if (ai[3] >= 1 - SSS.Delta && ai[3] <= 1f)
                                    {
                                        ai[4] = 1;
                                        for (int i = 0; i < 5; i++)
                                            Core.SpawnNPC(new NPC(2, 200, 0, PhantomType.None), Transform.Centre + new Vector2(i * 2, 0));
                                    }
                                }
                                break;
                            #endregion
                            #region Mad Tramp (no projectile)
                            //1 - boosts, 2 - ram cooldown, 3 - boost cooldown, 4 - attack cooldown, 5 - ram, 6 - slingshot timer
                            case 7:
                                Ramming = false;
                                if (ai[4] > 0)
                                    ai[4] -= SSS.Delta;

                                if (ai[0] == 0)
                                {
                                    updateNodes = true;

                                    Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, NavController.NextNodePos.X < Transform.Centre.X ? -Speed : Speed, 0.1f));
                                    if (MathF.Abs(NavController.NextNodePos.X - Transform.Centre.X) < NavController.NodeDistance && Transform.Position.Y > NavController.NextNodePos.Y && isTouchingWall)
                                    {
                                        ai[0] = 1;
                                        Grounded = false;
                                    }
                                    if (ai[4] <= 0 && DistToPlayer < 500 && CanSeePlayer)
                                    {
                                        ai[0] = 3;
                                        ai[1] = 0;
                                        Transform.ChangeVelocityY(-Speed * 3);
                                    }
                                }
                                else if (ai[0] == 1)
                                {
                                    updateNodes = false;

                                    if (isTouchingWall)
                                    {
                                        if (Transform.Position.Y > NavController.NextNodePos.Y)
                                            Transform.ChangeVelocityY(-Speed);
                                        else
                                        {
                                            ai[0] = 0;
                                            Transform.ChangeVelocityY(-Speed * 2);
                                        }
                                    }
                                    if (ai[4] <= 0 && (DistToPlayer < Transform.WHMedian() || !isTouchingWall))
                                    {
                                        ai[0] = 3;
                                        ai[1] = 0;
                                        Transform.ChangeVelocityY(-Speed * 3);
                                    }
                                }
                                else if (ai[0] == 3)
                                {
                                    updateNodes = false;
                                    Ramming = MathF.Abs(ai[5]) > Speed * 2;
                                    int dir = Core._Player.Transform.Centre.X < Transform.Centre.X ? -1 : 1;
                                    ai[5] = MathHelper.Lerp(ai[5], 0, 0.1f);

                                    if (!Ramming)
                                    {
                                        Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, Speed * dir, 0.1f));
                                        Vector2 difVec = new Vector2(MathF.Abs(MathUtil.Diff(Transform.Centre.X, Core._Player.Transform.Centre.X)), MathF.Abs(MathUtil.Diff(Transform.Centre.Y, Core._Player.Transform.Centre.Y)));
                                        if (difVec.Y < Core._Player.Transform.Height + 2 && difVec.X < 64 && ai[2] <= 0)
                                        {
                                            ai[2] = 2.9f;
                                            AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                            PreRam = true;
                                        }
                                        if (ai[2] >= 2.5f - SSS.Delta)
                                        {
                                            ai[2] -= SSS.Delta;
                                            if (ai[2] <= 2.5f)
                                            {
                                                ai[2] = 2.5f;
                                                ai[5] = RamAmount * dir;
                                                AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                            }
                                        }
                                    }
                                    if (ai[2] > 0 && ai[2] <= 2.5f)
                                        ai[2] -= SSS.Delta;
                                    if (MathF.Abs(ai[5]) > 0.1f)
                                        Transform.ChangeVelocityX(ai[5]);

                                    if (ai[1] < MaxBoosts)
                                    {
                                        if (ai[3] <= 0)
                                        {
                                            if (Transform.Position.Y + 2 > Core._Player.Transform.Position.Y + Core._Player.Transform.Height && MathF.Abs(Transform.Velocity.X) < Speed)
                                            {
                                                Transform.ChangeVelocityY(-Speed * 3);
                                                ai[1] += 1;
                                                ai[3] = 0.5f;
                                            }
                                        }
                                        else
                                            ai[3] -= SSS.Delta;
                                    }

                                    if (Grounded && !PreRam && !Ramming)
                                    {
                                        ai[3] = 0;
                                        ai[0] = 4;
                                        ai[4] = 3;
                                        ai[2] = 0;
                                    }
                                }
                                else if (ai[0] == 4)
                                {
                                    updateNodes = false;
                                    Transform.ChangeVelocityX(0);
                                    if (ai[6] < 1f)
                                        ai[6] += SSS.Delta;
                                    else
                                    {
                                        ai[6] = 0;
                                        ai[0] = 0;
                                    }
                                }
                                break;
                            #endregion
                            #region Mad Citizen (no projectile)
                            //1 - boosts, 2 - ram cooldown, 3 - boost cooldown, 4 - attack cooldown, 5 - ram, 6 - climb timer, 7 - shoot chance
                            case 8:
                                Ramming = false;
                                rs = 2.1f;
                                if (ai[4] > 0)
                                    ai[4] -= SSS.Delta;

                                if (ai[0] == 0)
                                {
                                    updateNodes = true;
                                    Transform.ChangeVelocityX(NavController.NextNodePos.X < Transform.Centre.X ? -Speed : Speed);
                                    if (Transform.Position.Y > NavController.NextNodePos.Y)
                                    {
                                        if (isTouchingWall)
                                        {
                                            ai[0] = 1;
                                            Grounded = false;
                                        }
                                        else if (MathF.Abs(NavController.NextNodePos.X - Transform.Centre.X) < Transform.Width)
                                        {
                                            if (Grounded)
                                                Transform.ChangeVelocityY(-Speed * 4);
                                            Transform.ChangeVelocityX(Transform.Velocity.X / 6);
                                        }
                                    }
                                    if (ai[4] <= 0 && DistToPlayer < 500 && CanSeePlayer)
                                    {
                                        ai[0] = 2;
                                        ai[2] = rs - 0.4f - SSS.Delta * 2;
                                        ai[1] = 0;
                                        Transform.ChangeVelocityY(-Speed * 3);
                                    }
                                }
                                else if (ai[0] == 1)
                                {
                                    updateNodes = false;
                                    if (Transform.Position.Y > NavController.NextNodePos.Y)
                                    {
                                        if (isTouchingWall)
                                        {
                                            if (ai[6] < 0.5f)
                                                ai[6] += SSS.Delta;
                                            else
                                                Transform.Velocity = new Vector2(Speed * -lastTouchedWallDir * 3, -Speed * 1.5f);
                                        }
                                        else
                                        {
                                            ai[6] = 0;
                                            Transform.ChangeVelocityX(Transform.Velocity.X + (Speed * lastTouchedWallDir * 0.8f));
                                            if (Grounded || MathF.Abs(Transform.Velocity.X) > Speed * 15) //extra precautions
                                                ai[0] = 0;
                                        }
                                    }
                                    else
                                    {
                                        ai[0] = 0;
                                        Transform.ChangeVelocityY(-Speed * 2);
                                    }
                                    if (ai[4] <= 0 && (DistToPlayer < Transform.WHMedian() * 2))
                                    {
                                        ai[0] = 2;
                                        ai[2] = rs - 0.4f - SSS.Delta * 2;
                                        ai[1] = 0;
                                    }
                                }
                                else if (ai[0] == 2)
                                {
                                    updateNodes = false;
                                    Ramming = MathF.Abs(ai[5]) > Speed * 2;
                                    int dir = Core._Player.Transform.Centre.X < Transform.Centre.X ? -1 : 1;
                                    int mdir = Core._Player.Transform.Centre.X < Transform.Centre.X + (40 * dir) ? -1 : 1;
                                    ai[5] = MathHelper.Lerp(ai[5], 0, 0.1f);

                                    if (!Ramming)
                                    {
                                        Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, Speed * mdir, 0.1f));
                                        Vector2 difVec = new Vector2(MathF.Abs(MathUtil.Diff(Transform.Centre.X, Core._Player.Transform.Centre.X)), MathF.Abs(MathUtil.Diff(Transform.Centre.Y, Core._Player.Transform.Centre.Y)));
                                        if (difVec.Y < Core._Player.Transform.Height * 1.4f && difVec.X < 86 && ai[2] <= 0)
                                        {
                                            ai[2] = rs;
                                            ai[7] = Random.Shared.Next(0, 10);
                                            AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                            aiFlags[0] = true;
                                            PreRam = true;
                                        }
                                        if (ai[2] >= rs - 0.4f - SSS.Delta && ai[2] < rs)
                                        {
                                            //ai[2] -= SSS.Delta;
                                            if (ai[2] <= rs - 0.4f)
                                            {
                                                aiFlags[0] = false;
                                                ai[2] = rs - 0.4f;
                                                ai[5] = RamAmount * dir;
                                                AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                            }
                                        }
                                    }
                                    if (ai[2] > 0)
                                        ai[2] -= SSS.Delta;

                                    if (MathF.Abs(ai[5]) > 0.1f)
                                        Transform.ChangeVelocityX(ai[5]);

                                    if (ai[1] < MaxBoosts)
                                    {
                                        if (ai[3] <= 0)
                                        {
                                            if (Transform.Position.Y + 2 > Core._Player.Transform.Position.Y + Core._Player.Transform.Height && MathF.Abs(Transform.Velocity.X) < Speed)
                                            {
                                                Transform.ChangeVelocityY(-Speed * 3);
                                                ai[1] += 1;
                                                ai[3] = 0.8f;
                                            }
                                        }
                                        else
                                            ai[3] -= SSS.Delta;
                                    }

                                    if (Grounded && !Ramming && !PreRam && timeInCurrentState > 1f && aiFlags[0])
                                    {
                                        ai[3] = 0;
                                        ai[0] = 3;
                                        ai[4] = 1.5f;
                                        ai[2] = rs - 0.4f - SSS.Delta * 2;
                                    }
                                }
                                else if (ai[0] == 3)
                                {
                                    updateNodes = false;
                                    if (ai[4] > 0)
                                    {
                                        if (!isTouchingWall && Grounded)
                                        {
                                            int dir = MathUtil.IntSign(Transform.Centre.X - Core._Player.Transform.Centre.X);
                                            Transform.ChangeVelocityX(Speed / 2 * dir);
                                        }
                                        else
                                        {
                                            if (Grounded)
                                                Transform.ChangeVelocityY(-Speed * 3);
                                            Transform.ChangeVelocityX(0);
                                            if (Transform.Velocity.Y >= 0)
                                                Transform.ChangeVelocityY(0);
                                        }
                                    }
                                    bool dist = DistToPlayer < Transform.WHMedian() * 2;
                                    if (ai[4] <= 0 || dist)
                                    {
                                        ai[0] = ai[4] <= 0 ? 0 : 2;
                                        ai[4] = 0;
                                        ai[1] = 0;
                                        if (dist)
                                            Transform.ChangeVelocityY(-Speed * 3);
                                    }
                                }
                                break;
                            #endregion
                            #region Soano Stalker
                            case 9:
                                Ramming = false;
                                StopDraw = ai[0] == 0 || ai[1] == 0 ? true : false;

                                if (ai[0] == 0)
                                {
                                    Transform.Velocity = Vector2.Zero;
                                    ai[1] = 25;
                                    ai[2] = 0;
                                    if (ai[3] != -1)
                                    {
                                        AudioSystem.StopEvent((int)ai[3], false);
                                        ai[3] = -1;
                                    }
                                }
                                else
                                    ai[1] -= SSS.Delta;

                                if (ai[0] == 1)
                                {
                                    int[] stages = new int[4] { 5, 10, 15, 0 };
                                    if (stages.Where(x => ai[1] <= x + SSS.Delta && ai[1] >= x).Count() == 1 || DistToPlayer < 48)
                                    {
                                        int x = Random.Shared.Next(296, 296 + 1440);
                                        int y = Random.Shared.Next(568, 568 + 1552);
                                        Transform.Position = new Vector2(x, y);

                                        if (ai[3] > -1)
                                            AudioSystem.StopEvent((int)ai[3], false);
                                        if (ai[1] > 1 + SSS.Delta)
                                        {
                                            ai[3] = AudioSystem.PlayEvent("head", false, Transform.Centre, true, 0.8f);
                                        }
                                        else
                                        {
                                            ai[3] = AudioSystem.PlayEvent("head", false, Vector2.Zero, true, 1, false);
                                            ai[0] = 2;
                                            Transform.Position = Core._Player.Transform.Position + (dirToPlayer * 128);
                                        }
                                    }
                                    Transform.Velocity = dirToPlayer * 0.4f;
                                }
                                else if (ai[0] == 2)
                                {
                                    Transform.Velocity = dirToPlayer;
                                    if (CollisionEngine.RectInRect(Transform.GetRectangle(), Core._Player.Transform.GetRectangle()))
                                    {
                                        AudioSystem.StopEvent((int)ai[3], false);
                                        Invincible = false;
                                        Kill();
                                        Core.Die();
                                    }
                                }
                                break;
                            #endregion
                            #region Rock Crawdad
                            //0 - Burrowed, 1 - Backing off, 2 - Ramming, 3 - Packing, 4 - Climbing
                            case 10:
                                Ramming = false;
                                updateNodes = false;
                                CantStun = true;

                                if (ai[0] == 0)
                                {
                                    Transform.Velocity = new Vector2(0, Transform.Velocity.Y);
                                    if (DistToPlayer <= 32f || MathF.Abs(Transform.Velocity.Y) >= 2f || SimplePackCheck(UUID))
                                    {
                                        ai[0] = Random.Shared.Next(1, 3);
                                        ai[1] = 2;
                                        ai[5] = JoinPack(this) ? 1 : 0;
                                    }
                                }
                                else if (ai[0] == 1)
                                {
                                    float dir = Transform.Position.X < Core._Player.Transform.Position.X ? -1 : 1;
                                    dir = Core._Player.Speed * (DistToPlayer < 48 ? 1f : 0.25f) * dir;
                                    Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, dir, 0.1f));

                                    if (ai[1] <= 0f)
                                    {
                                        ai[0] = 2;
                                        if (ai[5] == 1)
                                        {
                                            Pack = GetPack(UUID, out packLeader, out packID);
                                            if (!packLeader)
                                                ai[0] = 3;
                                            ai[6] = 0;
                                        }
                                    }
                                    else
                                        ai[1] -= SSS.Delta;
                                }
                                else if (ai[0] == 2)
                                {
                                    Ramming = MathF.Abs(ai[2]) > Speed * 2;
                                    ai[2] = MathHelper.Lerp(ai[2], 0, 0.1f);

                                    float dir = Transform.Position.X < Core._Player.Transform.Position.X ? 1 : -1;
                                    if (DistToPlayer < 48 || ai[3] != 0)
                                    {
                                        if (Grounded && ai[3] == 0f)
                                            Transform.Velocity = new Vector2(Transform.Velocity.X, -1);
                                        else
                                        {
                                            if (ai[3] == 0f)
                                            {
                                                PreRam = true;
                                                AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                            }

                                            if (ai[3] < 0.4f)
                                                ai[3] += SSS.Delta;
                                            else if (ai[3] < 10f)
                                            {
                                                ai[2] = RamAmount * dir;
                                                AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                                ai[3] = 10f;
                                            }
                                        }
                                    }
                                    else if (ai[3] != 10f)
                                    {
                                        dir = Speed * dir * 1.5f;
                                        Transform.ChangeVelocityX(dir);
                                    }

                                    if (MathF.Abs(ai[2]) > 0.1f)
                                        Transform.ChangeVelocityX(ai[2]);

                                    bool above = MathF.Abs(Transform.Position.X - Core._Player.Transform.Position.X) < 3 && Transform.Position.Y > Core._Player.Transform.Position.Y + Core._Player.Transform.Height;
                                    if (((!Ramming && !PreRam && Grounded) || above) && ai[3] >= 2f)
                                    {
                                        PreRam = false;
                                        int max = packLeader ? 2 : 5;
                                        ai[0] = Random.Shared.Next(0, max) == 0 ? 4 : 1;
                                        ai[0] = above ? 4 : ai[0];
                                        ai[3] = 0;
                                        ai[1] = 2;
                                        if (ai[5] == 0)
                                            ai[5] = JoinPack(this) ? 1 : 0;
                                    }
                                }
                                else if (ai[0] == 3)
                                {
                                    CantStun = false;
                                    bool deadLeader = false;
                                    bool prePack = Pack != -1 && !packLeader && Packs.ElementAtOrDefault(Pack) != null;
                                    if (prePack)
                                    {
                                        NPC leader = Core.GetNPC(Packs[Pack].First());
                                        Ramming = leader.Ramming;

                                        Vector2 target = leader.Transform.Centre - new Vector2(0, Transform.Height * packID);
                                        Vector2 targetVel = Vector2.Normalize(target - Transform.Centre);

                                        if (Vector2.Distance(target, Transform.Centre) > 2 && ai[6] == 0) {
                                            ai[7] = Random.Shared.Next(1, 8); //pissball timer
                                            Transform.Velocity = Vector2.Lerp(Transform.Velocity, targetVel * 2, 0.1f);
                                        }
                                        else
                                        {
                                            ai[6] = 1;
                                            float rate = 0.2f / (1 + ((packID - 1) / 2f));

                                            Transform.Velocity = Ramming ? leader.Transform.Velocity * new Vector2(1.5f * (1 + ((packID - 1) / 4f)), 0) : Vector2.Zero;
                                            if (leader.ai[0] == 4) //if leader is on wall
                                            {
                                                Vector2 tVel = MathUtil.SafeNomralize(leader.Transform.Velocity);
                                                target = leader.Transform.Centre - (tVel * (packID - 1) * Transform.Height) - new Vector2(leader.Transform.Width / 2, leader.Transform.Height / 2);
                                                Transform.Position = Vector2.Lerp(Transform.Position, target, 0.1f);
                                            }
                                            else
                                                Transform.Position = new Vector2(MathHelper.Lerp(Transform.Position.X, target.X - Transform.Width / 2, rate), target.Y - Transform.Height / 2);

                                            if (ai[7] <= 0f) //pissballs
                                            {
                                                ai[7] = Random.Shared.Next(1, 16);
                                                Core.SpawnProjectile(new Projectile(0, this), Transform.Centre, dirToPlayer);
                                            }
                                            else
                                                ai[7] -= SSS.Delta;
                                        }

                                        deadLeader = leader.Corpse;
                                    }

                                    if (!prePack || deadLeader)
                                    {
                                        ai[1] = 2;
                                        ai[0] = 1;
                                        ai[2] = 0;
                                        ai[3] = 0;
                                        ai[4] = 0;
                                        ai[5] = 0;
                                        ai[6] = 0;
                                        ai[7] = 0;
                                    }
                                }
                                else if (ai[0] == 4)
                                {
                                    Vector2 target = dirToPlayer;
                                    if (!CanSeePlayer || DistToPlayer > 200)
                                    {
                                        updateNodes = true;
                                        target = Vector2.Normalize(NavController.NextNodePos - Transform.Centre);
                                    }

                                    Transform.Gravity = true;
                                    Point roundedPos = new Point((int)(Transform.Position.X / 16) * 16, (int)(Transform.Position.Y / 16) * 16);
                                    var selectTiles = LevelHandler.DrawData.Where(x => x.LayerName == "background" && x.Destination.Location == roundedPos);
                                    if (selectTiles.Any())
                                    {
                                        if (selectTiles.First().GID != 0)
                                        {
                                            if (ai[4] < MathF.PI * 2)
                                                ai[4] = 0;
                                            ai[4] += SSS.Delta;

                                            Transform.Gravity = false;
                                            Transform.Velocity = Vector2.Lerp(Transform.Velocity, target + (new Vector2(MathF.Sin(ai[4]), MathF.Cos(ai[4])) * target * 4), 0.1f);
                                        }
                                    }

                                    if (DistToPlayer < 48 && timeInCurrentState > 1.5f)
                                    {
                                        Transform.Gravity = true;
                                        ai[1] = 2;
                                        ai[0] = 2;
                                    }
                                }

                                if (DistToPlayer >= 200 && !CanSeePlayer && ai[0] != 0 && ai[0] != 4 && (ai[5] == 0 || packLeader))
                                {
                                    ai[0] = 4;
                                    ai[3] = 0;
                                    ai[1] = 2;
                                }
                                break;
                            #endregion
                            #region Experiments
                            case 11: //0 - Walking + snapping, 1 - Ramming + cooldown (only when far enough away), 2 - Oozing or Shooting (dependant on player y pos)
                                Ramming = false;
                                updateNodes = false;
                                CantStun = false;

                                if (ai[5] > 0f)
                                    ai[5] -= SSS.Delta;

                                if (ai[0] == 0)
                                {
                                    Vector2 moveTarget = Core._Player.Transform.Centre;
                                    bool flag1 = false;
                                    if (CanSeePlayer)
                                    {
                                        if (DistToPlayer < 70)
                                        {
                                            if (ai[1] <= -1f)
                                            {
                                                AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                                ai[1] = 0.8f;
                                            }
                                            else if (ai[1] <= 0f)
                                                ai[1] -= SSS.Delta;
                                        }
                                        if (DistToPlayer > 120 && DistToPlayer < 300 && ai[5] <= 0f)
                                        {
                                            ai[2] = 0f;
                                            ai[1] = 0f;
                                            ai[3] = 0f;
                                            ai[5] = 10.4f;
                                            ai[4] = Transform.Centre.X < Core._Player.Transform.Centre.X ? 1 : -1;
                                            ai[0] = 1;
                                        }
                                    }
                                    else
                                    {
                                        updateNodes = true;
                                        moveTarget = NavController.NextNodePos;
                                        if (moveTarget.Y < Transform.Position.Y)
                                            Transform.ChangeVelocityY(-5);
                                    }

                                    if (ai[1] > 0f)
                                    {
                                        flag1 = true;
                                        ai[1] -= SSS.Delta;
                                        if (ai[1] <= 0.4f && !Hitboxes.ContainsKey(1))
                                        {
                                            int side = Transform.Centre.X < Core._Player.Transform.Centre.X ? 1 : -1;
                                            Hitboxes[1] = MakeHBSide(new Rectangle(Transform.Width * -side, Transform.Height - 12, 82, 8), side);
                                            AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                        }
                                        else
                                            PreRam = true;

                                        if (ai[1] <= 0f)
                                            Hitboxes.Remove(1);
                                    }

                                    int dir = Transform.Centre.X < moveTarget.X ? 1 : -1;
                                    dir = flag1 ? 0 : dir;
                                    Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, dir * Speed, 0.05f));

                                    if (ai[2] == 0f && DistToPlayer > 100f)
                                        ai[2] = Random.Shared.Next(4, 10);

                                    if (ai[2] > 0f)
                                        ai[2] -= SSS.Delta;
                                    bool doesNotShitEverywhere = Transform.Position.Y - Core._Player.Transform.Position.Y > 50 && Grounded;
                                    if (CanSeePlayer && (ai[2] < 0f || doesNotShitEverywhere))
                                    {
                                        ai[2] = 0f;
                                        ai[1] = 0f;
                                        ai[3] = 0f;
                                        ai[4] = doesNotShitEverywhere ? 1 : 0;
                                        ai[0] = 2;
                                        if (!doesNotShitEverywhere)
                                            Core.SpawnProjectile(new Projectile(3, this), new Vector2(Transform.Centre.X - 30 + Transform.Velocity.X, Transform.Position.Y + Transform.Height - 3), dirToPlayer);
                                    }
                                }
                                else if (ai[0] == 1)
                                {
                                    CantStun = true;
                                    if (ai[5] > 10.0f)
                                    {
                                        Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, 0, 0.05f));
                                        PreRam = true;
                                        if (ai[5] == 8.5f - SSS.Delta)
                                            AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                    }
                                    else if (ai[5] > 8.5f)
                                    {
                                        Ramming = true;
                                        Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, RamAmount * ai[4], 0.05f));
                                    }
                                    else if (ai[5] > 5.5f)
                                        Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, 0, 0.05f));
                                    else
                                        ai[0] = 0;
                                }
                                else if (ai[0] == 2)
                                {
                                    Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, 0, 0.05f));

                                    if (ai[3] < 2f)
                                    {
                                        ai[3] += SSS.Delta;
                                        if (ai[4] == 1)
                                        {
                                            float divisor = (int)MathF.Floor(ai[3] / 0.5f) + 1;
                                            divisor *= 0.5f;

                                            if (ai[3] < divisor && ai[3] >= divisor - SSS.Delta)
                                                Core.SpawnProjectile(new Projectile(0, this), Transform.Centre, dirToPlayer);
                                        }
                                    }
                                    else
                                        ai[0] = 0;
                                }
                                break;
                            #endregion
                            #region Oil Ball
                            case 12: //0 - Walking towards player/Ramming/Flicking, 1 - Seeking wall and lunging off (might spit), 2 - Merging into serpent
                                Ramming = false;
                                updateNodes = false;
                                CantStun = false;

                                if (ai[0] == 0)
                                {
                                    Ramming = MathF.Abs(ai[3]) > Speed * 2;
                                    if (ai[3] != -1)
                                        ai[3] = MathHelper.Lerp(ai[3], 0, 0.1f);

                                    int dir = Core._Player.Transform.Centre.X < Transform.Centre.X ? -1 : 1;
                                    float tTar = ai[1] > 0f || ai[3] == -1 ? -0.75f : 1;
                                    Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, dir * Speed * tTar, 0.1f));

                                    if ((DistToPlayer < 40 || ai[2] > 0f) && ai[1] <= 0f)
                                    {
                                        if (ai[2] > 0.4f)
                                        {
                                            ai[1] = 4;
                                            ai[2] = 0;
                                            Ramming = true;
                                            AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);

                                            int r = Random.Shared.Next(0, 3);
                                            if (r == 1)
                                            {
                                                ai[3] = -1;
                                                Hitboxes[1] = MakeHBSide(new Rectangle(0, Transform.Height - 12, 32, 5), dir);
                                            }
                                            else
                                            {
                                                ai[3] = RamAmount * dir;
                                                Transform.ChangeVelocityY(-1);
                                            }
                                        }
                                        else
                                        {
                                            PreRam = true;
                                            if (ai[2] == 0f)
                                                AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                            ai[2] += SSS.Delta;
                                        }
                                    }

                                    if (ai[1] > 0f)
                                    {
                                        ai[1] -= SSS.Delta;
                                        if (ai[3] == -1 && ai[1] > 3.5f)
                                            Ramming = true;
                                        else if (ai[3] == -1)
                                        {
                                            ai[3] = 0;
                                            ai[0] = 1;
                                            ai[1] = 0;
                                            ai[4] = -dir;
                                            ai[2] = 0;
                                            Hitboxes.Remove(1);
                                        }
                                    }

                                    if (MathF.Abs(ai[3]) > 0.1f)
                                        Transform.ChangeVelocityX(ai[3]);

                                    if (Grounded && ai[6] == 1)
                                    {
                                        JoinPack(this);
                                        ai[6] = 0;
                                    }

                                    if (SimplePackCheck(UUID))
                                    {
                                        ai[1] = 0; //crazy re-uses!!
                                        ai[4] = 0;
                                        ai[0] = 2;
                                    }
                                }
                                else if (ai[0] == 1)
                                {
                                    CantStun = true;
                                    if (!isTouchingWall)
                                    {
                                        Transform.ChangeVelocityX(ai[4] * Speed * 2);
                                        if (!Grounded && Transform.Velocity.Y > 1f)
                                        {
                                            Transform.ChangeVelocityY(-3.5f);
                                            ai[0] = 0;
                                        }
                                    }
                                    else
                                    {
                                        Transform.ChangeVelocityX(0);
                                        Transform.ChangeVelocityY(-Speed * -((ai[5] - 2) / 2) * 1.5f);
                                        if (ai[5] < 2f)
                                            ai[5] += SSS.Delta;
                                        else
                                        {
                                            int r = Random.Shared.Next(0, 2);
                                            ai[0] = 0;
                                            ai[6] = 1;
                                            if (r == 1)
                                            {
                                                Transform.ChangeVelocityX(-lastTouchedWallDir * Speed);
                                                Core.SpawnProjectile(new Projectile(0, this), Transform.Centre, dirToPlayer);
                                            }
                                            else
                                            {
                                                Ramming = true;
                                                ai[3] = RamAmount * -lastTouchedWallDir;
                                                Transform.ChangeVelocityY(1f);
                                                AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                            }
                                        }
                                    }
                                }
                                else if (ai[0] == 2)
                                {
                                    bool superdone = false;
                                    if (ai[1] == 0)
                                    {
                                        Pack = GetPack(UUID, out packLeader, out packID);
                                        ai[1] = 1;

                                        int max = Packs[Pack].Length;
                                        superdone = max == 1;

                                        if (!superdone)
                                        {
                                            Vector2 avg = Vector2.Zero;
                                            for (int i = 0; i < max; i++)
                                                avg += Core.GetNPC(Packs[Pack][i]).Transform.Centre;
                                            avg /= max;

                                            ai[2] = avg.X; //MORE RE-USES OH MY GOD!!!111!!
                                            ai[3] = avg.Y - 32;
                                        }
                                    }

                                    if (!superdone)
                                    {
                                        Transform.Position = Vector2.Lerp(Transform.Position, new Vector2(ai[2], ai[3]), 0.05f);
                                        Transform.Velocity = Vector2.Zero;

                                        if (ai[4] < 3f)
                                            ai[4] += SSS.Delta;
                                        else
                                        {
                                            if (packLeader)
                                            {
                                                var spawn = new NPC(13, 400, 0, PhantomType.None);
                                                Core.SpawnNPC(spawn, new Vector2(ai[2], ai[3]) - new Vector2(spawn.Transform.Width / 2, spawn.Transform.Width / 2));
                                            }
                                            Kill();
                                        }
                                    }
                                    else
                                    {
                                        DisbandPack(UUID);
                                        ai[0] = 0;
                                        ai[1] = 0;
                                        ai[2] = 0;
                                        ai[3] = 0;
                                        ai[4] = 0;
                                        ai[5] = 0;
                                        ai[6] = 0;
                                    }
                                }
                                break;
                            #endregion
                            #region Oil Serpent
                            case 13: //0 - Walking + Snapping, 1 - Slam, 2 - Tendril Flick
                                Ramming = false;
                                updateNodes = false;
                                CantStun = false;

                                if (ai[0] == 0)
                                {
                                    Ramming = MathF.Abs(ai[3]) > Speed * 2;
                                    if (!aiFlags[0])
                                        ai[3] = MathHelper.Lerp(ai[3], 0f, 0.1f);

                                    if (ai[1] < MathF.PI)
                                        ai[1] += SSS.Delta * 4;
                                    else if (ai[1] < MathF.PI * 2)
                                        ai[1] += SSS.Delta * 10;
                                    else
                                        ai[1] = 0;

                                    Vector2 target = Core._Player.Transform.Centre;
                                    if (!CanSeePlayer)
                                    {
                                        updateNodes = true;
                                        target = NavController.NextNodePos;
                                    }
                                    if (Grounded && Transform.Position.Y - target.Y > 50)
                                        Transform.ChangeVelocityY(-3f);
                                    int dir = target.X < Transform.Centre.X ? -1 : 1;
                                    Transform.ChangeVelocityX(MathF.Sin(ai[1]) * Speed * dir);

                                    if (ai[2] < 5f)
                                        ai[2] += SSS.Delta;
                                    if ((DistToPlayer < 64 && MathF.Abs(ai[3]) <= 0.1f && !aiFlags[0]) || lastTickStunned)
                                    {
                                        if (lastTickStunned)
                                        {
                                            ai[4] = 0;
                                            aiFlags[0] = false;
                                            aiFlags[2] = false;
                                            ai[2] = 3;
                                        }
                                        if (ai[2] >= 5f)
                                        {
                                            ai[0] = Random.Shared.Next(0, 4) == 1 ? 2 : 1;
                                            ai[2] = 0;
                                            ai[3] = dir;
                                            Transform.ChangeVelocityX(RamAmount / 2 * dir);
                                            AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                        }
                                        else if (ai[4] <= 0f)
                                        {
                                            AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                            aiFlags[0] = true;
                                            ai[3] = 0.4f;
                                            ai[4] = 1.5f;
                                        }
                                    }

                                    if (aiFlags[0])
                                    {
                                        PreRam = true;
                                        if (ai[3] <= 0f)
                                        {
                                            AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                            PreRam = false;
                                            aiFlags[0] = false;
                                            ai[3] = RamAmount * dir;
                                        }
                                        else
                                            ai[3] -= SSS.Delta;
                                    }

                                    if (MathF.Abs(ai[3]) > 0.1f && !aiFlags[0])
                                        Transform.ChangeVelocityX(ai[3]);

                                    if (ai[4] > 0f)
                                        ai[4] -= SSS.Delta;
                                }
                                else if (ai[0] == 1)
                                {
                                    CantStun = true;
                                    ai[2] += SSS.Delta;
                                    if (ai[2] < 0.4f)
                                        PreRam = true;
                                    else if (ai[2] >= 0.4f && ai[2] < 0.8f)
                                    {
                                        Ramming = true;
                                        if (ai[2] <= 0.4f + SSS.Delta)
                                            AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                        Transform.ChangeVelocityX(Transform.Velocity.X + (0.5f * ai[3]));
                                    }
                                    else if (ai[2] < 2f)
                                    {
                                        if (Grounded)
                                            Transform.ChangeVelocityY(-2f);
                                        int dir = Core._Player.Transform.Centre.X < Transform.Centre.X ? -1 : 1;
                                        Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, dir * 4, 0.1f));

                                        if (ai[2] >= 2f - 0.4f)
                                        {
                                            PreRam = true;
                                            if (ai[2] <= 2f - 0.4f + SSS.Delta)
                                                AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                        }
                                    }
                                    else if (ai[2] < 4f)
                                    {
                                        Ramming = !Grounded;
                                        Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, 0, 0.1f));
                                        if (ai[2] < 2f + SSS.Delta)
                                        {
                                            AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                            Transform.ChangeVelocityY(8f);
                                        }
                                    }
                                    else
                                    {
                                        ai[0] = 0;
                                        ai[2] = 0;
                                        ai[3] = 0;
                                    }
                                }
                                else if (ai[0] == 2)
                                {
                                    CantStun = true;
                                    Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, 0, 0.1f));
                                    ai[2] += SSS.Delta;
                                    if (ai[2] < 1f)
                                        PreRam = true;
                                    else if (ai[2] < 5f)
                                    {
                                        float divisor = (int)MathF.Floor(ai[2] / 0.5f) + 1;
                                        divisor *= 0.5f;

                                        if (ai[2] < divisor && ai[2] >= divisor - SSS.Delta)
                                        {
                                            if (Hitboxes.ContainsKey(1))
                                                Hitboxes.Remove(1);
                                            ai[4] = ai[4] == 1 ? -1 : 1;
                                            AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                            Hitboxes[1] = MakeHBSide(new Rectangle(0, Transform.Height - 12, 48, 8), (int)ai[4]);
                                            Transform.ChangeVelocityX((int)ai[4] * RamAmount / 2);
                                        }
                                    }
                                    else if (ai[2] < 5f + SSS.Delta)
                                        Hitboxes.Remove(1);
                                    else if (ai[2] > 6.4f)
                                    {
                                        ai[0] = 0;
                                        ai[2] = 0;
                                        ai[3] = 0;
                                    }
                                }
                                break;
                            #endregion
                            #region Normal Kefal
                            //0 - Strafing (Can Pull In), 1 - Navigating, 2 - Combat, 3 - Swinging
                            //@1 Pull In Timer & Swing Intersect, @2 Combat cooldown timer, @3 Combat sub cooldown, @4 Ramming # & Swing Let Go Vel, @5 Ram Count, @6 Swing Timer
                            case 14:
                                Ramming = false;
                                updateNodes = false;

                                if (ai[0] == 0)
                                {
                                    bool pullingIn = ai[1] > 0f && ai[1] <= SSS.Delta;

                                    float targetVel = pullingIn ? 0 : -horiDirToPlayer * Speed * 0.75f;
                                    Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, targetVel, 0.1f));
                                    if (timeInCurrentState > 6f || (timeInCurrentState > 1f && DistToPlayer <= 32))
                                    {
                                        ai[0] = !CanSeePlayer && DistToPlayer > 100 ? 1 : 2;
                                        if (ai[0] == 2)
                                        {
                                            Transform.ChangeVelocityY(-3f);
                                            Core._Player.Transform.ChangeVelocityY(-3f);
                                            Core._Player.externalVelocity = Vector2.Zero;
                                        }
                                    }

                                    if (ai[1] == 0)
                                        ai[1] = Random.Shared.Next(1, 6);
                                    else if (!pullingIn)
                                    {
                                        ai[1] -= SSS.Delta;
                                        aiFlags[0] = Core._Player.externalVelocity == Vector2.Zero;
                                    }
                                    else if (pullingIn && aiFlags[0] && ai[0] == 0) //stupid check for above ai[0] set
                                    {
                                        if (CanSeePlayer && DistToPlayer <= 300 && DistToPlayer > 16)
                                            Core._Player.externalVelocity = Vector2.Lerp(Core._Player.externalVelocity, new Vector2(-horiDirToPlayer * 4, (Transform.Centre.Y < Core._Player.Transform.Centre.Y ? -1 : 1) * 0.2f), 0.05f);
                                        else if (CanSeePlayer && DistToPlayer <= 16)
                                        {
                                            ai[1] = -1;
                                            Core._Player.Transform.ChangeVelocityY(-3f);
                                            Core._Player.externalVelocity = Vector2.Zero;
                                        }
                                    }

                                    if ((!CanSeePlayer && DistToPlayer > 64) || DistToPlayer > 350)
                                    {
                                        ai[0] = 1;
                                        ai[1] = 0;
                                        ai[2] = 0;
                                        ai[5] = 0;
                                    }
                                }
                                else if (ai[0] == 1)
                                {
                                    updateNodes = true;
                                    Vector2 toNode = NavController.NextNodePos - Transform.Centre;
                                    int dir = MathUtil.IntSign(toNode.X);
                                    Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, dir * Speed, 0.1f));

                                    if (MathF.Abs(toNode.X) <= Transform.WHMedian() && toNode.Y < -16)
                                        Transform.ChangeVelocityY(MathHelper.Lerp(Transform.Velocity.Y, -5f, 0.05f));

                                    if (CanSeePlayer && DistToPlayer < 300)
                                        ai[0] = 0;
                                }
                                else if (ai[0] == 2)
                                {
                                    Ramming = MathF.Abs(ai[4]) > Speed * 2;
                                    ai[4] = MathHelper.Lerp(ai[4], 0, 0.1f);

                                    if (!Ramming && Transform.Position.Y - Core._Player.Transform.Position.Y > 32)
                                        Transform.ChangeVelocityY(-3f);

                                    if (ai[2] <= 0)
                                    {
                                        bool swing = ai[5] >= MaxBoosts;
                                        int sideDir = Core._Player.Transform.Position.X - (40 * horiDirToPlayer) < Transform.Centre.X ? -1 : 1;
                                        Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, sideDir * Speed * 1.5f, 0.05f));

                                        if (DistToPlayer <= 300 && Transform.Position.Y > Core._Player.Transform.Position.Y - Core._Player.Transform.Height - Transform.Height
                                            && Transform.Position.Y < Core._Player.Transform.Position.Y + Core._Player.Transform.Height + (Transform.Height * 2) && !swing)
                                        {
                                            ai[3] += SSS.Delta;
                                            if (ai[3] >= 0.35f)
                                            {
                                                ai[2] = 2.8f;
                                                aiFlags[1] = true;
                                                AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                                ai[5]++;
                                            }
                                        }
                                        else
                                            ai[3] = 0;

                                        //This is Kefal specific
                                        if (swing)
                                        {
                                            ai[0] = 3;
                                            ai[1] = -1; //Reuse
                                            kefalSwingRay = new Ray2D(Transform.Centre, new Vector2(0, -1));
                                            List<float> bests = new List<float>();
                                            foreach (Collider col in Core._Colliders)
                                            {
                                                var inter = kefalSwingRay.Intersects(col.GetRectangle());
                                                if (inter.HasValue)
                                                    bests.Add(inter.Value);
                                            }

                                            if (bests.Any())
                                                ai[1] = bests.Min(); //Reuse
                                            ai[5] = 0;
                                            ai[6] = 0;
                                        }
                                    }

                                    if (lastTickStunned)
                                    {
                                        ai[4] = 0;
                                        PreRam = false;
                                        Ramming = false;
                                        ai[3] = 0;
                                        ai[2] = 1f;
                                        aiFlags[1] = false;
                                    }

                                    if (aiFlags[1] && !lastTickStunned)
                                    {
                                        PreRam = true;
                                        if (ai[3] >= 0.75f)
                                        {
                                            ai[3] = 0;
                                            aiFlags[1] = false;
                                            ai[4] = 7f; //ram amount world go here
                                            AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                        }
                                        else
                                            ai[3] += SSS.Delta;
                                    }
                                    else if (ai[2] > 0)
                                        ai[2] -= SSS.Delta;

                                    if (MathF.Abs(ai[4]) > 0.1f)
                                    {
                                        //float vertDirToPlayer = Transform.Centre.Y > Core._Player.Transform.Centre.Y ? -1 : 1;
                                        //Transform.Velocity = new Vector2(ai[4], MathHelper.Lerp(Transform.Velocity.Y, vertDirToPlayer * 5, 0.05f));
                                        Transform.Velocity = Vector2.Lerp(Transform.Velocity, dirToPlayer * ai[4] * (RamAmount / 4), 0.1f);
                                    }

                                    if (timeInCurrentState > 2f && (DistToPlayer > 500 || (!CanSeePlayer && DistToPlayer > 100)))
                                    {
                                        ai[5] = 0;
                                        ai[0] = 1;
                                    }
                                }
                                else if (ai[0] == 3)
                                {
                                    if (ai[1] == -1 || ai[1] > 250)
                                    {
                                        ai[1] = 0;
                                        ai[0] = 0;
                                    }
                                    else
                                    {
                                        Transform.Gravity = false;

                                        Vector2 PoI = kefalSwingRay.Position + (kefalSwingRay.Direction * ai[1]);
                                        float angle = MathF.Sin(MathHelper.ToRadians(ai[6] * 180)) * 90;
                                        Vector2 pos = Transform.RotateAroundAPoint(PoI, angle + 180, Math.Clamp(ai[1] / 6, 50, 120));

                                        if (ai[6] > 4)
                                        {
                                            Transform.Gravity = true;

                                            if (ai[2] == ai[6] / 2)
                                                Transform.ChangeVelocityY(-3f);

                                            ai[2] = MathHelper.Lerp(ai[2], 0, 0.1f);
                                            if (MathF.Abs(ai[2]) > 0.1f)
                                                Transform.ChangeVelocityX(ai[2]);
                                            else if (Grounded && timeInCurrentState > 1f)
                                                ai[0] = 0;
                                        }
                                        else
                                        {
                                            ai[6] += SSS.Delta;
                                            ai[2] = ai[6] / 2;
                                            Transform.Position = Vector2.Lerp(Transform.Position, pos, 0.1f);
                                        }
                                    }
                                }
                                break;
                            #endregion
                            #region Ranged Kefal
                            //0 - Strafing (Can Pull In), 1 - Navigating, 2 - Combat, 3 - Swinging
                            //@1 Swing Intersect, @2 Combat cooldown timer, @3 Combat sub cooldown, @4 Ramming # & Swing Let Go Vel, @5 Ram Count, @6 Swing Timer
                            case 15:
                                Ramming = false;
                                updateNodes = false;

                                if (ai[0] == 0)
                                {
                                    Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, -horiDirToPlayer * Speed * 0.75f, 0.1f));
                                    if (timeInCurrentState > 4f || (timeInCurrentState > 1f && DistToPlayer <= 32))
                                    {
                                        ai[0] = !CanSeePlayer && DistToPlayer > 100 ? 1 : 2;
                                        ai[5] = 0;
                                        ai[1] = 0;
                                        if (ai[0] == 2)
                                            Transform.ChangeVelocityY(-3f);
                                    }

                                    if ((!CanSeePlayer && DistToPlayer > 64) || DistToPlayer > 350)
                                    {
                                        ai[0] = 1;
                                        ai[1] = 0;
                                        ai[2] = 0;
                                        ai[5] = 0;
                                    }
                                }
                                else if (ai[0] == 1)
                                {
                                    updateNodes = true;
                                    Vector2 toNode = NavController.NextNodePos - Transform.Centre;
                                    int dir = MathUtil.IntSign(toNode.X);
                                    Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, dir * Speed, 0.1f));

                                    if (MathF.Abs(toNode.X) <= Transform.WHMedian() && toNode.Y < -16)
                                        Transform.ChangeVelocityY(MathHelper.Lerp(Transform.Velocity.Y, -5f, 0.05f));

                                    if (CanSeePlayer && DistToPlayer < 300)
                                        ai[0] = 0;
                                }
                                else if (ai[0] == 2)
                                {
                                    Ramming = MathF.Abs(ai[4]) > Speed * 2;
                                    ai[4] = MathHelper.Lerp(ai[4], 0, 0.1f);

                                    if (!Ramming && Transform.Position.Y - Core._Player.Transform.Position.Y > 32)
                                        Transform.ChangeVelocityY(-3f);

                                    if (ai[2] <= 0)
                                    {
                                        bool swing = ai[5] >= MaxBoosts;
                                        int sideDir = Core._Player.Transform.Position.X - (100 * horiDirToPlayer) < Transform.Centre.X ? -1 : 1;
                                        Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, sideDir * Speed, 0.1f));

                                        if (DistToPlayer <= 500 && !swing)
                                        {
                                            ai[3] += SSS.Delta;
                                            if (ai[3] >= 0.35f)
                                            {
                                                ai[2] = 2f;
                                                ai[5]++;

                                                if (DistToPlayer <= 40 && Transform.Position.Y > Core._Player.Transform.Position.Y - Core._Player.Transform.Height - Transform.Height
                                                    && Transform.Position.Y < Core._Player.Transform.Position.Y + Core._Player.Transform.Height + (Transform.Height * 2))
                                                {
                                                    aiFlags[1] = true;
                                                    AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                                }
                                                else if (Random.Shared.Next(0, 2) == 1)
                                                    Core.SpawnProjectile(new Projectile(1, this), Transform.Centre, dirToPlayer);
                                            }
                                        }
                                        else
                                            ai[3] = 0;

                                        if (swing)
                                        {
                                            ai[0] = 3;
                                            ai[1] = -1;
                                            kefalSwingRay = new Ray2D(Transform.Centre, new Vector2(0, -1));
                                            List<float> bests = new List<float>();
                                            foreach (Collider col in Core._Colliders)
                                            {
                                                var inter = kefalSwingRay.Intersects(col.GetRectangle());
                                                if (inter.HasValue)
                                                    bests.Add(inter.Value);
                                            }

                                            if (bests.Any())
                                                ai[1] = bests.Min();
                                            ai[6] = 0;
                                        }
                                    }

                                    if (lastTickStunned)
                                    {
                                        ai[4] = 0;
                                        PreRam = false;
                                        Ramming = false;
                                        ai[3] = 0;
                                        ai[2] = 1f;
                                        aiFlags[1] = false;
                                    }

                                    if (aiFlags[1] && !lastTickStunned)
                                    {
                                        PreRam = true;
                                        if (ai[3] >= 0.75f)
                                        {
                                            ai[4] = RamAmount * horiDirToPlayer;
                                            ai[3] = 0;
                                            aiFlags[1] = false;
                                            AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                        }
                                        else
                                            ai[3] += SSS.Delta;
                                    }
                                    else if (ai[2] > 0)
                                        ai[2] -= SSS.Delta;

                                    if (MathF.Abs(ai[4]) > 0.1f)
                                    {
                                        float vertDirToPlayer = Transform.Centre.Y > Core._Player.Transform.Centre.Y ? -1 : 1;
                                        Transform.Velocity = new Vector2(ai[4], MathHelper.Lerp(Transform.Velocity.Y, vertDirToPlayer * RamAmount, 0.05f));
                                    }

                                    if (DistToPlayer > 650 || (!CanSeePlayer && DistToPlayer > 100))
                                        ai[0] = 1;
                                }
                                else if (ai[0] == 3)
                                {
                                    if (ai[1] == -1 || ai[1] > 400)
                                    {
                                        ai[1] = 0;
                                        ai[0] = 0;
                                    }
                                    else
                                    {
                                        Transform.Gravity = false;

                                        Vector2 PoI = kefalSwingRay.Position + (kefalSwingRay.Direction * ai[1]);
                                        float angle = MathF.Sin(MathHelper.ToRadians(ai[6] * 80)) * 90;
                                        Vector2 pos = Transform.RotateAroundAPoint(PoI, angle + 180, Math.Clamp(ai[1] / 2, 50, 250));
                                        float distToPos = Vector2.Distance(Transform.Centre, PoI);

                                        if (ai[6] > 8 || DistToPlayer < 20)
                                        {
                                            Transform.Gravity = true;

                                            if (ai[2] == ai[6] / 2)
                                                Transform.ChangeVelocityY(-3f);

                                            ai[2] = MathHelper.Lerp(ai[2], 0, 0.1f);
                                            if (MathF.Abs(ai[2]) > 0.1f)
                                                Transform.ChangeVelocityX(ai[2]);
                                            else if (Grounded)
                                                ai[0] = 0;
                                        }
                                        else if (distToPos < 250)
                                        {
                                            ai[6] += SSS.Delta;
                                            ai[2] = ai[6] / 2;
                                            Transform.Position = Vector2.Lerp(Transform.Position, pos, 0.1f);
                                            Transform.Velocity = Vector2.Zero;

                                            float sect = MathF.Ceiling(ai[6]);
                                            if (ai[6] > sect - SSS.Delta && ai[6] < sect)
                                                Core.SpawnProjectile(new Projectile(1, this), Transform.Centre, dirToPlayer);
                                        }
                                        else
                                            Transform.Velocity = Vector2.Lerp(Transform.Velocity, Vector2.Normalize(PoI - Transform.Centre) * Speed, 0.1f);
                                    }
                                }
                                break;
                            #endregion
                            #region Deranged Kefal
                            //0 - Standing, 1 - Navigating, 2 - Combat
                            //@1 Pull In Timer & Swing Intersect, @2 Combat cooldown timer, @3 Combat sub cooldown, @4 Ramming # & Swing Let Go Vel, @5 Ram Count, @6 DTP Fly X, @7 DTP Fly Y
                            case 16:
                                Ramming = false;
                                updateNodes = false;

                                if (ai[0] == 0)
                                {
                                    if (isTouchingWall || isTouchingCeiling)
                                        Transform.ChangeVelocityY(0);

                                    Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, 0, 0.1f));
                                    if (timeInCurrentState > 6f || (timeInCurrentState > 1f && DistToPlayer <= 32))
                                        ai[0] = !CanSeePlayer && DistToPlayer > 100 ? 1 : 2;

                                    if ((!CanSeePlayer && DistToPlayer > 64) || DistToPlayer > 350)
                                    {
                                        ai[0] = 1;
                                        ai[1] = 0;
                                        ai[2] = 0;
                                        ai[5] = 0;
                                    }
                                }
                                else if (ai[0] == 1)
                                {
                                    updateNodes = true;
                                    Vector2 toNode = NavController.NextNodePos - Transform.Centre;
                                    int dir = MathUtil.IntSign(toNode.X);
                                    Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, dir * Speed, 0.1f));

                                    if (MathF.Abs(toNode.X) <= Transform.WHMedian() && toNode.Y < -16 && Grounded)
                                        Transform.ChangeVelocityY(-5f);

                                    if (CanSeePlayer && DistToPlayer < 300)
                                        ai[0] = 2;
                                }
                                else if (ai[0] == 2)
                                {
                                    Ramming = MathF.Abs(ai[4]) > Speed * 2;
                                    ai[4] = MathHelper.Lerp(ai[4], 0, 0.1f);

                                    Vector2 target = aiFlags[2] ? Core._Player.Transform.Centre - new Vector2(80 * horiDirToPlayer, 80) : Core._Player.Transform.Centre - new Vector2(40 * horiDirToPlayer, 0);
                                    Vector2 targetVel = Vector2.Normalize(target - Transform.Centre);
                                    if (!Ramming)
                                        Transform.Velocity = Vector2.Lerp(Transform.Velocity, targetVel * Speed * 2f, 0.05f);

                                    if (ai[2] <= 0)
                                    {
                                        bool done = ai[5] >= MaxBoosts;
                                        bool canPass = aiFlags[2] ? Vector2.Distance(Transform.Centre, target) <= 32 : Transform.Position.Y > Core._Player.Transform.Position.Y - Core._Player.Transform.Height - Transform.Height
                                            && Transform.Position.Y < Core._Player.Transform.Position.Y + Core._Player.Transform.Height + Transform.Height;

                                        if (DistToPlayer <= 300 && canPass && !done)
                                        {
                                            ai[3] += SSS.Delta;
                                            if (ai[3] >= 0.35f)
                                            {
                                                ai[2] = 3.5f;
                                                aiFlags[1] = true;
                                                AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                                ai[5]++;

                                                ai[6] = dirToPlayer.X;
                                                ai[7] = dirToPlayer.Y;
                                            }
                                        }
                                        else
                                            ai[3] = 0;

                                        if (done)
                                        {
                                            ai[0] = 0;
                                            ai[5] = 0;
                                        }
                                    }

                                    if (lastTickStunned)
                                    {
                                        Ramming = false;
                                        PreRam = false;
                                        aiFlags[1] = false;
                                        ai[3] = 0;
                                        aiFlags[2] = Random.Shared.Next(0, 2) == 1;
                                        ai[4] = 0;
                                    }

                                    if (aiFlags[1])
                                    {
                                        PreRam = true;

                                        if (ai[3] < 0.75f || aiFlags[2])
                                            ai[3] += SSS.Delta;

                                        if (ai[3] >= 0.75f)
                                        {
                                            if (aiFlags[2])
                                            {
                                                Ramming = true;
                                                PreRam = false;
                                                if (ai[3] < 1f)
                                                    Transform.Velocity = Vector2.Lerp(Transform.Velocity, new Vector2(ai[6], ai[7]) * RamAmount * 1.3f, 0.1f);
                                                else
                                                {
                                                    Transform.Velocity = Vector2.Lerp(Transform.Velocity, Vector2.Zero, 0.1f);
                                                    if (ai[3] > 1.75f)
                                                    {
                                                        aiFlags[1] = false;
                                                        Ramming = false;
                                                        aiFlags[2] = Random.Shared.Next(0, 2) == 1;
                                                        ai[3] = 0;
                                                    }

                                                    if (ai[3] <= 1f + SSS.Delta)
                                                        AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                                }
                                            }
                                            else
                                            {
                                                ai[4] = RamAmount * horiDirToPlayer;
                                                aiFlags[1] = false;
                                                ai[3] = 0;
                                                AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                                aiFlags[2] = Random.Shared.Next(0, 2) == 1;
                                            }

                                        }
                                    }
                                    else if (ai[2] > 0)
                                        ai[2] -= SSS.Delta;

                                    if (MathF.Abs(ai[4]) > 0.1f)
                                    {
                                        float vertDirToPlayer = Transform.Centre.Y > Core._Player.Transform.Centre.Y ? -1 : 1;
                                        Transform.Velocity = new Vector2(ai[4], MathHelper.Lerp(Transform.Velocity.Y, vertDirToPlayer * 5, 0.05f));
                                    }

                                    if (DistToPlayer > 500 || (!CanSeePlayer && DistToPlayer > 100))
                                        ai[0] = 1;
                                }
                                break;
                            #endregion
                            #region Bug ML
                            //0 - Hovering Around Player, 1 - Combat/Ramming, 2 - Stabbing, 3 - Following
                            //@1 - Hover timer, @2 - Ram timer, @3 - Ram Amount, @4 - Ram count, @5 - Stab phases, @6 & @7 - Random pack X Y
                            case 17:
                                updateNodes = false;
                                Ramming = false;

                                float timeVal = (float)time.TotalGameTime.TotalSeconds * 2;
                                Vector2 buzzVel = new Vector2(MathF.Sin(timeVal * 4), MathF.Cos(timeVal * 4)) / 3;

                                if (ai[0] == 0)
                                {
                                    Vector2 tpOffset = new Vector2(MathF.Sin(timeVal), MathF.Cos(timeVal) * 0.5f - 0.5f) * 40;
                                    Vector2 targetVel = Vector2.Normalize(Core._Player.Transform.Centre + tpOffset - Transform.Centre);
                                    targetVel += buzzVel;
                                    Transform.Velocity = Vector2.Lerp(Transform.Velocity, targetVel * Speed, 0.1f);

                                    if (ai[1] > 3)
                                    {
                                        JoinPack(this);
                                        ai[0] = 1;
                                    }
                                    else if (DistToPlayer < 100)
                                        ai[1] += SSS.Delta;
                                }
                                else if (ai[0] == 1)
                                {
                                    Ramming = MathF.Abs(ai[3]) > Speed * 2;
                                    ai[3] = MathHelper.Lerp(ai[3], 0, 0.1f);

                                    Vector2 subTarget = Core._Player.Transform.Centre - new Vector2(horiDirToPlayer * 70, 0);
                                    Vector2 targetVel = Ramming ? Vector2.Zero : Vector2.Normalize(subTarget - Transform.Centre);
                                    targetVel += buzzVel;
                                    Transform.Velocity = Vector2.Lerp(Transform.Velocity, targetVel * Speed, 0.1f);

                                    if (ai[2] <= 0 && Vector2.Distance(subTarget, Transform.Centre) < 16)
                                    {
                                        aiFlags[1] = true;
                                        AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                        ai[2] = 3;
                                    }

                                    if (aiFlags[1])
                                    {
                                        PreRam = true;
                                        if (ai[2] <= 3 - 0.4f)
                                        {
                                            PreRam = false;
                                            ai[3] = RamAmount * horiDirToPlayer;
                                            AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                            ai[4]++;
                                            aiFlags[1] = false;
                                        }
                                    }
                                    if (ai[2] > 0)
                                    {
                                        ai[2] -= SSS.Delta;
                                        if (CollisionEngine.RectInRect(Transform.GetRectangle(), Core._Player.Transform.GetRectangle()) && Ramming)
                                        {
                                            ai[2] = 0;
                                            ai[3] = 0;
                                            ai[1] = 0;
                                            ai[4] = 0;
                                            ai[0] = 2;
                                        }
                                    }

                                    if (MathF.Abs(ai[3]) > 0.1f)
                                        Transform.ChangeVelocityX(ai[3]);

                                    if (ai[4] >= MaxBoosts || DistToPlayer > 300)
                                    {
                                        ai[0] = 0;
                                        ai[1] = 0;
                                        ai[2] = 0;
                                        ai[3] = 0;
                                        ai[4] = 0;
                                        if (SimplePackCheck(UUID))
                                        {
                                            Pack = GetPack(UUID, out packLeader, out packID);
                                            ai[0] = packLeader ? ai[0] : 3;
                                            ai[6] = Random.Shared.Next(-16, 16);
                                            ai[7] = Random.Shared.Next(-16, 16);
                                        }
                                    }
                                }
                                else if (ai[0] == 2)
                                {
                                    float quarterPie = MathF.PI / 4;

                                    if (ai[5] < quarterPie)
                                        Transform.Velocity = Vector2.Lerp(Transform.Velocity, -dirToPlayer, 0.1f);
                                    else if (ai[5] < quarterPie + 0.4f)
                                    {
                                        if (ai[5] < quarterPie + SSS.Delta)
                                            AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                        PreRam = true;
                                        Transform.Velocity = Vector2.Lerp(Transform.Velocity, Vector2.Zero, 0.1f);
                                    }
                                    else if (ai[5] < quarterPie + 0.8f)
                                    {
                                        if (ai[5] < quarterPie + 0.4f + SSS.Delta)
                                            AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                        Ramming = true;
                                        Transform.Velocity = Vector2.Lerp(Transform.Velocity, dirToPlayer * RamAmount, 0.1f);
                                        if (CollisionEngine.RectInRect(Transform.GetRectangle(), Core._Player.Transform.GetRectangle()))
                                            aiFlags[2] = true;
                                    }
                                    else if (ai[5] < 6f)
                                        Transform.Velocity = Vector2.Lerp(Transform.Velocity, new Vector2(MathF.Sin(ai[5]), MathF.Cos(ai[5])), 0.1f);
                                    else
                                    {
                                        aiFlags[2] = false;
                                        ai[5] = 0;
                                        ai[0] = 0;
                                    }

                                    if (aiFlags[2])
                                    {
                                        Core._Player.Transform.Position = Transform.Centre - new Vector2(Core._Player.Transform.Width / 2, Core._Player.Transform.Height / 2);
                                        Core._Player.Transform.ChangeVelocityY(0);
                                    }
                                    ai[5] += SSS.Delta;
                                }
                                else if (ai[0] == 3)
                                {
                                    if (Packs.ElementAtOrDefault(Pack) != null)
                                    {
                                        NPC leader = Core.GetNPC(Packs[Pack].First());
                                        Ramming = leader.Ramming;

                                        Vector2 targetVel = Vector2.Normalize(leader.Transform.Centre + new Vector2(ai[6], ai[7]) - Transform.Centre);
                                        targetVel *= leader.Transform.Velocity.Length();
                                        targetVel += buzzVel;

                                        Transform.Velocity = Vector2.Lerp(Transform.Velocity, targetVel * Speed, 0.2f) * (Ramming ? 1.3f : 1);

                                        if (leader.Corpse)
                                            ai[0] = 0;
                                    }
                                    else
                                        ai[0] = 0;
                                }
                                break;
                            #endregion
                            #region Ponderosa Serpent
                            case 18: //0 - Walking + Snapping, 1 - Slam
                                Ramming = false;
                                updateNodes = false;
                                CantStun = false;

                                if (ai[0] == 0)
                                {
                                    Ramming = MathF.Abs(ai[3]) > Speed * 2f;
                                    if (!aiFlags[0])
                                    {
                                        if (aiFlags[1])
                                        {
                                            ai[3] = MathHelper.Lerp(ai[3], ai[5], 0.3f);
                                            if (MathF.Abs(ai[3]) >= MathF.Abs(ai[5]) - SSS.Delta)
                                                aiFlags[1] = false;
                                        }
                                        else
                                            ai[3] = MathHelper.Lerp(ai[3], 0f, 0.1f);
                                    }

                                    if (ai[1] < MathF.PI)
                                        ai[1] += SSS.Delta * 4;
                                    else if (ai[1] < MathF.PI * 2)
                                        ai[1] += SSS.Delta * 10;
                                    else
                                        ai[1] = 0;

                                    Vector2 target = Core._Player.Transform.Centre;
                                    if (!CanSeePlayer)
                                    {
                                        updateNodes = true;
                                        target = NavController.NextNodePos;
                                    }
                                    if (Grounded && Transform.Position.Y - target.Y > 50)
                                        Transform.ChangeVelocityY(-3f);
                                    int dir = target.X < Transform.Centre.X ? -1 : 1;
                                    Transform.ChangeVelocityX(MathF.Sin(ai[1]) * Speed * dir);

                                    if (ai[2] < 3f)
                                        ai[2] += SSS.Delta;
                                    if ((DistToPlayer < 64 && MathF.Abs(ai[3]) <= 0.1f && !aiFlags[0]) || lastTickStunned)
                                    {
                                        if (lastTickStunned)
                                        {
                                            ai[4] = 0;
                                            aiFlags[0] = false;
                                            aiFlags[2] = false;
                                            ai[2] = 3;
                                        }
                                        if (ai[4] <= 0f)
                                        {
                                            AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                            if (ai[2] >= 3f && Random.Shared.Next(0, 3) != 1)
                                            {
                                                ai[0] = 1;
                                                ai[1] = 0;
                                            }
                                            else
                                            {
                                                aiFlags[0] = true;
                                                aiFlags[2] = false;
                                                ai[3] = 0.6f;
                                                ai[4] = 1.5f;
                                            }
                                        }
                                    }

                                    if (aiFlags[0])
                                    {
                                        PreRam = true;
                                        if (ai[3] <= 0f)
                                        {
                                            AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                            PreRam = false;
                                            aiFlags[0] = false;
                                            ai[5] = RamAmount * dir;
                                            aiFlags[1] = true;
                                        }
                                        else
                                            ai[3] -= SSS.Delta;
                                    }

                                    if (MathF.Abs(ai[3]) > 0.1f && !aiFlags[0])
                                        Transform.ChangeVelocityX(ai[3]);

                                    if (aiFlags[1] && ai[2] >= 3f && Ramming && CollisionEngine.RectInRect(Transform.GetRectangle(), Core._Player.Transform.GetRectangle()))
                                    {
                                        ai[2] = 0;
                                        aiFlags[2] = true;
                                    }

                                    if (aiFlags[2])
                                        Core._Player.Transform.Position = Transform.Centre - new Vector2(Core._Player.Transform.Width / 2, Core._Player.Transform.Height / 2);

                                    if (ai[4] > 0f)
                                        ai[4] -= SSS.Delta;
                                }
                                else if (ai[0] == 1)
                                {
                                    CantStun = true;
                                    ai[1] += SSS.Delta;
                                    if (ai[1] < 1.8f)
                                    {
                                        PreRam = true;
                                        Transform.Gravity = false;
                                        Transform.Velocity = Vector2.Zero;
                                        Vector2 elipse = new Vector2(MathF.Sin(ai[1] * 4) * 1.5f * 2, MathF.Cos(ai[1] * 4) * 0.5f - 0.5f) * 60;
                                        Transform.Position = Vector2.Lerp(Transform.Position, Core._Player.Transform.Centre + elipse - new Vector2(Transform.Width / 2, 0), 0.05f);
                                    }
                                    else if (ai[1] < 3.5f)
                                    {
                                        PreRam = false;
                                        Ramming = MathF.Abs(ai[2]) > Speed * 2f;
                                        Transform.Gravity = true;

                                        if (ai[1] < 1.8f + SSS.Delta)
                                        {
                                            AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                            ai[2] = RamAmount * horiDirToPlayer * 5;
                                        }

                                        ai[2] = MathHelper.Lerp(ai[2], 0f, 0.1f);
                                        if (MathF.Abs(ai[2]) > 0.1f)
                                            Transform.ChangeVelocityX(ai[2]);
                                    }
                                    else if (ai[1] < 5f)
                                        Transform.Velocity = Vector2.Zero;
                                    else
                                    {
                                        ai[0] = 0;
                                        ai[1] = 0;
                                        ai[2] = 0;
                                        ai[3] = 0;
                                        ai[4] = 0;
                                        ai[5] = 0;
                                    }
                                }
                                break;
                            #endregion
                            #region Boss - Stenonat
                            case 19:
                                Ramming = false;
                                updateNodes = false;
                                CantStun = true;
                                Invincible = false;
                                CantKnockback = false;
                                //Transform.Gravity = false;
                                bool phaseTwo = Health < bossPhaseMakers[0];
                                int phaseNum = phaseTwo ? 1 : 0;
                                Speed = phaseTwo ? 4.25f : 3.5f;

                                /*if (!Ramming && !PreRam && ai[7] >= 10)
                                {
                                    ai[6] = ai[0];
                                    ai[7] = 0;
                                    if (DistToPlayer > 400)
                                        ai[0] = 3;
                                }
                                else
                                    ai[7] += SSS.Delta;*/

                                if (ai[0] == 0) //@1 - Ram State 1, @2 - Ram Amount, @3 - Ram Dir X, @4 - Ram Dir Y, @5 - Side + Ram Counter
                                {
                                    Ramming = MathF.Abs(ai[2]) > Speed * 2;
                                    ai[2] = MathHelper.Lerp(ai[2], 0, 0.1f);

                                    if (!aiFlags[0])
                                    {
                                        ai[1] += SSS.Delta;

                                        if (DistToPlayer < 80 && MathF.Abs(Core._Player.Transform.Centre.X - Transform.Centre.X) <= 80)
                                        {
                                            if (ai[1] >= 0.5f)
                                            {
                                                AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                                aiFlags[0] = true;
                                                ai[1] = 0f;
                                            }
                                        }
                                        else
                                            ai[1] = 0f;
                                    }

                                    if (aiFlags[0])
                                    {
                                        ai[1] += SSS.Delta;
                                        if (ai[1] > 0.4f)
                                        {
                                            if (ai[1] <= 0.4f + SSS.Delta)
                                            {
                                                PreRam = false;
                                                AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                                ai[2] = RamAmount;
                                                ai[3] = dirToPlayer.X;
                                                ai[4] = dirToPlayer.Y;
                                                ai[5]++;
                                            }
                                        }
                                        else
                                            PreRam = true;

                                        if (ai[1] > 0.8f)
                                        {
                                            aiFlags[0] = false;
                                            ai[1] = 0f;
                                        }
                                    }

                                    if (MathF.Abs(ai[2]) > 0.1f)
                                        Transform.Velocity = new Vector2(ai[3], ai[4]) * ai[2];
                                    else
                                    {
                                        int targetSide = ai[5] == 0 || ai[5] == 1 ? 1 : -1;
                                        Vector2 dirToTarget = new Vector2(Core._Player.Transform.Centre.X + (40 * targetSide), Core._Player.Transform.Centre.Y);
                                        dirToTarget = Vector2.Normalize(dirToTarget - Transform.Centre);
                                        Transform.Velocity = Vector2.Lerp(Transform.Velocity, dirToTarget * Speed, 0.055f + (0.02f * phaseNum));

                                        if (ai[5] > 2 + phaseNum)
                                        {
                                            ai[0] = 1;
                                            ai[1] = 0;
                                            ai[2] = 0;
                                            ai[3] = 0;
                                            ai[4] = 0;
                                            ai[5] = 0;
                                        }
                                    }
                                }
                                else if (ai[0] == 1)
                                {
                                    Invincible = true;
                                    CantKnockback = true;
                                    CantStun = true;
                                    Ramming = MathF.Abs(ai[2]) > Speed * 2;
                                    ai[2] = MathHelper.Lerp(ai[2], 0, 0.1f);

                                    if (!aiFlags[0] && !aiFlags[1] && ai[1] < 4f)
                                    {
                                        Vector2 dirToTarget = new Vector2(Core._Player.Transform.Centre.X - 60, Core._Player.Transform.Centre.Y);
                                        dirToTarget = Vector2.Normalize(dirToTarget - Transform.Centre);
                                        Transform.Velocity = Vector2.Lerp(Transform.Velocity, dirToTarget * Speed, 0.055f);

                                        if (DistToPlayer < 84 && MathF.Abs(Core._Player.Transform.Centre.X - Transform.Centre.X) <= 84)
                                        {
                                            ai[1] += SSS.Delta;

                                            if (MathF.Abs(Core._Player.Transform.Velocity.X) > Core._Player.RamAmount / 2)
                                            {
                                                ai[1] = 0;
                                                AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                                aiFlags[0] = true;
                                            }
                                        }
                                        if (ai[1] >= 4f)
                                        {
                                            ai[0] = 2;
                                            aiFlags[0] = false;
                                            aiFlags[1] = false;
                                            ai[1] = 0;
                                        }
                                    }

                                    if (aiFlags[0] && !aiFlags[1])
                                    {
                                        ai[1] += SSS.Delta * 8;
                                        PreRam = true;
                                        Transform.Velocity = Vector2.Zero;
                                        float rot = -90 - MathHelper.ToDegrees(ai[1]);
                                        Vector2 pos = Transform.RotateAroundAPoint(Core._Player.Transform.Centre, rot, 70);
                                        Transform.Position = Vector2.Lerp(Transform.Position, pos, 0.055f);

                                        if (rot < -360)
                                        {
                                            ai[1] = 0;
                                            aiFlags[1] = true;
                                            AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                            aiFlags[0] = false;
                                            aiFlags[2] = !phaseTwo;
                                            ai[2] = horiDirToPlayer * RamAmount;
                                        }
                                    }

                                    if (aiFlags[1])
                                    {
                                        PreRam = false;
                                        if (MathF.Abs(ai[2]) > 0.1f)
                                            Transform.ChangeVelocityX(ai[2]);
                                        else if (aiFlags[2] && timeInCurrentState > 4f)
                                        {
                                            ai[0] = 2;
                                            ai[1] = 0;
                                            aiFlags[0] = false;
                                            aiFlags[1] = false;
                                            ai[2] = 0;
                                        }

                                        if (!aiFlags[2] && MathF.Abs(ai[2]) <= 1f)
                                        {
                                            AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                            ai[2] = horiDirToPlayer * RamAmount;
                                            aiFlags[2] = true;
                                        }
                                    }
                                }
                                else if (ai[0] == 2)
                                {
                                    Ramming = MathF.Abs(ai[2]) > Speed * 2;
                                    ai[2] = MathHelper.Lerp(ai[2], 0, 0.1f);

                                    if(phaseTwo && timeInCurrentState >= 0.8f - SSS.Delta && timeInCurrentState < 0.8f)
                                        Core.SpawnProjectile(new Projectile(0, this), Transform.Centre, dirToPlayer);

                                    if (!aiFlags[0])
                                    {
                                        if (DistToPlayer < 80 && MathF.Abs(Core._Player.Transform.Centre.Y - Transform.Centre.Y) <= 80)
                                        {
                                            ai[1] += SSS.Delta;
                                            if (ai[1] > 0.5f)
                                            {
                                                ai[1] = 0;
                                                AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                                aiFlags[0] = true;
                                            }
                                        }
                                        else
                                        {
                                            ai[1] = 0;
                                            if (timeInCurrentState > 6)
                                            {
                                                ai[1] = 0;
                                                ai[2] = 0;
                                                aiFlags[0] = false;
                                                ai[0] = Random.Shared.Next(0, 4) != 1 ? 4 : 0;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        PreRam = true;
                                        ai[1] += SSS.Delta;
                                        if (ai[1] > 0.4f)
                                        {
                                            PreRam = false;
                                            if (ai[1] <= 0.4f + SSS.Delta)
                                            {
                                                AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                                ai[2] = RamAmount * (1.25f * (phaseNum + 1));
                                            }
                                            else if (ai[2] <= 0.1f)
                                            {
                                                ai[1] = 0;
                                                ai[2] = 0;
                                                aiFlags[0] = false;
                                                ai[0] = Random.Shared.Next(0, 4) != 1 ? 4 : 0;
                                            }
                                        }
                                    }

                                    if (ai[2] > 0.1f)
                                        Transform.ChangeVelocityY(ai[2]);
                                    else
                                    {
                                        Vector2 dirToTarget = new Vector2(Core._Player.Transform.Centre.X, Core._Player.Transform.Centre.Y - 40);
                                        dirToTarget = Vector2.Normalize(dirToTarget - Transform.Centre);
                                        Transform.Velocity = Vector2.Lerp(Transform.Velocity, dirToTarget * Speed, 0.07f + (0.03f * phaseNum));
                                    }
                                }
                                else if (ai[0] == 3)
                                {
                                    if (ai[1] < 1f)
                                    {
                                        if (ai[1] <= SSS.Delta)
                                            AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);

                                        ai[1] += SSS.Delta;
                                        Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, 0, 0.05f));
                                        PreRam = true;

                                        if (ai[1] >= 1f - SSS.Delta)
                                        {
                                            PreRam = false;
                                            AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                        }

                                        if (phaseTwo)
                                        {
                                            if (ai[8] >= 0.3f)
                                            {
                                                Core.SpawnProjectile(new Projectile(4, this), Transform.Centre, Vector2.Zero);
                                                ai[8] = 0;
                                            }
                                            ai[8] += SSS.Delta;
                                        }
                                    }
                                    else
                                    {
                                        Ramming = true;
                                        Transform.Velocity = Vector2.Lerp(Transform.Velocity, dirToPlayer * RamAmount * 2, 0.025f);

                                        if (DistToPlayer < 32 && timeInCurrentState > 1f)
                                        {
                                            ai[0] = ai[6];
                                            Ramming = false;
                                        }
                                    }
                                }
                                else if (ai[0] == 4)
                                {
                                    CantStun = true;
                                    if (ai[1] == 0)
                                    {
                                        Transform.ChangeVelocityX(0);
                                        if (timeInCurrentState > 1.5f)
                                        {
                                            if (Grounded)
                                            {
                                                ai[1] = 1;
                                                Transform.ChangeVelocityY(-5f);
                                                if(phaseTwo)
                                                    Core.SpawnProjectile(new Projectile(0, this), Transform.Centre, dirToPlayer);
                                            }
                                            if (timeInCurrentState > 4f)
                                            {
                                                ai[0] = Random.Shared.Next(0, 2) != 1 ? 6 : 0;
                                                ai[0] = Random.Shared.Next(0, 8) != 1 ? 3 : ai[0];
                                                ai[1] = 0;
                                                ai[2] = 0;
                                                ai[6] = 0;
                                            }
                                        }
                                    }
                                    else if (ai[1] == 1)
                                    {
                                        int hori = MathUtil.IntSign(Core._Player.Transform.Centre.X + 60 - Transform.Centre.X);
                                        Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, hori, 0.055f));
                                        if (DistToPlayer < 70)
                                        {
                                            AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                            ai[1] = 2;
                                        }

                                        if (timeInCurrentState > 7f)
                                        {
                                            ai[0] = Random.Shared.Next(0, 2) != 1 ? 6 : 0;
                                            ai[0] = Random.Shared.Next(0, 8) != 1 ? 3 : ai[0];
                                            ai[1] = 0;
                                            ai[2] = 0;
                                            ai[6] = 0;
                                        }
                                    }
                                    else if (ai[1] == 2)
                                    {
                                        if (ai[3] < 0.4f)
                                        {
                                            PreRam = true;
                                            ai[3] += SSS.Delta;
                                            if (ai[3] >= 0.4f - SSS.Delta)
                                            {
                                                AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                                ai[2] = RamAmount * horiDirToPlayer * 1.25f;
                                            }
                                        }
                                        else
                                        {
                                            PreRam = false;
                                            Ramming = MathF.Abs(ai[2]) > Speed * 2;
                                            ai[2] = MathHelper.Lerp(ai[2], 0, 0.1f);
                                            if (MathF.Abs(ai[2]) > 0.1f)
                                                Transform.ChangeVelocityX(ai[2]);
                                            else
                                            {
                                                ai[0] = Random.Shared.Next(0, 2) != 1 ? 6 : 0;
                                                ai[0] = Random.Shared.Next(0, 8) != 1 ? 3 : ai[0];
                                                ai[1] = 0;
                                                ai[2] = 0;
                                                ai[6] = 0;
                                            }
                                        }
                                    }
                                }
                                else if (ai[0] == 6)
                                {
                                    if (isTouchingWall)
                                        Transform.Velocity = Vector2.Zero;
                                    else
                                        Transform.ChangeVelocityX(-horiDirToPlayer * Speed / 4);

                                    if ((DistToPlayer < 32 && timeInCurrentState > 0.5f) || timeInCurrentState > 3f)
                                        ai[0] = 0;
                                }
                                break;
                            #endregion
                            #region Mini Boss - Gang Serpent
                            case 20:
                                Transform.Gravity = false;

                                if (CollisionEngine.RectInRect(Core._Player.Transform.GetRectangle(), Transform.GetRectangle()))
                                {
                                    Transform.Gravity = true;
                                    Phantom = false;
                                    if (ai[3] == 0)
                                        ai[3] = ai[0];
                                    ai[0] = 5;
                                }

                                if (ai[0] == 1)
                                {
                                    Transform.Velocity = new Vector2(0, Speed);
                                    if(Transform.Position.Y + Transform.Height >= 800)
                                    {
                                        Transform.Velocity = Vector2.Zero;
                                        Transform.Position = new Vector2(1536, 800 - Transform.Height);
                                        ai[0] = 2;
                                    }
                                }
                                else if (ai[0] == 2)
                                {
                                    if(timeInCurrentState > 1f)
                                    {
                                        if (MathF.Abs(Transform.Velocity.X) < Speed)
                                            Transform.ChangeVelocityX(Transform.Velocity.X + (0.03f * horiDirToPlayer));
                                    }

                                    if(Transform.Position.X >= 2016)
                                    {
                                        Transform.Position = new Vector2(2016, 800 - Transform.Height);
                                        Phantom = false;
                                        Transform.ChangeVelocityY(-Speed * 2);
                                        ai[0] = 3;
                                    }
                                }
                                else if (ai[0] == 3)
                                {
                                    Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, -Speed, 0.05f));
                                    Transform.Gravity = true;
                                    if (Transform.Position.Y <= 352)
                                    {
                                        ai[0] = 4;
                                        if (ai[1] == 1)
                                            ai[1] = 2;
                                    }
                                }
                                else if (ai[0] == 4)
                                {
                                    Transform.Gravity = true;
                                    if (Transform.Position.X <= 1600)
                                        Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, -Speed * 0.9f, 0.75f));
                                    else
                                        Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, -Speed * 1.1f, 0.05f));

                                    if (Transform.Position.X <= 1600 - Transform.Width && ai[1] != 2)
                                    {
                                        Transform.Velocity = Vector2.Zero;
                                        Transform.Position = new Vector2(1600 - Transform.Width, Transform.Position.Y);
                                        ai[0] = 1;
                                        Phantom = true;
                                    }
                                }
                                else if (ai[0] == 5)
                                {
                                    if (ai[3] == 1)
                                        Transform.Velocity = Vector2.Lerp(Transform.Velocity, Vector2.Zero, 0.75f);
                                    else
                                        Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, 0f, 0.75f));
                                    Core._Player.Transform.Velocity = Vector2.Zero;

                                    Vector2 targetPos = Transform.Centre - new Vector2(Core._Player.Transform.Width / 2, Core._Player.Transform.Height / 2);
                                    if (ai[3] == 1)
                                        targetPos += new Vector2(0, 1) * 32;
                                    else if (ai[3] == 2)
                                        targetPos += new Vector2(1, 0) * 32;
                                    else if (ai[3] == 3)
                                        targetPos += new Vector2(0, -1) * 32;
                                    else
                                        targetPos += new Vector2(-1, 0) * 32;

                                    Core._Player.Transform.Position = Vector2.Lerp(Core._Player.Transform.Position, targetPos, 0.1f);

                                    ai[2] += SSS.Delta;
                                    if (ai[2] > 1)
                                        Core._Player.Health = 0;
                                }
                                break;
                            #endregion
                            #region Mini Boss - Faucet Clog
                            case 21:
                                float sdist = Core._Player.Transform.Centre.X - Transform.Centre.X;
                                int aside = MathUtil.IntSign(sdist);
                                sdist = MathF.Abs(sdist);

                                if (ai[0] == 0)
                                {
                                    if (ai[3] > 0)
                                        ai[3] -= SSS.Delta;

                                    if (sdist < 80 && Core._Player.Transform.Position.Y > Transform.Position.Y && ai[3] <= 0)
                                    {
                                        ai[1] += SSS.Delta;
                                        if (ai[1] > 0.25f)
                                        {
                                            ai[1] = 0;
                                            ai[2] = 0;
                                            ai[3] = 2;
                                            ai[0] = MathF.Abs(Core._Player.Transform.Velocity.X) >= Core._Player.Speed ? 1 : 2;
                                        }
                                    }
                                    else
                                        ai[1] = 0;
                                }
                                else if (ai[0] == 1)
                                {
                                    if (ai[2] == 0)
                                    {
                                        Hitboxes[1] = MakeHBSide(new Rectangle(0, (int)(Core._Player.Transform.Position.Y - Transform.Position.Y), 70, 10), aside);
                                        AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                    }

                                    ai[2] += SSS.Delta;
                                    if (ai[2] >= 0.6f)
                                    {
                                        ai[0] = 0;
                                        Hitboxes.Remove(1);
                                    }
                                }
                                else if (ai[0] == 2)
                                {
                                    ai[2] += SSS.Delta;
                                    if (ai[2] >= 0.3f && ai[2] < 0.3f + SSS.Delta)
                                    {
                                        Hitboxes[1] = MakeHBSide(new Rectangle(0, (int)(Core._Player.Transform.Position.Y - Transform.Position.Y), 50, 8), aside);
                                        AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                    }
                                    else if (ai[2] >= 0.6f)
                                    {
                                        ai[0] = 0;
                                        Hitboxes.Remove(1);
                                    }
                                }
                                else if (ai[0] == 3)
                                {
                                    ai[4] += SSS.Delta;
                                    if (ai[4] >= 3f)
                                    {
                                        Invincible = false;
                                        Kill();
                                    }
                                }
                                break;
                            #endregion
                            #region Sky Scythe Stationary
                            case 24:
                                if (ai[2] == 0)
                                {
                                    ai[2] = Transform.Position.X;
                                    ai[3] = Transform.Position.Y;
                                }

                                ai[1] += SSS.Delta / 2;
                                if (ai[1] >= MathF.PI * 2)
                                    ai[1] = 0;
                                Transform.Position = new Vector2(ai[2], ai[3] + MathF.Sin(ai[1]) * 20);
                                break;
                            #endregion
                            #region Sky Scythe Trader
                            case 25:
                                if (ai[2] == 0)
                                {
                                    ai[2] = Transform.Position.X;
                                    ai[3] = Transform.Position.Y;
                                }

                                ai[1] += SSS.Delta / 2;
                                if (ai[1] >= MathF.PI * 2)
                                    ai[1] = 0;
                                Transform.Position = new Vector2(ai[2], ai[3] + MathF.Sin(ai[1]) * 20);

                                if (ai[0] == 0)
                                {
                                    if (Core._NPCs.Where(x => x.ID == 3).Any())
                                    {
                                        var closest = Core.GetClosestNPC(Transform.Centre, true, out float deadEnDist, 230);
                                        if (closest != null && closest.ID == 3)
                                            ai[0] = 1;
                                    }
                                    else
                                        ai[0] = 1;
                                }
                                if (ai[0] == 1)
                                {
                                    Core.SpawnNPC(new NPC(5, 300, 0, PhantomType.None), new Vector2(190, 703));
                                    ai[0] = 2;
                                }
                                break;
                            #endregion
                            #region Placeholder Talking NPC
                            case 26:
                                StopDraw = ai[0] == 1;
                                break;
                            #endregion
                            #region Gorjan
                            case 27:
                                gravity = true;
                                StopDraw = false;
                                Phantom = false;
                                updateNodes = true;
                                Speed = ai[1] == 0 ? 3f : 6f;
                                if (ai[0] == 1)
                                {
                                    Ramming = true;
                                    PreRam = true;
                                    Vector2 target = CanSeePlayer && (DistToPlayer < 128 || LevelHandler.CurrentLevel == 13) ? dirToPlayer : Vector2.Normalize(NavController.NextNodePos - Transform.Centre);
                                    Transform.Velocity = Vector2.Lerp(Transform.Velocity, target * Speed, 0.05f);
                                }
                                else if (ai[0] == 2)
                                {
                                    gravity = false;
                                    Phantom = true;
                                    StopDraw = true;
                                }
                                else if (ai[0] == 3)
                                {
                                    ai[2] += SSS.Delta;
                                    Transform.ChangeVelocityX(2);
                                    if (ai[2] >= 10)
                                        Kill();
                                }
                                else if (ai[0] == 4)
                                {
                                    if (ai[3] == 0)
                                    {
                                        Transform.Position = new Vector2(2592, 2032);
                                        Transform.Velocity = Vector2.Zero;
                                        Transform.ResetOldPosition();
                                        ai[3] = 1;
                                    }
                                    if (ai[3] == 2)
                                    {
                                        Ramming = false;
                                        Transform.Velocity = Vector2.Lerp(Transform.Velocity, dirToPlayer * Speed * 1.5f, 0.1f);
                                        ai[3] = CollisionEngine.RectInRect(Transform.GetRectangle(), Core._Player.Transform.GetRectangle()) ? 3 : 2;
                                        ai[4] = 16f;
                                    }
                                    else if (ai[3] == 3)
                                    {
                                        ai[4] = MathHelper.Lerp(ai[4], 8f, 0.05f);
                                        Transform.Velocity = new Vector2(ai[4], -0.02f);
                                    }
                                }
                                break;
                            #endregion
                            #region Boss - Fynalie
                            case 28:
                                Ramming = false;
                                updateNodes = false;

                                if (ai[0] == 0) //@1 - P1 Shoot timer + Reached centre platform, @2 - Evasion side change, @3 - Shoot movement cooldown, @4 - Centre Plat. timer, @5 - Know will P2 sweep, @6 - Reached bottom platform, @7 - Circle timer
                                {
                                    TakeNoPlayerDamage = true;
                                    CantStun = true;
                                    CantKnockback = true;
                                    if (ai[3] <= 0)
                                    {
                                        Vector2 target = Core._Player.Transform.Centre - new Vector2(90 * (ai[2] <= 9 ? -1 : 1), 30);
                                        Transform.Velocity = Vector2.Lerp(Transform.Velocity, Vector2.Normalize(target - Transform.Centre) * 4, 0.1f);
                                    }
                                    else
                                    {
                                        Transform.Velocity = new Vector2(MathHelper.Lerp(Transform.Velocity.X, 0, 0.1f), 0.25f);
                                        ai[3] -= SSS.Delta;
                                    }

                                    ai[2] += SSS.Delta;
                                    if (ai[2] >= 18)
                                        ai[2] = 0;

                                    if (ai[1] >= 3.5f)
                                    {
                                        ai[1] = 0;
                                        Core.SpawnProjectile(new Projectile(7, this), Transform.Centre, dirToPlayer);
                                        ai[3] = 0.8f;
                                    }
                                    else if (DistToPlayer < 130)
                                        ai[1] += SSS.Delta;

                                    if (Health == 7 || Health == 3)
                                    {
                                        ai[0] = 1;
                                        ai[1] = 0;
                                        ai[2] = 0;
                                        ai[3] = 0;
                                    }
                                }
                                else if (ai[0] == 1)
                                {
                                    if (ai[1] == 0)
                                    {
                                        Vector2 target = new Vector2(1680, 677);
                                        Transform.Velocity = Vector2.Lerp(Transform.Velocity, Vector2.Normalize(target - Transform.Centre) * 2, 0.1f);
                                        if (Vector2.Distance(Transform.Centre, target) < 5)
                                        {
                                            Transform.Velocity = Vector2.Zero;
                                            ai[1] = 1;
                                            ai[4] = 8;
                                        }
                                    }
                                    else
                                    {
                                        ai[4] -= SSS.Delta;
                                        if (DistToPlayer < 20 || ai[4] <= 0)
                                        {
                                            Health += 3;
                                            ai[1] = 0;
                                            ai[4] = 0;
                                            ai[0] = 0;
                                        }

                                        if (Health == 6 || Health == 2)
                                        {
                                            ai[1] = 0;
                                            ai[4] = 0;
                                            ai[0] = Health == 2 ? 2 : 0;
                                            if (ai[0] == 2)
                                            {
                                                ScriptManager.ExecuteFunction("custom_fynalie", Array.Empty<object>());
                                                Core._Player.Health = Core._Player.MaxHealth;
                                                Core._Player.RestoreBoosts();
                                            }
                                        }
                                    }
                                }
                                else if (ai[0] == 2)
                                {
                                    float ltar = Transform.Centre.X < 1824 ? 2 : 0;
                                    Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, ltar, 0.1f));
                                    ai[1] = 0;
                                    ai[2] = 0;
                                    ai[3] = 0;
                                    ai[4] = 0;
                                }
                                else if (ai[0] == 3)
                                {
                                    Invincible = true;
                                    int xpos = -1;
                                    if (ai[2] >= 5 && ai[2] < 10)
                                        xpos = 0;
                                    else if (ai[2] >= 10)
                                        xpos = 1;
                                    Vector2 target = Core._Player.Transform.Centre - new Vector2(140 * xpos, 30 * (ai[2] < 10 && ai[2] >= 5 ? 2.25f : 1));
                                    Transform.Velocity = Vector2.Lerp(Transform.Velocity, Vector2.Normalize(target - Transform.Centre) * 4, 0.1f);

                                    ai[2] += SSS.Delta;
                                    if (ai[2] >= 15)
                                        ai[2] = 0;

                                    if (ai[1] >= 2f)
                                    {
                                        ai[1] = 0;
                                        float angle = MathF.Atan2(Core._Player.Transform.Centre.Y - Transform.Centre.Y, Core._Player.Transform.Centre.X - Transform.Centre.X);
                                        Core.SpawnProjectile(new Projectile(0, this), Transform.Centre, new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 1.5f);
                                        Core.SpawnProjectile(new Projectile(0, this), Transform.Centre, new Vector2(MathF.Cos(angle + 0.15f), MathF.Sin(angle + 0.15f)) * 1.5f);
                                        Core.SpawnProjectile(new Projectile(0, this), Transform.Centre, new Vector2(MathF.Cos(angle - 0.15f), MathF.Sin(angle - 0.15f)) * 1.5f);
                                    }
                                    else
                                        ai[1] += SSS.Delta;

                                    if (ai[2] >= 15 - SSS.Delta)
                                    {
                                        if (ai[5] == 1)
                                        {
                                            ai[5] = 0;
                                            Transform.ChangeVelocityX(16);
                                            AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                            ai[0] = 4;
                                            ai[3] = 0;
                                        }
                                    }
                                    if (ai[2] >= 14.6 && ai[2] <= 14.6 + SSS.Delta)
                                    {
                                        ai[5] = 1;
                                        AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                    }
                                    PreRam = ai[5] == 1;
                                }
                                else if (ai[0] == 4)
                                {
                                    if (ai[3] > 0.5f)
                                    {
                                        Transform.Velocity = -dirToPlayer * 2;
                                        ai[4] += SSS.Delta;
                                        if (ai[4] > 0.2f)
                                        {
                                            ai[4] = 0;
                                            Core.SpawnProjectile(new Projectile(0, this), Transform.Centre, dirToPlayer);
                                        }
                                    }
                                    else
                                    {
                                        Ramming = MathF.Abs(Transform.Velocity.X) > Speed;
                                        Transform.ChangeVelocityX(Transform.Velocity.X / 1.075f);
                                    }

                                    if (ai[3] >= 4f)
                                    {
                                        ai[0] = 5;
                                        ai[1] = 0;
                                        ai[2] = 0;
                                        ai[3] = 0;
                                        ai[4] = 0;
                                    }

                                    ai[3] += SSS.Delta;
                                }
                                else if (ai[0] == 5)
                                {
                                    if (ai[6] == 0)
                                    {
                                        var dist = Vector2.Distance(Transform.Centre, new Vector2(1680, 1200));
                                        Transform.Velocity = Vector2.Normalize(new Vector2(1680, 1200) - Transform.Centre);
                                        if(dist <= 6)
                                        {
                                            ai[6] = 1;
                                            Transform.Velocity = Vector2.Zero;
                                        }
                                    }
                                    else
                                    {
                                        if (ai[6] == 1)
                                        {
                                            for (int ang = 0; ang < 360; ang += 10)
                                            {
                                                if (ang > 150 && ang < 220)
                                                    continue;
                                                Vector2 pos = Transform.Centre + (new Vector2(MathF.Cos(MathHelper.ToRadians(ang)), MathF.Sin(MathHelper.ToRadians(ang))) * 64);
                                                Core.SpawnProjectile(new Projectile(8, this), pos, Vector2.Zero);
                                            }

                                            for (int ang = 0; ang < 360; ang += 5)
                                            {
                                                if (ang > 70 && ang < 110)
                                                    continue;
                                                Vector2 pos = Transform.Centre + (new Vector2(MathF.Cos(MathHelper.ToRadians(ang)), MathF.Sin(MathHelper.ToRadians(ang))) * 135);
                                                Core.SpawnProjectile(new Projectile(8, this), pos, Vector2.Zero);
                                            }

                                            if (DistToPlayer < 50)
                                                Core._Player.Knockback(Transform, 4);

                                            ai[6] = 2;
                                        }

                                        if (ai[7] < 10)
                                        {
                                            ai[7] += SSS.Delta;
                                            CantKnockback = false;
                                            Invincible = false;
                                            TakeNoPlayerDamage = false;
                                        }

                                        if (ai[7] >= 10)
                                        {
                                            Core._Player.RestoreBoosts();
                                            CantKnockback = true;
                                            Invincible = true;
                                            TakeNoPlayerDamage = true;
                                            ai[7] = 0;
                                            ai[6] = 0;
                                            ai[5] = 0;
                                            ai[1] = 0;
                                            ai[2] = 0;
                                            ai[3] = 0;
                                            ai[4] = 0;
                                            ai[0] = 3;
                                        }
                                    }
                                }
                                break;
                            #endregion
                            #region Boss - LM Base
                            case 29:
                            CantKnockback = true;
                            updateNodes = false;

                            int lmside = Core._Player.Transform.Centre.X > Transform.Centre.X ? 1 : 0; //   0 [B] 1
                            if(ai[0] == 0) //@1 - General Attack Timer, @2 - Spew timer, @3 - Phase timer, @4 - Some counter, @5 - Phase, @6 & 7 - Head UUIDs
                            {
                                 if(ai[2] >= 2.25f){
                                    ai[2] = 0;
                                    var ran = new Random();
                                    var ranints = new float[2] { ran.Next(10, 20) / 10, ran.Next(10, 20) / 10 };
                                    Core.SpawnProjectile(new Projectile(9, this), Transform.Centre - new Vector2(0, 64), new Vector2(-1 * ranints[0], -ranints[1]));
                                    Core.SpawnProjectile(new Projectile(9, this), Transform.Centre - new Vector2(0, 64), new Vector2(1 * ranints[1], -ranints[0]));
                                }
                                else
                                    ai[2] += SSS.Delta;

                                float sideDist = Transform.Position.X + (Transform.Width * lmside) - Core._Player.Transform.Centre.X;
                                sideDist = MathF.Abs(sideDist);

                                if(ai[1] <= 0)
                                {
                                    if(sideDist < 70 && Core._Player.Transform.Position.Y > Transform.Position.Y && ai[1] > -1) {
                                        AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                        ai[1] = -1;
                                    }

                                    if(ai[1] <= -1){
                                        if(ai[1] > -1.4f)
                                            PreRam = true;
                                        ai[1] -= SSS.Delta;
                                    }

                                    if(ai[1] < -1.4f && ai[1] >= -1.4f - SSS.Delta)
                                    {
                                        AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                        Ramming = true;
                                        Hitboxes[1] = MakeHBSide(new Rectangle(0, 12, 80, 100),lmside);
                                    }
                                    else if(Ramming && ai[1] < -1.8f)
                                    {
                                        Ramming = false;
                                        Hitboxes.Remove(1);
                                        ai[1] = 3f;
                                    }
                                }
                                else
                                {
                                    ai[1] -= SSS.Delta;
                                }

                                ai[3] += SSS.Delta;
                                if(ai[3] > 8)
                                {
                                    ai[3] = -1;
                                    ai[2] = 0;
                                    ai[1] = 0;
                                    Hitboxes.Remove(1);
                                    Ramming = false;
                                    ai[0] = 1;
                                }
                            }
                            else if (ai[0] == 1)
                            {
                                if(ai[1] == 0) {
                                    if(ai[2] == 0)
                                        AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                    ai[2] += SSS.Delta;
                                    PreRam = true;
                                    if(ai[2] > 0.8f) {
                                        ai[2] = 0;
                                        ai[1] = 1;
                                        AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                    }
                                }
                                else if(ai[1] == 1) {
                                    ai[2] += SSS.Delta;
                                    Ramming = true;
                                    if(ai[2] > 0.8f) {
                                        Ramming = false;
                                        ai[2] = 0;
                                        ai[1] = 2;
                                    }
                                }
                                else {
                                    if(ai[2] <= 0.5f){
                                        if(ai[2] == 0){
                                            AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                            Hitboxes[1] = MakeHBSide(new Rectangle(0, 12, 80, 100), (int)ai[3]);
                                            ai[3] = ai[3] == -1 ? 1 : -1;
                                        }
                                        ai[2] += SSS.Delta;
                                    }
                                    else{
                                        ai[2] = 0;
                                        ai[4]++;
                                        Hitboxes.Remove(1);
                                        if(ai[4] == 8){
                                            ai[4] = 0;
                                            ai[3] = 0;
                                            ai[2] = 0;
                                            ai[1] = 0;
                                            ai[0] = 2;
                                        }
                                    }
                                }
                            }
                            else if(ai[0] == 2)
                            {
                                ai[1] += SSS.Delta;
                                if(ai[1] > 1.6f && ai[1] <= 2f){
                                    PreRam = true;
                                    if(ai[1] <= 1.6f + SSS.Delta){
                                        AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                        ai[2] = lmside;
                                    }
                                }

                                if(ai[1] > 2f && ai[1] <= 2f + SSS.Delta){
                                    AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                    Hitboxes[1] = MakeHBSide(new Rectangle(0, -25, 60, 70), (int)ai[2]);
                                }
                                else if (ai[1] > 2.8f && ai[1] <= 2.8f + SSS.Delta){
                                    Hitboxes.Remove(1);
                                    Hitboxes[1] = MakeHBSide(new Rectangle(0, 12, 80, 100), (int)ai[2]);
                                }
                                else if (ai[1] > 3.4f){
                                    Hitboxes.Remove(1);
                                    if(ai[1] > 4f){
                                            ai[1] = 0;
                                            ai[2] = 0;
                                            ai[0] = 3;
                                    }
                                }
                            }
                            else if (ai[0] == 3)
                            {
                                if(ai[1] > 2){
                                    if(ai[1] < 3)
                                    {
                                        Core.SpawnProjectile(new Projectile(10, this), Transform.Position + new Vector2(-100, Transform.Height), Vector2.Zero);
                                        Core.SpawnProjectile(new Projectile(10, this), Transform.Position + new Vector2(Transform.Width - 28, Transform.Height), Vector2.Zero);
                                        ai[1] = 3;
                                    }
                                    else if(ai[1] < 100)
                                    {
                                        if(ai[1] >= 3 + (ai[2] * 0.15f)) {
                                            ai[2]++;
                                            Core.SpawnProjectile(new Projectile(9, this), Transform.Centre - new Vector2(0, 64), new Vector2(MathF.Cos(3.141f + (ai[2] * 0.392f)), MathF.Sin(3.141f + (ai[2] * 0.392f))));

                                            if(ai[2] >= 8)
                                                ai[1] = 100;
                                        }
                                    }
                                }
                                ai[1] += SSS.Delta;

                                if(ai[1] >= 106){
                                    ai[1] = 0;
                                    ai[0] = 4;
                                }
                            }
                            else if(ai[0] == 4)
                            {
                                PreRam = ai[1] >= 2 && ai[1] < 2.1f;
                                Ramming = ai[1] > 2.1f && ai[1] < 2.7f;

                                if(ai[1] == 0)
                                    Transform.ChangeVelocityY(-7);
                                else if(ai[1] >= 2 && ai[1] <= 2 + SSS.Delta)
                                    AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                if(ai[1] >= 2.1f && ai[1] <= 2.1f + SSS.Delta){
                                    AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                    Transform.ChangeVelocityY(18);
                                }

                                if(ai[1] > 2.1f)
                                    Transform.ChangeVelocityX(0);
                                else
                                    Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, horiDirToPlayer * Speed, 0.1f));
                                ai[1] += SSS.Delta;

                                if(ai[1] >= 3){
                                    ai[1] = 0;
                                    ai[0] = 0;
                                }
                            }

                            if(Health == bossPhaseMakers[0] && ai[5] == 0){
                                ai[5] = 1;
                                ai[4] = 0;
                                ai[3] = 0;
                                ai[2] = 0;
                                ai[1] = 0;
                                ai[0] = 5;
                                TakeNoPlayerDamage = true;
                                Hitboxes.Remove(1);
                                var other = new NPC(30, 1000, 0, PhantomType.None);
                                ai[6] = other.UUID.MainUUID;
                                Core.SpawnNPC(other, Transform.Position - new Vector2(other.Transform.Width, 0));
                                other = new NPC(30, 1000, 0, PhantomType.None);
                                ai[7] = other.UUID.MainUUID;
                                Core.SpawnNPC(other, Transform.Position + new Vector2(Transform.Width, 0));
                            }

                            if(ai[0] == 5){
                                if(ai[1] == 0)
                                    Transform.ChangeVelocityY(-7);
                                Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, 0f, 0.1f));
                                ai[1] += SSS.Delta;

                                PreRam = ai[1] > 1.5f && ai[1] < 2f;
                                if(ai[1] > 1.5f && ai[1] < 1.5f + SSS.Delta)
                                    AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);

                                Ramming = Transform.Velocity.Y > 4f;
                                if(ai[1] > 2f && ai[1] < 2f + SSS.Delta){
                                    AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                    Transform.ChangeVelocityY(10);
                                }

                                if(ai[1] > 5f){
                                    ai[1] = 0;
                                    ai[0] = 6;
                                }
                            }
                            else if(ai[0] == 6){
                                ai[1] += SSS.Delta;
                                if(ai[1] > ai[2] && ai[2] < 2){
                                    ai[2]++;
                                    for(int a = 0; a < 360; a += 20){
                                        float ra = MathHelper.ToRadians(a);
                                        Core.SpawnProjectile(new Projectile(0, this), Transform.Centre , new Vector2(MathF.Cos(ra), MathF.Sin(ra)));
                                    }
                                }
                                else if(ai[1] > 5f){
                                    ai[1] = 0;
                                    ai[2] = 0;
                                    ai[0] = 7;
                                }
                            }
                            else if(ai[0] == 7){
                                if(ai[2] >= 2.25f){
                                    ai[2] = 0;
                                    var ran = new Random();
                                    var ranints = new float[2] { ran.Next(10, 20) / 10, ran.Next(10, 20) / 10 };
                                    Core.SpawnProjectile(new Projectile(9, this), Transform.Centre - new Vector2(0, 64), new Vector2(-1 * ranints[0], -ranints[1]));
                                    Core.SpawnProjectile(new Projectile(9, this), Transform.Centre - new Vector2(0, 64), new Vector2(1 * ranints[1], -ranints[0]));
                                }
                                else
                                    ai[2] += SSS.Delta;

                                ai[1] += SSS.Delta;
                                if(ai[1] > 7){
                                    ai[1] = 0;
                                    ai[2] = 0;
                                    ai[0] = 5;
                                }
                            }

                            if(ai[5] == 1 && !(ai[6] == -69 && ai[7] == -69)){
                                bool minion1Dead = !Core._NPCs.Where(x => x.UUID.MainUUID == ai[6]).Any();
                                bool minion2Dead = !Core._NPCs.Where(x => x.UUID.MainUUID == ai[7]).Any();
                                if(minion1Dead && ai[6] != -69){
                                    ai[6] = -69;
                                    Health--;
                                }
                                if(minion2Dead && ai[7] != -69){
                                    ai[7] = -69;
                                    Health--;
                                }
                            }
                            break;
                            #endregion
                            #region Boss - LM Head
                            case 30:
                            updateNodes = false;

                            if(ai[0] == 0){ //@1 - General attack timer, @2 - Jump check
                                if(ai[1] == 0)
                                    ai[6] = new Random().Next(2, 8);

                                ai[1] += SSS.Delta;
                                if(ai[2] == 0)
                                    Transform.ChangeVelocityX(MathF.Sin(ai[1] * 4) / 2);
                                else
                                    Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, horiDirToPlayer * Speed * 2, 0.1f));

                                if(((ai[1] > ai[6] / 2 && DistToPlayer < 64)|| ai[1] > ai[6]) && ai[2] == 0){
                                    ai[2] = 1;
                                    ai[1] = 0;
                                    AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                }

                                PreRam = ai[2] == 1;
                                if(ai[2] == 1 && ai[1] > 0.4f){
                                    Transform.ChangeVelocityY(-5);
                                    Ramming = true;
                                    AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                    ai[2] = 2;
                                    ai[1] = 0;
                                }

                                if(ai[2] == 2 && ai[1] > 0.7f){
                                    Ramming = false;
                                    if(ai[1] > 1.5f){
                                        var r = new Random().Next(1, 4);
                                        ai[0] = r;
                                        ai[1] = 0;
                                        ai[4] = 0;
                                        ai[3] = 0;
                                        ai[2] = 0;
                                    }
                                }
                            }
                            else if(ai[0] == 1){
                                ai[1] += SSS.Delta;
                                var tVel = horiDirToPlayer + (MathF.Sin(ai[1] * 4) / 2);
                                Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, tVel, 0.1f));

                                if(ai[1] > 6f || (ai[1] > 4f && DistToPlayer < 85)){
                                    ai[1] = 0;
                                    ai[2] = 0;
                                    ai[3] = 0;
                                    ai[4] = 0;
                                    ai[0] = 2;
                                }
                            }
                            else if(ai[0] == 2){
                                Ramming = false;

                                if(ai[2] == 0){
                                    Transform.Velocity = Vector2.Lerp(Transform.Velocity, dirToPlayer * Speed * 2, 0.1f);
                                    if(DistToPlayer < 120 && Core._Player.Transform.Centre.Y - Transform.Centre.Y < 6){
                                        ai[2] = 1;
                                        AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                    }
                                }
                                else if(ai[2] == 1){
                                    ai[1] += SSS.Delta;
                                    PreRam = ai[1] < 0.4f;

                                    Ramming = MathF.Abs(ai[3]) > Speed * 2;
                                    ai[3] = MathHelper.Lerp(ai[3], 0, 0.1f);
                                    if(MathF.Abs(ai[3]) > 0.2f)
                                        Transform.ChangeVelocityX(ai[3]);

                                    if(ai[1] > 0.4f && ai[1] <= 0.4f + SSS.Delta){
                                        AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                        ai[3] = RamAmount * horiDirToPlayer;
                                    }

                                    if(!Ramming && ai[1] > 1f){
                                        ai[1] = 0;
                                        ai[3] = 0;
                                        ai[2] = 2;
                                    }
                                }
                                else if(ai[2] == 2){
                                    if(ai[3] == 0)
                                        ai[3] = horiDirToPlayer;
                                    ai[1] += SSS.Delta;

                                    if(ai[1] >= 0.2f * ai[4] && ai[4] < 15){
                                        ai[4]++;
                                        var ran = new Random();
                                        float angle = ran.Next(0, 20) / 10;
                                        angle += (ai[3] == 1 ? 0 : 1) * 180;
                                        Core.SpawnProjectile(new Projectile(11, ai[4] == 1 ? this : null), Transform.Centre + new Vector2(Transform.Width / 2 * ai[3], 0), new Vector2(MathF.Cos(angle), MathF.Sin(angle)));
                                    }

                                    if(ai[4] >= 15){
                                        if( ai[1] > 8){
                                            ai[0] = 3;
                                            ai[1] = 0;
                                            ai[2] = 0;
                                            ai[3] = 0;
                                            ai[4] = 0;
                                        }
                                    }
                                    else
                                        Transform.Velocity = new Vector2(0, -1f);
                                }
                            }
                            else if(ai[0] == 3){
                                if(ai[1] == 0){
                                    var other = Core._NPCs.Where(x => x.UUID.MainUUID != UUID.MainUUID && x.ID == ID);
                                    if(other.Any()) {
                                        var f = other.First();
                                        f.ai[0] = 3;
                                        f.ai[1] = 0.1f;
                                        f.ai[2] = 0;
                                        f.ai[3] = 0;
                                        f.ai[4] = 0;
                                        f.ai[5] = 1;
                                        ai[5] = -1;
                                    }
                                }

                                ai[1] += SSS.Delta;
                                if(ai[2] == 0){
                                    Vector2 dirToCen = Vector2.Normalize(new Vector2(1768, 1447) - Transform.Centre);
                                    Transform.Velocity = (dirToCen * 1.5f) + new Vector2(MathF.Cos(ai[1]), MathF.Sin(ai[1]));
                                    float dist = Vector2.Distance(new Vector2(1768, 1447), Transform.Centre);
                                    if(dist < 64 || float.IsInfinity(dist)){
                                        PreRam = true;
                                        if(ai[3] == 0){
                                            ai[3] = 1;
                                            AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                        }

                                        if(ai[1] > 6f){
                                            ai[2] = 1;
                                            AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                            ai[1] = SSS.Delta;
                                            Transform.Velocity = new Vector2(ai[5] * RamAmount, 10);
                                        }
                                    }
                                }
                                else{
                                    Ramming = Transform.Velocity.Y != 0;
                                    Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, 0, 0.1f));

                                    if(Transform.Velocity.Y == 0 && ai[1] > 3f){
                                        ai[0] = 0;
                                        ai[1] = 0;
                                        ai[2] = 0;
                                        ai[3] = 0;
                                        ai[5] = 0;
                                    }
                                }
                            }
                            break;
                            #endregion
                            #region Boss - Jesersant
                            case 31:
                            CantStun = ai[0] == 0;
                            updateNodes = false;
                            if(ai[3] != ai[1]){
                                ai[1] = ai[3];
                                ai[2] = 0;
                                ai[7] = 0;
                                ai[5] = 0;
                                ai[0] = ai[1];
                                TakeNoPlayerDamage = false;
                                Health--;
                            }

                            if(ai[0] == 0){ //@1 - Plug counter, @2 - Plug cal blocker, @3 - Plug checker
                                if(ai[2] == 0 && ai[1] <= 2){
                                    ai[2] = 1;
                                    ScriptManager.ExecuteFunction("custom_jesersant", new object[1] { $"{ai[1] + 1}" });
                                    TakeNoPlayerDamage = true;
                                }
                                if(MathF.Abs(Transform.Position.X - Core._Player.Transform.Position.X) > 400)
                                    Transform.Position = new Vector2(952, 1257);
                            }
                            else if(ai[0] != 4){
                                int amount = ai[0] == 1 ? 5 : 3;
                                amount = ai[0] == 3 ? 1 : amount;
                                if(Health != amount){
                                    ai[0] = 0;
                                    Transform.Velocity = Vector2.Zero;
                                    Transform.Gravity = true;
                                    if(ai[0] != 3)
                                        Core.SpawnNPC(new NPC(32, 100, 0, PhantomType.None), Transform.Position - new Vector2(150, 0));
                                }
                                else{
                                    Vector2 circlemotion = new Vector2(MathF.Cos(ai[6] * 2), MathF.Sin(ai[6] + 1) / 2);

                                    if(ai[0] == 1)
                                        Transform.Position = new Vector2(952, 1257) + (circlemotion * 10);
                                    else if(ai[7] < 2.4f){
                                        Transform.Gravity = false;
                                        Vector2 targetvel = Vector2.Normalize(Core._Player.Transform.Centre + new Vector2(70, -16) - Transform.Centre) * (DistToPlayer < 200 ? 1 : 0);
                                        Transform.Velocity = Vector2.Lerp(Transform.Velocity, targetvel * 1.5f, 0.05f);
                                    }
                                }

                                bool distCheck = DistToPlayer > 128;
                                if(ai[4] >= 3 && distCheck){
                                    Core.SpawnProjectile(new Projectile(12, this), Transform.Centre, new Vector2(horiDirToPlayer, -0.75f) * 3);
                                    ai[4] = 0;
                                }
                                else if(distCheck)
                                    ai[4] += SSS.Delta;

                                if(ai[5] == 0){
                                    ai[5] = 1;
                                    for(int a = 0; a <= 300; a += ai[0] == 1 ? 20 : 40){
                                        var proj = new Projectile(13, this);
                                        proj.ai[5] = MathHelper.ToRadians(a);
                                        proj.ai[0] = proj.ai[5];
                                        Core.SpawnProjectile(proj, Transform.Centre, Vector2.Zero);
                                    }
                                }

                                ai[6] += SSS.Delta * 1.7f;
                                if(ai[6] >= MathF.PI * 2)
                                    ai[6] = 0;

                                if(ai[0] >= 2){
                                    if((ai[7] < 2 && DistToPlayer < 170) || ai[7] >= 2)
                                        ai[7] += SSS.Delta;

                                    if(ai[7] >= 2 && ai[7] < 2.4f){
                                        PreRam = true;
                                        if(ai[7] <= 2 + SSS.Delta)
                                            AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                    }
                                    else if(ai[7] >= 2.4f){
                                        Ramming = true;
                                        if(ai[7] <= 2.4f + SSS.Delta)
                                            Transform.Velocity = dirToPlayer * 15;
                                        Transform.Velocity = Vector2.Lerp(Transform.Velocity, Vector2.Zero, 0.1f);
                                        if(ai[7] > 3.6f){
                                            Ramming = false;
                                            ai[7] = 0;
                                        }
                                    }
                                }
                            }
                            else if(ai[0] == 4){
                                float ang = MathHelper.ToRadians(120);
                                Transform.Position = new Vector2(952, 1160) + (new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * 150);
                                Transform.Gravity = false;

                                if(ai[7] == 0){
                                    ai[7] = 1;
                                    for(int a = 0; a < 360; a += 40){
                                        if(a == 120)
                                            continue;
                                        ang = MathHelper.ToRadians(a);
                                        Vector2 pos = new Vector2(952, 1160) + (new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * 150);
                                        var npc = new NPC(33, 1000, 0, PhantomType.None);
                                        Core.SpawnNPC(npc, pos);
                                    }
                                }
                                else if(ai[7] == 2){
                                    foreach(NPC little in Core._NPCs.Where(x => x.ID == 33).ToList())
                                        little.Kill();
                                    ai[7] = 0;
                                    ai[0] = 3;
                                }
                            }
                            break;
                            #endregion
                            #region Boss - Jesersant Hallucination
                            case 32:
                            updateNodes = false;
                            if(ai[8] == 0){
                                var other = Core._NPCs.Where(x => x.ID == 32 && x.UUID != UUID);
                                ai[8] = other.Any() ? other.First().UUID.MainUUID : 0;
                            }
                            if(ai[1] == 0)
                                ai[1] = ai[8] != 0 ? -Core.GetNPC((int)ai[8]).ai[1] : 1;

                            if(ai[0] == 0){ //@1 - Target side, @2 - General attack timer
                                Vector2 targetPos2 = Core._Player.Transform.Centre + new Vector2((70 + Transform.Width) * ai[1], -Transform.Height / 2);
                                bool flag2 = Vector2.Distance(targetPos2, Transform.Centre) < 130;

                                if(flag2){
                                    Transform.Position = Vector2.Lerp(Transform.Position, targetPos2, 0.02f);
                                    ai[2] += SSS.Delta;
                                }
                                var ttdir = Vector2.Normalize(targetPos2 - Transform.Centre) * 2;
                                Transform.Velocity = Vector2.Lerp(Transform.Velocity, flag2 ? Vector2.Zero : ttdir, 0.1f);

                                if(ai[2] >= 4){
                                    ai[0] = ai[6] >= 3 ? 2 : 1;
                                    ai[2] = 0;
                                }
                            }
                            else if(ai[0] == 1){
                                if(ai[2] == 0)
                                    AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                PreRam = ai[2] < 0.4f;
                                ai[2] += SSS.Delta;

                                if(ai[2] >= 0.4f){
                                    Ramming = true;
                                    if(ai[2] <= 0.4f + SSS.Delta){
                                        AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                        ai[4] = dirToPlayer.X;
                                        ai[5] = dirToPlayer.Y;
                                    }

                                    var john = MathF.Abs(Transform.Velocity.Length());
                                    if(ai[3] == 0){
                                        Transform.Velocity += 0.1f * new Vector2(ai[4], ai[5]) * (ai[7] == 1 ? 5 : 1);
                                        if(john > 4 * (ai[7] == 1 ? 4 : 1))
                                            ai[3] = 1;
                                    }
                                    else{
                                        Transform.Velocity *= new Vector2(0.8f);
                                        if(john <= 0.05f) {
                                            ai[3] = 0;
                                            ai[2] = 0;
                                            ai[1] = ai[1] == 1 ? -1 : 1;
                                            ai[1] = ai[8] != 0 ? -Core.GetNPC((int)ai[8]).ai[1] : ai[1];
                                            ai[0] = 0;
                                            ai[7] = 0;
                                            ai[6]++;
                                            Ramming = false;
                                        }
                                    }
                                }
                            }
                            else if(ai[0] == 2){
                                ai[2] += SSS.Delta / 2;

                                if(ai[3] == 0){
                                    var vec = new Vector2(MathF.Cos(ai[2]), MathF.Sin(ai[2])) * 130;
                                    vec -= new Vector2(Transform.Width / 2, Transform.Height / 2);
                                    Transform.Position = Vector2.Lerp(Transform.Position, Core._Player.Transform.Centre + vec, 0.03f);

                                    if(ai[2] > 2f){
                                        ai[2] = 0;
                                        ai[1] = -horiDirToPlayer;
                                        ai[1] = ai[8] != 0 ? -Core.GetNPC((int)ai[8]).ai[1] : ai[1];
                                        ai[3] = 0;
                                        ai[6] = 0;
                                        ai[0] = 1;
                                        ai[7] = 1;
                                    }
                                }
                            }
                            break;
                            #endregion
                            #region Boss - Jesersant Clone
                            case 33:
                            if(Health == 1){
                                var mainBoss = Core._NPCs.Where(x => x.ID == 31);
                                mainBoss.First().ai[7] = 2;
                            }
                            if(ai[2] == 0){
                                ai[2] = MathHelper.ToRadians(new Random().Next(1, 360));
                                ai[3] = Transform.Position.X;
                                ai[4] = Transform.Position.Y;
                            }

                            ai[1] += SSS.Delta;
                            if(ai[1] > MathF.PI * 2)
                                ai[1] = 0;

                            Transform.Position = new Vector2(ai[3], ai[4]) + (new Vector2(MathF.Cos(ai[1] + ai[2]), MathF.Sin(ai[1] + ai[2])) * 5);
                            break;
                            #endregion
                            #region Boss - Bafixi
                            case 34:
                            updateNodes = false;
                            Ramming = false;
                            Phantom = false;
                            if(Health <= bossPhaseMakers[0] && ai[7] == 0){
                                for(int i = 0; i < 7; i++)
                                    ai[i] = 0;
                                ai[7] = 1;
                                Core.SpawnProjectile(new Projectile(17, this), Transform.Centre, Vector2.Zero);
                            }
                            else if(Health <= bossPhaseMakers[1] && ai[7] == 1){
                                for(int i = 1; i < 7; i++)
                                    ai[i] = 0;
                                ai[7] = 2;
                                ai[0] = 6;
                            }

                            if(ai[0] == 0){ //@1 - Shoot timer, @2 - Shoot count
                                ai[1] += SSS.Delta;
                                if(ai[1] >= 2 * MathF.PI){
                                    ai[1] = 0;
                                    ai[2]++;
                                    if(ai[2] >= 4){
                                        ai[2] = 0;
                                        ai[1] = 0;
                                        ai[0] = 1;
                                    }
                                }
                                if(ai[1] == SSS.Delta){
                                    for(int i = 0; i < 9; i++){
                                        var initAng = MathF.Atan2(dirToPlayer.Y, dirToPlayer.X);
                                        initAng += MathHelper.ToRadians(40 - (10 * i));
                                        Core.SpawnProjectile(new Projectile(0, this), Transform.Centre, new Vector2(MathF.Cos(initAng), MathF.Sin(initAng)));
                                    }
                                }

                                Transform.Gravity = false;
                                int side = ai[2] == 0 || ai[2] == 2 ? 1 : -1;
                                var dir = Vector2.Normalize(Core._Player.Transform.Centre - new Vector2(135 * side, 0) - Transform.Centre) * 3;
                                dir += new Vector2(MathF.Cos(ai[1] *  4), MathF.Sin(ai[1] *  4)) * 1.25f;
                                Transform.Velocity = Vector2.Lerp(Transform.Velocity, dir, 0.1f);
                            }
                            else if(ai[0] == 1){ //@1 - Sub phase state, @2 - Far ram flag & Ram timer, @3 - Ram amount, @4 - Ram counter, @5 - Do double hit
                                if(ai[1] == 0){
                                    bool dch = DistToPlayer > 150;
                                    ai[1] = dch ? 1 : 2;
                                    if(dch){
                                        AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                    }
                                }
                                else if(ai[1] == 1){
                                    Ramming = true;
                                    Transform.Velocity = Vector2.Lerp(Transform.Velocity, ai[2] == 0 ? dirToPlayer * 8 : Vector2.Zero, 0.05f);
                                    if(DistToPlayer < 20 || ai[2] != 0){
                                        Ramming = false;
                                        ai[2] += SSS.Delta;
                                        if(ai[2] > 1.4f){
                                            ai[1] = 2;
                                            ai[2] = 0;
                                        }
                                    }
                                }
                                else{
                                    Transform.Gravity = !Ramming;
                                    Ramming = MathF.Abs(Transform.Velocity.X) > Speed;
                                    ai[3] = MathHelper.Lerp(ai[3], 0, 0.05f);
                                    Transform.ChangeVelocityX(ai[3]);

                                    ai[2] += SSS.Delta;
                                    int length = ai[7] == 1 ? 9 : 6;
                                    if(ai[2] > 0.6f * ai[6] && ai[2] <= 0.6f * ai[6] + SSS.Delta && ai[6] < length){
                                        ai[4] = 0.5f;
                                        ai[5] = ai[6] == 1 || ai[6] == 3 || ai[6] == 7 ? 1 : 0;
                                        ai[6]++;
                                    }
                                    else if(ai[6] >= length && MathF.Abs(Transform.Velocity.X) <= 0.05f){
                                        ai[0] = Random.Shared.Next(0, 4) == 1 ? 4 : 2;
                                        ai[1] = 0;
                                        ai[2] = 0;
                                        ai[3] = 0;
                                        ai[4] = 0;
                                        ai[5] = 0;
                                        ai[6] = 0;
                                    }

                                    if(ai[4] > 0){
                                        if(ai[4] == 0.5f)
                                            AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                        ai[4] -= SSS.Delta;
                                        PreRam = ai[4] > 0.35f;

                                        bool canRam = ai[4] <= 0.35f && ai[4] >= 0.35f - SSS.Delta;
                                        canRam = ai[5] == 1 ? (ai[4] <= 0.1f && ai[4] >= 0.1f - SSS.Delta) || canRam : canRam;
                                        if(canRam) {
                                            AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                            float distMult = DistToPlayer > 70 ? 1 : 0.5f;
                                            ai[3] = RamAmount * horiDirToPlayer * (ai[6] == length ? 1.5f : 1) * distMult;
                                            Transform.Gravity = false;
                                            Transform.ChangeVelocityY(dirToPlayer.Y * 3);
                                            if(ai[7] == 1 && ai[6] == 5){
                                                var proj = new Projectile(2, this);
                                                Core.SpawnProjectile(proj, Transform.Centre - new Vector2(0, Transform.Height), new Vector2(horiDirToPlayer / proj.Transform.Weight, -0.5f));
                                            }
                                        }
                                    }
                                }
                            }
                            else if(ai[0] == 2){
                                ai[1] += SSS.Delta;
                                int length = ai[7] == 1 ? 3 : 5;
                                if(ai[1] > 0.2f * ai[2]){
                                    ai[2]++;
                                    int type = ai[2] == 6 && ai[3] == length ? ai[4] != 0 || ai[7] == 1 ? 14 : 1 : 1;
                                    if(ai[3] == 3 && ai[1] <= 1 && ai[7] == 1){
                                        for(int i = 0; i < 9; i++){
                                            var initAng = MathF.Atan2(dirToPlayer.Y, dirToPlayer.X);
                                            initAng += MathHelper.ToRadians(40 - (10 * i));
                                            Core.SpawnProjectile(new Projectile(0, this), Transform.Centre, new Vector2(MathF.Cos(initAng), MathF.Sin(initAng)));
                                        }
                                    }
                                    else
                                        Core.SpawnProjectile(new Projectile(type, this), Transform.Centre, dirToPlayer * (ai[7] == 1 ? 3.5f : 1.5f));
                                }
                                if(ai[2] >= 6){
                                    ai[3]++;
                                    ai[2] = 0;
                                    ai[1] = -1.75f;
                                    if(ai[3] > length){
                                        ai[0] = ai[4] == 0 ? 3 : 5;
                                        ai[1] = 0;
                                        ai[2] = 0;
                                        ai[3] = 0;
                                        ai[4] = 0;
                                        ai[5] = 0;
                                        ai[6] = 0;
                                    }
                                    else
                                        ai[4] = Random.Shared.Next(0, 2);
                                }

                                Transform.Gravity = false;
                                var dir = Vector2.Normalize(Core._Player.Transform.Centre - new Vector2(100, 0) - Transform.Centre) * 3;
                                dir += new Vector2(MathF.Cos(ai[1] *  4), MathF.Sin(ai[1] *  4)) * 1.25f;
                                Transform.Velocity = Vector2.Lerp(Transform.Velocity, dir, 0.1f);
                            }
                            else if(ai[0] == 3){
                                Transform.Gravity = !(ai[2] <= 6);
                                if(ai[1] < 0.5f){
                                    if(ai[2] == 0)
                                        ai[2] = MathUtil.IntSign(Transform.Centre.Y - Core._Player.Transform.Centre.Y);

                                    Vector2 target = Core._Player.Transform.Centre + new Vector2(0, 80 * ai[2]);
                                    target = Vector2.Normalize(target - Transform.Position);
                                    Transform.Velocity = Vector2.Lerp(Transform.Velocity, target * Speed, 0.1f);

                                    if(DistToPlayer < 100){
                                        ai[1] += SSS.Delta;
                                        ai[2] = 0;
                                    }
                                }
                                else{
                                    if(ai[1] < 0.5f + SSS.Delta)
                                        AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                    ai[1] += SSS.Delta;
                                    PreRam = true;
                                }

                                if(ai[1] > 0.53f){
                                    ai[3] += SSS.Delta;
                                    Ramming = ai[2] <= 6;

                                    if(ai[2] <= 6 && ai[3] > 0.2f * (ai[2] + 1)){
                                        AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                        ai[2]++;
                                        Vector2 tp = ai[2] switch {
                                            2 => new Vector2(1, -1),
                                            3 => new Vector2(-1, 1),
                                            4 => new Vector2(1, 1),
                                            6 => new Vector2(1, 1),
                                            _ => new Vector2(-1, -1)
                                        };
                                        ai[4] = (tp.X * 80) - (Transform.Width / 2);
                                        ai[5] = (tp.Y * 25) - (Transform.Height / 2);
                                        ai[4] += Core._Player.Transform.Centre.X;
                                        ai[5] += Core._Player.Transform.Centre.Y + 4;
                                    }
                                    else if(ai[2] != 0)
                                        Transform.Position = Vector2.Lerp(Transform.Position, new Vector2(ai[4], ai[5]), 0.1f);

                                    if(ai[2] > 6 && ai[3] > 2f){
                                        ai[0] = 5;
                                        ai[1] = 0;
                                        ai[2] = 0;
                                        ai[3] = 0;
                                        ai[4] = 0;
                                        ai[5] = 0;
                                        ai[6] = 0;
                                    }
                                }
                            }
                            else if(ai[0] == 4){
                                Transform.Gravity = ai[1] < 1;
                                if(ai[1] == 0){
                                    if(Transform.Centre.Y > Core._Player.Transform.Centre.Y)
                                        Transform.ChangeVelocityY(-Speed * 0.7f);
                                    else if(MathF.Abs(DistToPlayer) < 120)
                                        ai[1] = SSS.Delta;
                                    float dif = Core._Player.Transform.Position.X - 100 - Transform.Position.X;
                                    Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, MathUtil.IntSign(dif) * Speed, 0.1f));
                                }
                                else{
                                    ai[1] += SSS.Delta;
                                    if(ai[1] > 1 && ai[1] < 1.4f){
                                        PreRam = true;
                                        if(ai[1] < 1 + SSS.Delta)
                                            AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                        ai[3] = MathF.Atan2(Transform.Centre.Y - Core._Player.Transform.Centre.Y, Transform.Centre.X - Core._Player.Transform.Centre.X);
                                    }
                                    else if(ai[1] > 1.4f){
                                        if(ai[1] <= 1.4f + SSS.Delta)
                                            AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                        Ramming = true;

                                        ai[3] += SSS.Delta * (ai[7] == 1 ? 4.6f : 3.4f);
                                        if(ai[3] >= MathF.PI * (ai[7] == 1 ? 4 : 2))
                                            ai[3] = 0;

                                        Vector2 end = Core._Player.Transform.Centre + new Vector2(MathF.Cos(ai[3]) * 100, -72 + (MathF.Sin(ai[3]) * 70));
                                        Transform.Position = Vector2.Lerp(Transform.Position, end, 0.1f);

                                        float av = MathF.PI * (ai[7] == 1 ? 2 : 0);
                                        if(DistToPlayer < 50 && ai[3] > 1.13f + av && ai[3] < 2 + av){
                                            ai[1] = -1;
                                            Transform.ChangeVelocityX(-Speed * 2);
                                            Core.SpawnProjectile(new Projectile(15, this), Transform.Centre, Vector2.Zero);
                                            if(ai[7] == 1){
                                                var proj = new Projectile(2, this);
                                                Core.SpawnProjectile(proj, Transform.Centre - new Vector2(0, Transform.Height), new Vector2(1 / proj.Transform.Weight, -0.5f));
                                                Core.SpawnProjectile(new Projectile(2, this), Transform.Centre - new Vector2(0, Transform.Height), new Vector2(-1 / proj.Transform.Weight, -0.5f));
                                            }
                                        }
                                    }
                                    else if(ai[1] < 0){
                                        Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, 0f, 0.1f));
                                        if(ai[1] > -0.5f){
                                            ai[0] = 1;
                                            ai[1] = 0;
                                            ai[2] = 0;
                                            ai[3] = 0;
                                            ai[4] = 0;
                                            ai[5] = 0;
                                            ai[6] = 0;

                                            for(int a = 0; a <= 360; a += 20){
                                                var proj = new Projectile(16, this);
                                                proj.ai[5] = MathHelper.ToRadians(a);
                                                proj.ai[0] = proj.ai[5];
                                                Core.SpawnProjectile(proj, Transform.Centre, Vector2.Zero);
                                            }
                                        }
                                    }
                                    else
                                        Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, 0, 0.1f));
                                }
                            }
                            else if(ai[0] == 5){
                                if(ai[1] == 0){
                                    if(DistToPlayer > 40)
                                        Transform.Velocity =  Vector2.Lerp(Transform.Velocity, dirToPlayer * 8, 0.05f);
                                    else
                                        ai[1] = 1;
                                }
                                else{
                                    Transform.Gravity = !Ramming;
                                    Ramming = MathF.Abs(Transform.Velocity.X) > Speed && ai[6] != 4;
                                    ai[3] = MathHelper.Lerp(ai[3], 0, 0.05f);
                                    Transform.ChangeVelocityX(ai[3]);

                                    ai[2] += SSS.Delta;
                                    if(ai[2] > 0.6f * ai[6] && ai[2] <= 0.6f * ai[6] + SSS.Delta && ai[6] < 4){
                                        ai[4] = 0.5f;
                                        ai[5] = ai[6] == 2 ? 1 : 0;
                                        ai[6]++;
                                    }

                                    if(ai[4] > 0){
                                        if(ai[4] == 0.5f)
                                            AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                        ai[4] -= SSS.Delta;
                                        PreRam = ai[4] > 0.35f;

                                        bool canRam = ai[4] <= 0.35f && ai[4] >= 0.35f - SSS.Delta;
                                        canRam = ai[5] == 1 ? (ai[4] <= 0.1f && ai[4] >= 0.1f - SSS.Delta) || canRam : canRam;
                                        if(canRam) {
                                            AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                            float distMult = DistToPlayer > 70 ? 1 : 0.5f;
                                            ai[3] = RamAmount * horiDirToPlayer * (ai[6] == 4 ? 2 : 1) * distMult;
                                            Transform.Gravity = ai[6] != 4;
                                            Transform.ChangeVelocityY(dirToPlayer.Y * (ai[6] == 4 ? 0 : 3));
                                        }
                                    }

                                    if(ai[6] >= 4 && ai[4] <= 0){
                                        ai[4] -= SSS.Delta;
                                        if(ai[4] < -1.5f){
                                            for(int a = 0; a <= 360; a += 40){
                                                var proj = new Projectile(16, this);
                                                proj.ai[5] = MathHelper.ToRadians(a);
                                                proj.ai[0] = proj.ai[5];
                                                Core.SpawnProjectile(proj, Transform.Centre, Vector2.Zero);
                                            }
                                            ai[0] = 4;
                                            ai[1] = 0;
                                            ai[2] = 0;
                                            ai[3] = 0;
                                            ai[4] = 0;
                                            ai[5] = 0;
                                            ai[6] = 0;
                                        }
                                    }
                                }
                            }
                            else if (ai[0] == 6) {
                                if(ai[1] == 0){
                                    ai[1] = 1;
                                    ST.StartScene(14);
                                }
                            }
                            else if (ai[0] == 7) { //@1 - Fly timer, @2 - Ram timer, @3 - Ram counter/phase
                                Ramming = MathF.Abs(Transform.Velocity.Length()) > Speed * 2;
                                PreRam = false;

                                if(ai[1] < 1.8f){
                                    Transform.Gravity = false;
                                    Vector2 endPos = Core._Player.Transform.Centre - new Vector2(80, 32);
                                    Vector2 endVel = Vector2.Normalize(endPos - Transform.Centre);
                                    Transform.Velocity = Vector2.Lerp(Transform.Velocity, endVel * Speed, 0.1f);
                                    if(Vector2.Distance(Transform.Centre, endPos) < 40){
                                        if(ai[1] == 0)
                                            AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                        PreRam = true;
                                        ai[1] += SSS.Delta;
                                    }
                                }
                                else{
                                    if(ai[2] == 0){
                                        AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                        Vector2 velTarget = ai[3] switch {
                                            1 => Core._Player.Transform.Centre + new Vector2(80, -32),
                                            2 => Core._Player.Transform.Centre - new Vector2(80, 32),
                                            _ => Core._Player.Transform.Centre
                                         };
                                        Transform.Velocity = Vector2.Normalize(velTarget - Transform.Centre) * RamAmount;
                                    }
                                    Transform.Velocity = Vector2.Lerp(Transform.Velocity, Vector2.Zero, 0.1f);
                                    ai[2] += SSS.Delta;

                                    if(ai[2] >= 0.4f){
                                        ai[2] = 0;
                                        ai[3]++;
                                        if(ai[3] >= 3){
                                            ai[3] = 0;
                                            ai[4]++;
                                            if(ai[4] >= 3){
                                                ai[0] = Random.Shared.Next(8, 11);
                                                ai[1] = 0;
                                                ai[2] = 0;
                                                ai[3] = 0;
                                                ai[4] = 0;
                                                ai[5] = 0;
                                                ai[6] = 0;
                                            }
                                        }
                                    }
                                }
                            }
                            else if (ai[0] == 8) {
                                Ramming = MathF.Abs(Transform.Velocity.Length()) > Speed * 2;
                                PreRam = false;
                                if(ai[6] == 0)
                                    ai[6] = Random.Shared.Next(0, 5);

                                if(ai[1] < 0.8f){
                                    Transform.Gravity = false;
                                    Vector2 endPos = Core._Player.Transform.Centre - new Vector2(80, 0);
                                    Vector2 endVel = Vector2.Normalize(endPos - Transform.Centre);
                                    Transform.Velocity = Vector2.Lerp(Transform.Velocity, endVel * Speed, 0.1f);
                                    if(Vector2.Distance(Transform.Centre, endPos) < 40){
                                        ai[1] += SSS.Delta;
                                    }
                                }
                                else{
                                    ai[2] += SSS.Delta;
                                    PreRam = false;
                                    Transform.Velocity = Vector2.Lerp(Transform.Velocity, Vector2.Zero, 0.1f);
                                    Ramming = MathF.Abs(Transform.Velocity.Length()) > Speed * 2;

                                    if((ai[2] >= 0.2f && ai[2] <= 0.2f + SSS.Delta) || (ai[3] == 1 && ai[2] >= 0.4f && ai[2] <= 0.4f + SSS.Delta)){
                                        if(ai[5] == 0){
                                            AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                            Transform.Velocity = RamAmount * new Vector2(horiDirToPlayer, dirToPlayer.Y * 0.25f);
                                        }
                                        else{
                                            Transform.Velocity = RamAmount * (-new Vector2(horiDirToPlayer, dirToPlayer.Y)  * 0.25f);
                                            var initAng = MathF.Atan2(dirToPlayer.Y, dirToPlayer.X);
                                            Core.SpawnProjectile(new Projectile(0, this), Transform.Centre, new Vector2(MathF.Cos(initAng), MathF.Sin(initAng)));
                                            Core.SpawnProjectile(new Projectile(0, this), Transform.Centre, new Vector2(MathF.Cos(initAng + 0.1f), MathF.Sin(initAng + 0.1f)));
                                            Core.SpawnProjectile(new Projectile(0, this), Transform.Centre, new Vector2(MathF.Cos(initAng - 0.1f), MathF.Sin(initAng - 0.1f)));
                                        }
                                    }

                                    if(ai[2] > (ai[3] == 1 ? 0.6f : 0.4f)){
                                        if(ai[4] >= 7 + ai[6]){
                                            int next = Random.Shared.Next(0, 3);
                                            ai[0] = next switch { 1 => 9, 2 => 10, _ => 7};
                                            ai[1] = 0;
                                            ai[2] = 0;
                                            ai[3] = 0;
                                            ai[4] = 0;
                                            ai[5] = 0;
                                            ai[6] = 0;
                                        }
                                        else{
                                            ai[2] = 0;
                                            ai[4]++;
                                            ai[3] = ai[4] == 1 || ai[4] == 3 || ai[4] == 7 ? 1 : 0;
                                            ai[5] = ai[4] == 2 || ai[4] == 4 || ai[4] == 7 ? 1 : 0;
                                        }
                                    }
                                }
                            }
                            else if (ai[0] == 9) {
                                Ramming = MathF.Abs(Transform.Velocity.Length()) > Speed * 2;
                                PreRam = false;
                                Transform.Gravity = ai[1] < 0.8f;

                                float d = Core._Player.Transform.Position.X - Transform.Centre.X;
                                if(ai[1] < 2)
                                    Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, MathUtil.IntSign(d) * Speed, 0.09f));

                                if(ai[1] < 0.8f){
                                    if(Grounded)
                                        Transform.ChangeVelocityY(-3);
                                    if(MathF.Abs(d) < 64 && Transform.Centre.Y < Core._Player.Transform.Centre.Y){
                                        ai[1] += SSS.Delta;
                                        if(ai[1] <= SSS.Delta)
                                            AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                        PreRam = true;
                                    }
                                }
                                else if(ai[1] < 2){
                                    Phantom = true;

                                    if(ai[2] == 0){
                                        int to = MathUtil.IntSign(Core._Player.Transform.Centre.Y - Transform.Centre.Y);
                                        Transform.ChangeVelocityY(to * RamAmount * 2);
                                        AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                    }

                                    ai[2] += SSS.Delta;
                                    Transform.ChangeVelocityY(MathHelper.Lerp(Transform.Velocity.Y, 0f, 0.1f));
                                    float am = ai[3] switch { 0 => 0.33f, 1 => 0.16f, 4 => 0.58f, 5 => 0.25f, 6 => 0.16f, _ => 0.41f };
                                    if(ai[2] >= am){
                                        ai[2] = 0;
                                        ai[3]++;
                                        if(ai[3] == 3 || ai[3] == 6){
                                            for(int i = 0; i < 360; i += 20){
                                                var proj = new Projectile(0, this);
                                                float a = MathHelper.ToRadians(i);
                                                Core.SpawnProjectile(proj, Transform.Centre, new Vector2(MathF.Cos(a), MathF.Sin(a)));
                                            }
                                        }
                                        if(ai[3] > 6){
                                            ai[1] = 2;
                                            ai[2] = 0;
                                            ai[3] = 0;
                                        }
                                    }
                                }
                                else if(ai[1] < 3.4f){
                                    Phantom = false;
                                    Vector2 targetInit = Core._Player.Transform.Centre + new Vector2(100, 0);
                                    Vector2 target = Vector2.Normalize(targetInit - Transform.Centre) * Speed;
                                    Transform.Velocity = Vector2.Lerp(Transform.Velocity, target, 0.1f);
                                    if(Vector2.Distance(Transform.Centre, targetInit) < 64)
                                        ai[1] += SSS.Delta;

                                    PreRam = ai[1] > 3f;
                                    if(ai[1] > 3f && ai[1] <= 3 + SSS.Delta)
                                        AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                }
                                else{
                                    if(ai[2] == 0 && ai[3] < 7){
                                        Transform.Velocity = dirToPlayer * RamAmount * 2;
                                        AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                    }

                                    Transform.Velocity = Vector2.Lerp(Transform.Velocity, Vector2.Zero, 0.1f);
                                    ai[2] += SSS.Delta;
                                    if(ai[2] > 0.175f){
                                        ai[2] = 0;
                                        ai[3]++;
                                        if(ai[3] >= 6){
                                            int next = Random.Shared.Next(0, 3);
                                            ai[0] = next switch { 1 => 8, 2 => 10, _ => 7};
                                            ai[1] = 0;
                                            ai[2] = 0;
                                            ai[3] = 0;
                                            ai[4] = 0;
                                            ai[5] = 0;
                                            ai[6] = 0;
                                            for(int a = 0; a <= 360; a += 40){
                                                var proj = new Projectile(16, this);
                                                proj.ai[5] = MathHelper.ToRadians(a);
                                                proj.ai[0] = proj.ai[5];
                                                Core.SpawnProjectile(proj, Transform.Centre, Vector2.Zero);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (ai[0] == 10) {
                                Transform.Gravity = false;
                                if(ai[1] < 0.5f){
                                    Vector2 targetInit = new Vector2(976, 1520);
                                    Vector2 target = Vector2.Normalize(targetInit - Transform.Centre) * Speed;

                                    if(Vector2.Distance(Transform.Centre, targetInit) < 32){
                                        ai[1] += SSS.Delta;
                                        target = Vector2.Zero;
                                        ai[2] = Health;
                                        if(ai[1] >= 0.5f){
                                            for(int i = 0; i < 320; i += 10){
                                                var proj = new Projectile(18, this);
                                                proj.ai[3] = MathHelper.ToRadians(i);
                                                proj.ai[5] = 120;
                                                proj.ai[6] = 1;
                                                Core.SpawnProjectile(proj, Vector2.Zero, Vector2.Zero);
                                            }
                                            for(int i = 0; i < 340; i += 6){
                                                var proj = new Projectile(18, this);
                                                proj.ai[3] = MathHelper.ToRadians(i);
                                                proj.ai[5] = 250;
                                                proj.ai[6] = -1;
                                                Core.SpawnProjectile(proj, Vector2.Zero, Vector2.Zero);
                                            }
                                        }
                                    }

                                    Transform.Velocity = Vector2.Lerp(Transform.Velocity, target, 0.1f);
                                }
                                else if(ai[1] < 2){
                                    Transform.Velocity = Vector2.Zero;
                                    if(Health != ai[2]){
                                        ai[1] = 2;
                                        ai[2] = 0;
                                        ai[3] = 1;
                                    }
                                }
                                else if(ai[1] < 5){
                                    ai[2] += SSS.Delta * ai[3];
                                    float realAng = ai[2] - MathF.PI / 2;
                                    Vector2 target = Core._Player.Transform.Centre + (new Vector2(MathF.Cos(realAng), MathF.Sin(realAng)) * 120);
                                    if(MathF.Abs(ai[2]) > MathF.PI / 2)
                                        ai[3] = -ai[3];

                                    target = Vector2.Normalize(target - Transform.Centre);
                                    Transform.Velocity = Vector2.Lerp(Transform.Velocity, target * Speed, 0.09f);

                                    ai[1] += SSS.Delta;
                                    if(ai[1] - 2 > 0.15f * ai[4]){
                                        ai[4]++;
                                        Core.SpawnProjectile(new Projectile(0, this), Transform.Centre, dirToPlayer * 2);
                                    }

                                    PreRam = ai[1] > 4.5f;
                                    if(ai[1] > 4.5f && ai[1] < 4.5f + SSS.Delta)
                                        AudioSystem.PlayEvent("dodgePre", true, Transform.Centre, true);
                                    if(ai[1] >= 5 - SSS.Delta){
                                        ai[2] = 0;
                                        ai[3] = 0;
                                        ai[4] = 0;
                                    }
                                }
                                else{
                                    ai[2] += SSS.Delta * ai[3];
                                    ai[3] += SSS.Delta * 2;
                                    if(ai[3] < 15){
                                        PreRam = true;
                                        Phantom = true;
                                        Vector2 target = Core._Player.Transform.Centre + (new Vector2(MathF.Cos(ai[2]), MathF.Sin(ai[2])) * 150);
                                        Transform.Velocity = Vector2.Zero;
                                        if(ai[3] < 0.5f)
                                            Transform.Position = Vector2.Lerp(Transform.Position, target, 0.1f);
                                        else
                                            Transform.Position = target;

                                        if(ai[3] >= 15 - (SSS.Delta * 2)){
                                            for(int a = 0; a <= 360; a += 40){
                                                var proj = new Projectile(16, this);
                                                proj.ai[5] = MathHelper.ToRadians(a);
                                                proj.ai[0] = proj.ai[5];
                                                Core.SpawnProjectile(proj, Transform.Centre, Vector2.Zero);
                                            }
                                        }
                                    }
                                    else{
                                        PreRam = false;
                                        Transform.Gravity = true;
                                        Ramming = MathF.Abs(Transform.Velocity.Length()) > Speed * 2;
                                        Transform.Velocity = Vector2.Lerp(Transform.Velocity, Vector2.Zero, 0.1f);
                                        if(ai[4] == 0){
                                            ai[4] = 1;
                                            Transform.Velocity = dirToPlayer * RamAmount * 3;
                                            AudioSystem.PlayEvent("dodge", true, Transform.Centre, true);
                                        }

                                        if(Grounded && ai[6] == 0){
                                            ai[6] = 1;
                                            for(int i = 0; i < 360; i += 20){
                                                var proj = new Projectile(0, this);
                                                float a = MathHelper.ToRadians(i);
                                                Core.SpawnProjectile(proj, Transform.Centre, new Vector2(MathF.Cos(a), MathF.Sin(a)));
                                            }
                                        }
                                        ai[5] += SSS.Delta;
                                        if(ai[5] > 2f){
                                            Transform.Gravity = true;
                                            Phantom = false;
                                            int next = Random.Shared.Next(0, 3);
                                            ai[0] = next switch { 1 => 9, 2 => 8, _ => 7};
                                            ai[1] = 0;
                                            ai[2] = 0;
                                            ai[3] = 0;
                                            ai[4] = 0;
                                            ai[5] = 0;
                                            ai[6] = 0;
                                        }
                                    }
                                }
                            }
                            break;
                            #endregion
                            #region Boss - Azaprota
                            case 35:
                            updateNodes = false;
                            if(ai[0] == 0){
                                if(ai[1] == 0){
                                    ai[1] = 1;
                                    Transform.ChangeVelocityY(-0.3f);

                                    int i = -1;
                                    foreach(var dummy in Core._NPCs.Where(x => x.ID == 36)){
                                        i++;
                                        var actual = Core.GetNPC(dummy.UUID.MainUUID);
                                        actual.ai[0] = 1;
                                        actual.ai[1] = UUID.MainUUID;
                                        actual.ai[2] = i;
                                    }
                                    Core._Player.BlockMovement = true;
                                    Core._Player.Transform.Gravity = false;
                                }
                                else if(ai[1] < 14){
                                    ai[1] += SSS.Delta;
                                    if(ai[1] > 11 && Transform.Velocity.Y < -0.01f)
                                        Transform.ChangeVelocityY(MathHelper.Lerp(Transform.Velocity.Y, 0f, 0.05f));
                                }
                                else{
                                    Core._Player.BlockMovement = false;
                                    Core._Player.Transform.Gravity = true;
                                    ai[0] = 1;
                                    ai[2] = 1;
                                    ai[1] = -1;
                                    foreach(var dummy in Core._NPCs.Where(x => x.ID == 36)){
                                        var actual = Core.GetNPC(dummy.UUID.MainUUID);
                                        actual.ai[3] = 1;
                                    }
                                    ScriptManager.ExecuteFunction("custom_azaprota", []);
                                }

                                Core._Player.Transform.ChangeVelocityY(Transform.Velocity.Y);
                            }
                            else if(ai[0] == 1){
                                if(ai[1] == -1)
                                    ai[1] = Health;
                                if(Health != ai[1]){
                                    foreach(var dummy in Core._NPCs.Where(x => x.ID == 36)){
                                        var actual = Core.GetNPC(dummy.UUID.MainUUID);
                                        actual.ai[3] = -actual.ai[3];
                                    }
                                    ai[1] = Health;
                                    ai[2] = -ai[2];
                                }
                                
                                Vector2 a = new Vector2(80 * ai[2], -50);
                                Vector2 target = Vector2.Normalize(Core._Player.Transform.Centre + a - Transform.Centre);
                                Transform.Velocity = Vector2.Lerp(Transform.Velocity, target * 2, 0.07f);
                            }
                            else{
                                Transform.Velocity = Vector2.Lerp(Transform.Velocity, Vector2.Zero, 0.05f);
                                ai[4] += SSS.Delta;
                                if(ai[4] >= 8){
                                    //COMEBACK
                                } 
                            }
                            break;
                            #endregion
                            #region Boss - Azaprota Floating Head
                            case 36:
                            updateNodes = false;
                            if(ai[0] == 1){ //$1 - Sender UUID, $2 - Circle starting index, $3 - Motion
                                ai[4] += SSS.Delta * ai[3];
                                var am = (MathF.PI * 2 / 7 * ai[2]) + ai[4];
                                Vector2 target = Core.GetNPC((int)ai[1]).Transform.Centre + (new Vector2(MathF.Cos(am), MathF.Sin(am)) * 150);
                                Vector2 targetVel = Vector2.Normalize(target - Transform.Centre);
                                if(Vector2.Distance(Transform.Centre, target) < 10)
                                    targetVel = Vector2.Zero;
                                Transform.Velocity = targetVel * 3;
                            }
                            break;
                            #endregion
                        }
                    }

                    else if (!stuned)
                    {
                        Transform.Gravity = gravity;
                        if (NoticeDistance > 0 && !Boss)
                            Noticed = CanSeePlayer && DistToPlayer < NoticeDistance;
                        else if (!Boss)
                            Noticed = CanSeePlayer;
                        if (wanderAmount != 0)
                        {
                            if (MathUtil.Diff(Transform.Position.X, initPos.X) >= MathF.Abs(wanderAmount))
                                wanderDir = -1;
                            if (MathUtil.Diff(Transform.Position.X, initPos.X) <= 0.1f)
                                wanderDir = 1;
                            Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, Speed / 2 * MathUtil.IntSign(wanderAmount) * wanderDir, 0.1f));
                        }
                    }
                }

                if (Corpse || (!CantStun && stunTimer > 0))
                {
                    Ramming = false;
                    PreRam = false;
                    Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, 0, 0.1f));
                }

                //dont put any code that calculates with Grounded as it's changed by AI code.
                Grounded = false;
                isTouchingWall = false;
                isTouchingCeiling = false;
                Transform.Velocity += new Vector2(knockbackVel, 0f);
                Transform.Velocity = CollisionEngine.UpdateVelocity(Transform, SSS.Delta);
                CanSeePlayer = true;
                Ray2D visRay = new Ray2D(Transform.Centre, dirToPlayer);
                foreach (Collider col in Core._Colliders)
                {
                    var inter = visRay.Intersects(col.GetRectangle());
                    if (inter != null && inter < DistToPlayer)
                        CanSeePlayer = false;

                    if (col.Tags != null && col.Tags.Contains("ex_player_seeker") && ID == 5)
                        continue;

                    bool wall1 = col.Type != ColliderType.Slope && Transform.Position.Y + Transform.Height > col.GetRectangle().Top && Transform.Position.Y < col.GetRectangle().Bottom;
                    if (wall1 && (Transform.Position.X == col.X + col.Width || Transform.Position.X + Transform.Width == col.X))
                    {
                        isTouchingWall = true;
                        lastTouchedWallDir = col.X + (col.Width / 2) > Transform.Centre.X ? 1 : -1;
                    }

                    if (!Phantom)
                    {
                        if (CollisionEngine.CollideAll(col, Transform, out Vector2 oPos, out Vector2 oVel))
                        {
                            Transform.Position = oPos;
                            Transform.Velocity = oVel;
                            if (oVel.X == 0)
                                knockbackVel = 0f;
                            if (wall1)
                            {
                                isTouchingWall = true;
                                lastTouchedWallDir = col.X + (col.Width / 2) > Transform.Centre.X ? 1 : -1;
                            }
                            if (col.GetRectangle().Top >= Transform.Position.Y + Transform.Height)
                                Grounded = true;
                            if (col.GetRectangle().Bottom <= Transform.Position.Y)
                                isTouchingCeiling = true;
                        }
                    }
                }

                if(exploded && Grounded && Corpse)
                {
                    exploded = false;
                    var proj = new Projectile(2, this);
                    proj.Deflect();
                    Vector2 dir = Vector2.Normalize(Core.GetClosestNPC(Transform.Position, false, out float _).Transform.Position - Transform.Position);
                    dir *= new Vector2(0.75f, 1.4f);
                    Core.SpawnProjectile(proj, Transform.Centre, dir);
                }

                Transform.ApplyVelocity();
                Transform.Velocity -= new Vector2(knockbackVel, 0f);
            }
        }

        public void Draw(Renderer renderer)
        {
            if (!StopDraw)
            {
                Color color = Color.White;
                switch (ai[0])
                {
                    case 0:
                        color = Color.Red;
                        break;
                    case 1:
                        color = Color.Green;
                        break;
                    case 2:
                        color = Color.Blue;
                        break;
                    case 3:
                        color = Color.Yellow;
                        break;
                    case 4:
                        color = Color.Cyan;
                        break;
                }
                color = Ramming ? Color.Magenta : color;
                color = Corpse ? Color.Gray : color;
                color = !Noticed ? Color.White : color;

                Effect effect = null;
                if(PType != PhantomType.None && !Corpse)
                {
                    effect = AssetManager.Effects["phantom"];
                    var pCol = PType == PhantomType.Red ? new Vector3(0.941f, 0.164f, 0.227f) : new Vector3(0.227f, 0.164f, 0.941f);
                    effect.Parameters["phantomColor"]?.SetValue(pCol);
                }
                renderer.BasicDraw(SSS.Square, Transform.GetRectangle(), 1, 1, col: color, effect: effect);

                renderer.DrawText(Debug.DebugFont, ai[0].ToString(), Transform.Position - new Vector2(0, 10), 1, 1);
                if (PreRam && !Corpse && stunTimer <= 0)
                {
                    Texture2D tex = AssetManager.GetAsset("ui_dashInd");
                    Vector2 vec = Transform.Centre.X < Core._Player.Transform.Centre.X ? new Vector2(1, 0) : new Vector2(-1, 0);
                    if (Core._Player.Transform.Centre.X < Transform.Position.X + Transform.Width && Core._Player.Transform.Centre.X > Transform.Position.X)
                        vec = Transform.Centre.Y < Core._Player.Transform.Centre.Y ? new Vector2(0, 1) : new Vector2(0, -1);
                    vec *= new Vector2(Transform.Width / 2, Transform.Height / 2);
                    renderer.BasicDraw(tex, Transform.Centre + vec - new Vector2(tex.Width / 2, tex.Height / 2), 1, 1);
                }
                if(Hitboxes.Count > 1)
                {
                    foreach(Rectangle hb in Hitboxes.Values.ToArray())
                    {
                        renderer.BasicDraw(SSS.Square, new Rectangle(hb.X + (int)Transform.Position.X, hb.Y + (int)Transform.Position.Y, hb.Width, hb.Height), 1, 1, col: Color.Crimson);
                    }
                }
            }
        }

        public void Kill()
        {
            if (!Invincible)
            {
                Health = 0;
                if (Corpseable)
                {
                    Corpse = true;
                    Transform.Friction = true;
                    if (Pack != -1 && packLeader)
                        DisbandPack(UUID);
                    if (!Transform.Gravity && !Phantom)
                        Transform.Gravity = true;
                    int am = Transform.Width * Transform.Height / (Transform.Width + Transform.Height) / 2;
                    Core._Player.externalVelocity = Vector2.Zero;
                    ParticleManager.AddParticles(0, Transform.Centre, -Transform.Velocity / 8, 1f, am, 4, 4);
                }
                else
                    Decay();
                Core._Player.Health = Math.Clamp(Core._Player.Health + 1, 0, Core._Player.MaxHealth);
                Core._Player.AddSelfBoost(1);
                AudioSystem.PlayEvent("kill", true, Transform.Centre, true);

                if (entName != null)
                {
                    ScriptManager.ExecuteFunction("npc_" + entName, new object[0]);
                }
                if (PType == PhantomType.Red)
                    PhantomManager.AddDeath();

                if (Boss)
                    PhantomManager.TickStats(0);

                if (permaID != -1)
                    GameStateManager.States[permaID] = permaSlotValue;

                if(ID == 31){ //JESERSANT
                    foreach(NPC little in Core._NPCs.Where(x => x.ID == 32).ToList()){
                        little.Invincible = false;
                        little.Kill();
                    }
                    if(ai[0] == 4){
                        foreach(NPC little in Core._NPCs.Where(x => x.ID == 33).ToList())
                            little.Kill();
                    }
                }
            }
        }

        public void Decay()
        {
            if (Corpse)
            {
                Core._NPCs.Remove(this);
                Freeze = true;
            }
            else if (!Corpseable)
            {
                int am = Transform.Width * Transform.Height / (Transform.Width + Transform.Height) / 2;
                ParticleManager.AddParticles(0, Transform.Centre, -Transform.Velocity / 8, 1f, am, 4, 4);
                Core._NPCs.Remove(this);
            }
        }

        public void Knockback()
        {
            if (!CantKnockback)
            {
                Vector2 dir = Vector2.Normalize(Transform.Position - Core._Player.Transform.Position);
                float amount = Core._Player.JetBoosted ? Core._Player.JetBoostFling : Core._Player.RamAmount;
                knockbackVel = dir.X * amount * 2;
                float vertAm = Core.HasUpgradeOn(Upgrade.Angers_Display) ? -3.3f : 1;
                Transform.Velocity = new Vector2(Transform.Velocity.X, dir.Y * vertAm);
                stunTimer = stunTimeMult;
                Ramming = false;
            }
        }

        private static bool JoinPack(NPC target)
        {
            List<int> preCon = new List<int>();
            foreach (NPC npc in Core._NPCs.Where(x => x.ID == target.ID && !x.Corpse))
            {
                if (Vector2.Distance(npc.Transform.Centre, target.Transform.Centre) < target.PackDist)
                    preCon.Add(npc.UUID.MainUUID);
            }

            int[] connections = preCon.ToArray();
            if (connections.Length > 0 && !Packs.Contains(connections) && connections.Length <= target.MaxPackCount)
            {
                bool didSimilar = false;
                for (int i = 0; i < Packs.Count; i++)
                {
                    if (Packs[i].Intersect(connections).Any())
                    {
                        Packs[i] = Packs[i].Union(connections).ToArray();
                        didSimilar = true;
                    }
                }
                if (!didSimilar)
                    Packs.Add(connections);
                return true;
            }
            else
                return false;
        }

        public static int GetPack(UUID id, out bool leader, out int packID)
        {
            leader = false;
            packID = -1;
            for (int i = 0; i < Packs.Count; i++)
            {
                if (Packs[i].Contains(id.MainUUID))
                {
                    leader = id.MainUUID == Packs[i].First();
                    packID = Array.IndexOf(Packs[i], id.MainUUID);
                    return i;
                }
            }
            return -1;
        }

        public static bool SimplePackCheck(UUID id)
        {
            return Packs.Where(x => x.Contains(id.MainUUID)).Any();
        }

        public static void DisbandPack(UUID id)
        {
            for(int i = 0; i < Packs.Count; i++)
            {
                if (Packs[i].Contains(id.MainUUID))
                    Packs.Remove(Packs[i]);
            }
        }

        public void ChangeAIState(int index, float state)
        {
            ai[Math.Clamp(index, 0, ai.Length - 1)] = state;
        }

        public float GetAIState(int index)
        {
            return ai[Math.Clamp(index, 0, ai.Length - 1)];
        }

        private Rectangle MakeHBSide(Rectangle zhb, int side = 0)
        {
            if (side == 0)
                side = Core._Player.Transform.Centre.X < Transform.Centre.X ? -1 : 1;
            if (side < 0)
                zhb.Location = new Point(zhb.X - zhb.Width, zhb.Y);
            else
                zhb.Location = new Point(zhb.X + Transform.Width, zhb.Y);
            return zhb;
        }

        private bool AlignWithPlayer(float offset, bool vert)
        {
            int tcd = vert ? Transform.Width : Transform.Height;
            int pcd = vert ? Core._Player.Transform.Width : Core._Player.Transform.Height;
            if (tcd < pcd)
            {
                if (vert)
                {
                    return Transform.Position.X + Transform.Width <= Core._Player.Transform.Position.X + Core._Player.Transform.Width + offset
                        && Transform.Position.X >= Core._Player.Transform.Position.X - offset;
                }
                else
                {
                    return Transform.Position.Y + Transform.Height <= Core._Player.Transform.Position.Y + Core._Player.Transform.Height + offset
                        && Transform.Position.Y >= Core._Player.Transform.Position.Y - offset;
                }
            }
            else
            {
                if (vert)
                {
                    return Core._Player.Transform.Position.X >= Transform.Position.X - offset
                        && Core._Player.Transform.Position.X + Core._Player.Transform.Width <= Transform.Position.X + Transform.Width + offset;
                }
                else
                {
                    return Core._Player.Transform.Position.Y >= Transform.Position.Y - offset
                        && Core._Player.Transform.Position.Y + Core._Player.Transform.Height <= Transform.Position.Y + Transform.Height + offset;
                }
            }
        }
    }

    public enum PhantomType
    {
        None,
        Red,
        Blue
    }
}

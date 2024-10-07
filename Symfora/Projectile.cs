using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TripleS;
using TripleS.Physics;

namespace Symfora {
    public class Projectile {

        public Transform Transform { get; set; }
        public UUID UUID { get; private set; }
        public int ID { get; private set; }
        public float Speed { get; private set; }
        public float Life { get; private set; }
        public float MaxLife { get; private set; }
        public bool Phantom { get; private set; }
        public bool Freeze { get; set; }
        public bool HurtPlayer { get; private set; }
        public bool Peirce { get; set; }
        public bool Deflected { get; private set; }
        public bool Undeflectable { get; private set; }
        public bool Gravity { get; private set; }
        private Transform Target { get; set; }

        private NPC sender;
        public float[] ai;
        private Vector2 initVelocity;
        private bool collisionFlag;
        private bool solid;

        /// <param name="sender">Leave as null for player.</param>
        public Projectile(int id, NPC npcSender)
        {
            UUID = new UUID(2);
            ID = id;
            Vector2 dimensions = Vector2.One;
            float weight = 1;
            switch (ID)
            {
                case 0:
                    dimensions = new Vector2(4, 4);
                    weight = 1;
                    Speed = 3f;
                    Phantom = false;
                    MaxLife = 20;
                    HurtPlayer = true;
                    break;
                case 1:
                    dimensions = new Vector2(4, 4);
                    weight = 1;
                    Speed = 2.6f;
                    Phantom = false;
                    MaxLife = 20;
                    HurtPlayer = true;
                    break;
                case 2:
                    dimensions = new Vector2(10, 10);
                    weight = 10;
                    Speed = 5;
                    Phantom = false;
                    MaxLife = 20;
                    HurtPlayer = true;
                    //Undeflectable = true;
                    Gravity = true;
                    break;
                case 3:
                    dimensions = new Vector2(60, 6);
                    weight = 1;
                    Speed = 0;
                    Phantom = true;
                    MaxLife = 5;
                    HurtPlayer = true;
                    Undeflectable = true;
                    Gravity = false;
                    break;
                case 4:
                    dimensions = new Vector2(9, 9);
                    weight = 1;
                    Speed = 1;
                    Phantom = true;
                    MaxLife = 5;
                    HurtPlayer = true;
                    Undeflectable = true;
                    Gravity = false;
                    break;
                case 5:
                    dimensions = new Vector2(9, 9);
                    weight = 5;
                    Speed = 1;
                    Phantom = false;
                    MaxLife = 20;
                    HurtPlayer = true;
                    Undeflectable = true;
                    Gravity = true;
                    solid = true;
                    break;
                case 6:
                    dimensions = new Vector2(4, 4);
                    weight = 1;
                    Speed = 6f;
                    Phantom = false;
                    MaxLife = 20;
                    HurtPlayer = false;
                    break;
                case 7:
                    dimensions = new Vector2(4, 4);
                    weight = 1;
                    Speed = 3f;
                    Phantom = false;
                    MaxLife = 20;
                    HurtPlayer = true;
                    break;
                case 8:
                    dimensions = new Vector2(4, 4);
                    weight = 1;
                    Speed = 1;
                    Phantom = true;
                    MaxLife = 20;
                    HurtPlayer = false;
                    Undeflectable = true;
                    Gravity = false;
                    break;
                case 9:
                    dimensions = new Vector2(10, 10);
                    weight = 7;
                    Speed = 3.5f;
                    Phantom = false;
                    MaxLife = 20;
                    HurtPlayer = true;
                    Undeflectable = true;
                    Gravity = true;
                    break;
                case 10:
                    dimensions = new Vector2(128, 10);
                    weight = 1;
                    Speed = 0;
                    Phantom = true;
                    MaxLife = 9;
                    HurtPlayer = true;
                    Undeflectable = true;
                    Gravity = false;
                    break;
                case 11:
                    dimensions = new Vector2(13, 13);
                    weight = 10;
                    Speed = 1;
                    Phantom = false;
                    MaxLife = 20;
                    HurtPlayer = true;
                    Undeflectable = true;
                    Gravity = true;
                    break;
                case 12:
                    dimensions = new Vector2(18, 18);
                    weight = 10;
                    Speed = 2;
                    Phantom = false;
                    MaxLife = 20;
                    HurtPlayer = true;
                    Undeflectable = true;
                    Gravity = true;
                    break;
                case 13:
                    dimensions = new Vector2(4, 4);
                    weight = 1;
                    Speed = 1;
                    Phantom = true;
                    MaxLife = -1;
                    HurtPlayer = true;
                    Undeflectable = true;
                    Gravity = false;
                    break;
                case 14:
                    dimensions = new Vector2(4, 4);
                    weight = 1;
                    Speed = 3f;
                    Phantom = false;
                    MaxLife = 20;
                    Undeflectable = true;
                    HurtPlayer = true;
                    break;
                case 15:
                    dimensions = new Vector2(50, 50);
                    weight = 1;
                    Phantom = true;
                    Gravity = false;
                    Undeflectable = true;
                    MaxLife = 1;
                    HurtPlayer = true;
                    break;
                case 16:
                    dimensions = new Vector2(4, 4);
                    weight = 1;
                    Speed = 1;
                    Phantom = true;
                    MaxLife = 10;
                    HurtPlayer = true;
                    Undeflectable = true;
                    Gravity = false;
                    break;
                case 17:
                    dimensions = new Vector2(16, 16);
                    weight = 1;
                    Speed = 1;
                    Phantom = true;
                    MaxLife = -1;
                    HurtPlayer = true;
                    Undeflectable = true;
                    Gravity = false;
                    Peirce = true;
                    break;
                case 18:
                    dimensions = new Vector2(4, 4);
                    weight = 1;
                    Speed = 1;
                    Phantom = true;
                    MaxLife = -1;
                    HurtPlayer = true;
                    Undeflectable = true;
                    Gravity = false;
                    break;
            }
            Transform = new Transform(Vector2.Zero, (int)dimensions.X, (int)dimensions.Y, weight, false, Gravity);
            ai = new float[8];
            sender = npcSender;
        }

        public void Spawn(Vector2 position, Vector2 velocity)
        {
            Transform.Position = position;
            Core._Projectiles.Add(this);
            Transform.ResetOldPosition();
            initVelocity = velocity * Speed;
            Transform.Velocity = initVelocity;
            Life = MaxLife;
        }

        public void Update()
        {
            if (!Freeze)
            {
                if (sender != null)
                    Target = Deflected ? sender.Transform : Core._Player.Transform;

                if (Life <= 0 && Life != -1)
                    Kill();
                else if(Life != -1)
                    Life -= SSS.Delta;

                switch (ID)
                {
                    case 1:
                        if (!Deflected && ai[0] < 0.9f) {
                            Vector2 dir = Vector2.Normalize(Target.Centre - Transform.Centre);
                            Transform.Velocity = Vector2.Lerp(Transform.Velocity, dir * Speed, 0.1f);
                            ai[0] += SSS.Delta;
                        }
                        break;
                    case 2:
                        float dir1 = Target.Centre.X - Transform.Centre.X;
                        dir1 /= Transform.Weight;
                        Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, dir1, 0.02f));
                        break;
                    case 4:
                        if (ai[0] > MathF.PI * 2)
                            ai[0] = 0;
                        ai[0] += SSS.Delta * 10;
                        Vector2 dir2 = Vector2.Normalize(Target.Centre - Transform.Centre);
                        Transform.Velocity = dir2 + new Vector2(MathF.Sin(ai[0]), MathF.Cos(ai[0])) / 2;
                        break;
                    case 7:
                        if (ai[0] < 0.9f)
                        {
                            Vector2 dir = Vector2.Normalize(Target.Centre - Transform.Centre);
                            Transform.Velocity = Vector2.Lerp(Transform.Velocity, dir * Speed, 0.1f);
                            ai[0] += SSS.Delta;
                        }
                        break;
                    case 8:
                        if (ai[0] <= 1f)
                        {
                            ai[0] += SSS.Delta;
                            if (ai[0] >= 1 - SSS.Delta) 
                            {
                                var rand = new Random();
                                ai[1] = rand.Next(0, 50);
                                ai[3] = sender.Health;
                            }
                        }
                        else
                        {
                            HurtPlayer = true;
                            ai[2] += SSS.Delta * 6 * (1 + ai[1] / 100);
                            if (ai[2] >= MathF.PI * 2)
                                ai[2] = 0;
                            Transform.Velocity = new Vector2(MathF.Cos(ai[2] + ai[1]), MathF.Sin(ai[2] + ai[1])) * 0.2f;
                            if (sender.Health != ai[3])
                                Kill();
                        }
                        break;
                    case 9:
                        if(ai[0] == 0){
                            ai[0] = MathUtil.IntSign(Transform.Velocity.X);
                            ai[2] = Transform.Velocity.X;
                            Transform.ChangeVelocityX(ai[2] / 8);
                        }
                        
                        if(Transform.Velocity.Y > 0)
                        {
                            if(ai[2] != 0){
                                Transform.ChangeVelocityX(ai[2]);
                                ai[2] = 0;
                            }

                            Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, 0.35f * ai[0], 0.1f));
                            if(ai[1] > MathF.PI * 2)
                                ai[1] = 0;
                            ai[1] += SSS.Delta * 4;
                            Transform.ChangeVelocityX(Transform.Velocity.X + (MathF.Sin(ai[1]) * ai[0] * 0.1f));
                        }
                        break;
                    case 12:
                        var tdir = Vector2.Normalize(Target.Centre - Transform.Centre).X;
                        Transform.ChangeVelocityX(MathHelper.Lerp(Transform.Velocity.X, tdir * 3, 0.05f));
                        break;
                    case 13:
                        if(ai[0] >= (MathF.PI * 2) + ai[5])
                            ai[0] = ai[5];
                        ai[0] += SSS.Delta;
                        ai[4] += SSS.Delta * 3;
                        
                        Transform.Position -= new Vector2(ai[2], ai[3]);
                        var ttdir = MathUtil.SafeNomralize(sender.Transform.Centre - Transform.Position);
                        ttdir *= MathF.Sin(ai[4] + 1) / 2;
                        ai[2] += ttdir.X;
                        ai[3] += ttdir.Y;
                        Transform.Position = sender.Transform.Centre + (new Vector2(MathF.Cos(ai[0]), MathF.Sin(ai[0])) * 80);
                        Transform.Position += new Vector2(ai[2], ai[3]);

                        if(ai[1] == 0)
                            ai[1] = sender.Health;
                        if(sender.Health != ai[1])
                            Kill();
                        break;
                    case 14:
                        if (ai[0] < 0.9f) {
                            Vector2 dir = Vector2.Normalize(Target.Centre - Transform.Centre);
                            Transform.Velocity = Vector2.Lerp(Transform.Velocity, dir * Speed, 0.1f);
                            ai[0] += SSS.Delta;
                        }
                        break;
                    case 16:
                        if(ai[1] == 0){
                            ai[1] = sender.Transform.Centre.X;
                            ai[6] = sender.Transform.Centre.Y;
                        }
                        if(ai[0] >= (MathF.PI * 2) + ai[5])
                            ai[0] = ai[5];
                        ai[0] += SSS.Delta;
                        ai[4] += SSS.Delta * 3;
                        
                        Transform.Position -= new Vector2(ai[2], ai[3]);
                        var ttdir2 = MathUtil.SafeNomralize(new Vector2(ai[1], ai[6]) - Transform.Position);
                        ttdir2 *= MathF.Sin(ai[4] + 1) / 2;
                        ai[2] += ttdir2.X;
                        ai[3] += ttdir2.Y;
                        Transform.Position = new Vector2(ai[1], ai[6]) + (new Vector2(MathF.Cos(ai[0]), MathF.Sin(ai[0])) * 80);
                        Transform.Position += new Vector2(ai[2], ai[3]);
                        break;
                    case 17:
                        ai[0] += SSS.Delta;
                        if(ai[0] > 16)
                            ai[0] = 0;
                        
                        int side = ai[0] > 8 ? 1 : -1;
                        Vector2 targetPos = Core._Player.Transform.Centre - (new Vector2(70, 30) * side);
                        Transform.Position = Vector2.Lerp(Transform.Position, targetPos, 0.03f);

                        if(sender.GetAIState(7) != 1)
                            Kill();
                        break;
                    case 18:
                        if(ai[1] == 0){
                            ai[1] = sender.Transform.Centre.X;
                            ai[2] = sender.Transform.Centre.Y;
                            ai[4] = sender.Health;
                        }
                        if(ai[0] >= (MathF.PI * 2))
                            ai[0] = 0;
                        ai[0] += SSS.Delta * ai[6];

                        Transform.Position = new Vector2(ai[1], ai[2]) + (new Vector2(MathF.Cos(ai[0] + ai[3]), MathF.Sin(ai[0] + ai[3])) * ai[5]);
                        if(sender.Health != ai[4])
                            Kill();
                        break;
                }

                bool sharpDUpgrade = Core.HasUpgradeOn(Upgrade.Sharp_Deflection);
                if (!Phantom)
                {
                    Transform.Velocity = CollisionEngine.UpdateVelocity(Transform, SSS.Delta);
                    foreach (Collider col in Core._Colliders.ToArray())
                    {
                        if (CollisionEngine.CollideAll(col, Transform, out Vector2 oPos, out Vector2 oVel))
                        {
                            if (!solid)
                            {
                                Kill();
                                if(ID == 11 && sender != null)
                                    Core.SpawnProjectile(new Projectile(10, null), Transform.Position + new Vector2(-Transform.Width / 2, Transform.Height), Vector2.Zero);

                                if (sharpDUpgrade || ID == 0)
                                {
                                    foreach (EntBlockade eblo in Core._LevelHandler.EntityMan.Entities.Where(x => x.GetType() == typeof(EntBlockade)))
                                    {
                                        if (eblo.Collider.Equals(col))
                                            eblo.Destroy();
                                    }
                                }
                            }
                            else 
                            {
                                Transform.Velocity = oVel;
                                Transform.Position = oPos;
                            }
                        }
                    }
                }

                if (CollisionEngine.RectInRect(Transform.GetRectangle(), Core._Player.ProjCataBounds) && !Deflected)
                    Core._Player.CanProjCata = true;

                if (HurtPlayer || ID == 5)
                {
                    if (CollisionEngine.RectInRect(Transform.GetRectangle(), CollisionEngine.InflateRect(Core._Player.Transform.GetRectangle(), 3)))
                    {
                        if (!collisionFlag)
                        {
                            collisionFlag = true;
                            if (Core._Player.Ramming && !Deflected && !Undeflectable)
                            {
                                Deflect();
                            }
                            else
                            {
                                if (ID != 5)
                                {
                                    Core._Player.Health -= 1;
                                    AudioSystem.PlayEvent("dodgeCollide", true, Transform.Centre, true);
                                }
                                else
                                {
                                    AudioSystem.PlayEvent("kill", true, Transform.Centre, true);
                                    Core._Player.Health = Core._Player.MaxHealth;
                                }
                                if (!Peirce)
                                    Kill();
                            }
                        }
                    }
                    else if (collisionFlag)
                        collisionFlag = false;
                }
                else
                {
                    NPC closest = Core.GetClosestNPC(Transform.Centre, false, out float _);
                    if (closest != null && CollisionEngine.RectInRect(Transform.GetRectangle(), closest.Transform.GetRectangle()) && !closest.Corpse)
                    {
                        if (!collisionFlag)
                        {
                            collisionFlag = true;
                            if (closest.Ramming && closest.CanDeflect && Deflected && !Undeflectable)
                            {
                                Deflect();
                            }
                            else
                            {
                                closest.Health -= sharpDUpgrade ? 2 : 1;
                                AudioSystem.PlayEvent("dodgeCollide", true, Transform.Centre, true, pitch: 1.4f);
                                if (!Peirce)
                                    Kill();
                            }
                        }
                    }
                    else if (collisionFlag)
                        collisionFlag = false;
                }

                Transform.ApplyVelocity();
            }
        }

        public void Draw(Renderer renderer)
        {
            float rot = MathF.Atan2(Transform.Velocity.Y, Transform.Velocity.X);
            renderer.BasicDraw(SSS.Square, new Rectangle((int)Transform.Position.X, (int)Transform.Position.Y, Transform.Width, Transform.Height), 1, 1, rot, col: Undeflectable ? Color.Maroon : Color.White);   
        }

        public void Kill()
        {
            ParticleManager.AddParticles(1, Transform.Centre, new Vector2(-Transform.Velocity.X, MathF.Abs(Transform.Velocity.Y)) / 8, 1f, 2, 4, 4);
            Core._Projectiles.Remove(this);
            if(ID == 14)
                Core.SpawnProjectile(new Projectile(15, null), Transform.Centre - new Vector2(25, 25), Vector2.Zero);
        }

        public void Deflect()
        {
            Deflected = true;
            HurtPlayer = !HurtPlayer;
            collisionFlag = false;
            Vector2 dir = Vector2.Normalize((sender == null ? Transform.Centre : sender.Transform.Centre) - Core._Player.Transform.Centre);
            if (HurtPlayer)
                dir = Vector2.Normalize(Core._Player.Transform.Centre - Transform.Centre);
            Transform.Velocity = dir * Speed;
            AudioSystem.PlayEvent("deflect", true, Transform.Centre, true);

            if (Core.HasUpgradeOn(Upgrade.Double_Deflection))
            {
                Core.SpawnProjectile(new Projectile(ID, sender), Transform.Position, dir * -1);
            }
        }
    }
}

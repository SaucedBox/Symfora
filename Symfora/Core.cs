using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TripleS;
using TripleS.Lighting;
using TripleS.Physics;
using TripleS.Scripting;
using TripleS.UI;

namespace Symfora {
    public class Core : Game, ITripleSGame {

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        public static Renderer _Renderer { get; set; }
        public static LevelHandler _LevelHandler { get; set; }
        public static List<Collider> _Colliders { get; set; }
        public static Player _Player { get; set; }
        public static List<NPC> _NPCs { get; set; }
        public static List<int> _Settings { get; set; }
        public static List<Projectile> _Projectiles { get; private set; }
        public static SkyboxManager _Skybox { get; private set; }
        private static float DeltaTime { get; set; }
        public static bool Paused { get; set; }
        public static bool FirstLevelLoad { get; set; }
        public static bool Dead { get; private set; }
        public static int Progress { get; private set; }
        //public static bool InBossFight { get { return _NPCs.Any(x => x.Boss && x.Noticed && !x.Corpse); } }

        public static Vector2 CamTarget { get; set; }
        private string[] bootArgs;
        private static bool loadDebugLevel;
        private static bool doneFirstLoad;

        public Core(string[] args)
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 400;
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 400;
            Window.TextInput += GameInputs.TextInputHand;
            bootArgs = args;
        }

        protected override void Initialize()
        {           
            SSS.Init(this, "Symfora", 3, bootArgs);
            if (!SSS.PreviewMode)
            {
                AudioSystem.Initialize();
                InitOrganize();
            }

            LocalizationManager.Initialize(Languages.English);

            var entTypes = new Type[24] {
                typeof(EntCatalyst),
                typeof(EntJetCatalyst),
                typeof(EntDoor),
                typeof(EntCataCharger),
                typeof(EntBlockade),
                typeof(EntTimer),
                typeof(EntLadder),
                typeof(EntWire),
                typeof(EntNPC),
                typeof(EntMagnet),
                typeof(EntBallista),
                typeof(EntSpiralDoor),
                typeof(EntHouseDoor),
                typeof(EntSwitch),
                typeof(EntTrigger),
                typeof(EntTransition),
                typeof(EntSoanoHead),
                typeof(EntHubItem),
                typeof(EntCollider),
                typeof(EntRope),
                typeof(EntProp),
                typeof(EntTriggerInput),
                typeof(EntAdvancedScene),
                typeof(EntSinkpit)
            };
            _LevelHandler = new LevelHandler("lev", "til", "script", 1, entTypes);
            _Skybox = new SkyboxManager();

            _Player = new Player();

            if (SSS.FirstLoad)
            {
                for (int i = 0; i < Enum.GetNames<Upgrade>().Length; i++)
                {
                    GameStateManager.States[48 + i] = -1;
                    if (i == 1 || i == 2)
                        GameStateManager.States[48 + i] = 0;
                }
                GameStateManager.States[66] = 3;
            }

            SetSettings(true);
            PhantomManager.Initialize();
            base.Initialize();
        }

        protected void InitOrganize() { }

        protected override void LoadContent()
        {
            Debug.StartLogTimer();
            spriteBatch = new SpriteBatch(GraphicsDevice);
            _Renderer = new Renderer(spriteBatch, SSS.DefaultInfo(), new GameView(), 1);
            AssetManager.Load(_Renderer);
            _Renderer.Load();
            _Renderer.View.Zoom = 4;
            AssetManager.LoadEffect("phantom");
            AssetManager.LoadPostEffect("bloom", true);
            AssetManager.LoadPostEffect("soano", false);

            loadDebugLevel = false;
            int lev = loadDebugLevel ? 0 : 1;
            LoadLevel(lev);
            Oragnizer.LoadComps(Content);

            if (!SSS.PreviewMode)
            {
                AudioSystem.Load("Game");
            }

            Debug.Load(Content, Keys.OemTilde);
            Debug.SetOSS("preview mode", SSS.PreviewMode);
            Debug.EndLogTimer("boot up");
        }

        public static void LoadLevel(int id, string transTar = null)
        {
            if(id != 1)
                Debug.StartLogTimer();
            _Colliders = new List<Collider>();
            _NPCs = new List<NPC>(64);
            NPC.Packs = new List<int[]>();
            _Projectiles = new List<Projectile>(128);
            RopeEngine.Ropes?.Clear();
            _LevelHandler.Initialize(id);
            _LevelHandler.Load(SSS.Game.Content, _Renderer);

            FirstLevelLoad = SSS.FirstLoad && !LevelHandler.MainMenu && GameStateManager.States[0] == 1;
            if (FirstLevelLoad)
                GameStateManager.States[0] = 2;

            GameInputs.BlockAllInputs = false;
            _Renderer.EnableLigthing = false;
            if (!LevelHandler.MainMenu)
            {
                _Renderer.EnableLigthing = true;
                _Renderer.Lighting.Load(SSS.Game.Content, _Renderer, "effects/lighting", "effects/areaLighting", LevelHandler.Colliders.ToArray());
                ParticleManager.Load();

                Vector2 spawnPos = !loadDebugLevel ? new Vector2(54, 784) : new Vector2(208, 592);
                if (GameStateManager.States.ContainsKey(33) && !loadDebugLevel)
                    spawnPos = new Vector2(GameStateManager.States[33], GameStateManager.States[34]);
                if (transTar != null)
                {
                    var target = _LevelHandler.EntityMan.GetEnt(transTar);
                    spawnPos = target.Centre - new Vector2(_Player.Transform.Width / 2, _Player.Transform.Height / 2);
                }
                _Player.Spawn(spawnPos);

                if (GameStateManager.States.ContainsKey(35))
                {
                    bool aceLevel = int.TryParse(_LevelHandler.levName.Remove(0, 1), out int lev);
                    if (aceLevel)
                    {
                        lev -= 1;
                        Progress = (int)GameStateManager.States[35];
                        if (Progress == lev - 1 && lev < 8)
                        {
                            Progress++;
                            GameStateManager.States[35] = Progress;
                        }
                    }
                }
                else
                    GameStateManager.States[35] = Progress;

                if (id == 20 || id == 21 || id == 19)
                    AssetManager.TogglePostEffect(true, "soano");
                else if(id != 20)
                    AssetManager.TogglePostEffect(false, "soano");
            }

            UIDesigner.Load(false);
            if (LevelHandler.CurrentLevel != 1)
            {
                UIDesigner.ChangeMenu("game");
                _Skybox.LoadSkybox(ParamLoader.GetParam<int>("level", SSS.PreviewMode ? LevelHandler.CurrentLevel : id, "sky"));
                NodegraphBuiler.CalculateGraph(LevelHandler.Colliders.ToArray());
                _Colliders.AddRange(LevelHandler.Colliders);
            }
            else 
            { 
                UIDesigner.ChangeMenu("mainMenu");
            }

            if(!SSS.PreviewMode)
                AudioSystem.StopAllEvents();
            Dead = false;
            Paused = false;
            if (id != 1)
                Debug.EndLogTimer("loading level");
        }

        protected override void Update(GameTime gameTime)
        {
            DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            SSS.Delta = DeltaTime;

            GameInputs.UpdateState();
            if (!SSS.PreviewMode)
            {
                Oragnizer.UpdateMinimalComps(gameTime);
                AudioSystem.Update(LevelHandler.MainMenu ? Vector2.Zero : _Player.Transform.Centre);
            }
            if (!Paused && !Dead)
            {
                _LevelHandler.Update(gameTime);
                _Player.Update();

                if (!SSS.PreviewMode && !LevelHandler.MainMenu)
                {
                    RopeEngine.Update();
                    ParticleManager.Update();
                    foreach (NPC npc in _NPCs.ToArray())
                        npc.Update(gameTime);
                    foreach (Projectile proj in _Projectiles.ToArray())
                        proj.Update();
                }
            }
            UIDesigner.Update(gameTime);

            if (LevelHandler.CurrentLevel != 1 && GameInputs.OncePress(Keys.Escape) && !Dead && !UIManager.blockButtons)
            {
                if (Paused)
                    UIDesigner.ChangeMenu(UIDesigner.PreviousPauseMenu);
                else
                {
                    UIDesigner.Load(true);
                    UIDesigner.ChangeMenu("pause");
                    UIDesigner.ChangeUI(2);
                }
                Paused = !Paused;
            }

            if (Dead && !LevelHandler.MainMenu)
            {
                deathTimer -= SSS.Delta;
                if(deathTimer <= 0)
                {
                    //if(LevelHandler.CurrentLevel == 21 || LevelHandler.CurrentLevel == 20)
                        LoadLevel((int)GameStateManager.States[1]);
                    //else
                        //LoadLevel(LevelHandler.CurrentLevel);
                }
            }

            Debug.Update(out bool pause);
            if (pause)
                Paused = Debug.Active;
            //Debug.SetOSS("player pos", _Player.Transform.Position);
            Debug.SetOSS("cam pos", _Renderer.View.Position);

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            if (!_Player.FallKilling)
                CamTarget = LevelHandler.MainMenu ? Vector2.Zero : _Player.Transform.Centre;

            _Renderer.NormalsPhase(_LevelHandler);
            _Renderer.PhaseOne();

            _Skybox.Draw(_Renderer);
            _Renderer.PhaseTwo();
            _LevelHandler.StandardDraw(_Renderer);
            Debug.DrawVisuals(_Renderer);
            if (!SSS.PreviewMode && !LevelHandler.MainMenu)
            {
                ParticleManager.Draw(_Renderer);
                Oragnizer.DrawComps(_Renderer);
                foreach (NPC npc in _NPCs.ToArray())
                    npc.Draw(_Renderer);
                foreach (Projectile proj in _Projectiles.ToArray())
                    proj.Draw(_Renderer);
                RopeEngine.Draw(_Renderer);
            }
            _Player.Draw(_Renderer);

            _Renderer.PhaseThree((float)gameTime.TotalGameTime.TotalSeconds);
            if (!SSS.PreviewMode)
            {
                UIManager.Draw(_Renderer);
                UIDesigner.Draw(_Renderer);
                DialogueManager.Draw(_Renderer);
            }
            Debug.Draw(_Renderer, gameTime);
            _Renderer.PhaseFour();

            _Renderer.View.TargetPosition = CamTarget;
            _Renderer.View.Update(graphics.GraphicsDevice);
        }

        public static void SpawnNPC(NPC npc, Vector2 position, string entity = null, int permaSlot = -1, int permaValue = 0)
        {
            npc.Spawn(position, entity, permaSlot, permaValue);
        }

        public static void SpawnProjectile(Projectile proj, Vector2 position, Vector2 velocity)
        {
            proj.Spawn(position, velocity);
        }

        public static NPC GetClosestNPC(Vector2 pos, bool corpses, out float distance, float min = float.PositiveInfinity)
        {
            if (corpses && _NPCs.Where(x => x.Corpse).Count() == _NPCs.Count)
                corpses = true;

            NPC bestNPC = null;
            float bestDist = min;
            foreach (NPC npc in _NPCs)
            {
                if (!corpses && npc.Corpse)
                    continue;

                float dist = Vector2.Distance(pos, npc.Transform.Position);
                if (dist < bestDist)
                {
                    bestNPC = npc;
                    bestDist = dist;
                }
            }
            distance = bestDist;
            return bestNPC;
        }

        public static Projectile GetClosestProjectile(Vector2 pos, float min = float.PositiveInfinity)
        {
            Projectile bestProj = null;
            float bestDist = min;
            foreach (Projectile npc in _Projectiles)
            {
                float dist = Vector2.Distance(pos, npc.Transform.Position);
                if (dist < bestDist)
                {
                    bestProj = npc;
                    bestDist = dist;
                }
            }
            return bestProj;
        }

        public static void ExitGame()
        {
            SSS.Game.Exit();
        }

        //hire me microsoft (jk imma kill bill gates)
        private static bool doneDefaultSettings;
        public static void SetSettings(bool load)
        {
            List<string> names = new List<string>(9) { "jump", "ramLeft", "ramRight", "interact", "up", "down", "left", "right", "boost" };

            if (load && !SSS.FirstLoad) {
                GameInputs.Controls = new Dictionary<string, Keys>(9);
                _Settings = new List<int>(4);
                for (int i = 0; i < 13; i++)
                {
                    if (i < 4)
                        _Settings.Add((int)GameStateManager.States[i + 2]);
                    else
                        GameInputs.Controls[names[i- 4]] = (Keys)GameStateManager.States[i + 2];
                }
            }
            else {
                if (SSS.FirstLoad && !doneDefaultSettings)
                {
                    doneDefaultSettings = true;
                    GameInputs.Controls = new Dictionary<string, Keys>(9) { { "jump", Keys.Space }, { "ramLeft", Keys.Q }, { "ramRight", Keys.E }, { "interact", Keys.F }, 
                        { "up", Keys.W }, { "down", Keys.S }, { "left", Keys.A }, { "right", Keys.D }, { "boost", Keys.W } };
                    _Settings = new List<int>(4) { 0, 2, 0, 2 };
                }

                for (int i = 0; i < 13; i++)
                {
                    if(i < 4)
                        GameStateManager.States[i + 2] = _Settings[i];
                    else
                        GameStateManager.States[i + 2] = (int)GameInputs.Controls[names[i - 4]];
                }
            }
        }

        public static bool IsOnScreen(Vector2 point)
        {
            return CollisionEngine.PointInRect(_Renderer.View.ViewRect, point);
        }
        public static bool IsOnScreen(Rectangle target)
        {
            return CollisionEngine.RectInRect(_Renderer.View.ViewRect, target);
        }

        public static Color CahngeColorBrightness(Color col, float amount)
        {
            return new Color(col.R / 255f * amount, col.G / 255f * amount, col.B / 255f * amount);
        }

        public static NPC GetNPC(int uuid)
        {
            return _NPCs.Where(x => x.UUID.MainUUID == uuid).First();
        }

        private static float deathTimer;
        public static void Die()
        {
            if (!Dead && !_Player.Invincible)
            {
                ScriptManager.ExecuteFunction("death");
                GameInputs.BlockAllInputs = false;
                Paused = true;
                Dead = true;
                deathTimer = 3.8f;
                UIDesigner.ChangeMenu("death", false);
                AudioSystem.PlayEvent("death", true, Vector2.Zero, false, force3D: false);
            }
        }

        public static void SaveGame(bool saveLevel)
        {
            if(saveLevel)
                GameStateManager.States[1] = LevelHandler.CurrentLevel;
            GameStateManager.States[15] = _Player.MaxHealth;
            GameStateManager.States[16] = _Player.Speed;
            GameStateManager.States[17] = _Player.JumpVel;
            GameStateManager.States[18] = _Player.MaxBoosts;
            GameStateManager.States[19] = _Player.MaxSelfBoosts;
            GameStateManager.States[20] = _Player.RamAmount;
            GameStateManager.States[21] = _Player.MaxJetTime;
            GameStateManager.States[22] = _Player.KnockbackRes;
            GameStateManager.States[36] = (int)_Player.Upgrades[0];
            GameStateManager.States[37] = (int)_Player.Upgrades[1];
            GameStateManager.States[38] = PhantomManager.QPT(0, 0);
            GameStateManager.States[39] = PhantomManager.QPT(1, 0);
            GameStateManager.States[40] = PhantomManager.QPT(2, 0);
            GameStateManager.States[41] = PhantomManager.QPT(2, 1);
            GameStateManager.States[42] = PhantomManager.QPT(3, 0);
            GameStateManager.States[43] = PhantomManager.QPT(3, 1);
            GameStateManager.States[44] = PhantomManager.QPT(4, 0);
            GameStateManager.States[45] = PhantomManager.QPT(4, 1);
            GameStateManager.States[46] = PhantomManager.QPT(5, 0);
            GameStateManager.States[47] = PhantomManager.QPT(5, 1);
            GameStateManager.States[66] = PhantomManager.StatPoints;
            GameStateManager.States[300] = (int)_Player.WeaponType;
            GameStateManager.WriteSave();
        }

        public static int GetStat(int id)
        {
            float stat = 0;
            switch (id)
            {
                case 0: stat = _Player.MaxHealth; break;
                case 1: stat = _Player.MaxBoosts; break;
                case 2: stat = _Player.MaxSelfBoosts; break;
                case 3: stat = (int)_Player.MaxJetTime; break;
                case 4: stat = (int)(_Player.Speed * (5 / 2f)); break;
                case 5: stat = (int)(_Player.JumpVel * (5 / 1.8f)); break;
                case 6: stat = (int)(_Player.RamAmount * (5 / 8.5f)); break;
                case 7: stat = (int)(_Player.KnockbackRes * 5); break;
            }
            return (int)stat;
        }

        public static bool HasUpgradeOn(Upgrade upgrade)
        {
            return _Player.Upgrades != null && _Player.Upgrades.Contains(upgrade);
        }
    }
}
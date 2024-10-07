using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using TripleS.Lighting;
using TripleS.Tiled;

namespace TripleS.Scripting {

    /// <summary>
    /// Base class for a map entity.
    /// </summary>
    public abstract class Entity {

        public Vector2 Position { get; set; }
        public Rectangle? Bounds { get; set; }
        public string Name { get; set; }
        public List<EntityProp> Properties { get; set; }
        public bool Active { get; set; }
        public int UUID { get; set; }
        public virtual DefaultProp[] DefaultProperties { get; protected set; }
        public virtual string ID { get; protected set; }
        public bool Static { get; protected set; }
        public bool Point { get; protected set; }
        public string HumanName { get; protected set; }
        public bool Saves { get; protected set; }
        public string StartActiveProperty { get; protected set; }
        public Polygon? Polyigonal { get; set; }
        public int DrawLayer { get; set; }
        public Vector2 Centre 
        { 
            private set { }
            get
            {
                return Bounds.HasValue ? new Vector2(Position.X + (Bounds.Value.Width / 2), Position.Y + (Bounds.Value.Height / 2)) : Position;
            }
        }

        public virtual void Init() { }
        public virtual void Load(ContentManager content) {
            if (Active)
                Activate();

            if(StartActiveProperty != "" && StartActiveProperty != null)
                Active = GetEntProp<bool>(StartActiveProperty);
        }
        public virtual void Draw(Renderer renderer) { }
        public virtual void Update(GameTime time) { }

        public virtual void Deactivate() { Active = false; }
        public virtual void Activate() { Active = true; }

        public object GetEntProp(string name)
        {
            return Properties.Where(x => x.ParentProp.ID == name).First().GetValue();
        }

        public T GetEntProp<T>(string name)
        {
            return (T)Properties.Where(x => x.ParentProp.ID == name).First().GetValue();
        }
    }

    public struct EntityProp
    {
        public DefaultProp ParentProp { get; private set; }
        public string Value { get; set; }

        public EntityProp(DefaultProp parent, string value)
        {
            Value = value;
            ParentProp = parent;
        }

        public object GetValue()
        {
            object val = Value;
            switch (ParentProp.Type)
            {
                case TiledPropertyType.Int:
                    val = int.Parse(Value);
                    break;
                case TiledPropertyType.Float:
                    val = float.Parse(Value);
                    break;
                case TiledPropertyType.Bool:
                    val = bool.Parse(Value);
                    break;
            }
            return val;
        }
    }

    public struct DefaultProp
    {
        public string ID { get; set; }
        public TiledPropertyType Type { get; set; }

        public DefaultProp(string id, TiledPropertyType type)
        {
            ID = id;
            Type = type;
        }
    }

    //BUILT-IN ENTS

    public class EntNavNode : Entity {

        public EntNavNode()
        {
            ID = "node";
            DefaultProperties = new DefaultProp[0]
            { };
            Static = true;
            Point = true;
        }

        public override void Load(ContentManager content)
        {
            base.Load(content);
            NodegraphBuiler.AddNode(Position);
        }

        public override void Draw(Renderer renderer)
        {
            base.Draw(renderer);
            //renderer.BasicDraw(SSS.Square, new Rectangle((int)Position.X + 2, (int)Position.Y + 2, -4, -4), 1, 1, col: new Color(0, 1f, 0));
        }
    }

    public class EntReverbZone : Entity
    {
        public EntReverbZone()
        {
            ID = "reverb";
            DefaultProperties = new DefaultProp[3] 
            { 
                new DefaultProp("minRadius", TiledPropertyType.Float),    
                new DefaultProp("wetness", TiledPropertyType.Float),    
                new DefaultProp("reverbType", TiledPropertyType.Int)
            };
            Static = true;
            Point = false;
        }

        public override void Init()
        {
            base.Init();
            ReverbZone reverb = new ReverbZone(Centre, GetEntProp<float>("minRadius"), Bounds.Value.Width / 2, GetEntProp<int>("reverbType"), GetEntProp<float>("wetness"));
            AudioSystem.SpacialReverbs ??= new List<ReverbZone>();
            AudioSystem.SpacialReverbs.Add(reverb);
        }
    }

    public class EntLight : Entity {

        public UUID LightUUID { get; private set; }

        public EntLight()
        {
            ID = "light";
            DefaultProperties = new DefaultProp[9]
            {
                new DefaultProp("color", TiledPropertyType.Color),
                new DefaultProp("radius", TiledPropertyType.Float),
                new DefaultProp("brightness", TiledPropertyType.Float),
                new DefaultProp("directional", TiledPropertyType.Bool),
                new DefaultProp("shadows", TiledPropertyType.Bool),
                new DefaultProp("angle", TiledPropertyType.Float),
                new DefaultProp("falloff", TiledPropertyType.Float),
                new DefaultProp("dirRadius", TiledPropertyType.Float),
                new DefaultProp("startActive", TiledPropertyType.Bool)
            };
            Static = true;
            Point = true;
            StartActiveProperty = "startActive";
        }

        public override void Load(ContentManager content)
        {
            base.Load(content); 
            Vector3 col = MathUtil.HexToRGB(GetEntProp<string>("color"));
            LightUUID = LightingEngine.AddLight(new LightData(Position, GetEntProp<float>("brightness"), GetEntProp<float>("radius"), col, Active, GetEntProp<float>("falloff"), false, GetEntProp<bool>("directional"), GetEntProp<bool>("shadows"), GetEntProp<float>("angle"), GetEntProp<float>("dirRadius")));
        }

        public override void Draw(Renderer renderer)
        {
            base.Draw(renderer);

            bool vis = LightingEngine.Lights[LightUUID].Visible;
            if (Active && !vis)
                renderer.Lighting.SetLightVisible(LightUUID, true);
            else if(!Active && vis)
                renderer.Lighting.SetLightVisible(LightUUID, false);
        }
    }

    public class EntAreaLight : Entity {

        public UUID LightUUID { get; private set; }

        public EntAreaLight()
        {
            ID = "light_area";
            DefaultProperties = new DefaultProp[4]
            {
                new DefaultProp("color", TiledPropertyType.Color),
                new DefaultProp("radius", TiledPropertyType.Float),
                new DefaultProp("brightness", TiledPropertyType.Float),
                new DefaultProp("startActive", TiledPropertyType.Bool)
            };
            Static = true;
            Point = false;
            StartActiveProperty = "startActive";
        }

        public override void Load(ContentManager content)
        {
            base.Load(content);
            Vector3 col = MathUtil.HexToRGB(GetEntProp<string>("color"));
            LightUUID = LightingEngine.AddLight(new LightData(Position, GetEntProp<float>("brightness"), GetEntProp<float>("radius"), col, Active, 1f, true, false, false, bounds: new Vector2(Bounds.Value.Width, Bounds.Value.Height)));
        }

        public override void Draw(Renderer renderer)
        {
            base.Draw(renderer);

            bool vis = LightingEngine.Lights[LightUUID].Visible;
            if (Active && !vis)
                renderer.Lighting.SetLightVisible(LightUUID, true);
            else if (!Active && vis)
                renderer.Lighting.SetLightVisible(LightUUID, false);
        }
    }

    public class EntTileRegion : Entity {

        public int RegionSetID { get; private set; }
        public bool Transitions { get; private set; }

        public EntTileRegion()
        {
            ID = "tileRegion";
            DefaultProperties = new DefaultProp[2]
            {
                new DefaultProp("tilesetID", TiledPropertyType.Int),
                new DefaultProp("transition", TiledPropertyType.Bool)
            };
            Static = true;
            Point = false;
            Active = true;
        }

        public override void Load(ContentManager content)
        {
            base.Load(content);
            RegionSetID = GetEntProp<int>("tilesetID");
            Transitions = GetEntProp<bool>("transition");
        }
    }
}

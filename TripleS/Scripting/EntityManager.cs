using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using TripleS.Lighting;
using TripleS.Tiled;

namespace TripleS.Scripting {
    public class EntityManager {

        private ContentManager content;
        private Type[] entityTypes;
        public List<Entity> Entities { get; private set; }

        public EntityManager(ContentManager cm, Type[] types)
        {
            content = cm;
            entityTypes = types;
        }

        public void Load(IEnumerable<TiledLayer> objectLayers, Vector2 offset, int uuidOff)
        {
            Entities = new List<Entity>();
            int uuidp1 = uuidOff;
            foreach (TiledLayer layer in objectLayers)
            {
                uuidp1 += 1000;
                int uuidp2 = -1;
                foreach (TiledObject obj in layer.objects)
                {
                    uuidp2 += 10;
                    dynamic ent = null;
                    Type usingType = null;
                    foreach (Type type in entityTypes)
                    {
                        var tempEnt = (Entity)Activator.CreateInstance(type);
                        if (tempEnt.ID == obj.@class)
                        {
                            usingType = type;
                            ent = tempEnt;
                        }
                    }
                    
                    if (ent != null)
                    {
                        ent.Properties = new List<EntityProp>();
                        foreach (DefaultProp parentProp in ent.DefaultProperties)
                        {
                            var mathcingProp = obj.properties.Where(x => x.name == parentProp.ID && x.type == parentProp.Type).First();
                            ent.Properties.Add(new EntityProp(parentProp, mathcingProp.value));
                        }

                        Vector2 position = new Vector2(obj.x + offset.X, obj.y + offset.Y);
                        Rectangle? bounds = null;
                        Polygon? polygon = null;

                        if (obj.point == null && !ent.Point)
                        {
                            if (obj.polygon == null)
                                bounds = new Rectangle((int)position.X, (int)position.Y, (int)obj.width, (int)obj.height);
                            else
                            {
                                Vector2[] prePoints = new Vector2[obj.polygon.points.Length / 2];
                                for(int i = 0; i < obj.polygon.points.Length; i += 2)
                                {
                                    prePoints[i / 2] = position + new Vector2(obj.polygon.points[i], obj.polygon.points[i + 1]);
                                }
                                polygon = new Polygon() { Points = prePoints };
                            }
                        }

                        ent.GID = obj.gid;
                        ent.Rotation = obj.rotation;
                        ent.Position = position;
                        ent.Bounds = bounds;
                        ent.Name = obj.name;
                        ent.UUID = uuidp1 + uuidp2;
                        ent.Polyigonal = polygon;

                        Entities.Add(ent);
                    }
                }
            }
            Entities = Entities.OrderBy(x => -(x.DrawLayer - 1)).ToList();
            if (Entities != null && Entities.Count > 0)
            {
                foreach (Entity ent in Entities)
                {
                    ent.Init();
                    ent.Load(content);
                }
            }

            Debug.Log("Loaded ents: " + Entities.Count, LogType.Log);
        }

        public void UpdateEnts(GameTime time)
        {
            foreach (Entity ent in Entities.ToArray())
            {
                ent.Update(time);
            }
        }

        public void DrawEnts(Renderer ren)
        {
            foreach (Entity ent in Entities.ToArray())
            {
                ent.Draw(ren);
            }
        }

        public Entity GetEnt(string name)
        {
            if (Entities != null && Entities.Count > 0)
            {
                foreach (Entity ent in Entities)
                {
                    if (ent.Name == name)
                        return ent;
                }
            }
            return null;
        }

        public IEnumerable<Entity> GetAllEnts(string name)
        {
            if (Entities != null && Entities.Count > 0)
            {
                return Entities.Where(x => x.Name == name);
            }
            return null;
        }

        public int NameToUUID(string name)
        {
            if (Entities != null && Entities.Count > 0)
            {
                foreach (Entity ent in Entities)
                {
                    if (ent.Name == name)
                        return ent.UUID;
                }
            }
            return -1;
        }


        public EntityProp GetEntityProperty(string entName, string propName)
        {
            if (Entities != null && Entities.Count > 0)
            {
                Entity ent = GetEnt(entName);
                if (ent != null)
                {
                    foreach (EntityProp prop in ent.Properties)
                    {
                        if (prop.ParentProp.ID == propName)
                            return prop;
                    }
                }
            }
            return new EntityProp();
        }

        public void KillEnt(string name)
        {
            Entities.Remove(GetEnt(name));
        }
    }
}

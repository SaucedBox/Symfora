using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace TripleS {

    /// <summary>
    /// Keeps services and components nice and organized.
    /// </summary>
    public static class Oragnizer {

        private static List<IGameComponent> components = new List<IGameComponent>();

        public static void AddService(object service)
        {
            Type type = service.GetType();
            foreach(Type interf in type.GetInterfaces())
            {
                if(interf != typeof(IGameComponent))
                {
                    SSS.Game.Services.AddService(type, service);
                }
            }
        }

        public static void AddComponent(IGameComponent component, bool single)
        {
            if (components.Count != 0 && !single)
            {
                foreach (IGameComponent comp in components)
                {
                    Type t = comp.GetType();
                    if (t != component.GetType())
                    {
                        components.Add(component);
                        SSS.Game.Components.Add(component);
                    }
                }
            }
            else if(single)
            {
                components.Add(component);
                SSS.Game.Components.Add(component);
            }
        }

        public static T GetService<T>()
        {
            var serv = SSS.Game.Services.GetService(typeof(T));
            if (serv == null)
                throw new NullReferenceException();
            return (T)serv;
        }

        public static IGameComponent GetComponent(Type componentType)
        {
            foreach (IGameComponent comp in components)
            {
                Type t = comp.GetType();
                if (t == componentType)
                    return comp;
            }
            return null;
        }

        public static void UpdateMinimalComps(GameTime gameTime)
        {
            foreach (IGameComponent comp in components)
            {
                if(comp.GetType().BaseType == typeof(MinimalComponent))
                {
                    MinimalComponent mc = (MinimalComponent)comp;
                    mc.Update(gameTime);
                }
            }
        }

        public static void DrawComps(Renderer renderer)
        {
            foreach(IGameComponent comp in components)
            {
                if (comp.GetType().BaseType == typeof(MinimalComponent))
                {
                    MinimalComponent mc = (MinimalComponent)comp;
                    mc.Draw(renderer);
                }
            }
        }

        public static void LoadComps(ContentManager content)
        {
            foreach (IGameComponent comp in components)
            {
                if (comp.GetType().BaseType == typeof(MinimalComponent))
                {
                    MinimalComponent mc = (MinimalComponent)comp;
                    mc.Load(content);
                }
            }
        }
    }
}

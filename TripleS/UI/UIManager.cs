using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TripleS.UI {

    /// <summary>
    /// UI manager and renderer.
    /// </summary>
    public static class UIManager {

        public static Dictionary<string, SpriteFont> fonts = new Dictionary<string, SpriteFont>();
        public static List<Element> elements = new List<Element>();
        public static ScreenSizer sizer = new ScreenSizer();
        public static float scale = 1;
        public static bool blockButtons;

        /// <summary>
        /// Draws all UI elements.
        /// </summary>
        public static void Draw(Renderer renderer)
        {
            sizer.view = renderer.View;
            foreach(Element element in elements.ToArray())
            {
                DrawIndElement(element, renderer);
            }
        }

        public static void DrawIndElement(Element element, Renderer renderer, bool overrideVisible = false)
        {
            if ((element.Visible || overrideVisible) && element.TrueVisible)
            {
                Vector2 position = sizer.GetPos(element.Position, element.Anchor, element.Sizing);
                SpriteFont font = element.Font != null ? fonts[element.Font] : null;
                float teScale = (scale + element.Scale) / element.TextSize;
                Vector2 textDims = font != null ? font.MeasureString(element.Conents) * teScale : Vector2.Zero;

                if (element.Sizing == SizingStyle.Sretch)
                {
                    int width = element.Width;
                    int height = element.Height;
                    if (element.Conents != "" && font != null)
                    {
                        width = (int)(textDims.X + (element.Padding * 6));
                        height = (int)(textDims.Y + (element.Padding * 2));
                    }

                    position = sizer.ApplyOffset(element.Anchor, position, width, height);
                    element.bounds = new Rectangle((int)position.X, (int)position.Y, width, height);
                    renderer.BasicDraw(element.Image, element.bounds, 1, element.SubLayer / 10 + 0.01f, element.Scale, col: element.Color * element.Alpha, effect: element.Effect);
                }
                else
                {
                    position = sizer.ApplyOffset(element.Anchor, position, element.Width, element.Height);
                    element.bounds = new Rectangle((int)position.X, (int)position.Y, element.Width, element.Height);
                    renderer.BasicDraw(element.Image, position, 1, element.SubLayer / 10, element.Scale, col: element.Color * element.Alpha, effect: element.Effect);
                }

                if (element.Conents != "" && font != null)
                {
                    Vector2 textPos = sizer.GetTextPos(position, element.Padding, element.TextAlignment);
                    renderer.DrawText(font, element.Conents, textPos, 1, element.SubLayer / 10, col: element.TextColor * element.Alpha, size: teScale, effect: element.Effect);
                }
            }
        }

        /// <summary>
        /// Updates UI states.
        /// </summary>
        public static void Update(GameTime gameTime, Matrix view)
        {
            Vector2 mousePos = Vector2.Transform(GameInputs.GetMouse().Position.ToVector2(), Matrix.Invert(view));
            foreach (Element element in elements.ToArray())
            {
                if (element.TrueVisible && element.Visible && element.Alpha != 0)
                {
                    if (Physics.CollisionEngine.PointInRect(element.bounds, mousePos) && !blockButtons)
                    {
                        if (GameInputs.OnceLeftClick(ButtonState.Pressed))
                        {
                            element.highTimer = 0.2f;
                            element.EvokeEvent();
                        }
                        else
                            element.State = ElementState.Highlighted;
                    }
                    else
                        element.State = ElementState.None;

                    if (element.highTimer > 0f)
                    {
                        element.highTimer -= SSS.Delta;
                        element.State = ElementState.Pressed;
                    }
                    else
                        element.highTimer = 0f;

                    element.Update(gameTime);
                }
            }
        }

        public static void AddElement(Element element)
        {
            elements.Add(element);
        }

        public static void AddFont(string id, string path, ContentManager content, float spacing)
        {
            SpriteFont tex = content.Load<SpriteFont>(path);
            tex.Spacing = spacing;
            fonts.Add(id, tex);
        }

        public static void Reset()
        {
            fonts = new Dictionary<string, SpriteFont>();
            elements = new List<Element>();
        }

        public static Element GetElement(string id)
        {
            return elements.Where(x => x.ID == id).First();
        }
    }
}

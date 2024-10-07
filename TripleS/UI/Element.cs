using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq.Expressions;

namespace TripleS.UI {

    /// <summary>
    /// Base of UI element.
    /// </summary>
    public class Element {
        public string Conents { get; set; }
        public Vector2 Position { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Color Color { get; set; }
        public Color TextColor { get; set; }
        public string Font { get; set; }
        public ElementState State { get; set; }
        public AnchorPoint Anchor { get; set; }
        public SizingStyle Sizing { get; set; }
        public TextPosition TextAlignment { get; set; }
        public Texture2D Image { get; set; }
        public Effect Effect { get; set; }
        public string ID { get; set; }
        public string MenuID { get { return ID.Remove(ID.IndexOf('_')); } set { } }
        public bool Visible { get; set; }
        public bool TrueVisible { get; set; }
        public int SubLayer { get; set; }
        public int Padding { get; set; }
        public float Scale { get; set; }
        public float TextSize { get; set; }
        /// <summary>
        /// DO NOT CHANGE for other reasons besides transitioning. If you need to, change the base color of the element.
        /// </summary>
        public float Alpha { get; set; }
        public int Style { get; set; }
        public bool Transitions { get; set; }

        public event EventHandler<ButtonEventArgs> OnClick;
        public Rectangle bounds;
        public float highTimer;

        public Element(string name)
        {
            bounds = new Rectangle();
            ID = name;
            Position = Vector2.Zero;
            Width = 1;
            Height = 1;
            Color = Color.White;
            TextColor = Color.White;
            State = ElementState.None;
            Anchor = AnchorPoint.TopLeft;
            Sizing = SizingStyle.None;
            Visible = false;
            Effect = null;
            Transitions = false;
            Image = SSS.Square;
            Conents = "";
            Scale = 1;
            TextSize = 1;
            Style = -1;
            TrueVisible = true;
            TextAlignment = TextPosition.Middle;
        }

        public virtual void Update(GameTime time) 
        {
            Alpha = Math.Clamp(Alpha, 0.0f, 1.0f);
        }

        public void EvokeEvent()
        {
            OnClick?.Invoke(this, new ButtonEventArgs(ID));
        }
    }

    public enum ElementState
    {
        None,
        Pressed,
        Highlighted,
        In,
        Out
    }

    public enum AnchorPoint
    {
        TopLeft,
        TopMiddle,
        TopRight,
        MiddleLeft,
        Middle,
        MiddleRight,
        BottomLeft,
        BottomMiddle,
        BottomRight
    }

    public enum SizingStyle
    {
        Anchor,
        Sretch,
        None
    }

    public enum TextPosition
    {
        Left,
        Middle,
    }


    public class ButtonEventArgs : EventArgs {
        public string ElName { get; set; }

        public ButtonEventArgs(string name)
        {
            ElName = name;
        }
    }
}

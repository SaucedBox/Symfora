using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TripleS;
using TripleS.UI;

namespace Symfora {
    public class ButtonElement : Element {

        public Color BaseColor { get; set; }
        public Color SelectColor { get; set; }
        public Color HighlightColor { get; set; }
        public Color TextHighlightColor { get; set; }

        private Color initTCol;

        public ButtonElement(string name, Color baseCol, Color seleCol, Color highCol, Color textBase, Color textHigh) : base(name)
        {
            BaseColor = baseCol;
            SelectColor = seleCol;
            HighlightColor = highCol;
            initTCol = textBase;
            TextColor = textBase;
            TextHighlightColor = textHigh;
        }

        public override void Update(GameTime time)
        {
            base.Update(time);
            if (State == ElementState.None)
            {
                TextColor = initTCol;
                Color = BaseColor;
            }
            else if (State == ElementState.Highlighted)
            {
                TextColor = TextHighlightColor;
                Color = HighlightColor;
            }
            else if (State == ElementState.Pressed)
            {
                TextColor = TextHighlightColor;
                Color = SelectColor;
            }
        }

        public void ChangeTextColor(Color col)
        {
            initTCol = col;
        }
    }
}

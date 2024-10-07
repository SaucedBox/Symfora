using Microsoft.Xna.Framework;

namespace TripleS.UI {
    public class ScreenSizer {

        public GameView view;

        public Vector2 GetPos(Vector2 original, AnchorPoint anchor, SizingStyle style)
        {
            if (style == SizingStyle.None)
                return view.Position + original * view.Zoom;
            else if(style == SizingStyle.Anchor || style == SizingStyle.Sretch)
            {
                Vector2 ap = GetAnchorPos(anchor) + view.Position;
                return ap + original * view.Zoom;
            }
            return view.Position * view.Zoom;
        }

        public Vector2 GetTextPos(Vector2 elPos, int pad, TextPosition align)
        {
            if (align == TextPosition.Left)
                return new Vector2(elPos.X + pad / 2, elPos.Y + pad / 2);
            else
                return new Vector2(elPos.X + (pad * 6 / 2), elPos.Y + pad);
        }

        public Vector2 GetAnchorPos(AnchorPoint point)
        {
            switch (point)
            {
                case AnchorPoint.TopLeft:
                    return Vector2.Zero;
                case AnchorPoint.TopMiddle:
                    return new Vector2(view.windowWidth / 2, 0);
                case AnchorPoint.TopRight:
                    return new Vector2(view.windowWidth, 0);
                case AnchorPoint.MiddleLeft:
                    return new Vector2(0, view.windowHeight / 2);
                case AnchorPoint.Middle:
                    return new Vector2(view.windowWidth / 2, view.windowHeight / 2);
                case AnchorPoint.MiddleRight:
                    return new Vector2(view.windowWidth, view.windowHeight / 2);
                case AnchorPoint.BottomLeft:
                    return new Vector2(0, view.windowHeight);
                case AnchorPoint.BottomMiddle:
                    return new Vector2(view.windowWidth / 2, view.windowHeight);
                case AnchorPoint.BottomRight:
                    return new Vector2(view.windowWidth, view.windowHeight);
            }
            return Vector2.Zero;
        }

        public float GetXOffset(AnchorPoint point)
        {
            float offset = 0;
            switch (point) 
            {
                case AnchorPoint.Middle:
                    offset = 2;
                    break;
                case AnchorPoint.TopMiddle:
                    offset = 2;
                    break;
                case AnchorPoint.BottomMiddle:
                    offset = 2;
                    break;
                case AnchorPoint.MiddleRight:
                    offset = 1;
                    break;
                case AnchorPoint.TopRight:
                    offset = 1;
                    break;
                case AnchorPoint.BottomRight:
                    offset = 1;
                    break;
            }
            return offset;
        }

        public float GetYOffset(AnchorPoint point)
        {
            float offset = 0;
            switch (point)
            {
                case AnchorPoint.BottomLeft:
                    offset = 1;
                    break;
                case AnchorPoint.BottomRight:
                    offset = 1;
                    break;
                case AnchorPoint.BottomMiddle:
                    offset = 1;
                    break;
                case AnchorPoint.Middle:
                    offset = 2;
                    break;
                case AnchorPoint.MiddleRight:
                    offset = 2;
                    break;
                case AnchorPoint.MiddleLeft:
                    offset = 2;
                    break;
            }
            return offset;
        }

        public Vector2 ApplyOffset(AnchorPoint point, Vector2 position, int w, int h)
        {
            float xOff = GetXOffset(point);
            float yOff = GetYOffset(point);

            if (xOff != 0)
                position.X -= w / xOff;
            if (yOff != 0)
                position.Y -= h / yOff;

            return position;
        }
    }
}


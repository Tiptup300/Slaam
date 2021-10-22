using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SlaamMono.Library.Resources;

namespace SlaamMono.SubClasses
{

    public class DrawingBlock
    {
        private Rectangle DrawingRectangle;
        private Color DrawingColor;

        private readonly IWhitePixelResolver _whitePixelResolver;

        public DrawingBlock(Rectangle drawingrect, Color drawingcol)
        {
            DrawingRectangle = drawingrect;
            DrawingColor = drawingcol;

            _whitePixelResolver = DiImplementer.Instance.Get<IWhitePixelResolver>();
        }

        public void ChangeColor(Color newcol)
        {
            DrawingColor = newcol;
        }

        public void Draw(SpriteBatch batch, Vector2 Offset)
        {
            batch.Draw(_whitePixelResolver.GetWhitePixel(), new Rectangle(DrawingRectangle.X + (int)Offset.X, DrawingRectangle.Y + (int)Offset.Y, DrawingRectangle.Width, DrawingRectangle.Height), DrawingColor);
        }

    }
}

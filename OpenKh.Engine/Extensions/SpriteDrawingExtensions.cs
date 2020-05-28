using OpenKh.Engine.Renders;

namespace OpenKh.Engine.Extensions
{
    public static class SpriteDrawingExtensions
    {
        public static void FillRectangle(this ISpriteDrawing drawing, float x, float y, float width, float height, ColorF color)
        {
            drawing.AppendSprite(new SpriteDrawingContext()
                .Source(0, 0, 1, 1)
                .Position(x, y)
                .DestinationSize(width, height)
                .Color(color));
        }

        public static void DrawRectangle(this ISpriteDrawing drawing, float x, float y, float width, float height, ColorF color, float thickness = 1.0f)
        {
            drawing.FillRectangle(x, y, width, thickness, color);
            drawing.FillRectangle(x, y + height - 1, width - 1, thickness, color);
            drawing.FillRectangle(x, y, thickness, height, color);
            drawing.FillRectangle(x + width - 1, y, thickness, height, color);
        }
    }
}

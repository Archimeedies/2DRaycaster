// Inside your Sprite class (if not already defined)
using System.Drawing;

public class Sprite
{
    public float X { get; set; }
    public float Y { get; set; }
    public Bitmap Texture { get; set; }

    public Sprite(float x, float y, Bitmap texture)
    {
        X = x;
        Y = y;
        Texture = texture;
    }

    public RectangleF GetBoundingBox()
    {
        // Assuming each sprite has a square bounding box for simplicity
        const float spriteSize = 1.0f; // Adjust as needed based on your sprite size
        return new RectangleF(X - spriteSize / 2, Y - spriteSize / 2, spriteSize, spriteSize);
    }
}

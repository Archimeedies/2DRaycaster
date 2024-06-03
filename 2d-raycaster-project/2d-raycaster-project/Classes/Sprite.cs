using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2d_raycaster_project.Classes
{
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
    }
}

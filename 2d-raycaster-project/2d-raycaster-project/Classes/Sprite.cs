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
        public PointF Position { get; set; }
        public SizeF Size { get; set; }
        public Bitmap Texture { get; set; }

        public float ScreenX { get; set; }
        public float ScreenY { get; set; }

        public Sprite(PointF position, SizeF size, Bitmap texture)
        {
            Position = position;
            Size = size;
            Texture = texture;
        }
    }


}

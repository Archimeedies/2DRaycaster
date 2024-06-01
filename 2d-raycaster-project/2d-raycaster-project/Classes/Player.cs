using System;
using System.Drawing;
using System.Windows.Forms;

namespace _2d_raycaster_project
{
    public class Player
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Direction { get; set; }
        public float FOV { get; set; }
        public Player(float x, float y, float direction, float fov)
        {
            X = x;
            Y = y;
            Direction = direction;
            FOV = fov;
        }
    }
}

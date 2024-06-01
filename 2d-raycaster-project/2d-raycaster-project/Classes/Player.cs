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
        public float DirectionX { get; set; }
        public float DirectionY { get; set; }
        public float PlaneX { get; set; }
        public float PlaneY { get; set; }
        public float FOV { get; set; }

        public Player(float x, float y, float direction, float fov)
        {
            X = x;
            Y = y;
            Direction = direction;
            FOV = fov;

            // Initialize direction vector
            DirectionX = (float)Math.Cos(Direction);
            DirectionY = (float)Math.Sin(Direction);

            // Initialize camera plane (perpendicular to the direction vector)
            float planeMagnitude = (float)Math.Tan(FOV / 2.0f);
            PlaneX = -DirectionY * planeMagnitude;
            PlaneY = DirectionX * planeMagnitude;
        }
    }
}

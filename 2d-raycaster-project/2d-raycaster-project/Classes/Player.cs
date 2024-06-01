using System;

namespace _2d_raycaster_project
{
    public class Player
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Direction { get; private set; }
        public float FOV { get; private set; }

        public Player(float x, float y, float direction, float fov)
        {
            X = x;
            Y = y;
            Direction = direction;
            FOV = fov;
        }

        public void MoveForward(float distance)
        {
            X += (float)Math.Cos(Direction) * distance;
            Y += (float)Math.Sin(Direction) * distance;
        }

        public void MoveBackward(float distance)
        {
            X -= (float)Math.Cos(Direction) * distance;
            Y -= (float)Math.Sin(Direction) * distance;
        }

        public void RotateLeft(float angle)
        {
            Direction -= angle;
        }

        public void RotateRight(float angle)
        {
            Direction += angle;
        }
    }
}

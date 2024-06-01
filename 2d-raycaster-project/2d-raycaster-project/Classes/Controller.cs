using System;
using System.Drawing;
using System.Windows.Forms;

namespace _2d_raycaster_project
{
    public class Controller
    {
        private Raycaster raycaster;

        public Controller(Bitmap bitmap, Graphics graphics, Size clientSize)
        {
            raycaster = new Raycaster(bitmap, graphics, clientSize);
        }

        public void Run()
        {
            raycaster.Update();
        }

        public void KeyMovement(KeyEventArgs e, float moveSpeed)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    raycaster.MoveForward(moveSpeed);
                    break;
                case Keys.S:
                    raycaster.MoveBackward(moveSpeed);
                    break;
                case Keys.A:
                    raycaster.MoveLeft(moveSpeed);
                    break;
                case Keys.D:
                    raycaster.MoveRight(moveSpeed);
                    break;
                case Keys.Escape:
                    Application.Exit();
                    break;
            }
        }
        public void PlayerMouseMove(Form1 form, ref Point lastMousePosition)
        {
            raycaster.PlayerMouseMove(form, ref lastMousePosition);
        }
    }
}

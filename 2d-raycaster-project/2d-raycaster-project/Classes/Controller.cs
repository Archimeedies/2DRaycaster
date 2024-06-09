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
            clientSize.Width = bitmap.Width;
            clientSize.Height = bitmap.Height;
            raycaster = new Raycaster(bitmap, graphics, clientSize);
        }

        public void Run()
        {
            raycaster.Update();
        }

        public void PlayerKeyMovement(KeyEventArgs e, float moveSpeed)
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
                case Keys.Space:
                    raycaster.MoveJump();
                    break;
                case Keys.Escape:
                    Application.Exit();
                    break;
            }
        }
        public void PlayerMouseMove(RaycasterEngine form, ref Point lastMousePosition, float sensitivity)
        {
            raycaster.MouseMove(form, ref lastMousePosition, sensitivity);
        }
    }
}
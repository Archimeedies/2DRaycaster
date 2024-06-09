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

        public void PlayerKeyDownMovement(KeyEventArgs e, float moveSpeed)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    raycaster.StartMove(e, moveSpeed);
                    break;
                case Keys.S:
                    raycaster.StartMove(e, moveSpeed);
                    break;
                case Keys.A:
                    raycaster.StartMove(e, moveSpeed);
                    break;
                case Keys.D:
                    raycaster.StartMove(e, moveSpeed);
                    break;
                case Keys.Escape:
                    Application.Exit();
                    break;
            }
        }
        public void PlayerKeyUpMovement(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    raycaster.StopMove(e);
                    break;
                case Keys.S:
                    raycaster.StopMove(e);
                    break;
                case Keys.A:
                    raycaster.StopMove(e);
                    break;
                case Keys.D:
                    raycaster.StopMove(e);
                    break;
            }
        }
        public void PlayerMouseMove(RaycasterEngine form, ref Point lastMousePosition, float sensitivity)
        {
            raycaster.MouseMove(form, ref lastMousePosition, sensitivity);
        }
    }
}
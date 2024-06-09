using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace _2d_raycaster_project
{
    public partial class RaycasterEngine : Form
    {
        // clientSize
        //const int WIDTH = 640;
        //const int HEIGHT = 480;
        private const int WIDTH = 960;
        private const int HEIGHT = 540;
        private int upscaleFactor = 3;
        

        // graphics
        private Bitmap offScreenBitmap;
        private Graphics offScreenGraphics;
        private Graphics graphics;

        // classes
        private Controller controller;

        // Mouse look variables
        private Point lastMousePosition;
        private bool isMouseCaptured = false;

        // player settings
        private const float MOVE_SPEED = 0.1f; // adjust for player move seed
        private const float MOUSE_SENSITIVITY = 0.002f; // adjust for player mouse speed
        private const float FOV = (float)Math.PI / 3; // still have to find a way to initalize this
        private const float JUMP_STRENGTH = 0; // still have to find a way to implement this

        public RaycasterEngine()
        {
            InitializeComponent();
            this.Width = WIDTH;
            this.Height = HEIGHT;

            // initializing graphics
            offScreenBitmap = new Bitmap(WIDTH / upscaleFactor, HEIGHT / upscaleFactor);
            offScreenGraphics = Graphics.FromImage(offScreenBitmap);
            graphics = this.CreateGraphics();

            // initializing classes
            controller = new Controller(offScreenBitmap, offScreenGraphics, ClientSize);

            // timer settings
            timer1.Interval = 1; // 16 should be approximately 60 FPS, set to 1 for more updates per second
            timer1.Start();

            // Capture the mouse
            CaptureMouse();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            controller.Run();

            // Calculate scaling factors
            float scaleX = (float)this.ClientSize.Width / offScreenBitmap.Width;
            float scaleY = (float)this.ClientSize.Height / offScreenBitmap.Height;
            float scale = Math.Min(scaleX, scaleY);

            // Calculate scaled width and height
            int scaledWidth = (int)(offScreenBitmap.Width * scale);
            int scaledHeight = (int)(offScreenBitmap.Height * scale);

            // Draw the scaled bitmap to the form
            graphics.DrawImage(offScreenBitmap, 0, 0, scaledWidth, scaledHeight);
        }


        private void Form1_KeyDown_1(object sender, KeyEventArgs e)
        {
            controller.PlayerKeyMovement(e, MOVE_SPEED);
        }
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseCaptured)
            {
                controller.PlayerMouseMove(this, ref lastMousePosition, MOUSE_SENSITIVITY);
            }
        }
        private void CaptureMouse()
        {
            Cursor.Hide();
            this.Capture = true;
            lastMousePosition = this.PointToClient(Cursor.Position);
            isMouseCaptured = true;
        }
    }
}

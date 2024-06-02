using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace _2d_raycaster_project
{
    public partial class Form1 : Form
    {
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
        const float MOVE_SPEED = 0.1f; // adjust for player move seed
        const float MOUSE_SENSITIVITY = 0.002f; // adjust for player mouse speed
        const float FOV = (float)Math.PI / 3; // still have to find a way to initalize this

        public Form1()
        {
            InitializeComponent();

            // initializing graphics
            offScreenBitmap = new Bitmap(this.Width, this.Height);
            offScreenGraphics = Graphics.FromImage(offScreenBitmap);
            graphics = this.CreateGraphics();
            this.DoubleBuffered = true;

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
            graphics.DrawImage(offScreenBitmap, 0, 0); // Draw the offscreen bitmap to the form
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

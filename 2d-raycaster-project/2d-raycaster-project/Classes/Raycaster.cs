using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _2d_raycaster_project
{
    public class Raycaster
    {
        private Bitmap _bitmap;
        private Graphics _graphics;
        private Size _clientSize;

        // Player instance
        private Player player;
        private bool isJumping = false;
        private int jumpTime = 0;

        // floor and ceiling variables
        private bool isFloorCeilingInitialized = false;
        private Bitmap floorCeilingBitmap;

        // fps tracker
        private Stopwatch stopwatch = new Stopwatch();
        private int frameCount = 0;
        private double fps = 0.0;
        private const int fpsUpdateInterval = 16; // Update FPS value every 16 frames
        private const float BufferDistance = 0.1f; // Adjust this value as needed
        private float FOV = 2.7f; // Edit this for FOV settings

        private const int MAP_WIDTH = 10;
        private const int MAP_HEIGHT = 10;
        private int[,] map = new int[MAP_WIDTH, MAP_HEIGHT]
        {
                    {1,1,1,1,1,1,1,1,1,1},
                    {2,0,0,0,0,3,0,3,0,2},
                    {2,0,0,0,0,3,0,3,0,2},
                    {2,0,0,0,0,3,0,3,0,2},
                    {2,0,0,0,0,0,0,0,0,2},
                    {2,0,0,0,0,0,0,0,0,2},
                    {2,0,0,0,0,0,0,0,0,2},
                    {2,0,0,0,0,0,0,0,0,2},
                    {2,0,0,0,0,0,0,0,0,2},
                    {1,1,1,1,1,1,1,1,1,1}
        };
        private Dictionary<int, Bitmap> wallTextures = new Dictionary<int, Bitmap>(); // Map wall types to texture images
        public Raycaster(Bitmap bitmap, Graphics graphics, Size clientSize)
        {
            this._bitmap = bitmap;
            this._graphics = graphics;
            this._clientSize = clientSize;
            player = new Player(1.5f, 1.5f, 0.0f, (float)Math.PI / FOV); // Starting at (1.5, 1.5), looking straight ahead, with a 60 degree FOV

            LoadTextures();
        }
        private void LoadTextures()
        {
            wallTextures.Add(1, Properties.Resources.BrickTexture); // Example: Brick texture
            wallTextures.Add(2, Properties.Resources.MossyTexture); // Example: Mossy texture
            wallTextures.Add(3, Properties.Resources.WoodTexture);
            // Add more textures as needed
        }
        private void InitializeFloorCeiling(int screenWidth, int screenHeight)
        {
            floorCeilingBitmap = new Bitmap(screenWidth, screenHeight);
            using (Graphics g = Graphics.FromImage(floorCeilingBitmap))
            {
                // Draw floor and ceiling with gradient
                for (int i = screenHeight / 2; i < screenHeight; i++)
                {
                    // Floor color gradient
                    int gradientFactor = (int)(255 * (i - screenHeight / 2) / (screenHeight / 2));
                    Color floorColor = Color.FromArgb(gradientFactor, gradientFactor, gradientFactor);
                    g.DrawLine(new Pen(floorColor), 0, i, screenWidth, i);

                    // Ceiling color gradient
                    gradientFactor = 255 - gradientFactor;
                    Color ceilingColor = Color.FromArgb(gradientFactor, gradientFactor, 255);
                    g.DrawLine(new Pen(ceilingColor), 0, screenHeight - i, screenWidth, screenHeight - i);
                }
            }
            isFloorCeilingInitialized = true;
        }
        public void Update()
        {
            // clear the screen
            _graphics.FillRectangle(Brushes.Black, 0, 0, _clientSize.Width, _clientSize.Height);

            int screenWidth = _clientSize.Width;
            int screenHeight = _clientSize.Height;

            // Initialize floor and ceiling drawing if not done already
            if (!isFloorCeilingInitialized)
            {
                InitializeFloorCeiling(screenWidth, screenHeight);
            }
            // Draw the pre-rendered floor and ceiling
            _graphics.DrawImage(floorCeilingBitmap, 0, 0);

            // raycasting
            for (int i = 0; i < screenWidth; i++)
            {
                // Calculate ray position and direction
                float cameraX = 2 * i / (float)screenWidth - 1; // x-coordinate in camera space
                float rayDirX = player.DirectionX + player.PlaneX * cameraX;
                float rayDirY = player.DirectionY + player.PlaneY * cameraX;

                // Which box of the map we're in
                int mapX = (int)player.X;
                int mapY = (int)player.Y;   

                // Length of ray from one x or y-side to next x or y-side
                float deltaDistX = Math.Abs(1 / rayDirX);
                float deltaDistY = Math.Abs(1 / rayDirY);

                float sideDistX, sideDistY;

                // What direction to step in x or y-direction (either +1 or -1)
                int stepX, stepY;

                bool hit = false; // Was there a wall hit?
                int side = 0; // Was a NS or a EW wall hit?

                // Calculate step and initial sideDist
                if (rayDirX < 0)
                {
                    stepX = -1;
                    sideDistX = (player.X - mapX) * deltaDistX;
                }
                else
                {
                    stepX = 1;
                    sideDistX = (mapX + 1.0f - player.X) * deltaDistX;
                }
                if (rayDirY < 0)
                {
                    stepY = -1;
                    sideDistY = (player.Y - mapY) * deltaDistY;
                }
                else
                {
                    stepY = 1;
                    sideDistY = (mapY + 1.0f - player.Y) * deltaDistY;
                }

                // Perform DDA
                while (!hit)
                {
                    // Jump to next map square, OR in x-direction, OR in y-direction
                    if (sideDistX < sideDistY)
                    {
                        sideDistX += deltaDistX;
                        mapX += stepX;
                        side = 0;
                    }
                    else
                    {
                        sideDistY += deltaDistY;
                        mapY += stepY;
                        side = 1;
                    }
                    // Check if ray has hit a wall
                    if (map[mapX, mapY] > 0) hit = true;
                }

                // Calculate distance to the point of impact
                float perpWallDist;
                if (side == 0)
                    perpWallDist = (mapX - player.X + (1 - stepX) / 2) / rayDirX;
                else
                    perpWallDist = (mapY - player.Y + (1 - stepY) / 2) / rayDirY;

                // Calculate height of line to draw on screen
                int lineHeight = (int)(screenHeight / perpWallDist);

                // Calculates whether the player is jumping or not, and push the wall rendering down
                // to appear jumping
                int drawStart;
                if (isJumping)
                {
                    jumpTime++;
                    drawStart = screenHeight / 2 + lineHeight / -5;
                    if (jumpTime == 9500)
                    {
                        isJumping = false;
                        jumpTime = 0;
                    }
                }
                else
                {
                    drawStart = -lineHeight / 2 + screenHeight / 2;
                }
             

                // Choose wall color or texture
                Bitmap texture;
                switch (map[mapX, mapY])
                {
                    case 1:
                        texture = wallTextures[1]; // Use the brick texture for this wall type
                        break;
                    case 2:
                        texture = wallTextures[2]; // Use the mossy texture for this wall type
                        break;
                    case 3:
                        texture = wallTextures[3]; // Use the wood texture for this wall type
                        break;
                    default:
                        texture = null; // No texture for this wall type
                        break;
                }

                if (texture != null)
                {
                    // Calculate texture coordinates based on wall hit
                    double wallHitX;
                    if (side == 0) // Ray hits a vertical wall
                    {
                        wallHitX = player.Y + perpWallDist * rayDirY;
                    }
                    else // Ray hits a horizontal wall
                    {
                        wallHitX = player.X + perpWallDist * rayDirX;
                    }
                    wallHitX -= Math.Floor(wallHitX); // Normalize wallHitX to a fraction between 0 and 1

                    // Calculate texture coordinates based on wall hit
                    int texX = (int)(texture.Width * wallHitX);

                    // Define source and destination rectangles for drawing the textured wall
                    Rectangle srcRect = new Rectangle(texX, 0, 1, texture.Height); // Source rectangle from texture
                    Rectangle destRect = new Rectangle(i, drawStart, 1, lineHeight); // Destination rectangle on screen

                    // Draw the textured vertical line
                    _graphics.DrawImage(texture, destRect, srcRect, GraphicsUnit.Pixel);
                }
            }

            // calculate FPS
            frameCount++;
            if (frameCount >= fpsUpdateInterval)
            {
                stopwatch.Stop();
                fps = frameCount / (stopwatch.ElapsedMilliseconds / 1000.0);
                frameCount = 0;
                stopwatch.Restart();
            }
            //draw FPS
            string fpsText = $"FPS: {fps:F0}";
            _graphics.DrawString(fpsText, new Font("Arial", 12), Brushes.White, new PointF(10, 10));

            _graphics.DrawImage(_bitmap, 0, 0);
        }

        // player movement
        private bool IsWallCollision(float x, float y)
        {
            int mapX = (int)x;
            int mapY = (int)y;

            // Check if the player is within the buffer distance of any wall
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (mapX + i >= 0 && mapX + i < MAP_WIDTH && mapY + j >= 0 && mapY + j < MAP_HEIGHT)
                    {
                        if (map[mapX + i, mapY + j] > 0)
                        {
                            float nearestX = Math.Max(mapX + i, Math.Min(x, mapX + i + 1));
                            float nearestY = Math.Max(mapY + j, Math.Min(y, mapY + j + 1));

                            float deltaX = x - nearestX;
                            float deltaY = y - nearestY;

                            if ((deltaX * deltaX + deltaY * deltaY) < (BufferDistance * BufferDistance))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private void PlayerSlide(float x, float y)
        {
            // Determine the nearest wall direction to slide along
            bool xCollision = IsWallCollision(x, player.Y);
            bool yCollision = IsWallCollision(player.X, y);

            if (xCollision && !yCollision)
            {
                // Slide along the Y axis (vertical wall)
                player.Y = y;
            }
            else if (yCollision && !xCollision)
            {
                // Slide along the X axis (horizontal wall)
                player.X = x;
            }
            else if (xCollision && yCollision)
            {
                // Both x and y collisions, stop movement
            }
        }
        public void MoveForward(float distance)
        {
            float newX = player.X + (float)Math.Cos(player.Direction) * distance;
            float newY = player.Y + (float)Math.Sin(player.Direction) * distance;

            if (!IsWallCollision(newX, newY))
            {
                player.X = newX;
                player.Y = newY;
            }
            else
            {
                PlayerSlide(newX, newY);
            }
        }
        public void MoveBackward(float distance)
        {
            float newX = player.X - (float)Math.Cos(player.Direction) * distance;
            float newY = player.Y - (float)Math.Sin(player.Direction) * distance;

            if (!IsWallCollision(newX, newY))
            {
                player.X = newX;
                player.Y = newY;
            }
            else
            {
                PlayerSlide(newX, newY);
            }
        }
        public void MoveLeft(float distance)
        {
            float newX = player.X + (float)Math.Sin(player.Direction) * distance;
            float newY = player.Y - (float)Math.Cos(player.Direction) * distance;

            if (!IsWallCollision(newX, newY))
            {
                player.X = newX;
                player.Y = newY;
            }
            else
            {
                PlayerSlide(newX, newY);
            }
        }

        public void MoveRight(float distance)
        {
            float newX = player.X - (float)Math.Sin(player.Direction) * distance;
            float newY = player.Y + (float)Math.Cos(player.Direction) * distance;

            if (!IsWallCollision(newX, newY))
            {
                player.X = newX;
                player.Y = newY;
            }
            else
            {
                PlayerSlide(newX, newY);
            }
        }
        public void MoveJump()
        {
            isJumping = true;
        }
        public void MouseMove(Form form, ref Point lastMousePosition, float sensitivity)
        {
            Point currentMousePosition = form.PointToClient(Cursor.Position);
            int deltaX = currentMousePosition.X - lastMousePosition.X;

            player.Direction += deltaX * sensitivity;

            // Wrap the player direction to stay within 0 to 2*PI range
            if (player.Direction < 0) player.Direction += 2 * (float)Math.PI;
            if (player.Direction >= 2 * (float)Math.PI) player.Direction -= 2 * (float)Math.PI;

            // Update direction vectors
            player.DirectionX = (float)Math.Cos(player.Direction);
            player.DirectionY = (float)Math.Sin(player.Direction);

            // Update camera plane
            float planeMagnitude = (float)Math.Tan(player.FOV / 2.0f);
            player.PlaneX = -player.DirectionY * planeMagnitude;
            player.PlaneY = player.DirectionX * planeMagnitude;

            // Reset the cursor to the center of the form
            Cursor.Position = form.PointToScreen(new Point(form.Width / 2, form.Height / 2));
            lastMousePosition = form.PointToClient(Cursor.Position);
        }
    }
}

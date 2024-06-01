using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2d_raycaster_project
{
    public class Raycaster
    {
        private Bitmap _bitmap;
        private Graphics _graphics;
        private Size _clientSize;

        // Player instance
        private Player player;

        // fps tracker
        private Stopwatch stopwatch = new Stopwatch();
        private int frameCount = 0;
        private double fps = 0.0;
        private const int fpsUpdateInterval = 10; // Update FPS value every 10 frames

        private const int MAP_WIDTH = 10;
        private const int MAP_HEIGHT = 10;
        private int[,] map = new int[MAP_WIDTH, MAP_HEIGHT]
        {
                    {0,1,1,1,1,1,1,0,1,0},
                    {2,0,0,0,0,0,0,3,0,2},
                    {2,0,0,0,0,0,0,3,0,2},
                    {2,0,0,0,0,0,0,3,0,2},
                    {2,0,0,0,0,0,0,0,0,2},
                    {2,0,0,0,0,0,0,0,0,2},
                    {2,0,0,0,0,0,0,0,0,2},
                    {2,0,0,0,0,0,0,0,0,2},
                    {2,0,0,0,0,0,0,0,0,2},
                    {0,1,1,1,1,1,1,1,1,0}
        };
        private Dictionary<int, Bitmap> wallTextures = new Dictionary<int, Bitmap>(); // Map wall types to texture images
        public Raycaster(Bitmap bitmap, Graphics graphics, Size clientSize)
        {
            this._bitmap = bitmap;
            this._graphics = graphics;
            this._clientSize = clientSize;
            player = new Player(5.0f, 5.0f, 0.0f, (float)Math.PI / 3f); // Initialize the player
            LoadTextures();
        }
        private void LoadTextures()
        {
            wallTextures.Add(1, Properties.Resources.BrickTexture); // Example: Brick texture
            wallTextures.Add(2, Properties.Resources.MossyTexture); // Example: Mossy texture
            wallTextures.Add(3, Properties.Resources.WoodTexture);
            // Add more textures as needed
        }
        public void Update()
        {
            // clear the screen
            _graphics.FillRectangle(Brushes.Black, 0, 0, _clientSize.Width, _clientSize.Height);

            int screenWidth = _clientSize.Width;
            int screenHeight = _clientSize.Height;

            // Floor rendering
            _graphics.FillRectangle(Brushes.Gray, 0, _clientSize.Height / 2, _clientSize.Width, _clientSize.Height / 2);

            // ceiling rendering
            _graphics.FillRectangle(Brushes.LightBlue, 0, 0, _clientSize.Width, _clientSize.Height / 2);

            // raycasting
            for (int i = 0; i < screenWidth; i++)
            {
                // Calculate ray position and direction
                float rayDirX = (float)Math.Cos(player.Direction - player.FOV / 2.0f + i * player.FOV / screenWidth);
                float rayDirY = (float)Math.Sin(player.Direction - player.FOV / 2.0f + i * player.FOV / screenWidth);

                // Which box of the map we're in
                int mapX = (int)player.X;
                int mapY = (int)player.Y;

                // Length of ray from one x or y-side to next x or y-side
                float deltaDistX = Math.Abs(1 / rayDirX);
                float deltaDistY = Math.Abs(1 / rayDirY);

                float sideDistX;
                float sideDistY;

                // What direction to step in x or y-direction (either +1 or -1)
                int stepX;
                int stepY;

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
                if (side == 0) perpWallDist = (sideDistX - deltaDistX);
                else perpWallDist = (sideDistY - deltaDistY);

                // Calculate height of line to draw on screen
                int lineHeight = (int)(screenHeight / perpWallDist);

                // Calculate lowest and highest pixel to fill in current stripe
                int drawStart = -lineHeight / 2 + screenHeight / 2;
                if (drawStart < 0) drawStart = 0;
                int drawEnd = lineHeight / 2 + screenHeight / 2;
                if (drawEnd >= screenHeight) drawEnd = screenHeight - 1;

                // Choose wall color or texture
                Color color;
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
                        color = Color.White; // Default color
                        break;
                }

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
                if (texture != null)
                {
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
        public void MoveForward(float distance)
        {
            player.MoveForward(distance);
        }

        public void MoveBackward(float distance)
        {
            player.MoveBackward(distance);
        }

        public void RotateLeft(float angle)
        {
            player.RotateLeft(angle);
        }

        public void RotateRight(float angle)
        {
            player.RotateRight(angle);
        }
    }
}

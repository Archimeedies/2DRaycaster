using _2d_raycaster_project.Classes;
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
        private float moveSpeed; // Adjust speed as needed
        private Dictionary<Keys, bool> keyStates = new Dictionary<Keys, bool>
        {
            { Keys.W, false },
            { Keys.S, false },
            { Keys.A, false },
            { Keys.D, false },
        };

        // sprites
        private List<Sprite> sprites = new List<Sprite>();

        // ceiling variables
        private bool isCeilingInitialized = false;
        private Bitmap ceilingBitmap;

        // fps tracker
        private Stopwatch stopwatch = new Stopwatch();
        private int frameCount = 0;
        private double fps = 0.0;
        private const int fpsUpdateInterval = 16; // Update FPS value every 16 frames
        private const float BufferDistance = 0.1f; // Adjust this value as needed
        private float FOV = 2.7f; // Edit this for FOV settings

        private const int MAP_WIDTH = 10;
        private const int MAP_HEIGHT = 11;
        private int[,] map = new int[MAP_WIDTH, MAP_HEIGHT]
        {
                    {0,1,4,4,1,0,1,0,1,1,0},
                    {2,0,0,0,0,3,0,3,0,0,5},
                    {2,0,0,0,0,3,0,3,0,0,5},
                    {2,0,0,0,0,3,0,3,0,0,5},
                    {2,0,0,0,0,0,0,0,0,0,5},
                    {2,0,0,0,0,0,0,0,0,0,5},
                    {2,0,0,0,0,0,0,0,0,0,5},
                    {2,0,0,0,0,0,0,0,0,0,5},
                    {2,0,0,0,0,0,0,0,0,0,5},
                    {0,6,6,6,6,6,6,6,6,6,0}
        };
        private Dictionary<int, Bitmap> wallTextures = new Dictionary<int, Bitmap>(); // Map wall types to texture images
        public Raycaster(Bitmap bitmap, Graphics graphics, Size clientSize)
        {
            this._graphics = graphics;
            this._clientSize = clientSize;
            this._bitmap = bitmap;
            player = new Player(1.5f, 1.5f, 0.0f, (float)Math.PI / FOV); // Starting at (1.5, 1.5), looking straight ahead, with a 60 degree FOV

            LoadTextures();
            LoadSprites();
        }
        private void LoadTextures()
        {
            wallTextures.Add(1, Properties.Resources.redbrick); // Example: Brick texture
            wallTextures.Add(2, Properties.Resources.mossy); // Example: Mossy texture
            wallTextures.Add(3, Properties.Resources.wood);
            wallTextures.Add(4, Properties.Resources.eagle);
            wallTextures.Add(5, Properties.Resources.bluestone);
            wallTextures.Add(6, Properties.Resources.greystone);
            wallTextures.Add(7, Properties.Resources.colorstone);
            // Add more textures as needed
        }
        private void LoadSprites()
        {
            Bitmap barrelTexture = Properties.Resources.newbarrel; // Example texture
            sprites.Add(new Sprite(1.5f, 6.5f, barrelTexture)); // Add more sprites as needed, first to floats are where the sprite spawns in the map
        }
        public void Update()
        {
            // clear the screen
            _graphics.FillRectangle(Brushes.Black, 0, 0, _clientSize.Width, _clientSize.Height);

            int screenWidth = _clientSize.Width;
            int screenHeight = _clientSize.Height;


            // Initialize ceiling drawing if not done already
            if (!isCeilingInitialized)
            {
                // rendering ceiling
                RenderCeiling(screenWidth, screenHeight);
            }
            // Draw the pre-rendered floor and ceiling
            _graphics.DrawImage(ceilingBitmap, 0, 0);

            // rendering floors
            RenderFloor(screenWidth, screenHeight);


            // rendering walls
            RenderWalls(screenWidth, screenHeight);

            // UNCOMMENT TO Render sprites
            RenderSprites(screenWidth, screenHeight);

            // update player movements
            UpdatePlayerMovement();

            // update FPS
            CalculateFPS();

            _graphics.DrawImage(_bitmap, 0, 0);
        }
        private void RenderCeiling(int screenWidth, int screenHeight)
        {
            ceilingBitmap = new Bitmap(screenWidth, screenHeight);
            using (Graphics g = Graphics.FromImage(ceilingBitmap))
            {
                // Draw ceiling with gradient
                for (int i = screenHeight / 2; i < screenHeight; i++)
                {
                    // Ceiling color gradient
                    int gradientFactor = (int)(255 * (i - screenHeight / 2) / (screenHeight / 2));
                    gradientFactor = 255 - gradientFactor;
                    Color ceilingColor = Color.FromArgb(gradientFactor, gradientFactor, 255);
                    g.DrawLine(new Pen(ceilingColor), 0, screenHeight - i, screenWidth, screenHeight - i);
                }
            }
            isCeilingInitialized = true;
        }
        private void RenderFloor(int screenWidth, int screenHeight)
        {
            for (int y = screenHeight / 2; y < screenHeight; y++)
            {
                // rayDir for leftmost ray (x = 0) and rightmost ray (x = screenWidth)
                float rayDirX0 = player.DirectionX - player.PlaneX;
                float rayDirY0 = player.DirectionY - player.PlaneY;
                float rayDirX1 = player.DirectionX + player.PlaneX;
                float rayDirY1 = player.DirectionY + player.PlaneY;

                // Current y position compared to the center of the screen (the horizon)
                int p = y - screenHeight / 2;

                // Vertical position of the camera.
                float posZ = (float)0.5 * screenHeight;

                // Horizontal distance from the camera to the floor for the current row.
                // 0.5 is the z position exactly in the middle between floor and ceiling.
                float rowDistance = posZ / p;

                // calculate the real world step vector we have to add for each x (parallel to camera plane)
                // adding step by step avoids multiplications with a weight in the inner loop
                float floorStepX = rowDistance * (rayDirX1 - rayDirX0) / screenWidth;
                float floorStepY = rowDistance * (rayDirY1 - rayDirY0) / screenWidth;

                // real world coordinates of the leftmost column. This will be updated as we step to the right.
                float floorX = player.X + rowDistance * rayDirX0;
                float floorY = player.Y + rowDistance * rayDirY0;

                for (int x = 0; x < screenWidth; x++)
                {
                    // the cell coord is simply got from the integer parts of floorX and floorY
                    int cellX = (int)floorX;
                    int cellY = (int)floorY;

                    // Choose a floor texture. For now, we use the same as wall texture 4
                    Bitmap floorTexture = wallTextures[7];

                    // get the texture coordinate from the fractional part
                    int tx = (int)(floorTexture.Width * (floorX - cellX)) & (floorTexture.Width - 1);
                    int ty = (int)(floorTexture.Height * (floorY - cellY)) & (floorTexture.Height - 1);

                    floorX += floorStepX;
                    floorY += floorStepY;

                    // Get the color from the texture
                    Color color = floorTexture.GetPixel(tx, ty);

                    // Draw the pixel on the bitmap
                    _bitmap.SetPixel(x, y, color);
                }
            }
        }
        private void RenderWalls(int screenWidth, int screenHeight)
        {
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
                // Calculate the drawing start and end positions for the wall, considering whether the player is jumping or not
                int drawStart;
                int drawEnd;

                // Normal drawing positions when not jumping
                drawStart = -lineHeight / 2 + screenHeight / 2;
                if (drawStart < 0) drawStart = 0; // Ensure the draw start is not above the screen
                drawEnd = lineHeight / 2 + screenHeight / 2;
                if (drawEnd >= screenHeight) drawEnd = screenHeight - 1; // Ensure the draw end is not below the screen

                // Choose the wall texture based on the map value at the current position
                int texNum = map[mapX, mapY];
                Bitmap texture = wallTextures[texNum];

                int texWidth = texture.Width;
                int texHeight = texture.Height;

                // Calculate the exact position on the wall (x-coordinate) where the ray hits
                float wallX = (side == 0) ? player.Y + perpWallDist * rayDirY : player.X + perpWallDist * rayDirX;
                wallX -= (float)Math.Floor(wallX);

                // Calculate the x-coordinate on the texture corresponding to the wall hit position
                int texX = (int)(wallX * (float)texWidth);
                if (side == 0 && rayDirX > 0) texX = texWidth - texX - 1;
                if (side == 1 && rayDirY < 0) texX = texWidth - texX - 1;

                // Loop through each pixel from drawStart to drawEnd on the current vertical stripe
                for (int y = drawStart; y < drawEnd; y++)
                {
                    // Calculate the corresponding y-coordinate on the texture
                    int d = y * 256 - screenHeight * 128 + lineHeight * 128;
                    int texY = ((d * texHeight) / lineHeight) / 256;

                    // Get the color from the texture at the calculated (texX, texY) position
                    Color color = texture.GetPixel(texX, texY);

                    // Darken the color for sides perpendicular to the ray direction to create a shading effect
                    if (side == 1)
                    {
                        color = Color.FromArgb(color.R / 2, color.G / 2, color.B / 2);
                    }

                    // Set the pixel color on the bitmap at the current stripe (i) and y-coordinate
                    _bitmap.SetPixel(i, y, color);
                }

            }
        }
        private void RenderSprites(int screenWidth, int screenHeight)
        {
            // Sort sprites by distance from player (farthest to closest)
            var sortedSprites = sprites.OrderByDescending(s => Math.Pow(player.X - s.X, 2) + Math.Pow(player.Y - s.Y, 2)).ToList();

            foreach (var sprite in sortedSprites)
            {
                // Calculate sprite position relative to the player
                float spriteX = sprite.X - player.X;
                float spriteY = sprite.Y - player.Y;

                // Perform visibility and occlusion checks here
                if (!IsSpriteVisible(spriteX, spriteY))
                {
                    continue; // Skip rendering if the sprite is not visible
                }

                // Inverse camera matrix to transform sprite position to camera space
                float invDet = 1.0f / (player.PlaneX * player.DirectionY - player.DirectionX * player.PlaneY);
                float transformX = invDet * (player.DirectionY * spriteX - player.DirectionX * spriteY);
                float transformY = invDet * (-player.PlaneY * spriteX + player.PlaneX * spriteY);

                int spriteScreenX = (int)((screenWidth / 2) * (1 + transformX / transformY));

                // Calculate height and width of the sprite on screen
                int spriteHeight = Math.Abs((int)(screenHeight / transformY));
                int spriteWidth = spriteHeight; // Assume square sprites for simplicity

                int drawStartY = -spriteHeight / 2 + screenHeight / 2;
                if (drawStartY < 0) drawStartY = 0;
                int drawEndY = spriteHeight / 2 + screenHeight / 2;
                if (drawEndY >= screenHeight) drawEndY = screenHeight - 1;

                int drawStartX = -spriteWidth / 2 + spriteScreenX;
                if (drawStartX < 0) drawStartX = 0;
                int drawEndX = spriteWidth / 2 + spriteScreenX;
                if (drawEndX >= screenWidth) drawEndX = screenWidth - 1;

                // Draw the sprite
                Rectangle srcRect = new Rectangle(0, 0, sprite.Texture.Width, sprite.Texture.Height);
                Rectangle destRect = new Rectangle(drawStartX, drawStartY, drawEndX - drawStartX, drawEndY - drawStartY);
                _graphics.DrawImage(sprite.Texture, destRect, srcRect, GraphicsUnit.Pixel);
            }
        }
        private bool IsSpriteVisible(float spriteX, float spriteY)
        {
            // Calculate the angle between the player's direction and the sprite
            float angleToSprite = (float)(Math.Atan2(spriteY, spriteX) - Math.Atan2(player.DirectionY, player.DirectionX));
            angleToSprite = (angleToSprite + (float)(Math.PI * 2)) % ((float)Math.PI * 2); // Normalize angle to [0, 2π]

            // Perform raycasting to check if the sprite is obstructed by walls
            float distanceToSprite = (float)Math.Sqrt(spriteX * spriteX + spriteY * spriteY);
            float rayDirX = spriteX / distanceToSprite;
            float rayDirY = spriteY / distanceToSprite;

            float stepSize = 0.1f; // Adjust as needed for accuracy vs performance

            // Start at player position and incrementally check along the ray direction
            float rayX = player.X;
            float rayY = player.Y;
            float rayDistance = 0;

            while (rayDistance < distanceToSprite)
            {
                rayX += rayDirX * stepSize;
                rayY += rayDirY * stepSize;
                rayDistance += stepSize;

                // Check if the current position intersects with a wall
                int mapX = (int)Math.Floor(rayX);
                int mapY = (int)Math.Floor(rayY);

                if (mapX >= 0 && mapX < MAP_WIDTH && mapY >= 0 && mapY < MAP_HEIGHT && map[mapX, mapY] > 0)
                {
                    return false; // Sprite is obstructed by a wall
                }
            }

            return true; // Sprite is visible and not obstructed
        }

        private void CalculateFPS()
        {
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
            _graphics.DrawString(fpsText, new Font("Arial", 12 / 2), Brushes.White, new PointF(5, 5));
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
        public void StartMove(KeyEventArgs e, float moveSpeed)
        {
            this.moveSpeed = moveSpeed;
            if (keyStates.ContainsKey(e.KeyCode))
            {
                keyStates[e.KeyCode] = true;
            }
        }
        public void StopMove(KeyEventArgs e)
        {
            if (keyStates.ContainsKey(e.KeyCode))
            {
                keyStates[e.KeyCode] = false;
            }
        }
        private void UpdatePlayerMovement()
        {
            if (keyStates[Keys.W])
            {
                Move(moveSpeed, 1);
            }
            if (keyStates[Keys.S])
            {
                Move(moveSpeed, 2);
            }
            if (keyStates[Keys.A])
            {
                Move(moveSpeed, 3);
            }
            if (keyStates[Keys.D])
            {
                Move(moveSpeed, 4);
            }
        }
        private void Move(float distance, int moveOption)
        {
            float newX = 0;
            float newY = 0;
            switch (moveOption)
            {
                case 1:
                    newX = player.X + (float)Math.Cos(player.Direction) * distance;
                    newY = player.Y + (float)Math.Sin(player.Direction) * distance;
                    break;
                case 2:
                    newX = player.X - (float)Math.Cos(player.Direction) * distance;
                    newY = player.Y - (float)Math.Sin(player.Direction) * distance;
                    break;
                case 3:
                    newX = player.X + (float)Math.Sin(player.Direction) * distance;
                    newY = player.Y - (float)Math.Cos(player.Direction) * distance;
                    break;
                case 4:
                    newX = player.X - (float)Math.Sin(player.Direction) * distance;
                    newY = player.Y + (float)Math.Cos(player.Direction) * distance;
                    break;
            }

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
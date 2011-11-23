#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Text;
using System.Threading;
using System.Collections;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace RabiesX
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    /// 
    class GameplayScreen : GameScreen
    {
        #region Fields

        private const float PLAYER_FORWARD_SPEED = 120.0f;
        private const float PLAYER_HEADING_SPEED = 120.0f;
        private const float PLAYER_ROLLING_SPEED = 280.0f;

        private const float TERRAIN_WIDTH = 1258.0f;
        private const float TERRAIN_HEIGHT = 1258.0f;

        private const float CAMERA_FOVX = 80.0f;
        private const float CAMERA_ZFAR = TERRAIN_WIDTH * 2.0f;
        private const float CAMERA_ZNEAR = 1.0f;
        private const float CAMERA_MAX_SPRING_CONSTANT = 100.0f;
        private const float CAMERA_MIN_SPRING_CONSTANT = 1.0f;

        private const int MAX_TIME_LEFT = 300;
        private int timeLeft = MAX_TIME_LEFT;
        int elapsedUpdateTime = 0;

        private int TOTAL_RABID_DOGS = 3;

        ContentManager content;
        SpriteFont gameFont;

        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;

        Vector2 playerPosition = new Vector2(100, 100);
        Vector2 enemyPosition = new Vector2(100, 100);

        // Set triangle indicator for level.
        Effect effect;
        Vector3 indicatorPos;
        int indicatorScale = 5;
        private List<VertexPositionColor[]> vertices;

        private float angle = 0.0f;

        // Set bullets for level;
        Quad quad;
        Effect quadEffect;
        struct Bullet
        {
            public Vector3 position;
            public Quaternion rotation;
        }
        List<Bullet> bulletList = new List<Bullet> ();
        double lastBulletTime = 0;
        float bulletSpeed = 1.0f;
        Texture2D bullets;

        // Set health bar for level.
        SpriteBatch mBatch;
        Texture2D mHealthBar;

        // Set current health for level.
        int mCurrentHealth = 100;
        float playerHealthDecr = 0.0f;
        
        // Set sky and terrain for level.
        Model terrain;
        Sky sky;

        // Set the 3D model to draw.
        private MyModel playerModel;
        private List<MyModel> rabidDogModels;

        // Aspect ratio determines how to scale 3d to 2d projection.
        float aspectRatio;

        Random random = new Random();

        float pauseAlpha;

        int screenWidth;
        int screenHeight;
        
        private Vector2 fontPos;
        private int frames;
        private int framesPerSecond;
        private Entity playerEntity;
        private Entity terrainEntity;
        private List<int> rabidDogHealths;
        private List<float> rabidDogHealthDecrs;
        private List<Entity> rabidDogEntities;
        private List<Vector3> rabidDogPreviousPositions;
        private float playerRadius;
        private float terrainRadius;
        private List<float> rabidDogRadii;
        private Matrix[] modelTransforms;
        private List<Matrix[]> modelEnemyTransforms;
        private TimeSpan elapsedTime = TimeSpan.Zero;
        private TimeSpan prevElapsedTime = TimeSpan.Zero;
        private bool displayHelp;

        private ThirdPersonCamera camera;
        private Vector3 cameraUpDirection;

        private MouseState curMouseState;
        private MouseState prevMouseState;
        private KeyboardState curKeyboardState;
        private KeyboardState prevKeyboardState;

        BoundingSphere playerBounds;
        BoundingSphere terrainBounds;
        List<BoundingSphere> rabidDogBounds;

        private bool flicker;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
            #if WINDOWS_PHONE
                        // Frame rate is 30 fps by default for Windows Phone.
                        ScreenManager.Game.TargetElapsedTime = TimeSpan.FromTicks(333333);

                        graphics.IsFullScreen = true;
            #endif
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            aspectRatio = (float)ScreenManager.Game.GraphicsDevice.Viewport.Width / (float)ScreenManager.Game.GraphicsDevice.Viewport.Height;

            // Initialize flicker for when health bar on danger.
            flicker = true;

            // Get current screen width and height.
            screenWidth = ScreenManager.Game.GraphicsDevice.Viewport.Width;
            screenHeight = ScreenManager.Game.GraphicsDevice.Viewport.Height;

            // Setup frame buffer.
            GameStateManagementGame.graphics.SynchronizeWithVerticalRetrace = false;
            GameStateManagementGame.graphics.PreferredBackBufferWidth = screenWidth;
            GameStateManagementGame.graphics.PreferredBackBufferHeight = screenHeight;
            GameStateManagementGame.graphics.PreferMultiSampling = true;
            GameStateManagementGame.graphics.ApplyChanges();

            // Position the in-game text.
            fontPos = new Vector2(1.0f, 1.0f);

            // Setup the initial input states.
            curMouseState = Mouse.GetState();
            curKeyboardState = Keyboard.GetState();

            // Load the game font.
            gameFont = content.Load<SpriteFont>("Fonts\\gamefont");

            // Initialize the sprite batch for the in-game font.
            spriteBatch = new SpriteBatch(ScreenManager.GraphicsDevice);

            // Load the in-game font.
            spriteFont = content.Load<SpriteFont>("Fonts\\ingamefont");

            // Initialize the sprite batch for the health bar.
            mBatch = new SpriteBatch(ScreenManager.GraphicsDevice);

            // Load the health bar image.
            mHealthBar = content.Load<Texture2D>("healthbar");
            
            // Load the bullet image and setup bullet effects.
            quadEffect = content.Load<Effect>("Effects\\effects");
            quad = new Quad(ScreenManager.GraphicsDevice, Vector3.Zero, Vector3.Up, screenWidth, screenHeight);
            bullets = content.Load<Texture2D>("bullet");

            // Load the effects and setup vertices for triangle indicator.
            vertices = new List<VertexPositionColor[]>();
            effect = content.Load<Effect>("Effects\\effects");
            indicatorPos = new Vector3(5, 45, 0);
            SetUpVertices(indicatorPos, indicatorScale);

            // Intialize lists for enemies.
            rabidDogHealths = new List<int>();
            rabidDogRadii = new List<float>();
            rabidDogModels = new List<MyModel>();
            rabidDogEntities = new List<Entity>();
            rabidDogHealthDecrs = new List<float>();
            rabidDogBounds = new List<BoundingSphere>();
            modelEnemyTransforms = new List<Matrix[]>();
            rabidDogPreviousPositions = new List<Vector3>();
            for (int i = 0; i < TOTAL_RABID_DOGS; i++)
            {
                rabidDogHealths.Add(100);
                rabidDogHealthDecrs.Add(0.0f);
                modelEnemyTransforms.Add(null);
                rabidDogPreviousPositions.Add(new Vector3());
            }

            // Load models and set aspect ratio.
            playerModel = new MyModel("Models\\ball", content);
            playerModel.Texture("Textures\\wedge_p1_diff_v1", content);
            for (int i = 0; i < TOTAL_RABID_DOGS; i++)
            {
                rabidDogModels.Add(new MyModel("Models\\ball", content));
                rabidDogModels[i].Texture("Textures\\wedge_p1_diff_v1", content);
            }

            // Load terrain and sky.
            terrain = content.Load<Model>("terrain");
            sky = content.Load<Sky>("sky");

            // Determine the radius of the player model.           
            BoundingSphere bounds = new BoundingSphere();
            foreach (ModelMesh mesh in playerModel.ModelHeld.Meshes)
                bounds = BoundingSphere.CreateMerged(bounds, mesh.BoundingSphere);
            playerRadius = bounds.Radius;

            playerBounds = bounds;

            // Determine the radii of the enemy models.  
            for (int i = 0; i < TOTAL_RABID_DOGS; i++)
            {
                BoundingSphere rbounds = new BoundingSphere();
                foreach (ModelMesh mesh in rabidDogModels[i].ModelHeld.Meshes)
                    rbounds = BoundingSphere.CreateMerged(rbounds, mesh.BoundingSphere);
                rabidDogRadii.Add(rbounds.Radius);

                rabidDogBounds.Add(rbounds);
            }

            // Determine the radius of the height map.           
            BoundingSphere tbounds = new BoundingSphere();
            foreach (ModelMesh mesh in terrain.Meshes)
                tbounds = BoundingSphere.CreateMerged(tbounds, mesh.BoundingSphere);
            terrainRadius = tbounds.Radius;

            terrainBounds = tbounds;

            // Setup the camera.
            camera = new ThirdPersonCamera();
            camera.Perspective(CAMERA_FOVX, (float)screenWidth / (float)screenHeight,
                CAMERA_ZNEAR, CAMERA_ZFAR);
            camera.LookAt(new Vector3(0.0f, playerRadius * 3.0f, playerRadius * 7.0f),
                Vector3.Zero, Vector3.Up);

            // Setup the player entity.
            playerEntity = new Entity();
            playerEntity.ConstrainToWorldYAxis = true;
            playerEntity.Position = new Vector3(0.0f, 1.0f + playerRadius, 0.0f);
            
            // Setup the enemy entities.
            for (int i = 0; i < TOTAL_RABID_DOGS; i++)
            {
                rabidDogEntities.Add(new Entity());
                rabidDogEntities[i].ConstrainToWorldYAxis = true;
                rabidDogEntities[i].Position = new Vector3(random.Next(1000) * -1.0f + 200.0f, 1.0f + rabidDogRadii[i], random.Next(500));
            }

            // Setup the terrain entity.
            terrainEntity = new Entity();
            terrainEntity.ConstrainToWorldYAxis = true;
            terrainEntity.Position = new Vector3(0.0f, 1.0f + terrainRadius, 0.0f);

            // A real game would probably have more content than this sample, so
            // it would take longer to load. We simulate that by delaying for a
            // while, giving you a chance to admire the beautiful loading screen.
            Thread.Sleep(3000);

            // Once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }

        private void SetUpVertices(Vector3 pos, int scale)
        {
            Vector3 vertex1 = new Vector3(pos.X - scale, pos.Y - scale, pos.Z + 2*scale);
            Vector3 vertex2 = new Vector3(pos.X + scale, pos.Y + scale, pos.Z);
            Vector3 vertex3 = new Vector3(pos.X + scale, pos.Y - scale, pos.Z + 2*scale);
            Vector3 vertex4 = new Vector3(pos.X - scale, pos.Y - scale, pos.Z);

            vertices.Add(new VertexPositionColor[3]); // front face
            vertices[0][0] = new VertexPositionColor(vertex1, Color.Blue); // bottom-left
            vertices[0][1] = new VertexPositionColor(vertex2, Color.Red); // top vertex
            vertices[0][2] = new VertexPositionColor(vertex3, Color.Green); // bottom-right

            vertices.Add(new VertexPositionColor[3]); // left face
            vertices[1][0] = new VertexPositionColor(vertex4, Color.Blue); // bottom-left
            vertices[1][1] = new VertexPositionColor(vertex2, Color.Red); // top vertex
            vertices[1][2] = new VertexPositionColor(vertex1, Color.Green); // bottom-right

            vertices.Add(new VertexPositionColor[3]); // right face
            vertices[2][0] = new VertexPositionColor(vertex3, Color.Blue); // bottom-left
            vertices[2][1] = new VertexPositionColor(vertex2, Color.Red); // top vertex
            vertices[2][2] = new VertexPositionColor(vertex4, Color.Green); // bottom-right

            vertices.Add(new VertexPositionColor[3]); // bottom face
            vertices[3][0] = new VertexPositionColor(vertex1, Color.Blue); // bottom-left
            vertices[3][1] = new VertexPositionColor(vertex4, Color.Red); // top vertex
            vertices[3][2] = new VertexPositionColor(vertex3, Color.Green); // bottom-right
        }

        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
                pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
            else
                pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);

            if (IsActive)
            {
                float moveSpeed = gameTime.ElapsedGameTime.Milliseconds / 500.0f * bulletSpeed;

                ProcessKeyboard();
                UpdatePlayer(gameTime);
                UpdateEnemies(gameTime);
                UpdateFrameRate(gameTime);
                UpdateBulletPositions(moveSpeed);

                // Rotate triangle level indicator.
                angle += 0.005f;

                // Apply some random jitter to make the enemy move around.
                const float randomization = 10;

                enemyPosition.X += (float)(random.NextDouble() - 0.5) * randomization;
                enemyPosition.Y += (float)(random.NextDouble() - 0.5) * randomization;

                // Apply a stabilizing force to stop the enemy moving off the screen.
                Vector2 targetPosition = new Vector2(
                    ScreenManager.GraphicsDevice.Viewport.Width / 2 - gameFont.MeasureString("Insert Gameplay Here").X / 2, 
                    200);

                enemyPosition = Vector2.Lerp(enemyPosition, targetPosition, 0.05f);
                
                base.Update(gameTime, otherScreenHasFocus, false);

                // TODO: this game isn't very fun! You could probably improve
                // it by inserting something more interesting in this space :-)
            }
        }
        
        private bool KeyJustPressed(Keys key)
        {
            return curKeyboardState.IsKeyDown(key) && prevKeyboardState.IsKeyUp(key);
        }

        private void ProcessKeyboard()
        {
            prevKeyboardState = curKeyboardState;
            curKeyboardState = Keyboard.GetState();
            
            if (KeyJustPressed(Keys.H))
                displayHelp = !displayHelp;

            if (KeyJustPressed(Keys.Space))
                camera.EnableSpringSystem = !camera.EnableSpringSystem;

            if (curKeyboardState.IsKeyDown(Keys.LeftAlt) ||
                curKeyboardState.IsKeyDown(Keys.RightAlt))
            {
                if (KeyJustPressed(Keys.Enter))
                    ToggleFullScreen();
            }

            if (KeyJustPressed(Keys.Add))
            {
                float springConstant = camera.SpringConstant + 0.1f;

                springConstant = Math.Min(CAMERA_MAX_SPRING_CONSTANT, springConstant);
                camera.SpringConstant = springConstant;
            }

            if (KeyJustPressed(Keys.Subtract))
            {
                float springConstant = camera.SpringConstant - 0.1f;

                springConstant = Math.Max(CAMERA_MIN_SPRING_CONSTANT, springConstant);
                camera.SpringConstant = springConstant;
            }
        }

        private void ToggleFullScreen()
        {
            int newWidth = 0;
            int newHeight = 0;

            GameStateManagementGame.graphics.IsFullScreen = !GameStateManagementGame.graphics.IsFullScreen;

            if (GameStateManagementGame.graphics.IsFullScreen)
            {
                newWidth = ScreenManager.GraphicsDevice.DisplayMode.Width;
                newHeight = ScreenManager.GraphicsDevice.DisplayMode.Height;
            }
            else
            {
                newWidth = screenWidth;
                newHeight = screenHeight;
            }

            GameStateManagementGame.graphics.PreferredBackBufferWidth = newWidth;
            GameStateManagementGame.graphics.PreferredBackBufferHeight = newHeight;
            GameStateManagementGame.graphics.ApplyChanges();

            camera.Perspective(CAMERA_FOVX, (float)newWidth / (float)newHeight,
                CAMERA_ZNEAR, CAMERA_ZFAR);

            // Set current screen width and height.
            screenWidth = newWidth;
            screenHeight = newHeight;
        }

        private void UpdatePlayer(GameTime gameTime)
        {
            float pitch = 0.0f;
            float heading = 0.0f;
            float forwardSpeed = 0.0f;
            Vector3 playerPrevPosition = playerEntity.Position;
            curMouseState = new MouseState();

            if (curKeyboardState.IsKeyDown(Keys.W) ||
                curKeyboardState.IsKeyDown(Keys.Up))
            {
                forwardSpeed = PLAYER_FORWARD_SPEED;
                pitch = -PLAYER_ROLLING_SPEED;
            }

            if (curKeyboardState.IsKeyDown(Keys.S) ||
                curKeyboardState.IsKeyDown(Keys.Down))
            {
                forwardSpeed = -PLAYER_FORWARD_SPEED;
                pitch = PLAYER_ROLLING_SPEED;
            }

            if (curKeyboardState.IsKeyDown(Keys.D) ||
                curKeyboardState.IsKeyDown(Keys.Right))
            {
                heading = -PLAYER_HEADING_SPEED;
            }

            if (curKeyboardState.IsKeyDown(Keys.A) ||
                curKeyboardState.IsKeyDown(Keys.Left))
            {
                heading = PLAYER_HEADING_SPEED;
            }

            if (((curMouseState.LeftButton == ButtonState.Pressed) && (prevMouseState.LeftButton == ButtonState.Released)) || curKeyboardState.IsKeyDown(Keys.RightControl))
            {
                double currentTime = gameTime.TotalGameTime.TotalMilliseconds;
                if ((currentTime - lastBulletTime) > 100)
                {
                    Bullet newBullet = new Bullet();
                    newBullet.position = playerEntity.Position;
                    newBullet.rotation = playerEntity.Rotation;
                    bulletList.Add(newBullet);

                    lastBulletTime = currentTime;
                }
            }
            prevMouseState = curMouseState; 

            // Prevent the player from moving off the edge of the floor.
            float floorBoundaryZ = TERRAIN_HEIGHT * 0.5f - playerRadius;
            float floorBoundaryX = TERRAIN_WIDTH * 0.5f - playerRadius;
            float elapsedTimeSec = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float velocity = forwardSpeed * elapsedTimeSec;
            Vector3 newPlayerPos = playerEntity.Position + playerEntity.Forward * velocity;

            if (newPlayerPos.Z > floorBoundaryZ)
                forwardSpeed = 0.0f;
            else if (newPlayerPos.Z < -floorBoundaryZ)
                forwardSpeed = 0.0f;
            else if (newPlayerPos.X > floorBoundaryX)
                forwardSpeed = 0.0f;
            else if (newPlayerPos.X < -floorBoundaryX)
                forwardSpeed = 0.0f;

            // Update the player's state.
            playerEntity.Velocity = new Vector3(0.0f, 0.0f, forwardSpeed);
            playerEntity.Orient(heading, 0.0f, 0.0f);
            playerEntity.Rotate(0.0f, pitch, 0.0f);
            playerEntity.Update(gameTime);

            // Then move the camera based on where the player has moved to.
            // When the player is moving backwards rotations are inverted to
            // match the direction of travel. Consequently the camera's
            // rotation needs to be inverted as well.

            Vector3 camup = new Vector3(0, 1, 0);
            camup = Vector3.Transform(camup, Matrix.CreateFromQuaternion(playerEntity.Rotation));
            cameraUpDirection = camup;
            camera.Rotate((forwardSpeed >= 0.0f) ? heading : -heading, 0.0f);
            camera.LookAt(playerEntity.Position);
            camera.Update(gameTime);

            bool enemiesDeadChk = true;
            // keep player from colliding with enemies and decrease health if collision
            for (int k = 0; k < TOTAL_RABID_DOGS; k++)
            {
                if ((((Math.Abs(playerEntity.Position.X - rabidDogEntities[k].Position.X) + Math.Abs(playerEntity.Position.Z - rabidDogEntities[k].Position.Z)) / 2) < (playerRadius + rabidDogRadii[k] - 19.5)))
                {
                    playerEntity.Position = playerPrevPosition;
                    playerHealthDecr += 0.1f; // add up damage to player
                    if (playerHealthDecr >= 1.0f)
                    {
                        mCurrentHealth -= 1; // decrease player health by 1 unit
                        //Force the health to remain between 0 and 100.           
                        mCurrentHealth = (int)MathHelper.Clamp(mCurrentHealth, 0, 100);
                        playerHealthDecr = 0.0f;
                    }
                }
                if ((enemiesDeadChk == true) && (rabidDogHealths[k] > 0))
                {
                    enemiesDeadChk = false;
                }
            }

            if ((enemiesDeadChk == true) && (((Math.Abs(playerEntity.Position.X - indicatorPos.X) + Math.Abs(playerEntity.Position.Z - indicatorPos.Z)) / 2) < (playerRadius + indicatorScale*2 - 19.5)))
            {
                // player can advance to next level
                ScreenManager.AddScreen(new NextLevelScreen(), ControllingPlayer);
            }

            // Test current health bar.

            // If Page Up is pressed, increase the health bar.
            if (curKeyboardState.IsKeyDown(Keys.PageUp) == true)
            {
                mCurrentHealth += 1;
            }
            // If Page Down is pressed, decrease the health bar.
            if (curKeyboardState.IsKeyDown(Keys.PageDown) == true)
            {
                mCurrentHealth -= 1;
            }
            //Force the health to remain between 0 and 100.           
            mCurrentHealth = (int)MathHelper.Clamp(mCurrentHealth, 0, 100);

            if (mCurrentHealth == 0)
            {
                // Game is over, so go to continue or quit screen.
                ScreenManager.AddScreen(new GameOverScreen(), ControllingPlayer);
            }
        }

        private void MoveForward(ref Vector3 position, Quaternion rotationQuat, float speed)
        {
            Vector3 addVector = Vector3.Transform(new Vector3(0, 32, -1), rotationQuat);
            position += addVector * speed;
        }

        private void UpdateBulletPositions(float moveSpeed)
        {
            for (int i = 0; i < bulletList.Count; i++)
            {
                Bullet currentBullet = bulletList[i];
                MoveForward(ref currentBullet.position, currentBullet.rotation, moveSpeed * 2.0f);
                bulletList[i] = currentBullet;

                // check if bullet hits an enemy and decrease the enemy's health
                BoundingSphere bulletSphere = new BoundingSphere(currentBullet.position, 0.05f);
                for (int j = 0; j < TOTAL_RABID_DOGS; j++)
                {
                    BoundingSphere enemySphere = new BoundingSphere(rabidDogEntities[j].Position, rabidDogModels[j].ModelHeld.Meshes[0].BoundingSphere.Radius);
                    if (bulletSphere.Intersects(enemySphere))                    
                    {
                        rabidDogEntities[j].Position = rabidDogPreviousPositions[j];

                        bulletList.RemoveAt(i);
                        i--;

                        rabidDogHealthDecrs[j] += 0.5f; // add up damage to enemy
                        if (rabidDogHealthDecrs[j] >= 1.0f)
                        {
                            rabidDogHealths[j] -= 15; // decrease enemy health by 1 unit
                            //Force the health to remain between 0 and 100.           
                            rabidDogHealthDecrs[j] = 0.0f;
                        }
                        // check enemy health and destroy enemy if no health left
                        if (rabidDogHealths[j] <= 0)
                        {
                            rabidDogRadii.RemoveAt(j);
                            rabidDogModels.RemoveAt(j);
                            rabidDogBounds.RemoveAt(j);
                            rabidDogHealths.RemoveAt(j);
                            rabidDogEntities.RemoveAt(j);
                            rabidDogHealthDecrs.RemoveAt(j);
                            rabidDogPreviousPositions.RemoveAt(j);
                            TOTAL_RABID_DOGS--;
                            break;
                        }
                    }
            }
        }
        }

        private void UpdateEnemies(GameTime gameTime)
        {
            for (int i = 0; i < TOTAL_RABID_DOGS; i++)
            {
                rabidDogPreviousPositions[i] = rabidDogEntities[i].Position;
                double xDiff = rabidDogEntities[i].Position.X - playerEntity.Position.X;
                double zDiff = rabidDogEntities[i].Position.Z - playerEntity.Position.Z;
                double distFromPlayer = Math.Sqrt(Math.Pow(xDiff, 2) + Math.Pow(zDiff, 2));
                if (distFromPlayer < 700)
                {
                    if (rabidDogEntities[i].Position.X < playerEntity.Position.X)
                    {
                        rabidDogEntities[i].Position = new Vector3(rabidDogEntities[i].Position.X + 1, rabidDogEntities[i].Position.Y, rabidDogEntities[i].Position.Z);
                    }
                    else if (rabidDogEntities[i].Position.X > playerEntity.Position.X)
                    {
                        rabidDogEntities[i].Position = new Vector3(rabidDogEntities[i].Position.X - 1, rabidDogEntities[i].Position.Y, rabidDogEntities[i].Position.Z);
                    }
                    if (rabidDogEntities[i].Position.Z < playerEntity.Position.Z)
                    {
                        rabidDogEntities[i].Position = new Vector3(rabidDogEntities[i].Position.X, rabidDogEntities[i].Position.Y, rabidDogEntities[i].Position.Z + 1);
                    }
                    else if (rabidDogEntities[i].Position.Z > playerEntity.Position.Z)
                    {
                        rabidDogEntities[i].Position = new Vector3(rabidDogEntities[i].Position.X, rabidDogEntities[i].Position.Y, rabidDogEntities[i].Position.Z - 1);
                    }
                }
                else
                {
                    // enemies stay where they are, and separate if they are too close to each other
                }
                for (int j = 0; j < TOTAL_RABID_DOGS; j++)
                {
                    if (j != i)
                    {        
                        // keep enemies from colliding with each other
                        if ((((Math.Abs(rabidDogEntities[i].Position.X - rabidDogEntities[j].Position.X) + Math.Abs(rabidDogEntities[i].Position.Z - rabidDogEntities[j].Position.Z)) / 2) < (rabidDogRadii[i] + rabidDogRadii[j] - 5)))
                        {
                            rabidDogEntities[i].Position = rabidDogPreviousPositions[i];
                            break;
                        }
                    }
                }
                // keep enemies from colliding with the player and decrease health if collision
                if ((((Math.Abs(rabidDogEntities[i].Position.X - playerEntity.Position.X) + Math.Abs(rabidDogEntities[i].Position.Z - playerEntity.Position.Z)) / 2) < (rabidDogRadii[i] + playerRadius - 19.5)))
                {
                    rabidDogEntities[i].Position = rabidDogPreviousPositions[i];
                    playerHealthDecr += 0.1f; // add up damage to player
                    if (playerHealthDecr >= 1.0f)
                    {
                        mCurrentHealth -= 1; // decrease player health by 1 unit
                        //Force the health to remain between 0 and 100.           
                        mCurrentHealth = (int)MathHelper.Clamp(mCurrentHealth, 0, 100);
                        playerHealthDecr = 0.0f;
                    }
                }
                rabidDogEntities[i].Update(gameTime);
            }
        }

        private void UpdateFrameRate(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            // Update Frame Rate
            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                framesPerSecond = frames;
                frames = 0;
            }

            
            elapsedUpdateTime += gameTime.ElapsedGameTime.Milliseconds;

            // Update Time Left
            if (elapsedUpdateTime >= 1000)
            {
                timeLeft--;
                elapsedUpdateTime = 0; // reset counter
                if (timeLeft <= 0)
                {
                    // Game is over, so go to continue or quit screen.
                    ScreenManager.AddScreen(new GameOverScreen(), ControllingPlayer);
                }
            }
        }

        private void IncrementFrameCounter()
        {
            ++frames;
        }

        private void DrawIndicator()
        {
            effect.CurrentTechnique = effect.Techniques["ColoredNoShading"];
            effect.Parameters["xView"].SetValue(camera.ViewMatrix);
            effect.Parameters["xProjection"].SetValue(camera.ProjectionMatrix);
            Matrix worldMatrix = Matrix.CreateRotationY(3 * angle);
            effect.Parameters["xWorld"].SetValue(worldMatrix);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                foreach (VertexPositionColor[] v in vertices)
                {
                    ScreenManager.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, v, 0, 1, VertexPositionColor.VertexDeclaration);
                }
            }
        }

        private void DrawBullets()
        {
            if (bulletList.Count > 0)
            {
                quadEffect.CurrentTechnique = quadEffect.Techniques["Textured_2_0"];
                quadEffect.Parameters["xView"].SetValue(camera.ViewMatrix);
                quadEffect.Parameters["xProjection"].SetValue(camera.ProjectionMatrix);
                quadEffect.Parameters["xTexture"].SetValue(bullets);
                ScreenManager.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                
                for (int i = 0; i < bulletList.Count; i++)
                {
                    // scale down the quad
                    Matrix worldMatrix = Matrix.CreateScale(0.05f, 0.05f, 0.05f) * Matrix.CreateBillboard(bulletList[i].position, camera.Position, cameraUpDirection, Vector3.Forward);

                    foreach (EffectPass pass in quadEffect.CurrentTechnique.Passes)
                    {
                        quadEffect.Parameters["xWorld"].SetValue(worldMatrix);

                        pass.Apply();

                        ScreenManager.GraphicsDevice.SetVertexBuffer(quad.VertexBuffer);
                        ScreenManager.GraphicsDevice.Indices = quad.IndexBuffer;
                        ScreenManager.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);                        
                    }
                }

                ScreenManager.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            }
        }

        private void DrawPlayer()
        {
            if (modelTransforms == null)
                modelTransforms = new Matrix[playerModel.ModelHeld.Bones.Count];

            playerModel.ModelHeld.CopyAbsoluteBoneTransformsTo(modelTransforms);

            foreach (ModelMesh m in playerModel.ModelHeld.Meshes)
            {
                foreach (BasicEffect e in m.Effects)
                {
                    e.PreferPerPixelLighting = true;
                    e.TextureEnabled = true;
                    e.EnableDefaultLighting();
                    e.World = modelTransforms[m.ParentBone.Index] * playerEntity.WorldMatrix;
                    e.View = camera.ViewMatrix;
                    e.Projection = camera.ProjectionMatrix;
                }

                m.Draw();
            }
        }

        private void DrawEnemies()
        {
            for (int i = 0; i < TOTAL_RABID_DOGS; i++)
            {
                if (modelEnemyTransforms[i] == null)
                    modelEnemyTransforms[i] = new Matrix[rabidDogModels[i].ModelHeld.Bones.Count];

                rabidDogModels[i].ModelHeld.CopyAbsoluteBoneTransformsTo(modelEnemyTransforms[i]);

                foreach (ModelMesh m in rabidDogModels[i].ModelHeld.Meshes)
                {
                    foreach (BasicEffect e in m.Effects)
                    {
                        e.PreferPerPixelLighting = true;
                        e.TextureEnabled = true;
                        e.EnableDefaultLighting();
                        e.World = modelEnemyTransforms[i][m.ParentBone.Index] * rabidDogEntities[i].WorldMatrix;
                        e.View = camera.ViewMatrix;
                        e.Projection = camera.ProjectionMatrix;
                    }

                    m.Draw();
                }
            }
        }

        private void DrawText()
        {
            StringBuilder buffer = new StringBuilder();

            if (displayHelp)
            {
                buffer.AppendLine("Press W or UP to move the player forwards");
                buffer.AppendLine("Press S or DOWN to roll the player backwards");
                buffer.AppendLine("Press D or RIGHT to turn the player to the right");
                buffer.AppendLine("Press A or LEFT to turn the player to the left");
                buffer.AppendLine();
                buffer.AppendLine("Press RIGHT CONTROL to shoot player weaponry.");
                buffer.AppendLine("Press PAGEUP and PAGEDOWN to change the player's health bar.");
                buffer.AppendLine("Press SPACE to enable and disable the camera's spring system");
                buffer.AppendLine("Press + and - to change the camera's spring constant");
                buffer.AppendLine("Press ALT and ENTER to toggle full screen");
                buffer.AppendLine("Press ESCAPE to exit");
                buffer.AppendLine();
                buffer.AppendLine("Press H to hide help");
            }
            else
            {
                buffer.AppendFormat("FPS: {0}\n", framesPerSecond);

                bool springOn = camera.EnableSpringSystem;
                float springConstant = camera.SpringConstant;
                float dampingConstant = camera.DampingConstant;

                buffer.AppendLine();
                buffer.AppendLine("Camera");
                buffer.AppendFormat("  Spring {0}\n", (springOn ? "enabled" : "disabled"));
                buffer.AppendFormat("  Spring constant: {0}\n", springConstant.ToString("f2"));
                buffer.AppendFormat("  Damping constant: {0}\n", dampingConstant.ToString("f2"));
                buffer.AppendLine();
                buffer.AppendLine("Press H to display help");
            }

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            spriteBatch.DrawString(spriteFont, buffer.ToString(), fontPos, Color.Yellow);
            spriteBatch.End();
        }

        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            bool gamePadDisconnected = !gamePadState.IsConnected &&
                                       input.GamePadWasConnected[playerIndex];

            if (input.IsPauseGame(ControllingPlayer) || gamePadDisconnected)
            {
                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
            }
            else
            {
                // Otherwise move the player position.
                Vector2 movement = Vector2.Zero;
                
                if (keyboardState.IsKeyDown(Keys.A))
                    movement.X--;

                if (keyboardState.IsKeyDown(Keys.D))
                    movement.X++;

                if (keyboardState.IsKeyDown(Keys.W))
                    movement.Y--;

                if (keyboardState.IsKeyDown(Keys.S))
                    movement.Y++;
                
                Vector2 thumbstick = gamePadState.ThumbSticks.Left;

                movement.X += thumbstick.X;
                movement.Y -= thumbstick.Y;

                if (movement.Length() > 1)
                    movement.Normalize();

                playerPosition += movement * 2;
            }
        }

   
        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(Color.Black);

            ScreenManager.GraphicsDevice.BlendState = BlendState.Opaque;
            ScreenManager.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            ScreenManager.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
                        
            // Draw the terrain first, then the sky. This is faster than
            // drawing the sky first, because the depth buffer can skip
            // bothering to draw sky pixels that are covered up by the
            // terrain. This trick works because the code used to draw
            // the sky forces all the sky vertices to be as far away as
            // possible, and turns depth testing on but depth writes off.
            
            DrawPlayer();

            DrawEnemies();

            DrawBullets();

            DrawIndicator();

            sky.Draw(camera.ViewMatrix, camera.ProjectionMatrix);

            DrawTerrain(camera.ViewMatrix, camera.ProjectionMatrix);
            
            DrawText();

            // If there was any alpha blended translucent geometry in
            // the scene, that would be drawn here, after the sky.

            mBatch.Begin(0, BlendState.AlphaBlend);

            //Draw the negative space for the health bar.
            mBatch.Draw(mHealthBar, new Rectangle(ScreenManager.Game.GraphicsDevice.Viewport.Width - mHealthBar.Width - 30, 30, mHealthBar.Width, 25), new Rectangle(0, 45, mHealthBar.Width, 25), Color.Gray);

            // Draw the current health for the health bar.
            if (mCurrentHealth > 50)
            {
                mBatch.Draw(mHealthBar, new Rectangle(ScreenManager.Game.GraphicsDevice.Viewport.Width - mHealthBar.Width - 30, 30, (int)(mHealthBar.Width * ((double)mCurrentHealth / 100)), 25), new Rectangle(0, 45, mHealthBar.Width, 25), Color.DarkRed);
            }
            else if ((mCurrentHealth <= 50) && (mCurrentHealth > 25))
            {
                mBatch.Draw(mHealthBar, new Rectangle(ScreenManager.Game.GraphicsDevice.Viewport.Width - mHealthBar.Width - 30, 30, (int)(mHealthBar.Width * ((double)mCurrentHealth / 100)), 25), new Rectangle(0, 45, mHealthBar.Width, 25), Color.Red);
            }
            else
            {
                if (flicker == true)
                {
                    mBatch.Draw(mHealthBar, new Rectangle(ScreenManager.Game.GraphicsDevice.Viewport.Width - mHealthBar.Width - 30, 30, (int)(mHealthBar.Width * ((double)mCurrentHealth / 100)), 25), new Rectangle(0, 45, mHealthBar.Width, 25), Color.Red);
                    prevElapsedTime = elapsedTime;
                    flicker = false;
                }
                else if ((flicker == false) && (elapsedTime != prevElapsedTime))
                {
                    mBatch.Draw(mHealthBar, new Rectangle(ScreenManager.Game.GraphicsDevice.Viewport.Width - mHealthBar.Width - 30, 30, (int)(mHealthBar.Width * ((double)mCurrentHealth / 100)), 25), new Rectangle(0, 45, mHealthBar.Width, 25), Color.Transparent);
                    flicker = true;
                }
            }

            // Draw the box around the health bar.
            mBatch.Draw(mHealthBar, new Rectangle(ScreenManager.Game.GraphicsDevice.Viewport.Width - mHealthBar.Width - 30, 30, mHealthBar.Width, 25), new Rectangle(0, 0, mHealthBar.Width, 25), Color.White);

            mBatch.End();

            // Our player and enemy are both actually just text strings.
            SpriteBatch spriteBatchAlpha = ScreenManager.SpriteBatch;

            spriteBatchAlpha.Begin(0, BlendState.AlphaBlend);
            
            spriteBatchAlpha.DrawString(gameFont, "// TODO", playerPosition, Color.Green);

            spriteBatchAlpha.DrawString(gameFont, "Insert Gameplay Here", enemyPosition, Color.DarkRed);

            // Draw timer text.
            string secs = (timeLeft%60).ToString();
            if ((timeLeft % 60) < 10)
            {
                secs = "0" + secs;
            }
            Color timeLeftColor = Color.White;
            if (timeLeft < 60)
            {
                timeLeftColor = Color.Red;
            }
            spriteBatchAlpha.DrawString(gameFont, (timeLeft / 60) + ":" + secs, new Vector2(screenWidth/2 - 45, 10), timeLeftColor);

            spriteBatchAlpha.End();
            base.Draw(gameTime);
            
            IncrementFrameCounter(); // Increment counter for frames per second.

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }
        }

        /// <summary>
        /// Helper for drawing the terrain model.
        /// </summary>
        void DrawTerrain(Matrix view, Matrix projection)
        {
            foreach (ModelMesh mesh in terrain.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.View = view;
                    effect.Projection = projection;

                    effect.EnableDefaultLighting();

                    // Set the specular lighting to match the sky color.
                    effect.SpecularColor = new Vector3(0.6f, 0.4f, 0.2f);
                    effect.SpecularPower = 8;

                    // Set the fog to match the distant mountains
                    // that are drawn into the sky texture.
                    effect.FogEnabled = true;
                    effect.FogColor = new Vector3(0.15f);
                    effect.FogStart = 100;
                    effect.FogEnd = 320;
                }

                mesh.Draw();
            }
        }

        #endregion
    }
}
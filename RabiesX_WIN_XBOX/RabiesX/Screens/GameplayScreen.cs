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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
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

        private const float PLAYER_FORWARD_SPEED = 150.0f;
        private const float PLAYER_HEADING_SPEED = 120.0f;
        private const float PLAYER_ROLLING_SPEED = 280.0f;

        private const float TERRAIN_WIDTH = 1258.0f;
        private const float TERRAIN_HEIGHT = 1258.0f;

        private const float CAMERA_FOVX = 80.0f;
        private const float CAMERA_ZFAR = TERRAIN_WIDTH * 2.0f;
        private const float CAMERA_ZNEAR = 1.0f;
        private const float CAMERA_MAX_SPRING_CONSTANT = 100.0f;
        private const float CAMERA_MIN_SPRING_CONSTANT = 1.0f;

        private const int NUMBER_OF_COLLECTIBLES_ON_THE_MAP = 120;

        ContentManager content;
        SpriteFont gameFont;

        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;

        Vector2 playerPosition = new Vector2(100, 100);
        Vector2 enemyPosition = new Vector2(100, 100);

        // Set health bar for level.
        SpriteBatch mBatch;
        Texture2D mHealthBar;

        // Set current health for level.
        int mCurrentHealth = 100;
        
        // Set sky and terrain for level.
        Model terrain;
        Sky sky;

        // Set the 3D model to draw.
        private MyModel playerModel;

        //Sets the sounds.
        private SoundEffect sound;
        private SoundEffectInstance soundInstance;

        private Storage storage;

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

        private List<Entity> araguzCollectibleEntities;
        private List<MyModel> araguzCollectibleModels;
        private List<Collectible> araguzCollectibles;
        private List<BoundingSphere> araguzCollectibleBounds;
        private List<float> araguzRadii;
        private List<Entity> jacksonCollectibleEntities;
        private List<MyModel> jacksonCollectibleModels;
        private List<Collectible> jacksonCollectibles;
        private List<BoundingSphere> jacksonCollectibleBounds;
        private List<float> jacksonRadii;

        //private string difficultyLevel;
        private float playerRadius;
        private float terrainRadius;
        private Matrix[] modelTransforms;
        private TimeSpan elapsedTime = TimeSpan.Zero;
        private TimeSpan prevElapsedTime = TimeSpan.Zero;
        private bool displayHelp;
        private bool metJackson;

        //private Random rand;

        private ThirdPersonCamera camera;

        private KeyboardState curKeyboardState;
        private KeyboardState prevKeyboardState;

        private Character araguz;
        private Character jackson;

        BoundingSphere playerBounds;
        BoundingSphere terrainBounds;

        //private FileStream readStream;
        //private FileStream writeStream;
        //private StreamReader reader;
        //private StreamWriter writer;

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
           
            // Load models and set aspect ratio.
            playerModel = new MyModel("Models\\isabella", content);
            //playerModel.Texture("Textures\\wedge_p1_diff_v1", content);
            playerModel.Texture("Textures\\guzcruiseroofmiddle", content);
            playerModel.Texture("Textures\\guzcruiseroof1", content);
            playerModel.Texture("Textures\\cushions", content);
            playerModel.Texture("Textures\\dooropen", content);
            playerModel.Texture("Textures\\water", content);

            sound = content.Load<SoundEffect>("Audio\\Waves\\carengine");
            soundInstance = sound.CreateInstance();
            soundInstance.IsLooped.Equals(true);

            //Vector3[] humanPositions = new Vector3[20];
            //for (int i = 0; i < 200; i++)
            //{
            //    X = random.Next(-10000, 10000);
            //    Y = 0;
            //    Z = random.Next(-10000, 10000);
            //    humanPositions[i].X = X;
            //    humanPositions[i].Y = Y;
            //    humanPositions[i].Z = Z;
            //}
            //Vector3[] dogPositions = new Vector3[20];
            //for (int i = 0; i < 200; i++)
            //{
            //    X = random.Next(-10000, 10000);
            //    Y = 0;
            //    Z = random.Next(-10000, 10000);
            //    dogPositions[i].X = X;
            //    dogPositions[i].Y = Y;
            //    dogPositions[i].Z = Z;
            //}
            //Vector3[] birdPositions = new Vector3[20];
            //for (int i = 0; i < 200; i++)
            //{
            //    X = random.Next(-10000, 10000);
            //    Y = 0;
            //    Z = random.Next(-10000, 10000);
            //    birdPositions[i].X = X;
            //    birdPositions[i].Y = Y;
            //    birdPositions[i].Z = Z;
            //}

            // Load terrain and sky.
            terrain = content.Load<Model>("terrain");
            sky = content.Load<Sky>("sky");
            //sky = content.Load<Sky>("GraySky");

            // Determine the radius of the player model.           
            BoundingSphere bounds = new BoundingSphere();
            foreach (ModelMesh mesh in playerModel.ModelHeld.Meshes)
                bounds = BoundingSphere.CreateMerged(bounds, mesh.BoundingSphere);
            playerRadius = bounds.Radius;

            playerBounds = bounds;

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

            // Setup the terrain entity.
            terrainEntity = new Entity();
            terrainEntity.ConstrainToWorldYAxis = true;
            terrainEntity.Position = new Vector3(0.0f, 1.0f + terrainRadius, 0.0f);


            //creates random collectible entities to help Araguz.
            GenerateAraguzCollectibles();
            
            //creates Geraldo Araguz.
            araguz = new Protagonist();
            ((Protagonist)araguz).plasmaGun = new PlasmaGun();            
                  
            /*creates Russell Jackson aka Sadulgo Randol. His type will be antagonist even if he
            initially takes cover as Araguz's "ally" */
            jackson = new Antagonist();
            ((Antagonist)jackson).sword = new Sword();

            storage = new Storage();
            metJackson = storage.MetJackson;
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
                ProcessKeyboard();
                UpdatePlayer(gameTime);
                UpdateFrameRate(gameTime);

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
        }

        private void UpdatePlayer(GameTime gameTime)
        {
            float pitch = 0.0f;
            float heading = 0.0f;
            float forwardSpeed = 0.0f;

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
            //playerEntity.Rotate(0.0f, pitch, 0.0f);
            playerEntity.Update(gameTime);

            // Then move the camera based on where the player has moved to.
            // When the player is moving backwards rotations are inverted to
            // match the direction of travel. Consequently the camera's
            // rotation needs to be inverted as well.

            camera.Rotate((forwardSpeed >= 0.0f) ? heading : -heading, 0.0f);
            camera.LookAt(playerEntity.Position);
            camera.Update(gameTime);
            
            // Test current health bar.

            // If Page Up is pressed, increase the health bar.
            //if (curKeyboardState.IsKeyDown(Keys.PageUp) == true)
            //{
                //mCurrentHealth += 1;
                //araguz.Heal(1);
            //}
            // If Page Down is pressed, decrease the health bar.
            //if (curKeyboardState.IsKeyDown(Keys.PageDown) == true)
            //{
                //mCurrentHealth -= 1;
                //araguz.Wound(1);
            //}
            //Force the health to remain between 0 and 100.           
            //mCurrentHealth = (int)MathHelper.Clamp(mCurrentHealth, 0, 100);
            araguz.Health = (int)MathHelper.Clamp(mCurrentHealth, 0, 100);

            //if (mCurrentHealth == 0)
            if(araguz.Health == 0)
            {
                // Game is over, so go to continue or quit screen.
                if (metJackson)
                    jackson.Health = 0;
                ScreenManager.AddScreen(new GameOverScreen(), ControllingPlayer);
            }
        }

        private void UpdateFrameRate(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                framesPerSecond = frames;
                frames = 0;
            }
        }

        private void IncrementFrameCounter()
        {
            ++frames;
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

        private void DrawCollectible(MyModel model, Entity entity)
        {
            if (modelTransforms == null)
                modelTransforms = new Matrix[model.ModelHeld.Bones.Count];

            model.ModelHeld.CopyAbsoluteBoneTransformsTo(modelTransforms);

            foreach (ModelMesh m in model.ModelHeld.Meshes)
            {
                foreach (BasicEffect e in m.Effects)
                {
                    e.PreferPerPixelLighting = true;
                    e.TextureEnabled = true;
                    e.EnableDefaultLighting();
                    e.World = modelTransforms[m.ParentBone.Index] * entity.WorldMatrix;
                    e.View = camera.ViewMatrix;
                    e.Projection = camera.ProjectionMatrix;
                }

                m.Draw();
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

                if (keyboardState.IsKeyDown(Keys.W))
                {
                    if(OtherKeysUp(keyboardState, Keys.W))
                        soundInstance.Play();
                    movement.Y--;
                }
                if (keyboardState.IsKeyDown(Keys.S))
                {
                    if(OtherKeysUp(keyboardState, Keys.S))
                        soundInstance.Play();
                    movement.Y++;
                }
                if (keyboardState.IsKeyDown(Keys.A))
                {
                    if(OtherKeysUp(keyboardState, Keys.A))
                        soundInstance.Play();
                    movement.X--;
                }
                if (keyboardState.IsKeyDown(Keys.D))
                {
                    if(OtherKeysUp(keyboardState, Keys.D))
                        soundInstance.Play();
                    movement.X++;
                }              
                if (keyboardState.IsKeyDown(Keys.Up))
                {
                    if(OtherKeysUp(keyboardState, Keys.Up))
                        soundInstance.Play();
                    movement.Y--;
                }
                if (keyboardState.IsKeyDown(Keys.Down))
                {
                    if(OtherKeysUp(keyboardState, Keys.Down))
                        soundInstance.Play();
                    movement.Y++;
                }
                if (keyboardState.IsKeyDown(Keys.Left))
                {
                    if(OtherKeysUp(keyboardState, Keys.Left))
                        soundInstance.Play();
                    movement.X--;
                }
                if (keyboardState.IsKeyDown(Keys.Right))
                {
                    if(OtherKeysUp(keyboardState, Keys.Right))
                        soundInstance.Play();
                    movement.X++;
                }
                if (keyboardState.IsKeyUp(Keys.W) && OtherKeysUp(keyboardState, Keys.W))
                    soundInstance.Stop();
                if (keyboardState.IsKeyUp(Keys.Up) && OtherKeysUp(keyboardState, Keys.Up))
                    soundInstance.Stop();              
                if (keyboardState.IsKeyUp(Keys.S) && OtherKeysUp(keyboardState, Keys.S))
                    soundInstance.Stop();
                if (keyboardState.IsKeyUp(Keys.Down) && OtherKeysUp(keyboardState, Keys.Down))
                    soundInstance.Stop();
                if (keyboardState.IsKeyUp(Keys.A) && OtherKeysUp(keyboardState, Keys.A))
                    soundInstance.Stop();
                if (keyboardState.IsKeyUp(Keys.Left) && OtherKeysUp(keyboardState, Keys.Left))
                    soundInstance.Stop();
                if (keyboardState.IsKeyUp(Keys.D) && OtherKeysUp(keyboardState, Keys.D))
                    soundInstance.Stop();
                if (keyboardState.IsKeyUp(Keys.Right) && OtherKeysUp(keyboardState, Keys.Right))
                    soundInstance.Stop();
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

            for (int index = 0; index < NUMBER_OF_COLLECTIBLES_ON_THE_MAP / 2; index++)
                DrawCollectible(araguzCollectibleModels[index], araguzCollectibleEntities[index]);

            sky.Draw(camera.ViewMatrix, camera.ProjectionMatrix);

            DrawTerrain(camera.ViewMatrix, camera.ProjectionMatrix);
            
            DrawText();

            // If there was any alpha blended translucent geometry in
            // the scene, that would be drawn here, after the sky.

            mBatch.Begin(0, BlendState.AlphaBlend);

            //Draw the negative space for the health bar.
            mBatch.Draw(mHealthBar, new Rectangle(ScreenManager.Game.GraphicsDevice.Viewport.Width - mHealthBar.Width - 30, 30, mHealthBar.Width, 25), new Rectangle(0, 45, mHealthBar.Width, 25), Color.Gray);
            // Draw the current health for the health bar.
            //if (mCurrentHealth > 50)
            if (araguz.Health > 50)
            {
                mBatch.Draw(mHealthBar, new Rectangle(ScreenManager.Game.GraphicsDevice.Viewport.Width - mHealthBar.Width - 30, 30, (int)(mHealthBar.Width * ((double)araguz.Health / 100)), 25), new Rectangle(0, 45, mHealthBar.Width, 25), Color.DarkRed);
            }
            //else if ((mCurrentHealth <= 50) && (mCurrentHealth > 25))
            else if((araguz.Health <= 50) && (araguz.Health > 25))
            {
                mBatch.Draw(mHealthBar, new Rectangle(ScreenManager.Game.GraphicsDevice.Viewport.Width - mHealthBar.Width - 30, 30, (int)(mHealthBar.Width * ((double)araguz.Health / 100)), 25), new Rectangle(0, 45, mHealthBar.Width, 25), Color.Red);
            }
            else
            {
                if (flicker == true)
                {
                    mBatch.Draw(mHealthBar, new Rectangle(ScreenManager.Game.GraphicsDevice.Viewport.Width - mHealthBar.Width - 30, 30, (int)(mHealthBar.Width * ((double)araguz.Health / 100)), 25), new Rectangle(0, 45, mHealthBar.Width, 25), Color.Red);
                    prevElapsedTime = elapsedTime;
                    flicker = false;
                }
                else if ((flicker == false) && (elapsedTime != prevElapsedTime))
                {
                    mBatch.Draw(mHealthBar, new Rectangle(ScreenManager.Game.GraphicsDevice.Viewport.Width - mHealthBar.Width - 30, 30, (int)(mHealthBar.Width * ((double)araguz.Health / 100)), 25), new Rectangle(0, 45, mHealthBar.Width, 25), Color.Transparent);
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

            spriteBatchAlpha.DrawString(gameFont, "Insert Gameplay Here",
                                   enemyPosition, Color.DarkRed);

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

        private void GenerateAraguzCollectibles()
        {
            int X, Y, Z;
            Vector3[] araguzPositions = new Vector3[NUMBER_OF_COLLECTIBLES_ON_THE_MAP / 2];
            araguzCollectibleEntities = new List<Entity>();
            araguzCollectibleModels = new List<MyModel>();
            araguzCollectibles = new List<Collectible>();
            araguzCollectibleBounds = new List<BoundingSphere>();
            araguzRadii = new List<float>();
            Random collectibleRandom;
            int index;
            int collectibleIndex;
            for (index = 0; index < NUMBER_OF_COLLECTIBLES_ON_THE_MAP / 2; index++)
            {
                collectibleRandom = new Random();
                X = random.Next(-100, 100);
                Y = 0;
                Z = random.Next(-100, 100);
                araguzCollectibleEntities.Add(new Entity());
                araguzCollectibleEntities[index].ConstrainToWorldYAxis = true;
                araguzCollectibleEntities[index].Position = new Vector3(X, Y, Z);
                collectibleIndex = collectibleRandom.Next() % 2;
                if (collectibleIndex == 0)
                {
                    araguzCollectibles.Add(new Collectible("araguz", "plasma container", 2));
                    araguzCollectibleModels.Add(new MyModel("Models\\plasma_container", content));
                    araguzCollectibleModels[index].Texture("Textures\\Bucket", content);
                    araguzCollectibleModels[index].Texture("Textures\\White", content);
                    araguzCollectibleModels[index].Position = new Vector3(X, Y, Z);
                }
                else
                {
                    araguzCollectibles.Add(new Collectible("araguz", "chemicals", 2));
                    araguzCollectibleModels.Add(new MyModel("Models\\chemicals", content));
                    araguzCollectibleModels[index].Texture("Textures\\Chemical1", content);
                    araguzCollectibleModels[index].Texture("Textures\\Chemical2", content);
                    araguzCollectibleModels[index].Texture("Textures\\RedLiquid", content);
                    araguzCollectibleModels[index].Texture("Textures\\TopOfFlask", content);
                    araguzCollectibleEntities[index].Position = new Vector3(X, Y, Z);
                }
                BoundingSphere sphere = new BoundingSphere();
                foreach (ModelMesh mesh in araguzCollectibleModels[index].ModelHeld.Meshes)
                    sphere = BoundingSphere.CreateMerged(sphere, mesh.BoundingSphere);
                araguzRadii.Add(sphere.Radius);
                araguzCollectibleBounds.Add(sphere);
            }
        }

        private void GenerateJacksonCollectibles()
        {
            int X, Y, Z;
            Vector3[] jacksonPositions = new Vector3[NUMBER_OF_COLLECTIBLES_ON_THE_MAP / 2];
            jacksonCollectibleEntities = new List<Entity>();
            jacksonCollectibleModels = new List<MyModel>();
            jacksonCollectibles = new List<Collectible>();
            jacksonCollectibleBounds = new List<BoundingSphere>();
            jacksonRadii = new List<float>();
            Random collectibleRandom;
            int index;
            int collectibleIndex;
            for (index = 0; index < NUMBER_OF_COLLECTIBLES_ON_THE_MAP / 2; index++)
            {
                collectibleRandom = new Random();
                X = random.Next(-100, 100);
                Y = 0;
                Z = random.Next(-100, 100);
                jacksonCollectibleEntities.Add(new Entity());
                jacksonCollectibleEntities[index].ConstrainToWorldYAxis = true;
                jacksonCollectibleEntities[index].Position = new Vector3(X, Y, Z);
                collectibleIndex = collectibleRandom.Next() % 2;
                if (collectibleIndex == 0)
                {
                    jacksonCollectibles.Add(new Collectible("araguz", "plasma container", 2));
                    jacksonCollectibleModels.Add(new MyModel("Models\\plasma_container", content));
                    jacksonCollectibleModels[index].Texture("Textures\\Bucket", content);
                    jacksonCollectibleModels[index].Texture("Textures\\White", content);
                    jacksonCollectibleModels[index].Position = new Vector3(X, Y, Z);
                }
                else
                {
                    jacksonCollectibles.Add(new Collectible("araguz", "chemicals", 2));
                    jacksonCollectibleModels.Add(new MyModel("Models\\chemicals", content));
                    jacksonCollectibleModels[index].Texture("Textures\\Chemical1", content);
                    jacksonCollectibleModels[index].Texture("Textures\\Chemical2", content);
                    jacksonCollectibleModels[index].Texture("Textures\\RedLiquid", content);
                    jacksonCollectibleModels[index].Texture("Textures\\TopOfFlask", content);
                    jacksonCollectibleEntities[index].Position = new Vector3(X, Y, Z);
                }
                BoundingSphere sphere = new BoundingSphere();
                foreach (ModelMesh mesh in jacksonCollectibleModels[index].ModelHeld.Meshes)
                    sphere = BoundingSphere.CreateMerged(sphere, mesh.BoundingSphere);
                jacksonRadii.Add(sphere.Radius);
                jacksonCollectibleBounds.Add(sphere);
            }
        }

        private bool OtherKeysUp(KeyboardState state, Keys theKey)
        {
            Keys[] gameKeys = {Keys.H, Keys.Space, Keys.LeftAlt, Keys.RightAlt, Keys.Enter, Keys.Add,
                                  Keys.Subtract, Keys.A, Keys.W, Keys.S, Keys.D, Keys.Up, Keys.Left,
                                  Keys.Right, Keys.Down, Keys.E, Keys.R};
            bool keysAreUp = true;
            foreach (Keys key in gameKeys)
            {
                if (key != theKey)
                    keysAreUp = keysAreUp && state.IsKeyUp(key);
            }
            return keysAreUp;
        }

        private bool Collision(Vector3 position1, Vector3 position2)
        {
            if (position1 == position2)
                return true;
            return false;
        }
    }
}